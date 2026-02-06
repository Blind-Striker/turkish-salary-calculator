using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Context for validation containing all relevant parameters.
/// </summary>
/// <remarks>
/// Used by ValidationEngine to determine capabilities and validate input.
/// Factory methods provide clean conversion from request DTOs.
/// </remarks>
internal sealed record ValidationContext
{
    /// <summary>Gets the calculation year.</summary>
    public required int Year { get; init; }

    /// <summary>Gets the calculation mode.</summary>
    public required CalculationMode Mode { get; init; }

    /// <summary>Gets the employee type identifier.</summary>
    public required EmployeeTypeId EmployeeType { get; init; }

    /// <summary>Gets whether the employee is a pensioner.</summary>
    public required bool IsPensioner { get; init; }

    /// <summary>Gets the monthly inputs.</summary>
    public required IReadOnlyList<MonthlyInput> Months { get; init; }

    /// <summary>Gets the AGI settings (nullable).</summary>
    public AgiSettings? Agi { get; init; }

    /// <summary>Gets the R&amp;D settings (nullable).</summary>
    public RnDSettings? RnD { get; init; }

    /// <summary>Gets whether to apply minimum wage tax exemption.</summary>
    public bool ApplyMinWageExemption { get; init; }

    /// <summary>Gets whether to apply 5746 SGK discount.</summary>
    public bool Apply5746Discount { get; init; }

    /// <summary>Gets the disability degree.</summary>
    public DisabilityDegreeId Disability { get; init; }

    /// <summary>
    /// Gets whether AGI should be included in net for binary search target.
    /// Only meaningful for NetToGross calculations.
    /// </summary>
    public bool IsAgiIncludedInNet { get; init; }

    /// <summary>
    /// Creates a validation context from a <see cref="SalaryCalculationRequest"/>.
    /// </summary>
    public static ValidationContext FromRequest(SalaryCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ValidationContext
        {
            Year = request.Year,
            Mode = request.Mode,
            EmployeeType = request.EmployeeType,
            IsPensioner = request.IsPensioner,
            Months = request.Months,
            Agi = request.Agi,
            RnD = request.RnD,
            ApplyMinWageExemption = request.ApplyMinWageExemption,
            Apply5746Discount = request.Apply5746Discount,
            Disability = request.Disability,
            IsAgiIncludedInNet = request.IsAgiIncludedInNet,
        };
    }
}
