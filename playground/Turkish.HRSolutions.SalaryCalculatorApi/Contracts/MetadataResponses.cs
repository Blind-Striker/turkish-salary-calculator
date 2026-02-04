using Turkish.HRSolutions.SalaryCalculator.Application.Validation;

namespace Turkish.HRSolutions.SalaryCalculatorApi.Contracts;

// ═══════════════════════════════════════════════════════════════════════════════
// Metadata Responses - Most use library types directly
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Response containing all available calculation years.
/// </summary>
/// <param name="Years">List of available years in descending order.</param>
public sealed record AvailableYearsResponse(IReadOnlyList<int> Years);

/// <summary>
/// Response containing all employee types.
/// Uses library EmployeeTypeInfo directly.
/// </summary>
/// <param name="EmployeeTypes">List of employee types.</param>
public sealed record EmployeeTypesResponse(IReadOnlyList<EmployeeTypeInfo> EmployeeTypes);

/// <summary>
/// Response containing all education types.
/// Uses library EducationTypeInfo directly.
/// </summary>
/// <param name="EducationTypes">List of education types.</param>
public sealed record EducationTypesResponse(IReadOnlyList<EducationTypeInfo> EducationTypes);

/// <summary>
/// Response containing all disability degrees.
/// Uses library DisabilityDegreeInfo directly.
/// </summary>
/// <param name="DisabilityDegrees">List of disability degrees.</param>
public sealed record DisabilityDegreesResponse(IReadOnlyList<DisabilityDegreeInfo> DisabilityDegrees);

/// <summary>
/// Capability information for dynamic UI behavior.
/// </summary>
/// <param name="DisabledCapabilities">List of disabled capability codes.</param>
/// <param name="AvailableCapabilities">List of available capability codes.</param>
public sealed record CapabilitiesResponse(IReadOnlyList<string> DisabledCapabilities, IReadOnlyList<string> AvailableCapabilities);
