using Microsoft.Extensions.DependencyInjection;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.DependencyInjection;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddSalaryCalculator_Should_RegisterDefaults_When_NoActionProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSalaryCalculator();

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var yearProvider = serviceProvider.GetService<IYearParameterProvider>();
        var constantsProvider = serviceProvider.GetService<ICalculationConstantsProvider>();
        var metadata = serviceProvider.GetService<Turkish.HRSolutions.SalaryCalculator.Application.Calculator.ISalaryCalculatorMetadata>();
        var validationEngine = serviceProvider.GetService<IValidationEngine>();

        await Assert.That(yearProvider).IsTypeOf<EmbeddedYearParameterProvider>();
        await Assert.That(constantsProvider).IsTypeOf<EmbeddedCalculationConstantsProvider>();
        await Assert.That(metadata).IsNotNull();
        await Assert.That(validationEngine).IsNotNull();
    }

    [Test]
    public async Task AddSalaryCalculator_Should_ApplyConfiguration_When_ActionProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSalaryCalculator(config => config.UseFileSystem("y.json", "c.json"));

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var yearProvider = serviceProvider.GetService<IYearParameterProvider>();

        await Assert.That(yearProvider).IsTypeOf<FileSystemYearParameterProvider>();
    }

    [Test]
    public async Task AddSalaryCalculator_Should_ThrowInvalidOperationException_When_ConfigActionProvided_But_NoProvidersRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        await Assert.That(() => services.AddSalaryCalculator(_ =>
        {
            // User provides a configure action but doesn't call any methods to register providers
        })).Throws<InvalidOperationException>();
    }
}
