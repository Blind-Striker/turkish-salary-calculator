#pragma warning disable MA0048 // File name must match type name (CalculateNetToGrossMonthlyRequest.cs) - disabled because of the record types
namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary.Services;

public record CalculateSalaryRequest(uint Year, string EmployeeTypeCode, string EducationTypeCode, string DisabilityTypeCode,
    IEnumerable<MonthlySalary> MonthlySalaries, SpouseWorkStatus SpouseWorkStatus = SpouseWorkStatus.Unmarried, uint NumberOfChildren = 0,
    bool IsPensioner = false, bool ApplyEmployerDiscount5746 = false, bool IsAgiIncludedTax = false, bool IsAgiIncludedNet = false);

public record MonthlySalary(MonthsOfYear MonthsOfYear, double SalaryAmount, uint WorkedDay = 30, uint ResearchAndDevelopmentWorkedDays = 0);
