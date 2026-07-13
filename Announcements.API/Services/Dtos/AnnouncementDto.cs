using Announcements.API.Entities.Announcements;

namespace Announcements.API.Services.Dtos
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public ClashEventType EventType { get; set; }

        public DateTime? EventDate { get; set; }

        public AnnouncementTone Tone { get; set; }

        public AnnouncementStatus Status { get; set; }

        public string RoughNotes { get; set; } = string.Empty;

        public string? DiscordContent { get; set; }

        public string? ClanMailContent { get; set; }

        public string? ClanChatContent { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}
