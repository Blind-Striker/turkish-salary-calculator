using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Configuration;

public class ServiceCollectionCalculatorConfiguratorTests
{
    [Test]
    public async Task UseEmbeddedResources_Should_RegisterEmbeddedProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurator = new ServiceCollectionCalculatorConfigurator(services);

        // Act
        configurator.UseEmbeddedResources();

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var yearProvider = serviceProvider.GetService<IYearParameterProvider>();
        var constantsProvider = serviceProvider.GetService<ICalculationConstantsProvider>();

        await Assert.That(yearProvider).IsTypeOf<EmbeddedYearParameterProvider>();
        await Assert.That(constantsProvider).IsTypeOf<EmbeddedCalculationConstantsProvider>();
    }

    [Test]
    public async Task UseFileSystem_Should_RegisterFileSystemProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurator = new ServiceCollectionCalculatorConfigurator(services);

        // Act
        configurator.UseFileSystem("years.json", "constants.json");

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var yearProvider = serviceProvider.GetService<IYearParameterProvider>();
        var constantsProvider = serviceProvider.GetService<ICalculationConstantsProvider>();

        await Assert.That(yearProvider).IsTypeOf<FileSystemYearParameterProvider>();
        await Assert.That(constantsProvider).IsTypeOf<FileSystemCalculationConstantsProvider>();
    }

    /// <summary>
    /// Custom year provider instantiated by DI container via WithCustomYearProvider&lt;T&gt;().
    /// </summary>
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    private sealed class CustomYearProvider : IYearParameterProvider
    {
        public IReadOnlyList<int> AvailableYears => [];
        public YearParameter? GetParameter(int year) => null;
        public bool HasYear(int year) => false;
    }

    [Test]
    public async Task WithCustomYearProvider_Should_RegisterCustomProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurator = new ServiceCollectionCalculatorConfigurator(services);

        // Act
        configurator.WithCustomYearProvider<CustomYearProvider>();

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var yearProvider = serviceProvider.GetService<IYearParameterProvider>();

        await Assert.That(yearProvider).IsTypeOf<CustomYearProvider>();
    }
}
