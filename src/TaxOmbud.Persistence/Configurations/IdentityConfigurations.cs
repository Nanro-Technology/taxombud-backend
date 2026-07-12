using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;


namespace TaxOmbud.Persistence.Configurations;

// ─── Role ─────────────────────────────────────────────────────────────────────
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.IsSystemRole).IsRequired();
        builder.Property(r => r.IsActive).IsRequired();

        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// ─── Permission ────────────────────────────────────────────────────────────────
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Module)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Action)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Ensure no duplicate Module + Action combinations
        builder.HasIndex(p => new { p.Module, p.Action }).IsUnique();
    }
}

// ─── RolePermission ───────────────────────────────────────────────────────────
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rp => rp.Id);

        // Prevent duplicate role-permission pairs
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─── User ─────────────────────────────────────────────────────────────────────
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Ignore(u => u.Username);
        builder.Property(u => u.UserName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(30);
        builder.Property(u => u.AltPhone).HasMaxLength(30);
        builder.Property(u => u.JobTitle).HasMaxLength(200);
        builder.Property(u => u.EmploymentType).HasMaxLength(50);
        builder.Property(u => u.CaldavPassword).HasMaxLength(256);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(u => u.UserType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // Soft delete query filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Single role FK (Estate Management pattern)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Department)
            .WithMany()
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// ─── RefreshToken ─────────────────────────────────────────────────────────────
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).HasMaxLength(512).IsRequired();
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(512);
    }
}

// ─── Department ───────────────────────────────────────────────────────────────
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.RoutingMode).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500);

        builder.HasOne(d => d.HeadUser)
            .WithMany()
            .HasForeignKey(d => d.HeadUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
