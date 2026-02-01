using Turkish.HRSolutions.SalaryCalculator.Common.Results;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Validates calculation requests and returns capability information.
/// </summary>
/// <remarks>
/// <para>
/// The validation engine performs:
/// </para>
/// <list type="bullet">
/// <item><description>Input validation (year, employee type, monthly inputs)</description></item>
/// <item><description>Capability violation detection (warnings for ignored settings)</description></item>
/// </list>
/// <para>
/// Capability determination is delegated to <see cref="Services.ICapabilityResolver"/>.
/// </para>
/// </remarks>
internal interface IValidationEngine
{
    /// <summary>
    /// Validates the calculation request and returns capability information.
    /// </summary>
    /// <param name="context">The validation context containing all request parameters.</param>
    /// <returns>
    /// A result containing capability info with warnings on success,
    /// or blocking errors on validation failure.
    /// </returns>
    public Result<CapabilityInfo> Validate(ValidationContext context);
}
