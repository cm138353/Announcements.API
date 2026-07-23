using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public class DiscordUser
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [JsonPropertyName("global_name")]
        public string? GlobalName { get; init; }

        [JsonPropertyName("bot")]
        public bool IsBot { get; init; }
    }
}
