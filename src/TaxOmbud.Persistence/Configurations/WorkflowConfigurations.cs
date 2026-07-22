using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Workflows;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Description).HasMaxLength(2000).IsRequired();
        builder.Property(w => w.CaseCategory).HasMaxLength(100).IsRequired();

        builder.HasQueryFilter(w => !w.IsDeleted);

        builder.HasMany(w => w.Levels)
            .WithOne(l => l.Workflow)
            .HasForeignKey(l => l.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Versions)
            .WithOne(v => v.Workflow)
            .HasForeignKey(v => v.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowLevelConfiguration : IEntityTypeConfiguration<WorkflowLevel>
{
    public void Configure(EntityTypeBuilder<WorkflowLevel> builder)
    {
        builder.ToTable("WorkflowLevels");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(1000);

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}

public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("WorkflowVersions");
        builder.HasKey(v => v.Id);

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");
        builder.HasKey(i => i.Id);

        builder.HasQueryFilter(i => !i.IsDeleted);

        // Explicitly configure 1-to-many relationship from Case to WorkflowInstance
        builder.HasOne(i => i.Case)
            .WithMany()
            .HasForeignKey(i => i.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Workflow)
            .WithMany()
            .HasForeignKey(i => i.WorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.WorkflowVersion)
            .WithMany()
            .HasForeignKey(i => i.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.InstanceLevels)
            .WithOne(il => il.WorkflowInstance)
            .HasForeignKey(il => il.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.ApprovalTasks)
            .WithOne(t => t.WorkflowInstance)
            .HasForeignKey(t => t.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowInstanceLevelConfiguration : IEntityTypeConfiguration<WorkflowInstanceLevel>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceLevel> builder)
    {
        builder.ToTable("WorkflowInstanceLevels");
        builder.HasKey(il => il.Id);

        builder.HasQueryFilter(il => !il.IsDeleted);

        builder.HasOne(il => il.WorkflowLevel)
            .WithMany()
            .HasForeignKey(il => il.WorkflowLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CaseApprovalTaskConfiguration : IEntityTypeConfiguration<CaseApprovalTask>
{
    public void Configure(EntityTypeBuilder<CaseApprovalTask> builder)
    {
        builder.ToTable("CaseApprovalTasks");
        builder.HasKey(t => t.Id);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.Case)
            .WithMany()
            .HasForeignKey(t => t.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.WorkflowInstanceLevel)
            .WithMany()
            .HasForeignKey(t => t.WorkflowInstanceLevelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CaseWorkflowAuditLogConfiguration : IEntityTypeConfiguration<CaseWorkflowAuditLog>
{
    public void Configure(EntityTypeBuilder<CaseWorkflowAuditLog> builder)
    {
        builder.ToTable("CaseWorkflowAuditLogs");
        builder.HasKey(a => a.Id);

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasOne(a => a.Case)
            .WithMany(c => c.AuditLogs)
            .HasForeignKey(a => a.CaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
