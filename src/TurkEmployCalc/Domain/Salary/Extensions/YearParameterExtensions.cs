namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.Extensions;

public static class YearParameterExtensions
{
    public static MinGrossWageParameter GetMinGrossWageParameter(this YearParameter yearParams, MonthsOfYear monthsOfYear)
    {
        ArgumentNullException.ThrowIfNull(yearParams);

        if (yearParams.MinGrossWages?.Any(parameter => parameter.StartMonth == 1) != true)
        {
            throw new ArgumentNullException(nameof(yearParams),
                "The year parameter does not contain any minimum gross wage parameters or any minimum gross wage parameter for January.");
        }

        return yearParams.MinGrossWages.First(minGrossWage => monthsOfYear.Number >= minGrossWage.StartMonth);
    }
}