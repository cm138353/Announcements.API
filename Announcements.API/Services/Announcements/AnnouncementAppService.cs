using Announcements.API.Entities.Announcements;
using Announcements.API.Services.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Announcements.API.Services.Announcements
{
    public class AnnouncementAppService : APIAppService, IAnnouncementAppService
    {
        public IRepository<Announcement, Guid> AnnouncementRepository { get; set; }
        public AnnouncementAppService() { }

        public Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto input)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<GenerateAnnouncementResultDto> GenerateAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<AnnouncementDto> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<AnnouncementDto>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PublishAnnouncementDto> PublishAsync(Guid id, PublishAnnouncementDto input)
        {
            throw new NotImplementedException();
        }

        public Task<AnnouncementDto> UpdateAsync(Guid id, UpdateAnnouncementDto input)
        {
            throw new NotImplementedException();
        }
    }
}
