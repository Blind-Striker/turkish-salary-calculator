using Turkish.HRSolutions.SalaryCalculator.Api.Requests;
using Turkish.HRSolutions.SalaryCalculator.Api.Responses;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Api.Builders;

/// <summary>
/// Fluent builder for Total-Cost-to-Gross calculations.
/// Wraps <see cref="ISalaryCalculator"/> — calling Calculate() builds the request and forwards to the calculator.
/// </summary>
public interface ITotalToGrossBuilder
{
    /// <summary>Sets the calculation year. Required.</summary>
    public ITotalToGrossBuilder ForYear(int year);

    /// <summary>Sets the employee type. Default: Standard.</summary>
    public ITotalToGrossBuilder WithEmployeeType(EmployeeTypeId type);

    /// <summary>Sets the disability degree. Default: None.</summary>
    public ITotalToGrossBuilder WithDisability(DisabilityDegreeId degree);

    /// <summary>Marks the employee as a pensioner. Default: false.</summary>
    public ITotalToGrossBuilder AsPensioner(bool isPensioner = true);

    /// <summary>Apply minimum wage tax exemption. Default: true for 2022+.</summary>
    public ITotalToGrossBuilder WithMinWageExemption(bool apply = true);

    /// <summary>Apply 5746 SGK discount. Only valid for applicable employee types.</summary>
    public ITotalToGrossBuilder With5746Discount(bool apply = true);

    /// <summary>Configures AGI (Minimum Living Allowance) for pre-2022 calculations.</summary>
    public ITotalToGrossBuilder WithAgi(Action<IAgiBuilder> configure);

    /// <summary>Configures R&amp;D/Education settings for 5746 employee types.</summary>
    public ITotalToGrossBuilder WithRnDSettings(Action<IRnDBuilder> configure);

    /// <summary>Builds the request object without calculating (for serialization/inspection).</summary>
    public TotalToGrossRequest Build(IEnumerable<MonthlyInput> months);

    /// <summary>Calculate with uniform total cost budget for all 12 months.</summary>
    public Result<YearlySalarySnapshot> Calculate(decimal totalCostBudget);

    /// <summary>Calculate with explicit monthly inputs.</summary>
    public Result<YearlySalarySnapshot> Calculate(IEnumerable<MonthlyInput> months);

    /// <summary>Calculate with monthly inputs from a Result (seamless FillForward integration).</summary>
    public Result<YearlySalarySnapshot> Calculate(Result<IReadOnlyList<MonthlyInput>> monthsResult);
}
