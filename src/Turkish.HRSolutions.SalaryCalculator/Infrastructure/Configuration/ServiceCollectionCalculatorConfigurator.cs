using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;
using AppProviders = Turkish.HRSolutions.SalaryCalculator.Application.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;

internal sealed class ServiceCollectionCalculatorConfigurator(IServiceCollection services) : IServiceCollectionCalculatorConfigurator
{
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator UseEmbeddedResources()
    {
        // Replace causes the last registration to win, effectively "configuring" the choice
        Services.Replace(ServiceDescriptor.Singleton<AppProviders.IYearParameterProvider, EmbeddedYearParameterProvider>());
        Services.Replace(ServiceDescriptor.Singleton<AppProviders.ICalculationConstantsProvider, EmbeddedCalculationConstantsProvider>());
        return this;
    }

    /// <summary>Explicit interface implementation for base interface.</summary>
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseEmbeddedResources() => UseEmbeddedResources();

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath)
    {
        Services.Replace(ServiceDescriptor.Singleton<AppProviders.IYearParameterProvider>(_ =>
            new FileSystemYearParameterProvider(yearParametersPath)));

        Services.Replace(ServiceDescriptor.Singleton<AppProviders.ICalculationConstantsProvider>(_ =>
            new FileSystemCalculationConstantsProvider(calculationConstantsPath)));

        return this;
    }

    /// <summary>Explicit interface implementation for base interface.</summary>
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseFileSystem(string yearParametersPath, string calculationConstantsPath)
        => UseFileSystem(yearParametersPath, calculationConstantsPath);

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, AppProviders.IYearParameterProvider
    {
        Services.Replace(new ServiceDescriptor(typeof(AppProviders.IYearParameterProvider), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomYearProvider(Func<IServiceProvider, AppProviders.IYearParameterProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Services.Replace(new ServiceDescriptor(typeof(AppProviders.IYearParameterProvider), factory, lifetime));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, AppProviders.ICalculationConstantsProvider
    {
        Services.Replace(new ServiceDescriptor(typeof(AppProviders.ICalculationConstantsProvider), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollectionCalculatorConfigurator WithCustomConstantsProvider(Func<IServiceProvider, AppProviders.ICalculationConstantsProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Services.Replace(new ServiceDescriptor(typeof(AppProviders.ICalculationConstantsProvider), factory, lifetime));
        return this;
    }
}
