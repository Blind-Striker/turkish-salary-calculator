using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.Models;

public sealed class YearCalculationModel
{
    private readonly EmployeeYearlyParameters _parameters;
    private readonly List<MonthCalculationModel> _months = [];
    private int _numOfCalculatedMonths;

    public YearCalculationModel(EmployeeYearlyParameters parameters, CalculationOptions? calcOptions = null)
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        calcOptions ??= new CalculationOptions();

        MonthCalculationModel? previousMonth = null;
        foreach (var monthlyInput in _parameters.MonthlySalaries)
        {
            // Build the "per-month" parameters
            var empMonthlyParams = new EmployeeMonthlyParameters(
                YearParameter: _parameters.YearParameter,
                MonthlySalary: monthlyInput,
                EmployeeTypeConstant: _parameters.EmployeeTypeConstant,
                CalculationConstants: _parameters.CalculationConstants,
                EmployeeEducationExemptionRate: _parameters.EmployeeEducationExemptionRate,
                AgiRate: _parameters.AgiRate,
                DisabilityDegree: _parameters.DisabilityDegree,
                IsPensioner: _parameters.IsPensioner
            );

            // Construct the MonthCalculationModel
            var monthCalc = new MonthCalculationModel(empMonthlyParams, previousMonth, calcOptions);

            _months.Add(monthCalc);
            previousMonth = monthCalc;
        }
    }

    /// <summary>
    /// Main entry point to perform calculations on all months.
    /// For each MonthCalculationModel in _months, we call its Calculate(...)
    /// with the requested calcMode.
    /// </summary>
    public void Calculate(CalculationMode calcMode)
    {
        _numOfCalculatedMonths = 0;

        foreach (var month in _months)
        {
            // Perform monthly calculation
            month.Calculate(calcMode);

            // If the month ended up with a positive "CalculatedGrossSalary",
            // increment the number of calculated months
            if (month.CalculatedGrossSalary > 0)
            {
                _numOfCalculatedMonths++;
            }
        }
    }

    /// <summary>
    /// Exposes the underlying MonthCalculationModel list
    /// if needed by a UI or something else.
    /// </summary>
    public ImmutableList<MonthCalculationModel> Months => [.. _months];

    /// <summary>
    /// Sum of all months' WorkedDays.
    /// </summary>
    public decimal TotalWorkDays => _months.Sum(m => m.WorkedDays);

    public decimal AvgWorkDays => _numOfCalculatedMonths > 0
        ? TotalWorkDays / _numOfCalculatedMonths
        : 0;

    public decimal TotalResearchAndDevelopmentWorkedDays => _months.Sum(m => m.ResearchAndDevelopmentWorkedDays);

    public decimal AvgResearchAndDevelopmentWorkedDays
        => _numOfCalculatedMonths > 0
            ? TotalResearchAndDevelopmentWorkedDays / _numOfCalculatedMonths
            : 0;

    // -- CalculatedGrossSalary
    public decimal CalculatedGrossSalary => _months.Sum(m => m.CalculatedGrossSalary);

    public decimal AvgCalculatedGrossSalary
        => _numOfCalculatedMonths > 0
            ? CalculatedGrossSalary / _numOfCalculatedMonths
            : 0;

    // -- Employee SGK
    public decimal EmployeeSgkDeduction => _months.Sum(m => m.EmployeeSgkDeduction);

    public decimal AvgEmployeeSgkDeduction
        => _numOfCalculatedMonths > 0
            ? EmployeeSgkDeduction / _numOfCalculatedMonths
            : 0;

    public decimal EmployeeSgkExemption => _months.Sum(m => m.EmployeeSgkExemption);

    public decimal AvgEmployeeSgkExemption
        => _numOfCalculatedMonths > 0
            ? EmployeeSgkExemption / _numOfCalculatedMonths
            : 0;

    public decimal EmployeeFinalSgkDeduction
        => _months.Sum(m => m.EmployeeFinalSgkDeduction);

    public decimal AvgEmployeeFinalSgkDeduction
        => _numOfCalculatedMonths > 0
            ? EmployeeFinalSgkDeduction / _numOfCalculatedMonths
            : 0;

    // -- Employee Unemployment
    public decimal EmployeeUnemploymentInsuranceDeduction
        => _months.Sum(m => m.EmployeeUnemploymentInsuranceDeduction);

    public decimal AvgEmployeeUnemploymentInsuranceDeduction
        => _numOfCalculatedMonths > 0
            ? EmployeeUnemploymentInsuranceDeduction / _numOfCalculatedMonths
            : 0;

    public decimal EmployeeUnemploymentInsuranceExemption
        => _months.Sum(m => m.EmployeeUnemploymentInsuranceExemption);

    public decimal AvgEmployeeUnemploymentInsuranceExemption
        => _numOfCalculatedMonths > 0
            ? EmployeeUnemploymentInsuranceExemption / _numOfCalculatedMonths
            : 0;

    // -- Income tax
    public decimal EmployeeIncomeTax
        => _months.Sum(m => m.EmployeeIncomeTax);

    public decimal AvgEmployeeIncomeTax
        => _numOfCalculatedMonths > 0
            ? EmployeeIncomeTax / _numOfCalculatedMonths
            : 0;

    // -- Stamp tax
    public decimal StampTax
        => _months.Sum(m => m.StampTax);

    public decimal AvgStampTax
        => _numOfCalculatedMonths > 0
            ? StampTax / _numOfCalculatedMonths
            : 0;

    public decimal EmployerStampTax
        => _months.Sum(m => m.EmployerStampTax);

    public decimal AvgEmployerStampTax
        => _numOfCalculatedMonths > 0
            ? EmployerStampTax / _numOfCalculatedMonths
            : 0;

    public decimal EmployerStampTaxExemption
        => _months.Sum(m => m.EmployerStampTaxExemption);

    public decimal AvgEmployerStampTaxExemption
        => _numOfCalculatedMonths > 0
            ? EmployerStampTaxExemption / _numOfCalculatedMonths
            : 0;

    public decimal TotalStampTaxExemption
        => _months.Sum(m => m.TotalStampTaxExemption);

    public decimal AvgTotalStampTaxExemption
        => _numOfCalculatedMonths > 0
            ? TotalStampTaxExemption / _numOfCalculatedMonths
            : 0;

    // -- Net salary
    public decimal NetSalary
        => _months.Sum(m => m.NetSalary);

    public decimal AvgNetSalary
        => _numOfCalculatedMonths > 0
            ? NetSalary / _numOfCalculatedMonths
            : 0;

    // -- AGI
    public decimal AgiAmount
        => _months.Sum(m => m.AgiAmount);

    public decimal AvgAgiAmount
        => _numOfCalculatedMonths > 0
            ? AgiAmount / _numOfCalculatedMonths
            : 0;

    public decimal FinalNetSalary
        => _months.Sum(m => m.FinalNetSalary);

    public decimal AvgFinalNetSalary
        => _numOfCalculatedMonths > 0
            ? FinalNetSalary / _numOfCalculatedMonths
            : 0;

    // -- Employer SGK
    public decimal EmployerSgkDeduction
        => _months.Sum(m => m.EmployerSgkDeduction);

    public decimal AvgEmployerSgkDeduction
        => _numOfCalculatedMonths > 0
            ? EmployerSgkDeduction / _numOfCalculatedMonths
            : 0;

    public decimal EmployerSgkExemption
        => _months.Sum(m => m.EmployerSgkExemption);

    public decimal AvgEmployerSgkExemption
        => _numOfCalculatedMonths > 0
            ? EmployerSgkExemption / _numOfCalculatedMonths
            : 0;

    public decimal EmployerTotalSgkCost
        => _months.Sum(m => m.EmployerTotalSgkCost);

    public decimal AvgEmployerTotalSgkCost
        => _numOfCalculatedMonths > 0
            ? EmployerTotalSgkCost / _numOfCalculatedMonths
            : 0;

    // -- Employer Unemployment
    public decimal EmployerUnemploymentInsuranceDeduction
        => _months.Sum(m => m.EmployerUnemploymentInsuranceDeduction);

    public decimal AvgEmployerUnemploymentInsuranceDeduction
        => _numOfCalculatedMonths > 0
            ? EmployerUnemploymentInsuranceDeduction / _numOfCalculatedMonths
            : 0;

    public decimal EmployerUnemploymentInsuranceExemption
        => _months.Sum(m => m.EmployerUnemploymentInsuranceExemption);

    public decimal AvgEmployerUnemploymentInsuranceExemption
        => _numOfCalculatedMonths > 0
            ? EmployerUnemploymentInsuranceExemption / _numOfCalculatedMonths
            : 0;

    public decimal EmployerFinalIncomeTax
        => _months.Sum(m => m.EmployerFinalIncomeTax);

    public decimal AvgEmployerFinalIncomeTax
        => _numOfCalculatedMonths > 0
            ? EmployerFinalIncomeTax / _numOfCalculatedMonths
            : 0;

    public decimal EmployerIncomeTaxExemptionAmount
        => _months.Sum(m => m.EmployerIncomeTaxExemptionAmount);

    public decimal AvgEmployerIncomeTaxExemptionAmount
        => _numOfCalculatedMonths > 0
            ? EmployerIncomeTaxExemptionAmount / _numOfCalculatedMonths
            : 0;

    public decimal EmployeeMinWageTaxExemptionAmount
        => _months.Sum(m => m.EmployeeMinWageTaxExemptionAmount);

    public decimal AvgEmployeeMinWageTaxExemptionAmount
        => _numOfCalculatedMonths > 0
            ? EmployeeMinWageTaxExemptionAmount / _numOfCalculatedMonths
            : 0;

    public decimal TotalSgkExemption
        => _months.Sum(m => m.TotalSgkExemption);

    public decimal AvgTotalSgkExemption
        => _numOfCalculatedMonths > 0
            ? TotalSgkExemption / _numOfCalculatedMonths
            : 0;

    public decimal EmployerTotalCost
        => _months.Sum(m => m.EmployerTotalCost);

    public decimal AvgEmployerTotalCost
        => _numOfCalculatedMonths > 0
            ? EmployerTotalCost / _numOfCalculatedMonths
            : 0;

    public int GetSemesterWorkedDays(string half)
    {
        // half: "first" or "second"
        // TypeScript logic: 0..5 => first half, 6..11 => second half
        // in TS we had:
        //   let r = _months.length - 6; ...
        // We replicate that approach:

        var l = 0;
        var r = _months.Count - 6;
        if (half.Equals("second", StringComparison.OrdinalIgnoreCase))
        {
            l = 6;
            r = _months.Count;
        }

        var totalWorkedDays = 0;
        for (var i = l; i < r; i++)
        {
            totalWorkedDays += (int)_months[i].WorkedDays;
        }

        return totalWorkedDays;
    }

    public decimal GetSemesterTubitakAvgCost(string half)
    {
        // matches TS:
        // return yearHalfEmployerAvgTotalCost(half, false)
        //   * (this.monthDayCount * 6) / this.getSemesterWorkedDays(half);

        var halfAvgCost = YearHalfEmployerAvgTotalCost(half, skipNonWorkedMonths: false);
        var daysInSixMonths = _parameters.CalculationConstants.MonthDayCount * 6m;
        var semesterWorkedDays = GetSemesterWorkedDays(half);

        return semesterWorkedDays == 0
            ? 0
            : halfAvgCost * (daysInSixMonths / semesterWorkedDays);
    }

    public decimal EmployerHalfTotalCost(string half)
    {
        var l = 0;
        var r = _months.Count - 6;
        if (half.Equals("second", StringComparison.OrdinalIgnoreCase))
        {
            l = 6;
            r = _months.Count;
        }

        decimal sum = 0;
        for (var i = l; i < r; i++)
        {
            sum += _months[i].EmployerTotalCost;
        }

        return sum;
    }

    public decimal YearHalfEmployerAvgTotalCost(string half, bool skipNonWorkedMonths = true)
    {
        var l = 0;
        var r = _months.Count - 6;
        if (half.Equals("second", StringComparison.OrdinalIgnoreCase))
        {
            l = 6;
            r = _months.Count;
        }

        var workedMonths = 0;
        decimal totalCost = 0;

        for (var i = l; i < r; i++)
        {
            var month = _months[i];
            if (!skipNonWorkedMonths || month.CalculatedGrossSalary > 0)
            {
                workedMonths++;
            }

            totalCost += month.EmployerTotalCost;
        }

        return workedMonths == 0
            ? 0m
            : totalCost / workedMonths;
    }
}
