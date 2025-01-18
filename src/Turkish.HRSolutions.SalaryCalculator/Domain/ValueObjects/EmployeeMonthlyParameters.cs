namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

public record EmployeeMonthlyParameters(
    YearParameter YearParameter,
    MonthlySalary MonthlySalary,
    EmployeeTypeConstant EmployeeTypeConstant,
    CalculationConstant CalculationConstants,
    double EmployeeEducationExemptionRate,
    double AgiRate,
    int DisabilityDegree,
    bool IsPensioner = false);
