using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

/// <summary>
/// Provides year parameters loaded from embedded assembly resources.
/// </summary>
/// <remarks>
/// <para>
/// Year parameters are loaded lazily on first access and cached for
/// the lifetime of the application. The embedded JSON covers years 2016-2026.
/// </para>
/// <para>
/// This is the default provider used when no custom provider is configured.
/// </para>
/// </remarks>
public sealed class EmbeddedYearParameterProvider : IYearParameterProvider
{
    private const string ResourceName = "Turkish.HRSolutions.SalaryCalculator.Assets.year-constants.json";

    private readonly Lazy<Result<YearParameters>> _lazyParameters;
    private readonly Lazy<ImmutableDictionary<int, YearParameter>> _lazyYearLookup;
    private readonly Lazy<IReadOnlyList<int>> _lazyAvailableYears;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedYearParameterProvider"/> class.
    /// </summary>
    public EmbeddedYearParameterProvider()
    {
        _lazyParameters = new Lazy<Result<YearParameters>>(LoadFromEmbeddedResource);
        _lazyYearLookup = new Lazy<ImmutableDictionary<int, YearParameter>>(BuildYearLookup);
        _lazyAvailableYears = new Lazy<IReadOnlyList<int>>(() => [.. _lazyYearLookup.Value.Keys.Order()]);
    }

    /// <inheritdoc />
    public IReadOnlyList<int> AvailableYears => _lazyAvailableYears.Value;

    /// <inheritdoc />
    public YearParameter? GetParameter(int year) => CollectionExtensions.GetValueOrDefault(_lazyYearLookup.Value, year);

    /// <inheritdoc />
    public bool HasYear(int year) => _lazyYearLookup.Value.ContainsKey(year);

    /// <summary>
    /// Gets the result of loading the embedded resource.
    /// Use this to check for loading errors.
    /// </summary>
    public Result<YearParameters> LoadResult => _lazyParameters.Value;

    private static Result<YearParameters> LoadFromEmbeddedResource()
    {
        return JsonLoader.LoadFromEmbeddedResource(
            typeof(EmbeddedYearParameterProvider).Assembly,
            ResourceName,
            SalaryCalculatorJsonContext.Default.YearParameters);
    }

    private ImmutableDictionary<int, YearParameter> BuildYearLookup()
    {
        var result = _lazyParameters.Value;

        if (result.IsFailure || result.Value?.Parameters is null)
        {
            return ImmutableDictionary.Create<int, YearParameter>();
        }

        return result.Value.Parameters.ToImmutableDictionary(p => p.Year);
    }
}
