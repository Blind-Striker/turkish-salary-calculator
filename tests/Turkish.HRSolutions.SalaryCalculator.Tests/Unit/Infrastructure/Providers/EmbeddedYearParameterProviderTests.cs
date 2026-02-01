#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Providers;

public class EmbeddedYearParameterProviderTests
{
    [Test]
    public async Task Constructor_Should_InitializeLazily()
    {
        // Act
        var provider = new EmbeddedYearParameterProvider();

        // Assert
        await Assert.That(provider).IsNotNull();
        // Accessing properties will trigger loading, so we just check object creation here
    }

    [Test]
    public async Task AvailableYears_Should_Contain_Supported_Years()
    {
        // Arrange
        var provider = new EmbeddedYearParameterProvider();

        // Act
        var years = provider.AvailableYears;

        // Assert
        await Assert.That(years).IsNotEmpty();
        await Assert.That(years).Contains(2025);
        await Assert.That(years).Contains(2026);
    }

    [Test]
    public async Task GetParameter_Should_ReturnParameter_When_YearExists()
    {
        // Arrange
        var provider = new EmbeddedYearParameterProvider();

        // Act
        var param = provider.GetParameter(2026);

        // Assert
        await Assert.That(param).IsNotNull();
        await Assert.That(param!.Year).IsEqualTo(2026);
    }

    [Test]
    public async Task GetParameter_Should_ReturnNull_When_YearDoesNotExist()
    {
        // Arrange
        var provider = new EmbeddedYearParameterProvider();

        // Act
        var param = provider.GetParameter(1900);

        // Assert
        await Assert.That(param).IsNull();
    }
}
