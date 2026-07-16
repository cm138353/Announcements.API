using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Announcements.API.Entities.Announcements
{
    public class Announcement : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }

        public string Title { get; private set; } = string.Empty;

        public ClashEventType EventType { get; private set; }

        public DateTime EventDate { get; private set; }

        public AnnouncementTone Tone { get; private set; }

        public AnnouncementStatus Status { get; private set; }

        public string RoughNotes { get; private set; } = string.Empty;

        public string DiscordContent { get; private set; } = string.Empty;

        public string ClanMailContent { get; private set; } = string.Empty;

        public string ClanChatContent { get; private set; } = string.Empty;

        public DateTime? PublishedAt { get; private set; }

        protected Announcement()
        {
        }

        public Announcement(
            Guid userId,
            string title,
            ClashEventType eventType,
            DateTime eventDate,
            AnnouncementTone tone,
            string roughNotes)
        {
            UserId = userId;
            Title = title;
            EventType = eventType;
            EventDate = eventDate;
            Tone = tone;
            RoughNotes = roughNotes;

            Status = AnnouncementStatus.Draft;
        }

        public void SetUser(Guid userId)
        {
            UserId = userId;
        }

        public void Generate(
            string discordContent,
            string clanMailContent,
            string clanChatContent)
        {
            DiscordContent = discordContent;
            ClanMailContent = clanMailContent;
            ClanChatContent = clanChatContent;

            Status = AnnouncementStatus.Generated;
        }

        public void Update(
            string title,
            ClashEventType eventType,
            DateTime eventDate,
            AnnouncementTone tone,
            string roughNotes,
            string discordContent,
            string clanMailContent,
            string clanChatContent)
        {
            Title = title;
            EventType = eventType;
            EventDate = eventDate;
            Tone = tone;
            RoughNotes = roughNotes;

            DiscordContent = discordContent;
            ClanMailContent = clanMailContent;
            ClanChatContent = clanChatContent;
        }

        public void Publish(DateTime publishedAt)
        {
            if (Status == AnnouncementStatus.Published)
            {
                throw new BusinessException(
                    "Announcement has already been published.");
            }

            Status = AnnouncementStatus.Published;
            PublishedAt = publishedAt;
        }
    }
}
