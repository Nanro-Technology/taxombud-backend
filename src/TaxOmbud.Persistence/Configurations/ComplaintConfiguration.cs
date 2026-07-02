using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("Complaints");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.ReferenceNumber).IsUnique();

        builder.Property(c => c.Subject).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(5000).IsRequired();
        builder.Property(c => c.WhyOtoHandle).HasMaxLength(2000);

        builder.Property(c => c.TaxType).HasMaxLength(50).IsRequired();
        builder.Property(c => c.TaxPeriod).HasMaxLength(50).IsRequired();
        builder.Property(c => c.ComplaintCategory).HasMaxLength(100).IsRequired();
        builder.Property(c => c.TaxOfficeRef).HasMaxLength(100);
        builder.Property(c => c.TinNumber).HasMaxLength(50);
        builder.Property(c => c.Priority).HasMaxLength(20).IsRequired();
        builder.Property(c => c.CurrentStage).HasMaxLength(50).IsRequired();
        builder.Property(c => c.ClosureReason).HasMaxLength(1000);
        builder.Property(c => c.WithdrawalReason).HasMaxLength(1000);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Taxpayer FK — restrict delete so data is preserved
        builder.HasOne(c => c.Taxpayer)
            .WithMany()
            .HasForeignKey(c => c.TaxpayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AssignedOfficer)
            .WithMany()
            .HasForeignKey(c => c.AssignedOfficerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(c => c.Notes)
            .WithOne(n => n.Complaint)
            .HasForeignKey(n => n.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.StatusHistory)
            .WithOne(sh => sh.Complaint)
            .HasForeignKey(sh => sh.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Links)
            .WithOne(l => l.SourceComplaint)
            .HasForeignKey(l => l.SourceComplaintId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComplaintLinkConfiguration : IEntityTypeConfiguration<ComplaintLink>
{
    public void Configure(EntityTypeBuilder<ComplaintLink> builder)
    {
        builder.ToTable("ComplaintLinks");
        builder.HasKey(cl => cl.Id);

        builder.HasOne(cl => cl.SourceComplaint)
            .WithMany(c => c.Links)
            .HasForeignKey(cl => cl.SourceComplaintId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cl => cl.TargetComplaint)
            .WithMany()
            .HasForeignKey(cl => cl.TargetComplaintId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
