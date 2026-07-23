using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSec.Cryptography;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscordInteractionsController> _logger;

        public DiscordInteractionsController(
            IConfiguration configuration,
            ILogger<DiscordInteractionsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleAsync()
        {
            var signatureHex =
                Request.Headers["X-Signature-Ed25519"].FirstOrDefault();

            var timestamp =
                Request.Headers["X-Signature-Timestamp"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signatureHex) ||
                string.IsNullOrWhiteSpace(timestamp))
            {
                return Unauthorized();
            }

            using var reader = new StreamReader(
                Request.Body,
                Encoding.UTF8);

            var rawBody = await reader.ReadToEndAsync();

            if (!IsValidDiscordSignature(
                    signatureHex,
                    timestamp,
                    rawBody))
            {
                _logger.LogWarning(
                    "Discord interaction signature validation failed.");

                return Unauthorized();
            }

            using var document = JsonDocument.Parse(rawBody);

            var interactionType = document.RootElement
                .GetProperty("type")
                .GetInt32();

            // Discord endpoint verification ping
            if (interactionType == 1)
            {
                return Ok(new
                {
                    type = 1
                });
            }

            return Ok(new
            {
                type = 4,
                data = new
                {
                    content = "Announcement Bot received the command.",
                    flags = 64
                }
            });
        }

        private bool IsValidDiscordSignature(
            string signatureHex,
            string timestamp,
            string rawBody)
        {
            try
            {
                var publicKeyHex =
                    _configuration["Discord:PublicKey"];

                if (string.IsNullOrWhiteSpace(publicKeyHex))
                {
                    throw new InvalidOperationException(
                        "Discord:PublicKey is not configured.");
                }

                var publicKeyBytes =
                    Convert.FromHexString(publicKeyHex);

                var signatureBytes =
                    Convert.FromHexString(signatureHex);

                var messageBytes =
                    Encoding.UTF8.GetBytes(timestamp + rawBody);

                var algorithm = SignatureAlgorithm.Ed25519;

                var publicKey = PublicKey.Import(
                    algorithm,
                    publicKeyBytes,
                    KeyBlobFormat.RawPublicKey);

                return algorithm.Verify(
                    publicKey,
                    messageBytes,
                    signatureBytes);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An error occurred while validating Discord's signature.");

                return false;
            }
        }
    }
}
