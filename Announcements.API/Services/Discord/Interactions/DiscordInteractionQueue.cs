using Announcements.API.Services.Discord.Interactions.Models;
using System.Threading.Channels;
using Volo.Abp.DependencyInjection;

namespace Announcements.API.Services.Discord.Interactions
{
    public class DiscordInteractionQueue : IDiscordInteractionQueue, ISingletonDependency
    {
        private const int QueueCapacity = 1_000;

        private readonly Channel<DiscordInteractionRequest> _queue;

        public DiscordInteractionQueue()
        {
            var options = new BoundedChannelOptions(QueueCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropWrite
            };

            _queue =
                Channel.CreateBounded<DiscordInteractionRequest>(options);
        }

        public ValueTask EnqueueAsync(
            DiscordInteractionRequest interaction,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(interaction);

            cancellationToken.ThrowIfCancellationRequested();

            if (!_queue.Writer.TryWrite(interaction))
            {
                throw new InvalidOperationException(
                    "The Discord interaction queue is currently full.");
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<DiscordInteractionRequest> DequeueAsync(
            CancellationToken cancellationToken = default)
        {
            return _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
