using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Enums.Extensions;

public static class EmployeeTypeConstantExtensions
{
    public static EmployeeTypeConstant FindEmployeeTypeConstant(this IImmutableList<EmployeeTypeConstant> employeeTypeOptions, EmployeeType employeeType)
    {
        ArgumentNullException.ThrowIfNull(employeeTypeOptions);

        return employeeTypeOptions.Single(type => type.Id == (int)employeeType);
    }
}
