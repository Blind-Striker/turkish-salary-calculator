#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Builders;

public class BuilderConsistencyTests
{
    private static ISalaryCalculator CreateCalculator() => SalaryCalculatorBuilder.Create();

    [Test]
    public async Task GrossToNetBuilder_Should_ProduceSameRequest_As_RequestBuilder_With_MinimalConfig()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(50_000m);

        var fromModeBuilder = calculator.UseGrossToNet
            .ForYear(2024)
            .Build(months);

        var fromRequestBuilder = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(months)
            .Build();

        await Assert.That(fromModeBuilder).IsEqualTo(fromRequestBuilder);
    }

    [Test]
    public async Task GrossToNetBuilder_Should_ProduceSameRequest_As_RequestBuilder_With_FullConfig()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(50_000m);
        var agi = new AgiSettings(SpouseStatus.SpouseNotWorking, 2, true);
        var rnd = new RnDSettings(EducationTypeId.Doctorate);

        var fromModeBuilder = calculator.UseGrossToNet
            .ForYear(2021)
            .WithEmployeeType(EmployeeTypeId.RnD5746)
            .WithDisability(DisabilityDegreeId.First)
            .AsPensioner()
            .WithMinWageExemption(false)
            .With5746Discount()
            .WithAgi(a => a.SpouseNotWorking().WithChildren(2).IncludeInTax())
            .WithRnDSettings(r => r.WithEducation(EducationTypeId.Doctorate))
            .Build(months);

        var fromRequestBuilder = SalaryCalculationRequest.For(2021)
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(months)
            .WithEmployeeType(EmployeeTypeId.RnD5746)
            .WithDisability(DisabilityDegreeId.First)
            .AsPensioner()
            .WithMinWageExemption(false)
            .With5746Discount()
            .WithAgi(agi)
            .WithRnD(rnd)
            .Build();

        await Assert.That(fromModeBuilder).IsEqualTo(fromRequestBuilder);
    }

    [Test]
    public async Task NetToGrossBuilder_Should_ProduceSameRequest_As_RequestBuilder_With_MinimalConfig()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(30_000m);

        var fromModeBuilder = calculator.UseNetToGross
            .ForYear(2024)
            .Build(months);

        var fromRequestBuilder = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.NetToGross)
            .WithMonths(months)
            .Build();

        await Assert.That(fromModeBuilder).IsEqualTo(fromRequestBuilder);
    }

    [Test]
    public async Task NetToGrossBuilder_Should_ProduceSameRequest_As_RequestBuilder_With_AgiIncludedInNet()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(30_000m);

        var fromModeBuilder = calculator.UseNetToGross
            .ForYear(2021)
            .WithAgiIncludedInNet()
            .Build(months);

        var fromRequestBuilder = SalaryCalculationRequest.For(2021)
            .WithMode(CalculationMode.NetToGross)
            .WithMonths(months)
            .WithAgiIncludedInNet()
            .Build();

        await Assert.That(fromModeBuilder).IsEqualTo(fromRequestBuilder);
    }

    [Test]
    public async Task TotalToGrossBuilder_Should_ProduceSameRequest_As_RequestBuilder_With_MinimalConfig()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(80_000m);

        var fromModeBuilder = calculator.UseTotalToGross
            .ForYear(2024)
            .Build(months);

        var fromRequestBuilder = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.TotalToGross)
            .WithMonths(months)
            .Build();

        await Assert.That(fromModeBuilder).IsEqualTo(fromRequestBuilder);
    }

    [Test]
    public async Task GrossToNet_Calculation_Should_ProduceSameResult_Regardless_Of_BuilderPath()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(50_000m);

        var resultA = calculator.UseGrossToNet
            .ForYear(2024)
            .Calculate(months);

        var request = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(months)
            .Build();
        var resultB = calculator.Calculate(request);

        await Assert.That(resultA.IsSuccess).IsTrue();
        await Assert.That(resultB.IsSuccess).IsTrue();
        await Assert.That(resultA.Value.NetSalary).IsEqualTo(resultB.Value.NetSalary);
        await Assert.That(resultA.Value.CalculatedGrossSalary).IsEqualTo(resultB.Value.CalculatedGrossSalary);
        await Assert.That(resultA.Value.EmployerTotalCost).IsEqualTo(resultB.Value.EmployerTotalCost);
    }

    [Test]
    public async Task NetToGross_Calculation_Should_ProduceSameResult_Regardless_Of_BuilderPath()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(30_000m);

        var resultA = calculator.UseNetToGross
            .ForYear(2024)
            .Calculate(months);

        var request = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.NetToGross)
            .WithMonths(months)
            .Build();
        var resultB = calculator.Calculate(request);

        await Assert.That(resultA.IsSuccess).IsTrue();
        await Assert.That(resultB.IsSuccess).IsTrue();
        await Assert.That(resultA.Value.NetSalary).IsEqualTo(resultB.Value.NetSalary);
        await Assert.That(resultA.Value.CalculatedGrossSalary).IsEqualTo(resultB.Value.CalculatedGrossSalary);
        await Assert.That(resultA.Value.EmployerTotalCost).IsEqualTo(resultB.Value.EmployerTotalCost);
    }

    [Test]
    public async Task TotalToGross_Calculation_Should_ProduceSameResult_Regardless_Of_BuilderPath()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(80_000m);

        var resultA = calculator.UseTotalToGross
            .ForYear(2024)
            .Calculate(months);

        var request = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.TotalToGross)
            .WithMonths(months)
            .Build();
        var resultB = calculator.Calculate(request);

        await Assert.That(resultA.IsSuccess).IsTrue();
        await Assert.That(resultB.IsSuccess).IsTrue();
        await Assert.That(resultA.Value.NetSalary).IsEqualTo(resultB.Value.NetSalary);
        await Assert.That(resultA.Value.CalculatedGrossSalary).IsEqualTo(resultB.Value.CalculatedGrossSalary);
        await Assert.That(resultA.Value.EmployerTotalCost).IsEqualTo(resultB.Value.EmployerTotalCost);
    }

    [Test]
    public async Task FullConfig_Calculation_Should_ProduceSameResult_Regardless_Of_BuilderPath()
    {
        var calculator = CreateCalculator();
        var months = MonthlyInput.Uniform(80_000m);

        var resultA = calculator.UseGrossToNet
            .ForYear(2024)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .WithDisability(DisabilityDegreeId.Second)
            .With5746Discount()
            .WithRnDSettings(r => r.WithEducation(EducationTypeId.MastersOrFundamentalSciences))
            .Calculate(months);

        var request = SalaryCalculationRequest.For(2024)
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(months)
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .WithDisability(DisabilityDegreeId.Second)
            .With5746Discount()
            .WithRnD(new RnDSettings(EducationTypeId.MastersOrFundamentalSciences))
            .Build();
        var resultB = calculator.Calculate(request);

        await Assert.That(resultA.IsSuccess).IsTrue();
        await Assert.That(resultB.IsSuccess).IsTrue();
        await Assert.That(resultA.Value.NetSalary).IsEqualTo(resultB.Value.NetSalary);
        await Assert.That(resultA.Value.EmployerTotalCost).IsEqualTo(resultB.Value.EmployerTotalCost);
        await Assert.That(resultA.Value.EmployerIncomeTaxExemptionAmount).IsEqualTo(resultB.Value.EmployerIncomeTaxExemptionAmount);
    }
}
