using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Enums.Extensions;

public static class DisabilityOptionExtensions
{
    public static DisabilityConstant FindDisabilityConstant(this IImmutableList<DisabilityConstant> disabilityOptions, DisabilityDegree degree)
    {
        ArgumentNullException.ThrowIfNull(disabilityOptions);

        return disabilityOptions.Single(type => type.Degree == (int)degree);
    }
}
