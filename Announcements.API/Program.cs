using Announcements.API.Data;
using Announcements.API.Services.AI;
using Announcements.API.Services.Discord;
using Serilog;
using Serilog.Events;
using System;
using Volo.Abp.Data;

namespace Announcements.API;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson(
                    optional: true,
                    reloadOnChange: false
                )
                .UseAutofac()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    if (IsMigrateDatabase(args))
                    {
                        loggerConfiguration
                            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .WriteTo.Async(c => c.Console(standardErrorFromLevel: LogEventLevel.Error));
                    }
                    else
                    {
                        loggerConfiguration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services)
                            .WriteTo.Async(c => c.AbpStudio(services));
                    }
                });
            if (IsMigrateDatabase(args))
            {
                builder.Services.AddDataMigrationEnvironment();
            }

            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
            builder.Services.Configure<DiscordOptions>(builder.Configuration.GetSection(DiscordOptions.SectionName));
            builder.Services.AddHttpClient<IOpenAiService, OpenAiService>();
            builder.Services.AddHttpClient<IDiscordService, DiscordService>();
            builder.Services.AddHttpClient<DiscordCommandRegistrationService>();
            await builder.AddApplicationAsync<APIModule>();
            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                var auth = context.Request.Headers.Authorization.ToString();
                Console.WriteLine(auth);

                await next();
            });
            await app.InitializeApplicationAsync();
            
            using (var scope = app.Services.CreateScope())
            {
                var registrationService =
                    scope.ServiceProvider
                        .GetRequiredService<DiscordCommandRegistrationService>();

                await registrationService.RegisterGuildCommandsAsync();
            }
            
            if (IsMigrateDatabase(args))
            {
                await app.Services.GetRequiredService<APIDbMigrationService>().MigrateAsync();
                var previous = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Migration completed.");
                Console.ForegroundColor = previous;
                return 0;
            }

            Log.Information("Starting Announcements.API.");
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Announcements.API terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static bool IsMigrateDatabase(string[] args)
    {
        return args.Any(x => x.Contains("--migrate-database", StringComparison.OrdinalIgnoreCase));
    }
}
