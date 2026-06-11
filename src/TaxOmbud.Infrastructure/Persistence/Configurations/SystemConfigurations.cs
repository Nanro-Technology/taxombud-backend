using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(al => al.Id);

        builder.Property(al => al.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(al => al.Action).HasMaxLength(100).IsRequired();
        builder.Property(al => al.IPAddress).HasMaxLength(45);
        builder.Property(al => al.UserAgent).HasMaxLength(500);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Key).HasMaxLength(200).IsRequired();
        builder.HasIndex(ss => ss.Key).IsUnique();

        builder.Property(ss => ss.Value).HasMaxLength(4000).IsRequired();
        builder.Property(ss => ss.Description).HasMaxLength(500);

        builder.HasQueryFilter(ss => !ss.IsDeleted);
    }
}

public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("FeatureFlags");
        builder.HasKey(ff => ff.Id);

        builder.Property(ff => ff.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(ff => ff.Name).IsUnique();

        builder.Property(ff => ff.Description).HasMaxLength(500);

        builder.HasQueryFilter(ff => !ff.IsDeleted);
    }
}

public class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("WebhookSubscriptions");
        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Url).HasMaxLength(500).IsRequired();
        builder.Property(ws => ws.Secret).HasMaxLength(256).IsRequired();
        builder.Property(ws => ws.EventTypes).HasMaxLength(1000).IsRequired();

        builder.HasQueryFilter(ws => !ws.IsDeleted);
    }
}

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("WebhookDeliveries");
        builder.HasKey(wd => wd.Id);

        builder.Property(wd => wd.EventType).HasMaxLength(100).IsRequired();
        builder.Property(wd => wd.Signature).HasMaxLength(256).IsRequired();

        builder.HasOne(wd => wd.Subscription)
            .WithMany()
            .HasForeignKey(wd => wd.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ScheduledReportConfiguration : IEntityTypeConfiguration<ScheduledReport>
{
    public void Configure(EntityTypeBuilder<ScheduledReport> builder)
    {
        builder.ToTable("ScheduledReports");
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.ReportName).HasMaxLength(200).IsRequired();
        builder.Property(sr => sr.CronExpression).HasMaxLength(100).IsRequired();
        builder.Property(sr => sr.Recipients).HasMaxLength(2000).IsRequired();
        builder.Property(sr => sr.Format).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(sr => !sr.IsDeleted);
    }
}
