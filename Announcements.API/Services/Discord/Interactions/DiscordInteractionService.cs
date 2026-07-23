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
    public sealed class DiscordInteractionService : IDiscordInteractionService, ITransientDependency
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

        public DiscordInteractionService(
            IBackgroundJobManager backgroundJobManager,
            IRepository<DiscordConnection, Guid> discordConnectionRepository)
        {
            _backgroundJobManager = backgroundJobManager;
            _discordConnectionRepository = discordConnectionRepository;
        }

        public async Task<DiscordInteractionResponse> HandleAsync(
            DiscordInteractionRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.Type switch
            {
                DiscordInteractionType.Ping => DiscordInteractionResponse.Pong(),

                DiscordInteractionType.ApplicationCommand => await HandleApplicationCommandAsync(request, cancellationToken),

                _ => DiscordInteractionResponse.EphemeralMessage("This Discord interaction type is not supported.")
            };
        }

        private async Task<DiscordInteractionResponse> HandleApplicationCommandAsync(DiscordInteractionRequest request, CancellationToken cancellationToken)
        {
            var commandName = request.Data?.Name;

            if (string.IsNullOrWhiteSpace(commandName))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "The interaction did not include a command name.");
            }

            return commandName.ToLowerInvariant() switch
            {
                AnnouncementCommandName =>
                    await HandleAnnouncementCommandAsync(
                        request,
                        cancellationToken),

                _ => DiscordInteractionResponse.EphemeralMessage(
                    $"The command `/{commandName}` is not supported.")
            };
        }

        private async Task<DiscordInteractionResponse>
            HandleAnnouncementCommandAsync(
                DiscordInteractionRequest request,
                CancellationToken cancellationToken)
        {
            var subcommand = request.Data?.Options?
                .FirstOrDefault();

            if (subcommand is null ||
                string.IsNullOrWhiteSpace(subcommand.Name))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Please provide an announcement subcommand.");
            }

            return subcommand.Name.ToLowerInvariant() switch
            {
                CreateSubcommandName =>
                    await HandleCreateAnnouncementAsync(
                        request,
                        subcommand,
                        cancellationToken),

                // Add future subcommands here:
                // EditSubcommandName =>
                //     await HandleEditAnnouncementAsync(
                //         request,
                //         subcommand,
                //         cancellationToken),

                // ScheduleSubcommandName =>
                //     await HandleScheduleAnnouncementAsync(
                //         request,
                //         subcommand,
                //         cancellationToken),

                _ => DiscordInteractionResponse.EphemeralMessage(
                    $"The announcement subcommand `{subcommand.Name}` " +
                    "is not supported.")
            };
        }

        private async Task<DiscordInteractionResponse> HandleCreateAnnouncementAsync(DiscordInteractionRequest request, DiscordInteractionOption subcommand, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ApplicationId) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Discord did not provide the required interaction data.");
            }

            var discordUserId = GetDiscordUserId(request);

            if (string.IsNullOrWhiteSpace(discordUserId))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Discord could not identify your account.");
            }

            var connection =
                await _discordConnectionRepository.FirstOrDefaultAsync(
                    connection =>
                        connection.DiscordUserId == discordUserId,
                    cancellationToken: cancellationToken);

            if (connection is null)
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Your Discord account is not linked to an application account.");
            }

            var title = GetOptionValue(subcommand, TitleOptionName);
            var eventTypeValue = GetOptionValue(subcommand, EventTypeOptionName);
            var eventDateValue = GetOptionValue(subcommand, EventDateOptionName);
            var toneValue = GetOptionValue(subcommand, ToneOptionName);
            var roughNotes = GetOptionValue(subcommand, NotesOptionName);

            if (string.IsNullOrWhiteSpace(title))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Please provide an announcement title.");
            }

            if (!Enum.TryParse<ClashEventType>(
                    eventTypeValue,
                    ignoreCase: true,
                    out var eventType))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Please select a valid event type.");
            }

            if (!DateTime.TryParseExact(
                    eventDateValue,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var eventDate))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "The event date must use the format YYYY-MM-DD.");
            }

            if (!Enum.TryParse<AnnouncementTone>(
                    toneValue,
                    ignoreCase: true,
                    out var tone))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Please select a valid announcement tone.");
            }

            if (string.IsNullOrWhiteSpace(roughNotes))
            {
                return DiscordInteractionResponse.EphemeralMessage(
                    "Please provide announcement notes.");
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

            return DiscordInteractionResponse.DeferredChannelMessage();
        }

        private static string? GetOptionValue(
            DiscordInteractionOption subcommand,
            string optionName)
        {
            return subcommand.Options?
                .FirstOrDefault(option =>
                    string.Equals(
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
