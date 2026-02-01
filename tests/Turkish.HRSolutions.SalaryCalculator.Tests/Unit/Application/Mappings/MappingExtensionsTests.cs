#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Mappings;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Mappings;

/// <summary>
/// Tests to verify that mapping from domain models to API response snapshots
/// preserves all values exactly.
/// </summary>
public class MappingExtensionsTests
{
    private static EmbeddedYearParameterProvider YearProvider { get; } = new();
    private static EmbeddedCalculationConstantsProvider ConstantsProvider { get; } = new();

    private static YearCalculationModel CreateAndCalculateYearModel(int year = 2024, decimal grossSalary = 50_000m, CalculationMode mode = CalculationMode.GrossToNet)
    {
        var yearParam = YearProvider.GetParameter(year)!;
        var employeeType = ConstantsProvider.GetEmployeeType(EmployeeTypeId.Standard)!;
        var monthlySalaries = CreateMonthlySalaries(grossSalary);

        var yearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParam,
            MonthlySalaries: monthlySalaries,
            EmployeeTypeConstant: employeeType,
            StandardEmployeeTypeConstant: employeeType,
            CalculationConstants: ConstantsProvider.Constants,
            EmployeeEducationExemptionRate: 0.0,
            AgiRate: 0.0,
            DisabilityDegree: 0,
            IsPensioner: false
        );

        var calcOptions = new CalculationOptions(
            ApplyMinWageTaxExemption: true,
            ApplyEmployerDiscount5746: false,
            IsAgiCalculationEnabled: false,
            IsAgiIncludedTax: false,
            IsAgiIncludedNet: false
        );

        var yearModel = new YearCalculationModel(yearlyParams, calcOptions);
        yearModel.Calculate(mode);

