using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Domain.Entities.Identity;

public class User : IdentityUser<Guid>, ISoftDelete
{
    // Identity properties from the PRD & original class:
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Phone { get; set; }
    public string? AltPhone { get; set; }
    public string? JobTitle { get; set; }

    // Org structure
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? EmploymentType { get; set; } // Contract, FullTime
    public Guid? PayGradeId { get; set; }

    // Status
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsActive => Status == UserStatus.Active;
    public bool CanSignIn { get; set; } = true;

    // Email verification & reset tokens
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTimeOffset? PasswordResetTokenExpiresAt { get; set; }

    // CalDAV integration
    public string? CaldavPassword { get; set; }

    // User Classification / UserType
    public UserType UserType { get; set; } = UserType.StaffUser;

    // ─── Role (Estate Management pattern)
    public Guid? RoleId { get; set; }
    public Role? Role { get; set; }

    // Soft delete support
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Audit fields (since it no longer inherits from BaseEntity/BaseAuditableEntity)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? LastModifiedByUserId { get; set; }

    // Compatibility wrapper for original Username property
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string Username 
    { 
        get => UserName ?? string.Empty; 
        set => UserName = value; 
    }

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public MfaToken? MfaToken { get; set; }

    // ─── Factory 
    public static User Create(string firstName, string lastName, Email email, string? phone = null, UserType userType = UserType.StaffUser)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.Value,
            UserName = email.Value,
            Phone = phone,
            Status = UserStatus.Active,
            CanSignIn = true,
            UserType = userType
        };
    }

    // ─── Compatibility Mutators ──────────────────────────────────────────────────
    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void SetUserType(UserType userType) => UserType = userType;
    public void AssignRole(Guid? roleId) => RoleId = roleId;
    public void SetDepartment(Guid? departmentId) => DepartmentId = departmentId;
    public void SetPayGrade(Guid? payGradeId) => PayGradeId = payGradeId;
    public void SetEmploymentType(string? type) => EmploymentType = type;

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
