namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

public record EmployeeYearlyParameters(
    YearParameter YearParameter,
    IEnumerable<MonthlySalary> MonthlySalaries,
    EmployeeTypeConstant EmployeeTypeConstant,
    CalculationConstant CalculationConstants,
    double EmployeeEducationExemptionRate,
    double AgiRate,
    int DisabilityDegree,
    bool IsPensioner = false);
