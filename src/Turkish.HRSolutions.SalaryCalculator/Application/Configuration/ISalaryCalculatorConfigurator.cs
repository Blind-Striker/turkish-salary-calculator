namespace Turkish.HRSolutions.SalaryCalculator.Application.Configuration;

/// <summary>
/// Defines the contract for configuring the Turkish Salary Calculator
/// in both DI and standalone scenarios.
/// </summary>
public interface ISalaryCalculatorConfigurator
{
    /// <summary>
    /// Configures the calculator to use embedded year parameters and calculation constants.
    /// This is the default zero-configuration mode.
    /// </summary>
    public ISalaryCalculatorConfigurator UseEmbeddedResources();

    /// <summary>
    /// Configures the calculator to load parameters from JSON files on the file system.
    /// </summary>
    /// <param name="yearParametersPath">Path to year-constants.json</param>
    /// <param name="calculationConstantsPath">Path to calculation-constants.json</param>
    public ISalaryCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath);
}
