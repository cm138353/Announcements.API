using Announcements.API.Entities.Announcements;
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

            var requestUri =
                $"https://discord.com/api/v10/applications/{applicationId}" +
                $"/guilds/{guildId}/commands";

            var payload = new
            {
                name = "announcement",
                description = "Create and manage announcements",
                type = 1, // CHAT_INPUT

                options = new object[]
                {
                    new
                    {
                        type = 1, // SUB_COMMAND
                        name = "create",
                        description = "Create a new announcement",

                        options = new object[]
                        {
                            new
                            {
                                type = 3, // STRING
                                name = "title",
                                description = "Announcement title",
                                required = true,
                                max_length = 100
                            },
                            new
                            {
                                type = 3, // STRING
                                name = "event-type",
                                description = "Type of Clash event",
                                required = true,

                                choices = Enum
                                    .GetNames<ClashEventType>()
                                    .Select(value => new
                                    {
                                        name = FormatChoiceName(value),
                                        value
                                    })
                                    .ToArray()
                            },
                            new
                            {
                                type = 3, // STRING
                                name = "event-date",
                                description =
                                    "Event date in YYYY-MM-DD format",
                                required = true,
                                min_length = 10,
                                max_length = 10
                            },
                            new
                            {
                                type = 3, // STRING
                                name = "tone",
                                description = "Announcement tone",
                                required = true,

                                choices = Enum
                                    .GetNames<AnnouncementTone>()
                                    .Select(value => new
                                    {
                                        name = FormatChoiceName(value),
                                        value
                                    })
                                    .ToArray()
                            },
                            new
                            {
                                type = 3, // STRING
                                name = "notes",
                                description =
                                    "Details to include in the announcement",
                                required = true,
                                max_length = 2000
                            }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bot", botToken);

            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    "Discord command registration failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            _logger.LogInformation(
                "Discord guild command /announcement create " +
                "registered successfully.");
        }

        private string GetRequiredSetting(string key)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Missing configuration setting: {key}");
            }

            return value;
        }

        private static string FormatChoiceName(string value)
        {
            return string.Concat(
                value.Select((character, index) =>
                    index > 0 &&
                    char.IsUpper(character) &&
                    !char.IsUpper(value[index - 1])
                        ? $" {character}"
                        : character.ToString()));
        }
    }
}
