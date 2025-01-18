namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.GrossToNet;

public class GrossToNetYearlyCalcModel : BaseNetToGrossYearlyCalcModel<GrossToNetMonthlyCalcModel>
{
    public GrossToNetYearlyCalcModel(EmployeeYearlyParameters employeeYearlyParameters) : base(employeeYearlyParameters)
    {
        //Check months consistency
        var monthlySalaries = employeeYearlyParameters.MonthlySalaries.ToList();

        GrossToNetMonthlyCalcModel? previousMonth = null;
        foreach (var monthlySalary in monthlySalaries)
        {
            var employeeMonthlyParameters = new EmployeeMonthlyParameters(employeeYearlyParameters.YearParameter,
                employeeYearlyParameters.EmployeeTypeParameter, employeeYearlyParameters.CalculationConstants, employeeYearlyParameters.EducationExemptionRate,
                employeeYearlyParameters.MinimumLivingAllowanceRate, employeeYearlyParameters.DisabilityDegree, monthlySalary,
                employeeYearlyParameters.IsPensioner);

            var netToGrossMonthlyCalcModel = new GrossToNetMonthlyCalcModel(employeeMonthlyParameters, previousMonth);
            previousMonth = netToGrossMonthlyCalcModel;

            MonthlyCalcModelsInt.Add(netToGrossMonthlyCalcModel);
        }
    }
}