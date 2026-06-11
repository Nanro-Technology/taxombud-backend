using System.Collections.Generic;

namespace TaxOmbud.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public IReadOnlyList<string> Errors { get; private set; }
    public bool IsNotFound { get; private set; }
    public bool IsForbidden { get; private set; }
    public bool IsConflict { get; private set; }
    public bool IsValidationFailure { get; private set; }

    private Result(bool isSuccess, T? value, IReadOnlyList<string> errors,
        bool isNotFound = false, bool isForbidden = false, bool isConflict = false, bool isValidationFailure = false)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
        IsNotFound = isNotFound;
        IsForbidden = isForbidden;
        IsConflict = isConflict;
        IsValidationFailure = isValidationFailure;
    }

    public static Result<T> Success(T value) =>
        new(true, value, []);

    public static Result<T> Failure(string error) =>
        new(false, default, [error]);

    public static Result<T> Failure(IReadOnlyList<string> errors) =>
        new(false, default, errors);

    public static Result<T> NotFound(string error) =>
        new(false, default, [error], isNotFound: true);

    public static Result<T> Forbidden(string error = "Access denied.") =>
        new(false, default, [error], isForbidden: true);

    public static Result<T> Conflict(string error) =>
        new(false, default, [error], isConflict: true);

    public static Result<T> ValidationFailure(IReadOnlyList<string> errors) =>
        new(false, default, errors, isValidationFailure: true);
}

public static class Result
{
    public static Result<object?> Success() => Result<object?>.Success(null);
    public static Result<object?> Failure(string error) => Result<object?>.Failure(error);
    public static Result<object?> NotFound(string error) => Result<object?>.NotFound(error);
    public static Result<object?> Forbidden(string error = "Access denied.") => Result<object?>.Forbidden(error);
    public static Result<object?> Conflict(string error) => Result<object?>.Conflict(error);
}
