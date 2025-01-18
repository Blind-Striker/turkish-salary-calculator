namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.Base;

public class BaseNetToGrossYearlyCalcModel<TMonthlyCalcModel> where TMonthlyCalcModel : BaseMonthlyCalculationModel
{
    protected IList<TMonthlyCalcModel> MonthlyCalcModelsInt { get; }
    public int NumOfCalculatedMonths { get; protected set; }

    public bool IsAgiIncludedNet { get; set; }
    public bool IsAgiIncludedTax { get; set; }
    public bool IsAgiCalculationEnabled { get; set; }
    public bool ApplyMinWageTaxExemption { get; set; }
    public bool ApplyEmployerDiscount5746 { get; set; }

    public BaseNetToGrossYearlyCalcModel(EmployeeYearlyParameters employeeYearlyParameters)
    {
        ArgumentNullException.ThrowIfNull(employeeYearlyParameters);

        IsAgiCalculationEnabled = true;
        IsAgiIncludedTax = true;
        ApplyMinWageTaxExemption = true;

        //Check months consistency
        var monthlySalaries = employeeYearlyParameters.MonthlySalaries.ToList();
        MonthlyCalcModelsInt = new List<TMonthlyCalcModel>(monthlySalaries.Count);
    }

    public void Calculate()
    {
        NumOfCalculatedMonths = 0;

        foreach (var monthlyCalcModel in MonthlyCalcModelsInt)
        {
            monthlyCalcModel.Calculate(ApplyEmployerDiscount5746, IsAgiCalculationEnabled, IsAgiIncludedTax, ApplyMinWageTaxExemption);

            if (monthlyCalcModel.CalculatedGrossSalary > 0)
            {
                NumOfCalculatedMonths++;
            }
        }
    }

    public IImmutableList<TMonthlyCalcModel> MonthlyCalcModels => MonthlyCalcModelsInt.ToImmutableList();

    public double CalculatedGrossSalary
    {
        get
        {
            var totalSum = MonthlyCalcModelsInt.Sum(month => month.CalculatedGrossSalary);
            return double.IsNaN(totalSum) ? 0 : totalSum;
        }
    }

    public double TotalWorkDays
    {
        get
        {
            double totalSum = MonthlyCalcModelsInt.Sum(month => month.WorkedDay);
            return double.IsNaN(totalSum) ? 0 : totalSum;
        }
    }

    public double AvgWorkDays => NumOfCalculatedMonths > 0 ? TotalWorkDays / NumOfCalculatedMonths : 0;
}