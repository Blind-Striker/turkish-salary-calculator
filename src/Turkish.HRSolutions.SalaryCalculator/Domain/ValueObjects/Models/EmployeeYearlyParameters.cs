using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Models;

public record EmployeeYearlyParameters(
    YearParameter YearParameter,
    IEnumerable<MonthlySalary> MonthlySalaries,
    EmployeeTypeConstant EmployeeTypeConstant,
    EmployeeTypeConstant StandardEmployeeTypeConstant,
    CalculationConstant CalculationConstants,
    double EmployeeEducationExemptionRate,
    double AgiRate,
    int DisabilityDegree,
    bool IsPensioner = false);
