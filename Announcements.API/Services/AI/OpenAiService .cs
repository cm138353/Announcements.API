using Announcements.API.Entities.Announcements;
using Announcements.API.Services.Dtos;
using Microsoft.Extensions.Options;
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
                new Uri("https://api.openai.com/v1/");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _options.ApiKey);
        }

        public async Task<GenerateAnnouncementResultDto> GenerateAsync(
            Announcement announcement)
        {

            throw new NotImplementedException();
        }
    }
}
