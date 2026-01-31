using Turkish.HRSolutions.SalaryCalculator.Api.Requests;
using Turkish.HRSolutions.SalaryCalculator.Api.Responses;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Api;

/// <summary>
/// Primary interface for the Turkish Salary Calculator service.
/// Validates requests and performs salary calculations.
/// </summary>
/// <remarks>
/// <para>
/// Use via DI (<c>services.AddSalaryCalculator()</c>) or standalone
/// (<c>TurkishSalaryCalculator.Create()</c>).
/// </para>
/// <para>
/// All methods return <see cref="Result{T}"/> instead of throwing exceptions
/// for expected validation failures.
/// </para>
/// <para>
/// Provider access is exposed for introspection (available years, employee types, etc.).
/// Providers are immutable and set at construction time.
/// </para>
/// </remarks>
public interface ISalaryCalculator
{
    /// <summary>
    /// Gets the year parameter provider used by this calculator instance.
    /// </summary>
    /// <remarks>
    /// Provides access to available years, tax brackets, minimum wages, and other year-specific parameters.
    /// Immutable - set at construction time.
    /// </remarks>
    public Result<IYearParameterProvider> YearProvider { get; }

    /// <summary>
    /// Gets the calculation constants provider used by this calculator instance.
    /// </summary>
    /// <remarks>
    /// Provides access to employee types, AGI options, disability degrees, and other constants.
    /// Immutable - set at construction time.
    /// </remarks>
    public Result<ICalculationConstantsProvider> ConstantsProvider { get; }
    /// <summary>
    /// Calculates salary from gross amounts.
    /// </summary>
    /// <param name="request">The gross-to-net calculation request.</param>
    /// <returns>A result containing the yearly salary snapshot, or errors if validation fails.</returns>
    public Result<YearlySalarySnapshot> Calculate(GrossToNetRequest request);

    /// <summary>
    /// Calculates the required gross salary from desired net salary using binary search.
    /// </summary>
    /// <param name="request">The net-to-gross calculation request.</param>
    /// <returns>A result containing the yearly salary snapshot, or errors if validation fails.</returns>
    public Result<YearlySalarySnapshot> Calculate(NetToGrossRequest request);

    /// <summary>
    /// Calculates gross salary from total employer cost budget using binary search.
    /// </summary>
    /// <param name="request">The total-to-gross calculation request.</param>
    /// <returns>A result containing the yearly salary snapshot, or errors if validation fails.</returns>
    public Result<YearlySalarySnapshot> Calculate(TotalToGrossRequest request);
}
