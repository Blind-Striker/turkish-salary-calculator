using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

/// <summary>
/// Provides year-specific parameters (minimum wage, tax brackets, SGK ceiling).
/// </summary>
/// <remarks>
/// <para>
/// Year parameters are updated annually as Turkish regulations change.
/// The library ships with embedded defaults covering 2016-2026.
/// </para>
/// <para>
/// Use <see cref="EmbeddedYearParameterProvider"/> for default behavior,
/// or implement your own provider to load from external sources.
/// </para>
/// </remarks>
public interface IYearParameterProvider
{
    /// <summary>
    /// Gets parameters for the specified year, or null if not found.
    /// </summary>
    /// <param name="year">The calendar year (e.g., 2026).</param>
    /// <returns>Year parameters if available; otherwise null.</returns>
    public YearParameter? GetParameter(int year);

    /// <summary>
    /// Gets all available years from this provider.
    /// </summary>
    public IReadOnlyList<int> AvailableYears { get; }

    /// <summary>
    /// Returns true if this provider has parameters for the specified year.
    /// </summary>
    /// <param name="year">The calendar year to check.</param>
    public bool HasYear(int year);
}
