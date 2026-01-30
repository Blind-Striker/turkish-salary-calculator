namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

/// <summary>
/// Identifies an employee type for salary calculation.
/// </summary>
/// <remarks>
/// <para>
/// This is a "dumb" identifier with no business logic. Use static members for known types.
/// Use <see cref="FromId"/> for custom types defined in configuration overrides.
/// </para>
/// <para>
/// <b>Important:</b> To check employee type capabilities (IsRnDType, SupportsEducationExemption, etc.),
/// query the <c>ICalculationConstantsProvider</c> instead of relying on hardcoded logic.
/// This ensures custom types work correctly.
/// </para>
/// </remarks>
public readonly record struct EmployeeTypeId : IEquatable<EmployeeTypeId>
{
    // ═══════════════════════════════════════════════════════════════
    // Known Values (Compile-Time Friendly)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Standard employee - all taxes apply, basic 5% SGK discount available.</summary>
    public static EmployeeTypeId Standard => new(1);

    /// <summary>Teknokent (Law 4691) - R&amp;D tax exemption, additional 50% SGK discount.</summary>
    public static EmployeeTypeId Teknokent4691 => new(2);

    /// <summary>R&amp;D Personnel (Law 5746) - R&amp;D + education-based exemption, full 50% additional discount.</summary>
    public static EmployeeTypeId RnD5746 => new(3);

    /// <summary>Law 6111 - Full employer SGK exemption.</summary>
    public static EmployeeTypeId Law6111 => new(4);

    /// <summary>Employer/Owner - No SGK/unemployment (company owner salary).</summary>
    public static EmployeeTypeId Employer => new(5);

    /// <summary>Personnel 27103 - Min-wage based SGK &amp; tax exemption.</summary>
    public static EmployeeTypeId Personnel27103 => new(6);

    /// <summary>Personnel 17103 - Alternative min-wage formula for exemptions.</summary>
    public static EmployeeTypeId Personnel17103 => new(7);

    /// <summary>Employer in Teknokent (Law 4691).</summary>
    public static EmployeeTypeId EmployerTeknokent4691 => new(8);

    /// <summary>Employer in R&amp;D (Law 5746).</summary>
    public static EmployeeTypeId EmployerRnD5746 => new(9);

    /// <summary>Domestic services - No income tax, no stamp tax.</summary>
    public static EmployeeTypeId DomesticServices => new(10);

    // ═══════════════════════════════════════════════════════════════
    // Extensibility (Runtime Flexible)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an employee type ID from a numeric value.
    /// Use for custom types defined in configuration overrides.
    /// </summary>
    /// <param name="id">The numeric identifier for the employee type.</param>
    /// <returns>An <see cref="EmployeeTypeId"/> with the specified value.</returns>
    public static EmployeeTypeId FromId(int id) => new(id);

    /// <summary>
    /// Gets all known (built-in) employee type IDs.
    /// </summary>
    public static IReadOnlyList<EmployeeTypeId> KnownTypes =>
    [
        Standard,
        Teknokent4691,
        RnD5746,
        Law6111,
        Employer,
        Personnel27103,
        Personnel17103,
        EmployerTeknokent4691,
        EmployerRnD5746,
        DomesticServices,
    ];

    // ═══════════════════════════════════════════════════════════════
    // Implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// The numeric value of this employee type ID.
    /// </summary>
    public int Value { get; }

    private EmployeeTypeId(int value) => Value = value;

    /// <summary>
    /// Implicit conversion to int for interop with int-based APIs.
    /// </summary>
    public static implicit operator int(EmployeeTypeId id) => id.Value;

    /// <inheritdoc />
    public override string ToString() => Value switch
    {
        1 => nameof(Standard),
        2 => nameof(Teknokent4691),
        3 => nameof(RnD5746),
        4 => nameof(Law6111),
        5 => nameof(Employer),
        6 => nameof(Personnel27103),
        7 => nameof(Personnel17103),
        8 => nameof(EmployerTeknokent4691),
        9 => nameof(EmployerRnD5746),
        10 => nameof(DomesticServices),
        _ => $"Custom({Value})",
    };

    /// <summary>
    /// Converts this ID to an Int32. Alternate for implicit operator.
    /// To satisfy CA2225: Provide a method named 'ToInt32' or 'FromDisabilityDegreeId' as an alternate for operator op_Implicit
    /// </summary>
    // ReSharper disable once UnusedMember.Local
#pragma warning disable CA2225,IDE0051, S1144, RCS1213
    private int ToInt32() => Value;
#pragma warning restore CA2225, IDE0051, S1144, RCS1213
}
