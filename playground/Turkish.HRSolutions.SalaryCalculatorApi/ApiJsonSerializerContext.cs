using System.Text.Json.Serialization;
using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculatorApi.Contracts;

namespace Turkish.HRSolutions.SalaryCalculatorApi;

/// <summary>
/// Source-generated JSON serialization context for AOT-safe API serialization.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
// ═══════════════════════════════════════════════════════════════════════════════
// Library Types - Used directly in API responses
// ═══════════════════════════════════════════════════════════════════════════════
// Responses
[JsonSerializable(typeof(YearlySalarySnapshot))]
[JsonSerializable(typeof(MonthlyBreakdown))]
[JsonSerializable(typeof(TaxSliceSnapshot))]
[JsonSerializable(typeof(IReadOnlyList<MonthlyBreakdown>))]
[JsonSerializable(typeof(IReadOnlyList<TaxSliceSnapshot>))]
// Metadata info types
[JsonSerializable(typeof(EmployeeTypeInfo))]
[JsonSerializable(typeof(EducationTypeInfo))]
[JsonSerializable(typeof(DisabilityDegreeInfo))]
[JsonSerializable(typeof(IReadOnlyList<EmployeeTypeInfo>))]
[JsonSerializable(typeof(IReadOnlyList<EducationTypeInfo>))]
[JsonSerializable(typeof(IReadOnlyList<DisabilityDegreeInfo>))]
// Identifiers (for JSON conversion)
[JsonSerializable(typeof(EmployeeTypeId))]
[JsonSerializable(typeof(DisabilityDegreeId))]
[JsonSerializable(typeof(EducationTypeId))]
// Enums
[JsonSerializable(typeof(MonthsOfYear))]
// ═══════════════════════════════════════════════════════════════════════════════
// API Contract Types
// ═══════════════════════════════════════════════════════════════════════════════
// Metadata responses
[JsonSerializable(typeof(AvailableYearsResponse))]
[JsonSerializable(typeof(EmployeeTypesResponse))]
[JsonSerializable(typeof(EducationTypesResponse))]
[JsonSerializable(typeof(DisabilityDegreesResponse))]
[JsonSerializable(typeof(CapabilitiesResponse))]
// Calculation requests
[JsonSerializable(typeof(CalculationRequestDto))]
[JsonSerializable(typeof(UniformCalculationRequestDto))]
[JsonSerializable(typeof(MonthlyInputDto))]
[JsonSerializable(typeof(AgiSettingsDto))]
[JsonSerializable(typeof(RnDSettingsDto))]
[JsonSerializable(typeof(IReadOnlyList<MonthlyInputDto>))]
// Calculation responses
[JsonSerializable(typeof(CalculationResponse))]
[JsonSerializable(typeof(ApiWarning))]
[JsonSerializable(typeof(IReadOnlyList<ApiWarning>))]
// Error responses
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ErrorDetail))]
[JsonSerializable(typeof(IReadOnlyList<ErrorDetail>))]
// Primitives
[JsonSerializable(typeof(IReadOnlyList<int>))]
[JsonSerializable(typeof(IReadOnlyList<string>))]
internal sealed partial class ApiJsonSerializerContext : JsonSerializerContext;
