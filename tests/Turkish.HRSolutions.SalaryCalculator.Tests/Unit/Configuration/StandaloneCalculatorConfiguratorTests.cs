using Turkish.HRSolutions.SalaryCalculator.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Configuration;

public class StandaloneCalculatorConfiguratorTests
{
    [Test]
    public async Task UseEmbeddedResources_Should_ConfigureEmbeddedProviders()
    {
        // Arrange
        var configurator = new StandaloneCalculatorConfigurator();

        // Act
        configurator.UseEmbeddedResources();
        var (yearProvider, constantsProvider) = configurator.Build();

        // Assert
        await Assert.That(yearProvider).IsTypeOf<EmbeddedYearParameterProvider>();
        await Assert.That(constantsProvider).IsTypeOf<EmbeddedCalculationConstantsProvider>();
    }

    [Test]
    public async Task UseFileSystem_Should_ConfigureFileSystemProviders()
    {
        // Arrange
        var configurator = new StandaloneCalculatorConfigurator();

        // Act
        configurator.UseFileSystem("years.json", "constants.json");
        var (yearProvider, constantsProvider) = configurator.Build();

        // Assert
        await Assert.That(yearProvider).IsTypeOf<FileSystemYearParameterProvider>();
        await Assert.That(constantsProvider).IsTypeOf<FileSystemCalculationConstantsProvider>();
    }

    private sealed class CustomYearProvider : IYearParameterProvider
    {
        public IReadOnlyList<int> AvailableYears => [];
        public YearParameter? GetParameter(int year) => null;
        public bool HasYear(int year) => false;
    }

    [Test]
    public async Task WithCustomYearProvider_Should_UseProvidedInstance()
    {
        // Arrange
        var configurator = new StandaloneCalculatorConfigurator();
        var custom = new CustomYearProvider();

        // Act
        configurator.WithCustomYearProvider(custom);
        var (yearProvider, _) = configurator.Build();

        // Assert
        await Assert.That(yearProvider).IsSameReferenceAs(custom);
    }
}
