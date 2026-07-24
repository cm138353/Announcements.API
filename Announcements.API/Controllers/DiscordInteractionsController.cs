using Announcements.API.Services.Discord.Interactions;
using Announcements.API.Services.Discord.Interactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;

namespace Announcements.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [DisableAuditing]
    [UnitOfWork(IsDisabled = true)]
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
        private readonly IDiscordInteractionQueue _interactionQueue;
        private readonly ILogger<DiscordInteractionsController> _logger;

        public DiscordInteractionsController(
            DiscordSignatureValidator signatureValidator,
            IDiscordInteractionQueue interactionQueue,
            ILogger<DiscordInteractionsController> logger)
        {
            _signatureValidator = signatureValidator;
            _interactionQueue = interactionQueue;
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/json")]
        public virtual async Task<IActionResult> HandleAsync(
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
                await _interactionQueue.EnqueueAsync(
                    interaction,
                    cancellationToken);

                _logger.LogInformation(
                    "Accepted Discord interaction {InteractionId} " +
                    "for deferred processing.",
                    interaction.Id);

                return Ok(
                    DiscordInteractionResponse.DeferredChannelMessage());
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
                    "Could not enqueue Discord interaction {InteractionId}.",
                    interaction.Id);

                return Ok(
                    DiscordInteractionResponse.EphemeralMessage(
                        "❌ The command could not be started. Please try again."));
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