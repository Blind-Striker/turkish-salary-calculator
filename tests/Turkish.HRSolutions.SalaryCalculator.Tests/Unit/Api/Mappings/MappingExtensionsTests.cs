using System.IO.Abstractions;
using Turkish.HRSolutions.SalaryCalculator.Api.Mappings;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Api.Mappings;

/// <summary>
/// Tests to verify that mapping from domain models to API response snapshots
/// preserves all values exactly.
/// </summary>
public class MappingExtensionsTests
{
    private ConstantParameters _constantParameters = null!;
    private YearParameters _yearParameters = null!;

    [Before(Test)]
    public async Task SetupAsync()
    {
        var fileSystem = new FileSystem();
        var assetsPath = FindAssetsPath(fileSystem);
        var parameterProvider = new JsonParameterProvider(fileSystem);

        var yearConstantsPath = fileSystem.Path.Combine(assetsPath, "year-constants.json");
        var calcConstantsPath = fileSystem.Path.Combine(assetsPath, "calculation-constants.json");

        var yearParametersResult = await parameterProvider.LoadYearParametersAsync(yearConstantsPath);
        var constantParametersResult = await parameterProvider.LoadFixtureAsync(calcConstantsPath);

        _yearParameters = yearParametersResult.Value!;
        _constantParameters = constantParametersResult.Value!;
    }

    private static string FindAssetsPath(FileSystem fileSystem)
    {
        var currentDir = fileSystem.Directory.GetCurrentDirectory();
        var searchDir = fileSystem.DirectoryInfo.New(currentDir);

        while (searchDir != null)
        {
            var assetsPath = fileSystem.Path.Combine(searchDir.FullName, "assets");
            if (fileSystem.Directory.Exists(assetsPath))
            {
                return assetsPath;
            }

            searchDir = searchDir.Parent;
        }

        throw new InvalidOperationException("Could not find assets directory");
    }

