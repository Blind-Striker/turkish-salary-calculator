using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using static Turkish.HRSolutions.SalaryCalculator.Domain.Services.CalculationFormulasDomainService;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.Models;

public sealed class MonthCalculationModel
{
    private readonly EmployeeMonthlyParameters _parameters;
    private readonly MonthCalculationModel? _previousMonthCalculation;
    private readonly CalculationOptions _calculationOptions;
    private readonly MonthsOfYear _monthsOfYear;
    private readonly MinGrossWage _minGrossWage;

    private decimal _grossSalaryForTaxBaseCalculation;
    private decimal _calculatedGrossSalary;
    private decimal _sgkBase;
    private decimal _employeeSgkDeduction;
    private decimal _employeeSgkExemptionAmount;
    private decimal _employeeUnemploymentInsuranceDeduction;
    private decimal _employeeUnemploymentInsuranceExemptionAmount;
    private decimal _employeeIncomeTax;
    private decimal _employeeIncomeTaxExemptionAmount;
    private decimal _employeeDisabledIncomeTaxBaseAmount;
    private decimal _stampTax;
    private decimal _netSalary;
    private decimal _employerStampTax;
    private decimal _employerStampTaxExemption;
    private decimal _employeeStampTaxExemption;
    private decimal _employerSgkDeduction;
    private decimal _employerSgkExemptionAmount;
    private decimal _employerUnemploymentInsuranceDeduction;
    private decimal _employerUnemploymentInsuranceExemptionAmount;
    private decimal _agiAmount;
    private decimal _employerIncomeTaxExemptionAmount;

    private int _workedDays;
    private int _researchAndDevelopmentWorkedDays;

    // The applied tax slices for logging or debug
    private List<TaxSlice> _appliedTaxSlices = [];

