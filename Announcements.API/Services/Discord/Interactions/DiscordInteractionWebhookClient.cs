using Volo.Abp.DependencyInjection;

namespace Announcements.API.Services.Discord.Interactions
{
    public class DiscordInteractionWebhookClient : IDiscordInteractionWebhookClient, ITransientDependency
    {
        private const int DiscordMessageCharacterLimit = 2000;

        private readonly IHttpClientFactory _httpClientFactory;

        public DiscordInteractionWebhookClient(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task EditOriginalResponseAsync(string applicationId, string interactionToken, string content, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentException(
                    "Discord application ID is required.",
                    nameof(applicationId));
            }

            if (string.IsNullOrWhiteSpace(interactionToken))
            {
                throw new ArgumentException(
                    "Discord interaction token is required.",
                    nameof(interactionToken));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "Discord response content is required.",
                    nameof(content));
            }

            if (content.Length > DiscordMessageCharacterLimit)
            {
                throw new ArgumentException(
                    $"Discord response content cannot exceed " +
                    $"{DiscordMessageCharacterLimit} characters.",
                    nameof(content));
            }

            var applicationIdSegment = Uri.EscapeDataString(applicationId);

            var interactionTokenSegment = Uri.EscapeDataString(interactionToken);

            var requestUri =
                $"https://discord.com/api/v10/webhooks/" +
                $"{applicationIdSegment}/" +
                $"{interactionTokenSegment}/messages/@original";

            var requestBody = new DiscordWebhookMessage
            {
                Content = content
            };

            var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.PatchAsJsonAsync(requestUri, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                throw new HttpRequestException(
                    $"Discord rejected the webhook response. " +
                    $"Status code: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}",
                    null,
                    response.StatusCode);
            }
        }

        private sealed class DiscordWebhookMessage
        {
            public string Content { get; init; } = string.Empty;
        }
    }
}
