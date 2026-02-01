using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Providers;

/// <summary>
/// Provides calculation constants (rates, employee type definitions).
/// </summary>
/// <remarks>
/// <para>
/// Calculation constants include employee type flags (17 booleans per type),
/// SGK rates, stamp tax rates, and other fixed parameters defined by Turkish law.
/// </para>
/// <para>
/// By default, embedded constants are used. These are legally mandated values
/// and should not normally be modified.
/// </para>
/// <para>
/// <b>Warning:</b> If you override calculation constants, you are responsible for
/// ensuring correctness. Validation rules depend on employee type flags.
/// Incorrect flags will cause validation errors or wrong calculations.
/// </para>
/// </remarks>
public interface ICalculationConstantsProvider
{
    /// <summary>
    /// Gets the core calculation constants (rates, day counts, etc.).
    /// </summary>
    public CalculationConstant Constants { get; }

    /// <summary>
    /// Gets the employee type definition for the specified type ID.
    /// </summary>
    /// <param name="typeId">The employee type identifier.</param>
    /// <returns>The employee type constant if found; otherwise null.</returns>
    public EmployeeTypeConstant? GetEmployeeType(EmployeeTypeId typeId);

    /// <summary>
    /// Gets the employee type definition for the specified type ID value.
    /// </summary>
    /// <param name="typeId">The numeric employee type identifier.</param>
    /// <returns>The employee type constant if found; otherwise null.</returns>
    public EmployeeTypeConstant? GetEmployeeType(int typeId);

    /// <summary>
    /// Gets all defined employee types.
    /// </summary>
    public IReadOnlyList<EmployeeTypeConstant> AllEmployeeTypes { get; }

    /// <summary>
    /// Gets the disability constant for the specified degree.
    /// </summary>
    /// <param name="degreeId">The disability degree identifier.</param>
    /// <returns>The disability constant if found; otherwise null.</returns>
    public DisabilityConstant? GetDisability(DisabilityDegreeId degreeId);

    /// <summary>
    /// Gets the disability constant for the specified degree value.
    /// </summary>
    /// <param name="degree">The numeric disability degree.</param>
    /// <returns>The disability constant if found; otherwise null.</returns>
    public DisabilityConstant? GetDisability(int degree);

    /// <summary>
    /// Gets the education type constant for the specified type ID.
    /// </summary>
    /// <param name="typeId">The education type identifier.</param>
    /// <returns>The education type constant if found; otherwise null.</returns>
    public EmployeeEducationTypeConstant? GetEducationType(EducationTypeId typeId);

    /// <summary>
    /// Gets all defined education types.
    /// </summary>
    public IReadOnlyList<EmployeeEducationTypeConstant> AllEducationTypes { get; }

    /// <summary>
    /// Gets all AGI options.
    /// </summary>
    public IReadOnlyList<AgiConstant> AllAgiOptions { get; }

    /// <summary>
    /// Gets the AGI constant for the specified ID.
    /// </summary>
    /// <param name="agiId">The AGI option identifier.</param>
    /// <returns>The AGI constant if found; otherwise null.</returns>
    public AgiConstant? GetAgi(int agiId);

    /// <summary>
    /// Resolves AGI rate from spouse status and children count.
    /// Provider owns its data schema interpretation.
    /// </summary>
    /// <param name="spouseStatus">The spouse/marital status.</param>
    /// <param name="numberOfChildren">The number of children.</param>
    /// <returns>The AGI rate (typically 0.0 to 0.5 range).</returns>
    public double GetAgiRate(SpouseStatus spouseStatus, int numberOfChildren)
    {
        var agiOptions = AllAgiOptions;
        if (agiOptions.Count == 0)
        {
            return 0d;
        }

        // Use SpouseStatus.EffectiveAgiTypeKey directly
        var typeFilter = spouseStatus.EffectiveAgiTypeKey;

        var filtered = agiOptions.Where(a => a.Type.Equals(typeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        if (filtered.Count == 0)
        {
            return 0d;
        }

        // Unmarried has a single entry (no children matching)
        if (spouseStatus == SpouseStatus.Unmarried)
        {
            return filtered[0].Rate;
        }

        // Match by equality operator + children count
        var match = filtered.FirstOrDefault(p => p.Equality switch
        {
            "Eq" => numberOfChildren == p.Children,
            "Gt" => numberOfChildren > p.Children,
            "Gte" => numberOfChildren >= p.Children,
            "Lt" => numberOfChildren < p.Children,
            "Lte" => numberOfChildren <= p.Children,
            _ => false,
        });

        return match?.Rate ?? filtered.OrderBy(p => Math.Abs(p.Children - numberOfChildren)).First().Rate;
    }
}
