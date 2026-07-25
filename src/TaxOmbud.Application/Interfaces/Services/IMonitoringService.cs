using TaxOmbud.Application.System.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IMonitoringService
{
    Task<Response<SystemMonitoringDashboardDto>> GetMonitoringMetricsAsync(GetSystemMonitoringQuery request, CancellationToken cancellationToken = default);
    Task<Response<SystemMonitoringDashboardDto>> RunDiagnosticHealthCheckAsync(CancellationToken cancellationToken = default);
    Task<Response<GranularMonitoringDashboardDto>> GetGranularMonitoringMetricsAsync(GetGranularMonitoringQuery request, CancellationToken cancellationToken = default);
    void RecordRequestTelemetry(string method, string path, int statusCode, long latencyMs, string clientIp, string userAgent, string user);
}
