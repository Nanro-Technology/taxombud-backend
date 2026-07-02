using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Taxpayers;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class TaxpayerConfiguration : IEntityTypeConfiguration<Taxpayer>
{
    public void Configure(EntityTypeBuilder<Taxpayer> builder)
    {
        builder.ToTable("Taxpayers");
        builder.HasKey(t => t.Id);

        // Email and TaxId value converters are globally set in ConfigureConventions
        builder.Property(t => t.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(t => t.Email).IsUnique();

        builder.Property(t => t.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.MiddleName).HasMaxLength(100);
        builder.Property(t => t.Phone).HasMaxLength(30).IsRequired();
        builder.Property(t => t.AltPhone).HasMaxLength(30);
        builder.Property(t => t.Gender).HasMaxLength(20);
        builder.Property(t => t.Nin).HasMaxLength(50);
        builder.Property(t => t.Bvn).HasMaxLength(50);
        builder.Property(t => t.TaxId).HasMaxLength(50);
        builder.Property(t => t.Address).HasMaxLength(500);
        builder.Property(t => t.City).HasMaxLength(100);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TaxpayerProfileConfiguration : IEntityTypeConfiguration<TaxpayerProfile>
{
    public void Configure(EntityTypeBuilder<TaxpayerProfile> builder)
    {
        builder.ToTable("TaxpayerProfiles");
        builder.HasKey(tp => tp.Id);

        builder.Property(tp => tp.TaxpayerType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(tp => tp.TinNumber).HasMaxLength(50);
        builder.Property(tp => tp.Nin).HasMaxLength(50);
        builder.Property(tp => tp.Bvn).HasMaxLength(50);
        builder.Property(tp => tp.Gender).HasMaxLength(20);
        builder.Property(tp => tp.CompanyName).HasMaxLength(200);
        builder.Property(tp => tp.RcNumber).HasMaxLength(50);
        builder.Property(tp => tp.Address).HasMaxLength(500);
        builder.Property(tp => tp.City).HasMaxLength(100);
        builder.Property(tp => tp.State).HasMaxLength(100);

        builder.HasQueryFilter(tp => !tp.IsDeleted);

        builder.HasOne(tp => tp.User)
            .WithMany()
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TaxpayerAddressConfiguration : IEntityTypeConfiguration<TaxpayerAddress>
{
    public void Configure(EntityTypeBuilder<TaxpayerAddress> builder)
    {
        builder.ToTable("TaxpayerAddresses");
        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.AddressLine1).HasMaxLength(256).IsRequired();
        builder.Property(ta => ta.AddressLine2).HasMaxLength(256);
        builder.Property(ta => ta.City).HasMaxLength(100).IsRequired();
        builder.Property(ta => ta.State).HasMaxLength(100).IsRequired();
        builder.Property(ta => ta.PostalCode).HasMaxLength(20);
        builder.Property(ta => ta.Country).HasMaxLength(100).IsRequired();

        builder.HasQueryFilter(ta => !ta.IsDeleted);

        builder.HasOne(ta => ta.Taxpayer)
            .WithMany()
            .HasForeignKey(ta => ta.TaxpayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TaxpayerContactDetailConfiguration : IEntityTypeConfiguration<TaxpayerContactDetail>
{
    public void Configure(EntityTypeBuilder<TaxpayerContactDetail> builder)
    {
        builder.ToTable("TaxpayerContactDetails");
        builder.HasKey(tcd => tcd.Id);

        builder.Property(tcd => tcd.PrimaryEmail).HasMaxLength(256).IsRequired();
        builder.Property(tcd => tcd.PrimaryPhone).HasMaxLength(30).IsRequired();
        builder.Property(tcd => tcd.AlternativePhone).HasMaxLength(30);
        builder.Property(tcd => tcd.PreferredContactMethod).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(tcd => !tcd.IsDeleted);

        builder.HasOne(tcd => tcd.Taxpayer)
            .WithMany()
            .HasForeignKey(tcd => tcd.TaxpayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
