using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Centralized validation engine for salary calculations.
/// Implements the 7 capability rules from Angular UI + input validation.
/// </summary>
internal sealed class ValidationEngine : IValidationEngine
{
    /// <summary>
    /// Validates the calculation request and returns capability information.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="yearProvider">Year parameter provider.</param>
    /// <param name="constantsProvider">Calculation constants provider.</param>
    /// <returns>A result containing capability info with errors/warnings, or failure.</returns>
    public Result<CapabilityInfo> Validate(
        ValidationContext context,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider)
    {
        var errors = new List<Error>();
        var warnings = new List<Error>();

        // 1. Input validation (blocking errors)
        ValidateInput(context, yearProvider, constantsProvider, errors);

        if (errors.Count > 0)
        {
            return Result<CapabilityInfo>.Failure(errors);
        }

        // 2. Get employee type and year for capability rules
        var employeeType = constantsProvider.GetEmployeeType(context.EmployeeType)!;
        var yearParam = yearProvider.GetParameter(context.Year)!;

        // 3. Determine disabled capabilities
        var disabledCapabilities = DetermineDisabledCapabilities(
            context, employeeType, yearParam);

        // 4. Check for capability violations (warnings)
        CheckCapabilityViolations(context, disabledCapabilities, warnings);

        var capabilityInfo = new CapabilityInfo(disabledCapabilities);
        return Result<CapabilityInfo>.Success(capabilityInfo, warnings);
    }

    /// <summary>
    /// Validates only the capabilities for a given configuration.
    /// Used by ISalaryCalculatorMetadata.GetCapabilities.
    /// </summary>
    public Result<CapabilityInfo> ValidateCapabilities(
        int year,
        EmployeeTypeId employeeType,
        bool isPensioner,
        CalculationMode mode,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider)
    {
        var errors = new List<Error>();

        // Basic validation
        var yearParam = yearProvider.GetParameter(year);
        if (yearParam is null)
        {
            errors.Add(new Error(
                ErrorCode.YearNotSupported,
                $"Year {year} is not supported."));
            return Result<CapabilityInfo>.Failure(errors);
        }

        var empType = constantsProvider.GetEmployeeType(employeeType);
        if (empType is null)
        {
            errors.Add(new Error(
                ErrorCode.MissingEmployeeTypeDefinition,
                $"Employee type {employeeType.Value} is not defined."));
            return Result<CapabilityInfo>.Failure(errors);
        }

        // Create minimal context for capability determination
        var context = new ValidationContext
        {
            Year = year,
            Mode = mode,
            EmployeeType = employeeType,
            IsPensioner = isPensioner,
            Months = [],
        };

        var disabledCapabilities = DetermineDisabledCapabilities(context, empType, yearParam);
        return Result<CapabilityInfo>.Success(new CapabilityInfo(disabledCapabilities));
    }

    /// <summary>
    /// Validates configuration providers at startup.
    /// </summary>
    public Result<Unit> ValidateProviders(
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider)
    {
        var errors = new List<Error>();

        // Check year provider has data
        if (yearProvider.AvailableYears.Count == 0)
        {
            errors.Add(Error.Configuration(
                ErrorCode.InvalidYearParameterProvider,
                "Year parameter provider has no available years."));
        }

        // Check all 10 known employee types exist
        for (var id = 1; id <= 10; id++)
        {
            if (constantsProvider.GetEmployeeType(id) is null)
            {
                errors.Add(Error.Configuration(
                    ErrorCode.MissingEmployeeTypeDefinition,
                    $"Employee type {id} is not defined in constants provider."));
            }
        }

        return errors.Count > 0
            ? Result<Unit>.Failure(errors)
            : Result<Unit>.Success(Unit.Value);
    }

    private static void ValidateInput(
        ValidationContext context,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider,
        List<Error> errors)
    {
        // Year validation
        if (context.Year == 0)
        {
            errors.Add(new Error(ErrorCode.YearNotSpecified, "Calculation year is required."));
        }
        else if (yearProvider.GetParameter(context.Year) is null)
        {
            errors.Add(new Error(
                ErrorCode.YearNotSupported,
                $"Year {context.Year} is not supported. Available years: {string.Join(", ", yearProvider.AvailableYears)}"));
        }

        // Employee type validation
        if (context.EmployeeType.Value == 0)
        {
            errors.Add(new Error(ErrorCode.MissingEmployeeTypeDefinition, "Employee type is required."));
        }
        else if (constantsProvider.GetEmployeeType(context.EmployeeType) is null)
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

            if (month.Salary <= 0)
            {
                errors.Add(Error.Validation(
                    ErrorCode.InvalidSalaryAmount,
                    $"Month {i + 1}: Salary must be positive.",
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
    /// Determines which capabilities are disabled based on Angular UI logic.
    /// </summary>
    private static Capability DetermineDisabledCapabilities(
        ValidationContext context,
        EmployeeTypeConstant employeeType,
        YearParameter yearParam)
    {
        var disabled = Capability.None;

        // Rule 1: AGI Selection
        // Angular: disabled = !AGIApplicable || minWageEmployeeTaxExemption
        if (!employeeType.AgiApplicable || yearParam.MinWageEmployeeTaxExemption)
        {
            disabled |= Capability.AgiSelection;
        }

        // Rule 2: Education Type
        // Angular: disabled = !employerEducationIncomeTaxExemption
        if (!employeeType.EmployerEducationIncomeTaxExemption)
        {
            disabled |= Capability.EducationType;
        }

        // Rule 3: Min Wage Exemption
        // Angular: disabled = !minWageEmployeeTaxExemption
        if (!yearParam.MinWageEmployeeTaxExemption)
        {
            disabled |= Capability.MinWageExemption;
        }

        // Rule 4: AGI Calculation
        // Angular: disabled = minWageEmployeeTaxExemption
        if (yearParam.MinWageEmployeeTaxExemption)
        {
            disabled |= Capability.AgiCalculation;
        }

        // Rule 5: 5746 Discount
        // Angular: disabled = isPensioner || !employerSGKDiscount5746Applicable
        if (context.IsPensioner || !employeeType.EmployerSgkDiscount5746Applicable)
        {
            disabled |= Capability.Discount5746;
        }

        // Rule 6: R&D Days Input
        // Angular: disabled = !researchAndDevelopmentTaxExemption
        if (!employeeType.ResearchAndDevelopmentTaxExemption)
        {
            disabled |= Capability.RnDDaysInput;
        }

        // Rule 7: AGI Included In Net
        // Angular: disabled = selectedCalcMode != 'NET_TO_GROSS'
        if (context.Mode != CalculationMode.NetToGross)
        {
            disabled |= Capability.AgiIncludedInNet;
        }

        return disabled;
    }

    /// <summary>
    /// Checks if user provided settings that conflict with disabled capabilities.
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

/// <summary>
/// Unit type for Result{Unit} where no value is needed.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Gets the singleton Unit value.
    /// </summary>
    public static Unit Value => default;
}
