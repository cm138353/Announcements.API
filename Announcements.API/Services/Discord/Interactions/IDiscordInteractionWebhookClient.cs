namespace Announcements.API.Services.Discord.Interactions
{
    public interface IDiscordInteractionWebhookClient
    {
        Task EditOriginalResponseAsync(string applicationId, string interactionToken, string content, CancellationToken cancellationToken = default);
    }
}
