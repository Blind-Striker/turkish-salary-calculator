using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ISalaryCalculatorMetadata"/> that provides
/// calculator metadata, constants, and capabilities.
/// </summary>
internal sealed class SalaryCalculatorMetadataService : ISalaryCalculatorMetadata
{
    private readonly ICapabilityResolver _capabilityResolver;
    private readonly IYearParameterProvider _yearProvider;
    private readonly ICalculationConstantsProvider _constantsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SalaryCalculatorMetadataService"/> class.
    /// </summary>
    /// <param name="capabilityResolver">Resolver for determining calculator capabilities.</param>
    /// <param name="yearProvider">Provider for year-specific parameters.</param>
    /// <param name="constantsProvider">Provider for calculation constants.</param>
    public SalaryCalculatorMetadataService(
        ICapabilityResolver capabilityResolver,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider)
    {
        _capabilityResolver = capabilityResolver ?? throw new ArgumentNullException(nameof(capabilityResolver));
        _yearProvider = yearProvider ?? throw new ArgumentNullException(nameof(yearProvider));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
    }

    /// <inheritdoc />
    public IReadOnlyList<int> GetAvailableYears() => _yearProvider.AvailableYears;

    /// <inheritdoc />
    public IReadOnlyList<EmployeeTypeInfo> GetEmployeeTypes()
    {
        return
        [
            .. _constantsProvider
                .AllEmployeeTypes
                .Where(t => t.Show) // Only include types marked as visible
                .OrderBy(t => t.Order)
                .Select(t => new EmployeeTypeInfo(EmployeeTypeId.FromId(t.Id), t.Text, t.Desc)),
        ];
    }

    /// <inheritdoc />
    public IReadOnlyList<EducationTypeInfo> GetEducationTypes()
    {
        return
        [
            .. _constantsProvider
                .AllEducationTypes
                .Select(t => new EducationTypeInfo(EducationTypeId.FromId(t.Id), t.Text)),
        ];
    }

    /// <inheritdoc />
    public IReadOnlyList<DisabilityDegreeInfo> GetDisabilityDegrees()
    {
        // Return static list of disability degrees
        // These are fixed by Turkish law (None, 1st, 2nd, 3rd degree)
        return
        [
            new DisabilityDegreeInfo(DisabilityDegreeId.None, "Engelli Değil"),
            new DisabilityDegreeInfo(DisabilityDegreeId.First, "1. Derece Engelli"),
            new DisabilityDegreeInfo(DisabilityDegreeId.Second, "2. Derece Engelli"),
            new DisabilityDegreeInfo(DisabilityDegreeId.Third, "3. Derece Engelli"),
        ];
    }

    /// <inheritdoc />
    public Result<CapabilityInfo> GetCapabilities(int year, EmployeeTypeId? employeeType = null, bool isPensioner = false)
    {
        // Delegate directly to ICapabilityResolver - no more static mode hack
        return _capabilityResolver.ResolveCapabilities(
            year,
            employeeType ?? EmployeeTypeId.Standard,
            isPensioner,
            CalculationMode.GrossToNet); // Mode only affects AgiIncludedInNet, which is N/A for metadata queries
    }

    /// <inheritdoc />
    public bool IsCapabilityAvailable(Capability capability, int year, EmployeeTypeId? employeeType = null, bool isPensioner = false)
    {
        var result = GetCapabilities(year, employeeType, isPensioner);
        return result.IsSuccess && result.Value.IsAvailable(capability);
    }
}
