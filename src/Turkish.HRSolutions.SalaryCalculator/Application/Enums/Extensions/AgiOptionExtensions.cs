using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Enums.Extensions;

public static class AgiOptionExtensions
{
    public static AgiConstant FindAgiConstant(this IImmutableList<AgiConstant> allowanceParameters, SpouseWorkStatus spouseWorkStatus, uint numberOfChildren)
    {
        ArgumentNullException.ThrowIfNull(allowanceParameters);

        // Filter parameters based on SpouseWorkStatus
        var filteredParameters = allowanceParameters
            .Where(p => p.Type.Equals(spouseWorkStatus.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (filteredParameters.Count == 0)
        {
            throw new InvalidOperationException("No matching parameters found for the specified SpouseWorkStatus.");
        }

        // If Unmarried, return the single matching option
        if (spouseWorkStatus == SpouseWorkStatus.Unmarried)
        {
            return filteredParameters.Single();
        }

        // Find matching option based on Equality and numberOfChildren
        var agiOptionResult = filteredParameters
            .FirstOrDefault(p => p.Equality switch
            {
                "Eq" => numberOfChildren == p.Children,
                "Gt" => numberOfChildren > p.Children,
                "Gte" => numberOfChildren >= p.Children,
                "Lt" => numberOfChildren < p.Children,
                "Lte" => numberOfChildren <= p.Children,
                _ => throw new InvalidOperationException($"Invalid Equality value: {p.Equality}"),
            });

        if (agiOptionResult != null)
        {
            return agiOptionResult;
        }

        // Find the closest match by Children difference if no exact match
        return filteredParameters
            .OrderBy(p => Math.Abs(p.Children - numberOfChildren))
            .First();
    }
}
