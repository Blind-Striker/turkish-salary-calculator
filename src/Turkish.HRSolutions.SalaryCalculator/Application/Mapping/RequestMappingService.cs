using Turkish.HRSolutions.SalaryCalculator.Api.Requests;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Mapping;

/// <summary>
/// Maps V2 API requests to domain model types.
/// </summary>
internal static class RequestMappingService
{
    /// <summary>
    /// Maps a V2 API request to domain parameters and calculation options.
    /// </summary>
    /// <param name="year">The calculation year.</param>
    /// <param name="months">Monthly inputs from the request.</param>
    /// <param name="employeeType">The employee type identifier.</param>
    /// <param name="disability">The disability degree identifier.</param>
    /// <param name="isPensioner">Whether the employee is a pensioner.</param>
    /// <param name="applyMinWageExemption">Whether to apply minimum wage tax exemption.</param>
    /// <param name="apply5746Discount">Whether to apply 5746 SGK discount.</param>
    /// <param name="agi">AGI settings (nullable).</param>
    /// <param name="rnd">R&amp;D settings (nullable).</param>
    /// <param name="yearProvider">Year parameter provider.</param>
    /// <param name="constantsProvider">Calculation constants provider.</param>
    /// <returns>A result containing the mapped domain parameters, or errors.</returns>
    internal static Result<(EmployeeYearlyParameters YearlyParams, CalculationOptions Options)> MapToDomain(
        int year,
        IReadOnlyList<MonthlyInput> months,
        EmployeeTypeId employeeType,
        DisabilityDegreeId disability,
        bool isPensioner,
        bool applyMinWageExemption,
        bool apply5746Discount,
        AgiSettings? agi,
        RnDSettings? rnd,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider)
    {
        // 1. Look up year parameter
        var yearParameter = yearProvider.GetParameter(year);
        if (yearParameter is null)
        {
            return Result<(EmployeeYearlyParameters, CalculationOptions)>.Failure(
                ErrorCode.YearNotSupported,
                $"Year {year} is not supported. Available years: {string.Join(", ", yearProvider.AvailableYears)}");
        }

        // 2. Look up employee type constant
        var employeeTypeConstant = constantsProvider.GetEmployeeType(employeeType);
        if (employeeTypeConstant is null)
        {
            return Result<(EmployeeYearlyParameters, CalculationOptions)>.Failure(
                ErrorCode.MissingEmployeeTypeDefinition,
                $"Employee type with ID {employeeType.Value} is not defined.");
        }

        // 3. Standard employee type (always ID=1, used for exemption baselines)
        var standardEmployeeTypeConstant = constantsProvider.GetEmployeeType(EmployeeTypeId.Standard);
        if (standardEmployeeTypeConstant is null)
        {
            return Result<(EmployeeYearlyParameters, CalculationOptions)>.Failure(
                ErrorCode.MissingEmployeeTypeDefinition,
                "Standard employee type (ID=1) is missing from configuration.");
        }

        // 4. Look up disability constant
        var disabilityConstant = constantsProvider.GetDisability(disability);
        if (disabilityConstant is null)
        {
            return Result<(EmployeeYearlyParameters, CalculationOptions)>.Failure(
                ErrorCode.InvalidConstantsProvider,
                $"Disability degree with value {disability.Value} is not defined.");
        }

        // 5. Look up education exemption rate
        var educationType = rnd?.Education ?? EducationTypeId.MastersOrFundamentalSciences;
        var educationConstant = constantsProvider.GetEducationType(educationType);
        var educationExemptionRate = educationConstant?.ExemptionRate ?? 0d;

        // 6. Look up AGI rate
        var agiRate = ResolveAgiRate(agi, constantsProvider);

        // 7. Convert MonthlyInput[] → MonthlySalary[]
        var monthlySalaries = MapMonthlyInputs(months);

        // 8. Build domain parameters
        var yearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParameter,
            MonthlySalaries: monthlySalaries,
            EmployeeTypeConstant: employeeTypeConstant,
            StandardEmployeeTypeConstant: standardEmployeeTypeConstant,
            CalculationConstants: constantsProvider.Constants,
            EmployeeEducationExemptionRate: educationExemptionRate,
            AgiRate: agiRate,
            DisabilityDegree: disabilityConstant.Degree,
            IsPensioner: isPensioner);

        // 9. Build calculation options
        var options = new CalculationOptions(
            ApplyMinWageTaxExemption: applyMinWageExemption,
            ApplyEmployerDiscount5746: apply5746Discount,
            IsAgiCalculationEnabled: agi is not null,
            IsAgiIncludedTax: agi?.IncludeInTax ?? false,
            IsAgiIncludedNet: agi?.IncludeInNet ?? false);

        return Result<(EmployeeYearlyParameters, CalculationOptions)>.Success((yearlyParams, options));
    }

    /// <summary>
    /// Converts V2 MonthlyInput values to domain MonthlySalary values.
    /// </summary>
    internal static IEnumerable<MonthlySalary> MapMonthlyInputs(IReadOnlyList<MonthlyInput> months)
    {
        return months.Select(m => new MonthlySalary(
            MonthsOfYear: m.Month,
            SalaryAmount: m.Salary,
            WorkedDay: (uint)m.WorkedDays,
            ResearchAndDevelopmentWorkedDays: (uint)m.RnDDays));
    }

    /// <summary>
    /// Resolves the AGI rate from settings using the same lookup logic as the legacy service.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="SpouseStatus.EffectiveAgiTypeKey"/> for direct lookup against AGI constants.
    /// </remarks>
    internal static double ResolveAgiRate(AgiSettings? agi, ICalculationConstantsProvider constantsProvider)
    {
        if (agi is null)
        {
            return ResolveDefaultAgiRate(constantsProvider);
        }

        var agiOptions = constantsProvider.AllAgiOptions;
        if (agiOptions.Count == 0)
        {
            return 0d;
        }

        // Use SpouseStatus.EffectiveAgiTypeKey directly - no mapping needed
        var typeFilter = agi.SpouseStatus.EffectiveAgiTypeKey;

        var filtered = agiOptions.Where(a => a.Type.Equals(typeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        if (filtered.Count == 0)
        {
            return 0d;
        }

        // Unmarried has a single entry (no children matching)
        if (agi.SpouseStatus == SpouseStatus.Unmarried)
        {
            return filtered[0].Rate;
        }

        // Match by equality operator + children count (same as legacy FindAgiConstant)
        var numberOfChildren = agi.NumberOfChildren;
        var match = filtered.FirstOrDefault(p => p.Equality switch
        {
            "Eq" => numberOfChildren == p.Children,
            "Gt" => numberOfChildren > p.Children,
            "Gte" => numberOfChildren >= p.Children,
            "Lt" => numberOfChildren < p.Children,
            "Lte" => numberOfChildren <= p.Children,
            _ => false,
        });

        if (match is not null)
        {
            return match.Rate;
        }

        // Fallback: closest match by children difference
        return filtered.OrderBy(p => Math.Abs(p.Children - numberOfChildren)).First().Rate;
    }

    private static double ResolveDefaultAgiRate(ICalculationConstantsProvider constantsProvider)
    {
        var agiOptions = constantsProvider.AllAgiOptions;
        if (agiOptions.Count == 0)
        {
            return 0d;
        }

        // Default: unmarried with 0 children - use const key for lookup
        var unmarried = agiOptions.FirstOrDefault(a => a.Type.Equals(SpouseStatus.UnmarriedKey, StringComparison.OrdinalIgnoreCase));
        return unmarried?.Rate ?? 0d;
    }
}
