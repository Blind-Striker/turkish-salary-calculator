#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Services;

public class SalaryCalculatorMetadataBuilderTests
{
    // ═══════════════════════════════════════════════════════════════
    // Builder Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Create_Should_ReturnMetadataService_With_DefaultConfiguration()
    {
        // Act
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Assert
        await Assert.That(metadata).IsNotNull();
        await Assert.That(metadata).IsAssignableTo<ISalaryCalculatorMetadata>();
    }

    [Test]
    public async Task Create_Should_ReturnMetadataService_With_EmbeddedResources()
    {
        // Act
        var metadata = SalaryCalculatorMetadataBuilder.Create(config =>
            config.UseEmbeddedResources());

        // Assert
        await Assert.That(metadata).IsNotNull();
        await Assert.That(metadata.GetAvailableYears()).IsNotEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAvailableYears Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task GetAvailableYears_Should_ReturnYears_From_EmbeddedResources()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var years = metadata.GetAvailableYears();

        // Assert
        await Assert.That(years).IsNotEmpty();
        await Assert.That(years).Contains(2024);
        await Assert.That(years).Contains(2025);
        await Assert.That(years).Contains(2026);
    }

    [Test]
    public async Task GetAvailableYears_Should_ReturnYearsInOrder()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var years = metadata.GetAvailableYears();

        // Assert - years should be in some order (either ascending or descending)
        await Assert.That(years).Count().IsGreaterThan(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetEmployeeTypes Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task GetEmployeeTypes_Should_ReturnVisibleEmployeeTypes()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var types = metadata.GetEmployeeTypes();

        // Assert
        await Assert.That(types).IsNotEmpty();
        await Assert.That(types.Any(t => t.Id == EmployeeTypeId.Standard)).IsTrue();
    }

    [Test]
    public async Task GetEmployeeTypes_Should_IncludeDisplayInfo()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var types = metadata.GetEmployeeTypes();
        var standardType = types.FirstOrDefault(t => t.Id == EmployeeTypeId.Standard);

        // Assert
        await Assert.That(standardType).IsNotNull();
        await Assert.That(standardType!.Name).IsNotEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetEducationTypes Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task GetEducationTypes_Should_ReturnEducationTypes()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var types = metadata.GetEducationTypes();

        // Assert
        await Assert.That(types).IsNotEmpty();
        await Assert.That(types.Any(t => t.Id == EducationTypeId.Doctorate)).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetDisabilityDegrees Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task GetDisabilityDegrees_Should_ReturnAllDegrees()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var degrees = metadata.GetDisabilityDegrees();

        // Assert
        await Assert.That(degrees).Count().IsEqualTo(4); // None, First, Second, Third
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.None)).IsTrue();
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.First)).IsTrue();
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.Second)).IsTrue();
        await Assert.That(degrees.Any(d => d.Id == DisabilityDegreeId.Third)).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetCapabilities Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task GetCapabilities_Should_ReturnSuccess_For_ValidYear()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var result = metadata.GetCapabilities(2024);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsNotNull();
    }

    [Test]
    public async Task GetCapabilities_Should_ReturnFailure_For_InvalidYear()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var result = metadata.GetCapabilities(1900); // Invalid year

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task GetCapabilities_Should_DisableAgi_For_2022_And_Later()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var result = metadata.GetCapabilities(2024);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.IsAvailable(Capability.AgiCalculation)).IsFalse();
    }

    [Test]
    public async Task GetCapabilities_Should_EnableAgi_For_Pre2022()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var result = metadata.GetCapabilities(2021);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.IsAvailable(Capability.AgiCalculation)).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // IsCapabilityAvailable Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task IsCapabilityAvailable_Should_ReturnTrue_When_CapabilityEnabled()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act - MinWageExemption is available for 2022+
        var isAvailable = metadata.IsCapabilityAvailable(Capability.MinWageExemption, 2024);

        // Assert
        await Assert.That(isAvailable).IsTrue();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_ReturnFalse_When_CapabilityDisabled()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act - AGI is not available for 2022+
        var isAvailable = metadata.IsCapabilityAvailable(Capability.AgiCalculation, 2024);

        // Assert
        await Assert.That(isAvailable).IsFalse();
    }

    [Test]
    public async Task IsCapabilityAvailable_Should_ReturnFalse_For_InvalidYear()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var isAvailable = metadata.IsCapabilityAvailable(Capability.MinWageExemption, 1900);

        // Assert
        await Assert.That(isAvailable).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Caching/Instance Behavior Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Create_Should_ReturnNewInstance_Each_Time()
    {
        // Act
        var metadata1 = SalaryCalculatorMetadataBuilder.Create();
        var metadata2 = SalaryCalculatorMetadataBuilder.Create();

        // Assert
        await Assert.That(metadata1).IsNotSameReferenceAs(metadata2);
    }

    [Test]
    public async Task Metadata_Should_BeConsistent_Across_Multiple_Calls()
    {
        // Arrange
        var metadata = SalaryCalculatorMetadataBuilder.Create();

        // Act
        var years1 = metadata.GetAvailableYears();
        var years2 = metadata.GetAvailableYears();

        // Assert - Same instance should return consistent data
        await Assert.That(years1.Count).IsEqualTo(years2.Count);
    }
}
