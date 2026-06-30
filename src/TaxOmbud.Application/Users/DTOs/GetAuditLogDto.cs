using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Users.DTOs;

public record GetAuditLogQuery(
    Guid UserId,
    string? EntityType,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20
) ;

public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    Guid? UserId,
    Guid? ImpersonatorUserId,
    string? IPAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt
);