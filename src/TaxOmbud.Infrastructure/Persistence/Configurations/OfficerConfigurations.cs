using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Officers;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class OfficerConfiguration : IEntityTypeConfiguration<Officer>
{
    public void Configure(EntityTypeBuilder<Officer> builder)
    {
        builder.ToTable("Officers");
        builder.HasKey(o => o.Id);

        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.PerformanceRecords)
            .WithOne(pr => pr.Officer)
            .HasForeignKey(pr => pr.OfficerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OfficerProfileConfiguration : IEntityTypeConfiguration<OfficerProfile>
{
    public void Configure(EntityTypeBuilder<OfficerProfile> builder)
    {
        builder.ToTable("OfficerProfiles");
        builder.HasKey(op => op.Id);

        builder.Property(op => op.EmployeeNumber)
            .HasMaxLength(50);

        builder.Property(op => op.Specialisation)
            .HasMaxLength(100);

        builder.HasQueryFilter(op => !op.IsDeleted);

        builder.HasOne(op => op.User)
            .WithMany()
            .HasForeignKey(op => op.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OfficerCaseloadConfiguration : IEntityTypeConfiguration<OfficerCaseload>
{
    public void Configure(EntityTypeBuilder<OfficerCaseload> builder)
    {
        builder.ToTable("OfficerCaseloads");
        builder.HasKey(oc => oc.Id);

        builder.HasQueryFilter(oc => !oc.IsDeleted);

        builder.HasOne(oc => oc.OfficerProfile)
            .WithMany()
            .HasForeignKey(oc => oc.OfficerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OfficerPerformanceRecordConfiguration : IEntityTypeConfiguration<OfficerPerformanceRecord>
{
    public void Configure(EntityTypeBuilder<OfficerPerformanceRecord> builder)
    {
        builder.ToTable("OfficerPerformanceRecords");
        builder.HasKey(opr => opr.Id);

        builder.Property(opr => opr.AverageResolutionTimeDays)
            .HasPrecision(10, 2);

        builder.Property(opr => opr.CsatScore)
            .HasPrecision(5, 2);

        builder.HasQueryFilter(opr => !opr.IsDeleted);
    }
}
