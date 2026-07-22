using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Cases;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("Cases");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.Summary)
            .HasMaxLength(4000);

        builder.Property(c => c.Priority)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CurrentStage)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Outcome)
            .HasMaxLength(2000);

        builder.Property(c => c.FindingsSummary)
            .HasMaxLength(4000);

        // Value Converter for CaseNumber is applied globally via ConfigureConventions
        builder.Property(c => c.CaseNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.CaseNumber).IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);

        // Relationships
        builder.HasOne(c => c.Complaint)
            .WithMany()
            .HasForeignKey(c => c.ComplaintId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AssignedOfficer)
            .WithMany()
            .HasForeignKey(c => c.AssignedOfficerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Account)
            .WithMany()
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Findings)
            .WithOne(f => f.Case)
            .HasForeignKey(f => f.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Recommendations)
            .WithOne(r => r.Case)
            .HasForeignKey(r => r.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Milestones)
            .WithOne(m => m.Case)
            .HasForeignKey(m => m.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CommunicationLogs)
            .WithOne(cl => cl.Case)
            .HasForeignKey(cl => cl.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.StatusHistory)
            .WithOne(sh => sh.Case)
            .HasForeignKey(sh => sh.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ActiveWorkflowInstance)
            .WithMany()
            .HasForeignKey(c => c.ActiveWorkflowInstanceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CaseNoteConfiguration : IEntityTypeConfiguration<CaseNote>
{
    public void Configure(EntityTypeBuilder<CaseNote> builder)
    {
        builder.ToTable("CaseNotes");
        builder.HasKey(cn => cn.Id);

        builder.Property(cn => cn.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasQueryFilter(cn => !cn.IsDeleted);

        builder.HasOne(cn => cn.Case)
            .WithMany()
            .HasForeignKey(cn => cn.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cn => cn.Author)
            .WithMany()
            .HasForeignKey(cn => cn.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CaseFindingConfiguration : IEntityTypeConfiguration<CaseFinding>
{
    public void Configure(EntityTypeBuilder<CaseFinding> builder)
    {
        builder.ToTable("CaseFindings");
        builder.HasKey(cf => cf.Id);

        builder.Property(cf => cf.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasQueryFilter(cf => !cf.IsDeleted);
    }
}

public class CaseRecommendationConfiguration : IEntityTypeConfiguration<CaseRecommendation>
{
    public void Configure(EntityTypeBuilder<CaseRecommendation> builder)
    {
        builder.ToTable("CaseRecommendations");
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.RecommendationText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(cr => cr.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cr => cr.Notes)
            .HasMaxLength(2000);

        builder.HasQueryFilter(cr => !cr.IsDeleted);
    }
}

public class CaseMilestoneConfiguration : IEntityTypeConfiguration<CaseMilestone>
{
    public void Configure(EntityTypeBuilder<CaseMilestone> builder)
    {
        builder.ToTable("CaseMilestones");
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cm => cm.Description)
            .HasMaxLength(1000);

        builder.HasQueryFilter(cm => !cm.IsDeleted);
    }
}

public class CaseCommunicationLogConfiguration : IEntityTypeConfiguration<CaseCommunicationLog>
{
    public void Configure(EntityTypeBuilder<CaseCommunicationLog> builder)
    {
        builder.ToTable("CaseCommunicationLogs");
        builder.HasKey(ccl => ccl.Id);

        builder.Property(ccl => ccl.Sender)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(ccl => ccl.Recipient)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(ccl => ccl.Direction)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ccl => ccl.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ccl => ccl.Body)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(ccl => ccl.Channel)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasQueryFilter(ccl => !ccl.IsDeleted);
    }
}

public class CaseStatusHistoryConfiguration : IEntityTypeConfiguration<CaseStatusHistory>
{
    public void Configure(EntityTypeBuilder<CaseStatusHistory> builder)
    {
        builder.ToTable("CaseStatusHistories");
        builder.HasKey(csh => csh.Id);

        builder.Property(csh => csh.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(csh => csh.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(csh => csh.Reason)
            .HasMaxLength(1000);
    }
}
