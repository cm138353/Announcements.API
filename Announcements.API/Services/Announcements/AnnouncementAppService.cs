using Announcements.API.Entities.Announcements;
using Announcements.API.Services.AI;
using Announcements.API.Services.Discord;
using Announcements.API.Services.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Announcements.API.Services.Announcements
{
    public class AnnouncementAppService : APIAppService, IAnnouncementAppService
    {
        private readonly IRepository<Announcement, Guid> _announcementRepository;
        private readonly IOpenAiService _openAiService;
        private readonly IDiscordService _discordService;

        public AnnouncementAppService(
            IRepository<Announcement, Guid> announcementRepository,
            IOpenAiService openAiService,
            IDiscordService discordService)
        {
            _announcementRepository = announcementRepository;
            _openAiService = openAiService;
            _discordService = discordService;
        }

        public Task<AnnouncementDto> GenerateAsync(CreateAnnouncementDto input)
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

        public Task<AnnouncementDto> PublishAsync(Guid id, PublishAnnouncementDto input)
        {
            throw new NotImplementedException();
        }
    }
}
