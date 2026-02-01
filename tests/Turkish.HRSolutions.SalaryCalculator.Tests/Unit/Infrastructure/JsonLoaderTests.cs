#pragma warning disable CA1707

using System.Text;
using System.Text.Json.Serialization;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure;

public sealed partial class JsonLoaderTests
{
    private sealed record TestObject
    {
        public string Name { get; init; } = "";
        public int Value { get; init; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(TestObject))]
    private sealed partial class TestJsonContext : JsonSerializerContext;

    [Test]
    public async Task LoadFromStream_Should_ReturnSuccess_When_JsonIsValid()
    {
        // Arrange
        const string json = """
                   {
                     "Name": "Test",
                     "Value": 123
                   }
                   """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = JsonLoader.LoadFromStream(stream, TestJsonContext.Default.TestObject, "Test Source");

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsNotNull();
        await Assert.That(result.Value!.Name).IsEqualTo("Test");
        await Assert.That(result.Value.Value).IsEqualTo(123);
    }

    [Test]
    public async Task LoadFromStream_Should_ReturnFailure_When_JsonIsInvalid()
    {
        // Arrange
        const string json = "{ Invalid Json }";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = JsonLoader.LoadFromStream(stream, TestJsonContext.Default.TestObject, "Test Source");

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).Count().IsEqualTo(1);
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.JsonDeserializationFailed);
        await Assert.That(result.Errors[0].Message).Contains("Failed to deserialize");
    }

    [Test]
    public async Task LoadFromStream_Should_ReturnFailure_When_StreamIsEmpty()
    {
        // Arrange
        await using var stream = new MemoryStream([]);

        // Act
        var result = JsonLoader.LoadFromStream(stream, TestJsonContext.Default.TestObject, "Test Source");

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).Count().IsEqualTo(1);
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.JsonDeserializationFailed);
    }
}
