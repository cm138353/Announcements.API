using Announcements.API.Services.Dtos;

namespace Announcements.API.Services.Discord
{
    public interface IDiscordService
    {
        string BuildAuthorizationUrl(string state);

        Task<DiscordTokenDto> ExchangeCodeAsync(
            string code,
            CancellationToken cancellationToken = default);

        Task<DiscordUserDto> GetCurrentUserAsync(
            string accessToken,
            CancellationToken cancellationToken = default);

        Task<List<DiscordGuildDto>> GetManagedGuildsAsync(
            string accessToken,
            CancellationToken cancellationToken = default);

        Task<List<DiscordGuildDto>> GetBotGuildsAsync(
            CancellationToken cancellationToken = default);

        Task<List<DiscordChannelDto>> GetGuildChannelsAsync(
            string guildId,
            CancellationToken cancellationToken = default);

        Task SendMessageAsync(
            string channelId,
            string content,
            CancellationToken cancellationToken = default);
    }
}
