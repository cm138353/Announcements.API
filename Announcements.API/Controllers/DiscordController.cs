using Announcements.API.Entities.Discord;
using Announcements.API.Services.Discord;
using Announcements.API.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;

namespace Announcements.API.Controllers
{
    [Route("api/[Controller]")]
    public class DiscordController : AbpController
    {
        private readonly IDiscordService _discordService;
        private readonly IRepository<DiscordConnection, Guid> _connectionRepository;
        private readonly IDistributedCache<DiscordOAuthStateCacheItem, string> _stateCache;
        private readonly IStringEncryptionService _encryptionService;
        private readonly IConfiguration _configuration;

        public DiscordController(
            IDiscordService discordService,
            IRepository<DiscordConnection, Guid> connectionRepository,
            IDistributedCache<DiscordOAuthStateCacheItem, string> stateCache,
            IStringEncryptionService encryptionService,
            IConfiguration configuration)
        {
            _discordService = discordService;
            _connectionRepository = connectionRepository;
            _stateCache = stateCache;
            _encryptionService = encryptionService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync(
            [FromQuery] string code,
            [FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(state))
            {
                return RedirectToFrontend("discordError=invalid_callback");
            }

            var stateItem = await _stateCache.GetAsync(state);

            if (stateItem == null)
            {
                return RedirectToFrontend("discordError=invalid_state");
            }

            // State values should only be usable once.
            await _stateCache.RemoveAsync(state);

            var token = await _discordService.ExchangeCodeAsync(code);

            var discordUser = await _discordService.GetCurrentUserAsync(
                token.AccessToken);

            var connection = await _connectionRepository.FirstOrDefaultAsync(
                x => x.UserId == stateItem.UserId);

            if (connection == null)
            {
                connection = new DiscordConnection(
                    stateItem.UserId,
                    discordUser.Id);

                await _connectionRepository.InsertAsync(connection);
            }

            connection.UpdateConnection(
                discordUser.Id,
                _encryptionService.Encrypt(token.AccessToken),
                _encryptionService.Encrypt(token.RefreshToken),
                Clock.Now.AddSeconds(token.ExpiresIn));

            await _connectionRepository.UpdateAsync(
                connection,
                autoSave: true);

            return RedirectToFrontend("connected=true");
        }

        private IActionResult RedirectToFrontend(string query)
        {
            var clientUrl = _configuration["App:ClientUrl"]
                ?? throw new BusinessException(
                    "App:ClientUrl is not configured.");

            return Redirect(
                $"{clientUrl.TrimEnd('/')}/settings/discord?{query}");
        }
    }
}
