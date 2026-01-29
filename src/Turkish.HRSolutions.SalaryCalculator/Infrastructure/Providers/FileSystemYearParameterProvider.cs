using System.Collections.Immutable;
using System.IO.Abstractions;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

/// <summary>
/// Provides year parameters loaded from a JSON file on the file system.
/// </summary>
/// <remarks>
/// <para>
/// Year parameters are loaded lazily on first access and cached for
/// the lifetime of the provider instance.
/// </para>
/// <para>
/// Use this provider when you want to load year parameters from an external
/// JSON file rather than using the embedded assembly resources.
/// </para>
/// </remarks>
public sealed class FileSystemYearParameterProvider : IYearParameterProvider
{
    private readonly string _filePath;
    private readonly IFileSystem _fileSystem;
    private readonly Lazy<Result<YearParameters>> _lazyParameters;
    private readonly Lazy<ImmutableDictionary<int, YearParameter>> _lazyYearLookup;
    private readonly Lazy<IReadOnlyList<int>> _lazyAvailableYears;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemYearParameterProvider"/> class.
    /// </summary>
    /// <param name="filePath">The path to the year-constants.json file.</param>
    public FileSystemYearParameterProvider(string filePath)
        : this(filePath, new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemYearParameterProvider"/> class
    /// with a custom file system abstraction.
    /// </summary>
    /// <param name="filePath">The path to the year-constants.json file.</param>
    /// <param name="fileSystem">The file system abstraction to use.</param>
    public FileSystemYearParameterProvider(string filePath, IFileSystem fileSystem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _filePath = filePath;
        _fileSystem = fileSystem;
        _lazyParameters = new Lazy<Result<YearParameters>>(LoadFromFile);
        _lazyYearLookup = new Lazy<ImmutableDictionary<int, YearParameter>>(BuildYearLookup);
        _lazyAvailableYears = new Lazy<IReadOnlyList<int>>(() =>
            [.. _lazyYearLookup.Value.Keys.Order()]);
    }

    /// <inheritdoc />
    public IReadOnlyList<int> AvailableYears => _lazyAvailableYears.Value;

    /// <inheritdoc />
    public YearParameter? GetParameter(int year)
    {
        return _lazyYearLookup.Value.TryGetValue(year, out var param) ? param : null;
    }

    /// <inheritdoc />
    public bool HasYear(int year) => _lazyYearLookup.Value.ContainsKey(year);

    /// <summary>
    /// Gets the result of loading the file.
    /// Use this to check for loading errors.
    /// </summary>
    public Result<YearParameters> LoadResult => _lazyParameters.Value;

    private Result<YearParameters> LoadFromFile()
    {
        if (!_fileSystem.File.Exists(_filePath))
        {
            return Result<YearParameters>.Failure(Error.Configuration(
                ErrorCode.ConfigurationFileNotFound,
                $"Year parameters file not found: '{_filePath}'"));
        }

        try
        {
#pragma warning disable MA0045 // Synchronous file read is intentional for lazy loading
            var json = _fileSystem.File.ReadAllText(_filePath);
#pragma warning restore MA0045

            if (string.IsNullOrWhiteSpace(json))
            {
                return Result<YearParameters>.Failure(Error.Configuration(
                    ErrorCode.ConfigurationFileEmpty,
                    $"Year parameters file is empty: '{_filePath}'"));
            }

            return JsonLoader.LoadFromString(
                json,
                SalaryCalculatorJsonContext.Default.YearParameters,
                _filePath);
        }
        catch (IOException ex)
        {
            return Result<YearParameters>.Failure(Error.Infrastructure(
                ErrorCode.FileReadFailed,
                $"Failed to read year parameters file '{_filePath}': {ex.Message}",
                ex));
        }
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
