using Announcements.API.Entities.Announcements;

namespace Announcements.API.BackgroundJobs.Announcements
{
    public class GenerateAnnouncementJobArgs
    {
        // Discord interaction data
        public string ApplicationId { get; set; } = string.Empty;

        public string InteractionToken { get; set; } = string.Empty;

        public string DiscordUserId { get; set; } = string.Empty;

        public string? GuildId { get; set; }

        public string? ChannelId { get; set; }

        // Application announcement data
        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public ClashEventType EventType { get; set; }

        public DateTime EventDate { get; set; }

        public AnnouncementTone Tone { get; set; }

        public string RoughNotes { get; set; } = string.Empty;
    }
}
