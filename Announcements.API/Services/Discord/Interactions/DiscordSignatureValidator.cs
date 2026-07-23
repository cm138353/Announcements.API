using System.Text;
using Microsoft.Extensions.Options;
using NSec.Cryptography;
using Volo.Abp.DependencyInjection;

namespace Announcements.API.Services.Discord.Interactions
{
    public sealed class DiscordSignatureValidator : IDiscordSignatureValidator, ITransientDependency
    {
        private const int PublicKeyLength = 32;
        private const int SignatureLength = 64;

        private readonly PublicKey _publicKey;
        private readonly SignatureAlgorithm _algorithm = SignatureAlgorithm.Ed25519;

        public DiscordSignatureValidator(IOptions<DiscordOptions> options)
        {
            var publicKeyHex = options.Value.PublicKey;

            if (string.IsNullOrWhiteSpace(publicKeyHex))
            {
                throw new InvalidOperationException(
                    "Discord public key has not been configured.");
            }

            var publicKeyBytes = ConvertHex(publicKeyHex, PublicKeyLength);

            _publicKey = PublicKey.Import(
                _algorithm,
                publicKeyBytes,
                KeyBlobFormat.RawPublicKey);
        }

        public bool IsValid(
            string signature,
            string timestamp,
            string rawRequestBody)
        {
            if (string.IsNullOrWhiteSpace(signature) ||
                string.IsNullOrWhiteSpace(timestamp) ||
                string.IsNullOrEmpty(rawRequestBody))
            {
                return false;
            }

            try
            {
                var signatureBytes = ConvertHex(signature, SignatureLength);
                var messageBytes = Encoding.UTF8.GetBytes(
                    timestamp + rawRequestBody);

                return _algorithm.Verify(
                    _publicKey,
                    messageBytes,
                    signatureBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] ConvertHex(string value, int expectedByteLength)
        {
            value = value.Trim();

            if (value.Length != expectedByteLength * 2)
            {
                throw new FormatException(
                    $"Expected a {expectedByteLength}-byte hexadecimal value.");
            }

            var bytes = Convert.FromHexString(value);

            if (bytes.Length != expectedByteLength)
            {
                throw new FormatException(
                    $"Expected a {expectedByteLength}-byte hexadecimal value.");
            }

            return bytes;
        }
    }
}
