using Announcements.API.Entities.Discord;
using Announcements.API.Services.Announcements;
using Announcements.API.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace Announcements.API.Services.Discord
{
    [Authorize]
    public class DiscordAppService : APIAppService, IDiscordAppService
    {
        private readonly IDiscordService _discordService;
        private readonly IRepository<DiscordConnection, Guid> _connectionRepository;
        private readonly IDistributedCache<DiscordOAuthStateCacheItem, string> _stateCache;
        private readonly IStringEncryptionService _encryptionService;

        public DiscordAppService(
            IDiscordService discordService,
            IRepository<DiscordConnection, Guid> connectionRepository,
            IDistributedCache<DiscordOAuthStateCacheItem, string> stateCache,
            IStringEncryptionService encryptionService)
        {
            _discordService = discordService;
            _connectionRepository = connectionRepository;
            _stateCache = stateCache;
            _encryptionService = encryptionService;
        }

        public async Task<DiscordConnectUrlDto> GetConnectUrlAsync()
        {
            var state = Guid.NewGuid().ToString("N");

            await _stateCache.SetAsync(
                state,
                new DiscordOAuthStateCacheItem
                {
                    UserId = CurrentUser.GetId()
                },
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return new DiscordConnectUrlDto
            {
                Url = _discordService.BuildAuthorizationUrl(state),
                State = state
            };
        }

        public async Task<List<DiscordGuildDto>> GetServersAsync()
        {
            var connection = await GetConnectionAsync();

            var accessToken = _encryptionService.Decrypt(
                connection.AccessToken);

            return await _discordService.GetManagedGuildsAsync(
                accessToken);
        }

        public async Task<List<DiscordChannelDto>> GetChannelsAsync(
            string guildId)
        {
            return await _discordService.GetGuildChannelsAsync(guildId);
        }

        public async Task SaveConfigurationAsync(
            SaveDiscordConfigurationDto input)
        {
            var connection = await GetConnectionAsync();

            connection.SetConfiguration(
                input.GuildId,
                input.ChannelId);

            await _connectionRepository.UpdateAsync(
                connection,
                autoSave: true);
        }

        private async Task<DiscordConnection> GetConnectionAsync()
        {
            var connection = await _connectionRepository.FirstOrDefaultAsync(
                x => x.UserId == CurrentUser.GetId());

            if (connection == null)
            {
                throw new BusinessException(
                    "Discord account has not been connected.");
            }

            return connection;
        }

        public async Task<DiscordConfigurationDto> GetConfigurationAsync()
        {
            var connection = await _connectionRepository.FirstOrDefaultAsync(
                x => x.UserId == CurrentUser.GetId());

            if (connection == null)
            {
                return new DiscordConfigurationDto
                {
                    IsConnected = false
                };
            }

            return new DiscordConfigurationDto
            {
                IsConnected = true,
                GuildId = connection.GuildId,
                ChannelId = connection.ChannelId
            };
        }
    }
}
