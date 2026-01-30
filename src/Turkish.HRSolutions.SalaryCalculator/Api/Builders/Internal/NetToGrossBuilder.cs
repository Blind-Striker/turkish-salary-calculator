using Turkish.HRSolutions.SalaryCalculator.Api.Requests;
using Turkish.HRSolutions.SalaryCalculator.Api.Responses;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Api.Builders.Internal;

/// <inheritdoc />
internal sealed class NetToGrossBuilder : INetToGrossBuilder
{
    private readonly ISalaryCalculator _calculator;
    private int _year;
    private EmployeeTypeId _employeeType;
    private DisabilityDegreeId _disability;
    private bool _isPensioner;
    private bool _applyMinWageExemption = true;
    private bool _apply5746Discount;
    private AgiSettings? _agi;
    private RnDSettings? _rnd;

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
    public NetToGrossRequest Build(IEnumerable<MonthlyInput> months)
    {
        var monthList = months as IReadOnlyList<MonthlyInput> ?? [.. months];
        return new NetToGrossRequest(
            _year, monthList, _employeeType, _disability,
            _isPensioner, _applyMinWageExemption, _apply5746Discount,
            _agi, _rnd);
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
