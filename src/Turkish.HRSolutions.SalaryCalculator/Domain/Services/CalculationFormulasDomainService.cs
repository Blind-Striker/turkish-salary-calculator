using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.Services;

public static class CalculationFormulasDomainService
{
    public static decimal CalcGrossSalary(decimal grossSalary, int dayCount, int monthDayCount)
    {
        return grossSalary * dayCount / monthDayCount;
    }

    public static decimal CalcIncomeTaxBase(
        decimal calculatedGrossSalary,
        decimal employeeSgkDeduction,
        decimal employeeUnemploymentInsuranceDeduction,
        decimal employeeDisabledIncomeTaxBaseAmount = 0)
    {
        return calculatedGrossSalary - (employeeSgkDeduction + employeeUnemploymentInsuranceDeduction + employeeDisabledIncomeTaxBaseAmount);
    }

    public static decimal CalcSgkBase(MinGrossWage minGrossWage, decimal grossSalary, int workedDays, int monthDayCount)
    {
        ArgumentNullException.ThrowIfNull(minGrossWage);

        return Math.Min(grossSalary, minGrossWage.SgkCeil) * workedDays / monthDayCount;
    }

    public static decimal CalcDisabledIncomeTaxBaseAmount(YearParameter yearParameter, int disabilityDegree, int workedDays, int monthDayCount)
    {
        ArgumentNullException.ThrowIfNull(yearParameter);

        var disability = yearParameter.DisabledMonthlyIncomeTaxDiscountBases.FirstOrDefault(option => option.Degree == disabilityDegree);
        decimal baseAmount = 0;
        if (disability != null)
        {
            baseAmount = disability.Amount * workedDays / monthDayCount;
        }

        return baseAmount;
    }

    public static decimal CalcEmployeeSgkDeduction(CalculationConstant constants, EmployeeTypeConstant employeeType, decimal sgkBase, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.SgkApplicable)
        {
            return 0;
        }

        var rate = isPensioner ? constants.Employee.SgdpDeductionRate : constants.Employee.SgkDeductionRate;

