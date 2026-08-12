using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Middlewares;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    logger.LogError(contextFeature.Error, "Something went wrong");
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(
                            new Response<object>
                            {
                                StatusCode = StatusCodes.Status500InternalServerError,
                                Message = "An unexpected error occurred. Please try again.",
                            },
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                        )
                    );
                }
            });
        });
    }

    public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}
