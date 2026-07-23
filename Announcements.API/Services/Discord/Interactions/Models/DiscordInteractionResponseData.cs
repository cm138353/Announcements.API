using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public sealed class DiscordInteractionResponseData
    {
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Content { get; init; }

        [JsonPropertyName("flags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DiscordMessageFlags Flags { get; init; }
    }
}
