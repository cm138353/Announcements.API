using Announcements.API.Entities.Announcements;
using Announcements.API.Services.Dtos;

namespace Announcements.API.Services.AI
{
    public interface IOpenAiService
    {
        Task<GenerateAnnouncementResultDto> GenerateAsync(Announcement announcement);
    }
}
