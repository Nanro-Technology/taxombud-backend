using System;
using System.Security.Claims;

namespace TaxOmbud.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    string[] Roles { get; }
    ClaimsPrincipal? Principal { get; }
}
