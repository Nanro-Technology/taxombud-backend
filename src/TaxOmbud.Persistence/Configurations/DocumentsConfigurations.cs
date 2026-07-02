using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName).HasMaxLength(256).IsRequired();
        builder.Property(d => d.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();

        builder.Property(d => d.EntityType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.HasMany(d => d.Versions)
            .WithOne(v => v.Document)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");
        builder.HasKey(dv => dv.Id);

        builder.Property(dv => dv.FilePath).HasMaxLength(500).IsRequired();

        builder.HasQueryFilter(dv => !dv.IsDeleted);
    }
}
