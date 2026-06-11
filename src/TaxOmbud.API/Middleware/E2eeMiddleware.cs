using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;

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

        // Exclude Swagger, public key endpoint, and static files
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger") || path.Contains("/encryption/public-key"))
        {
            await _next(context);
            return;
        }

        // We skip multipart/form-data for E2EE (file uploads) as requested
        var isMultipart = context.Request.HasFormContentType;
        if (isMultipart)
        {
            await _next(context);
            return;
        }

        byte[]? aesSessionKey = null;

        // 2. Decrypt Incoming Request Body
        if (context.Request.ContentLength > 0 && context.Request.Method != HttpMethods.Get)
        {
            var keyHeader = context.Request.Headers["X-E2EE-Key"].ToString();
            var ivHeader = context.Request.Headers["X-E2EE-IV"].ToString();
            var tagHeader = context.Request.Headers["X-E2EE-Tag"].ToString();

            if (string.IsNullOrEmpty(keyHeader) || string.IsNullOrEmpty(ivHeader) || string.IsNullOrEmpty(tagHeader))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing E2EE headers.");
                return;
            }

            try
            {
                aesSessionKey = encryptionService.DecryptRsa(Convert.FromBase64String(keyHeader));
                var iv = Convert.FromBase64String(ivHeader);
                var tag = Convert.FromBase64String(tagHeader);

                using var reader = new StreamReader(context.Request.Body);
                var encryptedBodyBase64 = await reader.ReadToEndAsync();
                var encryptedBodyBytes = Convert.FromBase64String(encryptedBodyBase64);

                var decryptedBytes = encryptionService.DecryptAesGcm(encryptedBodyBytes, aesSessionKey, iv, tag);
                var decryptedJson = Encoding.UTF8.GetString(decryptedBytes);

                var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));
                context.Request.Body = requestStream;
                context.Request.ContentType = "application/json"; // Restore content type
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Failed to decrypt request payload.");
                return;
            }
        }
        else
        {
            // For GET requests, client must still provide X-E2EE-Key to receive encrypted response
            var keyHeader = context.Request.Headers["X-E2EE-Key"].ToString();
            if (!string.IsNullOrEmpty(keyHeader))
            {
                try
                {
                    aesSessionKey = encryptionService.DecryptRsa(Convert.FromBase64String(keyHeader));
                }
                catch
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid X-E2EE-Key.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing X-E2EE-Key header for response encryption.");
                return;
            }
        }

        // 3. Intercept Outgoing Response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);

        // 4. Encrypt Response Body if it is JSON
        if (responseBody.Length > 0 && context.Response.ContentType?.Contains("application/json") == true && aesSessionKey != null)
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
            // Even though original was application/json, the body is now a base64 string
            context.Response.ContentType = "text/plain"; 
            context.Response.ContentLength = encryptedResponseOutput.Length;

            await originalBodyStream.WriteAsync(encryptedResponseOutput, 0, encryptedResponseOutput.Length);
        }
        else
        {
            // Just copy over non-JSON responses (like file downloads or empty responses)
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
