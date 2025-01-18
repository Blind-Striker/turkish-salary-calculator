#pragma warning disable MA0048

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

public record ConstantParameters(
    [property: JsonPropertyName("CALCULATION_CONSTANTS")] CalculationConstant CalculationConstants,
    [property: JsonPropertyName("EMPLOYEE_EDUCATION_TYPES")] ImmutableList<EmployeeEducationTypeConstant> EmployeeEducationContants,
    [property: JsonPropertyName("DISABILITY_OPTIONS")] ImmutableList<DisabilityConstant> DisabilityConstants,
    [property: JsonPropertyName("EMPLOYEE_TYPES")] ImmutableList<EmployeeTypeConstant> EmployeeTypeConstants,
    [property: JsonPropertyName("AGI_OPTIONS")] ImmutableList<AgiConstant> AgiConstants
);

public record CalculationConstant(
    [property: JsonPropertyName("monthDayCount")] int MonthDayCount,
    [property: JsonPropertyName("stampTaxRate")] double StampTaxRate,
    [property: JsonPropertyName("employee")] EmployeeConstant Employee,
    [property: JsonPropertyName("employer")] EmployerConstant Employer
);

public record EmployeeConstant(
    [property: JsonPropertyName("SGKDeductionRate")] double SgkDeductionRate,
    [property: JsonPropertyName("SGDPDeductionRate")] double SgdpDeductionRate,
    [property: JsonPropertyName("unemploymentInsuranceRate")] double UnemploymentInsuranceRate,
    [property: JsonPropertyName("pensionerUnemploymentInsuranceRate")] double PensionerUnemploymentInsuranceRate
);

public record EmployerConstant(
    [property: JsonPropertyName("SGKDeductionRate")] double SgkDeductionRate,
    [property: JsonPropertyName("SGDPDeductionRate")] double SgdpDeductionRate,
    [property: JsonPropertyName("employerDiscount5746")] double EmployerDiscount5746,
    [property: JsonPropertyName("unemploymentInsuranceRate")] double UnemploymentInsuranceRate,
    [property: JsonPropertyName("pensionerUnemploymentInsuranceRate")] double PensionerUnemploymentInsuranceRate,
    [property: JsonPropertyName("SGK5746AdditionalDiscount")] double Sgk5746AdditionalDiscount
);

public record EmployeeEducationTypeConstant(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("exemptionRate")] double ExemptionRate
);

public record DisabilityConstant(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("degree")] int Degree,
    [property: JsonPropertyName("text")] string Text
);

public record EmployeeTypeConstant(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("desc")] string Desc,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("show")] bool Show,
    [property: JsonPropertyName("SGKApplicable")] bool SgkApplicable,
    [property: JsonPropertyName("SGKMinWageBasedExemption")] bool SgkMinWageBasedExemption,
    [property: JsonPropertyName("SGKMinWageBased17103Exemption")] bool SgkMinWageBased17103Exemption,
    [property: JsonPropertyName("unemploymentInsuranceApplicable")] bool UnemploymentInsuranceApplicable,
    [property: JsonPropertyName("AGIApplicable")] bool AgiApplicable,
    [property: JsonPropertyName("incomeTaxApplicable")] bool IncomeTaxApplicable,
    [property: JsonPropertyName("taxMinWageBasedExemption")] bool TaxMinWageBasedExemption,
    [property: JsonPropertyName("researchAndDevelopmentTaxExemption")] bool ResearchAndDevelopmentTaxExemption,
    [property: JsonPropertyName("stampTaxApplicable")] bool StampTaxApplicable,
    [property: JsonPropertyName("employerSGKApplicable")] bool EmployerSgkApplicable,
    [property: JsonPropertyName("employerUnemploymentInsuranceApplicable")] bool EmployerUnemploymentInsuranceApplicable,
    [property: JsonPropertyName("employerIncomeTaxApplicable")] bool EmployerIncomeTaxApplicable,
    [property: JsonPropertyName("employerStampTaxApplicable")] bool EmployerStampTaxApplicable,
    [property: JsonPropertyName("employerEducationIncomeTaxExemption")] bool EmployerEducationIncomeTaxExemption,
    [property: JsonPropertyName("employerSGKShareTotalExemption")] bool EmployerSgkShareTotalExemption,
    [property: JsonPropertyName("employerSGKDiscount5746Applicable")] bool EmployerSgkDiscount5746Applicable,
    [property: JsonPropertyName("employer5746AdditionalDiscountApplicable")] bool Employer5746AdditionalDiscountApplicable
);

public record AgiConstant(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("children")] int Children,
    [property: JsonPropertyName("equality")] string Equality,
    [property: JsonPropertyName("rate")] double Rate
);
