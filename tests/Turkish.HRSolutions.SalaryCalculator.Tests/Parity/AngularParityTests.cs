using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;
using Turkish.HRSolutions.SalaryCalculator.Application.Calculator;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.DependencyInjection;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Parity;

/// <summary>
/// Angular parity tests using per-scenario CLI calls.
/// Each test calls the Angular calculator CLI and compares with .NET.
/// </summary>
[Explicit]
[Category("Parity")]
[Category("Angular")]
[ParallelLimiter<AngularParityParallelLimit>]
public class AngularParityTests
{
    /// <summary>
    /// Shared Angular calculator client (per test session).
    /// Handles npm install and initialization once.
    /// </summary>
    [ClassDataSource<AngularCalculatorClient>(Shared = SharedType.PerTestSession)]
    public required AngularCalculatorClient AngularClient { get; init; }

    /// <summary>
    /// Shared .NET calculator and providers (per test session).
    /// </summary>
    [ClassDataSource<DotNetCalculatorContext>(Shared = SharedType.PerTestSession)]
    public required DotNetCalculatorContext DotNet { get; init; }

    [Test]
    [MethodDataSource(typeof(ParityScenarios), nameof(ParityScenarios.GetAllScenarios))]
    [DisplayName("Parity: $scenario")]
    [Property("TestId", "$scenario.TestId")]
    public async Task Scenario_Should_Match_Angular(ParityScenario scenario)
    {
        // 1. Call Angular CLI
        var angularResponse = await AngularClient.CalculateAsync(scenario.Input);

        await Assert.That(angularResponse.Success)
            .IsTrue()
            .Because("Angular CLI should succeed. Error: " + (angularResponse.Error?.Message ?? "none"));

        await Assert.That(angularResponse.Output)
            .IsNotNull()
            .Because("Angular CLI should return output");

        // 2. Calculate with .NET
        var dotNetResult = CalculateWithDotNet(scenario.Input);

        await Assert.That(dotNetResult.IsSuccess)
            .IsTrue()
            .Because(".NET calculation should succeed. Error: " + FormatErrors(dotNetResult));

        // 3. Compare results
        var comparer = new FieldComparer();
        var comparison = CompareResults(
            scenario,
            angularResponse.Output!,
            dotNetResult.Value,
            comparer);

        await Assert.That(comparison.Passed)
            .IsTrue()
            .Because(FormatFailure(comparison, angularResponse.ElapsedMs));
    }

    private Result<YearlySalarySnapshot> CalculateWithDotNet(TestInput input)
    {
        var months = MonthsOfYear.AllMonths
            .Select(month => new MonthlyInput(
                month,
                input.SalaryAmount,
                (int)input.WorkedDays,
                (int)input.ResearchAndDevelopmentWorkedDays))
            .ToList();

        var employeeTypeId = EmployeeTypeId.FromId(input.EmployeeTypeId);
        var disability = DisabilityDegreeId.FromDegree(input.DisabilityDegree);
        var agi = BuildAgiSettings(input);
        var rnd = BuildRnDSettings(input, employeeTypeId);

        return input.CalculationMode switch
        {
            "GROSS_TO_NET" => DotNet.Calculator.Calculate(new GrossToNetRequest(
                input.Year,
                months,
                employeeTypeId,
                disability,
                input.IsPensioner,
                input.ApplyMinWageTaxExemption,
                input.ApplyEmployerDiscount5746,
                agi,
                rnd)),
            "NET_TO_GROSS" => DotNet.Calculator.Calculate(new NetToGrossRequest(
                input.Year,
                months,
                employeeTypeId,
                disability,
                input.IsPensioner,
                input.ApplyMinWageTaxExemption,
                input.ApplyEmployerDiscount5746,
                agi,
                rnd,
                input.IsAgiIncludedNet)),
            "TOTAL_TO_GROSS" => DotNet.Calculator.Calculate(new TotalToGrossRequest(
                input.Year,
                months,
                employeeTypeId,
                disability,
                input.IsPensioner,
                input.ApplyMinWageTaxExemption,
                input.ApplyEmployerDiscount5746,
                agi,
                rnd)),
            _ => throw new InvalidOperationException($"Unsupported calculation mode: {input.CalculationMode}")
        };
    }

    private static AgiSettings? BuildAgiSettings(TestInput input)
    {
        if (!input.IsAgiCalculationEnabled)
        {
            return null;
        }

        var (spouseStatus, children) = MapAgiRate(input.AgiRate);
        return new AgiSettings(spouseStatus, children, input.IsAgiIncludedTax);
    }

    private RnDSettings? BuildRnDSettings(TestInput input, EmployeeTypeId employeeTypeId)
    {
        var employeeType = DotNet.ConstantsProvider.GetEmployeeType(employeeTypeId);
        if (employeeType?.EmployerEducationIncomeTaxExemption != true)
        {
            return null;
        }

        var educationType = DotNet.ConstantsProvider.AllEducationTypes
            .OrderBy(e => Math.Abs(e.ExemptionRate - (double)input.EducationExemptionRate))
            .FirstOrDefault();

        return educationType is null
            ? null
            : new RnDSettings(EducationTypeId.FromId(educationType.Id));
    }

