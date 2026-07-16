namespace Announcements.API.Services.Dtos
{
    public class DiscordGuildDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool Owner { get; set; }

        public string Permissions { get; set; } = "0";

        public string? Icon { get; set; }
    }
}
