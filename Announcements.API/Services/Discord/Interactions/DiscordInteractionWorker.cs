using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Announcements.API.Services.Discord.Interactions
{
    public sealed class DiscordInteractionWorker : BackgroundService
    {
        private readonly IDiscordInteractionQueue _interactionQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DiscordInteractionWorker> _logger;

        public DiscordInteractionWorker(
            IDiscordInteractionQueue interactionQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DiscordInteractionWorker> logger)
        {
            _interactionQueue = interactionQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Discord interaction worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var interaction =
                        await _interactionQueue.DequeueAsync(
                            stoppingToken);

                    _logger.LogInformation(
                        "Dequeued Discord interaction {InteractionId}.",
                        interaction.Id);

                    using var scope =
                        _serviceScopeFactory.CreateScope();

                    var interactionService =
                        scope.ServiceProvider
                            .GetRequiredService<IDiscordInteractionService>();

                    await interactionService.ProcessDeferredAsync(
                        interaction);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "An unexpected error occurred while processing a " +
                        "queued Discord interaction.");
                }
            }

            _logger.LogInformation(
                "Discord interaction worker stopped.");
        }
    }
}