using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Responses;

/// <summary>
/// Immutable snapshot of a single month's salary calculation.
/// </summary>
/// <remarks>
/// <para>
/// This is a pure data record that mirrors <see cref="Domain.Models.MonthCalculationModel"/>.
/// All values are captured at mapping time — no computed properties.
/// </para>
/// <para>
/// Property names match the domain model exactly for consistency.
/// </para>
/// </remarks>
public sealed record MonthlyBreakdown
{
    // ═══════════════════════════════════════════════════════════════
    // Month Identification
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// The month this breakdown represents.
    /// </summary>
    public required MonthsOfYear Month { get; init; }

    /// <summary>
    /// Number of days worked this month.
    /// </summary>
    public required int WorkedDays { get; init; }

    /// <summary>
    /// Number of R&amp;D days worked this month (for Teknokent/5746 employees).
    /// </summary>
    public required int ResearchAndDevelopmentWorkedDays { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Gross and Net Salary
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculated gross salary (may differ from input for partial months).
    /// </summary>
    public required decimal CalculatedGrossSalary { get; init; }

    /// <summary>
    /// SGK base amount used for social security calculations.
    /// </summary>
    public required decimal SgkBase { get; init; }

    /// <summary>
    /// Net salary after all deductions (before AGI).
    /// </summary>
    public required decimal NetSalary { get; init; }

    /// <summary>
    /// Final net salary including AGI (NetSalary + AgiAmount).
    /// </summary>
    public required decimal FinalNetSalary { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employee SGK (Social Security)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Employee SGK deduction (14% of SGK base).
    /// </summary>
    public required decimal EmployeeSgkDeduction { get; init; }

    /// <summary>
    /// Employee SGK exemption amount (for eligible employee types).
    /// </summary>
    public required decimal EmployeeSgkExemption { get; init; }

    /// <summary>
    /// Final employee SGK deduction after exemption.
    /// </summary>
    public required decimal EmployeeFinalSgkDeduction { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employee Unemployment Insurance
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Employee unemployment insurance deduction (1% of SGK base).
    /// </summary>
    public required decimal EmployeeUnemploymentInsuranceDeduction { get; init; }

    /// <summary>
    /// Employee unemployment insurance exemption amount.
    /// </summary>
    public required decimal EmployeeUnemploymentInsuranceExemption { get; init; }

    /// <summary>
    /// Final employee unemployment insurance deduction after exemption.
    /// </summary>
    public required decimal EmployeeFinalUnemploymentInsuranceDeduction { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Income Tax
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Income tax base (gross - SGK deductions - disability exemption).
    /// </summary>
    public required decimal IncomeTaxBase { get; init; }

    /// <summary>
    /// Cumulative income tax base from January to this month.
    /// </summary>
    public required decimal CumulativeIncomeTaxBase { get; init; }

    /// <summary>
    /// Cumulative gross salary from January to this month.
    /// </summary>
    public required decimal CumulativeSalary { get; init; }

    /// <summary>
    /// Employee income tax amount.
    /// </summary>
    public required decimal EmployeeIncomeTax { get; init; }

    /// <summary>
    /// Employee income tax exemption amount (min wage exemption for 2022+).
    /// </summary>
    public required decimal EmployeeIncomeTaxExemptionAmount { get; init; }

    /// <summary>
    /// Employee minimum wage tax exemption total (income tax + stamp tax exemption).
    /// </summary>
    public required decimal EmployeeMinWageTaxExemptionAmount { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Stamp Tax
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Stamp tax (damga vergisi).
    /// </summary>
    public required decimal StampTax { get; init; }

    /// <summary>
    /// Employee stamp tax exemption amount.
    /// </summary>
    public required decimal EmployeeStampTaxExemption { get; init; }

    /// <summary>
    /// Employer stamp tax amount.
    /// </summary>
    public required decimal EmployerStampTax { get; init; }

    /// <summary>
    /// Employer stamp tax exemption amount.
    /// </summary>
    public required decimal EmployerStampTaxExemption { get; init; }

    /// <summary>
    /// Total stamp tax exemption (employee + employer).
    /// </summary>
    public required decimal TotalStampTaxExemption { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // AGI (Minimum Living Allowance - Pre-2022)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// AGI (Asgari Geçim İndirimi) amount for pre-2022 calculations.
    /// </summary>
    public required decimal AgiAmount { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employer SGK (Social Security)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Employer SGK deduction (varies by employee type).
    /// </summary>
    public required decimal EmployerSgkDeduction { get; init; }

    /// <summary>
    /// Employer SGK exemption amount.
    /// </summary>
    public required decimal EmployerSgkExemption { get; init; }

    /// <summary>
    /// Final employer SGK deduction after exemption.
    /// </summary>
    public required decimal EmployerFinalSgkDeduction { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employer Unemployment Insurance
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Employer unemployment insurance deduction (2% of SGK base).
    /// </summary>
    public required decimal EmployerUnemploymentInsuranceDeduction { get; init; }

    /// <summary>
    /// Employer unemployment insurance exemption amount.
    /// </summary>
    public required decimal EmployerUnemploymentInsuranceExemption { get; init; }

    /// <summary>
    /// Final employer unemployment insurance deduction after exemption.
    /// </summary>
    public required decimal EmployerFinalUnemploymentInsuranceDeduction { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employer Income Tax
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Employer income tax exemption amount (for R&amp;D types).
    /// </summary>
    public required decimal EmployerIncomeTaxExemptionAmount { get; init; }

    /// <summary>
    /// Final employer income tax (after all exemptions and AGI).
    /// </summary>
    public required decimal EmployerFinalIncomeTax { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Totals
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total SGK exemption (all employee + employer SGK and unemployment exemptions).
    /// </summary>
    public required decimal TotalSgkExemption { get; init; }

    /// <summary>
    /// Total employer SGK cost (all SGK and unemployment after exemptions).
    /// </summary>
    public required decimal EmployerTotalSgkCost { get; init; }

    /// <summary>
    /// Total employer cost for this month.
    /// </summary>
    public required decimal EmployerTotalCost { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Applied Tax Slices
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Tax slices applied during income tax calculation for this month.
    /// </summary>
    public required IReadOnlyList<TaxSliceSnapshot> AppliedTaxSlices { get; init; }
}
