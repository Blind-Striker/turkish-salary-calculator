namespace TurkHRSolutions.TurkEmployCalc.Services.Parameter;

public class ParameterService
{
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptions _options;

    public ParameterService() : this(new FileSystem())
    {
    }

    public ParameterService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        _options = new JsonSerializerOptions
        {
            TypeInfoResolver = TurkEmployCalcSerializationContext.Default,
        };
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public TurkEmployCalcParameters LoadParameters(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!_fileSystem.File.Exists(path))
        {
            throw new FileNotFoundException("The specified JSON file path is not valid.", path);
        }

        var fileContent = _fileSystem.File.ReadAllText(path);

        var turkEmployCalcParameters = JsonSerializer.Deserialize<TurkEmployCalcParameters>(fileContent, _options) ?? throw new InvalidOperationException();

        return turkEmployCalcParameters;
    }
}