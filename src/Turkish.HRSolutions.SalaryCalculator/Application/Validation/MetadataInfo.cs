using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Information about an employee type for metadata queries.
/// </summary>
/// <param name="Id">The employee type identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="Description">The description text.</param>
public sealed record EmployeeTypeInfo(EmployeeTypeId Id, string Name, string? Description);

/// <summary>
/// Information about an education type for metadata queries.
/// </summary>
/// <param name="Id">The education type identifier.</param>
/// <param name="Name">The display name.</param>
public sealed record EducationTypeInfo(EducationTypeId Id, string Name);

/// <summary>
/// Information about a disability degree for metadata queries.
/// </summary>
/// <param name="Id">The disability degree identifier.</param>
/// <param name="Name">The display name.</param>
public sealed record DisabilityDegreeInfo(DisabilityDegreeId Id, string Name);
