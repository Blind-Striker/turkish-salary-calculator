using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Centralized validation engine for salary calculation requests.
/// </summary>
/// <remarks>
/// <para>
/// Responsibilities:
/// </para>
/// <list type="bullet">
/// <item><description>Input validation (year, employee type, monthly inputs)</description></item>
/// <item><description>Capability violation detection (warnings for ignored settings)</description></item>
/// </list>
/// <para>
/// Capability determination is delegated to <see cref="ICapabilityResolver"/>.
/// Provider validation is also handled by <see cref="ICapabilityResolver.ValidateProviders"/>.
/// </para>
/// </remarks>
internal sealed class ValidationEngine : IValidationEngine
{
    private readonly ICapabilityResolver _capabilityResolver;
    private readonly IYearParameterProvider _yearProvider;
    private readonly ICalculationConstantsProvider _constantsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEngine"/> class.
    /// </summary>
    /// <param name="capabilityResolver">Resolver for determining calculator capabilities.</param>
    /// <param name="yearProvider">Provider for year-specific parameters.</param>
    /// <param name="constantsProvider">Provider for calculation constants.</param>
    public ValidationEngine(
        ICapabilityResolver capabilityResolver,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider)
    {
        _capabilityResolver = capabilityResolver ?? throw new ArgumentNullException(nameof(capabilityResolver));
        _yearProvider = yearProvider ?? throw new ArgumentNullException(nameof(yearProvider));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
    }

    /// <inheritdoc />
    public Result<CapabilityInfo> Validate(ValidationContext context)
    {
        var errors = new List<Error>();
        var warnings = new List<Error>();

        // 1. Input validation (blocking errors)
        ValidateInput(context, errors);

        if (errors.Count > 0)
        {
            return Result<CapabilityInfo>.Failure(errors);
        }

        // 2. Resolve capabilities using ICapabilityResolver
        var capabilityResult = _capabilityResolver.ResolveCapabilities(
            context.Year,
            context.EmployeeType,
            context.IsPensioner,
            context.Mode);

        if (capabilityResult.IsFailure)
        {
            return capabilityResult;
        }

        // 3. Check for capability violations (warnings)
        CheckCapabilityViolations(context, capabilityResult.Value.DisabledCapabilities, warnings);

        return Result<CapabilityInfo>.Success(capabilityResult.Value, warnings);
    }

    private void ValidateInput(ValidationContext context, List<Error> errors)
    {
        // Year validation
        if (context.Year == 0)
        {
            errors.Add(new Error(ErrorCode.YearNotSpecified, "Calculation year is required."));
        }
        else if (_yearProvider.GetParameter(context.Year) is null)
        {
            errors.Add(new Error(
                ErrorCode.YearNotSupported,
                $"Year {context.Year} is not supported. Available years: {string.Join(", ", _yearProvider.AvailableYears)}"));
        }

        // Employee type validation
        if (context.EmployeeType.Value == 0)
        {
            errors.Add(new Error(ErrorCode.MissingEmployeeTypeDefinition, "Employee type is required."));
        }
        else if (_constantsProvider.GetEmployeeType(context.EmployeeType) is null)
        {
            errors.Add(new Error(
                ErrorCode.MissingEmployeeTypeDefinition,
                $"Employee type {context.EmployeeType.Value} is not defined."));
        }

        // Month validation
        if (context.Months.Count == 0)
        {
            errors.Add(new Error(ErrorCode.InvalidMonthCount, "At least one monthly input is required."));
        }
        else if (context.Months.Count != 12)
        {
            errors.Add(new Error(
                ErrorCode.InvalidMonthCount,
                $"Expected 12 monthly inputs, but got {context.Months.Count}."));
        }

        // Monthly input validation
        for (var i = 0; i < context.Months.Count; i++)
        {
            var month = context.Months[i];

            // Note: Zero salary is allowed for months where employee didn't work (new hire scenarios).
            // Angular handles this by returning zeros for such months.
            if (month.Salary < 0)
            {
                errors.Add(Error.Validation(
                    ErrorCode.InvalidSalaryAmount,
                    $"Month {i + 1}: Salary cannot be negative.",
                    $"Months[{i}].Salary",
                    month.Salary));
            }

            if (month.WorkedDays is < 0 or > 30)
            {
                errors.Add(Error.Validation(
                    ErrorCode.InvalidWorkedDays,
                    $"Month {i + 1}: Worked days must be between 0 and 30.",
                    $"Months[{i}].WorkedDays",
                    month.WorkedDays));
            }

            if (month.RnDDays < 0)
            {
                errors.Add(Error.Validation(
                    ErrorCode.InvalidRnDDays,
                    $"Month {i + 1}: R&D days cannot be negative.",
                    $"Months[{i}].RnDDays",
                    month.RnDDays));
            }

            if (month.RnDDays > month.WorkedDays)
            {
                errors.Add(Error.Validation(
                    ErrorCode.RnDDaysExceedWorkedDays,
                    $"Month {i + 1}: R&D days ({month.RnDDays}) cannot exceed worked days ({month.WorkedDays}).",
                    $"Months[{i}].RnDDays",
                    month.RnDDays));
            }
        }
    }

    /// <summary>
    /// Checks if the user provided settings that conflict with disabled capabilities.
    /// Adds warnings for settings that will be ignored.
    /// </summary>
    private static void CheckCapabilityViolations(
        ValidationContext context,
        Capability disabledCapabilities,
        List<Error> warnings)
    {
        // AGI Selection disabled but AGI settings provided
        if (disabledCapabilities.HasFlag(Capability.AgiSelection) && context.Agi is not null)
        {
            warnings.Add(Error.ValidationWarning(
                ErrorCode.AgiNotApplicable,
                "AGI settings will be ignored because AGI is not applicable for this configuration."));
        }

        // Education Type disabled but education specified in RnD settings
        if (disabledCapabilities.HasFlag(Capability.EducationType) &&
            context.RnD?.Education is not null &&
            context.RnD.Education != EducationTypeId.OtherRnDPersonnel)
        {
            warnings.Add(Error.ValidationWarning(
                ErrorCode.EducationExemptionNotApplicable,
                "Education type will be ignored because education exemption is not applicable for this employee type."));
        }

        // 5746 Discount disabled but Apply5746Discount is true
        if (disabledCapabilities.HasFlag(Capability.Discount5746) && context.Apply5746Discount)
        {
            warnings.Add(Error.ValidationWarning(
                ErrorCode.Discount5746NotApplicable,
                "5746 discount will be ignored because it is not applicable for this configuration."));
        }

        // R&D Days disabled but R&D days provided
        if (disabledCapabilities.HasFlag(Capability.RnDDaysInput) &&
            context.Months.Any(m => m.RnDDays > 0))
        {
            warnings.Add(Error.ValidationWarning(
                ErrorCode.RnDDaysNotApplicable,
                "R&D days will be ignored because R&D exemption is not applicable for this employee type."));
        }

        // AGI Included In Net disabled but IsAgiIncludedInNet is true
        if (disabledCapabilities.HasFlag(Capability.AgiIncludedInNet) && context.IsAgiIncludedInNet)
        {
            warnings.Add(Error.ValidationWarning(
                ErrorCode.AgiIncludedInNetNotApplicable,
                "AGI included in net will be ignored because it is only applicable in NET_TO_GROSS mode."));
        }
    }
}
