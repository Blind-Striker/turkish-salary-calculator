using Turkish.HRSolutions.SalaryCalculator.Application.Responses;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Mappings;

internal static class TaxSliceExtensions
{
#pragma warning disable CA1034 // False positive https://github.com/dotnet/sdk/issues/51681
    /// <summary>
    /// Extension methods for mapping <see cref="TaxSlice"/> to various representations.
    /// </summary>
    extension(TaxSlice taxSlice)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Maps a <see cref="TaxSlice"/> to a <see cref="TaxSliceSnapshot"/>.
        /// </summary>
        public TaxSliceSnapshot ToSnapshot()
        {
            return new TaxSliceSnapshot
            {
                Rate = taxSlice.Rate,
                Ceil = taxSlice.Ceil,
            };
        }
    }
}
