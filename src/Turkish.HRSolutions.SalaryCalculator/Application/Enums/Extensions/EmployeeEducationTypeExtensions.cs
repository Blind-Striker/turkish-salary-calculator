using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Enums.Extensions;

public static class EmployeeEducationTypeExtensions
{
    public static EmployeeEducationTypeConstant FindEmployeeEducationConstant(this IImmutableList<EmployeeEducationTypeConstant> educationTypes, EmployeeEducationType educationType)
    {
        ArgumentNullException.ThrowIfNull(educationTypes);

        return educationTypes.Single(type => type.Id == (int)educationType);
    }
}
