using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Api.Middleware;

public class E2eeMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] BypassPaths = { "/swagger", "/health", "/api/v1/security", "/hangfire" };

    public E2eeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICryptoService cryptoService, ICacheService cache, IApplicationDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (BypassPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        bool isE2eeEnabled = await GetE2eeStatusAsync(cache, dbContext, context.RequestAborted);
        if (!isE2eeEnabled)
        {
            await _next(context);
            return;
        }

        // --- INBOUND (Decryption) ---
        if (!context.Request.Headers.TryGetValue("X-Encryption-Key", out var encKeyHeader) ||
            !context.Request.Headers.TryGetValue("X-Encryption-IV", out var ivHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"End-to-End Encryption is strictly enforced. Missing X-Encryption-Key or X-Encryption-IV headers.\"}");
            return;
        }

        byte[] aesKey;
        byte[] aesIv;
        try
        {
            var rsaEncryptedAesKey = Convert.FromBase64String(encKeyHeader.ToString());
            aesKey = cryptoService.DecryptRsa(rsaEncryptedAesKey);
            aesIv = Convert.FromBase64String(ivHeader.ToString());
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Invalid encryption key or IV format.\"}");
            return;
        }

        // If request has a body, decrypt it
        if (context.Request.ContentLength > 0 || context.Request.Headers.TransferEncoding.Contains("chunked"))
        {
            try
            {
                using var ms = new MemoryStream();
                await context.Request.Body.CopyToAsync(ms);
                var encryptedBody = ms.ToArray();

                if (encryptedBody.Length > 0)
                {
                    var decryptedBody = cryptoService.DecryptAes(encryptedBody, aesKey, aesIv);
                    context.Request.Body = new MemoryStream(decryptedBody);
                    // Ensure downstream middleware reads this properly
                    context.Request.ContentLength = decryptedBody.Length;
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Failed to decrypt payload.\"}");
                return;
            }
        }

        // --- OUTBOUND (Encryption) ---
        var originalResponseBodyStream = context.Response.Body;
        using var responseMemoryStream = new MemoryStream();
        context.Response.Body = responseMemoryStream;

        try
        {
            await _next(context);

            responseMemoryStream.Seek(0, SeekOrigin.Begin);
            if (responseMemoryStream.Length > 0)
            {
                var responseBytes = responseMemoryStream.ToArray();
                var encryptedResponse = cryptoService.EncryptAes(responseBytes, aesKey, aesIv);
                
                context.Response.ContentLength = encryptedResponse.Length;
                context.Response.ContentType = "application/octet-stream"; // Client expects encrypted blob
                
                await originalResponseBodyStream.WriteAsync(encryptedResponse, 0, encryptedResponse.Length);
            }
        }
        finally
        {
            // Restore original stream just in case
            context.Response.Body = originalResponseBodyStream;
        }
    }

    private async Task<bool> GetE2eeStatusAsync(ICacheService cache, IApplicationDbContext dbContext, CancellationToken ct)
    {
        var cached = await cache.GetAsync<string>("E2EE_Enabled", ct);
        if (cached != null) return cached == "true";

        var setting = await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == "Security:E2EE_Enabled", ct);
        var isEnabled = setting?.Value == "true";
        
        await cache.SetAsync("E2EE_Enabled", isEnabled ? "true" : "false", TimeSpan.FromMinutes(5), ct);
        return isEnabled;
    }
}
