using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Builders;

/// <summary>
/// Fluent builder for Net-to-Gross calculations.
/// Wraps <see cref="ISalaryCalculator"/> — calling Calculate() builds the request and forwards to the calculator.
/// </summary>
public interface INetToGrossBuilder
{
    /// <summary>Sets the calculation year. Required.</summary>
    public INetToGrossBuilder ForYear(int year);

    /// <summary>Sets the employee type. Default: Standard.</summary>
    public INetToGrossBuilder WithEmployeeType(EmployeeTypeId type);

    /// <summary>Sets the disability degree. Default: None.</summary>
    public INetToGrossBuilder WithDisability(DisabilityDegreeId degree);

    /// <summary>Marks the employee as a pensioner. Default: false.</summary>
    public INetToGrossBuilder AsPensioner(bool isPensioner = true);

    /// <summary>Apply minimum wage tax exemption. Default: true for 2022+.</summary>
    public INetToGrossBuilder WithMinWageExemption(bool apply = true);

    /// <summary>Apply 5746 SGK discount. Only valid for applicable employee types.</summary>
    public INetToGrossBuilder With5746Discount(bool apply = true);

    /// <summary>Configures AGI (Minimum Living Allowance) for pre-2022 calculations.</summary>
    public INetToGrossBuilder WithAgi(Action<IAgiBuilder> configure);

    /// <summary>Configures R&amp;D/Education settings for 5746 employee types.</summary>
    public INetToGrossBuilder WithRnDSettings(Action<IRnDBuilder> configure);

    /// <summary>
    /// Sets whether the desired net salary includes AGI amount.
    /// When true, the binary search target is adjusted to include AGI.
    /// </summary>
    public INetToGrossBuilder WithAgiIncludedInNet(bool include = true);

    /// <summary>Builds the request object without calculating (for serialization/inspection).</summary>
    public SalaryCalculationRequest Build(IEnumerable<MonthlyInput> months);

    /// <summary>Calculate with uniform desired net salary for all 12 months.</summary>
    public Result<YearlySalarySnapshot> Calculate(decimal desiredNetSalary);

    /// <summary>Calculate with explicit monthly inputs.</summary>
    public Result<YearlySalarySnapshot> Calculate(IEnumerable<MonthlyInput> months);

    /// <summary>Calculate with monthly inputs from a Result (seamless FillForward integration).</summary>
    public Result<YearlySalarySnapshot> Calculate(Result<IReadOnlyList<MonthlyInput>> monthsResult);
}
