using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Models;

public record EmployeeMonthlyParameters(
    YearParameter YearParameter,
    MonthlySalary MonthlySalary,
    EmployeeTypeConstant EmployeeTypeConstant,
    EmployeeTypeConstant StandardEmployeeTypeConstant,
    CalculationConstant CalculationConstants,
    double EmployeeEducationExemptionRate,
    double AgiRate,
    int DisabilityDegree,
    bool IsPensioner = false);
