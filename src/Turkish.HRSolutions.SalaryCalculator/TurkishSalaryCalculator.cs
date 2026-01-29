using Turkish.HRSolutions.SalaryCalculator.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator;

/// <summary>
/// Static entry point for the Turkish Salary Calculator.
/// </summary>
/// <remarks>
/// <para>
/// Use this class for standalone (non-DI) scenarios. For DI scenarios,
/// use <c>services.AddSalaryCalculator()</c> extension method instead.
/// </para>
/// <para>
/// The calculator uses embedded assembly resources by default. You can
/// configure file-based providers via <see cref="SalaryCalculatorOptions"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple: embedded defaults
/// var calculator = TurkishSalaryCalculator.Calculator();
/// var result = calculator.Calculate(new GrossToNetRequest { ... });
///
/// // With file-based year parameters
/// var calculator = TurkishSalaryCalculator.Calculator(new SalaryCalculatorOptions
/// {
///     YearParametersFilePath = "data/year-constants.json"
/// });
///
/// // Query metadata
/// var metadata = TurkishSalaryCalculator.Metadata();
/// var years = metadata.GetAvailableYears();
///
/// // Fluent builder
/// var result = TurkishSalaryCalculator.Create()
///     .ForYear(2026)
///     .WithEmployeeType(EmployeeTypeId.Standard)
///     .CalculateGrossToNet()
///     .Calculate(30_000m);
/// </code>
/// </example>
public static class TurkishSalaryCalculator
{
    // Calculator(), Metadata(), Create() methods will be added in Phase 3
    // when ISalaryCalculator, ISalaryCalculatorMetadata, and ICalculatorSetup are implemented.

    /// <summary>
    /// Builds a year parameter provider based on the configuration options.
    /// </summary>
    /// <param name="options">Configuration options, or null for defaults.</param>
    /// <returns>A year parameter provider.</returns>
    internal static IYearParameterProvider BuildYearProvider(SalaryCalculatorOptions? options)
    {
        if (options?.YearParametersFilePath is not null)
        {
            return new FileSystemYearParameterProvider(options.YearParametersFilePath);
        }

        return new EmbeddedYearParameterProvider();
    }

    /// <summary>
    /// Builds a calculation constants provider based on the configuration options.
    /// </summary>
    /// <param name="options">Configuration options, or null for defaults.</param>
    /// <returns>A calculation constants provider.</returns>
    internal static ICalculationConstantsProvider BuildConstantsProvider(SalaryCalculatorOptions? options)
    {
        if (options?.CalculationConstantsFilePath is not null)
        {
            return new FileSystemCalculationConstantsProvider(options.CalculationConstantsFilePath);
        }

        return new EmbeddedCalculationConstantsProvider();
    }
}
