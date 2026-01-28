using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Api.Requests;

/// <summary>
/// Request for Net-to-Gross salary calculation.
/// </summary>
/// <remarks>
/// <para>
/// <b>Calculation Mode:</b> Given net salaries, calculates gross salaries using binary search.
/// Useful when employee contracts specify net salary amounts.
/// </para>
/// <para>
/// <b>Usage:</b> Create a request using the builder pattern:
/// <code>
/// var request = NetToGrossRequest.For(year: 2024)
///     .WithMonths(MonthlyInput.Uniform(35000m))
///     .WithEmployeeType(EmployeeTypeId.Standard)
///     .Build();
/// </code>
/// </para>
/// </remarks>
/// <param name="Year">The calculation year (required).</param>
/// <param name="Months">Monthly input data - 12 months of net salaries (required).</param>
/// <param name="EmployeeType">Employee type for tax/exemption calculation.</param>
/// <param name="Disability">Disability degree for tax deduction.</param>
/// <param name="IsPensioner">If true, employee is retired and re-employed (different SSK rates).</param>
/// <param name="ApplyMinWageExemption">Apply minimum wage income tax exemption (2022+).</param>
/// <param name="Apply5746Discount">Apply 5746 additional 50% SGK discount.</param>
/// <param name="Agi">AGI settings for years before 2022.</param>
/// <param name="RnD">R&amp;D settings for 5746 employee types.</param>
public sealed record NetToGrossRequest(
    int Year,
    IReadOnlyList<MonthlyInput> Months,
    EmployeeTypeId EmployeeType = default,
    DisabilityDegreeId Disability = default,
    bool IsPensioner = false,
    bool ApplyMinWageExemption = true,
    bool Apply5746Discount = false,
    AgiSettings? Agi = null,
    RnDSettings? RnD = null)
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
    /// Starts building a Net-to-Gross request for the specified year.
    /// </summary>
    /// <param name="year">The calculation year.</param>
    /// <returns>A builder to configure the request.</returns>
    public static NetToGrossRequestBuilder For(int year) => new(year);
}

/// <summary>
/// Fluent builder for <see cref="NetToGrossRequest"/>.
/// </summary>
public sealed class NetToGrossRequestBuilder
{
    private readonly int _year;
    private IReadOnlyList<MonthlyInput>? _months;
    private EmployeeTypeId _employeeType;
    private DisabilityDegreeId _disability;
    private bool _isPensioner;
    private bool _applyMinWageExemption = true;
    private bool _apply5746Discount;
    private AgiSettings? _agi;
    private RnDSettings? _rnd;

    internal NetToGrossRequestBuilder(int year) => _year = year;

    /// <summary>
    /// Sets the monthly input data.
    /// </summary>
    public NetToGrossRequestBuilder WithMonths(IReadOnlyList<MonthlyInput> months)
    {
        _months = months;
        return this;
    }

    /// <summary>
    /// Sets the employee type.
    /// </summary>
    public NetToGrossRequestBuilder WithEmployeeType(EmployeeTypeId employeeType)
    {
        _employeeType = employeeType;
        return this;
    }

    /// <summary>
    /// Sets the disability degree.
    /// </summary>
    public NetToGrossRequestBuilder WithDisability(DisabilityDegreeId disability)
    {
        _disability = disability;
        return this;
    }

    /// <summary>
    /// Sets the pensioner flag.
    /// </summary>
    public NetToGrossRequestBuilder AsPensioner(bool isPensioner = true)
    {
        _isPensioner = isPensioner;
        return this;
    }

    /// <summary>
    /// Sets whether to apply minimum wage tax exemption (2022+).
    /// </summary>
    public NetToGrossRequestBuilder WithMinWageExemption(bool apply = true)
    {
        _applyMinWageExemption = apply;
        return this;
    }

    /// <summary>
    /// Sets whether to apply 5746 additional 50% SGK discount.
    /// </summary>
    public NetToGrossRequestBuilder With5746Discount(bool apply = true)
    {
        _apply5746Discount = apply;
        return this;
    }

    /// <summary>
    /// Sets AGI settings for years before 2022.
    /// </summary>
    public NetToGrossRequestBuilder WithAgi(AgiSettings agi)
    {
        _agi = agi;
        return this;
    }

    /// <summary>
    /// Sets R&amp;D settings for 5746 employee types.
    /// </summary>
    public NetToGrossRequestBuilder WithRnD(RnDSettings rnd)
    {
        _rnd = rnd;
        return this;
    }

    /// <summary>
    /// Builds the request. Throws if required fields are missing.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if months are not specified.</exception>
    public NetToGrossRequest Build()
    {
        if (_months is null || _months.Count == 0)
        {
            throw new InvalidOperationException("Monthly input data is required. Use WithMonths() to specify.");
        }

        return new NetToGrossRequest(
            _year,
            _months,
            _employeeType,
            _disability,
            _isPensioner,
            _applyMinWageExemption,
            _apply5746Discount,
            _agi,
            _rnd);
    }
}
