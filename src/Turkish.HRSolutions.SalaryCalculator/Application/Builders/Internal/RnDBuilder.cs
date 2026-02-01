using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Builders.Internal;

/// <inheritdoc />
internal sealed class RnDBuilder : IRnDBuilder
{
    private EducationTypeId _education = EducationTypeId.OtherRnDPersonnel;

    /// <inheritdoc />
    public IRnDBuilder WithEducation(EducationTypeId education)
    {
        _education = education;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="RnDSettings"/> from accumulated configuration.
    /// </summary>
    internal RnDSettings Build() => new(_education);
}
