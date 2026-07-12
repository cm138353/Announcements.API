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

    public AnnouncementController(
        IAnnouncementAppService announcementAppService)
    {
        _announcementAppService = announcementAppService;
    }

    [HttpPost]
    public Task<AnnouncementDto> CreateAsync(
        [FromBody] CreateAnnouncementDto input)
    {
        return _announcementAppService.CreateAsync(input);
    }

    [HttpPost("{id:guid}/generate")]
    public Task<GenerateAnnouncementResultDto> GenerateAsync(Guid id)
    {
        return _announcementAppService.GenerateAsync(id);
    }

    [HttpPut("{id:guid}")]
    public Task<AnnouncementDto> UpdateAsync(Guid id,[FromBody] UpdateAnnouncementDto input)
    {
        return _announcementAppService.UpdateAsync(id, input);
    }

    [HttpPost("{id:guid}/publish")]
    public Task PublishAsync(Guid id, [FromBody] PublishAnnouncementDto input)
    {
        return _announcementAppService.PublishAsync(id, input);
    }

    [HttpGet("{id:guid}")]
    public Task<AnnouncementDto> GetAsync(Guid id)
    {
        return _announcementAppService.GetAsync(id);
    }

    [HttpGet]
    public Task<List<AnnouncementDto>> GetListAsync()
    {
        return _announcementAppService.GetListAsync();
    }

    [HttpDelete("{id:guid}")]
    public Task DeleteAsync(Guid id)
    {
        return _announcementAppService.DeleteAsync(id);
    }
}
