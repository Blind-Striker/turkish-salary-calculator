namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Represents calculator capabilities that can be enabled or disabled
/// based on year, employee type, and calculation mode.
/// </summary>
/// <remarks>
/// Mirrors Angular UI disabled states for feature parity.
/// Use ISalaryCalculatorMetadata.GetCapabilities to query available capabilities.
/// </remarks>
[Flags]
public enum Capability
{
    /// <summary>No capabilities.</summary>
    None = 0,

    /// <summary>
    /// AGI (Minimum Living Allowance) spouse/children dropdown selection.
    /// Disabled when AGI is not applicable for the employee type.
    /// </summary>
    AgiSelection = 1 << 0,

    /// <summary>
    /// Education type selection for R&amp;D exemption calculation.
    /// Disabled when employer education income tax exemption is not applicable.
    /// </summary>
    EducationType = 1 << 1,

    /// <summary>
    /// Minimum wage tax exemption toggle.
    /// Disabled when year doesn't support min wage employee tax exemption.
    /// </summary>
    MinWageExemption = 1 << 2,

    /// <summary>
    /// AGI calculation toggle.
    /// Disabled when year has min wage employee tax exemption (post-2022).
    /// </summary>
    AgiCalculation = 1 << 3,

    /// <summary>
    /// 5746 SGK discount toggle.
    /// Disabled when pensioner or employee type doesn't support 5746 discount.
    /// </summary>
    Discount5746 = 1 << 4,

    /// <summary>
    /// R&amp;D days input field.
    /// Disabled when R&amp;D tax exemption is not applicable for employee type.
    /// </summary>
    RnDDaysInput = 1 << 5,

    /// <summary>
    /// AGI included in net salary (NET_TO_GROSS mode only).
    /// Disabled for GROSS_TO_NET and TOTAL_TO_GROSS modes.
    /// </summary>
    AgiIncludedInNet = 1 << 6,

    /// <summary>All capabilities enabled.</summary>
    All = AgiSelection | EducationType | MinWageExemption | AgiCalculation
        | Discount5746 | RnDDaysInput | AgiIncludedInNet,
}
