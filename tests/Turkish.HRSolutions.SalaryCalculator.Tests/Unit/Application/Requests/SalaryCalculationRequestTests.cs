#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Requests;

public class SalaryCalculationRequestTests
{
    // ═══════════════════════════════════════════════════════════════
    // Record Construction Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Constructor_Should_SetAllProperties_When_Provided()
    {
        // Arrange
        var months = MonthlyInput.Uniform(50_000m);
        var agi = new AgiSettings(SpouseStatus.SpouseNotWorking, 2, false);
        var rnd = new RnDSettings(EducationTypeId.Doctorate);

        // Act
        var request = new SalaryCalculationRequest(
            CalculationMode.NetToGross,
            2025,
            months,
            EmployeeTypeId.Teknokent4691,
            DisabilityDegreeId.First,
            IsPensioner: true,
            ApplyMinWageExemption: false,
            Apply5746Discount: true,
            agi,
            rnd,
            IsAgiIncludedInNet: true);

        // Assert
        await Assert.That(request.Mode).IsEqualTo(CalculationMode.NetToGross);
        await Assert.That(request.Year).IsEqualTo(2025);
        await Assert.That(request.Months).IsEqualTo(months);
        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.Teknokent4691);
        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.First);
        await Assert.That(request.IsPensioner).IsTrue();
        await Assert.That(request.ApplyMinWageExemption).IsFalse();
        await Assert.That(request.Apply5746Discount).IsTrue();
        await Assert.That(request.Agi).IsEqualTo(agi);
        await Assert.That(request.RnD).IsEqualTo(rnd);
        await Assert.That(request.IsAgiIncludedInNet).IsTrue();
    }

    [Test]
    public async Task Constructor_Should_DefaultEmployeeType_To_Standard_When_NotSpecified()
    {
        // Arrange & Act
        var request = new SalaryCalculationRequest(
            CalculationMode.GrossToNet,
            2025,
            MonthlyInput.Uniform(50_000m));

        // Assert
        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.Standard);
    }

    [Test]
    public async Task Constructor_Should_DefaultDisability_To_None_When_NotSpecified()
    {
        // Arrange & Act
        var request = new SalaryCalculationRequest(
            CalculationMode.GrossToNet,
            2025,
            MonthlyInput.Uniform(50_000m));

        // Assert
        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.None);
    }

    [Test]
    public async Task Constructor_Should_DefaultApplyMinWageExemption_To_True()
    {
        // Arrange & Act
        var request = new SalaryCalculationRequest(
            CalculationMode.GrossToNet,
            2025,
            MonthlyInput.Uniform(50_000m));

        // Assert
        await Assert.That(request.ApplyMinWageExemption).IsTrue();
    }

    [Test]
    public async Task Constructor_Should_DefaultOptionalFlags_To_False()
    {
        // Arrange & Act
        var request = new SalaryCalculationRequest(
            CalculationMode.GrossToNet,
            2025,
            MonthlyInput.Uniform(50_000m));

        // Assert
        await Assert.That(request.IsPensioner).IsFalse();
        await Assert.That(request.Apply5746Discount).IsFalse();
        await Assert.That(request.IsAgiIncludedInNet).IsFalse();
    }

    [Test]
    public async Task Constructor_Should_DefaultOptionalSettings_To_Null()
    {
        // Arrange & Act
        var request = new SalaryCalculationRequest(
            CalculationMode.GrossToNet,
            2025,
            MonthlyInput.Uniform(50_000m));

        // Assert
        await Assert.That(request.Agi).IsNull();
        await Assert.That(request.RnD).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - Basic
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task For_Should_CreateBuilder_With_Year()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .Build();

        // Assert
        await Assert.That(request.Year).IsEqualTo(2025);
    }

    [Test]
    public async Task Builder_Should_DefaultMode_To_GrossToNet()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .Build();

        // Assert
        await Assert.That(request.Mode).IsEqualTo(CalculationMode.GrossToNet);
    }

    [Test]
    public async Task Build_Should_ThrowInvalidOperation_When_MonthsNotSpecified()
    {
        // Arrange
        var builder = SalaryCalculationRequest.For(2025);

        // Act & Assert
        await Assert.That(builder.Build)
            .Throws<InvalidOperationException>()
            .WithMessageContaining("Monthly input data is required", StringComparison.Ordinal);
    }

    [Test]
    public async Task Build_Should_ThrowInvalidOperation_When_MonthsEmpty()
    {
        // Arrange
        var builder = SalaryCalculationRequest.For(2025)
            .WithMonths([]);

        // Act & Assert
        await Assert.That(builder.Build)
            .Throws<InvalidOperationException>()
            .WithMessageContaining("Monthly input data is required", StringComparison.Ordinal);
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - Mode Setting
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [Arguments(CalculationMode.GrossToNet)]
    [Arguments(CalculationMode.NetToGross)]
    [Arguments(CalculationMode.TotalToGross)]
    public async Task WithMode_Should_SetMode(CalculationMode mode)
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMode(mode)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .Build();

        // Assert
        await Assert.That(request.Mode).IsEqualTo(mode);
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - Employee Configuration
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task WithEmployeeType_Should_SetEmployeeType()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .WithEmployeeType(EmployeeTypeId.Teknokent4691)
            .Build();

        // Assert
        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.Teknokent4691);
    }

    [Test]
    public async Task WithDisability_Should_SetDisability()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .WithDisability(DisabilityDegreeId.Second)
            .Build();

        // Assert
        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.Second);
    }

    [Test]
    public async Task AsPensioner_Should_SetIsPensionerTrue_By_Default()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .AsPensioner()
            .Build();

        // Assert
        await Assert.That(request.IsPensioner).IsTrue();
    }

    [Test]
    public async Task AsPensioner_Should_SetIsPensionerFalse_When_Specified()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .AsPensioner(false)
            .Build();

        // Assert
        await Assert.That(request.IsPensioner).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - Tax/Discount Settings
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task WithMinWageExemption_Should_SetApplyMinWageExemption()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .WithMinWageExemption(false)
            .Build();

        // Assert
        await Assert.That(request.ApplyMinWageExemption).IsFalse();
    }

    [Test]
    public async Task With5746Discount_Should_SetApply5746Discount()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .With5746Discount()
            .Build();

        // Assert
        await Assert.That(request.Apply5746Discount).IsTrue();
    }

    [Test]
    public async Task With5746Discount_Should_SetApply5746DiscountFalse_When_Specified()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .With5746Discount(false)
            .Build();

        // Assert
        await Assert.That(request.Apply5746Discount).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - AGI Settings
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task WithAgi_Should_SetAgiSettings()
    {
        // Arrange
        var agi = new AgiSettings(SpouseStatus.SpouseNotWorking, 3, true);

        // Act
        var request = SalaryCalculationRequest.For(2021) // Pre-2022 year
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(MonthlyInput.Uniform(30_000m))
            .WithAgi(agi)
            .Build();

        // Assert
        await Assert.That(request.Agi).IsNotNull();
        await Assert.That(request.Agi!.SpouseStatus).IsEqualTo(SpouseStatus.SpouseNotWorking);
        await Assert.That(request.Agi.NumberOfChildren).IsEqualTo(3);
        await Assert.That(request.Agi.IncludeInTax).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - R&D Settings
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task WithRnD_Should_SetRnDSettings()
    {
        // Arrange
        var rnd = new RnDSettings(EducationTypeId.Doctorate);

        // Act
        var request = SalaryCalculationRequest.For(2025)
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(MonthlyInput.Uniform(80_000m))
            .WithEmployeeType(EmployeeTypeId.RnD5746)
            .WithRnD(rnd)
            .Build();

        // Assert
        await Assert.That(request.RnD).IsNotNull();
        await Assert.That(request.RnD!.Education).IsEqualTo(EducationTypeId.Doctorate);
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - NetToGross Specific
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task WithAgiIncludedInNet_Should_SetIsAgiIncludedInNetTrue_By_Default()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2021)
            .WithMode(CalculationMode.NetToGross)
            .WithMonths(MonthlyInput.Uniform(25_000m))
            .WithAgiIncludedInNet()
            .Build();

        // Assert
        await Assert.That(request.IsAgiIncludedInNet).IsTrue();
    }

    [Test]
    public async Task WithAgiIncludedInNet_Should_SetIsAgiIncludedInNetFalse_When_Specified()
    {
        // Arrange & Act
        var request = SalaryCalculationRequest.For(2021)
            .WithMode(CalculationMode.NetToGross)
            .WithMonths(MonthlyInput.Uniform(25_000m))
            .WithAgiIncludedInNet(false)
            .Build();

        // Assert
        await Assert.That(request.IsAgiIncludedInNet).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Builder Tests - Fluent Chaining
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Builder_Should_SupportFullFluentChain()
    {
        // Arrange
        var agi = new AgiSettings(SpouseStatus.SpouseWorking, 1, false);
        var rnd = new RnDSettings(EducationTypeId.MastersOrFundamentalSciences);

        // Act
        var request = SalaryCalculationRequest.For(2021)
            .WithMode(CalculationMode.NetToGross)
            .WithMonths(MonthlyInput.Uniform(40_000m))
            .WithEmployeeType(EmployeeTypeId.RnD5746)
            .WithDisability(DisabilityDegreeId.Third)
            .AsPensioner()
            .WithMinWageExemption(false)
            .With5746Discount()
            .WithAgi(agi)
            .WithRnD(rnd)
            .WithAgiIncludedInNet()
            .Build();

        // Assert
        await Assert.That(request.Year).IsEqualTo(2021);
        await Assert.That(request.Mode).IsEqualTo(CalculationMode.NetToGross);
        await Assert.That(request.Months.Count).IsEqualTo(12);
        await Assert.That(request.EmployeeType).IsEqualTo(EmployeeTypeId.RnD5746);
        await Assert.That(request.Disability).IsEqualTo(DisabilityDegreeId.Third);
        await Assert.That(request.IsPensioner).IsTrue();
        await Assert.That(request.ApplyMinWageExemption).IsFalse();
        await Assert.That(request.Apply5746Discount).IsTrue();
        await Assert.That(request.Agi).IsEqualTo(agi);
        await Assert.That(request.RnD).IsEqualTo(rnd);
        await Assert.That(request.IsAgiIncludedInNet).IsTrue();
    }

    [Test]
    public async Task Builder_Should_AllowOverridingPreviousValues()
    {
        // Arrange & Act - Set mode twice, last one wins
        var request = SalaryCalculationRequest.For(2025)
            .WithMode(CalculationMode.GrossToNet)
            .WithMode(CalculationMode.TotalToGross)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .Build();

        // Assert
        await Assert.That(request.Mode).IsEqualTo(CalculationMode.TotalToGross);
    }

    // ═══════════════════════════════════════════════════════════════
    // Record Equality Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Record_Should_BeEqual_When_SameValues()
    {
        // Arrange
        var months = MonthlyInput.Uniform(50_000m);
        var request1 = new SalaryCalculationRequest(CalculationMode.GrossToNet, 2025, months);
        var request2 = new SalaryCalculationRequest(CalculationMode.GrossToNet, 2025, months);

        // Assert
        await Assert.That(request1).IsEqualTo(request2);
        await Assert.That(request1.GetHashCode()).IsEqualTo(request2.GetHashCode());
    }

    [Test]
    public async Task Record_Should_NotBeEqual_When_DifferentMode()
    {
        // Arrange
        var months = MonthlyInput.Uniform(50_000m);
        var request1 = new SalaryCalculationRequest(CalculationMode.GrossToNet, 2025, months);
        var request2 = new SalaryCalculationRequest(CalculationMode.NetToGross, 2025, months);

        // Assert
        await Assert.That(request1).IsNotEqualTo(request2);
    }

    [Test]
    public async Task Record_Should_SupportWith_Expression()
    {
        // Arrange
        var original = SalaryCalculationRequest.For(2025)
            .WithMode(CalculationMode.GrossToNet)
            .WithMonths(MonthlyInput.Uniform(50_000m))
            .Build();

        // Act
        var modified = original with { Mode = CalculationMode.NetToGross };

        // Assert
        await Assert.That(modified.Mode).IsEqualTo(CalculationMode.NetToGross);
        await Assert.That(modified.Year).IsEqualTo(original.Year);
        await Assert.That(modified.Months).IsEqualTo(original.Months);
    }
}
