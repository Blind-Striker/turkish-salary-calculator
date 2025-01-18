namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.NetToGross;

public class NetToGrossMonthlyCalcModel : BaseMonthlyCalculationModel
{
    public NetToGrossMonthlyCalcModel(EmployeeMonthlyParameters employeeEmployeeMonthlyParameters, NetToGrossMonthlyCalcModel? previousMonthCalculationModel = null)
        : base(employeeEmployeeMonthlyParameters, previousMonthCalculationModel)
    {
    }

    public override double Calculate(bool applyEmployerDiscount5746, bool isAgiCalculationEnabled, bool isAgiIncludedTax, bool applyMinWageTaxExemption)
    {
        (_, var monthlySalarySalaryAmount, var workedDay, _) = EmployeeMonthlyParameters.MonthlySalary;
        var monthlySalaryAmount = monthlySalarySalaryAmount * (workedDay / (double)CalculationConstants.MonthDayCount);

        var left = monthlySalaryAmount / 30;
        var right = 30 * monthlySalaryAmount;
        var middle = (right + left) / 2;
        const int itLimit = 100;
        double? calcNetSalary = null;
        int i;

        for (i = 0; i < itLimit && (calcNetSalary == null || Math.Abs(calcNetSalary.Value - monthlySalaryAmount) > 0.0001); i++)
        {
            // could not find
            if (right < left)
            {
                return -1;
            }

            InternalCalculate(applyEmployerDiscount5746, isAgiCalculationEnabled, isAgiIncludedTax, applyMinWageTaxExemption, middle);

            calcNetSalary = isAgiIncludedTax ? NetSalary + AgiAmount : NetSalary;
            if (calcNetSalary > monthlySalaryAmount)
            {
                right = middle;
            }
            else
            {
                left = middle;
            }

            middle = (right + left) / 2;
        }

        if (i == itLimit)
        {
            return -1;
        }

        return middle;
    }
}