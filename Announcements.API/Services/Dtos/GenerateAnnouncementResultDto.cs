namespace Announcements.API.Services.Dtos
{
    public class GenerateAnnouncementResultDto
    {
        public string DiscordContent { get; set; } = string.Empty;

        public string ClanMailContent { get; set; } = string.Empty;

        public string ClanChatContent { get; set; } = string.Empty;
    }
}
