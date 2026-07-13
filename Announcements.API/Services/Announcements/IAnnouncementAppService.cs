using Announcements.API.Services.Dtos;
using Volo.Abp.Application.Services;

namespace Announcements.API.Services.Announcements
{
    public interface IAnnouncementAppService : IApplicationService
    {
        Task<AnnouncementDto> GenerateAsync(CreateAnnouncementDto input);

        Task<AnnouncementDto> PublishAsync(
            Guid id,
            PublishAnnouncementDto input);

        Task<List<AnnouncementDto>> GetListAsync();

        Task<AnnouncementDto> GetAsync(Guid id);
    }
}
