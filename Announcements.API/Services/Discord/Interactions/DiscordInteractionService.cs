using Announcements.API.BackgroundJobs.Announcements;
using Announcements.API.Entities.Announcements;
using Announcements.API.Entities.Discord;
using Announcements.API.Services.Discord.Interactions.Models;
using System.Globalization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Announcements.API.Services.Discord.Interactions
{
    public sealed class DiscordInteractionService
        : IDiscordInteractionService,
          ITransientDependency
    {
        private const string AnnouncementCommandName = "announcement";
        private const string CreateSubcommandName = "create";

        private const string TitleOptionName = "title";
        private const string EventTypeOptionName = "event-type";
        private const string EventDateOptionName = "event-date";
        private const string ToneOptionName = "tone";
        private const string NotesOptionName = "notes";

        private readonly IBackgroundJobManager _backgroundJobManager;

        private readonly IRepository<DiscordConnection, Guid>
            _discordConnectionRepository;

        private readonly IDiscordInteractionWebhookClient
            _discordInteractionWebhookClient;

        private readonly ILogger<DiscordInteractionService> _logger;

        public DiscordInteractionService(
            IBackgroundJobManager backgroundJobManager,
            IRepository<DiscordConnection, Guid>
                discordConnectionRepository,
            IDiscordInteractionWebhookClient
                discordInteractionWebhookClient,
            ILogger<DiscordInteractionService> logger)
        {
            _backgroundJobManager = backgroundJobManager;
            _discordConnectionRepository = discordConnectionRepository;
            _discordInteractionWebhookClient =
                discordInteractionWebhookClient;
            _logger = logger;
        }

        public async Task ProcessDeferredAsync(
            DiscordInteractionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                await ProcessApplicationCommandAsync(
                    request,
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred while processing deferred " +
                    "Discord interaction {InteractionId}.",
                    request.Id);

                await TryEditOriginalResponseAsync(
                    request,
                    "❌ An unexpected error occurred while processing " +
                    "the command.");
            }
        }

        private async Task ProcessApplicationCommandAsync(
            DiscordInteractionRequest request,
            CancellationToken cancellationToken)
        {
            var commandName = request.Data?.Name;

            if (string.IsNullOrWhiteSpace(commandName))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "The interaction did not include a command name.");

                return;
            }

            switch (commandName.ToLowerInvariant())
            {
                case AnnouncementCommandName:
                    await ProcessAnnouncementCommandAsync(
                        request,
                        cancellationToken);

                    break;

                default:
                    await TryEditOriginalResponseAsync(
                        request,
                        $"The command `/{commandName}` is not supported.");

                    break;
            }
        }

        private async Task ProcessAnnouncementCommandAsync(
            DiscordInteractionRequest request,
            CancellationToken cancellationToken)
        {
            var subcommand = request.Data?.Options?.FirstOrDefault();

            if (subcommand is null ||
                string.IsNullOrWhiteSpace(subcommand.Name))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Please provide an announcement subcommand.");

                return;
            }

            switch (subcommand.Name.ToLowerInvariant())
            {
                case CreateSubcommandName:
                    await ProcessCreateAnnouncementAsync(
                        request,
                        subcommand,
                        cancellationToken);

                    break;

                // Add future subcommands here:
                //
                // case EditSubcommandName:
                //     await ProcessEditAnnouncementAsync(
                //         request,
                //         subcommand,
                //         cancellationToken);
                //     break;
                //
                // case ScheduleSubcommandName:
                //     await ProcessScheduleAnnouncementAsync(
                //         request,
                //         subcommand,
                //         cancellationToken);
                //     break;

                default:
                    await TryEditOriginalResponseAsync(
                        request,
                        $"The announcement subcommand " +
                        $"`{subcommand.Name}` is not supported.");

                    break;
            }
        }

        private async Task ProcessCreateAnnouncementAsync(
            DiscordInteractionRequest request,
            DiscordInteractionOption subcommand,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ApplicationId) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                _logger.LogWarning(
                    "Discord interaction {InteractionId} did not include an " +
                    "application ID or interaction token.",
                    request.Id);

                return;
            }

            var discordUserId = GetDiscordUserId(request);

            if (string.IsNullOrWhiteSpace(discordUserId))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Discord could not identify your account.");

                return;
            }

            var title = GetOptionValue(
                subcommand,
                TitleOptionName);

            var eventTypeValue = GetOptionValue(
                subcommand,
                EventTypeOptionName);

            var eventDateValue = GetOptionValue(
                subcommand,
                EventDateOptionName);

            var toneValue = GetOptionValue(
                subcommand,
                ToneOptionName);

            var roughNotes = GetOptionValue(
                subcommand,
                NotesOptionName);

            if (string.IsNullOrWhiteSpace(title))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Please provide an announcement title.");

                return;
            }

            if (!Enum.TryParse<ClashEventType>(
                    eventTypeValue,
                    ignoreCase: true,
                    out var eventType))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Please select a valid event type.");

                return;
            }

            if (!DateTime.TryParseExact(
                    eventDateValue,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var eventDate))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "The event date must use the format YYYY-MM-DD.");

                return;
            }

            if (!Enum.TryParse<AnnouncementTone>(
                    toneValue,
                    ignoreCase: true,
                    out var tone))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Please select a valid announcement tone.");

                return;
            }

            if (string.IsNullOrWhiteSpace(roughNotes))
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Please provide announcement notes.");

                return;
            }

            var connection =
                await _discordConnectionRepository.FirstOrDefaultAsync(
                    connection =>
                        connection.DiscordUserId == discordUserId,
                    cancellationToken: cancellationToken);

            if (connection is null)
            {
                await TryEditOriginalResponseAsync(
                    request,
                    "Your Discord account is not linked to an " +
                    "application account.");

                return;
            }

            await _backgroundJobManager.EnqueueAsync(
                new GenerateAnnouncementJobArgs
                {
                    ApplicationId = request.ApplicationId,
                    InteractionToken = request.Token,

                    UserId = connection.UserId,
                    DiscordUserId = discordUserId,

                    GuildId = request.GuildId,
                    ChannelId = request.ChannelId,

                    Title = title.Trim(),
                    EventType = eventType,
                    EventDate = eventDate,
                    Tone = tone,
                    RoughNotes = roughNotes.Trim()
                });

            _logger.LogInformation(
                "Queued announcement generation job for Discord " +
                "interaction {InteractionId} and user {DiscordUserId}.",
                request.Id,
                discordUserId);
        }

        private async Task TryEditOriginalResponseAsync(
            DiscordInteractionRequest request,
            string content)
        {
            if (string.IsNullOrWhiteSpace(request.ApplicationId) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                _logger.LogWarning(
                    "Could not edit the original response for Discord " +
                    "interaction {InteractionId} because the application ID " +
                    "or interaction token was missing.",
                    request.Id);

                return;
            }

            try
            {
                await _discordInteractionWebhookClient
                    .EditOriginalResponseAsync(
                        request.ApplicationId,
                        request.Token,
                        content);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Could not edit the original response for Discord " +
                    "interaction {InteractionId}.",
                    request.Id);
            }
        }

        private static string? GetOptionValue(
            DiscordInteractionOption subcommand,
            string optionName)
        {
            return subcommand.Options?
                .FirstOrDefault(
                    option => string.Equals(
                        option.Name,
                        optionName,
                        StringComparison.OrdinalIgnoreCase))
                ?.GetStringValue();
        }

        private static string? GetDiscordUserId(
            DiscordInteractionRequest request)
        {
            return request.Member?.User?.Id
                ?? request.User?.Id;
        }
    }
}