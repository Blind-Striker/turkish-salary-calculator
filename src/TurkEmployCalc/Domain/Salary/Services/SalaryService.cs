namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.Services;

public class SalaryService
{
    private readonly TurkEmployCalcParameters _turkEmployCalcParameters;

    public SalaryService(TurkEmployCalcParameters turkEmployCalcParameters)
    {
        _turkEmployCalcParameters = turkEmployCalcParameters;
    }

    public NetToGrossYearlyCalcModel CalculateNetToGrossMonthly(CalculateSalaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate request parameters

        var yearParameter = _turkEmployCalcParameters.YearParameters.Single(parameter => parameter.Year == request.Year);
        var employeeTypeParameter = _turkEmployCalcParameters.EmployeeTypeParameters.SingleOrDefault(parameter =>
            string.Equals(parameter.Code, request.EmployeeTypeCode, StringComparison.Ordinal));
        var educationTypeParameter = _turkEmployCalcParameters.EmployeeEducationTypeParameters.SingleOrDefault(parameter =>
            string.Equals(parameter.Code, request.EducationTypeCode, StringComparison.Ordinal));
        var disabilityTypeParameter = _turkEmployCalcParameters.DisabilityTypeParameters.SingleOrDefault(parameter =>
            string.Equals(parameter.Code, request.DisabilityTypeCode, StringComparison.Ordinal));

        var minimumLivingAllowanceParameter =
            _turkEmployCalcParameters.MinimumLivingAllowanceParameters.FindMinimumLivingAllowance(request.SpouseWorkStatus, request.NumberOfChildren);

        var employeeModelParameters = new EmployeeYearlyParameters(yearParameter, employeeTypeParameter!,
            _turkEmployCalcParameters.CalculationConstantParameters, educationTypeParameter!.ExemptionRate, minimumLivingAllowanceParameter.Rate,
            disabilityTypeParameter!.Degree, request.MonthlySalaries, request.IsPensioner);

        var netToGrossYearlyCalcModel = new NetToGrossYearlyCalcModel(employeeModelParameters);

        return netToGrossYearlyCalcModel;
    }

    public GrossToNetYearlyCalcModel CalculateGrossToNetMonthly(CalculateSalaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate rquest parameters

        var yearParameter = _turkEmployCalcParameters.YearParameters.Single(parameter => parameter.Year == request.Year);
        var employeeTypeParameter = _turkEmployCalcParameters.EmployeeTypeParameters
            .SingleOrDefault(parameter => string.Equals(parameter.Code, request.EmployeeTypeCode, StringComparison.Ordinal));
        var educationTypeParameter = _turkEmployCalcParameters.EmployeeEducationTypeParameters
            .SingleOrDefault(parameter => string.Equals(parameter.Code, request.EducationTypeCode, StringComparison.Ordinal));
        var disabilityTypeParameter = _turkEmployCalcParameters.DisabilityTypeParameters
            .SingleOrDefault(parameter => string.Equals(parameter.Code, request.DisabilityTypeCode, StringComparison.Ordinal));

        var minimumLivingAllowanceParameter =
            _turkEmployCalcParameters.MinimumLivingAllowanceParameters.FindMinimumLivingAllowance(request.SpouseWorkStatus, request.NumberOfChildren);

        var employeeModelParameters = new EmployeeYearlyParameters(yearParameter, employeeTypeParameter!, _turkEmployCalcParameters.CalculationConstantParameters,
            educationTypeParameter!.ExemptionRate, minimumLivingAllowanceParameter.Rate, disabilityTypeParameter!.Degree, request.MonthlySalaries, request.IsPensioner);

        var netToGrossYearlyCalcModel = new GrossToNetYearlyCalcModel(employeeModelParameters);

        return netToGrossYearlyCalcModel;
    }
}