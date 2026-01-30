using Turkish.HRSolutions.SalaryCalculator.Api;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Configuration;

namespace Turkish.HRSolutions.SalaryCalculator;

/// <summary>
/// Static entry point for the Turkish Salary Calculator.
/// </summary>
public static class TurkishSalaryCalculator
{
    /// <summary>
    /// Creates and configures a new instance of the salary calculator service.
    /// </summary>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>An initialized ISalaryCalculator instance.</returns>
    /// <example>
    /// <code>
    /// // Default (Embedded)
    /// var calculator = TurkishSalaryCalculator.Create();
    ///
    /// // Custom
    /// var calculator = TurkishSalaryCalculator.Create(config =>
    ///     config.UseFileSystem("years.json", "constants.json"));
    /// </code>
    /// </example>
    public static ISalaryCalculator Create(Action<IStandaloneCalculatorConfigurator>? configure = null)
    {
        var configurator = new StandaloneCalculatorConfigurator();
        configurator.UseEmbeddedResources();
        configure?.Invoke(configurator);

        var (yearProvider, constantsProvider) = configurator.Build();
        return new V2SalaryCalculatorService(yearProvider, constantsProvider);
    }
}
