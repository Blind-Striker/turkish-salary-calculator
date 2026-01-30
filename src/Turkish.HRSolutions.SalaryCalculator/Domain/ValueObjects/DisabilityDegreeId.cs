namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

/// <summary>
/// Identifies a disability degree for tax deduction calculation.
/// </summary>
/// <remarks>
/// <para>
/// Turkish tax law provides income tax base deductions for employees with disabilities.
/// The deduction amount varies by degree (1st degree = highest deduction).
/// </para>
/// <para>
/// Use <see cref="FromDegree"/> for runtime values or custom scenarios.
/// </para>
/// </remarks>
public readonly record struct DisabilityDegreeId : IEquatable<DisabilityDegreeId>
{
    // ═══════════════════════════════════════════════════════════════
    // Known Values
    // ═══════════════════════════════════════════════════════════════

    /// <summary>No disability - no tax deduction.</summary>
    public static DisabilityDegreeId None => new(-1);

    /// <summary>1st degree disability - highest deduction.</summary>
    public static DisabilityDegreeId First => new(1);

    /// <summary>2nd degree disability - medium deduction.</summary>
    public static DisabilityDegreeId Second => new(2);

    /// <summary>3rd degree disability - lowest deduction.</summary>
    public static DisabilityDegreeId Third => new(3);

    // ═══════════════════════════════════════════════════════════════
    // Extensibility
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a disability degree ID from a numeric value.
    /// </summary>
    /// <param name="degree">The numeric degree (1, 2, or 3), or -1 for none.</param>
    /// <returns>A <see cref="DisabilityDegreeId"/> with the specified value.</returns>
    public static DisabilityDegreeId FromDegree(int degree) => new(degree);

    /// <summary>
    /// Gets all known disability degree options.
    /// </summary>
    public static IReadOnlyList<DisabilityDegreeId> KnownDegrees =>
    [
        None,
        First,
        Second,
        Third,
    ];

    // ═══════════════════════════════════════════════════════════════
    // Implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// The numeric value of this disability degree.
    /// -1 = None, 1-3 = disability degrees.
    /// </summary>
    public int Value { get; }

    private DisabilityDegreeId(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns true if this represents a disability (not None).
    /// </summary>
    public bool HasDisability => Value > 0;

    /// <summary>
    /// Implicit conversion to int for interop with int-based APIs.
    /// </summary>
    public static implicit operator int(DisabilityDegreeId id) => id.Value;

    /// <inheritdoc />
    public override string ToString() => Value switch
    {
        -1 => nameof(None),
        1 => nameof(First),
        2 => nameof(Second),
        3 => nameof(Third),
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
