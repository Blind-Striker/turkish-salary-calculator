using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculatorApi.Contracts;

// ═══════════════════════════════════════════════════════════════════════════════
// API Request DTOs - Minimal types that map to library requests
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Monthly input for salary calculation.
/// </summary>
/// <param name="Month">Month number (1-12).</param>
/// <param name="Salary">Salary amount for this month.</param>
/// <param name="WorkedDays">Days worked (0-30, default 30).</param>
/// <param name="RnDDays">R&amp;D days worked (default 0).</param>
public sealed record MonthlyInputDto(int Month, decimal Salary, int WorkedDays = 30, int RnDDays = 0);

/// <summary>
/// AGI (Minimum Living Allowance) settings for pre-2022 calculations.
/// </summary>
/// <param name="SpouseStatus">Spouse status: "unmarried", "spouse-working", "spouse-not-working".</param>
/// <param name="NumberOfChildren">Number of children (affects AGI rate).</param>
/// <param name="IncludeInTax">If true, AGI shown separately; if false, pre-applied to net.</param>
public sealed record AgiSettingsDto(string SpouseStatus = "unmarried", int NumberOfChildren = 0, bool IncludeInTax = true);

/// <summary>
/// R&amp;D settings for Law 5746 employee types.
/// </summary>
/// <param name="EducationTypeId">Education type ID (1=Other, 2=Doctorate, 3=Masters).</param>
public sealed record RnDSettingsDto(int EducationTypeId = 1);

/// <summary>
/// Unified salary calculation request.
/// </summary>
/// <param name="Mode">Calculation mode: "gross-to-net", "net-to-gross", "total-to-gross".</param>
/// <param name="Year">Calculation year (required).</param>
/// <param name="Months">Monthly inputs (12 months required).</param>
/// <param name="EmployeeTypeId">Employee type ID (default 1/Standard).</param>
/// <param name="DisabilityDegreeId">Disability degree ID (default -1/None).</param>
/// <param name="IsPensioner">Whether employee is a pensioner.</param>
/// <param name="ApplyMinWageExemption">Apply min wage tax exemption (2022+).</param>
/// <param name="Apply5746Discount">Apply 5746 SGK discount.</param>
/// <param name="Agi">AGI settings (for pre-2022).</param>
/// <param name="RnD">R&amp;D settings (for 5746 types).</param>
/// <param name="IsAgiIncludedInNet">For NET_TO_GROSS: whether desired net includes AGI.</param>
public sealed record CalculationRequestDto(
    string Mode,
    int Year,
    IReadOnlyList<MonthlyInputDto> Months,
    int? EmployeeTypeId = null,
    int? DisabilityDegreeId = null,
    bool IsPensioner = false,
    bool ApplyMinWageExemption = true,
    bool Apply5746Discount = false,
    AgiSettingsDto? Agi = null,
    RnDSettingsDto? RnD = null,
    bool IsAgiIncludedInNet = false);

/// <summary>
/// Shorthand request for uniform salary (same amount all 12 months).
/// </summary>
/// <param name="Mode">Calculation mode: "gross-to-net", "net-to-gross", "total-to-gross".</param>
/// <param name="Year">Calculation year.</param>
/// <param name="Salary">Monthly salary amount (applied to all 12 months).</param>
/// <param name="WorkedDays">Days worked per month (default 30).</param>
/// <param name="RnDDays">R&amp;D days per month (default 0).</param>
/// <param name="EmployeeTypeId">Employee type ID (default 1/Standard).</param>
/// <param name="DisabilityDegreeId">Disability degree ID (default -1/None).</param>
/// <param name="IsPensioner">Whether employee is a pensioner.</param>
/// <param name="ApplyMinWageExemption">Apply min wage tax exemption (2022+).</param>
/// <param name="Apply5746Discount">Apply 5746 SGK discount.</param>
/// <param name="Agi">AGI settings (for pre-2022).</param>
/// <param name="RnD">R&amp;D settings (for 5746 types).</param>
/// <param name="IsAgiIncludedInNet">For NET_TO_GROSS: whether desired net includes AGI.</param>
public sealed record UniformCalculationRequestDto(
    string Mode,
    int Year,
    decimal Salary,
    int WorkedDays = 30,
    int RnDDays = 0,
    int? EmployeeTypeId = null,
    int? DisabilityDegreeId = null,
    bool IsPensioner = false,
    bool ApplyMinWageExemption = true,
    bool Apply5746Discount = false,
    AgiSettingsDto? Agi = null,
    RnDSettingsDto? RnD = null,
    bool IsAgiIncludedInNet = false);

