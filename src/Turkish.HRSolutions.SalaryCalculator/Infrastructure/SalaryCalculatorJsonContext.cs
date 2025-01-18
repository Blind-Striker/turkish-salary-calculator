#pragma warning disable S3963, CA1810

using System.Text.Json;
using System.Text.Json.Serialization;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(YearParameters))]
[JsonSerializable(typeof(YearParameter))]
[JsonSerializable(typeof(MinGrossWage))]
[JsonSerializable(typeof(TaxSlice))]
[JsonSerializable(typeof(DisabledMonthlyIncomeTaxDiscountBase))]
[JsonSerializable(typeof(ConstantParameters))]
[JsonSerializable(typeof(CalculationConstant))]
[JsonSerializable(typeof(EmployeeConstant))]
[JsonSerializable(typeof(EmployerConstant))]
[JsonSerializable(typeof(EmployeeEducationTypeConstant))]
[JsonSerializable(typeof(DisabilityConstant))]
[JsonSerializable(typeof(EmployeeTypeConstant))]
[JsonSerializable(typeof(AgiConstant))]
internal sealed partial class SalaryCalculatorJsonContext : JsonSerializerContext
{
    public static readonly JsonSerializerOptions JsonOptions;

    static SalaryCalculatorJsonContext()
    {
        JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = Default,
        };
    }
}
