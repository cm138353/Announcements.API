namespace Announcements.API.Services.Dtos
{
    public class DiscordConfigurationDto
    {
        public bool IsConnected { get; set; }

        public string? DiscordUserId { get; set; }

        public string? GuildId { get; set; }

        public string? ChannelId { get; set; }

        public DateTime? TokenExpiresAt { get; set; }
    }
}
