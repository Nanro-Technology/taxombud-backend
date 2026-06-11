using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TaxOmbud.Application.Common.Behaviours;

/// <summary>
/// Logs every incoming MediatR request and its elapsed time.
/// Warns if the handler exceeds 500 ms.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("→ Handling {RequestName}", name);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            _logger.LogWarning("⚠ Slow request: {RequestName} took {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation("← {RequestName} completed in {ElapsedMs} ms", name, sw.ElapsedMilliseconds);

        return response;
    }
}
