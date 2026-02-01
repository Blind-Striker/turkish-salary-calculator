using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

/// <summary>
/// Resolves calculator capabilities based on year, employee type, and other configuration.
/// This is the single source of truth for what features are available for a given configuration.
/// </summary>
/// <remarks>
/// <para>
/// Extracted from ValidationEngine to:
/// </para>
/// <list type="bullet">
/// <item><description>Eliminate duplication between validation and metadata services</description></item>
/// <item><description>Remove awkward dependency chains (MetadataService → ValidationEngine)</description></item>
/// <item><description>Provide a clean, focused API for capability queries</description></item>
/// </list>
/// </remarks>
public interface ICapabilityResolver
{
    /// <summary>
    /// Resolves which capabilities are disabled for the given configuration.
    /// </summary>
    /// <param name="year">The calculation year.</param>
    /// <param name="employeeType">The employee type identifier.</param>
    /// <param name="isPensioner">Whether the employee is a pensioner.</param>
    /// <param name="mode">The calculation mode (affects AgiIncludedInNet capability).</param>
    /// <returns>
    /// A result containing capability info on success, or errors if year/employee type is invalid.
    /// </returns>
    public Result<CapabilityInfo> ResolveCapabilities(
        int year,
        EmployeeTypeId employeeType,
        bool isPensioner,
        CalculationMode mode);

    /// <summary>
    /// Validates that providers are correctly configured.
    /// Called during calculator construction to fail fast on misconfiguration.
    /// </summary>
    /// <returns>Success if providers are valid; failure with configuration errors otherwise.</returns>
    public Result ValidateProviders();
}
