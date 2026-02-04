using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculatorApi.Contracts;

namespace Turkish.HRSolutions.SalaryCalculatorApi.Extensions;

/// <summary>
/// Extension methods for mapping Result&lt;T&gt; to HTTP responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a Result&lt;YearlySalarySnapshot&gt; to an IResult.
    /// </summary>
    /// <remarks>
    /// HTTP status code mapping:
    /// <list type="bullet">
    ///   <item>Success → 200 OK with CalculationResponse</item>
    ///   <item>1000-3999 (Input/Config validation) → 400 Bad Request</item>
    ///   <item>4000-4999 (Calculation runtime) → 422 Unprocessable Entity</item>
    ///   <item>5000+ (Provider/Infrastructure) → 500 Internal Server Error</item>
    /// </list>
    /// </remarks>
    public static IResult ToHttpResult(this Result<YearlySalarySnapshot> result)
    {
        if (result.IsSuccess)
        {
            var warnings = result.HasWarnings
                ? result.Warnings.Select(ToApiWarning).ToList()
                : null;

            return Results.Ok(new CalculationResponse(result.Value, warnings));
        }

        return ToErrorResult(result.Errors);
    }

    /// <summary>
    /// Maps a Result to an IResult (for operations without return value).
    /// </summary>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return ToErrorResult(result.Errors);
    }

    /// <summary>
    /// Maps a Result&lt;T&gt; to an IResult for capabilities endpoint.
    /// </summary>
    public static IResult ToCapabilitiesHttpResult(
        this Result<Turkish.HRSolutions.SalaryCalculator.Application.Validation.CapabilityInfo> result)
    {
        if (result.IsSuccess)
        {
            var capInfo = result.Value;
            var disabled = GetCapabilityNames(capInfo.DisabledCapabilities);
            var available = GetCapabilityNames(capInfo.AvailableCapabilities);

            return Results.Ok(new CapabilitiesResponse(disabled, available));
        }

        return ToErrorResult(result.Errors);
    }

    private static IResult ToErrorResult(IReadOnlyList<Error> errors)
    {
        var firstError = errors.FirstOrDefault(e => e.IsError) ?? errors[0];
        var statusCode = GetStatusCode(firstError.Code);

        var errorResponse = new ErrorResponse(
            Type: GetErrorTypeUri(statusCode),
            Title: GetErrorTitle(statusCode),
            Status: statusCode,
            Detail: errors.Count == 1 ? firstError.Message : null,
            Errors: [.. errors.Select(ToErrorDetail)]);

        return Results.Json(errorResponse, statusCode: statusCode);
    }

    private static int GetStatusCode(ErrorCode code)
    {
        var codeValue = (int)code;

        return codeValue switch
        {
            // 1000-3999: Input and configuration validation errors → 400 Bad Request
            >= 1000 and < 4000 => StatusCodes.Status400BadRequest,
            // 4000-4999: Calculation runtime errors → 422 Unprocessable Entity
            >= 4000 and < 5000 => StatusCodes.Status422UnprocessableEntity,
            // 5000+: Provider and infrastructure errors → 500 Internal Server Error
            >= 5000 => StatusCodes.Status500InternalServerError,
            // Default fallback
            _ => StatusCodes.Status400BadRequest,
        };
    }

    private static string GetErrorTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    };

    private static string GetErrorTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        _ => "Error",
    };

    private static ApiWarning ToApiWarning(Error error) =>
        new(error.Code.ToString(), error.Message, error.Field);

    private static ErrorDetail ToErrorDetail(Error error) =>
        new(error.Code.ToString(), error.Message, error.Field);

    private static List<string> GetCapabilityNames(
        Turkish.HRSolutions.SalaryCalculator.Application.Validation.Capability capabilities)
    {
        var names = new List<string>();

        foreach (var cap in Enum.GetValues<Turkish.HRSolutions.SalaryCalculator.Application.Validation.Capability>())
        {
            if (cap != Turkish.HRSolutions.SalaryCalculator.Application.Validation.Capability.None &&
                cap != Turkish.HRSolutions.SalaryCalculator.Application.Validation.Capability.All &&
                capabilities.HasFlag(cap))
            {
                names.Add(cap.ToString());
            }
        }

        return names;
    }
}
