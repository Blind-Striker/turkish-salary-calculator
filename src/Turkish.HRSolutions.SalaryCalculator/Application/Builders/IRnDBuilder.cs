using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Builders;

/// <summary>
/// Fluent builder for R&amp;D (Law 5746) exemption settings.
/// </summary>
public interface IRnDBuilder
{
    /// <summary>
    /// Sets the education type for 5746 exemption calculation.
    /// </summary>
    public IRnDBuilder WithEducation(EducationTypeId education);
}