    private YearCalculationModel CreateAndCalculateYearModel(
        int year = 2024,
        decimal grossSalary = 50_000m,
        CalculationMode mode = CalculationMode.GrossToNet)
    {
        var yearParam = _yearParameters.Parameters.First(p => p.Year == year);
        var employeeType = _constantParameters.EmployeeTypeConstants.First(e => e.Id == 1); // Standard
        var monthlySalaries = CreateMonthlySalaries(grossSalary);

        var yearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParam,
            MonthlySalaries: monthlySalaries,
            EmployeeTypeConstant: employeeType,
            StandardEmployeeTypeConstant: employeeType,
            CalculationConstants: _constantParameters.CalculationConstants,
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

    // ═══════════════════════════════════════════════════════════════
    // MonthlyBreakdown Mapping Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToBreakdown_Should_Map_All_MonthCalculationModel_Properties_Exactly()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        var monthModel = yearModel.Months[0]; // January
        var month = MonthsOfYear.January;

        // Act
        var breakdown = monthModel.ToBreakdown(month);

        // Assert - Verify every property matches
        await Assert.That(breakdown.Month).IsEqualTo(month);
        await Assert.That(breakdown.WorkedDays).IsEqualTo((int)monthModel.WorkedDays);
        await Assert.That(breakdown.ResearchAndDevelopmentWorkedDays).IsEqualTo((int)monthModel.ResearchAndDevelopmentWorkedDays);

        // Gross and Net
        await Assert.That(breakdown.CalculatedGrossSalary).IsEqualTo(monthModel.CalculatedGrossSalary);
        await Assert.That(breakdown.SgkBase).IsEqualTo(monthModel.SgkBase);
        await Assert.That(breakdown.NetSalary).IsEqualTo(monthModel.NetSalary);
        await Assert.That(breakdown.FinalNetSalary).IsEqualTo(monthModel.FinalNetSalary);

        // Employee SGK
        await Assert.That(breakdown.EmployeeSgkDeduction).IsEqualTo(monthModel.EmployeeSgkDeduction);
        await Assert.That(breakdown.EmployeeSgkExemption).IsEqualTo(monthModel.EmployeeSgkExemption);
        await Assert.That(breakdown.EmployeeFinalSgkDeduction).IsEqualTo(monthModel.EmployeeFinalSgkDeduction);

        // Employee Unemployment
        await Assert.That(breakdown.EmployeeUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployeeUnemploymentInsuranceDeduction);
        await Assert.That(breakdown.EmployeeUnemploymentInsuranceExemption).IsEqualTo(monthModel.EmployeeUnemploymentInsuranceExemption);
        await Assert.That(breakdown.EmployeeFinalUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployeeFinalUnemploymentInsuranceDeduction);

        // Income Tax
        await Assert.That(breakdown.IncomeTaxBase).IsEqualTo(monthModel.IncomeTaxBase);
        await Assert.That(breakdown.CumulativeIncomeTaxBase).IsEqualTo(monthModel.CumulativeIncomeTaxBase);
        await Assert.That(breakdown.CumulativeSalary).IsEqualTo(monthModel.CumulativeSalary);
        await Assert.That(breakdown.EmployeeIncomeTax).IsEqualTo(monthModel.EmployeeIncomeTax);
        await Assert.That(breakdown.EmployeeIncomeTaxExemptionAmount).IsEqualTo(monthModel.EmployeeIncomeTaxExemptionAmount);
        await Assert.That(breakdown.EmployeeMinWageTaxExemptionAmount).IsEqualTo(monthModel.EmployeeMinWageTaxExemptionAmount);

        // Stamp Tax
        await Assert.That(breakdown.StampTax).IsEqualTo(monthModel.StampTax);
        await Assert.That(breakdown.EmployeeStampTaxExemption).IsEqualTo(monthModel.EmployeeStampTaxExemption);
        await Assert.That(breakdown.EmployerStampTax).IsEqualTo(monthModel.EmployerStampTax);
        await Assert.That(breakdown.EmployerStampTaxExemption).IsEqualTo(monthModel.EmployerStampTaxExemption);
        await Assert.That(breakdown.TotalStampTaxExemption).IsEqualTo(monthModel.TotalStampTaxExemption);

        // AGI
        await Assert.That(breakdown.AgiAmount).IsEqualTo(monthModel.AgiAmount);

        // Employer SGK
        await Assert.That(breakdown.EmployerSgkDeduction).IsEqualTo(monthModel.EmployerSgkDeduction);
        await Assert.That(breakdown.EmployerSgkExemption).IsEqualTo(monthModel.EmployerSgkExemption);
        await Assert.That(breakdown.EmployerFinalSgkDeduction).IsEqualTo(monthModel.EmployerFinalSgkDeduction);

        // Employer Unemployment
        await Assert.That(breakdown.EmployerUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployerUnemploymentInsuranceDeduction);
        await Assert.That(breakdown.EmployerUnemploymentInsuranceExemption).IsEqualTo(monthModel.EmployerUnemploymentInsuranceExemption);
        await Assert.That(breakdown.EmployerFinalUnemploymentInsuranceDeduction).IsEqualTo(monthModel.EmployerFinalUnemploymentInsuranceDeduction);

        // Employer Income Tax
        await Assert.That(breakdown.EmployerIncomeTaxExemptionAmount).IsEqualTo(monthModel.EmployerIncomeTaxExemptionAmount);
        await Assert.That(breakdown.EmployerFinalIncomeTax).IsEqualTo(monthModel.EmployerFinalIncomeTax);

        // Totals
        await Assert.That(breakdown.TotalSgkExemption).IsEqualTo(monthModel.TotalSgkExemption);
        await Assert.That(breakdown.EmployerTotalSgkCost).IsEqualTo(monthModel.EmployerTotalSgkCost);
        await Assert.That(breakdown.EmployerTotalCost).IsEqualTo(monthModel.EmployerTotalCost);
    }

