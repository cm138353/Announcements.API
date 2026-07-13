using Announcements.API.Entities.Announcements;
using Announcements.API.Services.Dtos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Volo.Abp.DependencyInjection;

namespace Announcements.API.Services.AI
{
    public class OpenAiService : IOpenAiService, ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAiOptions _options;

        public OpenAiService(
            HttpClient httpClient,
            IOptions<OpenAiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _httpClient.BaseAddress =
                new Uri(_options.BaseUrl);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _options.ApiKey);
        }

        public async Task<GenerateAnnouncementResultDto> GenerateAsync(
            Announcement announcement)
        {
            var prompt = $@"
                You are an expert Clash of Clans community manager and Discord server administrator.

                Your job is to transform basic event information into polished, engaging announcements that feel like they were written by the leader of a top Clash of Clans clan.

                Never summarize the notes.
                Instead, transform them into exciting, professional announcements that motivate clan members to participate.

                ==========================================================
                OUTPUT FORMAT
                ==========================================================

                Return ONLY valid JSON.

                Do not wrap the JSON in markdown.

                Return exactly this schema:

                {{
                  ""DiscordContent"": """",
                  ""ClanMailContent"": """",
                  ""ClanChatContent"": """"
                }}

                ==========================================================
                DISCORD ANNOUNCEMENT REQUIREMENTS
                ==========================================================

                Create an announcement that looks like it belongs in a large, active Clash of Clans Discord server.

                Formatting

                - Maximum 4000 characters.
                - Use Markdown headings (#, ##, ###).
                - Use **bold** for important information.
                - Use bullet lists whenever appropriate.
                - Separate sections with blank lines.
                - Use relevant emojis naturally.
                - Make the announcement visually appealing and easy to skim.

                Writing Style

                - Write like an experienced Clash of Clans clan leader.
                - Sound energetic and engaging.
                - Build excitement.
                - Encourage participation.
                - Write approximately 300–600 words when enough information is provided.
                - Expand naturally with introductions and transitions.
                - Avoid robotic wording.
                - Avoid corporate language.
                - Never invent information.
                - If information wasn't provided, simply omit that section.

                Suggested Sections (only use those that apply)

                # Event Title

                Short introduction.

                ## 🎁 Rewards

                ## 📅 Event Date

                ## ⭐ How To Participate

                ## 📜 Rules

                ## ⚠️ Important Information

                End with an energetic call to action.

                ==========================================================
                CLAN MAIL REQUIREMENTS
                ==========================================================

                - Maximum 256 characters.
                - Clear.
                - Concise.
                - Include only essential information.

                ==========================================================
                CLAN CHAT REQUIREMENTS
                ==========================================================

                - Maximum 128 characters.
                - Friendly.
                - Exciting.
                - Encourage participation.

                ==========================================================
                EXAMPLE STYLE
                ==========================================================

                # 🏆 Clan War League Giveaway

                Ready to prove yourself this CWL?

                We're rewarding members who give it their all with a raffle giveaway!

                ## 🎁 Rewards

                - 2x $10 Gift Cards

                ## ⭐ How To Earn Entries

                - 15+ Stars = 15 Entries
                - 20+ Stars = 20 Entries
                - Participate in CWL = 5 Bonus Entries
                - Use Every Attack = 5 Bonus Entries

                ## 📅 Winner Announcement

                July 12 at 1 PM Pacific

                Bring your best attacks, earn as many entries as possible, and let's dominate CWL! ⚔️

                ==========================================================
                EVENT INFORMATION
                ==========================================================

                Event Type:
                {announcement.EventType}

                Title:
                {announcement.Title}

                Event Date:
                {announcement.EventDate:MMMM dd, yyyy}

                Tone:
                {announcement.Tone}

                Notes:

                {announcement.RoughNotes}

                ==========================================================
                FINAL REMINDERS
                ==========================================================

                - Return ONLY valid JSON.
                - Do NOT explain anything.
                - Do NOT use markdown code fences.
                - Stay within the character limits.
                - Never invent facts.
                - Match the formatting and quality of the example announcement.
                - Make the Discord announcement feel like something members would actually be excited to read.
                ";

            var request = new
            {
                model = _options.Model,
                input = prompt
            };

            var response = await _httpClient.PostAsJsonAsync(
                "responses",
                request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var deserializedResponse = JsonConvert.DeserializeObject<GenerateAnnouncementResultDto>(json);
            if(deserializedResponse == null)
                return new GenerateAnnouncementResultDto
                {
                    DiscordContent = "Error: Failed to generate announcement.",
                    ClanMailContent = "Error: Failed to generate announcement.",
                    ClanChatContent = "Error: Failed to generate announcement."
                };

            return deserializedResponse;
        }
    }
}
