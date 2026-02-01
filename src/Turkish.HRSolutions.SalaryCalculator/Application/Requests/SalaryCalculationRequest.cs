using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Requests;

/// <summary>
/// Unified salary calculation request that supports all calculation modes.
/// </summary>
/// <remarks>
/// <para>
/// This is the primary request type for salary calculations. The <see cref="Mode"/> property
/// determines how the salary amount is interpreted (gross, net, or total employer cost).
/// </para>
/// <para>
/// <b>Usage via fluent builder:</b>
/// <code>
/// // Gross to Net
/// var request = SalaryCalculationRequest.For(2024)
///     .WithMode(CalculationMode.GrossToNet)
///     .WithMonths(MonthlyInput.Uniform(50000m))
///     .Build();
///
/// // Net to Gross with AGI included
/// var request = SalaryCalculationRequest.For(2024)
///     .WithMode(CalculationMode.NetToGross)
///     .WithMonths(MonthlyInput.Uniform(35000m))
///     .WithAgiIncludedInNet()
///     .Build();
/// </code>
/// </para>
/// <para>
/// <b>Alternative:</b> Use the mode-specific fluent builders
/// (<see cref="Application.Builders.IGrossToNetBuilder"/>, etc.) for a more guided API.
/// </para>
/// </remarks>
/// <param name="Mode">The calculation mode determining how the salary amount is interpreted.</param>
/// <param name="Year">The calculation year (required).</param>
/// <param name="Months">Monthly input data - 12 months of salary amounts (required).</param>
/// <param name="EmployeeType">Employee type for tax/exemption calculation.</param>
/// <param name="Disability">Disability degree for tax deduction.</param>
/// <param name="IsPensioner">If true, the employee is retired and re-employed (different SSK rates).</param>
/// <param name="ApplyMinWageExemption">Apply minimum wage income tax exemption (2022+).</param>
/// <param name="Apply5746Discount">Apply 5746 additional 50% SGK discount.</param>
/// <param name="Agi">AGI settings for years before 2022.</param>
/// <param name="RnD">R&amp;D settings for 5746 employee types.</param>
/// <param name="IsAgiIncludedInNet">
/// If true, the desired net salary includes AGI amount (binary search target adjustment).
/// Only applies to <see cref="CalculationMode.NetToGross"/> calculations. Default: false.
/// </param>
public sealed record SalaryCalculationRequest(
    CalculationMode Mode,
    int Year,
    IReadOnlyList<MonthlyInput> Months,
    EmployeeTypeId EmployeeType = default,
    DisabilityDegreeId Disability = default,
    bool IsPensioner = false,
    bool ApplyMinWageExemption = true,
    bool Apply5746Discount = false,
    AgiSettings? Agi = null,
    RnDSettings? RnD = null,
    bool IsAgiIncludedInNet = false)
{
    /// <summary>
    /// Gets the employee type, defaulting to Standard if not specified.
    /// </summary>
    public EmployeeTypeId EmployeeType { get; init; } = EmployeeType == default
        ? EmployeeTypeId.Standard
        : EmployeeType;

    /// <summary>
    /// Gets the disability degree, defaulting to None if not specified.
    /// </summary>
    public DisabilityDegreeId Disability { get; init; } = Disability == default
        ? DisabilityDegreeId.None
        : Disability;

    /// <summary>
    /// Starts building a salary calculation request for the specified year.
    /// </summary>
    /// <param name="year">The calculation year.</param>
    /// <returns>A builder to configure the request.</returns>
    public static SalaryCalculationRequestBuilder For(int year) => new(year);
}

/// <summary>
/// Fluent builder for <see cref="SalaryCalculationRequest"/>.
/// </summary>
public sealed class SalaryCalculationRequestBuilder
{
    private readonly int _year;
    private CalculationMode _mode = CalculationMode.GrossToNet;
    private IReadOnlyList<MonthlyInput>? _months;
    private EmployeeTypeId _employeeType;
    private DisabilityDegreeId _disability;
    private bool _isPensioner;
    private bool _applyMinWageExemption = true;
    private bool _apply5746Discount;
    private AgiSettings? _agi;
    private RnDSettings? _rnd;
    private bool _isAgiIncludedInNet;

    internal SalaryCalculationRequestBuilder(int year) => _year = year;

    /// <summary>
    /// Sets the calculation mode.
    /// </summary>
    public SalaryCalculationRequestBuilder WithMode(CalculationMode mode)
    {
        _mode = mode;
        return this;
    }

    /// <summary>
    /// Sets the monthly input data.
    /// </summary>
    public SalaryCalculationRequestBuilder WithMonths(IReadOnlyList<MonthlyInput> months)
    {
        _months = months;
        return this;
    }

    /// <summary>
    /// Sets the employee type.
    /// </summary>
    public SalaryCalculationRequestBuilder WithEmployeeType(EmployeeTypeId employeeType)
    {
        _employeeType = employeeType;
        return this;
    }

    /// <summary>
    /// Sets the disability degree.
    /// </summary>
    public SalaryCalculationRequestBuilder WithDisability(DisabilityDegreeId disability)
    {
        _disability = disability;
        return this;
    }

    /// <summary>
    /// Sets the pensioner flag.
    /// </summary>
    public SalaryCalculationRequestBuilder AsPensioner(bool isPensioner = true)
    {
        _isPensioner = isPensioner;
        return this;
    }

    /// <summary>
    /// Sets whether to apply minimum wage tax exemption (2022+).
    /// </summary>
    public SalaryCalculationRequestBuilder WithMinWageExemption(bool apply = true)
    {
        _applyMinWageExemption = apply;
        return this;
    }

    /// <summary>
    /// Sets whether to apply 5746 additional 50% SGK discount.
    /// </summary>
    public SalaryCalculationRequestBuilder With5746Discount(bool apply = true)
    {
        _apply5746Discount = apply;
        return this;
    }

    /// <summary>
    /// Sets AGI settings for years before 2022.
    /// </summary>
    public SalaryCalculationRequestBuilder WithAgi(AgiSettings agi)
    {
        _agi = agi;
        return this;
    }

    /// <summary>
    /// Sets R&amp;D settings for 5746 employee types.
    /// </summary>
    public SalaryCalculationRequestBuilder WithRnD(RnDSettings rnd)
    {
        _rnd = rnd;
        return this;
    }

    /// <summary>
    /// Sets whether the desired net salary includes AGI amount.
    /// Only applicable for <see cref="CalculationMode.NetToGross"/> calculations.
    /// </summary>
    public SalaryCalculationRequestBuilder WithAgiIncludedInNet(bool include = true)
    {
        _isAgiIncludedInNet = include;
        return this;
    }

    /// <summary>
    /// Builds the request. Throws if required fields are missing.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if months are not specified.</exception>
    public SalaryCalculationRequest Build()
    {
        if (_months is null || _months.Count == 0)
        {
            throw new InvalidOperationException("Monthly input data is required. Use WithMonths() to specify.");
        }

        return new SalaryCalculationRequest(
            _mode,
            _year,
            _months,
            _employeeType,
            _disability,
            _isPensioner,
            _applyMinWageExemption,
            _apply5746Discount,
            _agi,
            _rnd,
            _isAgiIncludedInNet);
    }
}
