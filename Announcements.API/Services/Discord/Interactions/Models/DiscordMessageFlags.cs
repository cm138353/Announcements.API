namespace Announcements.API.Services.Discord.Interactions.Models
{
    [Flags]
    public enum DiscordMessageFlags
    {
        None = 0,
        Ephemeral = 1 << 6
    }
}
