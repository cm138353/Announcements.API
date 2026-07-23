using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public sealed class DiscordInteractionResponse
    {
        [JsonPropertyName("type")]
        public DiscordInteractionResponseType Type { get; init; }

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DiscordInteractionResponseData? Data { get; init; }

        public static DiscordInteractionResponse Pong()
        {
            return new DiscordInteractionResponse
            {
                Type = DiscordInteractionResponseType.Pong
            };
        }

        public static DiscordInteractionResponse DeferredChannelMessage()
        {
            return new DiscordInteractionResponse
            {
                Type = DiscordInteractionResponseType.DeferredChannelMessageWithSource
            };
        }

        public static DiscordInteractionResponse EphemeralMessage(
            string content)
        {
            return new DiscordInteractionResponse
            {
                Type = DiscordInteractionResponseType
                    .ChannelMessageWithSource,

                Data = new DiscordInteractionResponseData
                {
                    Content = content,
                    Flags = DiscordMessageFlags.Ephemeral
                }
            };
        }
    }
}
