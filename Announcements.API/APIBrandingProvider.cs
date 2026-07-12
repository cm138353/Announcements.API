using Microsoft.Extensions.Localization;
using Announcements.API.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Announcements.API;

[Dependency(ReplaceServices = true)]
public class APIBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<APIResource> _localizer;

    public APIBrandingProvider(IStringLocalizer<APIResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}