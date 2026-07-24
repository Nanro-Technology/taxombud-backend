using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;

namespace TaxOmbud.Api.Middleware;

public class E2eeMiddleware
{
    private readonly RequestDelegate _next;

    public E2eeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cache, IApplicationDbContext dbContext, IEncryptionService encryptionService)
    {
        // 1. Check if E2EE is globally enabled
        var isEnabledStr = await cache.GetAsync<string>("E2EE_ENABLED");
        if (isEnabledStr == null)
        {
            var setting = await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == "E2EE_ENABLED");
            isEnabledStr = setting?.Value ?? "false";
            await cache.SetAsync("E2EE_ENABLED", isEnabledStr, TimeSpan.FromMinutes(5));
        }

        if (!bool.TryParse(isEnabledStr, out var isEnabled) || !isEnabled)
        {
            await _next(context);
            return;
        }

        // Exclude Swagger, public key endpoint, health, and static files
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger") || path.Contains("/encryption/public-key") || path.Contains("/health"))
        {
            await _next(context);
            return;
        }

        // Skip multipart/form-data (file uploads)
        if (context.Request.HasFormContentType)
        {
            await _next(context);
            return;
        }

        byte[]? aesSessionKey = null;

        var keyHeader = context.Request.Headers["X-E2EE-Key"].ToString();
        var ivHeader = context.Request.Headers["X-E2EE-IV"].ToString();
        var tagHeader = context.Request.Headers["X-E2EE-Tag"].ToString();

        // 2. Decrypt Incoming Request Body if E2EE headers are present
        if (!string.IsNullOrEmpty(keyHeader))
        {
            try
            {
                aesSessionKey = encryptionService.DecryptRsa(Convert.FromBase64String(keyHeader));

                if (context.Request.ContentLength > 0 && context.Request.Method != HttpMethods.Get && !string.IsNullOrEmpty(ivHeader) && !string.IsNullOrEmpty(tagHeader))
                {
                    var iv = Convert.FromBase64String(ivHeader);
                    var tag = Convert.FromBase64String(tagHeader);

                    using var reader = new StreamReader(context.Request.Body);
                    var encryptedBodyBase64 = await reader.ReadToEndAsync();
                    var encryptedBodyBytes = Convert.FromBase64String(encryptedBodyBase64);

                    var decryptedBytes = encryptionService.DecryptAesGcm(encryptedBodyBytes, aesSessionKey, iv, tag);
                    var decryptedJson = Encoding.UTF8.GetString(decryptedBytes);

                    var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));
                    context.Request.Body = requestStream;
                    context.Request.ContentType = "application/json";
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Failed to process E2EE payload: {ex.Message}");
                return;
            }
        }

        // 3. Intercept Outgoing Response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            responseBody.Seek(0, SeekOrigin.Begin);

            // 4. Encrypt Response Body ONLY if aesSessionKey was established and response is JSON
            if (aesSessionKey != null && responseBody.Length > 0 && context.Response.ContentType?.Contains("application/json") == true)
            {
                var plainTextResponse = responseBody.ToArray();
                
                var responseIv = new byte[12];
                RandomNumberGenerator.Fill(responseIv);
                
                byte[] responseTag;
                var encryptedResponseBytes = encryptionService.EncryptAesGcm(plainTextResponse, aesSessionKey, responseIv, out responseTag);
                
                var encryptedResponseBase64 = Convert.ToBase64String(encryptedResponseBytes);
                var encryptedResponseOutput = Encoding.UTF8.GetBytes(encryptedResponseBase64);

                context.Response.Headers["X-E2EE-IV"] = Convert.ToBase64String(responseIv);
                context.Response.Headers["X-E2EE-Tag"] = Convert.ToBase64String(responseTag);
                context.Response.ContentType = "text/plain"; 
                context.Response.ContentLength = encryptedResponseOutput.Length;

                await originalBodyStream.WriteAsync(encryptedResponseOutput, 0, encryptedResponseOutput.Length);
            }
            else
            {
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
