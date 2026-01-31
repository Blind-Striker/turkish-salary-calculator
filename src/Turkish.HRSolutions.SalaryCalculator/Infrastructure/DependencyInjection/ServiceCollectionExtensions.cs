using Microsoft.Extensions.DependencyInjection;

using Turkish.HRSolutions.SalaryCalculator.Application.Calculator;
using Turkish.HRSolutions.SalaryCalculator.Application.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering Turkish Salary Calculator services with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
#pragma warning disable CA1034 // False positive https://github.com/dotnet/sdk/issues/51681
    /// <summary>
    /// Configures Turkish salary calculator services.
    /// </summary>
    extension(IServiceCollection services)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Adds Turkish Salary Calculator services to the service collection using the Configurator pattern.
        /// </summary>
        /// <param name="configure">Delegate to configure the calculator (optional).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// // Default (Embedded resources)
        /// services.AddSalaryCalculator();
        ///
        /// // Advanced: Use FileSystem
        /// services.AddSalaryCalculator(config =>
        ///     config.UseFileSystem("/data/years.json", "/data/constants.json"));
        ///
        /// // Expert: Custom Providers with Scoped/Transient lifecycles
        /// services.AddSalaryCalculator(config =>
        ///     config.WithCustomYearProvider&lt;MySqlYearProvider&gt;(ServiceLifetime.Scoped));
        /// </code>
        /// </example>
        public IServiceCollection AddSalaryCalculator(Action<IServiceCollectionCalculatorConfigurator>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            var configurator = new ServiceCollectionCalculatorConfigurator(services);

            if (configure is null)
            {
                // Default to embedded if no configuration provided
                configurator.UseEmbeddedResources();
            }
            else
            {
                // Apply user configuration
                configure(configurator);

                // Fail-fast: Ensure providers are registered when user supplies a configure action
                // This makes it explicit that when you provide configuration, you must register providers
                ValidateProvidersRegistered(services);
            }

            services.AddSingleton<IValidationEngine, ValidationEngine>();

            services.AddSingleton<ISalaryCalculatorMetadata>(sp =>
            {
                var validationEngine = sp.GetRequiredService<IValidationEngine>();
                var yearProvider = sp.GetRequiredService<IYearParameterProvider>();
                var constantsProvider = sp.GetRequiredService<ICalculationConstantsProvider>();
                return new SalaryCalculatorMetadataService(validationEngine, yearProvider, constantsProvider);
            });

            // Register ISalaryCalculator using providers from DI
            services.AddSingleton<ISalaryCalculator>(sp =>
            {
                var validationEngine = sp.GetRequiredService<IValidationEngine>();
                var yearProvider = sp.GetRequiredService<IYearParameterProvider>();
                var constantsProvider = sp.GetRequiredService<ICalculationConstantsProvider>();

                var providerValidation = validationEngine.ValidateProviders(yearProvider, constantsProvider);
                if (providerValidation.IsFailure)
                {
                    throw new InvalidOperationException(
                        "Salary calculator providers are invalid: " +
                        string.Join("; ", providerValidation.Errors.Select(e => $"{e.Code}: {e.Message}")));
                }

                return new SalaryCalculatorService(validationEngine, yearProvider, constantsProvider);
            });

            return services;
        }
    }

    private static void ValidateProvidersRegistered(IServiceCollection services)
    {
        var hasYearProvider = services.Any(sd => sd.ServiceType == typeof(IYearParameterProvider));
        var hasConstantsProvider = services.Any(sd => sd.ServiceType == typeof(ICalculationConstantsProvider));

        if (hasYearProvider && hasConstantsProvider)
        {
            return;
        }

        var missing = new List<string>();
        if (!hasYearProvider)
        {
            missing.Add(nameof(IYearParameterProvider));
        }

        if (!hasConstantsProvider)
        {
            missing.Add(nameof(ICalculationConstantsProvider));
        }

        throw new InvalidOperationException(
            "When providing a configure action, you must register all required providers. " +
            "Missing: " + string.Join(", ", missing) + ". " +
            "Call UseEmbeddedResources(), UseFileSystem(), or WithCustom...Provider() to register providers.");
    }
}
