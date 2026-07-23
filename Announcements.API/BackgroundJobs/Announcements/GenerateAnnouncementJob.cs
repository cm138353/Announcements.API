using Announcements.API.Services.Discord.Announcements;
using Announcements.API.Services.Discord.Interactions;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Announcements.API.BackgroundJobs.Announcements
{
    public class GenerateAnnouncementJob : AsyncBackgroundJob<GenerateAnnouncementJobArgs>, ITransientDependency
    {
        private readonly IDiscordAnnouncementService _discordAnnouncementService;

        private readonly IDiscordInteractionWebhookClient _discordInteractionWebhookClient;

        private readonly ILogger<GenerateAnnouncementJob> _logger;

        public GenerateAnnouncementJob(
            IDiscordAnnouncementService discordAnnouncementService,
            IDiscordInteractionWebhookClient discordInteractionWebhookClient,
            ILogger<GenerateAnnouncementJob> logger)
        {
            _discordAnnouncementService = discordAnnouncementService;
            _discordInteractionWebhookClient = discordInteractionWebhookClient;
            _logger = logger;
        }

        public override async Task ExecuteAsync(GenerateAnnouncementJobArgs args)
        {
            ArgumentNullException.ThrowIfNull(args);

            try
            {
                var content = await _discordAnnouncementService.GenerateAsync(
                    args.UserId,
                    args.Title,
                    args.EventType,
                    args.EventDate,
                    args.Tone,
                    args.RoughNotes);

                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new InvalidOperationException(
                        "Announcement generation returned empty content.");
                }

                await _discordInteractionWebhookClient.EditOriginalResponseAsync(args.ApplicationId,args.InteractionToken,content);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Discord announcement generation failed. " +
                    "GuildId: {GuildId}, ChannelId: {ChannelId}, " +
                    "UserId: {UserId}",
                    args.GuildId,
                    args.ChannelId,
                    args.UserId);

                await SendFailureResponseAsync(args);
            }
        }

        private async Task SendFailureResponseAsync(
            GenerateAnnouncementJobArgs args)
        {
            try
            {
                await _discordInteractionWebhookClient
                    .EditOriginalResponseAsync(
                        args.ApplicationId,
                        args.InteractionToken,
                        "I couldn't generate the announcement. Please try again.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to send the failure response to Discord. " +
                    "GuildId: {GuildId}, ChannelId: {ChannelId}",
                    args.GuildId,
                    args.ChannelId);
            }
        }
    }
}