    private static (SpouseStatus SpouseStatus, int Children) MapAgiRate(decimal agiRate)
    {
        return agiRate switch
        {
            0.50m => (SpouseStatus.Unmarried, 0),
            0.575m => (SpouseStatus.SpouseWorking, 1),
            0.60m => (SpouseStatus.SpouseNotWorking, 0),
            0.65m => (SpouseStatus.SpouseWorking, 2),
            0.675m => (SpouseStatus.SpouseNotWorking, 1),
            0.75m => (SpouseStatus.SpouseWorking, 3),
            0.80m => (SpouseStatus.SpouseWorking, 4),
            0.85m => (SpouseStatus.SpouseWorking, 5),
            _ => (SpouseStatus.Unmarried, 0)
        };
    }

    private static TestComparisonResult CompareResults(
        ParityScenario scenario,
        TestOutput expected,
        YearlySalarySnapshot actual,
        FieldComparer comparer)
    {
        var results = new List<FieldComparisonResult>();
        var breakdownsByMonth = actual.MonthlyBreakdowns.ToDictionary(b => (int)b.Month.Number, b => b);

        foreach (var expectedMonth in expected.Months)
        {
            if (!breakdownsByMonth.TryGetValue(expectedMonth.MonthNumber, out var actualMonth))
            {
                results.Add(new FieldComparisonResult(
                    $"Month {expectedMonth.MonthNumber:00} missing",
                    0m, 0m, 0m, false));
                continue;
            }

            AddMonthComparisons(results, expectedMonth, actualMonth, comparer);
        }

        AddComparison(results, "Totals.CalculatedGrossSalary", expected.Totals.CalculatedGrossSalary, actual.CalculatedGrossSalary, comparer);
        AddComparison(results, "Totals.NetSalary", expected.Totals.NetSalary, actual.NetSalary, comparer);
        AddComparison(results, "Totals.FinalNetSalary", expected.Totals.FinalNetSalary, actual.FinalNetSalary, comparer);
        AddComparison(results, "Totals.EmployerTotalCost", expected.Totals.EmployerTotalCost, actual.EmployerTotalCost, comparer);
        AddComparison(results, "Totals.TotalSgkExemption", expected.Totals.TotalSgkExemption, actual.TotalSgkExemption, comparer);

        return new TestComparisonResult
        {
            TestId = scenario.TestId,
            Description = scenario.Description,
            Passed = results.All(f => f.Passed),
            FieldResults = results
        };
    }

