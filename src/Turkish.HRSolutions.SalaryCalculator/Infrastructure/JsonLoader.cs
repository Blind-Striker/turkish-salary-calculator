using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;

namespace Turkish.HRSolutions.SalaryCalculator.Infrastructure;

/// <summary>
/// Internal utility for loading and deserializing JSON from various sources.
/// </summary>
internal static class JsonLoader
{
    /// <summary>
    /// Deserializes JSON from a stream using source-generated serialization.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="stream">The stream containing JSON data.</param>
    /// <param name="typeInfo">The source-generated type info for AOT-safe deserialization.</param>
    /// <param name="sourceName">A descriptive name for the source (for error messages).</param>
    /// <returns>A result containing the deserialized object or errors.</returns>
    public static Result<T> LoadFromStream<T>(Stream stream, JsonTypeInfo<T> typeInfo, string sourceName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(typeInfo);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        try
        {
            var result = JsonSerializer.Deserialize(stream, typeInfo);

            return result is not null
                ? Result<T>.Success(result)
                : Result<T>.Failure(Error.Infrastructure(
                    ErrorCode.JsonDeserializationFailed,
                    $"Deserialization of '{sourceName}' returned null."));
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure(Error.Infrastructure(
                ErrorCode.JsonDeserializationFailed,
                $"Failed to deserialize '{sourceName}': {ex.Message}",
                ex));
        }
    }

    /// <summary>
    /// Deserializes JSON from a string using source-generated serialization.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <param name="typeInfo">The source-generated type info for AOT-safe deserialization.</param>
    /// <param name="sourceName">A descriptive name for the source (for error messages).</param>
    /// <returns>A result containing the deserialized object or errors.</returns>
    public static Result<T> LoadFromString<T>(string json, JsonTypeInfo<T> typeInfo, string sourceName)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(typeInfo);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        try
        {
            var result = JsonSerializer.Deserialize(json, typeInfo);

            return result is not null
                ? Result<T>.Success(result)
                : Result<T>.Failure(Error.Infrastructure(ErrorCode.JsonDeserializationFailed, $"Deserialization of '{sourceName}' returned null."));
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure(Error.Infrastructure(ErrorCode.JsonDeserializationFailed, $"Failed to deserialize '{sourceName}': {ex.Message}", ex));
        }
    }

    /// <summary>
    /// Loads JSON from an embedded assembly resource.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="assembly">The assembly containing the resource.</param>
    /// <param name="resourceName">The fully-qualified resource name.</param>
    /// <param name="typeInfo">The source-generated type info for AOT-safe deserialization.</param>
    /// <returns>A result containing the deserialized object or errors.</returns>
    public static Result<T> LoadFromEmbeddedResource<T>(Assembly assembly, string resourceName, JsonTypeInfo<T> typeInfo)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentNullException.ThrowIfNull(typeInfo);

#pragma warning disable MA0045 // Synchronous method with synchronous stream disposal
        using var stream = assembly.GetManifestResourceStream(resourceName);
#pragma warning restore MA0045

        if (stream is null)
        {
            return Result<T>.Failure(Error.Infrastructure(
                ErrorCode.EmbeddedResourceNotFound,
                $"Embedded resource '{resourceName}' not found in assembly '{assembly.FullName}'."));
        }

        return LoadFromStream(stream, typeInfo, resourceName);
    }
}
