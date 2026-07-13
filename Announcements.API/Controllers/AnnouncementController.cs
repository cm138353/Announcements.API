using Announcements.API.Services.Announcements;
using Announcements.API.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Announcements.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnnouncementController : AbpController
{
    private readonly IAnnouncementAppService _announcementAppService;

    public AnnouncementController(IAnnouncementAppService announcementAppService)
    {
        _announcementAppService = announcementAppService;
    }

    [HttpPost("generate")]
    public Task<AnnouncementDto> GenerateAsync(
        [FromBody] CreateAnnouncementDto input)
    {
        return _announcementAppService.GenerateAsync(input);
    }

    [HttpPost("{id:guid}/publish")]
    public Task<AnnouncementDto> PublishAsync(
        Guid id,
        [FromBody] PublishAnnouncementDto input)
    {
        return _announcementAppService.PublishAsync(id, input);
    }

    [HttpGet]
    public Task<List<AnnouncementDto>> GetListAsync()
    {
        return _announcementAppService.GetListAsync();
    }

    [HttpGet("{id:guid}")]
    public Task<AnnouncementDto> GetAsync(Guid id)
    {
        return _announcementAppService.GetAsync(id);
    }
}
