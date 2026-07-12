using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Announcements.API.Data;

public class APIDbContextFactory : IDesignTimeDbContextFactory<APIDbContext>
{
    public APIDbContext CreateDbContext(string[] args)
    {
        APIGlobalFeatureConfigurator.Configure();
        APIModuleExtensionConfigurator.Configure();

        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        APIEfCoreEntityExtensionMappings.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<APIDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new APIDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}