using Announcements.API.Entities.Announcements;

namespace Announcements.API.Services.Dtos
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;

        public ClashEventType EventType { get; set; }

        public AnnouncementTone Tone { get; set; }

        public DateTime? EventDate { get; set; }

        public string RoughNotes { get; set; } = string.Empty;
    }
}
