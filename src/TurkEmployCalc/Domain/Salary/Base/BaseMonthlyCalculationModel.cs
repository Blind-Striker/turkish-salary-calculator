#pragma warning disable IDE0060

namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.Base;

public abstract class BaseMonthlyCalculationModel
{
    protected BaseMonthlyCalculationModel(EmployeeMonthlyParameters employeeEmployeeMonthlyParameters, BaseMonthlyCalculationModel? previousMonthCalculationModel = null)
    {
        ArgumentNullException.ThrowIfNull(employeeEmployeeMonthlyParameters);

        YearParameter = employeeEmployeeMonthlyParameters.YearParameter;
        EmployeeTypeParameter = employeeEmployeeMonthlyParameters.EmployeeTypeParameter;
        MinGrossWageParameter = YearParameter.GetMinGrossWageParameter(employeeEmployeeMonthlyParameters.MonthlySalary.MonthsOfYear);
        CalculationConstants = employeeEmployeeMonthlyParameters.CalculationConstants;
        EmployeeMonthlyParameters = employeeEmployeeMonthlyParameters;
        PreviousMonthCalculationModel = previousMonthCalculationModel;
    }

    protected YearParameter YearParameter { get; }
    protected EmployeeTypeParameter EmployeeTypeParameter { get; }
    protected MinGrossWageParameter MinGrossWageParameter { get; }
    protected CalculationConstants CalculationConstants { get; }
    protected EmployeeMonthlyParameters EmployeeMonthlyParameters { get; }
    protected BaseMonthlyCalculationModel? PreviousMonthCalculationModel { get; }

    public IEnumerable<TaxSliceDto>? AppliedTaxSlices { get; private set; }

    public double AgiAmount { get; private set; }

    public double CalculatedGrossSalary { get; private set; }

    public double SgkBase { get; private set; }

    public double StampTax { get; private set; }

    public double EmployeeDisabledIncomeTaxBaseAmount { get; private set; }

    public double EmployeeSgkDeduction { get; private set; }

    public double EmployeeSgkExemptionAmount { get; private set; }

    public double EmployeeUnemploymentInsuranceDeduction { get; private set; }

    public double EmployeeUnemploymentInsuranceExemptionAmount { get; private set; }

    public double EmployeeIncomeTax { get; private set; }

    public double EmployeeIncomeTaxExemptionAmount { get; private set; }

    public double NetSalary { get; private set; }

    public double EmployerStampTax { get; private set; }

    public double EmployerIncomeTaxExemptionAmount { get; private set; }

    public double EmployerStampTaxExemption { get; private set; }

    public double EmployeeStampTaxExemption { get; private set; }

    public double EmployerSgkDeduction { get; private set; }

    public double EmployerSgkExemptionAmount { get; private set; }

    public double EmployerUnemploymentInsuranceDeduction { get; private set; }

    public double EmployerUnemploymentInsuranceExemptionAmount { get; private set; }

    public double IncomeTaxBase =>
        CalcIncomeTaxBase(CalculatedGrossSalary, EmployeeSgkDeduction, EmployeeUnemploymentInsuranceDeduction, EmployeeDisabledIncomeTaxBaseAmount);

    public double CumulativeIncomeTaxBase =>
        PreviousMonthCalculationModel != null
            ? IncomeTaxBase + PreviousMonthCalculationModel.CumulativeIncomeTaxBase
            : IncomeTaxBase;

    public double CumulativeMinWageIncomeTaxBase =>
        PreviousMonthCalculationModel != null
            ? CalcMinWageIncomeTaxBase() + PreviousMonthCalculationModel.CumulativeMinWageIncomeTaxBase
            : CalcMinWageIncomeTaxBase();

    public uint WorkedDay => EmployeeMonthlyParameters.MonthlySalary.WorkedDay;

    public double GrossSalaryForTaxBaseCalculation { get; private set; }

    public double EmployeeFinalSgkDeduction => EmployeeSgkDeduction - EmployeeSgkExemptionAmount;

    public double EmployeeFinalUnemploymentInsuranceDeduction => EmployeeUnemploymentInsuranceDeduction - EmployeeUnemploymentInsuranceExemptionAmount;

    public double EmployerTotalSgkCost =>
        EmployeeFinalSgkDeduction + EmployeeFinalUnemploymentInsuranceDeduction + EmployerSgkDeduction + EmployerUnemploymentInsuranceDeduction;

