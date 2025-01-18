using Turkish.HRSolutions.SalaryCalculator.Application.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Requests;

public record CalculateSalaryRequest(
    uint Year,
    IEnumerable<MonthlySalary> MonthlySalaries,
    EmployeeType EmployeeType = EmployeeType.StandardEmployee,
    EmployeeEducationType EmployeeEducationType = EmployeeEducationType.MastersDegreeOrBachelorsInFundamentalSciences,
    DisabilityDegree DisabilityDegree = DisabilityDegree.None,
    SpouseWorkStatus SpouseWorkStatus = SpouseWorkStatus.Unmarried,
    uint NumberOfChildren = 0,
    bool IsPensioner = false,
    bool ApplyEmployerDiscount5746 = false,
    bool IsAgiIncludedTax = false,
    bool IsAgiIncludedNet = false);
