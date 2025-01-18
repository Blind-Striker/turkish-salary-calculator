namespace TurkHRSolutions.TurkEmployCalc.Services.Parameter.Extensions;

public static class MinimumLivingAllowanceParameterExtensions
{
    private static readonly ChildrenCompareType[] CompareTypes =
    [
        ChildrenCompareType.Gte,
        ChildrenCompareType.Gt,
        ChildrenCompareType.Lte,
        ChildrenCompareType.Lt,
    ];

    public static MinimumLivingAllowanceParameter FindMinimumLivingAllowance(this IEnumerable<MinimumLivingAllowanceParameter> allowanceParameters,
        SpouseWorkStatus spouseWorkStatus, uint numberOfChildren)
    {
        if (spouseWorkStatus == SpouseWorkStatus.Unmarried)
        {
            return allowanceParameters.Single(param => param.SpouseWorkStatus == SpouseWorkStatus.Unmarried);
        }

        var allowanceList = allowanceParameters.ToList();

        // Try to find exact match
        var exactMatch = allowanceList.Find(param =>
            param.SpouseWorkStatus == spouseWorkStatus &&
            param.ChildrenCompareType == ChildrenCompareType.Eq &&
            param.NumberOfChildren == numberOfChildren);

        if (exactMatch != null)
        {
            return exactMatch;
        }

        foreach (var type in CompareTypes)
        {
            var match = allowanceList.Find(param =>
                param.SpouseWorkStatus == spouseWorkStatus
                && param.ChildrenCompareType == type
                && DoesMatchComparisonType(param.NumberOfChildren, numberOfChildren, type)
            );

            if (match != null)
            {
                return match;
            }
        }

        // If still no match, return the closest one for the given SpouseWorkStatus
        var minimumLivingAllowanceParameter = allowanceList
            .Where(param => param.SpouseWorkStatus == spouseWorkStatus)
            .MinBy(param => Math.Abs(param.NumberOfChildren - numberOfChildren));

        return minimumLivingAllowanceParameter
               ?? allowanceList.Single(param => param.SpouseWorkStatus == SpouseWorkStatus.Unmarried);
    }

    private static bool DoesMatchComparisonType(uint paramChildren, uint numberOfChildren, ChildrenCompareType type)
    {
        return type switch
        {
            ChildrenCompareType.Gte => paramChildren >= numberOfChildren,
            ChildrenCompareType.Gt => paramChildren > numberOfChildren,
            ChildrenCompareType.Lte => paramChildren <= numberOfChildren,
            ChildrenCompareType.Lt => paramChildren < numberOfChildren,
            ChildrenCompareType.Eq => paramChildren == numberOfChildren,
            _ => false,
        };
    }
}
