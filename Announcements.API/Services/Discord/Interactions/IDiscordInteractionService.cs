using Announcements.API.Services.Discord.Interactions.Models;
using Announcements.API.Services.Dtos.Interactions;

namespace Announcements.API.Services.Discord.Interactions
{
    public interface IDiscordInteractionService
    {
        Task<DiscordInteractionResponse> HandleAsync(DiscordInteractionRequest request, CancellationToken cancellationToken = default);
    }
}
