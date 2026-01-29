using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Configuration;

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

    /// <summary>Explicit interface implementation for base interface.</summary>
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseEmbeddedResources() => UseEmbeddedResources();

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath)
    {
        Services.Replace(ServiceDescriptor.Singleton<IYearParameterProvider>(_ =>
            new FileSystemYearParameterProvider(yearParametersPath)));

        Services.Replace(ServiceDescriptor.Singleton<ICalculationConstantsProvider>(_ =>
            new FileSystemCalculationConstantsProvider(calculationConstantsPath)));

        return this;
    }

    /// <summary>Explicit interface implementation for base interface.</summary>
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
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider(Func<IServiceProvider, IYearParameterProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
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
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider(Func<IServiceProvider, ICalculationConstantsProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Services.Replace(new ServiceDescriptor(typeof(ICalculationConstantsProvider), factory, lifetime));
        return this;
    }
}
