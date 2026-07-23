using Announcements.API.Services.Discord.Interactions;
using Announcements.API.Services.Discord.Interactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Volo.Abp.AspNetCore.Mvc;

namespace Announcements.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/discord/interactions")]
    public class DiscordInteractionsController : AbpController
    {
        private const string SignatureHeaderName =
            "X-Signature-Ed25519";

        private const string TimestampHeaderName =
            "X-Signature-Timestamp";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly DiscordSignatureValidator _signatureValidator;
        private readonly IDiscordInteractionService _interactionService;
        private readonly ILogger<DiscordInteractionsController> _logger;

        public DiscordInteractionsController(
            DiscordSignatureValidator signatureValidator,
            IDiscordInteractionService interactionService,
            ILogger<DiscordInteractionsController> logger)
        {
            _signatureValidator = signatureValidator;
            _interactionService = interactionService;
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> HandleAsync(
            CancellationToken cancellationToken)
        {
            var signature = Request.Headers[SignatureHeaderName]
                .FirstOrDefault();

            var timestamp = Request.Headers[TimestampHeaderName]
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signature) ||
                string.IsNullOrWhiteSpace(timestamp))
            {
                _logger.LogWarning(
                    "Discord interaction request was missing one or more " +
                    "required signature headers.");

                return Unauthorized();
            }

            var rawBody = await ReadRawRequestBodyAsync(
                cancellationToken);

            if (string.IsNullOrWhiteSpace(rawBody))
            {
                _logger.LogWarning(
                    "Discord interaction request contained an empty body.");

                return BadRequest(new
                {
                    error = "The request body is required."
                });
            }

            if (!_signatureValidator.IsValid(
                    signature,
                    timestamp,
                    rawBody))
            {
                _logger.LogWarning(
                    "Discord interaction request failed signature validation.");

                return Unauthorized();
            }

            DiscordInteractionRequest? interaction;

            try
            {
                interaction =
                    JsonSerializer.Deserialize<DiscordInteractionRequest>(
                        rawBody,
                        JsonOptions);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Discord interaction request contained invalid JSON.");

                return BadRequest(new
                {
                    error = "The interaction payload was invalid."
                });
            }

            if (interaction is null)
            {
                _logger.LogWarning(
                    "Discord interaction payload deserialized to null.");

                return BadRequest(new
                {
                    error = "The interaction payload was invalid."
                });
            }

            if (interaction.Type == DiscordInteractionType.Ping)
            {
                return Ok(DiscordInteractionResponse.Pong());
            }

            if (interaction.Type !=
                DiscordInteractionType.ApplicationCommand)
            {
                return Ok(
                    DiscordInteractionResponse.EphemeralMessage(
                        "This Discord interaction type is not supported."));
            }

            try
            {
                Response.StatusCode = StatusCodes.Status200OK;
                Response.ContentType = "application/json";

                await JsonSerializer.SerializeAsync(
                    Response.Body,
                    DiscordInteractionResponse.DeferredChannelMessage(),
                    JsonOptions,
                    cancellationToken);

                await Response.Body.FlushAsync(cancellationToken);

                _logger.LogInformation(
                    "Deferred Discord interaction {InteractionId}.",
                    interaction.Id);

                await _interactionService.ProcessDeferredAsync(
                    interaction);

                return new EmptyResult();
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Discord interaction {InteractionId} was cancelled.",
                    interaction.Id);

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred while processing Discord " +
                    "interaction {InteractionId}.",
                    interaction.Id);

                // The deferred response may already have been written.
                // Do not attempt to write another response body.
                return new EmptyResult();
            }
        }

        private async Task<string> ReadRawRequestBodyAsync(
            CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(
                Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}