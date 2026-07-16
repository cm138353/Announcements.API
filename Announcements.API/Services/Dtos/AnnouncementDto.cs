using Announcements.API.Entities.Announcements;

namespace Announcements.API.Services.Dtos
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public ClashEventType EventType { get; set; }

        public DateTime EventDate { get; set; }

        public AnnouncementTone Tone { get; set; }

        public AnnouncementStatus Status { get; set; }

        public string RoughNotes { get; set; } = string.Empty;

        public string DiscordContent { get; set; } = string.Empty;

        public string ClanMailContent { get; set; } = string.Empty;

        public string ClanChatContent { get; set; } = string.Empty;

        public DateTime? PublishedAt { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
