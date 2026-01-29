namespace Turkish.HRSolutions.SalaryCalculator.Configuration;

/// <summary>
/// Configuration options for the Turkish Salary Calculator.
/// </summary>
/// <remarks>
/// <para>
/// By default, the calculator uses embedded year parameters and calculation constants
/// from assembly resources. This provides a zero-configuration experience.
/// </para>
/// <para>
/// For advanced scenarios, you can specify file paths to load parameters from JSON files:
/// <list type="bullet">
///   <item><see cref="YearParametersFilePath"/> - Path to year-constants.json</item>
///   <item><see cref="CalculationConstantsFilePath"/> - Path to calculation-constants.json</item>
/// </list>
/// </para>
/// <para>
/// For complete control, register custom implementations of <c>IYearParameterProvider</c>
/// and <c>ICalculationConstantsProvider</c> in your DI container before calling
/// <c>AddSalaryCalculator()</c>. The library uses <c>TryAdd</c>, so your registrations
/// take precedence.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Default: embedded resources
/// services.AddSalaryCalculator();
///
/// // File-based: load from JSON files
/// services.AddSalaryCalculator(opts =>
/// {
///     opts.YearParametersFilePath = "data/year-constants.json";
/// });
///
/// // Custom provider: register your own
/// services.AddSingleton&lt;IYearParameterProvider, MyDatabaseYearProvider&gt;();
/// services.AddSalaryCalculator(); // Won't override your provider
/// </code>
/// </example>
public sealed class SalaryCalculatorOptions
{
    /// <summary>
    /// Gets or sets the file path to load year parameters from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If set, the calculator will load year parameters from this JSON file
    /// instead of using embedded assembly resources.
    /// </para>
    /// <para>
    /// The file must be in the same format as the embedded year-constants.json.
    /// </para>
    /// <para>
    /// If null (default), embedded resources are used.
    /// </para>
    /// </remarks>
    public string? YearParametersFilePath { get; set; }

    /// <summary>
    /// Gets or sets the file path to load calculation constants from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If set, the calculator will load calculation constants from this JSON file
    /// instead of using embedded assembly resources.
    /// </para>
    /// <para>
    /// The file must be in the same format as the embedded calculation-constants.json.
    /// </para>
    /// <para>
    /// <b>WARNING:</b> Calculation constants include employee type flags that control
    /// validation and calculation logic. Incorrect values will cause validation errors
    /// or wrong calculations. Only override if you know what you're doing.
    /// </para>
    /// <para>
    /// If null (default), embedded resources are used.
    /// </para>
    /// </remarks>
    public string? CalculationConstantsFilePath { get; set; }
}
