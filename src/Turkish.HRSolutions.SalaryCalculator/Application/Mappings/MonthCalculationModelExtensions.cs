using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Mappings;

/// <summary>
/// Extension methods for mapping <see cref="MonthCalculationModel"/> to API responses.
/// </summary>
internal static class MonthCalculationModelExtensions
{
#pragma warning disable CA1034 // False positive https://github.com/dotnet/sdk/issues/51681
    /// <summary>
    /// Provides extension methods for the <see cref="MonthCalculationModel"/> class to enable mapping- and transformation-
    /// specific to application requirements.
    /// </summary>
    extension(MonthCalculationModel model)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Maps a <see cref="MonthCalculationModel"/> to a <see cref="MonthlyBreakdown"/> snapshot.
        /// </summary>
        /// <param name="month">The month this calculation represents.</param>
        /// <returns>An immutable snapshot of the monthly calculation.</returns>
        public MonthlyBreakdown ToBreakdown(MonthsOfYear month)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new MonthlyBreakdown
            {
                // Month Identification
                Month = month,
                WorkedDays = (int)model.WorkedDays,
                ResearchAndDevelopmentWorkedDays = (int)model.ResearchAndDevelopmentWorkedDays,

                // Gross and Net Salary
                CalculatedGrossSalary = model.CalculatedGrossSalary,
                SgkBase = model.SgkBase,
                NetSalary = model.NetSalary,
                FinalNetSalary = model.FinalNetSalary,

                // Employee SGK
                EmployeeSgkDeduction = model.EmployeeSgkDeduction,
                EmployeeSgkExemption = model.EmployeeSgkExemption,
                EmployeeFinalSgkDeduction = model.EmployeeFinalSgkDeduction,

                // Employee Unemployment Insurance
                EmployeeUnemploymentInsuranceDeduction = model.EmployeeUnemploymentInsuranceDeduction,
                EmployeeUnemploymentInsuranceExemption = model.EmployeeUnemploymentInsuranceExemption,
                EmployeeFinalUnemploymentInsuranceDeduction = model.EmployeeFinalUnemploymentInsuranceDeduction,

                // Income Tax
                IncomeTaxBase = model.IncomeTaxBase,
                CumulativeIncomeTaxBase = model.CumulativeIncomeTaxBase,
                CumulativeSalary = model.CumulativeSalary,
                EmployeeIncomeTax = model.EmployeeIncomeTax,
                EmployeeIncomeTaxExemptionAmount = model.EmployeeIncomeTaxExemptionAmount,
                EmployeeMinWageTaxExemptionAmount = model.EmployeeMinWageTaxExemptionAmount,

                // Stamp Tax
                StampTax = model.StampTax,
                EmployeeStampTaxExemption = model.EmployeeStampTaxExemption,
                EmployerStampTax = model.EmployerStampTax,
                EmployerStampTaxExemption = model.EmployerStampTaxExemption,
                TotalStampTaxExemption = model.TotalStampTaxExemption,

                // AGI
                AgiAmount = model.AgiAmount,

                // Employer SGK
                EmployerSgkDeduction = model.EmployerSgkDeduction,
                EmployerSgkExemption = model.EmployerSgkExemption,
                EmployerFinalSgkDeduction = model.EmployerFinalSgkDeduction,

                // Employer Unemployment Insurance
                EmployerUnemploymentInsuranceDeduction = model.EmployerUnemploymentInsuranceDeduction,
                EmployerUnemploymentInsuranceExemption = model.EmployerUnemploymentInsuranceExemption,
                EmployerFinalUnemploymentInsuranceDeduction = model.EmployerFinalUnemploymentInsuranceDeduction,

                // Employer Income Tax
                EmployerIncomeTaxExemptionAmount = model.EmployerIncomeTaxExemptionAmount,
                EmployerFinalIncomeTax = model.EmployerFinalIncomeTax,

                // Totals
                TotalSgkExemption = model.TotalSgkExemption,
                EmployerTotalSgkCost = model.EmployerTotalSgkCost,
                EmployerTotalCost = model.EmployerTotalCost,

                // Applied Tax Slices
                AppliedTaxSlices = MapTaxSlices(model.AppliedTaxSlices),
            };
        }
    }

    private static IReadOnlyList<TaxSliceSnapshot> MapTaxSlices(IEnumerable<TaxSlice> taxSlices) => [.. taxSlices.Select(ts => ts.ToSnapshot())];
}
