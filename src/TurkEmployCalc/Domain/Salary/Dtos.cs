#pragma warning disable MA0048 // File name must match type name (Dtos.cs) - disabled because of the record types

namespace TurkHRSolutions.TurkEmployCalc.Domain.Salary;

public record TaxSliceDto(double Rate, double? Ceil);

public record TaxResultDto(IEnumerable<TaxSliceDto> AppliedTaxSlices, double TaxAmount);