    public double EmployerFinalIncomeTax => EmployeeIncomeTax - EmployerIncomeTaxExemptionAmount - EmployeeIncomeTaxExemptionAmount - AgiAmount;

    public double EmployerTotalCost => NetSalary + AgiAmount + EmployerTotalSgkCost + EmployerStampTax + EmployerFinalIncomeTax;

    public double TotalSgkExemption => EmployerSgkExemptionAmount + EmployerUnemploymentInsuranceExemptionAmount + EmployeeSgkExemptionAmount +
                                       EmployeeUnemploymentInsuranceExemptionAmount;

    public static double CalcGrossSalary(double grossSalary, uint dayCount, uint monthDayCount)
    {
        return grossSalary * dayCount / monthDayCount;
    }

    public static double CalcIncomeTaxBase(double calculatedGrossSalary, double employeeSgkDeduction, double employeeUnemploymentInsuranceDeduction,
        double employeeDisabledIncomeTaxBaseAmount = 0)
    {
        return calculatedGrossSalary - (employeeSgkDeduction + employeeUnemploymentInsuranceDeduction + employeeDisabledIncomeTaxBaseAmount);
    }

    public static double CalcSgkBase(MinGrossWageParameter minGrossWage, double grossSalary, uint workedDays, uint monthDayCount)
    {
        ArgumentNullException.ThrowIfNull(minGrossWage);

        return Math.Min(grossSalary, minGrossWage.SgkCeil) * workedDays / monthDayCount;
    }

    public static double CalcDisabledIncomeTaxBaseAmount(YearParameter yearParams, int disabilityDegree, uint workedDays, uint monthDayCount)
    {
        ArgumentNullException.ThrowIfNull(yearParams);

        var disability = yearParams.DisabledMonthlyIncomeTaxDiscountBases.FirstOrDefault(option => option.Degree == disabilityDegree);

        return disability?.Amount * workedDays / monthDayCount ?? 0;
    }

    public static double CalcEmployeeSgkDeduction(EmployeeTypeParameter employeeType, CalculationConstants constants, double sgkBase, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.SgkApplicable)
        {
            return 0;
        }

        var rate = isPensioner ? constants.Employee.SgdpDeductionRate : constants.Employee.SgkDeductionRate;

