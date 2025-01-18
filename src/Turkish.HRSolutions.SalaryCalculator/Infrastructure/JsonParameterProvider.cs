using System.IO.Abstractions;
using System.Text.Json;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure;

public class JsonParameterProvider
{
    private readonly IFileSystem _fileSystem;

    public JsonParameterProvider(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task<Result<YearParameters>> LoadYearParametersAsync(string filePath, CancellationToken ct = default)
    {
        return await LoadParameterAsync<YearParameters>(filePath, ct);
    }

    public async Task<Result<ConstantParameters>> LoadFixtureAsync(string filePath, CancellationToken ct = default)
    {
        return await LoadParameterAsync<ConstantParameters>(filePath, ct);
    }

    private async Task<Result<TModel>> LoadParameterAsync<TModel>(string filePath, CancellationToken ct) where TModel : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fileInfo = _fileSystem.FileInfo.New(filePath);

        if (!fileInfo.Exists)
        {
            return Result<TModel>.Failure($"{typeof(TModel).Name} file not found at {fileInfo.FullName}");
        }

        try
        {
            var rawParameterJson = await fileInfo.ReadAllTextAsync(ct);

            if (string.IsNullOrWhiteSpace(rawParameterJson))
            {
                return Result<TModel>.Failure($"Parameter file '{fileInfo.FullName}' is empty.");
            }

            var deserialized = JsonSerializer.Deserialize<TModel>(rawParameterJson, SalaryCalculatorJsonContext.JsonOptions);

            return deserialized != null
                ? Result<TModel>.Success(deserialized)
                : Result<TModel>.Failure("JSON deserialization failed, deserialized value is null");
        }
        catch (IOException ex)
        {
            return Result<TModel>.Failure("File read failed", ex);
        }
        catch (JsonException ex)
        {
            return Result<TModel>.Failure("JSON deserialization failed", ex);
        }
    }
}