    public MonthCalculationModel(
        EmployeeMonthlyParameters parameters,
        MonthCalculationModel? previousMonthCalculation = null,
        CalculationOptions? calculationOptions = null)
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _previousMonthCalculation = previousMonthCalculation;
        _calculationOptions = calculationOptions ?? new CalculationOptions();
        _monthsOfYear = parameters.MonthlySalary.MonthsOfYear;
        _minGrossWage = GetMinGrossWage();
    }

    /// <summary>
    /// Main entry to perform the monthly calculation.
    /// <para>calcMode can be "GROSS_TO_NET", "NET_TO_GROSS", or "TOTAL_TO_GROSS".</para>
    /// </summary>
    public void Calculate(CalculationMode calcMode)
    {
        // 1) Reset old fields
        ResetFields();

        // 2) Basic validations
        var monthlySalary = _parameters.MonthlySalary; // has .SalaryAmount, .WorkedDay, etc.
        _workedDays = (int)monthlySalary.WorkedDay;
        _researchAndDevelopmentWorkedDays = (int)monthlySalary.ResearchAndDevelopmentWorkedDays;

        // If the user entered 0 or negative, let's bail
        if (monthlySalary.SalaryAmount <= 0 || _workedDays <= 0)
        {
            // No calculation
            return;
        }

        // If R&D days is bigger than workedDays, throw
        if (_researchAndDevelopmentWorkedDays > _workedDays && _parameters.EmployeeTypeConstant.ResearchAndDevelopmentTaxExemption)
        {
            throw new InvalidOperationException("Ar-Ge çalışma günü normal çalışma gününden fazla olamaz.");
        }

        // 3) Decide how to get final gross
        var finalGross = calcMode switch
        {
            CalculationMode.GrossToNet => monthlySalary.SalaryAmount,
            CalculationMode.NetToGross => FindGrossFromNet(monthlySalary.SalaryAmount),
            CalculationMode.TotalToGross => FindGrossFromTotalCost(monthlySalary.SalaryAmount),
            _ => monthlySalary.SalaryAmount, // fallback
        };

        if (finalGross < 0)
        {
            // Binary search or other method failed => reset
            ResetFields();
            throw new InvalidOperationException("Hesaplama başarısız");
        }

        // Force the finalGross not to be below min wage
        finalGross = Math.Max(finalGross, _minGrossWage.Amount);

        // 4) Do the actual monthly calculation
        DoMonthCalculation(finalGross);
    }

    public decimal CalcMinWageIncomeTaxBase()
    {
        var yearParam = _parameters.YearParameter;
        var empTypeConst = _parameters.EmployeeTypeConstant;
        var calcConstants = _parameters.CalculationConstants;
        var disabilityDegree = _parameters.DisabilityDegree;
        var isPensioner = _parameters.IsPensioner;

        return CalcTaxBaseOfGivenGrossSalary(
            yearParam,
            _minGrossWage,
            empTypeConst,
            calcConstants,
            _grossSalaryForTaxBaseCalculation,
            _workedDays,
            disabilityDegree,
            isPensioner);
    }

    public decimal CumulativeMinWageIncomeTaxBase()
    {
        return _previousMonthCalculation == null
            ? CalcMinWageIncomeTaxBase()
            : CalcMinWageIncomeTaxBase() + _previousMonthCalculation.CumulativeMinWageIncomeTaxBase();
    }

    private void DoMonthCalculation(decimal grossSalary)
    {
        var yearParam = _parameters.YearParameter;
        var empTypeConst = _parameters.EmployeeTypeConstant;
        var calcConstants = _parameters.CalculationConstants;
        var disabilityDegree = _parameters.DisabilityDegree;
        var isPensioner = _parameters.IsPensioner;
        var agiRate = (decimal)_parameters.AgiRate;
        var employeeEducationExemptionRate = _parameters.EmployeeEducationExemptionRate;
        var applyMinWageTaxExemption = _calculationOptions.ApplyMinWageTaxExemption;
        var isAgiCalculationEnabled = _calculationOptions.IsAgiCalculationEnabled;
        var applyEmployerDiscount5746 = _calculationOptions.ApplyEmployerDiscount5746;

        var cumulativeIncomeTaxBase = _previousMonthCalculation?.CumulativeIncomeTaxBase ?? 0m;
        var cumulativeMinWageIncomeTaxBase = _previousMonthCalculation?.CumulativeMinWageIncomeTaxBase() ?? 0m;

        _grossSalaryForTaxBaseCalculation = Math.Min(_minGrossWage.Amount, grossSalary);
        _calculatedGrossSalary = CalcGrossSalary(grossSalary, _workedDays, calcConstants.MonthDayCount);
        _sgkBase = CalcSgkBase(_minGrossWage, grossSalary, _workedDays, calcConstants.MonthDayCount);
        _employeeDisabledIncomeTaxBaseAmount = CalcDisabledIncomeTaxBaseAmount(yearParam, disabilityDegree, _workedDays, calcConstants.MonthDayCount);
        _employeeSgkDeduction = CalcEmployeeSgkDeduction(calcConstants, empTypeConst, _sgkBase, isPensioner);
        _employeeSgkExemptionAmount = CalcEmployeeSgkExemption(calcConstants, empTypeConst, _minGrossWage, _workedDays, _employeeSgkDeduction, isPensioner);
        _employeeUnemploymentInsuranceDeduction = CalcEmployeeUnemploymentInsuranceDeduction(calcConstants, empTypeConst, _sgkBase, isPensioner);
        _employeeUnemploymentInsuranceExemptionAmount = CalcEmployeeUnemploymentInsuranceExemption(calcConstants, empTypeConst, yearParam, _minGrossWage, _workedDays,
            _employeeUnemploymentInsuranceDeduction, isPensioner);
        _stampTax = CalcStampTax(calcConstants, empTypeConst, yearParam, _calculatedGrossSalary);
        _employeeStampTaxExemption = CalcEmployeeStampTaxExemption(calcConstants, empTypeConst, yearParam, _minGrossWage, _stampTax, applyMinWageTaxExemption);
        _employerStampTaxExemption = CalcEmployerStampTaxExemption(calcConstants, empTypeConst, yearParam, _minGrossWage, _stampTax, _workedDays, _researchAndDevelopmentWorkedDays,
            _employeeStampTaxExemption);
        _employerStampTax = empTypeConst.EmployerStampTaxApplicable
            ? _stampTax - _employerStampTaxExemption
            : 0;
        var (appliedSlices, taxValue) = CalcEmployeeIncomeTax(empTypeConst, yearParam, cumulativeSalary: cumulativeIncomeTaxBase, incomeTaxBase: IncomeTaxBase);
        _employeeIncomeTax = taxValue;
        _appliedTaxSlices = [.. appliedSlices];

        _employeeIncomeTaxExemptionAmount = CalcEmployeeIncomeTaxExemption(yearParam, _minGrossWage, empTypeConst, calcConstants, EmployeeIncomeTax, disabilityDegree,
            cumulativeMinWageIncomeTaxBase, applyMinWageTaxExemption);
        _agiAmount = CalcAgi(yearParam, _minGrossWage, empTypeConst, agiRate, _employeeIncomeTax, isAgiCalculationEnabled);
        _netSalary = CalcNetSalary(
            _calculatedGrossSalary,
            _employeeSgkDeduction,
            _employeeUnemploymentInsuranceDeduction,
            _stampTax,
            _employeeStampTaxExemption,
            _employerStampTaxExemption,
            _employeeIncomeTax,
            _employeeIncomeTaxExemptionAmount
        );

        _employerSgkDeduction = CalcEmployerSgkDeduction(calcConstants, empTypeConst, _sgkBase, isPensioner);
        _employerSgkExemptionAmount = CalcEmployerSgkExemption(calcConstants, empTypeConst, yearParam, _minGrossWage, _workedDays, _researchAndDevelopmentWorkedDays, _sgkBase,
            _employerSgkDeduction, applyEmployerDiscount5746, isPensioner);
        _employerUnemploymentInsuranceDeduction = CalcEmployerUnemploymentInsuranceDeduction(calcConstants, empTypeConst, _sgkBase, isPensioner);
        _employerUnemploymentInsuranceExemptionAmount =
            CalcEmployerUnemploymentInsuranceExemption(calcConstants, empTypeConst, _minGrossWage, _workedDays, _employerUnemploymentInsuranceDeduction, isPensioner);
        _employerIncomeTaxExemptionAmount = CalcEmployerIncomeTaxExemption(yearParam, _minGrossWage, empTypeConst, calcConstants, employeeEducationExemptionRate, _workedDays,
            _researchAndDevelopmentWorkedDays, _employeeIncomeTax, _agiAmount, _employeeIncomeTaxExemptionAmount, disabilityDegree, applyMinWageTaxExemption, isPensioner);
    }

    private decimal FindGrossFromNet(decimal desiredNet)
    {
        var monthDayCount = _parameters.CalculationConstants.MonthDayCount;
        var fractionFactor = _workedDays / (decimal)monthDayCount;
        desiredNet *= fractionFactor;

        var left = desiredNet / 30m;
        var right = desiredNet * 30m;
        var middle = (left + right) / 2m;

        const int itLimit = 100;

        for (var i = 0; i < itLimit; i++)
        {
            if (right < left)
            {
                return -1m;
            }

            DoMonthCalculation(middle);

            var calcNetSalary = _calculationOptions.IsAgiIncludedNet
                ? NetSalary + AgiAmount
                : NetSalary;

            var diff = Math.Abs(calcNetSalary - desiredNet);
            if (diff < 0.0001m)
            {
                return middle;
            }

            if (calcNetSalary > desiredNet)
            {
                right = middle;
            }
            else
            {
                left = middle;
            }

            middle = (left + right) / 2m;
        }

        return -1m;
    }

    private decimal FindGrossFromTotalCost(decimal desiredTotalCost)
    {
        decimal left = 0;
        var right = desiredTotalCost * 2;
        var middle = (left + right) / 2m;

        const int itLimit = 100;

        for (var i = 0; i < itLimit; i++)
        {
            if (right < left)
            {
                return -1m;
            }

            DoMonthCalculation(middle);

            var calcTotalCost = EmployerTotalCost;

            var diff = Math.Abs(calcTotalCost - desiredTotalCost);
            if (diff < 0.0001m)
            {
                // close enough
                return middle;
            }

            if (calcTotalCost > desiredTotalCost)
            {
                right = middle;
            }
            else
            {
                left = middle;
            }

            middle = (left + right) / 2m;
        }

        // Not found
        return -1m;
    }

    private MinGrossWage GetMinGrossWage()
    {
        var year = _parameters.YearParameter.Year;
        var month = _monthsOfYear.Number;

        foreach (var minWage in _parameters.YearParameter.MinGrossWages)
        {
            if (month >= minWage.StartMonth)
            {
                return minWage;
            }
        }

        throw new InvalidOperationException($"Month number '{month}' is not valid for year '{year}'");
    }

    private void ResetFields()
    {
        _grossSalaryForTaxBaseCalculation = 0;
        _calculatedGrossSalary = 0;
        _sgkBase = 0;
        _employeeSgkDeduction = 0;
        _employeeSgkExemptionAmount = 0;
        _employeeUnemploymentInsuranceDeduction = 0;
        _employeeUnemploymentInsuranceExemptionAmount = 0;
        _employeeIncomeTax = 0;
        _employeeIncomeTaxExemptionAmount = 0;
        _employeeDisabledIncomeTaxBaseAmount = 0;
        _stampTax = 0;
        _netSalary = 0;
        _agiAmount = 0;
        _employerSgkDeduction = 0;
        _employerSgkExemptionAmount = 0;
        _employerUnemploymentInsuranceDeduction = 0;
        _employerUnemploymentInsuranceExemptionAmount = 0;
        _employerStampTax = 0;
        _employerStampTaxExemption = 0;
        _employeeStampTaxExemption = 0;
        _employerIncomeTaxExemptionAmount = 0;
        _appliedTaxSlices.Clear();
    }

    public decimal CalculatedGrossSalary => _calculatedGrossSalary;
    public decimal SgkBase => _sgkBase;
    public decimal EmployeeSgkDeduction => _employeeSgkDeduction;
    public decimal EmployeeSgkExemption => _employeeSgkExemptionAmount;
    public decimal EmployeeFinalSgkDeduction => EmployeeSgkDeduction - EmployeeSgkExemption;
    public decimal EmployeeUnemploymentInsuranceDeduction => _employeeUnemploymentInsuranceDeduction;
    public decimal EmployeeUnemploymentInsuranceExemption => _employeeUnemploymentInsuranceExemptionAmount;
    public decimal EmployeeFinalUnemploymentInsuranceDeduction => EmployeeUnemploymentInsuranceDeduction - EmployeeUnemploymentInsuranceExemption;
    public decimal EmployeeIncomeTax => _employeeIncomeTax;
    public decimal EmployeeIncomeTaxExemptionAmount => _employeeIncomeTaxExemptionAmount;
    public decimal StampTax => _stampTax;
    public decimal EmployeeStampTaxExemption => _employeeStampTaxExemption;
    public decimal EmployerStampTaxExemption => _employerStampTaxExemption;
    public decimal EmployerStampTax => _employerStampTax;
    public decimal EmployerSgkDeduction => _employerSgkDeduction;
    public decimal EmployerSgkExemption => _employerSgkExemptionAmount;
    public decimal EmployerFinalSgkDeduction => EmployerSgkDeduction - EmployerSgkExemption;
    public decimal EmployerUnemploymentInsuranceDeduction => _employerUnemploymentInsuranceDeduction;
    public decimal EmployerUnemploymentInsuranceExemption => _employerUnemploymentInsuranceExemptionAmount;
    public decimal EmployerFinalUnemploymentInsuranceDeduction => EmployerUnemploymentInsuranceDeduction - EmployerUnemploymentInsuranceExemption;
    public decimal AgiAmount => _agiAmount;
    public decimal EmployerIncomeTaxExemptionAmount => _employerIncomeTaxExemptionAmount;
    public decimal NetSalary => _netSalary;
    public decimal TotalStampTaxExemption => EmployerStampTaxExemption + EmployeeStampTaxExemption;
    public decimal TotalSgkExemption => EmployerSgkExemption + EmployerUnemploymentInsuranceExemption + EmployeeSgkExemption + EmployeeUnemploymentInsuranceExemption;
    public decimal FinalNetSalary => NetSalary + AgiAmount;

    public decimal IncomeTaxBase
        => CalcIncomeTaxBase(
            _calculatedGrossSalary,
            _employeeSgkDeduction,
            _employeeUnemploymentInsuranceDeduction,
            _employeeDisabledIncomeTaxBaseAmount);

    public decimal CumulativeSalary
        => _previousMonthCalculation != null
            ? _calculatedGrossSalary + _previousMonthCalculation.CumulativeSalary
            : _calculatedGrossSalary;

    public decimal CumulativeIncomeTaxBase
        => _previousMonthCalculation != null
            ? IncomeTaxBase + _previousMonthCalculation.CumulativeIncomeTaxBase
            : IncomeTaxBase;

    public decimal EmployerFinalIncomeTax
        => _employeeIncomeTax
           - _employerIncomeTaxExemptionAmount
           - _employeeIncomeTaxExemptionAmount
           - _agiAmount;

    public decimal EmployerTotalCost
        => NetSalary
           + AgiAmount
           + EmployerTotalSgkCost
           + EmployerStampTax
           + EmployerFinalIncomeTax;

    public decimal EmployerTotalSgkCost
        => EmployeeFinalSgkDeduction
           + EmployeeFinalUnemploymentInsuranceDeduction
           + EmployerFinalSgkDeduction
           + EmployerFinalUnemploymentInsuranceDeduction;

    public decimal EmployeeMinWageTaxExemptionAmount => _employeeIncomeTaxExemptionAmount + _employeeStampTaxExemption;

    public decimal WorkedDays => _workedDays;
    public decimal ResearchAndDevelopmentWorkedDays => _researchAndDevelopmentWorkedDays;
    public IImmutableList<TaxSlice> AppliedTaxSlices => _appliedTaxSlices.ToImmutableList();
}
