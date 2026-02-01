namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

/// <summary>
/// Represents the spouse employment status for AGI (minimum living allowance) calculation.
/// </summary>
/// <remarks>
/// <para>
/// AGI was replaced by the minimum wage income tax exemption starting from 2022.
/// This type is only relevant for calculations before 2022.
/// </para>
/// <para>
/// The <see cref="AgiTypeKey"/> property maps to the "type" field in AGI constants JSON
/// (e.g., "Unmarried", "Working", "Unemployed").
/// </para>
/// </remarks>
public readonly struct SpouseStatus : IEquatable<SpouseStatus>
{
    // ═══════════════════════════════════════════════════════════════
    // AGI Type Keys (match JSON constants)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>AGI type key for unmarried employees.</summary>
    public const string UnmarriedKey = "Unmarried";

    /// <summary>AGI type key for employees with a working spouse.</summary>
    public const string WorkingKey = "Working";

    /// <summary>AGI type key for employees with a non-working spouse.</summary>
    public const string UnemployedKey = "Unemployed";

    // ═══════════════════════════════════════════════════════════════
    // Known Values
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Employee is unmarried
    /// </summary>
    public static readonly SpouseStatus Unmarried = new(UnmarriedKey, "Bekar");

    /// <summary>
    /// Employee is married, spouse is employed
    /// </summary>
    public static readonly SpouseStatus SpouseWorking = new(WorkingKey, "Eş Çalışan");

    /// <summary>
    /// Employee is married, spouse is not employed
    /// </summary>
    public static readonly SpouseStatus SpouseNotWorking = new(UnemployedKey, "Eş Çalışmayan");

    /// <summary>
    /// Gets all known spouse status values.
    /// </summary>
    public static IReadOnlyList<SpouseStatus> AllStatuses { get; } =
    [
        Unmarried,
        SpouseWorking,
        SpouseNotWorking,
    ];

    // ═══════════════════════════════════════════════════════════════
    // Implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the AGI type key used for JSON constant lookup.
    /// Maps to the "type" field in AGI_OPTIONS (e.g., "Unmarried", "Working", "Unemployed").
    /// </summary>
    public string AgiTypeKey { get; }

    /// <summary>
    /// Gets the Turkish display name for UI purposes.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets a value indicating whether this is a default/uninitialized instance.
    /// A default SpouseStatus is treated as Unmarried.
    /// </summary>
    public bool IsDefault => AgiTypeKey is null;

    private SpouseStatus(string agiTypeKey, string displayName)
    {
        AgiTypeKey = agiTypeKey;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the effective AGI type key, returning <see cref="UnmarriedKey"/> for default instances.
    /// </summary>
    public string EffectiveAgiTypeKey => AgiTypeKey ?? UnmarriedKey;

    /// <inheritdoc />
    public override string ToString() => AgiTypeKey ?? UnmarriedKey;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SpouseStatus other && Equals(other);

    /// <inheritdoc />
    public bool Equals(SpouseStatus other) =>
        string.Equals(EffectiveAgiTypeKey, other.EffectiveAgiTypeKey, StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode() => EffectiveAgiTypeKey.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(SpouseStatus left, SpouseStatus right) => left.Equals(right);

    public static bool operator !=(SpouseStatus left, SpouseStatus right) => !left.Equals(right);
}
