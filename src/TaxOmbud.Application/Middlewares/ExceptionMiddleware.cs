using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.CustomException;
using ApplicationException = TaxOmbud.Common.CustomException.ApplicationException;

namespace TaxOmbud.Application.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ApplicationException ex)
        {
            logger.LogError(ex, "Application exception occurred");
            await HandleExceptionAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        ApplicationException exception
    )
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)exception.StatusCode;
        var message = exception.Message;
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                new Response<object> { StatusCode = (int)exception.StatusCode, Message = message },
                JsonOptions
            )
        );
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var message = exception switch
        {
            BadHttpRequestException => "Invalid request payload supplied",
            NotImplementedException => "Method not implemented in logic",
            ApplicationException => exception.Message,
            UnauthorizedAccessException =>
                "User does not have required permission to access this endpoint",
            _ => env.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please contact the administrator.",
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                new Response<object>
                {
                    StatusCode = int.Parse(context.Response.StatusCode.ToString()),
                    Message = message,
                },
                JsonOptions
            )
        );
    }
}
