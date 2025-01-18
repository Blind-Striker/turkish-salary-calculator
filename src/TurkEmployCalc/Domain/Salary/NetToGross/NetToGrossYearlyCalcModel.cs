namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.NetToGross;

public class NetToGrossYearlyCalcModel : BaseNetToGrossYearlyCalcModel<NetToGrossMonthlyCalcModel>
{
    public NetToGrossYearlyCalcModel(EmployeeYearlyParameters employeeYearlyParameters) : base(employeeYearlyParameters)
    {
        //Check months consistency
        var monthlySalaries = employeeYearlyParameters.MonthlySalaries.ToList();

        NetToGrossMonthlyCalcModel? previousMonth = null;
        foreach (var monthlySalary in monthlySalaries)
        {
            var employeeMonthlyParameters = new EmployeeMonthlyParameters(employeeYearlyParameters.YearParameter,
                employeeYearlyParameters.EmployeeTypeParameter, employeeYearlyParameters.CalculationConstants, employeeYearlyParameters.EducationExemptionRate,
                employeeYearlyParameters.MinimumLivingAllowanceRate, employeeYearlyParameters.DisabilityDegree, monthlySalary,
                employeeYearlyParameters.IsPensioner);

            var netToGrossMonthlyCalcModel = new NetToGrossMonthlyCalcModel(employeeMonthlyParameters, previousMonth);
            previousMonth = netToGrossMonthlyCalcModel;

            MonthlyCalcModelsInt.Add(netToGrossMonthlyCalcModel);
        }
    }
}