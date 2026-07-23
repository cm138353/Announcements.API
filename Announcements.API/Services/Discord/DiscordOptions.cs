namespace Announcements.API.Services.Discord
{
    public class DiscordOptions
    {
        public const string SectionName = "Discord";

        public string BaseUrl { get; set; } = string.Empty;
        public string BotToken { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
    }
}
