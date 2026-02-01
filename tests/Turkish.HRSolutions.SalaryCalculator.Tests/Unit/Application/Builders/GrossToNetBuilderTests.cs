#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Builders;

public class GrossToNetBuilderTests
{
    private static ISalaryCalculator CreateCalculator() => SalaryCalculatorBuilder.Create();

    [Test]
    public async Task Build_Should_SetMode_To_GrossToNet()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.Mode).IsEqualTo(CalculationMode.GrossToNet);
    }

    [Test]
    public async Task ForYear_Should_SetYear()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2025)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.Year).IsEqualTo(2025);
    }

    [Test]
    public async Task WithEmployeeType_Should_SetEmployeeType()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.Teknokent4691);
    }

    [Test]
    public async Task WithDisability_Should_SetDisability()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .WithDisability(DisabilityDegreeId.First)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.First);
    }

    [Test]
    public async Task AsPensioner_Should_SetIsPensionerTrue()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .AsPensioner()
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.IsPensioner).IsTrue();
    }

    [Test]
    public async Task WithMinWageExemption_Should_SetApplyMinWageExemption()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .WithMinWageExemption(false)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.ApplyMinWageExemption).IsFalse();
    }

    [Test]
    public async Task With5746Discount_Should_SetApply5746Discount()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .With5746Discount()
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.Apply5746Discount).IsTrue();
    }

    [Test]
    public async Task WithAgi_Should_SetAgiSettings()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2021)
            .WithAgi(a => a.SpouseNotWorking().WithChildren(3))
            .Build(MonthlyInput.Uniform(30_000m));

        await Assert.That(request.Agi).IsNotNull();
        await Assert.That(request.Agi!.SpouseStatus).IsEqualTo(SpouseStatus.SpouseNotWorking);
        await Assert.That(request.Agi.NumberOfChildren).IsEqualTo(3);
    }

    [Test]
    public async Task WithRnDSettings_Should_SetRnDSettings()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.RnD5746)
            .WithRnDSettings(r => r.WithEducation(EducationTypeId.Doctorate))
            .Build(MonthlyInput.Uniform(80_000m));

        await Assert.That(request.RnD).IsNotNull();
        await Assert.That(request.RnD!.Education).IsEqualTo(EducationTypeId.Doctorate);
    }

    [Test]
    public async Task Calculate_With_Decimal_Should_CreateUniformMonths_And_Succeed()
    {
        var calculator = CreateCalculator();

        var result = calculator.UseGrossToNet
            .ForYear(2024)
            .Calculate(50_000m);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.CalculatedGrossSalary).IsGreaterThan(0);
    }

    [Test]
    public async Task Calculate_With_Months_Should_Succeed()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(50_000m);

        var result = calculator.UseGrossToNet
            .ForYear(2024)
            .Calculate(months);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Calculate_With_ResultMonths_Should_PropagateFailure()
    {
        var calculator = CreateCalculator();
        var failedMonths = Result<IReadOnlyList<MonthlyInput>>.Failure(
            ErrorCode.InvalidMonthCount, "Test failure");

        var result = calculator.UseGrossToNet
            .ForYear(2024)
            .Calculate(failedMonths);

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Calculate_With_ResultMonths_Should_PropagateSuccess()
    {
        var calculator = CreateCalculator();
        var successMonths = Result<IReadOnlyList<MonthlyInput>>.Success(MonthlyInput.Uniform(50_000m));

        var result = calculator.UseGrossToNet
            .ForYear(2024)
            .Calculate(successMonths);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Default_EmployeeType_Should_Be_Standard()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.Standard);
    }

    [Test]
    public async Task Default_Disability_Should_Be_None()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.None);
    }

    [Test]
    public async Task Default_ApplyMinWageExemption_Should_Be_True()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseGrossToNet
            .ForYear(2024)
            .Build(MonthlyInput.Uniform(50_000m));

        await Assert.That(request.ApplyMinWageExemption).IsTrue();
    }
}
