using Turkish.HRSolutions.SalaryCalculator.Application.Enums.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

/// <summary>
/// Service for calculating Turkish employee salaries with full support for
/// tax regulations, SGK, unemployment insurance, and various exemptions.
/// </summary>
public class SalaryCalculationService
{
    private readonly YearParameters _yearParameters;
    private readonly ConstantParameters _constantParameters;

    /// <summary>
    /// Initializes the salary calculation service with required parameters.
    /// </summary>
    /// <param name="yearParameters">Year-specific tax brackets and minimum wages.</param>
    /// <param name="constantParameters">Fixed calculation constants and employee type definitions.</param>
    public SalaryCalculationService(YearParameters yearParameters, ConstantParameters constantParameters)
    {
        _yearParameters = yearParameters;
        _constantParameters = constantParameters;
    }

    /// <summary>
    /// Calculates salary for an employee based on the provided request parameters.
    /// </summary>
    /// <param name="request">The salary calculation request containing all input parameters.</param>
    /// <returns>A <see cref="YearCalculationModel"/> containing monthly and aggregated calculation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the year parameter is not found.</exception>
    public YearCalculationModel CalculateSalary(CalculateSalaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1) Look up year-specific parameters
        var yearParameter = _yearParameters.Parameters.SingleOrDefault(parameter => parameter.Year == request.Year) ??
                            throw new InvalidOperationException($"No year parameter found for year={request.Year}");

        // 2) Resolve constants from enums
        var employeeTypeConstant = _constantParameters.EmployeeTypeConstants.FindEmployeeTypeConstant(request.EmployeeType);
        var standardEmployeeTypeConstant = _constantParameters.EmployeeTypeConstants.First(e => e.Id == 1);
        var employeeEducationTypeConstant = _constantParameters.EmployeeEducationContants.FindEmployeeEducationConstant(request.EmployeeEducationType);
        var disabilityConstant = _constantParameters.DisabilityConstants.FindDisabilityConstant(request.DisabilityDegree);
        var agiOption = _constantParameters.AgiConstants.FindAgiConstant(request.SpouseWorkStatus, request.NumberOfChildren);

        // 3) Build the domain's EmployeeYearlyParameters
        var employeeYearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParameter,
            MonthlySalaries: request.MonthlySalaries,
            EmployeeTypeConstant: employeeTypeConstant,
            StandardEmployeeTypeConstant: standardEmployeeTypeConstant,
            CalculationConstants: _constantParameters.CalculationConstants,
            EmployeeEducationExemptionRate: employeeEducationTypeConstant.ExemptionRate,
            AgiRate: agiOption.Rate,
            DisabilityDegree: disabilityConstant.Degree,
            IsPensioner: request.IsPensioner
        );

        // 4) Build the CalculationOptions from request toggles
        var calcOptions = new CalculationOptions(
            ApplyMinWageTaxExemption: request.ApplyMinWageTaxExemption,
            ApplyEmployerDiscount5746: request.ApplyEmployerDiscount5746,
            IsAgiCalculationEnabled: request.IsAgiCalculationEnabled,
            IsAgiIncludedTax: request.IsAgiIncludedTax,
            IsAgiIncludedNet: request.IsAgiIncludedNet
        );

        // 5) Construct the YearCalculationModel
        var yearCalc = new YearCalculationModel(employeeYearlyParams, calcOptions);

        // 6) Perform the yearly calculation with the specified mode
        yearCalc.Calculate(request.CalculationMode);

        // 7) Return the result for the caller to access aggregated properties
        return yearCalc;
    }
}
