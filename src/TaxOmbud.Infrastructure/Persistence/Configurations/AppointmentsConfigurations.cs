using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Appointments;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(ap => ap.Id);

        builder.Property(ap => ap.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ap => ap.Description)
            .HasMaxLength(1000);

        builder.Property(ap => ap.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ap => ap.Location)
            .HasMaxLength(200);

        builder.Property(ap => ap.MeetingUrl)
            .HasMaxLength(500);

        builder.HasQueryFilter(ap => !ap.IsDeleted);

        builder.HasOne(ap => ap.Officer)
            .WithMany()
            .HasForeignKey(ap => ap.OfficerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ap => ap.Taxpayer)
            .WithMany()
            .HasForeignKey(ap => ap.TaxpayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
