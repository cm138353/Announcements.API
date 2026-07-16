using Announcements.API.Entities.Announcements;
using Announcements.API.Entities.Discord;
using Announcements.API.Services.AI;
using Announcements.API.Services.Discord;
using Announcements.API.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Users;

namespace Announcements.API.Services.Announcements
{
    [Authorize]
    public class AnnouncementAppService : APIAppService, IAnnouncementAppService
    {
        private readonly IRepository<Announcement, Guid> _announcementRepository;
        private readonly IRepository<DiscordConnection, Guid> _discordConnectionRepository;
        private readonly IOpenAiService _openAiService;
        private readonly IDiscordService _discordService;

        public AnnouncementAppService(
            IRepository<Announcement, Guid> announcementRepository,
            IRepository<DiscordConnection, Guid> discordConnectionRepository,
            IOpenAiService openAiService,
            IDiscordService discordService)
        {
            _announcementRepository = announcementRepository;
            _discordConnectionRepository = discordConnectionRepository;
            _openAiService = openAiService;
            _discordService = discordService;
        }

        public async Task<List<AnnouncementDto>> GetListAsync()
        {
            var userId = CurrentUser.GetId();

            var announcements = await _announcementRepository.GetListAsync(
                x => x.UserId == userId);

            return ObjectMapper.Map<
                List<Announcement>,
                List<AnnouncementDto>>(announcements);
        }

        public async Task<AnnouncementDto> GetAsync(Guid id)
        {
            var userId = CurrentUser.GetId();

            var announcement = await _announcementRepository.GetAsync(
                x => x.Id == id && x.UserId == userId);

            return ObjectMapper.Map<Announcement, AnnouncementDto>(
                announcement);
        }

        public async Task<AnnouncementDto> GenerateAsync(
            CreateAnnouncementDto input)
        {
            var announcement = new Announcement(
                CurrentUser.GetId(),
                input.Title,
                input.EventType,
                input.EventDate,
                input.Tone,
                input.RoughNotes);

            var generated = await _openAiService.GenerateAsync(
                announcement);

            announcement.Generate(
                generated.DiscordContent,
                generated.ClanMailContent,
                generated.ClanChatContent);

            await _announcementRepository.InsertAsync(
                announcement,
                autoSave: true);

            return ObjectMapper.Map<Announcement, AnnouncementDto>(
                announcement);
        }

        public async Task<AnnouncementDto> UpdateAsync(
            Guid id,
            UpdateAnnouncementDto input)
        {
            var userId = CurrentUser.GetId();

            var announcement = await _announcementRepository.GetAsync(
                x => x.Id == id && x.UserId == userId);

            announcement.Update(
                input.Title,
                input.EventType,
                input.EventDate,
                input.Tone,
                input.RoughNotes,
                input.DiscordContent,
                input.ClanMailContent,
                input.ClanChatContent);

            await _announcementRepository.UpdateAsync(
                announcement,
                autoSave: true);

            return ObjectMapper.Map<Announcement, AnnouncementDto>(
                announcement);
        }

        public async Task<AnnouncementDto> PublishAsync(Guid id)
        {
            var userId = CurrentUser.GetId();

            var announcement = await _announcementRepository.GetAsync(
                x => x.Id == id && x.UserId == userId);

            if (string.IsNullOrWhiteSpace(
                    announcement.DiscordContent))
            {
                throw new BusinessException(
                    "Announcement has no Discord content.");
            }

            var connection =
                await _discordConnectionRepository.FirstOrDefaultAsync(
                    x => x.UserId == userId);

            if (connection == null ||
                string.IsNullOrWhiteSpace(connection.ChannelId))
            {
                throw new BusinessException(
                    "Discord channel has not been configured.");
            }

            await _discordService.SendMessageAsync(
                connection.ChannelId,
                announcement.DiscordContent);

            announcement.Publish(Clock.Now);

            await _announcementRepository.UpdateAsync(
                announcement,
                autoSave: true);

            return ObjectMapper.Map<Announcement, AnnouncementDto>(
                announcement);
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = CurrentUser.GetId();

            await _announcementRepository.DeleteAsync(
                x => x.Id == id && x.UserId == userId);
        }
    }
}
