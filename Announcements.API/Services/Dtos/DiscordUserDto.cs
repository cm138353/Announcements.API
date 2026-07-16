using System.Text.Json.Serialization;

namespace Announcements.API.Services.Dtos
{
    public class DiscordUserDto
    {
        public string Id { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("global_name")]
        public string? GlobalName { get; set; }

        public string? Avatar { get; set; }
    }
}
