using Turkish.HRSolutions.SalaryCalculator.Api;
using Turkish.HRSolutions.SalaryCalculator.Api.Mappings;
using Turkish.HRSolutions.SalaryCalculator.Api.Requests;
using Turkish.HRSolutions.SalaryCalculator.Api.Responses;
using Turkish.HRSolutions.SalaryCalculator.Application.Mapping;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

/// <summary>
/// V2 implementation of <see cref="ISalaryCalculator"/> that maps V2 API requests
/// to the domain calculation engine and returns immutable snapshots.
/// </summary>
internal sealed class V2SalaryCalculatorService : ISalaryCalculator
{
    private readonly IYearParameterProvider _yearProvider;
    private readonly ICalculationConstantsProvider _constantsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="V2SalaryCalculatorService"/> class.
    /// </summary>
    /// <param name="yearProvider">Provider for year-specific parameters.</param>
    /// <param name="constantsProvider">Provider for calculation constants.</param>
    internal V2SalaryCalculatorService(IYearParameterProvider yearProvider, ICalculationConstantsProvider constantsProvider)
    {
        _yearProvider = yearProvider ?? throw new ArgumentNullException(nameof(yearProvider));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
    }

    /// <inheritdoc />
    public Result<IYearParameterProvider> YearProvider =>
        Result<IYearParameterProvider>.Success(_yearProvider);

    /// <inheritdoc />
    public Result<ICalculationConstantsProvider> ConstantsProvider =>
        Result<ICalculationConstantsProvider>.Success(_constantsProvider);

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(GrossToNetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MapAndCalculate(request.Year, request.Months, request.EmployeeType, request.Disability,
            request.IsPensioner, request.ApplyMinWageExemption, request.Apply5746Discount,
            request.Agi, request.RnD, CalculationMode.GrossToNet);
    }

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(NetToGrossRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MapAndCalculate(request.Year, request.Months, request.EmployeeType, request.Disability,
            request.IsPensioner, request.ApplyMinWageExemption, request.Apply5746Discount,
            request.Agi, request.RnD, CalculationMode.NetToGross);
    }

    /// <inheritdoc />
    public Result<YearlySalarySnapshot> Calculate(TotalToGrossRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MapAndCalculate(request.Year, request.Months, request.EmployeeType, request.Disability,
            request.IsPensioner, request.ApplyMinWageExemption, request.Apply5746Discount,
            request.Agi, request.RnD, CalculationMode.TotalToGross);
    }

    private Result<YearlySalarySnapshot> MapAndCalculate(
        int year,
        IReadOnlyList<MonthlyInput> months,
        EmployeeTypeId employeeType,
        DisabilityDegreeId disability,
        bool isPensioner,
        bool applyMinWageExemption,
        bool apply5746Discount,
        AgiSettings? agi,
        RnDSettings? rnd,
        CalculationMode mode)
    {
        // 1. Map request parameters to domain types
        var mappingResult = RequestMappingService.MapToDomain(
            year, months, employeeType, disability, isPensioner,
            applyMinWageExemption, apply5746Discount, agi, rnd,
            _yearProvider, _constantsProvider);

        if (mappingResult.IsFailure)
        {
            return Result<YearlySalarySnapshot>.Failure(mappingResult.Errors);
        }

        var (yearlyParams, options) = mappingResult.Value;

        // 2. Execute domain calculation
        try
        {
            var yearCalc = new YearCalculationModel(yearlyParams, options);
            yearCalc.Calculate(mode);

            // 3. Map domain result to immutable snapshot
            var snapshot = yearCalc.ToSnapshot(year);
            return Result<YearlySalarySnapshot>.Success(snapshot);
        }
        catch (InvalidOperationException ex)
        {
            // Domain layer throws for R&D days > worked days and binary search failures.
            // This will be replaced by proper validation in Phase 4.
            return Result<YearlySalarySnapshot>.Failure(
                ErrorCode.CalculationFailed,
                ex.Message,
                ex);
        }
    }
}
