using System.Net;
using System.Text.Json;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Api.Middleware;

/// <summary>
/// Global exception handler — converts unhandled exceptions to RFC 7807 ProblemDetails JSON.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, title, errors) = exception switch
        {
            TaxOmbud.Application.Common.Exceptions.ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                "Validation Failed",
                ve.Errors.SelectMany(kv => kv.Value.Select(msg => $"{kv.Key}: {msg}")).ToArray()
            ),
            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                "Not Found",
                new[] { nfe.Message }
            ),
            ForbiddenException _ => (
                HttpStatusCode.Forbidden,
                "Access Denied",
                new[] { "You do not have permission to perform this action." }
            ),
            ConflictException ce => (
                HttpStatusCode.Conflict,
                "Conflict",
                new[] { ce.Message }
            ),
            DomainException de => (
                HttpStatusCode.BadRequest,
                "Domain Rule Violation",
                new[] { de.Message }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                new[] { "An unexpected error occurred. Please try again later." }
            )
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            traceId = context.TraceIdentifier,
            errors
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
