using System.Diagnostics.CodeAnalysis;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Mappings;

/// <summary>
/// Extension methods for mapping <see cref="YearCalculationModel"/> to API responses.
/// </summary>
public static class YearCalculationModelExtensions
{
    private const string FirstHalf = "first";
    private const string SecondHalf = "second";

#pragma warning disable CA1034 // False positive https://github.com/dotnet/sdk/issues/51681
    /// <summary>
    /// Provides extension methods for mapping a <see cref="YearCalculationModel"/> to API response models.
    /// </summary>
    extension(YearCalculationModel model)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Maps a <see cref="YearCalculationModel"/> to a <see cref="YearlySalarySnapshot"/>.
        /// </summary>
        /// <param name="year">The calculation year.</param>
        /// <returns>An immutable snapshot of the yearly calculation.</returns>
        public YearlySalarySnapshot ToSnapshot(int year)
        {
            ArgumentNullException.ThrowIfNull(model);

            var monthlyBreakdowns = MapMonthlyBreakdowns(model);

            return CreateSnapshot(model, year, monthlyBreakdowns);
        }
    }

    private static IReadOnlyList<MonthlyBreakdown> MapMonthlyBreakdowns(YearCalculationModel model)
    {
        return
        [
            .. model.Months.Select((m, index) => m.ToBreakdown(MonthsOfYear.AllMonths[index])),
        ];
    }

    [SuppressMessage("Design", "MA0051:Method is too long")]
    private static YearlySalarySnapshot CreateSnapshot(YearCalculationModel model, int year, IReadOnlyList<MonthlyBreakdown> monthlyBreakdowns)
    {
        return new YearlySalarySnapshot
        {
            Year = year,
            MonthlyBreakdowns = monthlyBreakdowns,

            // Work Days
            TotalWorkDays = model.TotalWorkDays,
            AvgWorkDays = model.AvgWorkDays,
            TotalResearchAndDevelopmentWorkedDays = model.TotalResearchAndDevelopmentWorkedDays,
            AvgResearchAndDevelopmentWorkedDays = model.AvgResearchAndDevelopmentWorkedDays,

            // Gross Salary
            CalculatedGrossSalary = model.CalculatedGrossSalary,
            AvgCalculatedGrossSalary = model.AvgCalculatedGrossSalary,

            // Employee SGK & Unemployment
            EmployeeSgkDeduction = model.EmployeeSgkDeduction,
            AvgEmployeeSgkDeduction = model.AvgEmployeeSgkDeduction,
            EmployeeSgkExemption = model.EmployeeSgkExemption,
            AvgEmployeeSgkExemption = model.AvgEmployeeSgkExemption,
            EmployeeFinalSgkDeduction = model.EmployeeFinalSgkDeduction,
            AvgEmployeeFinalSgkDeduction = model.AvgEmployeeFinalSgkDeduction,
            EmployeeUnemploymentInsuranceDeduction = model.EmployeeUnemploymentInsuranceDeduction,
            AvgEmployeeUnemploymentInsuranceDeduction = model.AvgEmployeeUnemploymentInsuranceDeduction,
            EmployeeUnemploymentInsuranceExemption = model.EmployeeUnemploymentInsuranceExemption,
            AvgEmployeeUnemploymentInsuranceExemption = model.AvgEmployeeUnemploymentInsuranceExemption,

            // Income Tax & Stamp Tax
            EmployeeIncomeTax = model.EmployeeIncomeTax,
            AvgEmployeeIncomeTax = model.AvgEmployeeIncomeTax,
            EmployeeMinWageTaxExemptionAmount = model.EmployeeMinWageTaxExemptionAmount,
            AvgEmployeeMinWageTaxExemptionAmount = model.AvgEmployeeMinWageTaxExemptionAmount,
            StampTax = model.StampTax,
            AvgStampTax = model.AvgStampTax,
            EmployerStampTax = model.EmployerStampTax,
            AvgEmployerStampTax = model.AvgEmployerStampTax,
            EmployerStampTaxExemption = model.EmployerStampTaxExemption,
            AvgEmployerStampTaxExemption = model.AvgEmployerStampTaxExemption,
            TotalStampTaxExemption = model.TotalStampTaxExemption,
            AvgTotalStampTaxExemption = model.AvgTotalStampTaxExemption,

            // Net Salary & AGI
            NetSalary = model.NetSalary,
            AvgNetSalary = model.AvgNetSalary,
            AgiAmount = model.AgiAmount,
            AvgAgiAmount = model.AvgAgiAmount,
            FinalNetSalary = model.FinalNetSalary,
            AvgFinalNetSalary = model.AvgFinalNetSalary,

            // Employer SGK & Unemployment
            EmployerSgkDeduction = model.EmployerSgkDeduction,
            AvgEmployerSgkDeduction = model.AvgEmployerSgkDeduction,
            EmployerSgkExemption = model.EmployerSgkExemption,
            AvgEmployerSgkExemption = model.AvgEmployerSgkExemption,
            EmployerTotalSgkCost = model.EmployerTotalSgkCost,
            AvgEmployerTotalSgkCost = model.AvgEmployerTotalSgkCost,
            EmployerUnemploymentInsuranceDeduction = model.EmployerUnemploymentInsuranceDeduction,
            AvgEmployerUnemploymentInsuranceDeduction = model.AvgEmployerUnemploymentInsuranceDeduction,
            EmployerUnemploymentInsuranceExemption = model.EmployerUnemploymentInsuranceExemption,
            AvgEmployerUnemploymentInsuranceExemption = model.AvgEmployerUnemploymentInsuranceExemption,

            // Employer Income Tax & Totals
            EmployerFinalIncomeTax = model.EmployerFinalIncomeTax,
            AvgEmployerFinalIncomeTax = model.AvgEmployerFinalIncomeTax,
            EmployerIncomeTaxExemptionAmount = model.EmployerIncomeTaxExemptionAmount,
            AvgEmployerIncomeTaxExemptionAmount = model.AvgEmployerIncomeTaxExemptionAmount,
            TotalSgkExemption = model.TotalSgkExemption,
            AvgTotalSgkExemption = model.AvgTotalSgkExemption,
            EmployerTotalCost = model.EmployerTotalCost,
            AvgEmployerTotalCost = model.AvgEmployerTotalCost,

            // First Half-Semester (January - June)
            FirstHalfWorkedDays = model.GetSemesterWorkedDays(FirstHalf),
            FirstHalfEmployerTotalCost = model.EmployerHalfTotalCost(FirstHalf),
            FirstHalfEmployerAvgTotalCostSkipNonWorked = model.YearHalfEmployerAvgTotalCost(FirstHalf, skipNonWorkedMonths: true),
            FirstHalfEmployerAvgTotalCostIncludeNonWorked = model.YearHalfEmployerAvgTotalCost(FirstHalf, skipNonWorkedMonths: false),
            FirstHalfTubitakAvgCost = model.GetSemesterTubitakAvgCost(FirstHalf),

            // Second Half Semester (July - December)
            SecondHalfWorkedDays = model.GetSemesterWorkedDays(SecondHalf),
            SecondHalfEmployerTotalCost = model.EmployerHalfTotalCost(SecondHalf),
            SecondHalfEmployerAvgTotalCostSkipNonWorked = model.YearHalfEmployerAvgTotalCost(SecondHalf, skipNonWorkedMonths: true),
            SecondHalfEmployerAvgTotalCostIncludeNonWorked = model.YearHalfEmployerAvgTotalCost(SecondHalf, skipNonWorkedMonths: false),
            SecondHalfTubitakAvgCost = model.GetSemesterTubitakAvgCost(SecondHalf),
        };
    }
}
