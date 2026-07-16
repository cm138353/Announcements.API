namespace Announcements.API.Services.Dtos
{
    public class SaveDiscordConfigurationDto
    {
        public string GuildId { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
    }
}
