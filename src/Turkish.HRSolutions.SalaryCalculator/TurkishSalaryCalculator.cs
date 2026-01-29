using Turkish.HRSolutions.SalaryCalculator.Api;
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

        // Phase 3: We will likely use these providers to instantiate the service.
        var (yearProvider, constantsProvider) = configurator.Build();
        _ = yearProvider;
        _ = constantsProvider;

        // Pending Phase 3 implementation
        // return new SalaryCalculationService(yearProvider, constantsProvider);
        throw new NotSupportedException("SalaryCalculator service construction will be implemented in Phase 3.");
    }
}
