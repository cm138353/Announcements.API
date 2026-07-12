using Announcements.API.Services.Dtos;
using Volo.Abp.Application.Services;

namespace Announcements.API.Services.Announcements
{
    public interface IAnnouncementAppService : IApplicationService
    {
        Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto input);
        Task<GenerateAnnouncementResultDto> GenerateAsync(Guid id);
        Task<AnnouncementDto> UpdateAsync(Guid id, UpdateAnnouncementDto input);
        Task<PublishAnnouncementDto> PublishAsync(Guid id, PublishAnnouncementDto input);
        Task<AnnouncementDto> GetAsync(Guid id);
        Task<List<AnnouncementDto>> GetListAsync();
        Task DeleteAsync(Guid id);
    }
}
