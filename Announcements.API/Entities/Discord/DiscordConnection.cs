using Volo.Abp.Domain.Entities.Auditing;

namespace Announcements.API.Entities.Discord
{
    public class DiscordConnection : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }

        public string DiscordUserId { get; private set; } = string.Empty;

        public string AccessToken { get; private set; } = string.Empty;

        public string RefreshToken { get; private set; } = string.Empty;

        public DateTime TokenExpiresAt { get; private set; }

        public string? GuildId { get; private set; }

        public string? ChannelId { get; private set; }

        protected DiscordConnection()
        {
        }

        public DiscordConnection(
            Guid userId,
            string discordUserId)
        {
            UserId = userId;
            DiscordUserId = discordUserId;
        }

        public void UpdateConnection(
            string discordUserId,
            string accessToken,
            string refreshToken,
            DateTime tokenExpiresAt)
        {
            DiscordUserId = discordUserId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            TokenExpiresAt = tokenExpiresAt;
        }

        public void SetConfiguration(
            string guildId,
            string channelId)
        {
            GuildId = guildId;
            ChannelId = channelId;
        }
    }
}
