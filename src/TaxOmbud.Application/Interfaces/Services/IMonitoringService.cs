using TaxOmbud.Application.System.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IMonitoringService
{
    Task<Response<SystemMonitoringDashboardDto>> GetMonitoringMetricsAsync(GetSystemMonitoringQuery request, CancellationToken cancellationToken = default);
    Task<Response<SystemMonitoringDashboardDto>> RunDiagnosticHealthCheckAsync(CancellationToken cancellationToken = default);
}
