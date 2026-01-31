using Turkish.HRSolutions.SalaryCalculator.Application.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;
using AppProviders = Turkish.HRSolutions.SalaryCalculator.Application.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;

internal sealed class StandaloneCalculatorConfigurator : IStandaloneCalculatorConfigurator
{
    private AppProviders.IYearParameterProvider _yearProvider = new EmbeddedYearParameterProvider();
    private AppProviders.ICalculationConstantsProvider _constantsProvider = new EmbeddedCalculationConstantsProvider();

    /// <inheritdoc />
    public IStandaloneCalculatorConfigurator UseEmbeddedResources()
    {
        _yearProvider = new EmbeddedYearParameterProvider();
        _constantsProvider = new EmbeddedCalculationConstantsProvider();
        return this;
    }

    /// <summary>Explicit interface implementation for base interface.</summary>
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseEmbeddedResources() => UseEmbeddedResources();

    /// <inheritdoc />
    public IStandaloneCalculatorConfigurator UseFileSystem(string yearParametersPath, string calculationConstantsPath)
    {
        _yearProvider = new FileSystemYearParameterProvider(yearParametersPath);
        _constantsProvider = new FileSystemCalculationConstantsProvider(calculationConstantsPath);
        return this;
    }

    /// <summary>Explicit interface implementation for base interface.</summary>
    ISalaryCalculatorConfigurator ISalaryCalculatorConfigurator.UseFileSystem(string yearParametersPath, string calculationConstantsPath)
        => UseFileSystem(yearParametersPath, calculationConstantsPath);

    /// <inheritdoc />
    public IStandaloneCalculatorConfigurator WithCustomYearProvider(AppProviders.IYearParameterProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _yearProvider = provider;
        return this;
    }

    /// <inheritdoc />
    public IStandaloneCalculatorConfigurator WithCustomConstantsProvider(AppProviders.ICalculationConstantsProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _constantsProvider = provider;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured provider(s).
    /// Internal for now, used by static entry point.
    /// </summary>
    internal (AppProviders.IYearParameterProvider YearProvider, AppProviders.ICalculationConstantsProvider ConstantsProvider) Build()
    {
        return (_yearProvider, _constantsProvider);
    }
}
