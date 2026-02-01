using Turkish.HRSolutions.SalaryCalculator.Application.Builders;
using Turkish.HRSolutions.SalaryCalculator.Application.Builders.Internal;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Extensions;

/// <summary>
/// Extension properties that provide fluent builder access from <see cref="ISalaryCalculator"/>.
/// This is convenience sugar — power users can construct requests directly.
/// </summary>
/// <remarks>
/// <para>
/// Using C# 14 Extension Blocks, the fluent API becomes extension properties on
/// <see cref="ISalaryCalculator"/> itself. The same API works for both DI and standalone:
/// </para>
/// <code>
/// // DI: Inject ISalaryCalculator
/// public class PayrollService(ISalaryCalculator calculator)
/// {
///     public Result Calculate() =&gt; calculator.UseGrossToNet.ForYear(2026).Calculate(30_000m);
/// }
///
/// // Standalone: Create ISalaryCalculator
/// var calculator = SalaryCalculatorBuilder.Create();
/// var result = calculator.UseGrossToNet.ForYear(2026).Calculate(30_000m);
/// </code>
/// </remarks>
public static class SalaryCalculatorExtensions
{
#pragma warning disable CA1034 // False positive https://github.com/dotnet/sdk/issues/51681
    /// <summary>
    /// Provides fluent builder access for <see cref="ISalaryCalculator"/>.
    /// </summary>
    extension(ISalaryCalculator calculator)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Start building a Gross-to-Net calculation.
        /// </summary>
        public IGrossToNetBuilder UseGrossToNet => new GrossToNetBuilder(calculator);

        /// <summary>
        /// Start building a Net-to-Gross calculation (binary search).
        /// </summary>
        public INetToGrossBuilder UseNetToGross => new NetToGrossBuilder(calculator);

        /// <summary>
        /// Start building a Total-Cost-to-Gross calculation (binary search).
        /// </summary>
        public ITotalToGrossBuilder UseTotalToGross => new TotalToGrossBuilder(calculator);
    }
}