        return sgkBase * (decimal)rate;
    }

    public static decimal CalcEmployeeSgkExemption(
        CalculationConstant constants,
        EmployeeTypeConstant employeeType,
        MinGrossWage minGrossWage,
        int workedDays,
        decimal employeeSgkDeduction,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);

        decimal exemption = 0;
        if (!employeeType.SgkApplicable || isPensioner)
        {
            return exemption;
        }

        if (employeeType.SgkMinWageBasedExemption)
        {
            return minGrossWage.Amount * (decimal)constants.Employee.SgkDeductionRate * workedDays / constants.MonthDayCount;
        }

        if (!employeeType.SgkMinWageBased17103Exemption)
        {
            return exemption;
        }

        var totalSgkRate = CalcTotalSgkRate(constants);
        exemption = minGrossWage.Amount / totalSgkRate * (decimal)constants.Employee.SgkDeductionRate * workedDays / constants.MonthDayCount;
        return Math.Min(employeeSgkDeduction, exemption);
    }

    public static decimal CalcTotalSgkRate(CalculationConstant constants)
    {
        ArgumentNullException.ThrowIfNull(constants);

        return (decimal)(constants.Employee.SgkDeductionRate + constants.Employee.UnemploymentInsuranceRate + constants.Employer.SgkDeductionRate +
                         constants.Employer.UnemploymentInsuranceRate);
    }

    public static decimal CalcEmployeeUnemploymentInsuranceDeduction(CalculationConstant constants, EmployeeTypeConstant employeeType, decimal sgkBase, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);

        if (!employeeType.UnemploymentInsuranceApplicable)
        {
            return 0;
        }

        var rate = isPensioner
            ? (decimal)constants.Employee.PensionerUnemploymentInsuranceRate
            : (decimal)constants.Employee.UnemploymentInsuranceRate;

        return sgkBase * rate;
    }

    public static decimal CalcStampTax(CalculationConstant constants, EmployeeTypeConstant employeeType, YearParameter yearParameter, decimal grossSalary)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(yearParameter);

        return !employeeType.StampTaxApplicable ? 0 : grossSalary * (decimal)constants.StampTaxRate;
    }

    public static decimal CalcEmployerStampTaxExemption(
        CalculationConstant constants,
        EmployeeTypeConstant employeeType,
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        decimal stampTaxAmount,
        int workedDays,
        int researchAndDevelopmentWorkedDays,
        decimal employeeStampTaxExemptionAmount)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);

        if (!employeeType.StampTaxApplicable)
        {
            return stampTaxAmount;
        }

        if (employeeType.ResearchAndDevelopmentTaxExemption)
        {
            return Math.Max(stampTaxAmount - employeeStampTaxExemptionAmount, 0) * (researchAndDevelopmentWorkedDays / (decimal)workedDays);
        }

        if (yearParameter.MinWageEmployeeTaxExemption || !employeeType.TaxMinWageBasedExemption)
        {
            return 0m;
        }

        var taxBase = CalcGrossSalary(minGrossWage.Amount, workedDays, constants.MonthDayCount);
        return CalcStampTax(constants, employeeType, yearParameter, taxBase);
    }

    public static decimal CalcEmployeeStampTaxExemption(
        CalculationConstant constants,
        EmployeeTypeConstant employeeType,
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        decimal stampTaxAmount,
        bool applyMinWageTaxExemption)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);

        decimal exemption = 0;
        if (!applyMinWageTaxExemption || !yearParameter.MinWageEmployeeTaxExemption)
        {
            return exemption;
        }

        var minWageTaxBase = CalcGrossSalary(minGrossWage.Amount, constants.MonthDayCount, constants.MonthDayCount);
        exemption = CalcStampTax(constants, employeeType, yearParameter, minWageTaxBase);
        exemption = Math.Min(stampTaxAmount, exemption);

        return exemption;
    }

    public static decimal CalcEmployeeUnemploymentInsuranceExemption(
        CalculationConstant constants,
        EmployeeTypeConstant employeeType,
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        int workedDays,
        decimal employeeUnemploymentInsuranceDeduction,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);

        decimal exemption = 0;
        if (!employeeType.UnemploymentInsuranceApplicable || isPensioner)
        {
            return exemption;
        }

        if (employeeType.SgkMinWageBasedExemption)
        {
            return minGrossWage.Amount * (decimal)constants.Employee.UnemploymentInsuranceRate * workedDays / constants.MonthDayCount;
        }

        if (!employeeType.SgkMinWageBased17103Exemption)
        {
            return exemption;
        }

        var totalSgkRate = CalcTotalSgkRate(constants);
        exemption = minGrossWage.Amount / totalSgkRate * (decimal)constants.Employee.UnemploymentInsuranceRate * workedDays / constants.MonthDayCount;
        return Math.Min(employeeUnemploymentInsuranceDeduction, exemption);
    }

    public static (IImmutableList<TaxSlice> appliedTaxSlices, decimal tax) CalcEmployeeIncomeTax(
        EmployeeTypeConstant employeeType,
        YearParameter yearParameter,
        decimal cumulativeSalary,
        decimal incomeTaxBase)
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(employeeType);

        var tax = 0m;
        var appliedTaxSlices = new List<TaxSlice>();

        if (!employeeType.IncomeTaxApplicable)
        {
            return (appliedTaxSlices.ToImmutableList(), tax);
        }

        var n = yearParameter.TaxSlices.Count - 1;

        for (var i = 0; i < n && incomeTaxBase > 0; i++)
        {
            var ceil = yearParameter.TaxSlices[i].Ceil;
            if (!(cumulativeSalary < ceil))
            {
                continue;
            }

            if (cumulativeSalary + incomeTaxBase <= ceil)
            {
                tax += incomeTaxBase * (decimal)yearParameter.TaxSlices[i].Rate;
                incomeTaxBase = 0;
            }
            else
            {
                var excessAmount = cumulativeSalary + incomeTaxBase - ceil.Value;
                tax += (incomeTaxBase - excessAmount) * (decimal)yearParameter.TaxSlices[i].Rate;
                cumulativeSalary = ceil.Value;
                incomeTaxBase = excessAmount;
            }

            appliedTaxSlices.Add(yearParameter.TaxSlices[i]);
        }

        if (incomeTaxBase <= 0)
        {
            return (appliedTaxSlices.ToImmutableList(), tax);
        }

        appliedTaxSlices.Add(yearParameter.TaxSlices[n]);
        tax += (decimal)yearParameter.TaxSlices[n].Rate * incomeTaxBase;

        return (appliedTaxSlices.ToImmutableList(), tax);
    }

    public static decimal CalcNetSalary(
        decimal grossSalary,
        decimal sgkDeduction,
        decimal employeeUnemploymentInsuranceDeduction,
        decimal stampTax,
        decimal employeeStampTaxExemption,
        decimal employerStampTaxExemption,
        decimal employeeIncomeTax,
        decimal employeeIncomeTaxExemption
    )
    {
        var stampTaxExemption = employeeStampTaxExemption + employerStampTaxExemption;
        return grossSalary + stampTaxExemption + employeeIncomeTaxExemption - (sgkDeduction + employeeUnemploymentInsuranceDeduction + stampTax + employeeIncomeTax);
    }

    public static decimal CalcEmployerSgkDeduction(CalculationConstant constants, EmployeeTypeConstant employeeType, decimal sgkBase, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.EmployerSgkApplicable)
        {
            return 0;
        }

        var rate = isPensioner ? constants.Employer.SgdpDeductionRate : constants.Employer.SgkDeductionRate;

        return sgkBase * (decimal)rate;
    }

    public static decimal CalcEmployerSgkExemption(
        CalculationConstant constants,
        EmployeeTypeConstant employeeType,
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        int workedDays,
        int researchAndDevelopmentWorkedDays,
        decimal sgkBase,
        decimal employerSgkDeduction,
        bool applyEmployerDiscount5746,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        decimal exemption = 0;
        if (!employeeType.EmployerSgkApplicable || isPensioner)
        {
            return exemption;
        }

        if (applyEmployerDiscount5746 && employeeType.EmployerSgkDiscount5746Applicable)
        {
            exemption += sgkBase * (decimal)constants.Employer.EmployerDiscount5746;
        }

        if (employeeType.Employer5746AdditionalDiscountApplicable)
        {
            if (applyEmployerDiscount5746 && employeeType.EmployerSgkDiscount5746Applicable)
            {
                exemption += sgkBase
                             * (researchAndDevelopmentWorkedDays / (decimal)workedDays)
                             * (decimal)(constants.Employer.SgkDeductionRate - constants.Employer.EmployerDiscount5746)
                             * (decimal)constants.Employer.Sgk5746AdditionalDiscount;
            }
            else
            {
                exemption += sgkBase * (researchAndDevelopmentWorkedDays / (decimal)workedDays) *
                             (decimal)constants.Employer.SgkDeductionRate * (decimal)constants.Employer.Sgk5746AdditionalDiscount;
            }
        }
        else if (employeeType.SgkMinWageBasedExemption)
        {
            var minWageSgkBase = CalcGrossSalary(minGrossWage.Amount, workedDays, constants.MonthDayCount);
            exemption = CalcEmployerSgkDeduction(constants, employeeType, minWageSgkBase, isPensioner);
        }
        else if (employeeType.SgkMinWageBased17103Exemption)
        {
            var totalSgkRate = CalcTotalSgkRate(constants);
            var minWageSgkBase = CalcGrossSalary(minGrossWage.Amount / totalSgkRate, workedDays, constants.MonthDayCount);
            exemption = CalcEmployerSgkDeduction(constants, employeeType, minWageSgkBase, isPensioner);
            exemption = Math.Min(employerSgkDeduction, exemption);
        }
        else if (employeeType.EmployerSgkShareTotalExemption)
        {
            exemption += employerSgkDeduction;
        }

        return exemption;
    }

    public static decimal CalcEmployerUnemploymentInsuranceDeduction(CalculationConstant constants, EmployeeTypeConstant employeeType, decimal sgkBase, bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);

        if (!employeeType.EmployerUnemploymentInsuranceApplicable)
        {
            return 0;
        }

        var rate = isPensioner
            ? (decimal)constants.Employer.PensionerUnemploymentInsuranceRate
            : (decimal)constants.Employer.UnemploymentInsuranceRate;

        return sgkBase * rate;
    }

    public static decimal CalcEmployerUnemploymentInsuranceExemption(
        CalculationConstant constants,
        EmployeeTypeConstant employeeType,
        MinGrossWage minGrossWage,
        int workedDays,
        decimal employerUnemploymentInsuranceDeduction,
        bool isPensioner = false)
    {
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(minGrossWage);

        decimal exemption = 0;
        if (!employeeType.EmployerUnemploymentInsuranceApplicable || isPensioner)
        {
            return exemption;
        }

        if (employeeType.SgkMinWageBasedExemption)
        {
            return minGrossWage.Amount * (decimal)constants.Employer.UnemploymentInsuranceRate * workedDays / constants.MonthDayCount;
        }

        if (!employeeType.SgkMinWageBased17103Exemption)
        {
            return exemption;
        }

        var totalSgkRate = CalcTotalSgkRate(constants);
        exemption = minGrossWage.Amount / totalSgkRate * (decimal)constants.Employer.UnemploymentInsuranceRate * workedDays / constants.MonthDayCount;
        return Math.Min(employerUnemploymentInsuranceDeduction, exemption);
    }

    public static decimal CalcAgi(
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        EmployeeTypeConstant employeeType,
        decimal agiRate,
        decimal employeeTax,
        bool isAgiCalculationEnabled = false)
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);

        if (!isAgiCalculationEnabled || !employeeType.AgiApplicable || yearParameter.MinWageEmployeeTaxExemption)
        {
            return 0;
        }

        var agi = minGrossWage.Amount * agiRate * (decimal)yearParameter.TaxSlices[0].Rate;

        return Math.Min(agi, employeeTax);
    }

    public static decimal CalcEmployeeIncomeTaxExemption(
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        EmployeeTypeConstant employeeType,
        CalculationConstant constants,
        decimal employeeIncomeTax,
        int disabilityDegree,
        decimal cumulativeIncomeTaxBase = 0m, // default
        bool applyMinWageTaxExemption = false)
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        const decimal exemption = 0m;

        if (!applyMinWageTaxExemption || !yearParameter.MinWageEmployeeTaxExemption)
        {
            return exemption;
        }

        if (employeeIncomeTax <= 0)
        {
            return exemption;
        }

        // 1) We need to compute the "minWageBasedIncomeTax"
        var (_, minWageBasedTax) = CalcEmployeeIncomeTaxOfTheGivenGrossSalary(
            yearParameter,
            minGrossWage,
            employeeType,
            constants,
            grossSalary: minGrossWage.Amount, // TS uses "minGrossWage.amount"
            workedDays: constants.MonthDayCount,
            isPensioner: false, // TS snippet calls with "false"
            disabilityDegree: disabilityDegree,
            cumulativeIncomeTaxBase // pass the parameter
        );

        // Exemption is the lesser of "employeeIncomeTax" or that "minWageBasedTax"
        return Math.Min(employeeIncomeTax, minWageBasedTax);
    }

    public static decimal CalcEmployerIncomeTaxExemption(
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        EmployeeTypeConstant employeeType,
        CalculationConstant constants,
        double employeeEduExemptionRate,
        int workedDays,
        int researchAndDevelopmentWorkedDays,
        decimal employeeIncomeTax,
        decimal agiAmount,
        decimal employeeIncomeTaxExemption,
        int disabilityDegree,
        bool applyMinWageTaxExemption,
        bool isPensioner = false
    )
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        if (!employeeType.EmployerIncomeTaxApplicable)
        {
            return employeeIncomeTax;
        }

        if (employeeType.EmployerEducationIncomeTaxExemption)
        {
            var baseTaxAmount = employeeIncomeTax - agiAmount - employeeIncomeTaxExemption;
            return (decimal)employeeEduExemptionRate * baseTaxAmount * (researchAndDevelopmentWorkedDays / (decimal)workedDays);
        }

        if (employeeType.ResearchAndDevelopmentTaxExemption)
        {
            var baseTaxAmount = employeeIncomeTax - agiAmount - employeeIncomeTaxExemption;
            return baseTaxAmount * (researchAndDevelopmentWorkedDays / (decimal)workedDays);
        }

        if (!applyMinWageTaxExemption || !employeeType.TaxMinWageBasedExemption || yearParameter.MinWageEmployeeTaxExemption)
        {
            return 0m;
        }

        // We only do this if we actually have some leftover tax above AGI
        var leftoverTaxAfterAgi = employeeIncomeTax - agiAmount;

        if (leftoverTaxAfterAgi <= 0m)
        {
            return 0m;
        }

        var (_, minWageBasedIncomeTax) = CalcEmployeeIncomeTaxOfTheGivenGrossSalary(
            yearParameter,
            minGrossWage,
            employeeType,
            constants,
            grossSalary: minGrossWage.Amount,
            workedDays: workedDays,
            isPensioner,
            disabilityDegree
        );

        var minWageExemption = Math.Max(minWageBasedIncomeTax - agiAmount, 0m);
        return Math.Min(leftoverTaxAfterAgi, minWageExemption);
    }

    public static (IImmutableList<TaxSlice> appliedTaxSlices, decimal tax) CalcEmployeeIncomeTaxOfTheGivenGrossSalary(
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        EmployeeTypeConstant employeeType,
        CalculationConstant constants,
        decimal grossSalary,
        int workedDays,
        bool isPensioner,
        int disabilityDegree,
        decimal cumulativeIncomeTaxBase = 0m
    )
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        var incomeTaxBase = CalcTaxBaseOfGivenGrossSalary(yearParameter, minGrossWage, employeeType, constants, grossSalary, workedDays, disabilityDegree, isPensioner);
        var (appliedSlices, tax) = CalcEmployeeIncomeTax(employeeType, yearParameter, cumulativeIncomeTaxBase, incomeTaxBase);

        return (appliedSlices, tax);
    }

    public static decimal CalcTaxBaseOfGivenGrossSalary(
        YearParameter yearParameter,
        MinGrossWage minGrossWage,
        EmployeeTypeConstant employeeType,
        CalculationConstant constants,
        decimal grossSalary,
        int workedDays,
        int disabilityDegree,
        bool isPensioner = false
    )
    {
        ArgumentNullException.ThrowIfNull(yearParameter);
        ArgumentNullException.ThrowIfNull(minGrossWage);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(constants);

        var taxBase = CalcGrossSalary(grossSalary, workedDays, constants.MonthDayCount);
        var sgkBase = CalcSgkBase(minGrossWage, grossSalary, workedDays, constants.MonthDayCount);
        var employeeSgkDeduction = CalcEmployeeSgkDeduction(constants, employeeType, sgkBase, isPensioner);
        var employeeUnemploymentInsuranceDeduction = CalcEmployeeUnemploymentInsuranceDeduction(constants, employeeType, sgkBase, isPensioner);
        var employeeDisabledIncomeTaxBaseAmount = CalcDisabledIncomeTaxBaseAmount(yearParameter, disabilityDegree, workedDays, constants.MonthDayCount);

        // final: calcIncomeTaxBase
        return CalcIncomeTaxBase(taxBase, employeeSgkDeduction, employeeUnemploymentInsuranceDeduction, employeeDisabledIncomeTaxBaseAmount);
    }

    public static decimal CalcEmployerStampTax(EmployeeTypeConstant employeeType, decimal stampTax, decimal stampTaxExemption)
    {
        ArgumentNullException.ThrowIfNull(employeeType);

        return !employeeType.EmployerStampTaxApplicable ? 0m : stampTax - stampTaxExemption;
    }
}
