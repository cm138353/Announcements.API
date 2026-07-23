using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Volo.Abp.AspNetCore.Mvc;

namespace Announcements.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/discord/interactions")]
    public class DiscordInteractionsController : AbpController
    {
        [HttpPost]
        public async Task<IActionResult> HandleAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            using var document = JsonDocument.Parse(body);

            var type = document.RootElement
                .GetProperty("type")
                .GetInt32();

            // Discord Ping
            if (type == 1)
            {
                return Ok(new
                {
                    type = 1
                });
            }

            return Ok(new
            {
                type = 4,
                data = new
                {
                    content = "Announcement Bot received the command.",
                    flags = 64
                }
            });
        }
    }
}
