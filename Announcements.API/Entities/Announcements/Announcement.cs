using Volo.Abp.Domain.Entities.Auditing;

namespace Announcements.API.Entities.Announcements
{
    public class Announcement : FullAuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;

        public ClashEventType EventType { get; set; }

        public AnnouncementTone Tone { get; set; }

        public DateTime? EventDate { get; set; }

        public string RoughNotes { get; set; } = string.Empty;

        public AnnouncementStatus Status { get; set; }

        public string DiscordContent { get; set; } = string.Empty;

        public string ClanMailContent { get; set; } = string.Empty;

        public string ClanChatContent { get; set; } = string.Empty;

        public DateTime? PublishedAt { get; set; }
    }
}
