#pragma warning disable MA0048

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

public record YearParameters([property: JsonPropertyName("yearParameters")] ImmutableList<YearParameter> Parameters);

public record YearParameter(
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("minGrossWages")] ImmutableList<MinGrossWage> MinGrossWages,
    [property: JsonPropertyName("minWageEmployeeTaxExemption")] bool MinWageEmployeeTaxExemption,
    [property: JsonPropertyName("taxSlices")] ImmutableList<TaxSlice> TaxSlices,
    [property: JsonPropertyName("disabledMonthlyIncomeTaxDiscountBases")] ImmutableList<DisabledMonthlyIncomeTaxDiscountBase> DisabledMonthlyIncomeTaxDiscountBases);

public record MinGrossWage(
    [property: JsonPropertyName("startMonth")] int StartMonth,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("SGKCeil")] decimal SgkCeil);

public record TaxSlice(
    [property: JsonPropertyName("rate")] double Rate,
    [property: JsonPropertyName("ceil")] decimal? Ceil);

public record DisabledMonthlyIncomeTaxDiscountBase(
    [property: JsonPropertyName("degree")] int Degree,
    [property: JsonPropertyName("amount")] decimal Amount);
