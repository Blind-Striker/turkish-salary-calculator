namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

/// <summary>
/// Unified error codes for all validation, calculation, and configuration errors.
/// Codes are grouped by category using a number of ranges for the organization.
/// </summary>
public enum ErrorCode
{
    // ═══════════════════════════════════════════════════════════════
    // Input Validation (1000-1999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Year is required but was not specified.</summary>
    YearNotSpecified = 1000,

    /// <summary>Year parameter not found for the specified year.</summary>
    YearNotSupported = 1001,

    /// <summary>Salary amount must be greater than zero.</summary>
    InvalidSalaryAmount = 1002,

    /// <summary>Worked days must be between 0 and 30.</summary>
    InvalidWorkedDays = 1003,

    /// <summary>R&amp;D days cannot exceed worked days.</summary>
    RnDDaysExceedWorkedDays = 1004,

    /// <summary>R&amp;D days specified for non-R&amp;D employee type.</summary>
    RnDDaysNotApplicable = 1005,

    /// <summary>No monthly inputs provided.</summary>
    NoMonthlyInputs = 1006,

    /// <summary>Monthly inputs must cover exactly 12 months.</summary>
    InvalidMonthCount = 1007,

    /// <summary>Duplicate month entries found.</summary>
    DuplicateMonthEntries = 1008,

    /// <summary>R&amp;D days cannot be negative.</summary>
    InvalidRnDDays = 1009,

    // ═══════════════════════════════════════════════════════════════
    // Input Structure - FillForward/FillBackward (2000-2999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>FillForward: First entry must be January.</summary>
    FillForwardMustStartWithJanuary = 2000,

    /// <summary>FillForward: Entries must be in chronological order.</summary>
    FillForwardNonSequentialMonths = 2001,

    /// <summary>FillBackward: Last entry must be December.</summary>
    FillBackwardMustEndWithDecember = 2002,

    /// <summary>FillBackward: Entries must be in chronological order.</summary>
    FillBackwardNonSequentialMonths = 2003,

    /// <summary>FillForward/FillBackward requires at least one entry.</summary>
    FillNoEntries = 2004,

    // ═══════════════════════════════════════════════════════════════
    // Configuration Validation (3000-3999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>AGI calculation is not available for years 2022 and later.</summary>
    AgiNotApplicableForYear = 3000,

    /// <summary>AGI is not applicable for this employee type.</summary>
    AgiNotApplicableForEmployeeType = 3001,

    /// <summary>Education exemption only applies to R&amp;D 5746 employee types.</summary>
    EducationExemptionNotApplicable = 3002,

    /// <summary>5746 SGK discount cannot be applied for pensioners.</summary>
    Discount5746NotApplicableForPensioner = 3003,

    /// <summary>5746 SGK discount is not applicable for this employee type.</summary>
    Discount5746NotApplicableForEmployeeType = 3004,

    /// <summary>Min wage tax exemption is only available for years 2022 and later.</summary>
    MinWageExemptionNotApplicableForYear = 3005,

    /// <summary>AGI is not applicable for this configuration.</summary>
    AgiNotApplicable = 3006,

    /// <summary>5746 SGK discount is not applicable for this configuration.</summary>
    Discount5746NotApplicable = 3007,

    /// <summary>AGI included in net is only applicable in NET_TO_GROSS mode.</summary>
    AgiIncludedInNetNotApplicable = 3008,

    // ═══════════════════════════════════════════════════════════════
    // Calculation Runtime (4000-4999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Binary search failed to converge (Net-to-Gross or Total-to-Gross).</summary>
    BinarySearchFailed = 4000,

    /// <summary>Calculated salary is below minimum wage.</summary>
    SalaryBelowMinimumWage = 4001,

    /// <summary>Calculation produced an unexpected result.</summary>
    CalculationFailed = 4002,

    // ═══════════════════════════════════════════════════════════════
    // Provider/Configuration (5000-5999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Year parameter provider is null or invalid.</summary>
    InvalidYearParameterProvider = 5000,

    /// <summary>Calculation constants provider is null or invalid.</summary>
    InvalidConstantsProvider = 5001,

    /// <summary>The required employee type definition is missing from the provider.</summary>
    MissingEmployeeTypeDefinition = 5002,

    /// <summary>Year parameter data is corrupted or invalid.</summary>
    InvalidYearParameterData = 5003,

    /// <summary>Configuration rate is outside valid range.</summary>
    ConfigurationRateOutOfRange = 5004,

    /// <summary>Year is listed but GetParameter returns null.</summary>
    YearParameterMismatch = 5005,

    /// <summary>Year has no minimum wage entries.</summary>
    MissingMinimumWageData = 5006,

    /// <summary>Year has no tax bracket entries.</summary>
    MissingTaxBracketData = 5007,

    // ═══════════════════════════════════════════════════════════════
    // Infrastructure (6000-6999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>File not found at the specified path.</summary>
    FileNotFound = 6000,

    /// <summary>JSON deserialization failed.</summary>
    JsonDeserializationFailed = 6001,

    /// <summary>Failed to read file contents.</summary>
    FileReadFailed = 6002,

    /// <summary>Embedded resource not found in assembly.</summary>
    EmbeddedResourceNotFound = 6003,

    /// <summary>Configuration file not found at the specified path.</summary>
    ConfigurationFileNotFound = 6004,

    /// <summary>Configuration file is empty.</summary>
    ConfigurationFileEmpty = 6005,

    /// <summary>An unexpected error occurred.</summary>
    UnexpectedError = 6999,
}
