using Microsoft.Extensions.DependencyInjection;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Configuration;

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

/// <summary>
/// Extended configurator interface for Dependency Injection scenarios.
/// Allows specifying service lifetimes for custom providers.
/// </summary>
public interface IServiceCollectionCalculatorConfigurator : ISalaryCalculatorConfigurator
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <inheritdoc cref="ISalaryCalculatorConfigurator.UseEmbeddedResources"/>
    public new IServiceCollectionCalculatorConfigurator UseEmbeddedResources();

    /// <inheritdoc cref="ISalaryCalculatorConfigurator.UseFileSystem"/>
    public new IServiceCollectionCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath);

    /// <summary>
    /// Registers a custom year parameter provider implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The provider implementation type.</typeparam>
    /// <param name="lifetime">The service lifetime (default: Singleton).</param>
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, IYearParameterProvider;

    /// <summary>
    /// Registers a custom year parameter provider using a factory delegate.
    /// </summary>
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider(Func<IServiceProvider, IYearParameterProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton);

    /// <summary>
    /// Registers a custom calculation constants provider implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The provider implementation type.</typeparam>
    /// <param name="lifetime">The service lifetime (default: Singleton).</param>
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, ICalculationConstantsProvider;

    /// <summary>
    /// Registers a custom calculation constants provider using a factory delegate.
    /// </summary>
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider(Func<IServiceProvider, ICalculationConstantsProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton);
}

/// <summary>
/// Extended configurator interface for Standalone (non-DI) scenarios.
/// Allows passing pre-constructed instances.
/// </summary>
public interface IStandaloneCalculatorConfigurator : ISalaryCalculatorConfigurator
{
    /// <inheritdoc cref="ISalaryCalculatorConfigurator.UseEmbeddedResources"/>
    public new IStandaloneCalculatorConfigurator UseEmbeddedResources();

    /// <inheritdoc cref="ISalaryCalculatorConfigurator.UseFileSystem"/>
    public new IStandaloneCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath);

    /// <summary>
    /// Uses a specific instance for year parameters.
    /// </summary>
    public IStandaloneCalculatorConfigurator WithCustomYearProvider(IYearParameterProvider provider);

    /// <summary>
    /// Uses a specific instance for calculation constants.
    /// </summary>
    public IStandaloneCalculatorConfigurator WithCustomConstantsProvider(ICalculationConstantsProvider provider);
}
