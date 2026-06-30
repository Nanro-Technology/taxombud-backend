using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Common.Behaviours;

/// <summary>
/// Runs all registered FluentValidation validators before the handler executes.
/// Short-circuits and returns a validation failure Result when any rule is violated.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures.Select(f => f.ErrorMessage).ToList();

        // If TResponse is a Result<T>, return ValidationFailure; otherwise throw.
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = responseType.GetMethod("ValidationFailure", global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Public);
            if (method != null)
                return (TResponse)method.Invoke(null, [errors.AsReadOnly()])!;
        }

        throw new Exceptions.ValidationException(failures);
    }
}
