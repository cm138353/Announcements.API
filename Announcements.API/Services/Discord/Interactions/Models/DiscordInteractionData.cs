using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public class DiscordInteractionData
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("type")]
        public int Type { get; init; }

        [JsonPropertyName("options")]
        public List<DiscordInteractionOption> Options { get; init; } = [];
    }
}
