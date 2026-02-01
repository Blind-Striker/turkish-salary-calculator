using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Exceptions;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

namespace Turkish.HRSolutions.SalaryCalculator;

/// <summary>
/// Static entry point for the Turkish Salary Calculator.
/// Builds and configures calculator instances using the fluent configurator pattern.
/// </summary>
public static class SalaryCalculatorBuilder
{
    /// <summary>
    /// Creates and configures a new instance of the salary calculator service.
    /// </summary>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>An initialized ISalaryCalculator instance.</returns>
    /// <example>
    /// <code>
    /// // Default (Embedded)
    /// var calculator = SalaryCalculatorBuilder.Create();
    ///
    /// // Custom
    /// var calculator = SalaryCalculatorBuilder.Create(config =>
    ///     config.UseFileSystem("years.json", "constants.json"));
    /// </code>
    /// </example>
    /// <exception cref="SalaryCalculatorConfigurationException">Thrown if salary calculator providers are invalid.</exception>
    public static ISalaryCalculator Create(Action<IStandaloneCalculatorConfigurator>? configure = null)
    {
        var configurator = new StandaloneCalculatorConfigurator();
        configurator.UseEmbeddedResources();
        configure?.Invoke(configurator);

        var (yearProvider, constantsProvider) = configurator.Build();

        // Create services in dependency order
        var capabilityResolver = new CapabilityResolver(yearProvider, constantsProvider);
        var providerValidation = capabilityResolver.ValidateProviders();

        if (providerValidation.IsFailure)
        {
            var errors = providerValidation.Errors.Select(e => $"{e.Code}: {e.Message}");
            throw new SalaryCalculatorConfigurationException(
                $"Salary calculator providers are invalid: {string.Join("; ", errors)}");
        }

        var validationEngine = new ValidationEngine(capabilityResolver, yearProvider, constantsProvider);
        return new SalaryCalculatorService(validationEngine, yearProvider, constantsProvider);
    }
}
