using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Requests;

/// <summary>
/// AGI (Asgari Geçim İndirimi / Minimum Living Allowance) settings.
/// </summary>
/// <remarks>
/// <para>
/// AGI was replaced by the minimum wage income tax exemption starting from 2022.
/// For years 2022 and later, AGI settings will generate a warning and be ignored.
/// </para>
/// <para>
/// The AGI amount is calculated based on the minimum wage, spouse employment status,
/// and number of children. It is deducted from the income tax calculated.
/// </para>
/// </remarks>
/// <param name="SpouseStatus">Employment status of the spouse. Default is Unmarried if not specified.</param>
/// <param name="NumberOfChildren">Number of children (affects AGI rate).</param>
/// <param name="IncludeInTax">
/// If true, AGI is shown as a separate deduction.
/// If false, AGI is applied but not shown separately (pre-applied to net).
/// </param>
public sealed record AgiSettings(SpouseStatus SpouseStatus = default, int NumberOfChildren = 0, bool IncludeInTax = true)
{
    /// <summary>
    /// Default AGI settings for an unmarried employee with no children.
    /// </summary>
    public static AgiSettings Default { get; } = new();

    /// <summary>
    /// Creates AGI settings for a single/unmarried employee.
    /// </summary>
    public static AgiSettings Unmarried(int numberOfChildren = 0, bool includeInTax = true) =>
        new(SpouseStatus.Unmarried, numberOfChildren, includeInTax);

    /// <summary>
    /// Creates AGI settings for a married employee with a working spouse.
    /// </summary>
    public static AgiSettings MarriedSpouseWorking(int numberOfChildren = 0, bool includeInTax = true) =>
        new(SpouseStatus.SpouseWorking, numberOfChildren, includeInTax);

    /// <summary>
    /// Creates AGI settings for a married employee with a non-working spouse.
    /// </summary>
    public static AgiSettings MarriedSpouseNotWorking(int numberOfChildren = 0, bool includeInTax = true) =>
        new(SpouseStatus.SpouseNotWorking, numberOfChildren, includeInTax);
}
