using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;

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
    /// <param name="factory">Factory delegate for creating year parameter provider instances.</param>
    /// <param name="lifetime">The service lifetime (default: Singleton).</param>
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider(Func<IServiceProvider, IYearParameterProvider> factory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton);

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
    /// <param name="factory">Factory delegate for creating year parameter provider instances.</param>
    /// <param name="lifetime">The service lifetime (default: Singleton).</param>
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider(Func<IServiceProvider, ICalculationConstantsProvider> factory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton);
}

internal sealed class ServiceCollectionCalculatorConfigurator(IServiceCollection services) : IServiceCollectionCalculatorConfigurator
{
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator UseEmbeddedResources()
    {
        // Replace causes the last registration to win, effectively "configuring" the choice
        Services.Replace(ServiceDescriptor.Singleton<IYearParameterProvider, EmbeddedYearParameterProvider>());
        Services.Replace(ServiceDescriptor.Singleton<ICalculationConstantsProvider, EmbeddedCalculationConstantsProvider>());
        return this;
    }

    /// <inheritdoc />
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseEmbeddedResources() => UseEmbeddedResources();

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yearParametersPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(calculationConstantsPath);

        Services.Replace(ServiceDescriptor.Singleton<IYearParameterProvider>(_ => new FileSystemYearParameterProvider(yearParametersPath)));
        Services.Replace(ServiceDescriptor.Singleton<ICalculationConstantsProvider>(_ => new FileSystemCalculationConstantsProvider(calculationConstantsPath)));

        return this;
    }

    /// <inheritdoc />
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseFileSystem(string yearParametersPath, string calculationConstantsPath)
        => UseFileSystem(yearParametersPath, calculationConstantsPath);

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, IYearParameterProvider
    {
        Services.Replace(new ServiceDescriptor(typeof(IYearParameterProvider), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider(Func<IServiceProvider, IYearParameterProvider> factory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Services.Replace(new ServiceDescriptor(typeof(IYearParameterProvider), factory, lifetime));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, ICalculationConstantsProvider
    {
        Services.Replace(new ServiceDescriptor(typeof(ICalculationConstantsProvider), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider(Func<IServiceProvider, ICalculationConstantsProvider> factory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Services.Replace(new ServiceDescriptor(typeof(ICalculationConstantsProvider), factory, lifetime));
        return this;
    }
}
