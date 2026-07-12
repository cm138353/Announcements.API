using Volo.Abp.Application.Services;
using Announcements.API.Localization;

namespace Announcements.API.Services;

/* Inherit your application services from this class. */
public abstract class APIAppService : ApplicationService
{
    protected APIAppService()
    {
        LocalizationResource = typeof(APIResource);
    }
}