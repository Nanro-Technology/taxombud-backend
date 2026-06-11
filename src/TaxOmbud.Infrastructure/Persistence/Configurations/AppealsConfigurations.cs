using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Appeals;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class AppealConfiguration : IEntityTypeConfiguration<Appeal>
{
    public void Configure(EntityTypeBuilder<Appeal> builder)
    {
        builder.ToTable("Appeals");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.ReviewNote)
            .HasMaxLength(2000);

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasOne(a => a.Case)
            .WithMany()
            .HasForeignKey(a => a.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.StatusHistory)
            .WithOne(sh => sh.Appeal)
            .HasForeignKey(sh => sh.AppealId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AppealGroundPointConfiguration : IEntityTypeConfiguration<AppealGroundPoint>
{
    public void Configure(EntityTypeBuilder<AppealGroundPoint> builder)
    {
        builder.ToTable("AppealGroundPoints");
        builder.HasKey(agp => agp.Id);

        builder.Property(agp => agp.GroundTitle)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(agp => agp.GroundDetail)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(agp => agp.OfficerResponse)
            .HasMaxLength(4000);

        builder.HasQueryFilter(agp => !agp.IsDeleted);

        builder.HasOne(agp => agp.Appeal)
            .WithMany()
            .HasForeignKey(agp => agp.AppealId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AppealStatusHistoryConfiguration : IEntityTypeConfiguration<AppealStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppealStatusHistory> builder)
    {
        builder.ToTable("AppealStatusHistories");
        builder.HasKey(ash => ash.Id);

        builder.Property(ash => ash.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ash => ash.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ash => ash.Reason)
            .HasMaxLength(1000);
    }
}
