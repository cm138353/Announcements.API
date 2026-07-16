using Announcements.API.Services.Dtos;
using Volo.Abp.Application.Services;

namespace Announcements.API.Services.Announcements
{
    public interface IAnnouncementAppService : IApplicationService
    {
        Task<List<AnnouncementDto>> GetListAsync();

        Task<AnnouncementDto> GetAsync(Guid id);

        Task<AnnouncementDto> GenerateAsync(CreateAnnouncementDto input);

        Task<AnnouncementDto> UpdateAsync(
            Guid id,
            UpdateAnnouncementDto input);

        Task<AnnouncementDto> PublishAsync(Guid id);

        Task DeleteAsync(Guid id);
    }
}
