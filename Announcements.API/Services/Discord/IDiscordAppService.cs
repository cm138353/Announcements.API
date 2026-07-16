using Announcements.API.Services.Dtos;
using Volo.Abp.Application.Services;

namespace Announcements.API.Services.Discord
{
    public interface IDiscordAppService : IApplicationService
    {
        Task<DiscordConnectUrlDto> GetConnectUrlAsync();

        Task<List<DiscordGuildDto>> GetServersAsync();

        Task<List<DiscordChannelDto>> GetChannelsAsync(string guildId);

        Task SaveConfigurationAsync(SaveDiscordConfigurationDto input);
        Task<DiscordConfigurationDto> GetConfigurationAsync();
    }
}
