namespace Turkish.HRSolutions.SalaryCalculator.Tests.Parity;

/// <summary>
/// Compares decimal values with configurable tolerance.
/// Uses relative tolerance for larger values and absolute tolerance for small ones.
/// </summary>
public sealed class FieldComparer
{
    private readonly decimal _absoluteTolerance;
    private readonly decimal _relativeTolerance;

    /// <summary>
    /// Creates a new field comparer with specified tolerances.
    /// </summary>
    /// <param name="absoluteTolerance">Absolute tolerance for values close to zero (default: 0.01 = 1 kuruş)</param>
    /// <param name="relativeTolerance">Relative tolerance for larger values (default: 0.0001 = 0.01%)</param>
    public FieldComparer(decimal absoluteTolerance = 0.01m, decimal relativeTolerance = 0.0001m)
    {
        _absoluteTolerance = absoluteTolerance;
        _relativeTolerance = relativeTolerance;
    }

    /// <summary>
    /// Compares two values and returns whether they are equal within tolerance.
    /// </summary>
    public bool AreEqual(decimal expected, decimal actual)
    {
        var diff = Math.Abs(expected - actual);

        // For values close to zero, use absolute tolerance
        if (Math.Abs(expected) < 1m)
        {
            return diff <= _absoluteTolerance;
        }

        // For larger values, use relative tolerance
        return diff / Math.Abs(expected) <= _relativeTolerance;
    }

    /// <summary>
    /// Compares a field and returns a detailed result.
    /// </summary>
    public FieldComparisonResult Compare(string fieldName, decimal expected, decimal actual)
    {
        var difference = actual - expected;
        var passed = AreEqual(expected, actual);

        return new FieldComparisonResult(
            FieldName: fieldName,
            Expected: expected,
            Actual: actual,
            Difference: difference,
            Passed: passed);
    }
}
