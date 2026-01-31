using System.Runtime.InteropServices;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Requests;

/// <summary>
/// Represents a single month's input for salary calculation.
/// </summary>
/// <remarks>
/// <para>
/// Use the static factory methods <c>FillForward</c>, <c>FillBackward</c>,
/// or <see cref="Uniform"/> to create 12-month arrays from sparse or uniform input.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public readonly record struct MonthlyInput
{
    /// <summary>
    /// The calendar month (January through December).
    /// </summary>
    public MonthsOfYear Month { get; }

    /// <summary>
    /// The salary amount for this month (gross, net, or total depending on calculation mode).
    /// </summary>
    public decimal Salary { get; }

    /// <summary>
    /// Number of days worked this month (0-30). Default is 30.
    /// </summary>
    public int WorkedDays { get; }

    /// <summary>
    /// Number of R&amp;D days worked (for Teknokent/5746 employee types). Default is 0.
    /// </summary>
    public int RnDDays { get; }

    /// <summary>
    /// Creates a monthly input with the specified values.
    /// </summary>
    /// <param name="month">The calendar month.</param>
    /// <param name="salary">The salary amount.</param>
    /// <param name="workedDays">Days worked (0-30, default 30).</param>
    /// <param name="rndDays">R&amp;D days worked (default 0).</param>
    public MonthlyInput(MonthsOfYear month, decimal salary, int workedDays = 30, int rndDays = 0)
    {
        Month = month;
        Salary = salary;
        WorkedDays = workedDays;
        RnDDays = rndDays;
    }

    // ═══════════════════════════════════════════════════════════════
    // FillForward: Anchors at START (January), propagates FORWARD
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a 12-month array from sparse entries, filling gaps FORWARD.
    /// </summary>
    /// <remarks>
    /// <para><b>INVARIANTS (returns Result.Failure if violated):</b></para>
    /// <list type="bullet">
    ///   <item>First entry MUST be January</item>
    ///   <item>Entries must be in strictly increasing month order</item>
    ///   <item>Each entry's value fills forward until the next entry</item>
    ///   <item>The last entry's value fills through December</item>
    /// </list>
    /// <para>
    /// <b>Use for:</b> Salary history with raises (most common). Start with January's salary,
    /// add entries for each raise. For new hires, start with (January, 0m).
    /// </para>
    /// </remarks>
    /// <param name="entries">Sparse entries with month, salary, worked days, and R&amp;D days.</param>
    /// <returns>Result containing the 12-month array, or failure with error details.</returns>
    public static Result<IReadOnlyList<MonthlyInput>> FillForward(
        params (MonthsOfYear month, decimal salary, int workedDays, int rndDays)[] entries)
    {
        if (entries.Length == 0)
        {
            return Result<IReadOnlyList<MonthlyInput>>.Failure(
                ErrorCode.FillNoEntries,
                "FillForward requires at least one entry.");
        }

        // Validate first entry is January
        if (entries[0].month != MonthsOfYear.January)
        {
            return Result<IReadOnlyList<MonthlyInput>>.Failure(
                Error.Validation(
                    ErrorCode.FillForwardMustStartWithJanuary,
                    $"FillForward entries must start with January. First entry was {entries[0].month}.",
                    "entries[0].month",
                    entries[0].month));
        }

        // Validate entries are in chronological order
        for (var i = 1; i < entries.Length; i++)
        {
            if (entries[i].month <= entries[i - 1].month)
            {
                return Result<IReadOnlyList<MonthlyInput>>.Failure(
                    Error.Validation(
                        ErrorCode.FillForwardNonSequentialMonths,
                        $"FillForward entries must be in chronological order. Entry {i} ({entries[i].month}) is not after entry {i - 1} ({entries[i - 1].month}).",
                        $"entries[{i}].month",
                        entries[i].month));
            }
        }

        // Build the 12-month array
        var result = new MonthlyInput[12];
        var entryIndex = 0;
        var currentEntry = entries[0];
        var allMonths = MonthsOfYear.AllMonths;

        for (var monthNum = 0; monthNum < 12; monthNum++)
        {
            var month = allMonths[monthNum];

            // Check if we've reached the next entry
            if (entryIndex + 1 < entries.Length && entries[entryIndex + 1].month == month)
            {
                entryIndex++;
                currentEntry = entries[entryIndex];
            }

            result[monthNum] = new MonthlyInput(month, currentEntry.salary, currentEntry.workedDays, currentEntry.rndDays);
        }

        return Result<IReadOnlyList<MonthlyInput>>.Success(result);
    }

    /// <summary>
    /// Creates a 12-month array from sparse entries (salary only, 30 worked days, 0 R&amp;D days).
    /// First entry MUST be January. Returns Result.Failure for invalid structure.
    /// </summary>
    /// <param name="entries">Sparse entries with month and salary.</param>
    /// <returns>Result containing the 12-month array, or failure with error details.</returns>
    public static Result<IReadOnlyList<MonthlyInput>> FillForward(
        params (MonthsOfYear month, decimal salary)[] entries)
    {
        var fullEntries = entries
            .Select(e => (e.month, e.salary, workedDays: 30, rndDays: 0))
            .ToArray();

        return FillForward(fullEntries);
    }

    // ═══════════════════════════════════════════════════════════════
    // FillBackward: Anchors at END (December), propagates BACKWARD
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a 12-month array from sparse entries, filling gaps BACKWARD.
    /// </summary>
    /// <remarks>
    /// <para><b>INVARIANTS (returns Result.Failure if violated):</b></para>
    /// <list type="bullet">
    ///   <item>Last entry MUST be December</item>
    ///   <item>Entries must be in strictly increasing month order</item>
    ///   <item>Each entry's value fills backward until the previous entry</item>
    ///   <item>The first entry's value fills back to January</item>
    /// </list>
    /// <para>
    /// <b>Use for:</b> When you know the end state and want to fill backward.
    /// Less common than FillForward.
    /// </para>
    /// </remarks>
    /// <param name="entries">Sparse entries with month, salary, worked days, and R&amp;D days.</param>
    /// <returns>Result containing the 12-month array, or failure with error details.</returns>
    public static Result<IReadOnlyList<MonthlyInput>> FillBackward(
        params (MonthsOfYear month, decimal salary, int workedDays, int rndDays)[] entries)
    {
        if (entries.Length == 0)
        {
            return Result<IReadOnlyList<MonthlyInput>>.Failure(
                ErrorCode.FillNoEntries,
                "FillBackward requires at least one entry.");
        }

        // Validate last entry is December
        if (entries[^1].month != MonthsOfYear.December)
        {
            return Result<IReadOnlyList<MonthlyInput>>.Failure(
                Error.Validation(
                    ErrorCode.FillBackwardMustEndWithDecember,
                    $"FillBackward entries must end with December. Last entry was {entries[^1].month}.",
                    $"entries[{entries.Length - 1}].month",
                    entries[^1].month));
        }

        // Validate entries are in chronological order
        for (var i = 1; i < entries.Length; i++)
        {
            if (entries[i].month.Number <= entries[i - 1].month.Number)
            {
                return Result<IReadOnlyList<MonthlyInput>>.Failure(
                    Error.Validation(
                        ErrorCode.FillBackwardNonSequentialMonths,
                        $"FillBackward entries must be in chronological order. Entry {i} ({entries[i].month}) is not after entry {i - 1} ({entries[i - 1].month}).",
                        $"entries[{i}].month",
                        entries[i].month));
            }
        }

        // Build the 12-month array (working backwards)
        var result = new MonthlyInput[12];
        var entryIndex = entries.Length - 1;
        var currentEntry = entries[^1];
        var allMonths = MonthsOfYear.AllMonths;

        for (var monthNum = 11; monthNum >= 0; monthNum--)
        {
            var month = allMonths[monthNum];

            switch (entryIndex)
            {
                // Check if we've reached the previous entry
                case > 0 when entries[entryIndex - 1].month == month:
                    currentEntry = entries[entryIndex - 1];
                    entryIndex--;
                    break;
                case >= 0 when entries[entryIndex].month == month:
                    currentEntry = entries[entryIndex];
                    break;
            }

            result[monthNum] = new MonthlyInput(month, currentEntry.salary, currentEntry.workedDays, currentEntry.rndDays);
        }

        return Result<IReadOnlyList<MonthlyInput>>.Success(result);
    }

    /// <summary>
    /// Creates a 12-month array from sparse entries (salary only, 30 worked days, 0 R&amp;D days).
    /// The last entry MUST be December. Returns Result.Failure for invalid structure.
    /// </summary>
    /// <param name="entries">Sparse entries with month and salary.</param>
    /// <returns>Result containing the 12-month array, or failure with error details.</returns>
    public static Result<IReadOnlyList<MonthlyInput>> FillBackward(
        params (MonthsOfYear month, decimal salary)[] entries)
    {
        var fullEntries = entries
            .Select(e => (e.month, e.salary, workedDays: 30, rndDays: 0))
            .ToArray();

        return FillBackward(fullEntries);
    }

    // ═══════════════════════════════════════════════════════════════
    // Uniform: All 12 months same value
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a 12-month array with uniform values for all months.
    /// </summary>
    /// <param name="salary">The salary amount for all months.</param>
    /// <param name="workedDays">Days worked for all months (default 30).</param>
    /// <param name="rndDays">R&amp;D days for all months (default 0).</param>
    /// <returns>A 12-month array with identical values.</returns>
    public static IReadOnlyList<MonthlyInput> Uniform(decimal salary, int workedDays = 30, int rndDays = 0)
        => [.. MonthsOfYear.AllMonths.Select(month => new MonthlyInput(month, salary, workedDays, rndDays)),];
}
