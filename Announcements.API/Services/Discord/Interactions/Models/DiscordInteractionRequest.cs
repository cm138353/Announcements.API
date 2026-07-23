using Announcements.API.Services.Dtos.Interactions;
using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public sealed class DiscordInteractionRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("application_id")]
        public string ApplicationId { get; init; } = string.Empty;

        [JsonPropertyName("type")]
        public DiscordInteractionType Type { get; init; }

        [JsonPropertyName("data")]
        public DiscordInteractionData? Data { get; init; }

        [JsonPropertyName("guild_id")]
        public string? GuildId { get; init; }

        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; init; }

        [JsonPropertyName("member")]
        public DiscordGuildMember? Member { get; init; }

        [JsonPropertyName("user")]
        public DiscordUser? User { get; init; }

        [JsonPropertyName("token")]
        public string Token { get; init; } = string.Empty;

        [JsonPropertyName("version")]
        public int Version { get; init; }
    }
}
