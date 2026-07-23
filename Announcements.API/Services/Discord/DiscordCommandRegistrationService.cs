using System.Net.Http.Headers;

namespace Announcements.API.Services.Discord
{
    public class DiscordCommandRegistrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscordCommandRegistrationService> _logger;

        public DiscordCommandRegistrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<DiscordCommandRegistrationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RegisterGuildCommandsAsync(
            CancellationToken cancellationToken = default)
        {
            var applicationId = GetRequiredSetting("Discord:ClientId");
            var botToken = GetRequiredSetting("Discord:BotToken");
            var guildId = GetRequiredSetting("Discord:GuildId");

            var url =
                $"https://discord.com/api/v10/applications/{applicationId}" +
                $"/guilds/{guildId}/commands";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bot", botToken);

            request.Content = JsonContent.Create(new
            {
                name = "announcement",
                description = "Create and manage announcements",
                type = 1,
                options = new object[]
                {
                new
                {
                    type = 1,
                    name = "create",
                    description = "Create a new announcement"
                },
                new
                {
                    type = 1,
                    name = "help",
                    description = "Show help for Announcement Bot"
                }
                }
            });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Discord command registration failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            _logger.LogInformation(
                "Discord guild slash commands registered successfully.");
        }

        private string GetRequiredSetting(string key)
        {
            return _configuration[key]
                ?? throw new InvalidOperationException(
                    $"Missing configuration setting: {key}");
        }
    }
}
