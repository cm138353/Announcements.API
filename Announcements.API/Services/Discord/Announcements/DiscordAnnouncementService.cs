using Announcements.API.Entities.Announcements;
using Announcements.API.Services.AI;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Announcements.API.Services.Discord.Announcements
{
    public class DiscordAnnouncementService : IDiscordAnnouncementService
    {
        private readonly IRepository<Announcement, Guid>
            _announcementRepository;

        private readonly IOpenAiService _openAiService;

        public DiscordAnnouncementService(
            IRepository<Announcement, Guid> announcementRepository,
            IOpenAiService openAiService)
        {
            _announcementRepository = announcementRepository;
            _openAiService = openAiService;
        }

        [UnitOfWork]
        public async Task<string> GenerateAsync(
            Guid userId,
            string title,
            ClashEventType eventType,
            DateTime eventDate,
            AnnouncementTone tone,
            string roughNotes,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid application user ID is required.",
                    nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "An announcement title is required.",
                    nameof(title));
            }

            if (string.IsNullOrWhiteSpace(roughNotes))
            {
                throw new ArgumentException(
                    "Announcement notes are required.",
                    nameof(roughNotes));
            }

            var announcement = new Announcement(
                userId,
                title.Trim(),
                eventType,
                eventDate,
                tone,
                roughNotes.Trim());

            var generatedContent =
                await _openAiService.GenerateAsync(
                    announcement);

            if (string.IsNullOrWhiteSpace(
                    generatedContent.DiscordContent))
            {
                throw new InvalidOperationException(
                    "OpenAI returned empty Discord content.");
            }

            announcement.Generate(
                generatedContent.DiscordContent,
                generatedContent.ClanMailContent,
                generatedContent.ClanChatContent);

            await _announcementRepository.InsertAsync(
                announcement,
                autoSave: true,
                cancellationToken: cancellationToken);

            return generatedContent.DiscordContent;
        }
    }
}