    private static void AddMonthComparisons(
        List<FieldComparisonResult> results,
        MonthOutput expected,
        MonthlyBreakdown actual,
        FieldComparer comparer)
    {
        var prefix = $"Month {expected.MonthNumber:00}";

        AddComparison(results, $"{prefix}.CalculatedGrossSalary", expected.CalculatedGrossSalary, actual.CalculatedGrossSalary, comparer);
        AddComparison(results, $"{prefix}.SgkBase", expected.SgkBase, actual.SgkBase, comparer);
        AddComparison(results, $"{prefix}.IncomeTaxBase", expected.IncomeTaxBase, actual.IncomeTaxBase, comparer);
        AddComparison(results, $"{prefix}.CumulativeIncomeTaxBase", expected.CumulativeIncomeTaxBase, actual.CumulativeIncomeTaxBase, comparer);
        AddComparison(results, $"{prefix}.EmployeeSgkDeduction", expected.EmployeeSgkDeduction, actual.EmployeeSgkDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployeeSgkExemption", expected.EmployeeSgkExemption, actual.EmployeeSgkExemption, comparer);
        AddComparison(results, $"{prefix}.EmployeeFinalSgkDeduction", expected.EmployeeFinalSgkDeduction, actual.EmployeeFinalSgkDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployeeUnemploymentInsuranceDeduction", expected.EmployeeUnemploymentInsuranceDeduction, actual.EmployeeUnemploymentInsuranceDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployeeUnemploymentInsuranceExemption", expected.EmployeeUnemploymentInsuranceExemption, actual.EmployeeUnemploymentInsuranceExemption, comparer);
        AddComparison(results, $"{prefix}.EmployeeFinalUnemploymentInsuranceDeduction", expected.EmployeeFinalUnemploymentInsuranceDeduction, actual.EmployeeFinalUnemploymentInsuranceDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployeeIncomeTax", expected.EmployeeIncomeTax, actual.EmployeeIncomeTax, comparer);
        AddComparison(results, $"{prefix}.EmployeeIncomeTaxExemption", expected.EmployeeIncomeTaxExemption, actual.EmployeeIncomeTaxExemptionAmount, comparer);
        AddComparison(results, $"{prefix}.StampTax", expected.StampTax, actual.StampTax, comparer);
        AddComparison(results, $"{prefix}.EmployeeStampTaxExemption", expected.EmployeeStampTaxExemption, actual.EmployeeStampTaxExemption, comparer);
        AddComparison(results, $"{prefix}.EmployerStampTaxExemption", expected.EmployerStampTaxExemption, actual.EmployerStampTaxExemption, comparer);
        AddComparison(results, $"{prefix}.TotalStampTaxExemption", expected.TotalStampTaxExemption, actual.TotalStampTaxExemption, comparer);
        AddComparison(results, $"{prefix}.EmployerStampTax", expected.EmployerStampTax, actual.EmployerStampTax, comparer);
        AddComparison(results, $"{prefix}.NetSalary", expected.NetSalary, actual.NetSalary, comparer);
        AddComparison(results, $"{prefix}.AgiAmount", expected.AgiAmount, actual.AgiAmount, comparer);
        AddComparison(results, $"{prefix}.FinalNetSalary", expected.FinalNetSalary, actual.FinalNetSalary, comparer);
        AddComparison(results, $"{prefix}.EmployerSgkDeduction", expected.EmployerSgkDeduction, actual.EmployerSgkDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployerSgkExemption", expected.EmployerSgkExemption, actual.EmployerSgkExemption, comparer);
        AddComparison(results, $"{prefix}.EmployerFinalSgkDeduction", expected.EmployerFinalSgkDeduction, actual.EmployerFinalSgkDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployerUnemploymentInsuranceDeduction", expected.EmployerUnemploymentInsuranceDeduction, actual.EmployerUnemploymentInsuranceDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployerUnemploymentInsuranceExemption", expected.EmployerUnemploymentInsuranceExemption, actual.EmployerUnemploymentInsuranceExemption, comparer);
        AddComparison(results, $"{prefix}.EmployerFinalUnemploymentInsuranceDeduction", expected.EmployerFinalUnemploymentInsuranceDeduction, actual.EmployerFinalUnemploymentInsuranceDeduction, comparer);
        AddComparison(results, $"{prefix}.EmployerIncomeTaxExemption", expected.EmployerIncomeTaxExemption, actual.EmployerIncomeTaxExemptionAmount, comparer);
        AddComparison(results, $"{prefix}.EmployerFinalIncomeTax", expected.EmployerFinalIncomeTax, actual.EmployerFinalIncomeTax, comparer);
        AddComparison(results, $"{prefix}.EmployerTotalSgkCost", expected.EmployerTotalSgkCost, actual.EmployerTotalSgkCost, comparer);
        AddComparison(results, $"{prefix}.EmployerTotalCost", expected.EmployerTotalCost, actual.EmployerTotalCost, comparer);
        AddComparison(results, $"{prefix}.TotalSgkExemption", expected.TotalSgkExemption, actual.TotalSgkExemption, comparer);
        AddComparison(results, $"{prefix}.EmployeeMinWageTaxExemption", expected.EmployeeMinWageTaxExemption, actual.EmployeeMinWageTaxExemptionAmount, comparer);
    }

    private static void AddComparison(
        List<FieldComparisonResult> results,
        string fieldName,
        decimal expected,
        decimal actual,
        FieldComparer comparer)
    {
        results.Add(comparer.Compare(fieldName, expected, actual));
    }

    private static string FormatFailure(TestComparisonResult result, long angularElapsedMs)
    {
        var failedFields = result.FailedFields
            .Take(10)
            .Select(field => $"  {field.FieldName}: expected {field.Expected:F4}, actual {field.Actual:F4}, diff {field.Difference:F4}")
            .ToList();

        var suffix = result.FailedFields.Count() > 10
            ? Environment.NewLine + "  ...and " + (result.FailedFields.Count() - 10) + " more"
            : string.Empty;

        return "[" + result.TestId + "] " + result.Description + " (Angular: " + angularElapsedMs + "ms)" + Environment.NewLine +
               string.Join(Environment.NewLine, failedFields) + suffix;
    }

    private static string FormatErrors(Result<YearlySalarySnapshot> result)
    {
        if (result.IsSuccess)
        {
            return "none";
        }

        return string.Join("; ", result.Errors.Select(e => e.Message));
    }
}

/// <summary>
/// Parallel limiter for Angular parity tests.
/// Limits concurrent CLI calls to avoid overwhelming the system.
/// </summary>
public sealed record AngularParityParallelLimit : IParallelLimit
{
    /// <summary>
    /// Gets the maximum number of concurrent tests.
    /// </summary>
    public int Limit => 8;
}

/// <summary>
/// Context for .NET calculator services (shared per test session).
/// </summary>
public sealed class DotNetCalculatorContext : IAsyncInitializer, IAsyncDisposable
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the salary calculator instance.
    /// </summary>
    public ISalaryCalculator Calculator { get; private set; } = null!;

    /// <summary>
    /// Gets the calculation constants provider instance.
    /// </summary>
    public ICalculationConstantsProvider ConstantsProvider { get; private set; } = null!;

    /// <summary>
    /// Initializes the DI container and resolves services.
    /// </summary>
    public Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddSalaryCalculator();

        _serviceProvider = services.BuildServiceProvider();
        Calculator = _serviceProvider.GetRequiredService<ISalaryCalculator>();
        ConstantsProvider = _serviceProvider.GetRequiredService<ICalculationConstantsProvider>();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the DI container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }
}
