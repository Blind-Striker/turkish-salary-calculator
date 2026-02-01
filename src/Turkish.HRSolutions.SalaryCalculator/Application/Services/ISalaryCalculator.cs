using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

/// <summary>
/// Primary interface for the Turkish Salary Calculator service.
/// Validates requests and performs salary calculations.
/// </summary>
/// <remarks>
/// <para>
/// Use via DI (<c>services.AddSalaryCalculator()</c>) or standalone
/// (<c>SalaryCalculatorBuilder.Create()</c>).
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
    /// Calculates salary using the unified request type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the primary calculation method that supports all calculation modes
    /// through a single entry point. The calculation mode is determined by the
    /// <see cref="SalaryCalculationRequest.Mode"/> property.
    /// </para>
    /// <para>
    /// Use the fluent builders (<see cref="Application.Builders.IGrossToNetBuilder"/>, etc.)
    /// for a more guided API experience, or construct <see cref="SalaryCalculationRequest"/>
    /// directly for full control.
    /// </para>
    /// </remarks>
    /// <param name="request">The unified salary calculation request.</param>
    /// <returns>A result containing the yearly salary snapshot, or errors if validation fails.</returns>
    public Result<YearlySalarySnapshot> Calculate(SalaryCalculationRequest request);
}
