using Hangfire;
using System.Linq.Expressions;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Infrastructure.HangfireServices;

/// <summary>
/// Wraps Hangfire's IBackgroundJobClient and IRecurringJobManager to provide a
/// clean application-layer interface for scheduling background work.
/// </summary>
public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJob;

    public BackgroundJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJob)
    {
        _jobClient   = jobClient;
        _recurringJob = recurringJob;
    }

    /// <inheritdoc/>
    public string Enqueue(Expression<Action> methodCall)
        => _jobClient.Enqueue(methodCall);

    /// <inheritdoc/>
    public string Enqueue<T>(Expression<Action<T>> methodCall)
        => _jobClient.Enqueue(methodCall);

    /// <inheritdoc/>
    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        => _jobClient.Schedule(methodCall, delay);

    /// <inheritdoc/>
    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        => _jobClient.Schedule(methodCall, delay);

    /// <inheritdoc/>
    public void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Action<T>> methodCall,
        string cronExpression)
        => _recurringJob.AddOrUpdate(jobId, methodCall, cronExpression);

    /// <inheritdoc/>
    public void RemoveRecurring(string jobId)
        => _recurringJob.RemoveIfExists(jobId);
}
