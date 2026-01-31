using System.Collections.Immutable;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

/// <summary>
/// Provides calculation constants loaded from embedded assembly resources.
/// </summary>
/// <remarks>
/// <para>
/// Calculation constants are loaded lazily on first access and cached for
/// the lifetime of the application.
/// </para>
/// <para>
/// Constants include employee type definitions (17 boolean flags each),
/// SGK rates, stamp tax rates, disability options, and AGI options.
/// </para>
/// <para>
/// This is the default provider used when no custom provider is configured.
/// </para>
/// </remarks>
public sealed class EmbeddedCalculationConstantsProvider : Application.Providers.ICalculationConstantsProvider
{
    private const string ResourceName = "Turkish.HRSolutions.SalaryCalculator.Assets.calculation-constants.json";

    private readonly Lazy<Result<ConstantParameters>> _lazyParameters;
    private readonly Lazy<ImmutableDictionary<int, EmployeeTypeConstant>> _lazyEmployeeTypeLookup;
    private readonly Lazy<ImmutableDictionary<int, DisabilityConstant>> _lazyDisabilityLookup;
    private readonly Lazy<ImmutableDictionary<int, EmployeeEducationTypeConstant>> _lazyEducationTypeLookup;
    private readonly Lazy<ImmutableDictionary<int, AgiConstant>> _lazyAgiLookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedCalculationConstantsProvider"/> class.
    /// </summary>
    public EmbeddedCalculationConstantsProvider()
    {
        _lazyParameters = new Lazy<Result<ConstantParameters>>(LoadFromEmbeddedResource);
        _lazyEmployeeTypeLookup = new Lazy<ImmutableDictionary<int, EmployeeTypeConstant>>(BuildEmployeeTypeLookup);
        _lazyDisabilityLookup = new Lazy<ImmutableDictionary<int, DisabilityConstant>>(BuildDisabilityLookup);
        _lazyEducationTypeLookup = new Lazy<ImmutableDictionary<int, EmployeeEducationTypeConstant>>(BuildEducationTypeLookup);
        _lazyAgiLookup = new Lazy<ImmutableDictionary<int, AgiConstant>>(BuildAgiLookup);
    }

