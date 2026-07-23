using System.Text.Json.Serialization;

namespace Announcements.API.Services.Discord.Interactions.Models
{
    public class DiscordGuildMember
    {
        [JsonPropertyName("user")]
        public DiscordUser? User { get; init; }

        [JsonPropertyName("nick")]
        public string? Nickname { get; init; }

        [JsonPropertyName("roles")]
        public List<string> RoleIds { get; init; } = [];
    }
}
