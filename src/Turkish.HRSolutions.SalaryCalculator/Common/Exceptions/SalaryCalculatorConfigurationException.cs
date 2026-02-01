namespace Turkish.HRSolutions.SalaryCalculator.Common.Exceptions;

/// <summary>
/// Exception thrown when salary calculator configuration fails during initialization.
/// </summary>
/// <remarks>
/// <para>
/// This exception indicates a problem with loading or validating configuration data
/// (e.g., year parameters, calculation constants). It is typically thrown at construction
/// time when the calculator cannot be properly initialized.
/// </para>
/// <para>
/// Common causes include:
/// <list type="bullet">
/// <item>Missing or corrupted embedded resources</item>
/// <item>Invalid JSON in configuration files</item>
/// <item>Missing required employee types or year parameters</item>
/// <item>Custom provider implementation errors</item>
/// </list>
/// </para>
/// </remarks>
public class SalaryCalculatorConfigurationException : InvalidOperationException
{
    public SalaryCalculatorConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SalaryCalculatorConfigurationException"/>.
    /// </summary>
    /// <param name="message">The error message describing the configuration failure.</param>
    public SalaryCalculatorConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SalaryCalculatorConfigurationException"/>.
    /// </summary>
    /// <param name="message">The error message describing the configuration failure.</param>
    /// <param name="innerException">The inner exception that caused this failure.</param>
    public SalaryCalculatorConfigurationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SalaryCalculatorConfigurationException"/>.
    /// </summary>
    /// <param name="message">The error message describing the configuration failure.</param>
    /// <param name="resourceName">The name of the resource that failed to load.</param>
    /// <param name="innerException">The inner exception that caused this failure (optional).</param>
    public SalaryCalculatorConfigurationException(string message, string resourceName, Exception? innerException = null)
        : base(message, innerException)
    {
        ResourceName = resourceName;
    }

    /// <summary>
    /// Gets the name of the resource or provider that failed to load (if applicable).
    /// </summary>
    public string? ResourceName { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return ResourceName is null
            ? base.ToString()
            : $"{base.ToString()}{Environment.NewLine}Resource: {ResourceName}";
    }
}
