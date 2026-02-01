#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Builders;

public class NetToGrossBuilderTests
{
    private static ISalaryCalculator CreateCalculator() => SalaryCalculatorBuilder.Create();

    [Test]
    public async Task Build_Should_SetMode_To_NetToGross()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2024)
            .Build(MonthlyInput.Uniform(30_000m));

        await Assert.That(request.Mode).IsEqualTo(CalculationMode.NetToGross);
    }

    [Test]
    public async Task ForYear_Should_SetYear()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2025)
            .Build(MonthlyInput.Uniform(30_000m));

        await Assert.That(request.Year).IsEqualTo(2025);
    }

    [Test]
    public async Task WithEmployeeType_Should_SetEmployeeType()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Build(MonthlyInput.Uniform(30_000m));

        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.Teknokent4691);
    }

    [Test]
    public async Task WithAgiIncludedInNet_Should_SetIsAgiIncludedInNetTrue()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2021)
            .WithAgiIncludedInNet()
            .Build(MonthlyInput.Uniform(25_000m));

        await Assert.That(request.IsAgiIncludedInNet).IsTrue();
    }

    [Test]
    public async Task WithAgiIncludedInNet_False_Should_SetIsAgiIncludedInNetFalse()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2021)
            .WithAgiIncludedInNet(false)
            .Build(MonthlyInput.Uniform(25_000m));

        await Assert.That(request.IsAgiIncludedInNet).IsFalse();
    }

    [Test]
    public async Task Default_IsAgiIncludedInNet_Should_Be_False()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2021)
            .Build(MonthlyInput.Uniform(25_000m));

        await Assert.That(request.IsAgiIncludedInNet).IsFalse();
    }

    [Test]
    public async Task WithAgiIncludedInNet_Should_AffectCalculation_For_Pre2022()
    {
        var calculator = CreateCalculator();

        var withoutAgi = calculator.UseNetToGross
            .ForYear(2021)
            .WithAgi(a => a.SpouseNotWorking().WithChildren(2))
            .WithAgiIncludedInNet(false)
            .Calculate(25_000m);

        var withAgi = calculator.UseNetToGross
            .ForYear(2021)
            .WithAgi(a => a.SpouseNotWorking().WithChildren(2))
            .WithAgiIncludedInNet(true)
            .Calculate(25_000m);

        await Assert.That(withoutAgi.IsSuccess).IsTrue();
        await Assert.That(withAgi.IsSuccess).IsTrue();
        await Assert.That(withoutAgi.Value.CalculatedGrossSalary)
            .IsNotEqualTo(withAgi.Value.CalculatedGrossSalary);
    }

    [Test]
    public async Task Calculate_With_Decimal_Should_Succeed()
    {
        var calculator = CreateCalculator();

        var result = calculator.UseNetToGross
            .ForYear(2024)
            .Calculate(30_000m);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.CalculatedGrossSalary).IsGreaterThan(30_000m);
    }

    [Test]
    public async Task Calculate_Should_FindGross_That_Produces_DesiredNet()
    {
        var calculator = CreateCalculator();
        const decimal desiredNet = 30_000m;

        var netToGrossResult = calculator.UseNetToGross
            .ForYear(2024)
            .Calculate(desiredNet);

        await Assert.That(netToGrossResult.IsSuccess).IsTrue();

        var foundGross = netToGrossResult.Value.AvgCalculatedGrossSalary;
        var verifyResult = calculator.UseGrossToNet
            .ForYear(2024)
            .Calculate(foundGross);

        await Assert.That(verifyResult.IsSuccess).IsTrue();
        var actualNet = verifyResult.Value.AvgFinalNetSalary;
        const decimal tolerance = desiredNet * 0.001m;
        await Assert.That(Math.Abs(actualNet - desiredNet)).IsLessThan(tolerance);
    }

    [Test]
    public async Task WithDisability_Should_SetDisability()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2024)
            .WithDisability(DisabilityDegreeId.Second)
            .Build(MonthlyInput.Uniform(30_000m));

        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.Second);
    }

    [Test]
    public async Task AsPensioner_Should_SetIsPensionerTrue()
    {
        var calculator = CreateCalculator();

        var request = calculator.UseNetToGross
            .ForYear(2024)
            .AsPensioner()
            .Build(MonthlyInput.Uniform(30_000m));

        await Assert.That(request.IsPensioner).IsTrue();
    }

    [Test]
    public async Task Calculate_With_ResultMonths_Should_PropagateFailure()
    {
        var calculator = CreateCalculator();
        var failedMonths = Result<IReadOnlyList<MonthlyInput>>.Failure(
            ErrorCode.InvalidMonthCount, "Test failure");

        var result = calculator.UseNetToGross
            .ForYear(2024)
            .Calculate(failedMonths);

        await Assert.That(result.IsFailure).IsTrue();
    }
}
