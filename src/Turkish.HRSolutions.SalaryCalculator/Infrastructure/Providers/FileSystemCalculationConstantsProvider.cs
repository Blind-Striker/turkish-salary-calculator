using System.Collections.Immutable;
using System.IO.Abstractions;
using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

/// <summary>
/// Provides calculation constants loaded from a JSON file on the file system.
/// </summary>
/// <remarks>
/// <para>
/// Calculation constants are loaded lazily on first access and cached for
/// the lifetime of the provider instance.
/// </para>
/// <para>
/// Use this provider when you want to load calculation constants from an external
/// JSON file rather than using the embedded assembly resources.
/// </para>
/// <para>
/// <b>WARNING:</b> Calculation constants include employee type flags that control
/// validation and calculation logic. Incorrect values will cause validation errors
/// or wrong calculations. Only use this provider if you know what you're doing.
/// </para>
/// </remarks>
public sealed class FileSystemCalculationConstantsProvider : ICalculationConstantsProvider
{
    private readonly string _filePath;
    private readonly IFileSystem _fileSystem;
    private readonly Lazy<Result<ConstantParameters>> _lazyParameters;
    private readonly Lazy<ImmutableDictionary<int, EmployeeTypeConstant>> _lazyEmployeeTypeLookup;
    private readonly Lazy<ImmutableDictionary<int, DisabilityConstant>> _lazyDisabilityLookup;
    private readonly Lazy<ImmutableDictionary<int, EmployeeEducationTypeConstant>> _lazyEducationTypeLookup;
    private readonly Lazy<ImmutableDictionary<int, AgiConstant>> _lazyAgiLookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemCalculationConstantsProvider"/> class.
    /// </summary>
    /// <param name="filePath">The path to the calculation-constants.json file.</param>
    public FileSystemCalculationConstantsProvider(string filePath)
        : this(filePath, new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemCalculationConstantsProvider"/> class
    /// with a custom file system abstraction.
    /// </summary>
    /// <param name="filePath">The path to the calculation-constants.json file.</param>
    /// <param name="fileSystem">The file system abstraction to use.</param>
    public FileSystemCalculationConstantsProvider(string filePath, IFileSystem fileSystem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _filePath = filePath;
        _fileSystem = fileSystem;
        _lazyParameters = new Lazy<Result<ConstantParameters>>(LoadFromFile);
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
                : throw new InvalidOperationException($"Failed to load calculation constants: {string.Join("; ", result.Errors.Select(e => e.Message))}");
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
    public EmployeeTypeConstant? GetEmployeeType(EmployeeTypeId typeId) => GetEmployeeType(typeId.Value);

    /// <inheritdoc />
    public EmployeeTypeConstant? GetEmployeeType(int typeId) => CollectionExtensions.GetValueOrDefault(_lazyEmployeeTypeLookup.Value, typeId);

    /// <inheritdoc />
    public DisabilityConstant? GetDisability(DisabilityDegreeId degreeId) => GetDisability(degreeId.Value);

    /// <inheritdoc />
    public DisabilityConstant? GetDisability(int degree) => CollectionExtensions.GetValueOrDefault(_lazyDisabilityLookup.Value, degree);

    /// <inheritdoc />
    public EmployeeEducationTypeConstant? GetEducationType(EducationTypeId typeId) => CollectionExtensions.GetValueOrDefault(_lazyEducationTypeLookup.Value, typeId.Value);

    /// <inheritdoc />
    public AgiConstant? GetAgi(int agiId) => CollectionExtensions.GetValueOrDefault(_lazyAgiLookup.Value, agiId);

    /// <summary>
    /// Gets the result of loading the file.
    /// Use this to check for loading errors.
    /// </summary>
    public Result<ConstantParameters> LoadResult => _lazyParameters.Value;

    private Result<ConstantParameters> LoadFromFile()
    {
        if (!_fileSystem.File.Exists(_filePath))
        {
            return Result<ConstantParameters>.Failure(Error.Configuration(ErrorCode.ConfigurationFileNotFound, $"Calculation constants file not found: '{_filePath}'"));
        }

        try
        {
#pragma warning disable MA0045 // Synchronous file read is intentional for lazy loading
            var json = _fileSystem.File.ReadAllText(_filePath);
#pragma warning restore MA0045

            if (string.IsNullOrWhiteSpace(json))
            {
                return Result<ConstantParameters>.Failure(Error.Configuration(ErrorCode.ConfigurationFileEmpty, $"Calculation constants file is empty: '{_filePath}'"));
            }

            return JsonLoader.LoadFromString(json, SalaryCalculatorJsonContext.Default.ConstantParameters, _filePath);
        }
        catch (IOException ex)
        {
            return Result<ConstantParameters>.Failure(Error.Infrastructure(
                ErrorCode.FileReadFailed,
                $"Failed to read calculation constants file '{_filePath}': {ex.Message}",
                ex));
        }
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
