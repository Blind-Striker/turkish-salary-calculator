using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICapabilityResolver"/> that determines
/// calculator capabilities based on year parameters and employee type constants.
/// </summary>
internal sealed class CapabilityResolver : ICapabilityResolver
{
    private readonly IYearParameterProvider _yearProvider;
    private readonly ICalculationConstantsProvider _constantsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilityResolver"/> class.
    /// </summary>
    /// <param name="yearProvider">Provider for year-specific parameters.</param>
    /// <param name="constantsProvider">Provider for calculation constants.</param>
    public CapabilityResolver(IYearParameterProvider yearProvider, ICalculationConstantsProvider constantsProvider)
    {
        _yearProvider = yearProvider ?? throw new ArgumentNullException(nameof(yearProvider));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
    }

    /// <inheritdoc />
    public Result<CapabilityInfo> ResolveCapabilities(int year, EmployeeTypeId employeeType, bool isPensioner, CalculationMode mode)
    {
        // Validate year
        var yearParam = _yearProvider.GetParameter(year);
        if (yearParam is null)
        {
            return Result<CapabilityInfo>.Failure(ErrorCode.YearNotSupported, $"Year {year} is not supported.");
        }

        // Validate employee type
        var empType = _constantsProvider.GetEmployeeType(employeeType);
        if (empType is null)
        {
            return Result<CapabilityInfo>.Failure(ErrorCode.MissingEmployeeTypeDefinition, $"Employee type {employeeType.Value} is not defined.");
        }

        // Determine disabled capabilities
        var disabledCapabilities = DetermineDisabledCapabilities(empType, yearParam, isPensioner, mode);
        return Result<CapabilityInfo>.Success(new CapabilityInfo(disabledCapabilities));
    }

    /// <inheritdoc />
    public Result ValidateProviders()
    {
        var errors = new List<Error>();

        // Check year provider has data
        if (_yearProvider.AvailableYears.Count == 0)
        {
            errors.Add(Error.Configuration(ErrorCode.InvalidYearParameterProvider, "Year parameter provider has no available years."));
        }

        // Check all 10 known employee types exist
        for (var id = 1; id <= 10; id++)
        {
            if (_constantsProvider.GetEmployeeType(id) is null)
            {
                errors.Add(Error.Configuration(ErrorCode.MissingEmployeeTypeDefinition, $"Employee type {id} is not defined in constants provider."));
            }
        }

        return errors.Count > 0
            ? Result.Failure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Determines which capabilities are disabled based on configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Capability rules (derived from Angular maas-hesaplama):
    /// </para>
    /// <list type="bullet">
    /// <item><description>AGI Selection: disabled when !AGIApplicable || minWageEmployeeTaxExemption</description></item>
    /// <item><description>Education Type: disabled when !employerEducationIncomeTaxExemption</description></item>
    /// <item><description>Min Wage Exemption: disabled when !minWageEmployeeTaxExemption</description></item>
    /// <item><description>AGI Calculation: disabled when minWageEmployeeTaxExemption</description></item>
    /// <item><description>5746 Discount: disabled when isPensioner || !employerSGKDiscount5746Applicable</description></item>
    /// <item><description>R&amp;D Days Input: disabled when !researchAndDevelopmentTaxExemption</description></item>
    /// <item><description>AGI Included In Net: disabled when mode != NetToGross</description></item>
    /// </list>
    /// </remarks>
    private static Capability DetermineDisabledCapabilities(EmployeeTypeConstant employeeType, YearParameter yearParam, bool isPensioner, CalculationMode mode)
    {
        var disabled = Capability.None;

        // Rule 1: AGI Selection
        // Disabled = !AGIApplicable || minWageEmployeeTaxExemption
        if (!employeeType.AgiApplicable || yearParam.MinWageEmployeeTaxExemption)
        {
            disabled |= Capability.AgiSelection;
        }

        // Rule 2: Education Type
        // Disabled = !employerEducationIncomeTaxExemption
        if (!employeeType.EmployerEducationIncomeTaxExemption)
        {
            disabled |= Capability.EducationType;
        }

        // Rule 3: Min Wage Exemption
        // Disabled = !minWageEmployeeTaxExemption
        if (!yearParam.MinWageEmployeeTaxExemption)
        {
            disabled |= Capability.MinWageExemption;
        }

        // Rule 4: AGI Calculation
        // Disabled = minWageEmployeeTaxExemption
        if (yearParam.MinWageEmployeeTaxExemption)
        {
            disabled |= Capability.AgiCalculation;
        }

        // Rule 5: 5746 Discount
        // Disabled = isPensioner || !employerSGKDiscount5746Applicable
        if (isPensioner || !employeeType.EmployerSgkDiscount5746Applicable)
        {
            disabled |= Capability.Discount5746;
        }

        // Rule 6: R&D Days Input
        // Disabled = !researchAndDevelopmentTaxExemption
        if (!employeeType.ResearchAndDevelopmentTaxExemption)
        {
            disabled |= Capability.RnDDaysInput;
        }

        // Rule 7: AGI Included In Net
        // Disabled = selectedCalcMode != 'NET_TO_GROSS'
        if (mode != CalculationMode.NetToGross)
        {
            disabled |= Capability.AgiIncludedInNet;
        }

        return disabled;
    }
}