        return sgkBase * rate;
    }

    public static double CalcTotalSgkRate(CalculationConstants constants)
    {
        ArgumentNullException.ThrowIfNull(constants);

        var employee = constants.Employee;
        var employer = constants.Employer;

        return employee.SgkDeductionRate + employee.UnemploymentInsuranceRate + employer.SgkDeductionRate + employer.UnemploymentInsuranceRate;
    }

    public static double CalcEmployeeSgkExemption(EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage, CalculationConstants constants,
        uint workedDays, double employeeSgkDeduction, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.SgkApplicable || isPensioner)
        {
            return 0;
        }

        var employeeSgkDeductionRate = constants.Employee.SgkDeductionRate;

        if (employeeType.SgkMinWageBasedExemption)
        {
            return minGrossWage.Amount * employeeSgkDeductionRate * workedDays / constants.MonthDayCount;
        }

        if (!employeeType.SgkMinWageBased17103Exemption)
        {
            return 0;
        }

        var totalSgkRate = CalcTotalSgkRate(constants);
        var exemption = minGrossWage.Amount / totalSgkRate * employeeSgkDeductionRate * workedDays / constants.MonthDayCount;
        return Math.Min(employeeSgkDeduction, exemption);
    }

    public static double CalcEmployeeUnemploymentInsuranceDeduction(EmployeeTypeParameter employeeType, CalculationConstants constants, double sgkBase,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.UnemploymentInsuranceApplicable)
        {
            return 0;
        }

        var rate = isPensioner ? constants.Employee.PensionerUnemploymentInsuranceRate : constants.Employee.UnemploymentInsuranceRate;

        return sgkBase * rate;
    }

    public static double CalcStampTax(YearParameter yearParams, EmployeeTypeParameter employeeType, CalculationConstants constants, double grossSalary)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        return !employeeType.StampTaxApplicable ? 0 : grossSalary * constants.StampTaxRate;
    }

    public static double CalcEmployerStampTaxExemption(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double stampTaxAmount, uint workedDays, uint researchAndDevelopmentWorkedDays, double employeeStampTaxExemptionAmount)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.StampTaxApplicable)
        {
            return stampTaxAmount;
        }

        if (employeeType.ResearchAndDevelopmentTaxExemption)
        {
            return Math.Max(stampTaxAmount - employeeStampTaxExemptionAmount, 0) * ((double)researchAndDevelopmentWorkedDays / workedDays);
        }

        if (yearParams.MinWageEmployeeTaxExemption || !employeeType.TaxMinWageBasedExemption)
        {
            return 0;
        }

        var grossSalary = CalcGrossSalary(minGrossWage.Amount, workedDays, constants.MonthDayCount);
        var exemption = CalcStampTax(yearParams, employeeType, constants, grossSalary);

        return exemption;
    }

    public static double CalcEmployeeStampTaxExemption(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double stampTaxAmount, bool applyMinWageTaxExemption)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        if (!applyMinWageTaxExemption || !yearParams.MinWageEmployeeTaxExemption)
        {
            return 0;
        }

        var minWageTaxBase = CalcGrossSalary(minGrossWage.Amount, constants.MonthDayCount, constants.MonthDayCount);
        var exemption = CalcStampTax(yearParams, employeeType, constants, minWageTaxBase);
        return Math.Min(stampTaxAmount, exemption);
    }

    public static double CalcEmployeeUnemploymentInsuranceExemption(YearParameter yearParams, EmployeeTypeParameter employeeType,
        MinGrossWageParameter minGrossWage, CalculationConstants constants, uint workedDays, double employeeUnemploymentInsuranceDeduction,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.UnemploymentInsuranceApplicable || isPensioner)
        {
            return 0;
        }

        if (employeeType.SgkMinWageBasedExemption)
        {
            return minGrossWage.Amount
                * constants.Employee.UnemploymentInsuranceRate
                * workedDays / constants.MonthDayCount;
        }

        if (!employeeType.SgkMinWageBased17103Exemption)
        {
            return 0;
        }

        var totalSgkRate = CalcTotalSgkRate(constants);
        var exemption = minGrossWage.Amount / totalSgkRate * constants.Employee.UnemploymentInsuranceRate * workedDays / constants.MonthDayCount;
        return Math.Min(employeeUnemploymentInsuranceDeduction, exemption);
    }

    public static TaxResultDto? CalcEmployeeIncomeTax(YearParameter yearParams, EmployeeTypeParameter employeeType, double cumulativeSalary,
        double incomeTaxBase)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);

        if (!employeeType.IncomeTaxApplicable)
        {
            return null;
        }

        var taxSlices = yearParams.TaxSlices.ToArray();
        var n = taxSlices.Length - 1;

        double totalTax = 0;
        IList<TaxSliceDto> taxSlicesDtos = [];
        for (var i = 0; i < n && incomeTaxBase > 0; i++)
        {
            var ceil = taxSlices[i].Ceil;

            if (!ceil.HasValue || !(cumulativeSalary < ceil))
            {
                continue;
            }

            if (cumulativeSalary + incomeTaxBase <= ceil)
            {
                totalTax += incomeTaxBase * taxSlices[i].Rate;
                incomeTaxBase = 0;
            }
            else
            {
                var excessAmount = cumulativeSalary + incomeTaxBase - ceil.Value;
                totalTax += (incomeTaxBase - excessAmount) * taxSlices[i].Rate;
                cumulativeSalary = ceil.Value;
                incomeTaxBase = excessAmount;
            }

            taxSlicesDtos.Add(new TaxSliceDto(taxSlices[i].Rate, taxSlices[i].Ceil));
        }

        if (incomeTaxBase <= 0)
        {
            return new TaxResultDto(taxSlicesDtos, totalTax);
        }

        taxSlicesDtos.Add(new TaxSliceDto(taxSlices[n].Rate, taxSlices[n].Ceil));
        totalTax += taxSlices[n].Rate * incomeTaxBase;

        return new TaxResultDto(taxSlicesDtos, totalTax);
    }

    public static double CalcNetSalary(YearParameter yearParams, double grossSalary, double sgkDeduction, double employeeUnemploymentInsuranceDeduction,
        double stampTax, double employeeStampTaxExemption, double employerStampTaxExemption, double employeeIncomeTax, double employeeIncomeTaxExemption)
    {
        ArgumentNullException.ThrowIfNull(yearParams);

        var stampTaxExemption = employeeStampTaxExemption;
        if (yearParams.MinWageEmployeeTaxExemption)
        {
            stampTaxExemption += employerStampTaxExemption;
        }

        return grossSalary + stampTaxExemption + employeeIncomeTaxExemption -
               (sgkDeduction + employeeUnemploymentInsuranceDeduction + stampTax + employeeIncomeTax);
    }

    public static double CalcEmployerSgkDeduction(EmployeeTypeParameter employeeType, CalculationConstants constants, double sgkBase, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.EmployerSgkApplicable)
        {
            return 0;
        }

        var rate = isPensioner ? constants.Employer.SgdpDeductionRate : constants.Employer.SgkDeductionRate;

        return sgkBase * rate;
    }

    public static double CalcEmployerSgkExemption(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double sgkBase, uint workedDays, uint researchAndDevelopmentWorkedDays, double employerSgkDeduction,
        bool applyEmployerDiscount5746 = false, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        double exemption = 0;

        if (!employeeType.EmployerSgkApplicable || isPensioner)
        {
            return exemption;
        }

        if (applyEmployerDiscount5746 && employeeType.EmployerSgkDiscount5746Applicable)
        {
            exemption += sgkBase * constants.Employer.EmployerDiscount5746;
        }

        if (employeeType.Employer5746AdditionalDiscountApplicable)
        {
            if (applyEmployerDiscount5746 && employeeType.EmployerSgkDiscount5746Applicable)
            {
                exemption += sgkBase
                             * (researchAndDevelopmentWorkedDays / (double)workedDays)
                             * (constants.Employer.SgkDeductionRate - constants.Employer.EmployerDiscount5746)
                             * constants.Employer.Sgk5746AdditionalDiscount;
            }
            else
            {
                exemption += sgkBase
                             * (researchAndDevelopmentWorkedDays / (double)workedDays)
                             * constants.Employer.SgkDeductionRate
                             * constants.Employer.Sgk5746AdditionalDiscount;
            }
        }
        else if (employeeType.SgkMinWageBasedExemption)
        {
            var minWageSgkBase = CalcGrossSalary(minGrossWage.Amount, workedDays, constants.MonthDayCount);
            exemption = CalcEmployerSgkDeduction(employeeType, constants, minWageSgkBase, isPensioner);
        }
        else if (employeeType.SgkMinWageBased17103Exemption)
        {
            var totalSgkRate = CalcTotalSgkRate(constants);
            var minWageSgkBase = CalcGrossSalary(minGrossWage.Amount / totalSgkRate, workedDays, constants.MonthDayCount);
            exemption = CalcEmployerSgkDeduction(employeeType, constants, minWageSgkBase, isPensioner);
            exemption = Math.Min(employerSgkDeduction, exemption);
        }
        else if (employeeType.EmployerSgkShareTotalExemption)
        {
            exemption += employerSgkDeduction;
        }

        return exemption;
    }

    public static double CalcEmployerUnemploymentInsuranceDeduction(EmployeeTypeParameter employeeType, CalculationConstants constants, double sgkBase,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.EmployerUnemploymentInsuranceApplicable)
        {
            return 0;
        }

        var rate = isPensioner ? constants.Employer.PensionerUnemploymentInsuranceRate : constants.Employer.UnemploymentInsuranceRate;
        return sgkBase * rate;
    }

    public static double CalcEmployerUnemploymentInsuranceExemption(EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double sgkBase, uint workedDays, double employerUnemploymentInsuranceDeduction, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.EmployerUnemploymentInsuranceApplicable || isPensioner)
        {
            return 0;
        }

        if (employeeType.SgkMinWageBasedExemption)
        {
            return minGrossWage.Amount * (workedDays / (double)constants.MonthDayCount) * constants.Employer.UnemploymentInsuranceRate;
        }

        if (!employeeType.SgkMinWageBased17103Exemption)
        {
            return 0;
        }

        var totalSgkRate = CalcTotalSgkRate(constants);
        var minWageSgkBase = CalcGrossSalary(minGrossWage.Amount / totalSgkRate, workedDays, constants.MonthDayCount);

        var exemption = CalcEmployerUnemploymentInsuranceDeduction(employeeType, constants, minWageSgkBase, isPensioner);
        return Math.Min(employerUnemploymentInsuranceDeduction, exemption);
    }

    public static double CalcAgi(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage, double agiRate,
        double employeeTax, bool isAgiCalculationEnabled)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);

        if (!isAgiCalculationEnabled || !employeeType.AgiApplicable || yearParams.MinWageEmployeeTaxExemption)
        {
            return 0;
        }

        var agi = minGrossWage.Amount * agiRate * yearParams.TaxSlices.ToArray()[0].Rate;

        return Math.Min(agi, employeeTax);
    }

    public static double CalcEmployeeIncomeTaxExemption(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double employeeIncomeTax, int disabilityDegree, double cumulativeIncomeTaxBase, bool applyMinWageTaxExemption)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        if (!applyMinWageTaxExemption || !yearParams.MinWageEmployeeTaxExemption)
        {
            return 0;
        }

        if (employeeIncomeTax <= 0)
        {
            return 0;
        }

        var minWageBasedIncomeTax = CalcEmployeeIncomeTaxOfTheGivenGrossSalary(
            yearParams,
            employeeType,
            minGrossWage,
            constants,
            minGrossWage.Amount,
            constants.MonthDayCount,
            disabilityDegree,
            cumulativeIncomeTaxBase);

        var taxAmount = minWageBasedIncomeTax?.TaxAmount ?? 0;
        var exemption = Math.Min(employeeIncomeTax, taxAmount);

        return exemption;
    }

    public static TaxResultDto? CalcEmployeeIncomeTaxOfTheGivenGrossSalary(YearParameter yearParams, EmployeeTypeParameter employeeType,
        MinGrossWageParameter minGrossWage, CalculationConstants constants, double grossSalary, uint workedDays, int disabilityDegree,
        double cumulativeIncomeTaxBase = 0, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        var incomeTaxBase =
            CalcTaxBaseOfGivenGrossSalary(yearParams, employeeType, minGrossWage, constants, grossSalary, workedDays, disabilityDegree, isPensioner);

        return CalcEmployeeIncomeTax(yearParams, employeeType, cumulativeIncomeTaxBase, incomeTaxBase);
    }

    public static double CalcEmployerIncomeTaxExemption(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double employeeEduExemptionRate, uint workedDays, uint researchAndDevelopmentWorkedDays,
        double employeeIncomeTax, double agiAmount, double employeeIncomeTaxExemption, int disabilityDegree, bool applyMinWageTaxExemption,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.EmployerIncomeTaxApplicable)
        {
            return employeeIncomeTax;
        }

        if (employeeType.EmployerEducationIncomeTaxExemption)
        {
            return employeeEduExemptionRate * (employeeIncomeTax - agiAmount - employeeIncomeTaxExemption) *
                   ((double)researchAndDevelopmentWorkedDays / workedDays);
        }

        if (employeeType.ResearchAndDevelopmentTaxExemption)
        {
            return (employeeIncomeTax - agiAmount - employeeIncomeTaxExemption) * ((double)researchAndDevelopmentWorkedDays / workedDays);
        }

        if (!applyMinWageTaxExemption || !employeeType.TaxMinWageBasedExemption || yearParams.MinWageEmployeeTaxExemption)
        {
            return 0;
        }

        if (employeeIncomeTax - agiAmount <= 0)
        {
            return 0;
        }

        var minWageBasedIncomeTax = CalcEmployeeIncomeTaxOfTheGivenGrossSalary(yearParams, employeeType, minGrossWage, constants, minGrossWage.Amount,
            workedDays, disabilityDegree, 0, isPensioner);

        var taxAmount = minWageBasedIncomeTax?.TaxAmount ?? 0;
        var minWageExemption = Math.Max(taxAmount - agiAmount, 0);

        return Math.Min(employeeIncomeTax - agiAmount, minWageExemption);
    }

    public static double CalcTaxBaseOfGivenGrossSalary(YearParameter yearParams, EmployeeTypeParameter employeeType, MinGrossWageParameter minGrossWage,
        CalculationConstants constants, double grossSalary, uint workedDays, int disabilityDegree, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(yearParams);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(constants);

        var taxBase = CalcGrossSalary(grossSalary, workedDays, constants.MonthDayCount);
        var sgkBase = CalcSgkBase(minGrossWage, grossSalary, workedDays, constants.MonthDayCount);

        var employeeSgkDeduction = CalcEmployeeSgkDeduction(employeeType, constants, sgkBase, isPensioner);
        var employeeUnemploymentInsuranceDeduction = CalcEmployeeUnemploymentInsuranceDeduction(employeeType, constants, sgkBase, isPensioner);
        var employeeDisabledIncomeTaxBaseAmount = CalcDisabledIncomeTaxBaseAmount(yearParams, disabilityDegree, workedDays, constants.MonthDayCount);

        var calcIncomeTaxBase = CalcIncomeTaxBase(taxBase, employeeSgkDeduction, employeeUnemploymentInsuranceDeduction, employeeDisabledIncomeTaxBaseAmount);

        return calcIncomeTaxBase;
    }

    public static double CalcEmployerStampTax(EmployeeTypeParameter employeeType, double stampTax, double stampTaxExemption)
    {
        ArgumentNullException.ThrowIfNull(employeeType);

        return employeeType.EmployerStampTaxApplicable ? stampTax - stampTaxExemption : 0;
    }

    public double CalcMinWageIncomeTaxBase()
    {
        var monthlySalary = EmployeeMonthlyParameters.MonthlySalary;

        return CalcTaxBaseOfGivenGrossSalary(monthlySalary.SalaryAmount, monthlySalary.WorkedDay, EmployeeMonthlyParameters.DisabilityDegree,
            EmployeeMonthlyParameters.IsPensioner);
    }

    protected double CalcSgkBase(double grossSalary, uint workedDays, uint monthDayCount)
    {
        return CalcSgkBase(MinGrossWageParameter, grossSalary, workedDays, monthDayCount);
    }

    protected double CalcDisabledIncomeTaxBaseAmount(int disabilityDegree, uint workedDays, uint monthDayCount)
    {
        return CalcDisabledIncomeTaxBaseAmount(YearParameter, disabilityDegree, workedDays, monthDayCount);
    }

    protected double CalcEmployeeSgkDeduction(double sgkBase, bool isPensioner = false)
    {
        return CalcEmployeeSgkDeduction(EmployeeTypeParameter, CalculationConstants, sgkBase, isPensioner);
    }

    protected double CalcEmployeeSgkExemption(uint workedDays, double employeeSgkDeduction, bool isPensioner = false)
    {
        return CalcEmployeeSgkExemption(EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, workedDays, employeeSgkDeduction, isPensioner);
    }

    protected double CalcEmployeeUnemploymentInsuranceDeduction(double sgkBase, bool isPensioner = false)
    {
        return CalcEmployeeUnemploymentInsuranceDeduction(EmployeeTypeParameter, CalculationConstants, sgkBase, isPensioner);
    }

    protected double CalcStampTax(double grossSalary)
    {
        return CalcStampTax(YearParameter, EmployeeTypeParameter, CalculationConstants, grossSalary);
    }

    protected double CalcEmployerStampTaxExemption(double stampTaxAmount, uint workedDays, uint researchAndDevelopmentWorkedDays,
        double employeeStampTaxExemptionAmount)
    {
        return CalcEmployerStampTaxExemption(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, stampTaxAmount, workedDays,
            researchAndDevelopmentWorkedDays, employeeStampTaxExemptionAmount);
    }

    protected double CalcEmployeeStampTaxExemption(double stampTaxAmount, bool applyMinWageTaxExemption)
    {
        return CalcEmployeeStampTaxExemption(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, stampTaxAmount,
            applyMinWageTaxExemption);
    }

    protected double CalcEmployeeUnemploymentInsuranceExemption(uint workedDays, double employeeUnemploymentInsuranceDeduction, bool isPensioner = false)
    {
        return CalcEmployeeUnemploymentInsuranceExemption(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, workedDays,
            employeeUnemploymentInsuranceDeduction, isPensioner);
    }

    protected TaxResultDto? CalcEmployeeIncomeTax(double cumulativeSalary, double incomeTaxBase)
    {
        return CalcEmployeeIncomeTax(YearParameter, EmployeeTypeParameter, cumulativeSalary, incomeTaxBase);
    }

    protected double CalcNetSalary(double grossSalary, double sgkDeduction, double employeeUnemploymentInsuranceDeduction, double stampTax,
        double employeeStampTaxExemption, double employerStampTaxExemption, double employeeIncomeTax, double employeeIncomeTaxExemption)
    {
        return CalcNetSalary(YearParameter, grossSalary, sgkDeduction, employeeUnemploymentInsuranceDeduction, stampTax, employeeStampTaxExemption,
            employerStampTaxExemption, employeeIncomeTax, employeeIncomeTaxExemption);
    }

    protected double CalcEmployerSgkDeduction(double sgkBase, bool isPensioner = false)
    {
        return CalcEmployerSgkDeduction(EmployeeTypeParameter, CalculationConstants, sgkBase, isPensioner);
    }

    protected double CalcEmployerSgkExemption(double sgkBase, uint workedDays, uint researchAndDevelopmentWorkedDays, double employerSgkDeduction,
        bool applyEmployerDiscount5746 = false, bool isPensioner = false)
    {
        return CalcEmployerSgkExemption(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, sgkBase, workedDays,
            researchAndDevelopmentWorkedDays, employerSgkDeduction, applyEmployerDiscount5746, isPensioner);
    }

    protected double CalcEmployerUnemploymentInsuranceDeduction(double sgkBase, bool isPensioner = false)
    {
        return CalcEmployerUnemploymentInsuranceDeduction(EmployeeTypeParameter, CalculationConstants, sgkBase, isPensioner);
    }

    protected double CalcEmployerUnemploymentInsuranceExemption(double sgkBase, uint workedDays, double employerUnemploymentInsuranceDeduction,
        bool isPensioner = false)
    {
        return CalcEmployerUnemploymentInsuranceExemption(EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, sgkBase, workedDays,
            employerUnemploymentInsuranceDeduction, isPensioner);
    }

    protected double CalcAgi(double agiRate, double employeeTax, bool isAgiCalculationEnabled)
    {
        return CalcAgi(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, agiRate, employeeTax, isAgiCalculationEnabled);
    }

    protected double CalcEmployeeIncomeTaxExemption(double employeeIncomeTax, int disabilityDegree, double cumulativeIncomeTaxBase,
        bool applyMinWageTaxExemption)
    {
        return CalcEmployeeIncomeTaxExemption(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, employeeIncomeTax,
            disabilityDegree, cumulativeIncomeTaxBase, applyMinWageTaxExemption);
    }

    protected TaxResultDto? CalcEmployeeIncomeTaxOfTheGivenGrossSalary(double grossSalary, uint workedDays, int disabilityDegree,
        double cumulativeIncomeTaxBase = 0, bool isPensioner = false)
    {
        return CalcEmployeeIncomeTaxOfTheGivenGrossSalary(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, grossSalary,
            workedDays, disabilityDegree, cumulativeIncomeTaxBase, isPensioner);
    }

    protected double CalcEmployerIncomeTaxExemption(double employeeEduExemptionRate, uint workedDays, uint researchAndDevelopmentWorkedDays,
        double employeeIncomeTax, double agiAmount, double employeeIncomeTaxExemption, int disabilityDegree, bool applyMinWageTaxExemption,
        bool isPensioner = false)
    {
        return CalcEmployerIncomeTaxExemption(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, employeeEduExemptionRate,
            workedDays, researchAndDevelopmentWorkedDays, employeeIncomeTax, agiAmount, employeeIncomeTaxExemption, disabilityDegree,
            applyMinWageTaxExemption, isPensioner);
    }

    protected double CalcTaxBaseOfGivenGrossSalary(double grossSalary, uint workedDays, int disabilityDegree, bool isPensioner = false)
    {
        return CalcTaxBaseOfGivenGrossSalary(YearParameter, EmployeeTypeParameter, MinGrossWageParameter, CalculationConstants, grossSalary, workedDays,
            disabilityDegree, isPensioner);
    }

    protected double CalcEmployerStampTax(double stampTax, double stampTaxExemption)
    {
        return CalcEmployerStampTax(EmployeeTypeParameter, stampTax, stampTaxExemption);
    }

    protected void InternalCalculate(bool applyEmployerDiscount5746, bool isAgiCalculationEnabled, bool isAgiIncludedTax, bool applyMinWageTaxExemption, double? salary = null)
    {
        var (_, salaryAmount, workedDay, researchAndDevelopmentWorkedDays) = EmployeeMonthlyParameters.MonthlySalary;

        if (salary.HasValue)
        {
            salaryAmount = salary.Value;
        }

        GrossSalaryForTaxBaseCalculation = Math.Min(MinGrossWageParameter.Amount, salaryAmount);
        var cumIncomeTaxBase = PreviousMonthCalculationModel?.CumulativeIncomeTaxBase ?? 0;
        var cumulativeMinWageIncomeTaxBase = PreviousMonthCalculationModel?.CumulativeMinWageIncomeTaxBase ?? 0;

        // Some calculations are dependent on others, so the order of execution matters
        CalculatedGrossSalary = CalcGrossSalary(salaryAmount, workedDay, CalculationConstants.MonthDayCount);
        SgkBase = CalcSgkBase(MinGrossWageParameter, salaryAmount, workedDay, CalculationConstants.MonthDayCount);

        EmployeeDisabledIncomeTaxBaseAmount = CalcDisabledIncomeTaxBaseAmount(EmployeeMonthlyParameters.DisabilityDegree, workedDay, CalculationConstants.MonthDayCount);

        EmployeeSgkDeduction = CalcEmployeeSgkDeduction(SgkBase, EmployeeMonthlyParameters.IsPensioner);
        EmployeeSgkExemptionAmount = CalcEmployeeSgkExemption(workedDay, EmployeeSgkDeduction, EmployeeMonthlyParameters.IsPensioner);
        EmployeeUnemploymentInsuranceDeduction = CalcEmployeeUnemploymentInsuranceDeduction(SgkBase, EmployeeMonthlyParameters.IsPensioner);

        EmployeeUnemploymentInsuranceExemptionAmount =
            CalcEmployeeUnemploymentInsuranceExemption(workedDay, EmployeeUnemploymentInsuranceDeduction, EmployeeMonthlyParameters.IsPensioner);

        StampTax = CalcStampTax(salaryAmount);
        EmployeeStampTaxExemption = CalcEmployeeStampTaxExemption(StampTax, applyMinWageTaxExemption);
        EmployerStampTaxExemption = CalcEmployerStampTaxExemption(StampTax, workedDay, researchAndDevelopmentWorkedDays,
            EmployeeStampTaxExemption);
        EmployerStampTax = CalcEmployerStampTax(StampTax, EmployeeStampTaxExemption);

        var incomeTaxResult = CalcEmployeeIncomeTax(cumIncomeTaxBase, IncomeTaxBase);
        EmployeeIncomeTax = incomeTaxResult?.TaxAmount ?? 0;
        AppliedTaxSlices = incomeTaxResult?.AppliedTaxSlices;

        EmployeeIncomeTaxExemptionAmount = CalcEmployeeIncomeTaxExemption(EmployeeIncomeTax, EmployeeMonthlyParameters.DisabilityDegree,
            cumulativeMinWageIncomeTaxBase, applyMinWageTaxExemption);

        AgiAmount = CalcAgi(EmployeeMonthlyParameters.MinimumLivingAllowanceRate, EmployeeIncomeTax, isAgiCalculationEnabled);

        NetSalary = CalcNetSalary(CalculatedGrossSalary, EmployeeSgkDeduction, EmployeeUnemploymentInsuranceDeduction, StampTax, EmployeeStampTaxExemption,
            EmployerStampTaxExemption, EmployeeIncomeTax, EmployeeIncomeTaxExemptionAmount);

        EmployerSgkDeduction = CalcEmployerSgkDeduction(SgkBase, EmployeeMonthlyParameters.IsPensioner);

        EmployerSgkExemptionAmount = CalcEmployerSgkExemption(SgkBase, workedDay, researchAndDevelopmentWorkedDays,
            EmployerSgkDeduction, applyEmployerDiscount5746, EmployeeMonthlyParameters.IsPensioner);

        EmployerUnemploymentInsuranceDeduction = CalcEmployerUnemploymentInsuranceDeduction(SgkBase, EmployeeMonthlyParameters.IsPensioner);

        EmployerUnemploymentInsuranceExemptionAmount = CalcEmployerUnemploymentInsuranceExemption(SgkBase, workedDay,
            EmployerUnemploymentInsuranceDeduction, EmployeeMonthlyParameters.IsPensioner);

        AgiAmount = CalcAgi(EmployeeMonthlyParameters.MinimumLivingAllowanceRate, EmployeeIncomeTax, isAgiCalculationEnabled);

        EmployerIncomeTaxExemptionAmount = CalcEmployerIncomeTaxExemption(EmployeeMonthlyParameters.EducationExemptionRate, workedDay,
            researchAndDevelopmentWorkedDays, EmployeeIncomeTax, AgiAmount, EmployeeIncomeTaxExemptionAmount,
            EmployeeMonthlyParameters.DisabilityDegree, applyMinWageTaxExemption, EmployeeMonthlyParameters.IsPensioner);
    }

    public abstract double Calculate(bool applyEmployerDiscount5746, bool isAgiCalculationEnabled, bool isAgiIncludedTax, bool applyMinWageTaxExemption);
}
