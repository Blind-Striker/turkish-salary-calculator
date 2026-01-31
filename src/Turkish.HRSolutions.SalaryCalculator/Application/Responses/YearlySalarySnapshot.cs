namespace Turkish.HRSolutions.SalaryCalculator.Application.Responses;

/// <summary>
/// Immutable snapshot of a complete year's salary calculation.
/// </summary>
/// <remarks>
/// <para>
/// This is a pure data record that mirrors <see cref="Domain.Models.YearCalculationModel"/>.
/// All values are captured at mapping time — no computed properties.
/// </para>
/// <para>
/// Property names match the domain model exactly for consistency.
/// </para>
/// <para>
/// Warnings are not included here — they belong in <c>Result&lt;YearlySalarySnapshot&gt;</c>
/// to maintain separation of concerns.
/// </para>
/// </remarks>
public sealed record YearlySalarySnapshot
{
    // ═══════════════════════════════════════════════════════════════
    // Metadata
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// The calculation year.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Detailed breakdown for each month.
    /// </summary>
    public required IReadOnlyList<MonthlyBreakdown> MonthlyBreakdowns { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Work Days (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total work days for the year.
    /// </summary>
    public required decimal TotalWorkDays { get; init; }

    /// <summary>
    /// Average work days per calculated month.
    /// </summary>
    public required decimal AvgWorkDays { get; init; }

    /// <summary>
    /// Total R&amp;D work days for the year.
    /// </summary>
    public required decimal TotalResearchAndDevelopmentWorkedDays { get; init; }

    /// <summary>
    /// Average R&amp;D work days per calculated month.
    /// </summary>
    public required decimal AvgResearchAndDevelopmentWorkedDays { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Gross Salary (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total calculated gross salary for the year.
    /// </summary>
    public required decimal CalculatedGrossSalary { get; init; }

    /// <summary>
    /// Average calculated gross salary per calculated month.
    /// </summary>
    public required decimal AvgCalculatedGrossSalary { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employee SGK (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total employee SGK deduction for the year.
    /// </summary>
    public required decimal EmployeeSgkDeduction { get; init; }

    /// <summary>
    /// Average employee SGK deduction per calculated month.
    /// </summary>
    public required decimal AvgEmployeeSgkDeduction { get; init; }

    /// <summary>
    /// Total employee SGK exemption for the year.
    /// </summary>
    public required decimal EmployeeSgkExemption { get; init; }

    /// <summary>
    /// Average employee SGK exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployeeSgkExemption { get; init; }

    /// <summary>
    /// Total final employee SGK deduction for the year.
    /// </summary>
    public required decimal EmployeeFinalSgkDeduction { get; init; }

    /// <summary>
    /// Average final employee SGK deduction per calculated month.
    /// </summary>
    public required decimal AvgEmployeeFinalSgkDeduction { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employee Unemployment Insurance (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total employee unemployment insurance deduction for the year.
    /// </summary>
    public required decimal EmployeeUnemploymentInsuranceDeduction { get; init; }

    /// <summary>
    /// Average employee unemployment insurance deduction per calculated month.
    /// </summary>
    public required decimal AvgEmployeeUnemploymentInsuranceDeduction { get; init; }

    /// <summary>
    /// Total employee unemployment insurance exemption for the year.
    /// </summary>
    public required decimal EmployeeUnemploymentInsuranceExemption { get; init; }

    /// <summary>
    /// Average employee unemployment insurance exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployeeUnemploymentInsuranceExemption { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Income Tax (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total employee income tax for the year.
    /// </summary>
    public required decimal EmployeeIncomeTax { get; init; }

    /// <summary>
    /// Average employee income tax per calculated month.
    /// </summary>
    public required decimal AvgEmployeeIncomeTax { get; init; }

    /// <summary>
    /// Total employee minimum wage tax exemption for the year.
    /// </summary>
    public required decimal EmployeeMinWageTaxExemptionAmount { get; init; }

    /// <summary>
    /// Average employee minimum wage tax exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployeeMinWageTaxExemptionAmount { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Stamp Tax (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total stamp tax for the year.
    /// </summary>
    public required decimal StampTax { get; init; }

    /// <summary>
    /// Average stamp tax per calculated month.
    /// </summary>
    public required decimal AvgStampTax { get; init; }

    /// <summary>
    /// Total employer stamp tax for the year.
    /// </summary>
    public required decimal EmployerStampTax { get; init; }

    /// <summary>
    /// Average employer stamp tax per calculated month.
    /// </summary>
    public required decimal AvgEmployerStampTax { get; init; }

    /// <summary>
    /// Total employer stamp tax exemption for the year.
    /// </summary>
    public required decimal EmployerStampTaxExemption { get; init; }

    /// <summary>
    /// Average employer stamp tax exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployerStampTaxExemption { get; init; }

    /// <summary>
    /// Total stamp tax exemption for the year.
    /// </summary>
    public required decimal TotalStampTaxExemption { get; init; }

    /// <summary>
    /// Average stamp tax exemption per calculated month.
    /// </summary>
    public required decimal AvgTotalStampTaxExemption { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Net Salary & AGI (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total net salary for the year.
    /// </summary>
    public required decimal NetSalary { get; init; }

    /// <summary>
    /// Average net salary per calculated month.
    /// </summary>
    public required decimal AvgNetSalary { get; init; }

    /// <summary>
    /// Total AGI amount for the year.
    /// </summary>
    public required decimal AgiAmount { get; init; }

    /// <summary>
    /// Average AGI amount per calculated month.
    /// </summary>
    public required decimal AvgAgiAmount { get; init; }

    /// <summary>
    /// Total final net salary for the year.
    /// </summary>
    public required decimal FinalNetSalary { get; init; }

    /// <summary>
    /// Average final net salary per calculated month.
    /// </summary>
    public required decimal AvgFinalNetSalary { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employer SGK (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total employer SGK deduction for the year.
    /// </summary>
    public required decimal EmployerSgkDeduction { get; init; }

    /// <summary>
    /// Average employer SGK deduction per calculated month.
    /// </summary>
    public required decimal AvgEmployerSgkDeduction { get; init; }

    /// <summary>
    /// Total employer SGK exemption for the year.
    /// </summary>
    public required decimal EmployerSgkExemption { get; init; }

    /// <summary>
    /// Average employer SGK exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployerSgkExemption { get; init; }

    /// <summary>
    /// Total employer SGK cost for the year.
    /// </summary>
    public required decimal EmployerTotalSgkCost { get; init; }

    /// <summary>
    /// Average employer SGK cost per calculated month.
    /// </summary>
    public required decimal AvgEmployerTotalSgkCost { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employer Unemployment Insurance (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total employer unemployment insurance deduction for the year.
    /// </summary>
    public required decimal EmployerUnemploymentInsuranceDeduction { get; init; }

    /// <summary>
    /// Average employer unemployment insurance deduction per calculated month.
    /// </summary>
    public required decimal AvgEmployerUnemploymentInsuranceDeduction { get; init; }

    /// <summary>
    /// Total employer unemployment insurance exemption for the year.
    /// </summary>
    public required decimal EmployerUnemploymentInsuranceExemption { get; init; }

    /// <summary>
    /// Average employer unemployment insurance exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployerUnemploymentInsuranceExemption { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Employer Income Tax (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total employer final income tax for the year.
    /// </summary>
    public required decimal EmployerFinalIncomeTax { get; init; }

    /// <summary>
    /// Average employer final income tax per calculated month.
    /// </summary>
    public required decimal AvgEmployerFinalIncomeTax { get; init; }

    /// <summary>
    /// Total employer income tax exemption for the year.
    /// </summary>
    public required decimal EmployerIncomeTaxExemptionAmount { get; init; }

    /// <summary>
    /// Average employer income tax exemption per calculated month.
    /// </summary>
    public required decimal AvgEmployerIncomeTaxExemptionAmount { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Overall Totals (Total & Average)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total SGK exemption for the year.
    /// </summary>
    public required decimal TotalSgkExemption { get; init; }

    /// <summary>
    /// Average SGK exemption per calculated month.
    /// </summary>
    public required decimal AvgTotalSgkExemption { get; init; }

    /// <summary>
    /// Total employer cost for the year.
    /// </summary>
    public required decimal EmployerTotalCost { get; init; }

    /// <summary>
    /// Average employer cost per calculated month.
    /// </summary>
    public required decimal AvgEmployerTotalCost { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // First Half (January - June) Semester Calculations
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total worked days in the first half (January-June).
    /// </summary>
    public required int FirstHalfWorkedDays { get; init; }

    /// <summary>
    /// Employer total cost for the first half.
    /// </summary>
    public required decimal FirstHalfEmployerTotalCost { get; init; }

    /// <summary>
    /// Average employer cost for first half (skipping non-worked months).
    /// </summary>
    public required decimal FirstHalfEmployerAvgTotalCostSkipNonWorked { get; init; }

    /// <summary>
    /// Average employer cost for first half (including non-worked months).
    /// </summary>
    public required decimal FirstHalfEmployerAvgTotalCostIncludeNonWorked { get; init; }

    /// <summary>
    /// TUBITAK average cost for first half (prorated for partial work).
    /// </summary>
    public required decimal FirstHalfTubitakAvgCost { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // Second Half (July - December) Semester Calculations
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Total worked days in the second half (July-December).
    /// </summary>
    public required int SecondHalfWorkedDays { get; init; }

    /// <summary>
    /// Employer total cost for the second half.
    /// </summary>
    public required decimal SecondHalfEmployerTotalCost { get; init; }

    /// <summary>
    /// Average employer cost for second half (skipping non-worked months).
    /// </summary>
    public required decimal SecondHalfEmployerAvgTotalCostSkipNonWorked { get; init; }

    /// <summary>
    /// Average employer cost for second half (including non-worked months).
    /// </summary>
    public required decimal SecondHalfEmployerAvgTotalCostIncludeNonWorked { get; init; }

    /// <summary>
    /// TUBITAK average cost for second half (prorated for partial work).
    /// </summary>
    public required decimal SecondHalfTubitakAvgCost { get; init; }
}