// ═══════════════════════════════════════════════════════════════════════════════
// Mapping Extensions - Convert API DTOs to library types
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Extension methods for converting API DTOs to library types.
/// </summary>
public static class CalculationRequestMapper
{
    /// <summary>
    /// Maps the API DTO to a library SalaryCalculationRequest.
    /// </summary>
    public static Result<SalaryCalculationRequest> ToLibraryRequest(this CalculationRequestDto dto)
    {
        var mode = ParseMode(dto.Mode);
        if (mode is null)
        {
            return Result<SalaryCalculationRequest>.Failure(
                ErrorCode.InvalidCalculationMode,
                $"Invalid calculation mode: '{dto.Mode}'. Use 'gross-to-net', 'net-to-gross', or 'total-to-gross'.");
        }

        if (dto.Months is null || dto.Months.Count == 0)
        {
            return Result<SalaryCalculationRequest>.Failure(
                ErrorCode.InvalidMonthCount,
                "Monthly input data is required. Provide 12 monthly entries.");
        }

        // Convert months (1-indexed to 0-indexed for AllMonths array)
        var months = dto.Months.Select(m => new MonthlyInput(
            MonthsOfYear.AllMonths[m.Month - 1],
            m.Salary,
            m.WorkedDays,
            m.RnDDays)).ToArray();

        return Result<SalaryCalculationRequest>.Success(new SalaryCalculationRequest(
            Mode: mode.Value,
            Year: dto.Year,
            Months: months,
            EmployeeType: dto.EmployeeTypeId.HasValue ? EmployeeTypeId.FromId(dto.EmployeeTypeId.Value) : default,
            Disability: dto.DisabilityDegreeId.HasValue ? DisabilityDegreeId.FromDegree(dto.DisabilityDegreeId.Value) : default,
            IsPensioner: dto.IsPensioner,
            ApplyMinWageExemption: dto.ApplyMinWageExemption,
            Apply5746Discount: dto.Apply5746Discount,
            Agi: dto.Agi?.ToLibrarySettings(),
            RnD: dto.RnD?.ToLibrarySettings(),
            IsAgiIncludedInNet: dto.IsAgiIncludedInNet));
    }

    /// <summary>
    /// Maps the uniform API DTO to a library SalaryCalculationRequest.
    /// </summary>
    public static Result<SalaryCalculationRequest> ToLibraryRequest(this UniformCalculationRequestDto dto)
    {
        var mode = ParseMode(dto.Mode);
        if (mode is null)
        {
            return Result<SalaryCalculationRequest>.Failure(
                ErrorCode.InvalidCalculationMode,
                $"Invalid calculation mode: '{dto.Mode}'. Use 'gross-to-net', 'net-to-gross', or 'total-to-gross'.");
        }

        // Create uniform months
        var months = MonthlyInput.Uniform(dto.Salary, dto.WorkedDays, dto.RnDDays);

        return Result<SalaryCalculationRequest>.Success(new SalaryCalculationRequest(
            Mode: mode.Value,
            Year: dto.Year,
            Months: months,
            EmployeeType: dto.EmployeeTypeId.HasValue ? EmployeeTypeId.FromId(dto.EmployeeTypeId.Value) : default,
            Disability: dto.DisabilityDegreeId.HasValue ? DisabilityDegreeId.FromDegree(dto.DisabilityDegreeId.Value) : default,
            IsPensioner: dto.IsPensioner,
            ApplyMinWageExemption: dto.ApplyMinWageExemption,
            Apply5746Discount: dto.Apply5746Discount,
            Agi: dto.Agi?.ToLibrarySettings(),
            RnD: dto.RnD?.ToLibrarySettings(),
            IsAgiIncludedInNet: dto.IsAgiIncludedInNet));
    }

    /// <summary>
    /// Maps AGI settings DTO to library type.
    /// </summary>
    public static AgiSettings ToLibrarySettings(this AgiSettingsDto dto) => new(ParseSpouseStatus(dto.SpouseStatus), dto.NumberOfChildren, dto.IncludeInTax);

    /// <summary>
    /// Maps R&amp;D settings DTO to library type.
    /// </summary>
    public static RnDSettings ToLibrarySettings(this RnDSettingsDto dto) => new(EducationTypeId.FromId(dto.EducationTypeId));

    private static CalculationMode? ParseMode(string? mode) => mode?.ToLowerInvariant() switch
    {
        "gross-to-net" or "grosstonet" => CalculationMode.GrossToNet,
        "net-to-gross" or "nettogross" => CalculationMode.NetToGross,
        "total-to-gross" or "totaltogross" => CalculationMode.TotalToGross,
        _ => null,
    };

    private static SpouseStatus ParseSpouseStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "spouse-working" or "spouseworking" => SpouseStatus.SpouseWorking,
        "spouse-not-working" or "spousenotworking" => SpouseStatus.SpouseNotWorking,
        _ => SpouseStatus.Unmarried,
    };
}
