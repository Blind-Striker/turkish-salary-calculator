using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

public record MonthlySalary(MonthsOfYear MonthsOfYear, decimal SalaryAmount, uint WorkedDay = 30, uint ResearchAndDevelopmentWorkedDays = 0);
