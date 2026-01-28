namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

/// <summary>
/// Indicates whether an error is blocking or can be safely ignored.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Non-blocking issue. Calculation proceeds, but the setting is ignored.
    /// Example: AGI settings on year 2026 (AGI replaced by min wage exemption).
    /// </summary>
    Warning = 0,

    /// <summary>
    /// Blocking issue. Calculation cannot proceed.
    /// Example: R and D days specified for Standard employee type.
    /// </summary>
    Error = 1,
}
