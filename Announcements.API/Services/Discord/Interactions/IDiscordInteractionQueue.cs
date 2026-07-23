using Announcements.API.Services.Discord.Interactions.Models;

namespace Announcements.API.Services.Discord.Interactions
{
    public interface IDiscordInteractionQueue
    {
        ValueTask EnqueueAsync(DiscordInteractionRequest interaction, CancellationToken cancellationToken = default);

        ValueTask<DiscordInteractionRequest> DequeueAsync(CancellationToken cancellationToken = default);
    }
}
