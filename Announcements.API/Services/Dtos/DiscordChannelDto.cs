using System.Text.Json.Serialization;

namespace Announcements.API.Services.Dtos
{
    public class DiscordChannelDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Type { get; set; }

        public int Position { get; set; }

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }
    }
}
