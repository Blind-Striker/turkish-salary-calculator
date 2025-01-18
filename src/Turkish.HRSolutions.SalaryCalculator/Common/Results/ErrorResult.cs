namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

public sealed record ErrorResult
{
    public string Message { get; }
    public Exception? Exception { get; }

    public ErrorResult(string message, Exception? exception = null)
    {
        Message = message;
        Exception = exception;
    }

    public static implicit operator ErrorResult(string message)
    {
        return FromString(message);
    }

    public static implicit operator ErrorResult(Exception exception)
    {
        return FromException(exception);
    }

    public static ErrorResult FromString(string message)
    {
        return new ErrorResult(message);
    }

    public static ErrorResult FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new ErrorResult(exception.Message, exception);
    }
}
