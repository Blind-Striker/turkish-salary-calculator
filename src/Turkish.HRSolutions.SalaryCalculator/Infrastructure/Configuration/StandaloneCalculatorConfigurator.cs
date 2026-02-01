using Turkish.HRSolutions.SalaryCalculator.Application.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;

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

internal sealed class StandaloneCalculatorConfigurator : IStandaloneCalculatorConfigurator
{
    private IYearParameterProvider _yearProvider = new EmbeddedYearParameterProvider();
    private ICalculationConstantsProvider _constantsProvider = new EmbeddedCalculationConstantsProvider();

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
    public IStandaloneCalculatorConfigurator WithCustomYearProvider(IYearParameterProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _yearProvider = provider;
        return this;
    }

    /// <inheritdoc />
    public IStandaloneCalculatorConfigurator WithCustomConstantsProvider(ICalculationConstantsProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _constantsProvider = provider;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured provider(s).
    /// Internal for now, used by static entry point.
    /// </summary>
    internal (IYearParameterProvider YearProvider, ICalculationConstantsProvider ConstantsProvider) Build() => (_yearProvider, _constantsProvider);
}
