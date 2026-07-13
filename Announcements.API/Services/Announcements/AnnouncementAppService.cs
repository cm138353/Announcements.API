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

        public async Task<AnnouncementDto> GenerateAsync(
            CreateAnnouncementDto input)
        {
            var announcement = new Announcement
            {
                Title = input.Title,
                EventType = input.EventType,
                EventDate = input.EventDate,
                Tone = input.Tone,
                RoughNotes = input.RoughNotes,
                Status = AnnouncementStatus.Draft
            };

            var generatedContent =
                await _openAiService.GenerateAsync(announcement);

            announcement.DiscordContent =
                generatedContent.DiscordContent;

            announcement.ClanMailContent =
                generatedContent.ClanMailContent;

            announcement.ClanChatContent =
                generatedContent.ClanChatContent;

            announcement.Status = AnnouncementStatus.Generated;

            await _announcementRepository.InsertAsync(
                announcement,
                autoSave: true);

            return ObjectMapper.Map<Announcement, AnnouncementDto>(announcement);
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
