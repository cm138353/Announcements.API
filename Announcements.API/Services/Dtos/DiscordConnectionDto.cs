namespace Announcements.API.Services.Dtos
{
    public class DiscordConnectionDto
    {
        public string DiscordUserId { get; set; } = string.Empty;
        public string? GuildId { get; set; }
        public string? ChannelId { get; set; }
    }
}
