using Turkish.HRSolutions.SalaryCalculator.Application.Mappings;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

/// <summary>
/// Implementation of <see cref="ISalaryCalculator"/> that maps API requests
/// to the domain calculation engine and returns immutable snapshots.
/// </summary>
internal sealed class SalaryCalculatorService : ISalaryCalculator
{
    private readonly IValidationEngine _validationEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="SalaryCalculatorService"/> class.
    /// </summary>
    /// <param name="validationEngine">Validation engine used for request and provider validation.</param>
    /// <param name="yearProvider">Provider for year-specific parameters.</param>
    /// <param name="constantsProvider">Provider for calculation constants.</param>
    internal SalaryCalculatorService(IValidationEngine validationEngine, IYearParameterProvider yearProvider, ICalculationConstantsProvider constantsProvider)
    {
        _validationEngine = validationEngine ?? throw new ArgumentNullException(nameof(validationEngine));
        YearProvider = yearProvider ?? throw new ArgumentNullException(nameof(yearProvider));
        ConstantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
    }

    /// <inheritdoc />
    public IYearParameterProvider YearProvider { get; }

    /// <inheritdoc />
    public ICalculationConstantsProvider ConstantsProvider { get; }

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(SalaryCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var context = ValidationContext.FromRequest(request);
        return ValidateAndCalculate(context, request.Mode);
    }

    private Result<YearlySalarySnapshot> ValidateAndCalculate(ValidationContext context, CalculationMode mode)
    {
        // 1. Validate using ValidationEngine (single source of truth)
        var validationResult = _validationEngine.Validate(context);

        if (validationResult.IsFailure)
        {
            return Result<YearlySalarySnapshot>.Failure(validationResult.Errors);
        }

        // Capture warnings from validation
        var warnings = validationResult.Warnings;

        // 2. Resolve lookups (guaranteed to succeed after validation)
        var yearParam = YearProvider.GetParameter(context.Year)!;
        var employeeType = ConstantsProvider.GetEmployeeType(context.EmployeeType)!;
        var standardType = ConstantsProvider.GetEmployeeType(EmployeeTypeId.Standard)!;
        var disability = ConstantsProvider.GetDisability(context.Disability)!;
        var educationType = context.RnD?.Education ?? EducationTypeId.OtherRnDPersonnel;
        var educationRate = ConstantsProvider.GetEducationType(educationType)?.ExemptionRate ?? 0d;
        var agiRate = ConstantsProvider.GetAgiRate(
            context.Agi?.SpouseStatus ?? SpouseStatus.Unmarried,
            context.Agi?.NumberOfChildren ?? 0);

        // 3. Map to domain types (pure conversion)
        var (yearlyParams, options) = RequestMappingService.MapToDomain(
            context,
            yearParam,
            employeeType,
            standardType,
            ConstantsProvider.Constants,
            disability.Degree,
            educationRate,
            agiRate);

        // 4. Execute domain calculation
        try
        {
            var yearCalc = new YearCalculationModel(yearlyParams, options);
            yearCalc.Calculate(mode);

            // 5. Map domain result to immutable snapshot
            var snapshot = yearCalc.ToSnapshot(context.Year);

            // Return success with any warnings from validation
            var warningList = warnings.ToList();
            return warningList.Count > 0
                ? Result<YearlySalarySnapshot>.Success(snapshot, warningList)
                : Result<YearlySalarySnapshot>.Success(snapshot);
        }
        catch (InvalidOperationException ex)
        {
            // Domain layer may still throw for edge cases (binary search failure)
            // This is a safety net - proper validation should catch most issues
            return Result<YearlySalarySnapshot>.Failure(ErrorCode.CalculationFailed, ex.Message, ex);
        }
        catch (Exception ex)
        {
            return Result<YearlySalarySnapshot>.Failure(ErrorCode.UnexpectedError, "Unhandled exception during salary calculation.", ex);
        }
    }
}
