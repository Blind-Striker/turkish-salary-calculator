#pragma warning disable CA1000

namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

public sealed class Result
{
    private readonly ErrorResult? _error;

    private Result()
    {
        IsSuccess = true;
    }

    private Result(ErrorResult error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public ErrorResult Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access error of successful result");

    public bool TryGetError(out ErrorResult? error)
    {
        if (IsFailure)
        {
            error = _error!;
            return true;
        }

        error = null;
        return false;
    }

    public static Result Success() => new();

    public static implicit operator Result(ErrorResult error) => FromErrorResult(error);
    public static implicit operator Result(string error) => FromString(error);
    public static implicit operator Result(Exception exception) => FromException(exception);

    public static Result FromErrorResult(ErrorResult error) => new(error);

    public static Result FromString(string message) => new(message);

    public static Result FromException(Exception exception) => new(exception);
}

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly ErrorResult? _error;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(ErrorResult error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access value of failed result. Error: {_error?.Message}");

    public ErrorResult Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access error of successful result");

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

    public bool TryGetError(out ErrorResult? error)
    {
        if (IsFailure)
        {
            error = _error!;
            return true;
        }

        error = null;
        return false;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(ErrorResult error) => new(error);
    public static Result<T> Failure(string message) => new(message);
    public static Result<T> Failure(Exception exception) => new(exception);
    public static Result<T> Failure(string message, Exception exception) => new(new ErrorResult(message, exception));
}