    [Test]
    public async Task ToBreakdown_Should_Map_AppliedTaxSlices_Exactly()
    {
        // Arrange - Higher salary to hit multiple brackets
        var yearModel = CreateAndCalculateYearModel(grossSalary: 100_000m);
        var monthModel = yearModel.Months[11]; // December - should have accumulated enough to hit multiple brackets
        var month = MonthsOfYear.December;

        // Act
        var breakdown = monthModel.ToBreakdown(month);

        // Assert
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
        // Arrange
        var yearModel = CreateAndCalculateYearModel();

        // Act & Assert
        for (var i = 0; i < 12; i++)
        {
            var monthModel = yearModel.Months[i];
            var expectedMonth = MonthsOfYear.AllMonths[i];
            var breakdown = monthModel.ToBreakdown(expectedMonth);

            await Assert.That(breakdown.Month).IsEqualTo(expectedMonth);
            await Assert.That(breakdown.Month.Number).IsEqualTo((short)(i + 1));
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // YearlySalarySnapshot Mapping Tests - Basic Properties
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToSnapshot_Should_Map_Metadata_And_WorkDays_Exactly()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Assert - Metadata
        await Assert.That(snapshot.Year).IsEqualTo(year);
        await Assert.That(snapshot.MonthlyBreakdowns).Count().IsEqualTo(12);

        // Work Days
        await Assert.That(snapshot.TotalWorkDays).IsEqualTo(yearModel.TotalWorkDays);
        await Assert.That(snapshot.AvgWorkDays).IsEqualTo(yearModel.AvgWorkDays);
        await Assert.That(snapshot.TotalResearchAndDevelopmentWorkedDays).IsEqualTo(yearModel.TotalResearchAndDevelopmentWorkedDays);
        await Assert.That(snapshot.AvgResearchAndDevelopmentWorkedDays).IsEqualTo(yearModel.AvgResearchAndDevelopmentWorkedDays);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_GrossAndEmployeeSgk_Exactly()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Gross Salary
        await Assert.That(snapshot.CalculatedGrossSalary).IsEqualTo(yearModel.CalculatedGrossSalary);
        await Assert.That(snapshot.AvgCalculatedGrossSalary).IsEqualTo(yearModel.AvgCalculatedGrossSalary);

        // Employee SGK
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
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Employee Unemployment
        await Assert.That(snapshot.EmployeeUnemploymentInsuranceDeduction).IsEqualTo(yearModel.EmployeeUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.AvgEmployeeUnemploymentInsuranceDeduction).IsEqualTo(yearModel.AvgEmployeeUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.EmployeeUnemploymentInsuranceExemption).IsEqualTo(yearModel.EmployeeUnemploymentInsuranceExemption);
        await Assert.That(snapshot.AvgEmployeeUnemploymentInsuranceExemption).IsEqualTo(yearModel.AvgEmployeeUnemploymentInsuranceExemption);

        // Income Tax
        await Assert.That(snapshot.EmployeeIncomeTax).IsEqualTo(yearModel.EmployeeIncomeTax);
        await Assert.That(snapshot.AvgEmployeeIncomeTax).IsEqualTo(yearModel.AvgEmployeeIncomeTax);
        await Assert.That(snapshot.EmployeeMinWageTaxExemptionAmount).IsEqualTo(yearModel.EmployeeMinWageTaxExemptionAmount);
        await Assert.That(snapshot.AvgEmployeeMinWageTaxExemptionAmount).IsEqualTo(yearModel.AvgEmployeeMinWageTaxExemptionAmount);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_StampTaxAndNetSalary_Exactly()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Stamp Tax
        await Assert.That(snapshot.StampTax).IsEqualTo(yearModel.StampTax);
        await Assert.That(snapshot.AvgStampTax).IsEqualTo(yearModel.AvgStampTax);
        await Assert.That(snapshot.EmployerStampTax).IsEqualTo(yearModel.EmployerStampTax);
        await Assert.That(snapshot.AvgEmployerStampTax).IsEqualTo(yearModel.AvgEmployerStampTax);
        await Assert.That(snapshot.EmployerStampTaxExemption).IsEqualTo(yearModel.EmployerStampTaxExemption);
        await Assert.That(snapshot.AvgEmployerStampTaxExemption).IsEqualTo(yearModel.AvgEmployerStampTaxExemption);
        await Assert.That(snapshot.TotalStampTaxExemption).IsEqualTo(yearModel.TotalStampTaxExemption);
        await Assert.That(snapshot.AvgTotalStampTaxExemption).IsEqualTo(yearModel.AvgTotalStampTaxExemption);

        // Net Salary & AGI
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
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Employer SGK
        await Assert.That(snapshot.EmployerSgkDeduction).IsEqualTo(yearModel.EmployerSgkDeduction);
        await Assert.That(snapshot.AvgEmployerSgkDeduction).IsEqualTo(yearModel.AvgEmployerSgkDeduction);
        await Assert.That(snapshot.EmployerSgkExemption).IsEqualTo(yearModel.EmployerSgkExemption);
        await Assert.That(snapshot.AvgEmployerSgkExemption).IsEqualTo(yearModel.AvgEmployerSgkExemption);
        await Assert.That(snapshot.EmployerTotalSgkCost).IsEqualTo(yearModel.EmployerTotalSgkCost);
        await Assert.That(snapshot.AvgEmployerTotalSgkCost).IsEqualTo(yearModel.AvgEmployerTotalSgkCost);

        // Employer Unemployment
        await Assert.That(snapshot.EmployerUnemploymentInsuranceDeduction).IsEqualTo(yearModel.EmployerUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.AvgEmployerUnemploymentInsuranceDeduction).IsEqualTo(yearModel.AvgEmployerUnemploymentInsuranceDeduction);
        await Assert.That(snapshot.EmployerUnemploymentInsuranceExemption).IsEqualTo(yearModel.EmployerUnemploymentInsuranceExemption);
        await Assert.That(snapshot.AvgEmployerUnemploymentInsuranceExemption).IsEqualTo(yearModel.AvgEmployerUnemploymentInsuranceExemption);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_EmployerIncomeTaxAndTotals_Exactly()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Employer Income Tax
        await Assert.That(snapshot.EmployerFinalIncomeTax).IsEqualTo(yearModel.EmployerFinalIncomeTax);
        await Assert.That(snapshot.AvgEmployerFinalIncomeTax).IsEqualTo(yearModel.AvgEmployerFinalIncomeTax);
        await Assert.That(snapshot.EmployerIncomeTaxExemptionAmount).IsEqualTo(yearModel.EmployerIncomeTaxExemptionAmount);
        await Assert.That(snapshot.AvgEmployerIncomeTaxExemptionAmount).IsEqualTo(yearModel.AvgEmployerIncomeTaxExemptionAmount);

        // Overall Totals
        await Assert.That(snapshot.TotalSgkExemption).IsEqualTo(yearModel.TotalSgkExemption);
        await Assert.That(snapshot.AvgTotalSgkExemption).IsEqualTo(yearModel.AvgTotalSgkExemption);
        await Assert.That(snapshot.EmployerTotalCost).IsEqualTo(yearModel.EmployerTotalCost);
        await Assert.That(snapshot.AvgEmployerTotalCost).IsEqualTo(yearModel.AvgEmployerTotalCost);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_Semester_Properties_Exactly()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Assert - First Half
        await Assert.That(snapshot.FirstHalfWorkedDays).IsEqualTo(yearModel.GetSemesterWorkedDays("first"));
        await Assert.That(snapshot.FirstHalfEmployerTotalCost).IsEqualTo(yearModel.EmployerHalfTotalCost("first"));
        await Assert.That(snapshot.FirstHalfEmployerAvgTotalCostSkipNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("first", skipNonWorkedMonths: true));
        await Assert.That(snapshot.FirstHalfEmployerAvgTotalCostIncludeNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("first", skipNonWorkedMonths: false));
        await Assert.That(snapshot.FirstHalfTubitakAvgCost).IsEqualTo(yearModel.GetSemesterTubitakAvgCost("first"));

        // Assert - Second Half
        await Assert.That(snapshot.SecondHalfWorkedDays).IsEqualTo(yearModel.GetSemesterWorkedDays("second"));
        await Assert.That(snapshot.SecondHalfEmployerTotalCost).IsEqualTo(yearModel.EmployerHalfTotalCost("second"));
        await Assert.That(snapshot.SecondHalfEmployerAvgTotalCostSkipNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("second", skipNonWorkedMonths: true));
        await Assert.That(snapshot.SecondHalfEmployerAvgTotalCostIncludeNonWorked).IsEqualTo(yearModel.YearHalfEmployerAvgTotalCost("second", skipNonWorkedMonths: false));
        await Assert.That(snapshot.SecondHalfTubitakAvgCost).IsEqualTo(yearModel.GetSemesterTubitakAvgCost("second"));
    }

    [Test]
    public async Task ToSnapshot_Should_Map_Monthly_Breakdowns_In_Order()
    {
        // Arrange
        var yearModel = CreateAndCalculateYearModel();
        const int year = 2024;

        // Act
        var snapshot = yearModel.ToSnapshot(year);

        // Assert
        await Assert.That(snapshot.MonthlyBreakdowns).Count().IsEqualTo(12);

        for (var i = 0; i < 12; i++)
        {
            var breakdown = snapshot.MonthlyBreakdowns[i];
            var expectedMonth = MonthsOfYear.AllMonths[i];

            await Assert.That(breakdown.Month).IsEqualTo(expectedMonth);
            await Assert.That(breakdown.CalculatedGrossSalary).IsEqualTo(yearModel.Months[i].CalculatedGrossSalary);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TaxSlice Mapping Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToSnapshot_Should_Map_TaxSlice_With_Ceil()
    {
        // Arrange
        var taxSlice = new TaxSlice(0.15, 110_000m);

        // Act
        var snapshot = taxSlice.ToSnapshot();

        // Assert
        await Assert.That(snapshot.Rate).IsEqualTo(0.15);
        await Assert.That(snapshot.Ceil).IsEqualTo(110_000m);
    }

    [Test]
    public async Task ToSnapshot_Should_Map_TaxSlice_Without_Ceil()
    {
        // Arrange
        var taxSlice = new TaxSlice(0.40, null);

        // Act
        var snapshot = taxSlice.ToSnapshot();

        // Assert
        await Assert.That(snapshot.Rate).IsEqualTo(0.40);
        await Assert.That(snapshot.Ceil).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // Edge Case Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToBreakdown_Should_Throw_ArgumentNullException_When_Model_Is_Null()
    {
        // Arrange
        MonthCalculationModel? model = null;

        // Act & Assert
        await Assert.That(() => model!.ToBreakdown(MonthsOfYear.January))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ToSnapshot_Year_Should_Throw_ArgumentNullException_When_Model_Is_Null()
    {
        // Arrange
        YearCalculationModel? model = null;

        // Act & Assert
        await Assert.That(() => model!.ToSnapshot(2024))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ToSnapshot_Should_Handle_Partial_Year_With_Zero_Worked_Days_In_Some_Months()
    {
        // Arrange - Create model with partial work (only first 6 months)
        var yearParam = _yearParameters.Parameters.First(p => p.Year == 2024);
        var employeeType = _constantParameters.EmployeeTypeConstants.First(e => e.Id == 1);

        var monthlySalaries = MonthsOfYear.AllMonths.Select((month, index) =>
            new MonthlySalary(
                month,
                index < 6 ? 50_000m : 0m, // Only first 6 months have salary
                index < 6 ? 30u : 0u,     // Only first 6 months have worked days
                0
            )
        );

        var yearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParam,
            MonthlySalaries: monthlySalaries,
            EmployeeTypeConstant: employeeType,
            StandardEmployeeTypeConstant: employeeType,
            CalculationConstants: _constantParameters.CalculationConstants,
            EmployeeEducationExemptionRate: 0.0,
            AgiRate: 0.0,
            DisabilityDegree: 0,
            IsPensioner: false
        );

        var yearModel = new YearCalculationModel(yearlyParams, new CalculationOptions());
        yearModel.Calculate(CalculationMode.GrossToNet);

        // Act
        var snapshot = yearModel.ToSnapshot(2024);

        // Assert - Second half should have zero values
        await Assert.That(snapshot.SecondHalfWorkedDays).IsEqualTo(0);
        await Assert.That(snapshot.SecondHalfEmployerTotalCost).IsEqualTo(0m);

        // First half should have positive values
        await Assert.That(snapshot.FirstHalfWorkedDays).IsGreaterThan(0);
        await Assert.That(snapshot.FirstHalfEmployerTotalCost).IsGreaterThan(0m);
    }
}
