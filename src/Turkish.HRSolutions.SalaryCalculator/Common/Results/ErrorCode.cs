namespace Turkish.HRSolutions.SalaryCalculator.Common.Results;

/// <summary>
/// Unified error codes for all validation, calculation, and configuration errors.
/// Codes are grouped by category using number ranges for organization.
/// </summary>
/// <remarks>
/// <para>Range mapping to HTTP status codes (used by API layer):</para>
/// <list type="bullet">
/// <item><description>1000-3999 (Input/Config validation) → 400 Bad Request</description></item>
/// <item><description>4000-4999 (Calculation runtime) → 422 Unprocessable Entity</description></item>
/// <item><description>5000+ (Provider/Infrastructure) → 500 Internal Server Error</description></item>
/// </list>
/// </remarks>
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

    /// <summary>Monthly inputs must cover exactly 12 months.</summary>
    InvalidMonthCount = 1007,

    /// <summary>R&amp;D days cannot be negative.</summary>
    InvalidRnDDays = 1009,

    /// <summary>Employee type identifier is not valid or not recognized.</summary>
    InvalidEmployeeType = 1010,

    /// <summary>Calculation mode string is not a recognized mode.</summary>
    InvalidCalculationMode = 1011,

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
    // Configuration Warnings (3000-3999)
    // Used by ValidationEngine for capability violation warnings.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Education exemption only applies to R&amp;D 5746 employee types.</summary>
    EducationExemptionNotApplicable = 3002,

    /// <summary>AGI is not applicable for this configuration.</summary>
    AgiNotApplicable = 3006,

    /// <summary>5746 SGK discount is not applicable for this configuration.</summary>
    Discount5746NotApplicable = 3007,

    /// <summary>AGI included in net is only applicable in NET_TO_GROSS mode.</summary>
    AgiIncludedInNetNotApplicable = 3008,

    // ═══════════════════════════════════════════════════════════════
    // Calculation Runtime (4000-4999)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Calculation produced an unexpected result.</summary>
    CalculationFailed = 4002,

    // ═══════════════════════════════════════════════════════════════
    // Provider/Configuration (5000-5999)
    // Bootstrap fail-fast errors — these indicate misconfigured providers.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Year parameter provider is null or invalid.</summary>
    InvalidYearParameterProvider = 5000,

    /// <summary>The required employee type definition is missing from the provider at bootstrap time.</summary>
    MissingEmployeeTypeDefinition = 5002,

    // ═══════════════════════════════════════════════════════════════
    // Infrastructure (6000-6999)
    // ═══════════════════════════════════════════════════════════════

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
