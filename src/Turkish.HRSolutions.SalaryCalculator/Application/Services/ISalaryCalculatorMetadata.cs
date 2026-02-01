using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

/// <summary>
/// Service for querying calculator metadata, constants, and capabilities.
/// </summary>
/// <remarks>
/// <para>
/// Use this interface to:
/// </para>
/// <list type="bullet">
/// <item><description>Expose REST API metadata endpoints</description></item>
/// <item><description>Query available years, employee types, and other configuration options</description></item>
/// </list>
/// </remarks>
public interface ISalaryCalculatorMetadata
{
    /// <summary>
    /// Gets all available calculation years from configuration.
    /// </summary>
    /// <returns>A list of years that can be used for calculations.</returns>
    public IReadOnlyList<int> GetAvailableYears();

    /// <summary>
    /// Gets all employee types with their display information.
    /// </summary>
    /// <returns>A list of employee type information.</returns>
    public IReadOnlyList<EmployeeTypeInfo> GetEmployeeTypes();

    /// <summary>
    /// Gets all education types for R&amp;D exemption.
    /// </summary>
    /// <returns>A list of education type information.</returns>
    public IReadOnlyList<EducationTypeInfo> GetEducationTypes();

    /// <summary>
    /// Gets all disability degree options.
    /// </summary>
    /// <returns>A list of disability degree information.</returns>
    public IReadOnlyList<DisabilityDegreeInfo> GetDisabilityDegrees();

    /// <summary>
    /// Gets capabilities for a given configuration.
    /// Use to build dynamic UIs that mirror Angular's disabled states.
    /// </summary>
    /// <param name="year">The calculation year.</param>
    /// <param name="employeeType">The employee type (defaults to Standard if not specified).</param>
    /// <param name="isPensioner">Whether the employee is a pensioner.</param>
    /// <returns>A result containing capability information or errors if validation fails.</returns>
    public Result<CapabilityInfo> GetCapabilities(int year, EmployeeTypeId? employeeType = null, bool isPensioner = false);

    /// <summary>
    /// Checks if a specific capability is available for the given configuration.
    /// </summary>
    /// <param name="capability">The capability to check.</param>
    /// <param name="year">The calculation year.</param>
    /// <param name="employeeType">The employee type (defaults to Standard if not specified).</param>
    /// <param name="isPensioner">Whether the employee is a pensioner.</param>
    /// <returns>True if the capability is available; false if disabled or validation fails.</returns>
    public bool IsCapabilityAvailable(Capability capability, int year, EmployeeTypeId? employeeType = null, bool isPensioner = false);
}
