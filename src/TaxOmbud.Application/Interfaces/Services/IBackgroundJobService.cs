using System.Linq.Expressions;

namespace TaxOmbud.Application.Interfaces.Services;

/// <summary>
/// Provides an abstraction over Hangfire for fire-and-forget, delayed, and recurring jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>Fire-and-forget: enqueue a static action immediately.</summary>
    string Enqueue(Expression<Action> methodCall);

    /// <summary>Fire-and-forget: enqueue a service-scoped action immediately.</summary>
    string Enqueue<T>(Expression<Action<T>> methodCall);

    /// <summary>Schedule a static action to run after the given delay.</summary>
    string Schedule(Expression<Action> methodCall, TimeSpan delay);

    /// <summary>Schedule a service-scoped action to run after the given delay.</summary>
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

    /// <summary>Add or update a recurring job identified by <paramref name="jobId"/>.</summary>
    void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);

    /// <summary>Remove a recurring job by its identifier (no-op if not found).</summary>
    void RemoveRecurring(string jobId);
}
