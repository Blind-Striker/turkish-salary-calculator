namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

/// <summary>
/// Unified error record for all validation, calculation, and configuration errors.
/// </summary>
/// <param name="Code">The error code identifying the type of error.</param>
/// <param name="Message">Human-readable description of the error.</param>
/// <param name="Severity">Whether this is a blocking error or a warning.</param>
/// <param name="Field">Optional field name that caused the error (for validation errors).</param>
/// <param name="AttemptedValue">Optional value that was attempted (for validation errors).</param>
/// <param name="Exception">Optional exception that caused the error (for infrastructure errors).</param>
public sealed record Error(
    ErrorCode Code,
    string Message,
    ErrorSeverity Severity = ErrorSeverity.Error,
    string? Field = null,
    object? AttemptedValue = null,
    Exception? Exception = null)
{
    /// <summary>
    /// Returns true if this is a warning (non-blocking).
    /// </summary>
    public bool IsWarning => Severity == ErrorSeverity.Warning;

    /// <summary>
    /// Returns true if this is an error (blocking).
    /// </summary>
    public bool IsError => Severity == ErrorSeverity.Error;

    /// <inheritdoc />
    public override string ToString() => Field is null
        ? $"[{Severity}:{Code}] {Message}"
        : $"[{Severity}:{Code}] {Field}: {Message}";

    // ═══════════════════════════════════════════════════════════════
    // Factory Methods for Common Scenarios
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a validation error with field information.
    /// </summary>
    public static Error Validation(ErrorCode code, string message, string field, object? attemptedValue = null) =>
        new(code, message, ErrorSeverity.Error, field, attemptedValue);

    /// <summary>
    /// Creates a validation warning with field information.
    /// </summary>
    public static Error ValidationWarning(ErrorCode code, string message, string? field = null) =>
        new(code, message, ErrorSeverity.Warning, field);

    /// <summary>
    /// Creates a configuration error.
    /// </summary>
    public static Error Configuration(ErrorCode code, string message) =>
        new(code, message, ErrorSeverity.Error);

    /// <summary>
    /// Creates a configuration warning.
    /// </summary>
    public static Error ConfigurationWarning(ErrorCode code, string message) =>
        new(code, message, ErrorSeverity.Warning);

    /// <summary>
    /// Creates an infrastructure error from an exception.
    /// </summary>
    public static Error Infrastructure(ErrorCode code, string message, Exception? exception = null) =>
        new(code, message, ErrorSeverity.Error, Exception: exception);
}
