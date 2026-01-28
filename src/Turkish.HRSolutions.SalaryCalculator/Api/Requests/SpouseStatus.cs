namespace Turkish.HRSolutions.SalaryCalculator.Api.Requests;

/// <summary>
/// Spouse employment status for AGI (minimum living allowance) calculation.
/// </summary>
/// <remarks>
/// AGI was replaced by the minimum wage income tax exemption starting from 2022.
/// This enum is only relevant for calculations before 2022.
/// </remarks>
public enum SpouseStatus
{
    /// <summary>
    /// Employee is unmarried - lowest AGI rate.
    /// </summary>
    Unmarried = 0,

    /// <summary>
    /// Employee is married, spouse is employed - medium AGI rate.
    /// </summary>
    SpouseWorking = 1,

    /// <summary>
    /// Employee is married, spouse is not employed - highest AGI rate.
    /// </summary>
    SpouseNotWorking = 2,
}
