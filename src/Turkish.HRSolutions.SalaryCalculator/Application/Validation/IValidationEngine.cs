using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

internal interface IValidationEngine
{
    public Result<CapabilityInfo> Validate(
        ValidationContext context,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider);

    public Result<CapabilityInfo> ValidateCapabilities(
        int year,
        EmployeeTypeId employeeType,
        bool isPensioner,
        CalculationMode mode,
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider);

    public Result<Unit> ValidateProviders(
        IYearParameterProvider yearProvider,
        ICalculationConstantsProvider constantsProvider);
}
