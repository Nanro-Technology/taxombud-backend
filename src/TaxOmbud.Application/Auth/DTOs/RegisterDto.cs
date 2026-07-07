using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Auth.DTOs;

// ─── Staff Registration (admin-only, internal) ────────────────────────────────
/// <summary>
/// Used by SuperAdmin/Admin to create internal staff accounts.
/// RoleId specifies which staff role to assign. If not provided, defaults to Officer.
/// </summary>
public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber,
    Guid? RoleId = null,
    UserType UserType = UserType.StaffUser
);

// ─── Taxpayer Self-Registration (public endpoint) ─────────────────────────────
/// <summary>
/// Used by taxpayers on the public portal to create their own account.
/// Captures all fields shown on the sign-up form.
/// </summary>
public record RegisterTaxpayerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    string? Gender,
    string? Nin,
    string? Address,
    string? City,
    string? State,
    string? Country,
    bool ConsentGiven = false
);

// ─── Shared Response ──────────────────────────────────────────────────────────
public record RegisterResponse(Guid UserId, string Email, string FullName, string UserType);
