using Turkish.HRSolutions.SalaryCalculator.Application.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Requests;

/// <summary>
/// Request parameters for salary calculation.
/// </summary>
/// <param name="Year">The tax year for calculation (e.g., 2026).</param>
/// <param name="MonthlySalaries">Monthly salary entries (1-12 months).</param>
/// <param name="CalculationMode">Direction of calculation: GrossToNet, NetToGross, or TotalToGross.</param>
/// <param name="EmployeeType">Type of employee affecting tax exemptions and deductions.</param>
/// <param name="EmployeeEducationType">Education level for R&amp;D exemption calculations.</param>
/// <param name="DisabilityDegree">Disability degree for tax base deduction.</param>
/// <param name="SpouseWorkStatus">Spouse employment status for AGI calculation.</param>
/// <param name="NumberOfChildren">Number of children for AGI calculation.</param>
/// <param name="IsPensioner">Whether the employee is a pensioner (affects SGK rates).</param>
/// <param name="ApplyEmployerDiscount5746">Apply 5% employer SGK discount (Law 5746).</param>
/// <param name="ApplyMinWageTaxExemption">Apply minimum wage tax exemption (default true for 2022+).</param>
/// <param name="IsAgiCalculationEnabled">Enable AGI calculation (only relevant for years before 2022).</param>
/// <param name="IsAgiIncludedTax">Include AGI in tax calculations.</param>
/// <param name="IsAgiIncludedNet">Include AGI in net salary calculations.</param>
public record CalculateSalaryRequest(
    uint Year,
    IEnumerable<MonthlySalary> MonthlySalaries,
    CalculationMode CalculationMode = CalculationMode.GrossToNet,
    EmployeeType EmployeeType = EmployeeType.StandardEmployee,
    EmployeeEducationType EmployeeEducationType = EmployeeEducationType.MastersDegreeOrBachelorsInFundamentalSciences,
    DisabilityDegree DisabilityDegree = DisabilityDegree.None,
    SpouseWorkStatus SpouseWorkStatus = SpouseWorkStatus.Unmarried,
    uint NumberOfChildren = 0,
    bool IsPensioner = false,
    bool ApplyEmployerDiscount5746 = false,
    bool ApplyMinWageTaxExemption = true,
    bool IsAgiCalculationEnabled = false,
    bool IsAgiIncludedTax = false,
    bool IsAgiIncludedNet = false);
