using System.Diagnostics;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Api.Middleware;

public class RequestTelemetryMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTelemetryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMonitoringService monitoringService)
    {
        // Skip static files or swagger assets to avoid noise
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger") || path.StartsWith("/favicon") || path.EndsWith(".css") || path.EndsWith(".js"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode;
            var latencyMs = sw.ElapsedMilliseconds;
            var method = context.Request.Method;
            var clientIp = HttpContextIpExtensions.GetClientIpAddress(context);
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var user = context.User?.Identity?.Name ?? "Anonymous";

            monitoringService.RecordRequestTelemetry(
                method: method,
                path: path,
                statusCode: statusCode,
                latencyMs: latencyMs,
                clientIp: clientIp,
                userAgent: userAgent,
                user: user
            );
        }
    }
}
