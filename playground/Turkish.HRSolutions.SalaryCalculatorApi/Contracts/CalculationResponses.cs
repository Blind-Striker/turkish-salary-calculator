using Turkish.HRSolutions.SalaryCalculator.Application.Responses;

namespace Turkish.HRSolutions.SalaryCalculatorApi.Contracts;

// ═══════════════════════════════════════════════════════════════════════════════
// Success Response - wraps library YearlySalarySnapshot with warnings
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Successful calculation response.
/// Uses library types directly - no DTO mapping overhead.
/// </summary>
/// <param name="Data">The calculation result (library type).</param>
/// <param name="Warnings">Validation warnings (settings that were ignored).</param>
public sealed record CalculationResponse(YearlySalarySnapshot Data, IReadOnlyList<ApiWarning>? Warnings = null);

/// <summary>
/// Warning information for API responses.
/// </summary>
/// <param name="Code">Warning code (e.g., "AgiNotApplicable").</param>
/// <param name="Message">Human-readable warning message.</param>
/// <param name="Field">Related field (if applicable).</param>
public sealed record ApiWarning(string Code, string Message, string? Field = null);

// ═══════════════════════════════════════════════════════════════════════════════
// Error Response - RFC 7807 Problem Details
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Error response following RFC 7807 Problem Details.
/// </summary>
/// <param name="Type">Error type URI.</param>
/// <param name="Title">Short error title.</param>
/// <param name="Status">HTTP status code.</param>
/// <param name="Detail">Detailed error message.</param>
/// <param name="Errors">List of individual errors.</param>
public sealed record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string? Detail = null,
    IReadOnlyList<ErrorDetail>? Errors = null);

/// <summary>
/// Individual error detail.
/// </summary>
/// <param name="Code">Error code.</param>
/// <param name="Message">Error message.</param>
/// <param name="Field">Related field (if applicable).</param>
public sealed record ErrorDetail(string Code, string Message, string? Field = null);
