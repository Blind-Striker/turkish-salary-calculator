#pragma warning disable CA1000, MA0018, CA2225, CA1024, RCS1085

namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

/// <summary>
/// Represents the result of an operation that has no return value.
/// Contains either success or a collection of errors.
/// </summary>
public sealed class Result
{
    private readonly IReadOnlyList<Error>? _errors;

    private Result()
    {
        IsSuccess = true;
        _errors = [];
    }

    private Result(IReadOnlyList<Error> errors)
    {
        _errors = errors;
        IsSuccess = false;
    }

    /// <summary>
    /// Returns true if the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Returns true if the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets all errors (empty if successful).
    /// </summary>
    public IReadOnlyList<Error> Errors => _errors ?? [];

    /// <summary>
    /// Gets only blocking errors (severity = Error).
    /// </summary>
    public IEnumerable<Error> BlockingErrors => Errors.Where(e => e.IsError);

    /// <summary>
    /// Gets only warnings (severity = Warning).
    /// </summary>
    public IEnumerable<Error> Warnings => Errors.Where(e => e.IsWarning);

    /// <summary>
    /// Returns true if there are any warnings.
    /// </summary>
    public bool HasWarnings => Errors.Any(e => e.IsWarning);

    // ─── Factory Methods ───

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new();

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    public static Result Failure(Error error) => new([error]);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors) => new([.. errors]);

    /// <summary>
    /// Creates a failed result with a single error from code and message.
    /// </summary>
    public static Result Failure(ErrorCode code, string message) =>
        new([new Error(code, message)]);

    /// <summary>
    /// Creates a failed result from a single error (alternate for implicit operator).
    /// </summary>
    public static Result FromError(Error error) => Failure(error);

    /// <summary>
    /// Creates a failed result from multiple errors (alternate for implicit operator).
    /// </summary>
    public static Result FromErrors(IReadOnlyList<Error> errors) => Failure(errors);

    // ─── Implicit Conversions ───

    public static implicit operator Result(Error error) => Failure(error);

    public static implicit operator Result(Error[] errors) => Failure(errors);
}

/// <summary>
/// Represents the result of an operation that returns a value of type <typeparamref name="T"/>.
/// Contains either the value (with optional warnings) or a collection of errors.
/// </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly IReadOnlyList<Error> _errors;

    private Result(T value, IReadOnlyList<Error>? warnings = null)
    {
        _value = value;
        _errors = warnings ?? [];
        IsSuccess = true;
    }

    private Result(IReadOnlyList<Error> errors)
    {
        _errors = errors;
        IsSuccess = false;
    }

    /// <summary>
    /// Returns true if the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Returns true if the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value if successful. Throws if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value of a failed result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access value of failed result. Errors: {string.Join("; ", _errors.Select(e => e.Message))}");

    /// <summary>
    /// Gets all errors (blocking errors if failed, warnings if successful).
    /// </summary>
    public IReadOnlyList<Error> Errors => _errors;

    /// <summary>
    /// Gets only blocking errors (severity = Error).
    /// </summary>
    public IEnumerable<Error> BlockingErrors => Errors.Where(e => e.IsError);

    /// <summary>
    /// Gets only warnings (severity = Warning).
    /// </summary>
    public IEnumerable<Error> Warnings => Errors.Where(e => e.IsWarning);

    /// <summary>
    /// Returns true if there are any warnings.
    /// </summary>
    public bool HasWarnings => Errors.Any(e => e.IsWarning);

    // ─── Access Methods ───

    /// <summary>
    /// Tries to get the value. Returns true if successful.
    /// </summary>
    public bool TryGetValue(out T? value)
    {
        if (IsSuccess)
        {
            value = _value!;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets the value or returns a default value if failed.
    /// </summary>
    public T? GetValueOrDefault(T? defaultValue = default) =>
        IsSuccess ? _value : defaultValue;

    /// <summary>
    /// Gets the value or throws an exception if failed.
    /// Use when you're confident the operation succeeded.
    /// </summary>
    public T GetValueOrThrow() => Value;

    // ─── Factory Methods ───

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a successful result with a value and warnings.
    /// </summary>
    public static Result<T> Success(T value, IEnumerable<Error> warnings) =>
        new(value, [.. warnings]);

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    public static Result<T> Failure(Error error) => new([error]);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    public static Result<T> Failure(IEnumerable<Error> errors) => new([.. errors]);

    /// <summary>
    /// Creates a failed result with a single error from code and message.
    /// </summary>
    public static Result<T> Failure(ErrorCode code, string message) =>
        new([new Error(code, message)]);

    /// <summary>
    /// Creates a failed result with a single error from code, message, and exception.
    /// </summary>
    public static Result<T> Failure(ErrorCode code, string message, Exception exception) =>
        new([new Error(code, message, Exception: exception)]);

    /// <summary>
    /// Creates a failed result from a single error (alternate for implicit operator).
    /// </summary>
    public static Result<T> FromError(Error error) => Failure(error);

    /// <summary>
    /// Creates a failed result from multiple errors (alternate for implicit operator).
    /// </summary>
    public static Result<T> FromErrors(IReadOnlyList<Error> errors) => Failure(errors);

    // ─── Implicit Conversions ───

    public static implicit operator Result<T>(Error error) => Failure(error);

    public static implicit operator Result<T>(Error[] errors) => Failure(errors);

    // ─── Deconstruction ───

    /// <summary>
    /// Deconstructs the result for pattern matching.
    /// </summary>
    public void Deconstruct(out bool isSuccess, out T? value, out IReadOnlyList<Error> errors)
    {
        isSuccess = IsSuccess;
        value = _value;
        errors = _errors;
    }
}
