using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Auth.DTOs;

public record LoginCommand(
    string Email,
    string Password,
    string? TotpCode = null
) ;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool MfaRequired,
    Guid UserId,
    string FullName,
    IReadOnlyList<string> Roles
);