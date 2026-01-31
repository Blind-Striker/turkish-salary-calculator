using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Tests for <see cref="SalaryCalculatorMetadataService"/>.
/// </summary>
public sealed class SalaryCalculatorMetadataServiceTests
{
    private static IYearParameterProvider YearProvider { get; } = new EmbeddedYearParameterProvider();
    private static ICalculationConstantsProvider ConstantsProvider { get; } = new EmbeddedCalculationConstantsProvider();
    private static ValidationEngine Engine { get; } = new();

    private static SalaryCalculatorMetadataService CreateService() =>
        new(Engine, YearProvider, ConstantsProvider);

    [Test]
    public async Task GetAvailableYears_Should_ReturnNonEmptyList()
    {
        var service = CreateService();

        var years = service.GetAvailableYears();

        await Assert.That(years).IsNotNull();
        await Assert.That(years.Count > 0).IsTrue();
    }

    [Test]
    public async Task GetAvailableYears_Should_IncludeRecentYears()
    {
        var service = CreateService();

        var years = service.GetAvailableYears();

        await Assert.That(years.Contains(2024)).IsTrue();
        await Assert.That(years.Contains(2025)).IsTrue();
    }

    [Test]
    public async Task GetEmployeeTypes_Should_ReturnNonEmptyList()
    {
        var service = CreateService();

        var types = service.GetEmployeeTypes();

        await Assert.That(types).IsNotNull();
        await Assert.That(types.Count > 0).IsTrue();
    }

    [Test]
    public async Task GetEmployeeTypes_Should_IncludeStandardEmployee()
    {
        var service = CreateService();

        var types = service.GetEmployeeTypes();

        var standard = types.FirstOrDefault(t => t.Id == EmployeeTypeId.Standard);
        await Assert.That(standard).IsNotNull();
    }

    [Test]
    public async Task GetEmployeeTypes_Should_OnlyIncludeVisibleTypes()
    {
        var service = CreateService();

        var types = service.GetEmployeeTypes();

        // All types should have non-empty names (visible types have names)
        await Assert.That(types.All(t => !string.IsNullOrWhiteSpace(t.Name))).IsTrue();
    }

    [Test]
    public async Task GetEducationTypes_Should_ReturnNonEmptyList()
    {
        var service = CreateService();

        var types = service.GetEducationTypes();

        await Assert.That(types).IsNotNull();
        await Assert.That(types.Count > 0).IsTrue();
    }

    [Test]
    public async Task GetEducationTypes_Should_IncludeOtherRnDPersonnel()
    {
        var service = CreateService();

        var types = service.GetEducationTypes();

        var other = types.FirstOrDefault(t => t.Id == EducationTypeId.OtherRnDPersonnel);
        await Assert.That(other).IsNotNull();
    }

    [Test]
    public async Task GetDisabilityDegrees_Should_ReturnFourDegrees()
    {
        var service = CreateService();

        var degrees = service.GetDisabilityDegrees();

        await Assert.That(degrees.Count).IsEqualTo(4);
    }

    [Test]
    public async Task GetDisabilityDegrees_Should_IncludeNone()
    {
        var service = CreateService();

        var degrees = service.GetDisabilityDegrees();

        var none = degrees.FirstOrDefault(d => d.Id == DisabilityDegreeId.None);
        await Assert.That(none).IsNotNull();
    }

    [Test]
    public async Task GetDisabilityDegrees_Should_IncludeAllDegrees()
    {
        var service = CreateService();

        var degrees = service.GetDisabilityDegrees();

        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.None)).IsTrue();
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.First)).IsTrue();
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.Second)).IsTrue();
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.Third)).IsTrue();
    }

    [Test]
    public async Task GetCapabilities_Should_ReturnSuccess_When_ValidYear()
    {
        var service = CreateService();

        var result = service.GetCapabilities(2024);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsNotNull();
    }

    [Test]
    public async Task GetCapabilities_Should_ReturnFailure_When_InvalidYear()
    {
        var service = CreateService();

        var result = service.GetCapabilities(1900);

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task GetCapabilities_Should_DisableAgiSelectionFor2024()
    {
        var service = CreateService();

        var result = service.GetCapabilities(2024);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.IsAvailable(Capability.AgiSelection)).IsFalse();
    }

    [Test]
    public async Task GetCapabilities_Should_EnableAgiSelectionFor2021()
    {
        var service = CreateService();

        var result = service.GetCapabilities(2021);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.IsAvailable(Capability.AgiSelection)).IsTrue();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_ReturnFalse_When_CapabilityDisabled()
    {
        var service = CreateService();

        // 2024 has min wage exemption, so AGI is disabled
        var result = service.IsCapabilityAvailable(Capability.AgiSelection, 2024);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_ReturnTrue_When_CapabilityEnabled()
    {
        var service = CreateService();

        // 2024 still allows min wage exemption toggle
        var result = service.IsCapabilityAvailable(Capability.MinWageExemption, 2024);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_ReturnFalse_When_YearNotSupported()
    {
        var service = CreateService();

        var result = service.IsCapabilityAvailable(Capability.AgiSelection, 1900);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_Consider_EmployeeType()
    {
        var service = CreateService();

        // Teknokent type (2) supports R&D
        var withRnD = service.IsCapabilityAvailable(Capability.RnDDaysInput, 2024, EmployeeTypeId.Teknokent4691);
        var withoutRnD = service.IsCapabilityAvailable(Capability.RnDDaysInput, 2024, EmployeeTypeId.Standard);

        await Assert.That(withRnD).IsTrue();
        await Assert.That(withoutRnD).IsFalse();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_Consider_PensionerStatus()
    {
        var service = CreateService();

        // Pensioners cannot use 5746 discount
        var notPensioner = service.IsCapabilityAvailable(Capability.Discount5746, 2024, isPensioner: false);
        var pensioner = service.IsCapabilityAvailable(Capability.Discount5746, 2024, isPensioner: true);

        await Assert.That(notPensioner).IsTrue();
        await Assert.That(pensioner).IsFalse();
    }
}
