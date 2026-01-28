using System.IO.Abstractions;
using System.Text.Json;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure;

public class JsonParameterProvider(IFileSystem fileSystem)
{
    public async Task<Result<YearParameters>> LoadYearParametersAsync(string filePath, CancellationToken ct = default) =>
        await LoadParameterAsync<YearParameters>(filePath, ct).ConfigureAwait(false);

    public async Task<Result<ConstantParameters>> LoadFixtureAsync(string filePath, CancellationToken ct = default) =>
        await LoadParameterAsync<ConstantParameters>(filePath, ct).ConfigureAwait(false);

    private async Task<Result<TModel>> LoadParameterAsync<TModel>(string filePath, CancellationToken ct) where TModel : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fileInfo = fileSystem.FileInfo.New(filePath);

        if (!fileInfo.Exists)
        {
            return Result<TModel>.Failure(Error.Configuration(
                ErrorCode.ConfigurationFileNotFound,
                $"{typeof(TModel).Name} file not found at {fileInfo.FullName}"));
        }

        try
        {
            var rawParameterJson = await fileInfo.ReadAllTextAsync(ct).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rawParameterJson))
            {
                return Result<TModel>.Failure(Error.Configuration(
                    ErrorCode.ConfigurationFileEmpty,
                    $"Parameter file '{fileInfo.FullName}' is empty."));
            }

            var deserialized = JsonSerializer.Deserialize<TModel>(rawParameterJson, SalaryCalculatorJsonContext.JsonOptions);

            return deserialized != null
                ? Result<TModel>.Success(deserialized)
                : Result<TModel>.Failure(Error.Configuration(
                    ErrorCode.JsonDeserializationFailed,
                    "JSON deserialization failed, deserialized value is null"));
        }
        catch (IOException ex)
        {
            return Result<TModel>.Failure(Error.Infrastructure(
                ErrorCode.FileReadFailed,
                "File read failed",
                ex));
        }
        catch (JsonException ex)
        {
            return Result<TModel>.Failure(Error.Infrastructure(
                ErrorCode.JsonDeserializationFailed,
                "JSON deserialization failed",
                ex));
        }
    }
}
