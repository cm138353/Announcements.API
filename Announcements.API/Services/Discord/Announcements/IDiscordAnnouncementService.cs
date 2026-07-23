using Announcements.API.Entities.Announcements;

namespace Announcements.API.Services.Discord.Announcements
{
    public interface IDiscordAnnouncementService
    {
        Task<string> GenerateAsync(Guid userId,string title,ClashEventType eventType,DateTime eventDate,AnnouncementTone tone,string roughNotes,CancellationToken cancellationToken = default);
    }
}
