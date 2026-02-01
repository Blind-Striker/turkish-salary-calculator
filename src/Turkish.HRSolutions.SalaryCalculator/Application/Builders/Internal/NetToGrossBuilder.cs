using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Builders.Internal;

/// <inheritdoc />
internal sealed class NetToGrossBuilder : INetToGrossBuilder
{
    private readonly ISalaryCalculator _calculator;
    private int _year;
    private EmployeeTypeId _employeeType = EmployeeTypeId.Standard;
    private DisabilityDegreeId _disability = DisabilityDegreeId.None;
    private bool _isPensioner;
    private bool _applyMinWageExemption = true;
    private bool _apply5746Discount;
    private AgiSettings? _agi;
    private RnDSettings? _rnd;
    private bool _isAgiIncludedInNet;

    internal NetToGrossBuilder(ISalaryCalculator calculator)
    {
        _calculator = calculator;
    }

    /// <inheritdoc />
    public INetToGrossBuilder ForYear(int year)
    {
        _year = year;
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder WithEmployeeType(EmployeeTypeId type)
    {
        _employeeType = type;
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder WithDisability(DisabilityDegreeId degree)
    {
        _disability = degree;
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder AsPensioner(bool isPensioner = true)
    {
        _isPensioner = isPensioner;
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder WithMinWageExemption(bool apply = true)
    {
        _applyMinWageExemption = apply;
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder With5746Discount(bool apply = true)
    {
        _apply5746Discount = apply;
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder WithAgi(Action<IAgiBuilder> configure)
    {
        var agiBuilder = new AgiBuilder();
        configure(agiBuilder);
        _agi = agiBuilder.Build();
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder WithRnDSettings(Action<IRnDBuilder> configure)
    {
        var rndBuilder = new RnDBuilder();
        configure(rndBuilder);
        _rnd = rndBuilder.Build();
        return this;
    }

    /// <inheritdoc />
    public INetToGrossBuilder WithAgiIncludedInNet(bool include = true)
    {
        _isAgiIncludedInNet = include;
        return this;
    }

    /// <inheritdoc />
    public SalaryCalculationRequest Build(IEnumerable<MonthlyInput> months)
    {
        var monthList = months as IReadOnlyList<MonthlyInput> ?? [.. months];
        return new SalaryCalculationRequest(
            CalculationMode.NetToGross,
            _year, monthList, _employeeType, _disability,
            _isPensioner, _applyMinWageExemption, _apply5746Discount,
            _agi, _rnd, _isAgiIncludedInNet);
    }

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(decimal desiredNetSalary)
    {
        return Calculate(MonthlyInput.Uniform(desiredNetSalary));
    }

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(IEnumerable<MonthlyInput> months)
    {
        var request = Build(months);
        return _calculator.Calculate(request);
    }

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(Result<IReadOnlyList<MonthlyInput>> monthsResult)
    {
        return monthsResult.IsFailure ? Result<YearlySalarySnapshot>.Failure(monthsResult.Errors) : Calculate(monthsResult.Value);
    }
}
