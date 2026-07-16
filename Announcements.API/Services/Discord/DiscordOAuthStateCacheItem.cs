namespace Announcements.API.Services.Discord
{
    [Serializable]  
    public class DiscordOAuthStateCacheItem
    {
        public Guid UserId { get; set; }
    }
}
