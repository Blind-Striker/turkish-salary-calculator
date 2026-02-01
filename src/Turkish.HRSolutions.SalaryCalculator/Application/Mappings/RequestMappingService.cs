using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Mappings;

/// <summary>
/// Maps API requests to domain model types.
/// Pure shape conversion - no lookups, no validation, no Result.
/// </summary>
internal static class RequestMappingService
{
    /// <summary>
    /// Maps a validated request context and resolved lookups to domain parameters.
    /// </summary>
    /// <remarks>
    /// This is a pure conversion function. All lookups must be resolved before calling.
    /// Validation must have passed before calling this method.
    /// </remarks>
    /// <param name="context">The validated request context.</param>
    /// <param name="yearParam">Resolved year parameter.</param>
    /// <param name="employeeType">Resolved employee type constant.</param>
    /// <param name="standardEmployeeType">Resolved standard employee type constant (ID=1).</param>
    /// <param name="constants">Calculation constants.</param>
    /// <param name="disabilityDegree">Resolved disability degree value.</param>
    /// <param name="educationExemptionRate">Resolved education exemption rate.</param>
    /// <param name="agiRate">Resolved AGI rate.</param>
    /// <returns>Domain parameters and calculation options (tuple).</returns>
    internal static (EmployeeYearlyParameters YearlyParams, CalculationOptions Options) MapToDomain(
        ValidationContext context,
        YearParameter yearParam,
        EmployeeTypeConstant employeeType,
        EmployeeTypeConstant standardEmployeeType,
        CalculationConstant constants,
        int disabilityDegree,
        double educationExemptionRate,
        double agiRate)
    {
        // Convert MonthlyInput[] → MonthlySalary[]
        var monthlySalaries = MapMonthlyInputs(context.Months);

        // Build domain parameters
        var yearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParam,
            MonthlySalaries: monthlySalaries,
            EmployeeTypeConstant: employeeType,
            StandardEmployeeTypeConstant: standardEmployeeType,
            CalculationConstants: constants,
            EmployeeEducationExemptionRate: educationExemptionRate,
            AgiRate: agiRate,
            DisabilityDegree: disabilityDegree,
            IsPensioner: context.IsPensioner);

        // Build calculation options
        var options = new CalculationOptions(
            ApplyMinWageTaxExemption: context.ApplyMinWageExemption,
            ApplyEmployerDiscount5746: context.Apply5746Discount,
            IsAgiCalculationEnabled: context.Agi is not null,
            IsAgiIncludedTax: context.Agi?.IncludeInTax ?? false,
            IsAgiIncludedNet: context.IsAgiIncludedInNet);

        return (yearlyParams, options);
    }

    /// <summary>
    /// Converts MonthlyInput values to domain MonthlySalary values.
    /// </summary>
    internal static IEnumerable<MonthlySalary> MapMonthlyInputs(IReadOnlyList<MonthlyInput> months)
    {
        return months.Select(m => new MonthlySalary(
            MonthsOfYear: m.Month,
            SalaryAmount: m.Salary,
            WorkedDay: (uint)m.WorkedDays,
            ResearchAndDevelopmentWorkedDays: (uint)m.RnDDays));
    }

}
