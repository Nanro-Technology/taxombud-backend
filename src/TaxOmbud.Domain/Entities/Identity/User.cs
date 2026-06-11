using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Domain.Entities.Identity;

public class User : BaseAuditableEntity
{
    // Core identity
    public string Email { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string? Phone { get; private set; }
    public string? AltPhone { get; private set; }
    public string? JobTitle { get; private set; }

    // Org structure
    public Guid? DepartmentId { get; private set; }
    public Department? Department { get; private set; }

    public string? EmploymentType { get; private set; } // Contract, FullTime
    public Guid? PayGradeId { get; private set; }

    // Status
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public bool IsActive => Status == UserStatus.Active;
    public bool CanSignIn { get; private set; } = true;

    // Email verification
    public bool EmailVerified { get; private set; } = false;
    public string? EmailVerificationToken { get; private set; }
    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; private set; }

    // Password reset
    public string? PasswordResetToken { get; private set; }
    public DateTimeOffset? PasswordResetTokenExpiresAt { get; private set; }

    public int ProfileCompletionPct
    {
        get
        {
            int score = 0;
            if (!string.IsNullOrWhiteSpace(FirstName)) score += 20;
            if (!string.IsNullOrWhiteSpace(LastName)) score += 20;
            if (!string.IsNullOrWhiteSpace(Email)) score += 20;
            if (!string.IsNullOrWhiteSpace(Phone)) score += 20;
            if (!string.IsNullOrWhiteSpace(JobTitle)) score += 20;
            return score;
        }
    }

    // Navigation
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<UserPermissionOverride> UserPermissionOverrides { get; private set; } = new List<UserPermissionOverride>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public MfaToken? MfaToken { get; private set; }

    // ─── Factory ─────────────────────────────────────────────────────────────────
    public static User Create(string firstName, string lastName, Email email, string? phone = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.Value,
            Username = email.Value,
            Phone = phone,
            Status = UserStatus.Active,
            CanSignIn = true
        };
    }

    // ─── Mutators ─────────────────────────────────────────────────────────────────
    public void SetPasswordHash(string hash) => PasswordHash = hash;

    public void AddRole(Guid roleId)
    {
        UserRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        CanSignIn = false;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        CanSignIn = true;
    }

    public void UpdateProfile(string firstName, string lastName, string? phone, string? jobTitle)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        JobTitle = jobTitle;
    }

    public void SetDepartment(Guid? departmentId) => DepartmentId = departmentId;
    public void SetPayGrade(Guid? payGradeId) => PayGradeId = payGradeId;
    public void SetEmploymentType(string? type) => EmploymentType = type;

    public void SetEmailVerificationToken(string token)
    {
        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
    }

    public void MarkEmailVerified()
    {
        EmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }

    public void SetPasswordResetToken(string token)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }
}