        return yearModel;
    }

    private static IEnumerable<MonthlySalary> CreateMonthlySalaries(decimal grossSalary, uint workedDays = 30)
    {
        return MonthsOfYear.AllMonths.Select(month =>
            new MonthlySalary(month, grossSalary, workedDays, 0)
        );
    }

    [Test]
    public async Task ToBreakdown_Should_Map_All_MonthCalculationModel_Properties_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        var monthModel = yearModel.Months[0]; // January
        var month = MonthsOfYear.January;

        var breakdown = monthModel.ToBreakdown(month);

        await Assert.That(breakdown.Month).IsEqualTo(month);
        await Assert.That(breakdown.WorkedDays).IsEqualTo((int)monthModel.WorkedDays);
        await Assert.That(breakdown.ResearchAndDevelopmentWorkedDays).IsEqualTo((int)monthModel.ResearchAndDevelopmentWorkedDays);

        await Assert.That(breakdown.CalculatedGrossSalary).IsEqualTo(monthModel.CalculatedGrossSalary);
        await Assert.That(breakdown.SgkBase).IsEqualTo(monthModel.SgkBase);
        await Assert.That(breakdown.NetSalary).IsEqualTo(monthModel.NetSalary);
        await Assert.That(breakdown.FinalNetSalary).IsEqualTo(monthModel.FinalNetSalary);

        await Assert.That(breakdown.EmployeeSgkDeduction).IsEqualTo(monthModel.EmployeeSgkDeduction);
        await Assert.That(breakdown.EmployeeSgkExemption).IsEqualTo(monthModel.EmployeeSgkExemption);
        await Assert.That(breakdown.EmployeeFinalSgkDeduction).IsEqualTo(monthModel.EmployeeFinalSgkDeduction);

        await Assert.That(breakdown.EmployeeUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployeeUnemploymentInsuranceDeduction);
        await Assert.That(breakdown.EmployeeUnemploymentInsuranceExemption).IsEqualTo(monthModel.EmployeeUnemploymentInsuranceExemption);
        await Assert.That(breakdown.EmployeeFinalUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployeeFinalUnemploymentInsuranceDeduction);

        await Assert.That(breakdown.IncomeTaxBase).IsEqualTo(monthModel.IncomeTaxBase);
        await Assert.That(breakdown.CumulativeIncomeTaxBase).IsEqualTo(monthModel.CumulativeIncomeTaxBase);
        await Assert.That(breakdown.CumulativeSalary).IsEqualTo(monthModel.CumulativeSalary);
        await Assert.That(breakdown.EmployeeIncomeTax).IsEqualTo(monthModel.EmployeeIncomeTax);
        await Assert.That(breakdown.EmployeeIncomeTaxExemptionAmount).IsEqualTo(monthModel.EmployeeIncomeTaxExemptionAmount);
        await Assert.That(breakdown.EmployeeMinWageTaxExemptionAmount).IsEqualTo(monthModel.EmployeeMinWageTaxExemptionAmount);

        await Assert.That(breakdown.StampTax).IsEqualTo(monthModel.StampTax);
        await Assert.That(breakdown.EmployeeStampTaxExemption).IsEqualTo(monthModel.EmployeeStampTaxExemption);
        await Assert.That(breakdown.EmployerStampTax).IsEqualTo(monthModel.EmployerStampTax);
        await Assert.That(breakdown.EmployerStampTaxExemption).IsEqualTo(monthModel.EmployerStampTaxExemption);
        await Assert.That(breakdown.TotalStampTaxExemption).IsEqualTo(monthModel.TotalStampTaxExemption);

        await Assert.That(breakdown.AgiAmount).IsEqualTo(monthModel.AgiAmount);

        await Assert.That(breakdown.EmployerSgkDeduction).IsEqualTo(monthModel.EmployerSgkDeduction);
        await Assert.That(breakdown.EmployerSgkExemption).IsEqualTo(monthModel.EmployerSgkExemption);
        await Assert.That(breakdown.EmployerFinalSgkDeduction).IsEqualTo(monthModel.EmployerFinalSgkDeduction);

        await Assert.That(breakdown.EmployerUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployerUnemploymentInsuranceDeduction);
        await Assert.That(breakdown.EmployerUnemploymentInsuranceExemption).IsEqualTo(monthModel.EmployerUnemploymentInsuranceExemption);
        await Assert.That(breakdown.EmployerFinalUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployerFinalUnemploymentInsuranceDeduction);

        await Assert.That(breakdown.EmployerIncomeTaxExemptionAmount).IsEqualTo(monthModel.EmployerIncomeTaxExemptionAmount);
        await Assert.That(breakdown.EmployerFinalIncomeTax).IsEqualTo(monthModel.EmployerFinalIncomeTax);

        await Assert.That(breakdown.TotalSgkExemption).IsEqualTo(monthModel.TotalSgkExemption);
        await Assert.That(breakdown.EmployerTotalSgkCost).IsEqualTo(monthModel.EmployerTotalSgkCost);
        await Assert.That(breakdown.EmployerTotalCost).IsEqualTo(monthModel.EmployerTotalCost);
    }

    [Test]
    public async Task ToBreakdown_Should_Map_AppliedTaxSlices_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel(grossSalary: 100_000m);
        var monthModel = yearModel.Months[11]; // December
        var month = MonthsOfYear.December;

        var breakdown = monthModel.ToBreakdown(month);

        await Assert.That(breakdown.AppliedTaxSlices.Count).IsEqualTo(monthModel.AppliedTaxSlices.Count);

        for (var i = 0; i < monthModel.AppliedTaxSlices.Count; i++)
        {
            var domainSlice = monthModel.AppliedTaxSlices[i];
            var snapshotSlice = breakdown.AppliedTaxSlices[i];

            await Assert.That(snapshotSlice.Rate).IsEqualTo(domainSlice.Rate);
            await Assert.That(snapshotSlice.Ceil).IsEqualTo(domainSlice.Ceil);
        }
    }

    [Test]
    public async Task ToBreakdown_Should_Map_All_Months_With_Correct_Month_Values()
    {
        var yearModel = CreateAndCalculateYearModel();

        for (var i = 0; i < 12; i++)
        {
            var monthModel = yearModel.Months[i];
            var expectedMonth = MonthsOfYear.AllMonths[i];
            var breakdown = monthModel.ToBreakdown(expectedMonth);

            await Assert.That(breakdown.Month).IsEqualTo(expectedMonth);
            await Assert.That(breakdown.Month.Number).IsEqualTo((short)(i + 1));
        }
    }

    [Test]
    public async Task ToSnapshot_Should_Map_Metadata_And_WorkDays_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.Year).IsEqualTo(year);
        await Assert.That(snapshot.MonthlyBreakdowns).Count().IsEqualTo(12);

        await Assert.That(snapshot.TotalWorkDays).IsEqualTo(yearModel.TotalWorkDays);
        await Assert.That(snapshot.AvgWorkDays).IsEqualTo(yearModel.AvgWorkDays);
        await Assert.That(snapshot.TotalResearchAndDevelopmentWorkedDays).IsEqualTo(yearModel.TotalResearchAndDevelopmentWorkedDays);
        await Assert.That(snapshot.AvgResearchAndDevelopmentWorkedDays).IsEqualTo(yearModel.AvgResearchAndDevelopmentWorkedDays);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_GrossAndEmployeeSgk_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.CalculatedGrossSalary).IsEqualTo(yearModel.CalculatedGrossSalary);
        await Assert.That(snapshot.AvgCalculatedGrossSalary).IsEqualTo(yearModel.AvgCalculatedGrossSalary);

        await Assert.That(snapshot.EmployeeSgkDeduction).IsEqualTo(yearModel.EmployeeSgkDeduction);
        await Assert.That(snapshot.AvgEmployeeSgkDeduction).IsEqualTo(yearModel.AvgEmployeeSgkDeduction);
        await Assert.That(snapshot.EmployeeSgkExemption).IsEqualTo(yearModel.EmployeeSgkExemption);
        await Assert.That(snapshot.AvgEmployeeSgkExemption).IsEqualTo(yearModel.AvgEmployeeSgkExemption);
        await Assert.That(snapshot.EmployeeFinalSgkDeduction).IsEqualTo(yearModel.EmployeeFinalSgkDeduction);
        await Assert.That(snapshot.AvgEmployeeFinalSgkDeduction).IsEqualTo(yearModel.AvgEmployeeFinalSgkDeduction);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_EmployeeUnemploymentAndIncomeTax_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.EmployeeUnemploymentInsuranceDeduction).IsEqualTo(yearModel.EmployeeUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.AvgEmployeeUnemploymentInsuranceDeduction).IsEqualTo(yearModel.AvgEmployeeUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.EmployeeUnemploymentInsuranceExemption).IsEqualTo(yearModel.EmployeeUnemploymentInsuranceExemption);
        await Assert.That(snapshot.AvgEmployeeUnemploymentInsuranceExemption).IsEqualTo(yearModel.AvgEmployeeUnemploymentInsuranceExemption);

        await Assert.That(snapshot.EmployeeIncomeTax).IsEqualTo(yearModel.EmployeeIncomeTax);
        await Assert.That(snapshot.AvgEmployeeIncomeTax).IsEqualTo(yearModel.AvgEmployeeIncomeTax);
        await Assert.That(snapshot.EmployeeMinWageTaxExemptionAmount).IsEqualTo(yearModel.EmployeeMinWageTaxExemptionAmount);
        await Assert.That(snapshot.AvgEmployeeMinWageTaxExemptionAmount).IsEqualTo(yearModel.AvgEmployeeMinWageTaxExemptionAmount);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_StampTaxAndNetSalary_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.StampTax).IsEqualTo(yearModel.StampTax);
        await Assert.That(snapshot.AvgStampTax).IsEqualTo(yearModel.AvgStampTax);
        await Assert.That(snapshot.EmployerStampTax).IsEqualTo(yearModel.EmployerStampTax);
        await Assert.That(snapshot.AvgEmployerStampTax).IsEqualTo(yearModel.AvgEmployerStampTax);
        await Assert.That(snapshot.EmployerStampTaxExemption).IsEqualTo(yearModel.EmployerStampTaxExemption);
        await Assert.That(snapshot.AvgEmployerStampTaxExemption).IsEqualTo(yearModel.AvgEmployerStampTaxExemption);
        await Assert.That(snapshot.TotalStampTaxExemption).IsEqualTo(yearModel.TotalStampTaxExemption);
        await Assert.That(snapshot.AvgTotalStampTaxExemption).IsEqualTo(yearModel.AvgTotalStampTaxExemption);

        await Assert.That(snapshot.NetSalary).IsEqualTo(yearModel.NetSalary);
        await Assert.That(snapshot.AvgNetSalary).IsEqualTo(yearModel.AvgNetSalary);
        await Assert.That(snapshot.AgiAmount).IsEqualTo(yearModel.AgiAmount);
        await Assert.That(snapshot.AvgAgiAmount).IsEqualTo(yearModel.AvgAgiAmount);
        await Assert.That(snapshot.FinalNetSalary).IsEqualTo(yearModel.FinalNetSalary);
        await Assert.That(snapshot.AvgFinalNetSalary).IsEqualTo(yearModel.AvgFinalNetSalary);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_EmployerSgkAndUnemployment_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.EmployerSgkDeduction).IsEqualTo(yearModel.EmployerSgkDeduction);
        await Assert.That(snapshot.AvgEmployerSgkDeduction).IsEqualTo(yearModel.AvgEmployerSgkDeduction);
        await Assert.That(snapshot.EmployerSgkExemption).IsEqualTo(yearModel.EmployerSgkExemption);
        await Assert.That(snapshot.AvgEmployerSgkExemption).IsEqualTo(yearModel.AvgEmployerSgkExemption);
        await Assert.That(snapshot.EmployerTotalSgkCost).IsEqualTo(yearModel.EmployerTotalSgkCost);
        await Assert.That(snapshot.AvgEmployerTotalSgkCost).IsEqualTo(yearModel.AvgEmployerTotalSgkCost);

        await Assert.That(snapshot.EmployerUnemploymentInsuranceDeduction).IsEqualTo(yearModel.EmployerUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.AvgEmployerUnemploymentInsuranceDeduction).IsEqualTo(yearModel.AvgEmployerUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.EmployerUnemploymentInsuranceExemption).IsEqualTo(yearModel.EmployerUnemploymentInsuranceExemption);
        await Assert.That(snapshot.AvgEmployerUnemploymentInsuranceExemption).IsEqualTo(yearModel.AvgEmployerUnemploymentInsuranceExemption);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_EmployerIncomeTaxAndTotals_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.EmployerFinalIncomeTax).IsEqualTo(yearModel.EmployerFinalIncomeTax);
        await Assert.That(snapshot.AvgEmployerFinalIncomeTax).IsEqualTo(yearModel.AvgEmployerFinalIncomeTax);
        await Assert.That(snapshot.EmployerIncomeTaxExemptionAmount).IsEqualTo(yearModel.EmployerIncomeTaxExemptionAmount);
        await Assert.That(snapshot.AvgEmployerIncomeTaxExemptionAmount).IsEqualTo(yearModel.AvgEmployerIncomeTaxExemptionAmount);

        await Assert.That(snapshot.TotalSgkExemption).IsEqualTo(yearModel.TotalSgkExemption);
        await Assert.That(snapshot.AvgTotalSgkExemption).IsEqualTo(yearModel.AvgTotalSgkExemption);
        await Assert.That(snapshot.EmployerTotalCost).IsEqualTo(yearModel.EmployerTotalCost);
        await Assert.That(snapshot.AvgEmployerTotalCost).IsEqualTo(yearModel.AvgEmployerTotalCost);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_Semester_Properties_Exactly()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.FirstHalfWorkedDays).IsEqualTo(yearModel.GetSemesterWorkedDays("first"));
        await Assert.That(snapshot.FirstHalfEmployerTotalCost).IsEqualTo(yearModel.EmployerHalfTotalCost("first"));
        await Assert.That(snapshot.FirstHalfEmployerAvgTotalCostSkipNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("first", skipNonWorkedMonths: true));
        await Assert.That(snapshot.FirstHalfEmployerAvgTotalCostIncludeNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("first", skipNonWorkedMonths: false));
        await Assert.That(snapshot.FirstHalfTubitakAvgCost).IsEqualTo(yearModel.GetSemesterTubitakAvgCost("first"));

        await Assert.That(snapshot.SecondHalfWorkedDays).IsEqualTo(yearModel.GetSemesterWorkedDays("second"));
        await Assert.That(snapshot.SecondHalfEmployerTotalCost).IsEqualTo(yearModel.EmployerHalfTotalCost("second"));
        await Assert.That(snapshot.SecondHalfEmployerAvgTotalCostSkipNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("second", skipNonWorkedMonths: true));
        await Assert.That(snapshot.SecondHalfEmployerAvgTotalCostIncludeNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("second", skipNonWorkedMonths: false));
        await Assert.That(snapshot.SecondHalfTubitakAvgCost).IsEqualTo(yearModel.GetSemesterTubitakAvgCost("second"));
    }

    [Test]
    public async Task ToSnapshot_Should_Map_Monthly_Breakdowns_In_Order()
    {
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        var snapshot = yearModel.ToSnapshot(year);

        await Assert.That(snapshot.MonthlyBreakdowns).Count().IsEqualTo(12);

        for (var i = 0; i < 12; i++)
        {
            var breakdown = snapshot.MonthlyBreakdowns[i];
            var expectedMonth = MonthsOfYear.AllMonths[i];

            await Assert.That(breakdown.Month).IsEqualTo(expectedMonth);
            await Assert.That(breakdown.CalculatedGrossSalary).IsEqualTo(yearModel.Months[i].CalculatedGrossSalary);
        }
    }

    [Test]
    public async Task ToSnapshot_Should_Map_TaxSlice_With_Ceil()
    {
        var taxSlice = new TaxSlice(0.15, 110_000m);

        var snapshot = taxSlice.ToSnapshot();

        await Assert.That(snapshot.Rate).IsEqualTo(0.15);
        await Assert.That(snapshot.Ceil).IsEqualTo(110_000m);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_TaxSlice_Without_Ceil()
    {
        var taxSlice = new TaxSlice(0.40, null);

        var snapshot = taxSlice.ToSnapshot();

        await Assert.That(snapshot.Rate).IsEqualTo(0.40);
        await Assert.That(snapshot.Ceil).IsNull();
    }

    [Test]
    public async Task ToBreakdown_Should_Throw_ArgumentNullException_When_Model_Is_Null()
    {
        MonthCalculationModel? model = null;

        await Assert.That(() => model!.ToBreakdown(MonthsOfYear.January))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ToSnapshot_Year_Should_Throw_ArgumentNullException_When_Model_Is_Null()
    {
        YearCalculationModel? model = null;

        await Assert.That(() => model!.ToSnapshot(2024))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ToSnapshot_Should_Handle_Partial_Year_With_Zero_Worked_Days_In_Some_Months()
    {
        var yearParam = YearProvider.GetParameter(2024)!;
        var employeeType = ConstantsProvider.GetEmployeeType(EmployeeTypeId.Standard)!;

        var monthlySalaries = MonthsOfYear.AllMonths.Select((month, index) =>
            new MonthlySalary(
                month,
                index < 6 ? 50_000m : 0m,
                index < 6 ? 30u : 0u,
                0
            )
        );

        var yearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParam,
            MonthlySalaries: monthlySalaries,
            EmployeeTypeConstant: employeeType,
            StandardEmployeeTypeConstant: employeeType,
            CalculationConstants: ConstantsProvider.Constants,
            EmployeeEducationExemptionRate: 0.0,
            AgiRate: 0.0,
            DisabilityDegree: 0,
            IsPensioner: false
        );

        var yearModel = new YearCalculationModel(yearlyParams, new CalculationOptions());
        yearModel.Calculate(CalculationMode.GrossToNet);

        var snapshot = yearModel.ToSnapshot(2024);

        await Assert.That(snapshot.SecondHalfWorkedDays).IsEqualTo(0);
        await Assert.That(snapshot.SecondHalfEmployerTotalCost).IsEqualTo(0m);

        await Assert.That(snapshot.FirstHalfWorkedDays).IsGreaterThan(0);
        await Assert.That(snapshot.FirstHalfEmployerTotalCost).IsGreaterThan(0m);
    }
}
