namespace Turkish.HRSolutions.SalaryCalculator.Application.Responses;

/// <summary>
/// Immutable snapshot of a tax slice applied during income tax calculation.
/// </summary>
/// <remarks>
/// Mirrors <see cref="Domain.ValueObjects.TaxSlice"/> as a pure data record.
/// </remarks>
public sealed record TaxSliceSnapshot
{
    /// <summary>
    /// The tax rate for this slice (e.g., 0.15 for 15%).
    /// </summary>
    public required double Rate { get; init; }

    /// <summary>
    /// The ceiling amount for this slice (null for the highest bracket).
    /// </summary>
    public required decimal? Ceil { get; init; }
}
