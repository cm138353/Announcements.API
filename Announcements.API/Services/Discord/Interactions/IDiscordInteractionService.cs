using Announcements.API.Services.Discord.Interactions.Models;

namespace Announcements.API.Services.Discord.Interactions
{
    public interface IDiscordInteractionService
    {
        Task ProcessDeferredAsync(DiscordInteractionRequest request);
    }
}