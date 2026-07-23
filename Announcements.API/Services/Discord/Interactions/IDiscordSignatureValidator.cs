namespace Announcements.API.Services.Discord.Interactions
{
    public interface IDiscordSignatureValidator
    {
        bool IsValid(string signature, string timestamp, string rawRequestBody);
    }
}