    /// <inheritdoc />
    public CalculationConstant Constants
    {
        get
        {
            var result = _lazyParameters.Value;

            return result.IsSuccess
                ? result.Value.CalculationConstants
                : throw new InvalidOperationException(
                    $"Failed to load calculation constants: {string.Join("; ", result.Errors.Select(e => e.Message))}");
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<EmployeeTypeConstant> AllEmployeeTypes =>
        _lazyParameters.Value.IsSuccess
            ? _lazyParameters.Value.Value.EmployeeTypeConstants
            : [];

    /// <inheritdoc />
    public IReadOnlyList<EmployeeEducationTypeConstant> AllEducationTypes =>
        _lazyParameters.Value.IsSuccess
            ? _lazyParameters.Value.Value.EmployeeEducationContants
            : [];

    /// <inheritdoc />
    public IReadOnlyList<AgiConstant> AllAgiOptions =>
        _lazyParameters.Value.IsSuccess
            ? _lazyParameters.Value.Value.AgiConstants
            : [];

    /// <inheritdoc />
    public EmployeeTypeConstant? GetEmployeeType(EmployeeTypeId typeId) =>
        GetEmployeeType(typeId.Value);

    /// <inheritdoc />
    public EmployeeTypeConstant? GetEmployeeType(int typeId) =>
        _lazyEmployeeTypeLookup.Value.TryGetValue(typeId, out var constant) ? constant : null;

    /// <inheritdoc />
    public DisabilityConstant? GetDisability(DisabilityDegreeId degreeId) =>
        GetDisability(degreeId.Value);

    /// <inheritdoc />
    public DisabilityConstant? GetDisability(int degree) =>
        _lazyDisabilityLookup.Value.TryGetValue(degree, out var constant) ? constant : null;

    /// <inheritdoc />
    public EmployeeEducationTypeConstant? GetEducationType(EducationTypeId typeId) =>
        _lazyEducationTypeLookup.Value.TryGetValue(typeId.Value, out var constant) ? constant : null;

    /// <inheritdoc />
    public AgiConstant? GetAgi(int agiId) =>
        _lazyAgiLookup.Value.TryGetValue(agiId, out var constant) ? constant : null;

    /// <inheritdoc />
    public double GetAgiRate(SpouseStatus spouseStatus, int numberOfChildren)
    {
        var agiOptions = AllAgiOptions;
        if (agiOptions.Count == 0)
        {
            return 0d;
        }

        // Use SpouseStatus.EffectiveAgiTypeKey directly
        var typeFilter = spouseStatus.EffectiveAgiTypeKey;

        var filtered = agiOptions.Where(a => a.Type.Equals(typeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        if (filtered.Count == 0)
        {
            return 0d;
        }

        // Unmarried has a single entry (no children matching)
        if (spouseStatus == SpouseStatus.Unmarried)
        {
            return filtered[0].Rate;
        }

        // Match by equality operator + children count
        var match = filtered.FirstOrDefault(p => p.Equality switch
        {
            "Eq" => numberOfChildren == p.Children,
            "Gt" => numberOfChildren > p.Children,
            "Gte" => numberOfChildren >= p.Children,
            "Lt" => numberOfChildren < p.Children,
            "Lte" => numberOfChildren <= p.Children,
            _ => false,
        });

        if (match is not null)
        {
            return match.Rate;
        }

        // Fallback: closest match by children difference
        return filtered.OrderBy(p => Math.Abs(p.Children - numberOfChildren)).First().Rate;
    }

    /// <summary>
    /// Gets the result of loading the embedded resource.
    /// Use this to check for loading errors.
    /// </summary>
    public Result<ConstantParameters> LoadResult => _lazyParameters.Value;

    private static Result<ConstantParameters> LoadFromEmbeddedResource()
    {
        return JsonLoader.LoadFromEmbeddedResource(
            typeof(EmbeddedCalculationConstantsProvider).Assembly,
            ResourceName,
            SalaryCalculatorJsonContext.Default.ConstantParameters);
    }

    private ImmutableDictionary<int, EmployeeTypeConstant> BuildEmployeeTypeLookup()
    {
        var result = _lazyParameters.Value;

        if (result.IsFailure || result.Value?.EmployeeTypeConstants is null)
        {
            return ImmutableDictionary.Create<int, EmployeeTypeConstant>();
        }

        return result.Value.EmployeeTypeConstants.ToImmutableDictionary(e => e.Id);
    }

    private ImmutableDictionary<int, DisabilityConstant> BuildDisabilityLookup()
    {
        var result = _lazyParameters.Value;

        if (result.IsFailure || result.Value?.DisabilityConstants is null)
        {
            return ImmutableDictionary.Create<int, DisabilityConstant>();
        }

        return result.Value.DisabilityConstants.ToImmutableDictionary(d => d.Degree);
    }

    private ImmutableDictionary<int, EmployeeEducationTypeConstant> BuildEducationTypeLookup()
    {
        var result = _lazyParameters.Value;

        if (result.IsFailure || result.Value?.EmployeeEducationContants is null)
        {
            return ImmutableDictionary.Create<int, EmployeeEducationTypeConstant>();
        }

        return result.Value.EmployeeEducationContants.ToImmutableDictionary(e => e.Id);
    }

    private ImmutableDictionary<int, AgiConstant> BuildAgiLookup()
    {
        var result = _lazyParameters.Value;

        if (result.IsFailure || result.Value?.AgiConstants is null)
        {
            return ImmutableDictionary.Create<int, AgiConstant>();
        }

        return result.Value.AgiConstants.ToImmutableDictionary(a => a.Id);
    }
}
