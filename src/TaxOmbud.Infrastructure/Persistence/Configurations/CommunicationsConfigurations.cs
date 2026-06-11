using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class CommunicationConfiguration : IEntityTypeConfiguration<Communication>
{
    public void Configure(EntityTypeBuilder<Communication> builder)
    {
        builder.ToTable("Communications");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Recipient).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Subject).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Body).HasMaxLength(4000).IsRequired();
        builder.Property(c => c.Channel).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Direction)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.ErrorMessage).HasMaxLength(1000);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
{
    public void Configure(EntityTypeBuilder<CommunicationLog> builder)
    {
        builder.ToTable("CommunicationLogs");
        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.RelatedEntityType).HasMaxLength(100);
        builder.Property(cl => cl.Recipient).HasMaxLength(256).IsRequired();
        builder.Property(cl => cl.RecipientName).HasMaxLength(256);
        builder.Property(cl => cl.Subject).HasMaxLength(500).IsRequired();
        builder.Property(cl => cl.Body).HasMaxLength(4000).IsRequired();
        builder.Property(cl => cl.Channel).HasMaxLength(50).IsRequired();
        builder.Property(cl => cl.Direction)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cl => cl.ErrorMessage).HasMaxLength(1000);

        builder.HasQueryFilter(cl => !cl.IsDeleted);
    }
}

public class CommunicationTemplateConfiguration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> builder)
    {
        builder.ToTable("CommunicationTemplates");
        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(ct => ct.Name).IsUnique();

        builder.Property(ct => ct.SubjectTemplate).HasMaxLength(500).IsRequired();
        builder.Property(ct => ct.BodyTemplate).HasMaxLength(4000).IsRequired();
        builder.Property(ct => ct.Channel).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(ct => !ct.IsDeleted);
    }
}
