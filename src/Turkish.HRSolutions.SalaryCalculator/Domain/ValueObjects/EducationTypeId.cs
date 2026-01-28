namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

/// <summary>
/// Identifies an education type for R&amp;D (Law 5746) exemption calculation.
/// </summary>
/// <remarks>
/// <para>
/// This only applies to employees with <see cref="EmployeeTypeId.RnD5746"/> or
/// <see cref="EmployeeTypeId.EmployerRnD5746"/> types. The education level affects
/// the employer's income tax exemption rate.
/// </para>
/// <para>
/// Use <see cref="FromId"/> for runtime values or custom scenarios.
/// </para>
/// </remarks>
public readonly record struct EducationTypeId : IEquatable<EducationTypeId>
{
    // ═══════════════════════════════════════════════════════════════
    // Known Values
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Other R&amp;D personnel - base exemption rate.</summary>
    public static EducationTypeId OtherRnDPersonnel => new(1);

    /// <summary>Doctorate degree - highest exemption rate.</summary>
    public static EducationTypeId Doctorate => new(2);

    /// <summary>Master's degree or bachelor's in fundamental sciences.</summary>
    public static EducationTypeId MastersOrFundamentalSciences => new(3);

    // ═══════════════════════════════════════════════════════════════
    // Extensibility
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an education type ID from a numeric value.
    /// </summary>
    /// <param name="id">The numeric identifier for the education type.</param>
    /// <returns>An <see cref="EducationTypeId"/> with the specified value.</returns>
    public static EducationTypeId FromId(int id) => new(id);

    /// <summary>
    /// Gets all known education type options.
    /// </summary>
    public static IReadOnlyList<EducationTypeId> KnownTypes =>
    [
        OtherRnDPersonnel,
        Doctorate,
        MastersOrFundamentalSciences,
    ];

    // ═══════════════════════════════════════════════════════════════
    // Implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// The numeric value of this education type ID.
    /// </summary>
    public int Value { get; }

    private EducationTypeId(int value) => Value = value;

    /// <summary>
    /// Converts this ID to an Int32. Alternate for implicit operator.
    /// </summary>
    public int ToInt32() => Value;

    /// <summary>
    /// Implicit conversion to int for interop with int-based APIs.
    /// </summary>
    public static implicit operator int(EducationTypeId id) => id.Value;

    /// <inheritdoc />
    public override string ToString() => Value switch
    {
        1 => nameof(OtherRnDPersonnel),
        2 => nameof(Doctorate),
        3 => nameof(MastersOrFundamentalSciences),
        _ => $"Custom({Value})",
    };
}
