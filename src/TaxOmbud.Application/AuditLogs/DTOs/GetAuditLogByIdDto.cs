using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.AuditLogs.DTOs;

public record GetAuditLogByIdQuery(Guid Id) ;

public record AuditLogDetailDto(
    Guid Id,
    string EntityType,
    Guid? EntityId,
    string Action,
    Guid? UserId,
    Guid? ImpersonatorUserId,
    string? OldValues,
    string? NewValues,
    string? IPAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt
);