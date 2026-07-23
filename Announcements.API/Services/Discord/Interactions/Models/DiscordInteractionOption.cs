using System.Text.Json;
using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public sealed class DiscordInteractionOption
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("type")]
        public DiscordApplicationCommandOptionType Type { get; init; }

        [JsonPropertyName("value")]
        public JsonElement? Value { get; init; }

        [JsonPropertyName("options")]
        public List<DiscordInteractionOption> Options { get; init; } = [];

        public string? GetStringValue()
        {
            if (!Value.HasValue)
            {
                return null;
            }

            return Value.Value.ValueKind switch
            {
                JsonValueKind.String => Value.Value.GetString(),
                JsonValueKind.Number => Value.Value.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }
    }
}
