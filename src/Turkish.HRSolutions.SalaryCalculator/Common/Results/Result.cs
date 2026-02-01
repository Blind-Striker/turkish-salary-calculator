#pragma warning disable CA1000, MA0018, CA2225, CA1024, RCS1085

namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

/// <summary>
/// Represents the result of an operation that has no return value.
/// Contains either success or a collection of errors.
/// </summary>
/// <remarks>
/// <para>
/// This is the base class for all result types. Use <see cref="Result"/> when an operation
/// has no return value, and <see cref="Result{T}"/> when it returns a value.
/// </para>
/// <para>
/// <see cref="Result{T}"/> inherits from <see cref="Result"/>, allowing polymorphic handling
/// of results when the value is not needed:
/// <code>
/// Result result = calculator.Calculate(request); // Upcast from Result&lt;T&gt;
/// if (result.IsFailure) HandleErrors(result.Errors);
/// </code>
/// </para>
/// </remarks>
public class Result
{
    private readonly IReadOnlyList<Error> _errors;

    /// <summary>
    /// Initializes a new successful result.
    /// </summary>
    protected Result()
    {
        IsSuccess = true;
        _errors = [];
    }

    /// <summary>
    /// Initializes a new successful result with warnings.
    /// </summary>
    /// <param name="warnings">The warnings to include.</param>
    protected Result(IReadOnlyList<Error> warnings)
    {
        IsSuccess = true;
        _errors = warnings;
    }

    /// <summary>
    /// Initializes a new failed result.
    /// </summary>
    /// <param name="errors">The errors that caused the failure.</param>
    /// <param name="isFailure">Must be true to indicate this is a failure constructor.</param>
    protected Result(IReadOnlyList<Error> errors, bool isFailure)
    {
        _errors = errors;
        IsSuccess = !isFailure;
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
    /// Gets all errors (empty if successful without warnings).
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

    // ─── Factory Methods ───

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new();

    /// <summary>
    /// Creates a successful result with warnings.
    /// </summary>
    public static Result Success(IEnumerable<Error> warnings) => new([.. warnings]);

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    public static Result Failure(Error error) => new([error], isFailure: true);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors) => new([.. errors], isFailure: true);

    /// <summary>
    /// Creates a failed result with a single error from code and message.
    /// </summary>
    public static Result Failure(ErrorCode code, string message) =>
        new([new Error(code, message)], isFailure: true);

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
/// <remarks>
/// <para>
/// Inherits from <see cref="Result"/>, allowing polymorphic handling when the value is not needed.
/// </para>
/// </remarks>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
    }

    private Result(T value, IReadOnlyList<Error> warnings)
        : base(warnings)
    {
        _value = value;
    }

    private Result(IReadOnlyList<Error> errors)
        : base(errors, isFailure: true)
    {
    }

    /// <summary>
    /// Gets the value if successful. Throws if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value of a failed result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access value of failed result. Errors: {string.Join("; ", Errors.Select(e => e.Message))}");

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
    public static new Result<T> Failure(Error error) => new([error]);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    public static new Result<T> Failure(IEnumerable<Error> errors) => new([.. errors]);

    /// <summary>
    /// Creates a failed result with a single error from code and message.
    /// </summary>
    public static new Result<T> Failure(ErrorCode code, string message) =>
        new([new Error(code, message)]);

    /// <summary>
    /// Creates a failed result with a single error from code, message, and exception.
    /// </summary>
    public static Result<T> Failure(ErrorCode code, string message, Exception exception) =>
        new([new Error(code, message, Exception: exception)]);

    /// <summary>
    /// Creates a failed result from a single error (alternate for implicit operator).
    /// </summary>
    public static new Result<T> FromError(Error error) => Failure(error);

    /// <summary>
    /// Creates a failed result from multiple errors (alternate for implicit operator).
    /// </summary>
    public static new Result<T> FromErrors(IReadOnlyList<Error> errors) => Failure(errors);

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
        errors = Errors;
    }
}
