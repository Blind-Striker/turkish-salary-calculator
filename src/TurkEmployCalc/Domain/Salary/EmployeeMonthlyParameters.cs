namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary;

public record EmployeeMonthlyParameters(YearParameter YearParameter, EmployeeTypeParameter EmployeeTypeParameter, CalculationConstants CalculationConstants,
    double EducationExemptionRate, double MinimumLivingAllowanceRate, int DisabilityDegree, MonthlySalary MonthlySalary, bool IsPensioner = false);