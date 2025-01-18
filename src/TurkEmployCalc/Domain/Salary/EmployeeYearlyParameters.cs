namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary;

public record EmployeeYearlyParameters(YearParameter YearParameter, EmployeeTypeParameter EmployeeTypeParameter, CalculationConstants CalculationConstants,
    double EducationExemptionRate, double MinimumLivingAllowanceRate, int DisabilityDegree, IEnumerable<MonthlySalary> MonthlySalaries,
    bool IsPensioner = false);