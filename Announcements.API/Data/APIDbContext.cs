using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Announcements.API.Entities.Announcements;

namespace Announcements.API.Data;

public class APIDbContext : AbpDbContext<APIDbContext>
{

    public const string DbSchema = "app";

    public DbSet<Announcement> Announcements { get; set; }


    public APIDbContext(DbContextOptions<APIDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigurePermissionManagement();
        builder.ConfigureBlobStoring();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();

        /* Configure your own entities here */

        builder.Entity<Announcement>(entity =>
        {
            entity.ToTable("Announcements", DbSchema);

            entity.ConfigureByConvention();

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.RoughNotes)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(x => x.DiscordContent)
                .HasMaxLength(4000);

            entity.Property(x => x.ClanMailContent)
                .HasMaxLength(256);

            entity.Property(x => x.ClanChatContent)
                .HasMaxLength(128);

            entity.Property(x => x.Tone)
                .IsRequired();

            entity.Property(x => x.EventType)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired();

            entity.HasIndex(x => x.CreationTime);
        });
    } 
}

