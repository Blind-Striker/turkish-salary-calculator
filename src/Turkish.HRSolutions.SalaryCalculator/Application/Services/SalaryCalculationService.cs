using Turkish.HRSolutions.SalaryCalculator.Application.Enums.Extensions;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.Models;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Services;

public class SalaryCalculationService
{
    private readonly YearParameters _yearParameters;
    private readonly ConstantParameters _constantParameters;

    public SalaryCalculationService(YearParameters yearParameters, ConstantParameters constantParameters)
    {
        _yearParameters = yearParameters;
        _constantParameters = constantParameters;
    }

    public YearCalculationModel CalculateSalary(CalculateSalaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var yearParameter = _yearParameters.Parameters.SingleOrDefault(parameter => parameter.Year == request.Year) ??
                            throw new InvalidOperationException($"No year parameter found for year={request.Year}");
        var employeeTypeConstant = _constantParameters.EmployeeTypeConstants.FindEmployeeTypeConstant(request.EmployeeType);
        var employeeEducationTypeConstant = _constantParameters.EmployeeEducationContants.FindEmployeeEducationConstant(request.EmployeeEducationType);
        var disabilityConstant = _constantParameters.DisabilityConstants.FindDisabilityConstant(request.DisabilityDegree);
        var agiOption = _constantParameters.AgiConstants.FindAgiConstant(request.SpouseWorkStatus, request.NumberOfChildren);

        // 3) Build the domain's EmployeeYearlyParameters
        //    We pass the request's MonthlySalaries (which are already domain MonthlySalary? If not, you'd map them).
        var employeeYearlyParams = new EmployeeYearlyParameters(
            YearParameter: yearParameter,
            MonthlySalaries: request.MonthlySalaries, // uses the domain MonthlySalary struct
            EmployeeTypeConstant: employeeTypeConstant,
            CalculationConstants: _constantParameters.CalculationConstants,
            EmployeeEducationExemptionRate: employeeEducationTypeConstant.ExemptionRate,
            AgiRate: agiOption.Rate,
            DisabilityDegree: disabilityConstant.Degree,
            IsPensioner: request.IsPensioner
        );

        // 4) Build the CalculationOptions from toggles
        var calcOptions = new CalculationOptions(
            ApplyMinWageTaxExemption: false, // or false, if the user wants
            ApplyEmployerDiscount5746: request.ApplyEmployerDiscount5746,
            IsAgiCalculationEnabled: false, // or maybe user can toggle
            IsAgiIncludedTax: request.IsAgiIncludedTax,
            IsAgiIncludedNet: request.IsAgiIncludedNet
        );

        // 5) Construct the YearCalculationModel
        var yearCalc = new YearCalculationModel(employeeYearlyParams, calcOptions);

        // 6) Decide the CalculationMode. If you want a default "GrossToNet", do:
        //    Or if your request had "CalculationMode" we map that to the domain enum.
        var calcMode = CalculationMode.NetToGross; // you can also choose "NetToGross" or "TotalToGross"

        // 7) Perform the yearly calculation
        yearCalc.Calculate(calcMode);

        // 8) Return the result so the caller can read aggregator props
        //    (like yearCalc.NetSalary, yearCalc.EmployerTotalCost, etc.)
        return yearCalc;
    }
}
