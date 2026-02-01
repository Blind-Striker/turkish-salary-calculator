#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Requests;

/// <summary>
/// Tests that verify different ways of constructing the same logical input
/// produce identical calculation results.
/// </summary>
public class InputConstructionParityTests
{
    private static ISalaryCalculator CreateCalculator() => SalaryCalculatorBuilder.Create();

    // ═══════════════════════════════════════════════════════════════
    // Uniform vs FillForward Parity
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Uniform_And_FillForward_SingleEntry_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();
        const decimal salary = 50_000m;

        var uniform = MonthlyInput.Uniform(salary);
        var fillForwardResult = MonthlyInput.FillForward((MonthsOfYear.January, salary));

        await Assert.That(fillForwardResult.IsSuccess).IsTrue();
        var fillForward = fillForwardResult.Value;

        var resultUniform = calculator.UseGrossToNet.ForYear(2024).Calculate(uniform);
        var resultFillForward = calculator.UseGrossToNet.ForYear(2024).Calculate(fillForward);

        await Assert.That(resultUniform.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultUniform.Value.NetSalary).IsEqualTo(resultFillForward.Value.NetSalary);
        await Assert.That(resultUniform.Value.CalculatedGrossSalary).IsEqualTo(resultFillForward.Value.CalculatedGrossSalary);
        await Assert.That(resultUniform.Value.EmployerTotalCost).IsEqualTo(resultFillForward.Value.EmployerTotalCost);
    }

    [Test]
    public async Task Uniform_And_Manual12Months_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();
        const decimal salary = 50_000m;

        var uniform = MonthlyInput.Uniform(salary);
        var manual = CreateManual12Months(salary);

        var resultUniform = calculator.UseGrossToNet.ForYear(2024).Calculate(uniform);
        var resultManual = calculator.UseGrossToNet.ForYear(2024).Calculate(manual);

        await Assert.That(resultUniform.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultUniform.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
        await Assert.That(resultUniform.Value.CalculatedGrossSalary).IsEqualTo(resultManual.Value.CalculatedGrossSalary);
        await Assert.That(resultUniform.Value.EmployerTotalCost).IsEqualTo(resultManual.Value.EmployerTotalCost);
    }

    [Test]
    public async Task FillForward_And_Manual12Months_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();
        const decimal salary = 50_000m;

        var fillForwardResult = MonthlyInput.FillForward((MonthsOfYear.January, salary));
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();
        var fillForward = fillForwardResult.Value;

        var manual = CreateManual12Months(salary);

        var resultFillForward = calculator.UseGrossToNet.ForYear(2024).Calculate(fillForward);
        var resultManual = calculator.UseGrossToNet.ForYear(2024).Calculate(manual);

        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
    }

    // ═══════════════════════════════════════════════════════════════
    // Mid-Year Salary Change Parity
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FillForward_MidYearRaise_And_Manual_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();
        const decimal salaryBefore = 30_000m;
        const decimal salaryAfter = 40_000m;

        // FillForward approach - raise in July
        var fillForwardResult = MonthlyInput.FillForward(
            (MonthsOfYear.January, salaryBefore),
            (MonthsOfYear.July, salaryAfter));
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();
        var fillForward = fillForwardResult.Value;

        // Manual approach - same pattern
        var manual = CreateManualMidYearRaise(salaryBefore, salaryAfter);

        var resultFillForward = calculator.UseGrossToNet.ForYear(2024).Calculate(fillForward);
        var resultManual = calculator.UseGrossToNet.ForYear(2024).Calculate(manual);

        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
        await Assert.That(resultFillForward.Value.CalculatedGrossSalary).IsEqualTo(resultManual.Value.CalculatedGrossSalary);
        await Assert.That(resultFillForward.Value.EmployerTotalCost).IsEqualTo(resultManual.Value.EmployerTotalCost);
    }

    [Test]
    public async Task MidYearRaise_Should_HaveDifferentMonthlyNetSalaries()
    {
        var calculator = CreateCalculator();

        var fillForwardResult = MonthlyInput.FillForward(
            (MonthsOfYear.January, 30_000m),
            (MonthsOfYear.July, 50_000m));
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();

        var result = calculator.UseGrossToNet.ForYear(2024).Calculate(fillForwardResult.Value);
        await Assert.That(result.IsSuccess).IsTrue();

        // First 6 months should have lower net than last 6 months
        var janNet = result.Value.MonthlyBreakdowns[0].FinalNetSalary;
        var julNet = result.Value.MonthlyBreakdowns[6].FinalNetSalary;
        await Assert.That(julNet).IsGreaterThan(janNet);
    }

    // ═══════════════════════════════════════════════════════════════
    // FillBackward Parity
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FillBackward_SingleDecember_And_Uniform_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();
        const decimal salary = 45_000m;

        var uniform = MonthlyInput.Uniform(salary);
        var fillBackwardResult = MonthlyInput.FillBackward((MonthsOfYear.December, salary));

        await Assert.That(fillBackwardResult.IsSuccess).IsTrue();
        var fillBackward = fillBackwardResult.Value;

        var resultUniform = calculator.UseGrossToNet.ForYear(2024).Calculate(uniform);
        var resultFillBackward = calculator.UseGrossToNet.ForYear(2024).Calculate(fillBackward);

        await Assert.That(resultUniform.IsSuccess).IsTrue();
        await Assert.That(resultFillBackward.IsSuccess).IsTrue();
        await Assert.That(resultUniform.Value.NetSalary).IsEqualTo(resultFillBackward.Value.NetSalary);
    }

    [Test]
    public async Task FillBackward_MidYearChange_And_Manual_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();

        // FillBackward: July=40k fills Jan-Jul, December=50k fills Aug-Dec
        var fillBackwardResult = MonthlyInput.FillBackward(
            (MonthsOfYear.July, 40_000m),
            (MonthsOfYear.December, 50_000m));
        await Assert.That(fillBackwardResult.IsSuccess).IsTrue();
        var fillBackward = fillBackwardResult.Value;

        // Manual with same pattern
        var manual = new List<MonthlyInput>();
        foreach (var month in MonthsOfYear.AllMonths)
        {
            var salary = month.Number <= 7 ? 40_000m : 50_000m;
            manual.Add(new MonthlyInput(month, salary));
        }

        var resultFillBackward = calculator.UseGrossToNet.ForYear(2024).Calculate(fillBackward);
        var resultManual = calculator.UseGrossToNet.ForYear(2024).Calculate(manual);

        await Assert.That(resultFillBackward.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultFillBackward.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
    }

    // ═══════════════════════════════════════════════════════════════
    // Varying WorkedDays Scenarios
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task PartialMonth_WorkedDays_Should_AffectCalculation()
    {
        var calculator = CreateCalculator();
        const decimal salary = 50_000m;

        // Full month (30 days)
        var fullMonth = MonthlyInput.Uniform(salary, workedDays: 30);

        // Partial month (15 days)
        var partialMonth = MonthlyInput.Uniform(salary, workedDays: 15);

        var resultFull = calculator.UseGrossToNet.ForYear(2024).Calculate(fullMonth);
        var resultPartial = calculator.UseGrossToNet.ForYear(2024).Calculate(partialMonth);

        await Assert.That(resultFull.IsSuccess).IsTrue();
        await Assert.That(resultPartial.IsSuccess).IsTrue();

        // Partial month should have lower net (pro-rated)
        await Assert.That(resultPartial.Value.NetSalary).IsLessThan(resultFull.Value.NetSalary);
    }

    [Test]
    public async Task FillForward_WithCustomWorkedDays_And_Manual_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();

        // FillForward with custom worked days
        var fillForwardResult = MonthlyInput.FillForward(
            (MonthsOfYear.January, 50_000m, 20, 0),  // Jan: 20 worked days
            (MonthsOfYear.July, 60_000m, 30, 0));    // Jul onwards: 30 worked days
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();
        var fillForward = fillForwardResult.Value;

        // Manual with same pattern
        var manual = new List<MonthlyInput>();
        foreach (var month in MonthsOfYear.AllMonths)
        {
            if (month.Number < 7)
            {
                manual.Add(new MonthlyInput(month, 50_000m, 20, 0));
            }
            else
            {
                manual.Add(new MonthlyInput(month, 60_000m, 30, 0));
            }
        }

        var resultFillForward = calculator.UseGrossToNet.ForYear(2024).Calculate(fillForward);
        var resultManual = calculator.UseGrossToNet.ForYear(2024).Calculate(manual);

        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
    }

    // ═══════════════════════════════════════════════════════════════
    // R&D Days Scenarios (Teknokent/5746)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task RnDDays_Uniform_And_Manual_Should_ProduceSameCalculation_ForTeknokent()
    {
        var calculator = CreateCalculator();
        const decimal salary = 80_000m;
        const int rndDays = 20;

        var uniform = MonthlyInput.Uniform(salary, rndDays: rndDays);
        var manual = CreateManual12Months(salary, workedDays: 30, rndDays: rndDays);

        var resultUniform = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Calculate(uniform);

        var resultManual = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Calculate(manual);

        await Assert.That(resultUniform.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultUniform.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
        await Assert.That(resultUniform.Value.EmployerIncomeTaxExemptionAmount)
            .IsEqualTo(resultManual.Value.EmployerIncomeTaxExemptionAmount);
    }

    [Test]
    public async Task VaryingRnDDays_FillForward_And_Manual_Should_ProduceSameCalculation()
    {
        var calculator = CreateCalculator();

        // FillForward with varying R&D days
        var fillForwardResult = MonthlyInput.FillForward(
            (MonthsOfYear.January, 80_000m, 30, 15),   // Jan-Jun: 15 R&D days
            (MonthsOfYear.July, 80_000m, 30, 25));     // Jul-Dec: 25 R&D days
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();
        var fillForward = fillForwardResult.Value;

        // Manual with same pattern
        var manual = new List<MonthlyInput>();
        foreach (var month in MonthsOfYear.AllMonths)
        {
            var rndDays = month.Number < 7 ? 15 : 25;
            manual.Add(new MonthlyInput(month, 80_000m, 30, rndDays));
        }

        var resultFillForward = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Calculate(fillForward);

        var resultManual = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Calculate(manual);

        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultManual.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.Value.NetSalary).IsEqualTo(resultManual.Value.NetSalary);
    }

    [Test]
    public async Task RnDDays_Should_AffectExemptionAmount_ForTeknokent()
    {
        var calculator = CreateCalculator();
        const decimal salary = 80_000m;

        // Low R&D days
        var lowRnd = MonthlyInput.Uniform(salary, rndDays: 5);

        // High R&D days
        var highRnd = MonthlyInput.Uniform(salary, rndDays: 25);

        var resultLow = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Calculate(lowRnd);

        var resultHigh = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Calculate(highRnd);

        await Assert.That(resultLow.IsSuccess).IsTrue();
        await Assert.That(resultHigh.IsSuccess).IsTrue();

        // Higher R&D days should result in higher exemption
        await Assert.That(resultHigh.Value.EmployerIncomeTaxExemptionAmount)
            .IsGreaterThan(resultLow.Value.EmployerIncomeTaxExemptionAmount);
    }

    // ═══════════════════════════════════════════════════════════════
    // New Hire Scenarios
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task NewHire_StartingMidYear_Should_AllowZeroSalaryMonths()
    {
        // New hire scenario: Employee starts in March, Jan-Feb have 0 salary/worked days
        // Angular handles this by returning zeros for non-worked months
        var calculator = CreateCalculator();

        // New hire starting in March (0 salary Jan-Feb, then 50k from March)
        var fillForwardResult = MonthlyInput.FillForward(
            (MonthsOfYear.January, 0m, 0, 0),     // Not employed
            (MonthsOfYear.March, 50_000m, 30, 0)); // Starts in March
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();
        var fillForward = fillForwardResult.Value;

        var result = calculator.UseGrossToNet.ForYear(2024).Calculate(fillForward);

        // Should succeed - zero salary months are valid
        await Assert.That(result.IsSuccess).IsTrue();

        // Jan-Feb should have 0 values (no work)
        await Assert.That(result.Value.MonthlyBreakdowns[0].FinalNetSalary).IsEqualTo(0);
        await Assert.That(result.Value.MonthlyBreakdowns[0].CalculatedGrossSalary).IsEqualTo(0);
        await Assert.That(result.Value.MonthlyBreakdowns[1].FinalNetSalary).IsEqualTo(0);
        await Assert.That(result.Value.MonthlyBreakdowns[1].CalculatedGrossSalary).IsEqualTo(0);

        // March onwards should have positive values
        await Assert.That(result.Value.MonthlyBreakdowns[2].FinalNetSalary).IsGreaterThan(0);
        await Assert.That(result.Value.MonthlyBreakdowns[2].CalculatedGrossSalary).IsGreaterThan(0);
    }

    [Test]
    public async Task NewHire_StartingMidYear_Should_CalculateCorrectYearlyTotals()
    {
        // Verify yearly totals only count worked months
        var calculator = CreateCalculator();

        // Employee starts in July (6 months of work)
        var input = new List<MonthlyInput>();
        foreach (var month in MonthsOfYear.AllMonths)
        {
            if (month.Number < 7)
            {
                input.Add(new MonthlyInput(month, 0m, 0, 0)); // Not employed
            }
            else
            {
                input.Add(new MonthlyInput(month, 50_000m, 30, 0)); // Employed
            }
        }

        var result = calculator.UseGrossToNet.ForYear(2024).Calculate(input);

        await Assert.That(result.IsSuccess).IsTrue();

        // Total gross should be 6 months worth (not 12)
        await Assert.That(result.Value.CalculatedGrossSalary).IsEqualTo(50_000m * 6);

        // First 6 months should have 0 values
        for (var i = 0; i < 6; i++)
        {
            await Assert.That(result.Value.MonthlyBreakdowns[i].CalculatedGrossSalary).IsEqualTo(0);
        }

        // Last 6 months should have positive values
        for (var i = 6; i < 12; i++)
        {
            await Assert.That(result.Value.MonthlyBreakdowns[i].CalculatedGrossSalary).IsEqualTo(50_000m);
        }
    }

    [Test]
    public async Task NewHire_NegativeSalary_Should_FailValidation()
    {
        // Negative salary should still be rejected
        var calculator = CreateCalculator();

        var input = MonthsOfYear.AllMonths
            .Select(month => new MonthlyInput(month, -1000m, 30, 0))
            .ToList();

        var result = calculator.UseGrossToNet.ForYear(2024).Calculate(input);

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task NewHire_PartialFirstMonth_WithNonZeroWorkedDays_Should_CalculateCorrectly()
    {
        var calculator = CreateCalculator();

        // New hire starting mid-March (15 worked days in March)
        // Note: All months must have some worked days - use 1 day for months not worked
        // This simulates an employee who worked full year but had partial first month
        var input = new List<MonthlyInput>
        {
            new(MonthsOfYear.January, 50_000m, 30, 0),
            new(MonthsOfYear.February, 50_000m, 30, 0),
            new(MonthsOfYear.March, 50_000m, 15, 0),  // Partial month
            new(MonthsOfYear.April, 50_000m, 30, 0),
            new(MonthsOfYear.May, 50_000m, 30, 0),
            new(MonthsOfYear.June, 50_000m, 30, 0),
            new(MonthsOfYear.July, 50_000m, 30, 0),
            new(MonthsOfYear.August, 50_000m, 30, 0),
            new(MonthsOfYear.September, 50_000m, 30, 0),
            new(MonthsOfYear.October, 50_000m, 30, 0),
            new(MonthsOfYear.November, 50_000m, 30, 0),
            new(MonthsOfYear.December, 50_000m, 30, 0),
        };

        var result = calculator.UseGrossToNet.ForYear(2024).Calculate(input);

        await Assert.That(result.IsSuccess).IsTrue();

        // March net should be lower than April (partial month)
        var marchNet = result.Value.MonthlyBreakdowns[2].FinalNetSalary;
        var aprilNet = result.Value.MonthlyBreakdowns[3].FinalNetSalary;
        await Assert.That(marchNet).IsLessThan(aprilNet);
    }

    // ═══════════════════════════════════════════════════════════════
    // Cross-Mode Parity (same input, different modes)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task SameInput_DifferentConstructionMethods_Should_ProduceSameResult_ForNetToGross()
    {
        var calculator = CreateCalculator();
        const decimal netSalary = 30_000m;

        var uniform = MonthlyInput.Uniform(netSalary);
        var fillForwardResult = MonthlyInput.FillForward((MonthsOfYear.January, netSalary));
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();

        var resultUniform = calculator.UseNetToGross.ForYear(2024).Calculate(uniform);
        var resultFillForward = calculator.UseNetToGross.ForYear(2024).Calculate(fillForwardResult.Value);

        await Assert.That(resultUniform.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultUniform.Value.CalculatedGrossSalary)
            .IsEqualTo(resultFillForward.Value.CalculatedGrossSalary);
    }

    [Test]
    public async Task SameInput_DifferentConstructionMethods_Should_ProduceSameResult_ForTotalToGross()
    {
        var calculator = CreateCalculator();
        const decimal totalCost = 80_000m;

        var uniform = MonthlyInput.Uniform(totalCost);
        var fillForwardResult = MonthlyInput.FillForward((MonthsOfYear.January, totalCost));
        await Assert.That(fillForwardResult.IsSuccess).IsTrue();

        var resultUniform = calculator.UseTotalToGross.ForYear(2024).Calculate(uniform);
        var resultFillForward = calculator.UseTotalToGross.ForYear(2024).Calculate(fillForwardResult.Value);

        await Assert.That(resultUniform.IsSuccess).IsTrue();
        await Assert.That(resultFillForward.IsSuccess).IsTrue();
        await Assert.That(resultUniform.Value.CalculatedGrossSalary)
            .IsEqualTo(resultFillForward.Value.CalculatedGrossSalary);
    }

    // ═══════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════

    private static List<MonthlyInput> CreateManual12Months(
        decimal salary,
        int workedDays = 30,
        int rndDays = 0)
        =>
        [
            .. MonthsOfYear.AllMonths.Select(month => new MonthlyInput(month, salary, workedDays, rndDays)),
        ];

    private static List<MonthlyInput> CreateManualMidYearRaise(
        decimal salaryBefore,
        decimal salaryAfter)
        =>
        [
            .. MonthsOfYear.AllMonths.Select(month =>
                new MonthlyInput(month, month.Number < 7 ? salaryBefore : salaryAfter)),
        ];
}

