using Turkish.HRSolutions.SalaryCalculator.Application.Calculator;
using Turkish.HRSolutions.SalaryCalculator.Application.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;

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
    public static ISalaryCalculator Create(Action<IStandaloneCalculatorConfigurator>? configure = null)
    {
        var configurator = new StandaloneCalculatorConfigurator();
        configurator.UseEmbeddedResources();
        configure?.Invoke(configurator);

        var (yearProvider, constantsProvider) = configurator.Build();

        var validationEngine = new ValidationEngine();
        var providerValidation = validationEngine.ValidateProviders(yearProvider, constantsProvider);
        if (providerValidation.IsFailure)
        {
            throw new InvalidOperationException(
                "Salary calculator providers are invalid: " +
                string.Join("; ", providerValidation.Errors.Select(e => $"{e.Code}: {e.Message}")));
        }

        return new SalaryCalculatorService(validationEngine, yearProvider, constantsProvider);
    }
}
