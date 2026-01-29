using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Turkish.HRSolutions.SalaryCalculator.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.DependencyInjection;

/// <summary>
/// Extension methods for registering Turkish Salary Calculator services with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Turkish Salary Calculator services to the service collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, this registers:
    /// <list type="bullet">
    ///   <item><c>IYearParameterProvider</c> → <c>EmbeddedYearParameterProvider</c> (singleton)</item>
    ///   <item><c>ICalculationConstantsProvider</c> → <c>EmbeddedCalculationConstantsProvider</c> (singleton)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Use the <paramref name="configure"/> delegate to customize options:
    /// <list type="bullet">
    ///   <item>Set <c>YearParametersFilePath</c> to load year parameters from a file</item>
    ///   <item>Set <c>CalculationConstantsFilePath</c> to load constants from a file</item>
    /// </list>
    /// </para>
    /// <para>
    /// To use custom providers, register them before calling this method:
    /// <code>
    /// services.AddSingleton&lt;IYearParameterProvider, MyDatabaseProvider&gt;();
    /// services.AddSalaryCalculator(); // Won't override your provider
    /// </code>
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // Default: embedded resources
    /// services.AddSalaryCalculator();
    ///
    /// // With file-based configuration
    /// services.AddSalaryCalculator(opts =>
    /// {
    ///     opts.YearParametersFilePath = configuration["SalaryCalculator:YearParametersPath"];
    /// });
    ///
    /// // With custom provider
    /// services.AddSingleton&lt;IYearParameterProvider, MyDatabaseYearProvider&gt;();
    /// services.AddSalaryCalculator();
    /// </code>
    /// </example>
    public static IServiceCollection AddSalaryCalculator(
        this IServiceCollection services,
        Action<SalaryCalculatorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register options using the proper Options pattern
        services.AddOptions<SalaryCalculatorOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        // TryAdd = won't override if user already registered custom providers
        services.TryAddSingleton<IYearParameterProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SalaryCalculatorOptions>>().Value;
            return TurkishSalaryCalculator.BuildYearProvider(options);
        });

        services.TryAddSingleton<ICalculationConstantsProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SalaryCalculatorOptions>>().Value;
            return TurkishSalaryCalculator.BuildConstantsProvider(options);
        });

        // ISalaryCalculator and ISalaryCalculatorMetadata registrations
        // will be added in Phase 3 when those interfaces are implemented.

        return services;
    }
}
