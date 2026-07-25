namespace TaxOmbud.Application.System.DTOs;

public record GetSystemMonitoringQuery();

public record SystemHealthMetricsDto(
    string Status,
    long UptimeSeconds,
    double MemoryUsageMb,
    int ActiveThreads,
    DateTime ServerTimeUtc
);

public record DatabaseMetricsDto(
    string Status,
    long QueryLatencyMs,
    int ActiveConnections,
    string DatabaseName,
    string EngineVersion
);

public record CacheMetricsDto(
    string Status,
    long PingLatencyMs,
    string Provider
);

public record SmtpMetricsDto(
    string Status,
    string Host,
    int Port,
    bool SSL,
    long ProbeLatencyMs
);

public record SecurityMonitoringDto(
    bool E2eeEnabled,
    int FailedLoginsLast1Hour,
    int FailedLoginsLast24Hours,
    int ActiveImpersonationsCount,
    int AuditEventsLast24Hours
);

public record TrafficMetricsDto(
    int TotalRequests24h,
    int SuccessRequests24h,
    int ClientErrorRequests24h,
    int ServerErrorRequests24h,
    long AvgLatencyMs
);

public record SystemMonitoringDashboardDto(
    SystemHealthMetricsDto System,
    DatabaseMetricsDto Database,
    CacheMetricsDto Cache,
    SmtpMetricsDto Smtp,
    SecurityMonitoringDto Security,
    TrafficMetricsDto Traffic,
    DateTime CheckedAt
);
