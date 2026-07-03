namespace TaxOmbud.Application.AuditLogs.DTOs;

public record GetAuditLogsQuery(
    string? EntityType,
    Guid? EntityId,
    Guid? UserId,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 50
) ;

public record AuditLogDto(
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
