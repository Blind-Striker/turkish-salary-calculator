namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.GrossToNet;

public class GrossToNetMonthlyCalcModel : BaseMonthlyCalculationModel
{
    public GrossToNetMonthlyCalcModel(EmployeeMonthlyParameters employeeEmployeeMonthlyParameters, GrossToNetMonthlyCalcModel? previousMonthCalculationModel = null)
        : base(employeeEmployeeMonthlyParameters, previousMonthCalculationModel)
    {
    }

    public override double Calculate(bool applyEmployerDiscount5746, bool isAgiCalculationEnabled, bool isAgiIncludedTax, bool applyMinWageTaxExemption)
    {
#pragma warning disable S125
        // if (!(enteredAmount > 0 && workedDays > 0)) {
#pragma warning restore S125
        //     this.resetFields();
        //     return;
        // }
        // if (employeeType.researchAndDevelopmentTaxExemption && researchAndDevelopmentWorkedDays > workedDays) {
        //     throw new Error("Ar-Ge çalışma günü normal çalışma günden fazla olamaz");
        // }

        var grossSalary = Math.Max(EmployeeMonthlyParameters.MonthlySalary.SalaryAmount, MinGrossWageParameter.Amount);

        InternalCalculate(applyEmployerDiscount5746, isAgiCalculationEnabled, isAgiIncludedTax, applyMinWageTaxExemption);

        return grossSalary;
    }
}