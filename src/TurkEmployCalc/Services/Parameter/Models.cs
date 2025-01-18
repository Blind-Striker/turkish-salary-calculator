#pragma warning disable MA0048 // File name must match type name (Models.cs) - disabled because of the record types

namespace TurkHRSolutions.TurkEmployCalc.Services.Parameter;

public record TurkEmployCalcParameters(
    IEnumerable<YearParameter> YearParameters,
    IEnumerable<MinimumLivingAllowanceParameter> MinimumLivingAllowanceParameters,
    IEnumerable<EmployeeTypeParameter> EmployeeTypeParameters,
    CalculationConstants CalculationConstantParameters,
    IEnumerable<EmployeeEducationType> EmployeeEducationTypeParameters,
    IEnumerable<DisabilityType> DisabilityTypeParameters
);

public record YearParameter(int Year, IEnumerable<MinGrossWageParameter> MinGrossWages, bool MinWageEmployeeTaxExemption,
    IEnumerable<TaxSliceParameter> TaxSlices, IEnumerable<DisabledMonthlyIncomeTaxDiscountBaseParameter> DisabledMonthlyIncomeTaxDiscountBases);

public record MinGrossWageParameter(int StartMonth, double Amount, double SgkCeil);

public record TaxSliceParameter(double Rate, double? Ceil);

public record DisabledMonthlyIncomeTaxDiscountBaseParameter(int Degree, double Amount);

public record MinimumLivingAllowanceParameter(SpouseWorkStatus SpouseWorkStatus, ChildrenCompareType ChildrenCompareType, uint NumberOfChildren, double Rate);

public record EmployeeTypeParameter
(
    string Code,
    string Description,
    bool SgkApplicable,
    bool SgkMinWageBasedExemption,
    bool SgkMinWageBased17103Exemption,
    bool UnemploymentInsuranceApplicable,
    bool AgiApplicable,
    bool IncomeTaxApplicable,
    bool TaxMinWageBasedExemption,
    bool ResearchAndDevelopmentTaxExemption,
    bool StampTaxApplicable,
    bool EmployerSgkApplicable,
    bool EmployerUnemploymentInsuranceApplicable,
    bool EmployerIncomeTaxApplicable,
    bool EmployerStampTaxApplicable,
    bool EmployerEducationIncomeTaxExemption,
    bool EmployerSgkShareTotalExemption,
    bool EmployerSgkDiscount5746Applicable,
    bool Employer5746AdditionalDiscountApplicable
);

public record CalculationConstants(uint MonthDayCount, double StampTaxRate, EmployeeConstants Employee, EmployerConstants Employer);

public record EmployeeConstants(double SgkDeductionRate, double SgdpDeductionRate, double UnemploymentInsuranceRate, double PensionerUnemploymentInsuranceRate);

public record EmployerConstants(double SgkDeductionRate, double SgdpDeductionRate, double EmployerDiscount5746, double UnemploymentInsuranceRate,
    double PensionerUnemploymentInsuranceRate, double Sgk5746AdditionalDiscount);

public record EmployeeEducationType(string Code, string Description, double ExemptionRate);

public record DisabilityType(string Code, string Description, int Degree);