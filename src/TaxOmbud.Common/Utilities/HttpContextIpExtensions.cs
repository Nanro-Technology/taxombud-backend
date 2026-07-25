using Microsoft.AspNetCore.Http;

namespace TaxOmbud.Common.Utilities;

public static class HttpContextIpExtensions
{
    /// <summary>
    /// Extracts the real client IP address from request headers (CF-Connecting-IP, X-Forwarded-For, X-Real-IP)
    /// or falls back to the HttpContext Connection RemoteIpAddress.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext? context)
    {
        if (context == null) return null;

        // 1. Cloudflare header
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
        {
            return cfIp.ToString().Trim();
        }

        // 2. X-Forwarded-For header (comma-separated list of IP addresses, client IP is the first entry)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0 && !string.IsNullOrWhiteSpace(ips[0]))
            {
                return ips[0];
            }
        }

        // 3. X-Real-IP header
        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp) && !string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.ToString().Trim();
        }

        // 4. Fallback to Connection RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
