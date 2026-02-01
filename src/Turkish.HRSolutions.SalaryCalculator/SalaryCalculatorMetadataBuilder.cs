using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Common.Exceptions;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Configuration;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

namespace Turkish.HRSolutions.SalaryCalculator;

/// <summary>
/// Static entry point for the Turkish Salary Calculator Metadata service.
/// Provides lightweight access to calculator metadata without full calculation capabilities.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder when you only need metadata such as:
/// </para>
/// <list type="bullet">
/// <item><description>Available years for dropdown population</description></item>
/// <item><description>Employee types for UI selection</description></item>
/// <item><description>Education types for R&amp;D configuration</description></item>
/// <item><description>Capability checks for dynamic form behavior</description></item>
/// </list>
/// <para>
/// This is more lightweight than <see cref="SalaryCalculatorBuilder.Create"/> as it
/// doesn't initialize the full validation and calculation pipeline.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get metadata with default embedded resources
/// var metadata = SalaryCalculatorMetadataBuilder.Create();
///
/// // Populate dropdowns
/// var years = metadata.GetAvailableYears();
/// var employeeTypes = metadata.GetEmployeeTypes();
///
/// // Check capabilities for dynamic UI
/// var caps = metadata.GetCapabilities(2024, EmployeeTypeId.Teknokent4691);
/// if (caps.IsSuccess &amp;&amp; caps.Value.IsAvailable(Capability.AgiConfiguration))
/// {
///     // Show AGI configuration section
/// }
/// </code>
/// </example>
public static class SalaryCalculatorMetadataBuilder
{
    /// <summary>
    /// Creates and configures a new instance of the salary calculator metadata service.
    /// </summary>
    /// <param name="configure">Optional configuration action for custom data sources.</param>
    /// <returns>An initialized ISalaryCalculatorMetadata instance.</returns>
    /// <example>
    /// <code>
    /// // Default (Embedded)
    /// var metadata = SalaryCalculatorMetadataBuilder.Create();
    ///
    /// // Custom file system
    /// var metadata = SalaryCalculatorMetadataBuilder.Create(config =>
    ///     config.UseFileSystem("years.json", "constants.json"));
    /// </code>
    /// </example>
    /// <exception cref="SalaryCalculatorConfigurationException">Thrown if providers are invalid.</exception>
    public static ISalaryCalculatorMetadata Create(Action<IStandaloneCalculatorConfigurator>? configure = null)
    {
        var configurator = new StandaloneCalculatorConfigurator();
        configurator.UseEmbeddedResources();
        configure?.Invoke(configurator);

        var (yearProvider, constantsProvider) = configurator.Build();

        // Create capability resolver for metadata queries
        var capabilityResolver = new CapabilityResolver(yearProvider, constantsProvider);
        var providerValidation = capabilityResolver.ValidateProviders();

        if (providerValidation.IsFailure)
        {
            var errors = providerValidation.Errors.Select(e => $"{e.Code}: {e.Message}");
            throw new SalaryCalculatorConfigurationException(
                $"Salary calculator providers are invalid: {string.Join("; ", errors)}");
        }

        return new SalaryCalculatorMetadataService(capabilityResolver, yearProvider, constantsProvider);
    }
}
