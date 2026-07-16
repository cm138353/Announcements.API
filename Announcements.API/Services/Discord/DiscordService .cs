using Announcements.API.Services.Dtos;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace Announcements.API.Services.Discord
{
    public class DiscordService : IDiscordService, ITransientDependency
    {
        private const long ManageGuildPermission = 1L << 5;

        private readonly HttpClient _httpClient;
        private readonly DiscordOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        public DiscordService(
            HttpClient httpClient,
            IOptions<DiscordOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _httpClient.BaseAddress =
                new Uri("https://discord.com/api/v10/");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public string BuildAuthorizationUrl(string state)
        {
            var query = new Dictionary<string, string?>
            {
                ["client_id"] = _options.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = _options.RedirectUri,
                ["scope"] = "identify guilds",
                ["state"] = state,
                ["prompt"] = "consent"
            };

            return QueryString.Create(query).ToUriComponent()
                .Insert(0, "https://discord.com/oauth2/authorize");
        }

        public async Task<DiscordTokenDto> ExchangeCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "oauth2/token");

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = _options.ClientId,
                    ["client_secret"] = _options.ClientSecret,
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = _options.RedirectUri
                });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            return await ReadResponseAsync<DiscordTokenDto>(
                response,
                cancellationToken);
        }

        public async Task<DiscordUserDto> GetCurrentUserAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Get,
                "users/@me",
                "Bearer",
                accessToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            return await ReadResponseAsync<DiscordUserDto>(
                response,
                cancellationToken);
        }

        public async Task<List<DiscordGuildDto>> GetManagedGuildsAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Get,
                "users/@me/guilds",
                "Bearer",
                accessToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var guilds = await ReadResponseAsync<List<DiscordGuildDto>>(
                response,
                cancellationToken);

            return guilds
                .Where(x => x.Owner || HasManageGuildPermission(x.Permissions))
                .ToList();
        }

        public async Task<List<DiscordGuildDto>> GetBotGuildsAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Get,
                "users/@me/guilds",
                "Bot",
                _options.BotToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            return await ReadResponseAsync<List<DiscordGuildDto>>(
                response,
                cancellationToken);
        }

        public async Task<List<DiscordChannelDto>> GetGuildChannelsAsync(
            string guildId,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Get,
                $"guilds/{guildId}/channels",
                "Bot",
                _options.BotToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var channels = await ReadResponseAsync<List<DiscordChannelDto>>(
                response,
                cancellationToken);

            // 0 = Guild Text, 5 = Guild Announcement
            return channels
                .Where(x => x.Type is 0 or 5)
                .OrderBy(x => x.Position)
                .ToList();
        }

        public async Task SendMessageAsync(
            string channelId,
            string content,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Post,
                $"channels/{channelId}/messages",
                "Bot",
                _options.BotToken);

            request.Content = JsonContent.Create(new
            {
                content
            });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }

        private static HttpRequestMessage CreateRequest(
            HttpMethod method,
            string url,
            string authenticationScheme,
            string token)
        {
            var request = new HttpRequestMessage(method, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    authenticationScheme,
                    token);

            return request;
        }

        private async Task<T> ReadResponseAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);

            return await response.Content.ReadFromJsonAsync<T>(
                       _jsonOptions,
                       cancellationToken)
                   ?? throw new InvalidOperationException(
                       "Discord returned an empty response.");
        }

        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"Discord returned {(int)response.StatusCode}: {body}");
        }

        private static bool HasManageGuildPermission(
            string permissions)
        {
            return long.TryParse(
                       permissions,
                       NumberStyles.None,
                       CultureInfo.InvariantCulture,
                       out var value)
                   && (value & ManageGuildPermission) != 0;
        }
    }
}
