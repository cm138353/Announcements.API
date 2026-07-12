using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Announcements.API.Data;

public class APIDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public APIDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the APIDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<APIDbContext>()
            .Database
            .MigrateAsync();

    }
}
