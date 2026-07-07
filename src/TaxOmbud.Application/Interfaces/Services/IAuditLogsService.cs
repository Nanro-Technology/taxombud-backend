using TaxOmbud.Application.AuditLogs.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IAuditLogsService
{
    Task<Response<AuditLogDetailDto>> GetAuditLogByIdAsync(GetAuditLogByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<AuditLogDto>>> GetAuditLogsAsync(GetAuditLogsQuery request, CancellationToken cancellationToken = default);
}
