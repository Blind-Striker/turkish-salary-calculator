#pragma warning disable CA1000, MA0018

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

    public static Result Success()
    {
        return new Result();
    }

    public static implicit operator Result(ErrorResult error)
    {
        return FromErrorResult(error);
    }

    public static implicit operator Result(string error)
    {
        return FromString(error);
    }

    public static implicit operator Result(Exception exception)
    {
        return FromException(exception);
    }

    public static Result FromErrorResult(ErrorResult error)
    {
        return new Result(error);
    }

    public static Result FromString(string message)
    {
        return new Result(message);
    }

    public static Result FromException(Exception exception)
    {
        return new Result(exception);
    }
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

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure(ErrorResult error)
    {
        return new Result<T>(error);
    }

    public static Result<T> Failure(string message)
    {
        return new Result<T>(message);
    }

    public static Result<T> Failure(Exception exception)
    {
        return new Result<T>(exception);
    }

    public static Result<T> Failure(string message, Exception exception)
    {
        return new Result<T>(new ErrorResult(message, exception));
    }
}
