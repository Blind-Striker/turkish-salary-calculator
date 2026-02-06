#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Common.Results;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Common.Results;

public class ResultTests
{
    // ═══════════════════════════════════════════════════════════════
    // Non-Generic Result Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Success_Should_Create_Successful_Result()
    {
        // Act
        var result = Result.Success();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.IsFailure).IsFalse();
        await Assert.That(result.Errors).IsEmpty();
    }

    [Test]
    public async Task Failure_With_Single_Error_Should_Create_Failed_Result()
    {
        // Arrange
        var error = new Error(ErrorCode.YearNotSpecified, "Year is required");

        // Act
        var result = Result.Failure(error);

        // Assert
        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).HasSingleItem();
        await Assert.That(result.Errors[0]).IsEqualTo(error);
    }

    [Test]
    public async Task Failure_With_Multiple_Errors_Should_Create_Failed_Result()
    {
        // Arrange
        var errors = new[]
        {
            new Error(ErrorCode.YearNotSpecified, "Year is required"),
            new Error(ErrorCode.InvalidSalaryAmount, "Salary must be positive"),
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        await Assert.That(result.Errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Failure_With_Code_And_Message_Should_Create_Failed_Result()
    {
        // Act
        var result = Result.Failure(ErrorCode.YearNotSupported, "Year 2015 not supported");

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.YearNotSupported);
        await Assert.That(result.Errors[0].Message).IsEqualTo("Year 2015 not supported");
    }

    [Test]
    public async Task BlockingErrors_Should_Filter_Only_Errors()
    {
        // Arrange
        var errors = new[]
        {
            new Error(ErrorCode.InvalidSalaryAmount, "Error 1", ErrorSeverity.Error),
            new Error(ErrorCode.AgiNotApplicable, "Warning 1", ErrorSeverity.Warning),
            new Error(ErrorCode.InvalidWorkedDays, "Error 2", ErrorSeverity.Error),
        };
        var result = Result.Failure(errors);

        // Assert
        await Assert.That(result.BlockingErrors.Count()).IsEqualTo(2);
    }

    [Test]
    public async Task Warnings_Should_Filter_Only_Warnings()
    {
        // Arrange
        var errors = new[]
        {
            new Error(ErrorCode.InvalidSalaryAmount, "Error 1", ErrorSeverity.Error),
            new Error(ErrorCode.AgiNotApplicable, "Warning 1", ErrorSeverity.Warning),
        };
        var result = Result.Failure(errors);

        // Assert
        await Assert.That(result.Warnings.Count()).IsEqualTo(1);
        await Assert.That(result.HasWarnings).IsTrue();
    }

    [Test]
    public async Task HasWarnings_Should_Return_False_When_No_Warnings()
    {
        // Arrange
        var result = Result.Failure(ErrorCode.YearNotSpecified, "Year is required");

        // Assert
        await Assert.That(result.HasWarnings).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Non-Generic Result - Implicit Conversions
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ImplicitConversion_From_Error_Should_Create_Failed_Result()
    {
        // Act
        Result result = new Error(ErrorCode.YearNotSpecified, "Year is required");

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).HasSingleItem();
    }

    [Test]
    public async Task ImplicitConversion_From_ErrorArray_Should_Create_Failed_Result()
    {
        // Act
        Result result = new Error[]
        {
            new(ErrorCode.YearNotSpecified, "Year is required"),
            new(ErrorCode.InvalidSalaryAmount, "Invalid salary"),
        };

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).Count().IsEqualTo(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // Generic Result<T> - Success Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Success_With_Value_Should_Create_Successful_Result()
    {
        // Act
        var result = Result<decimal>.Success(50_000m);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.IsFailure).IsFalse();
        await Assert.That(result.Value).IsEqualTo(50_000m);
        await Assert.That(result.Errors).IsEmpty();
    }

    [Test]
    public async Task Success_With_Warnings_Should_Preserve_Warnings()
    {
        // Arrange
        var warnings = new[]
        {
            new Error(ErrorCode.AgiNotApplicable, "AGI not applicable", ErrorSeverity.Warning),
        };

        // Act
        var result = Result<decimal>.Success(5_000m, warnings);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsEqualTo(5_000m);
        await Assert.That(result.HasWarnings).IsTrue();
        await Assert.That(result.Warnings.Count()).IsEqualTo(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // Generic Result<T> - Failure Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Failure_Should_Create_Failed_Result_With_Error()
    {
        // Arrange
        var error = new Error(ErrorCode.CalculationFailed, "Calculation failed");

        // Act
        var result = Result<decimal>.Failure(error);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).HasSingleItem();
    }

    [Test]
    public async Task GenericFailure_With_Code_And_Message_Should_Create_Failed_Result()
    {
        // Act
        var result = Result<string>.Failure(ErrorCode.EmbeddedResourceNotFound, "Resource not found");

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.EmbeddedResourceNotFound);
    }

    [Test]
    public async Task Failure_With_Exception_Should_Include_Exception()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = Result<int>.Failure(ErrorCode.UnexpectedError, "Unexpected error", exception);

        // Assert
        await Assert.That(result.Errors[0].Exception).IsEqualTo(exception);
    }

    // ═══════════════════════════════════════════════════════════════
    // Generic Result<T> - Value Access Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Value_Should_Throw_When_Failed()
    {
        // Arrange
        var result = Result<decimal>.Failure(ErrorCode.CalculationFailed, "Failed");

        // Act & Assert
        await Assert.That(() => result.Value).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TryGetValue_Should_Return_True_When_Successful()
    {
        // Arrange
        var result = Result<decimal>.Success(50_000m);

        // Act
        var success = result.TryGetValue(out var value);

        // Assert
        await Assert.That(success).IsTrue();
        await Assert.That(value).IsEqualTo(50_000m);
    }

    [Test]
    public async Task TryGetValue_Should_Return_False_When_Failed()
    {
        // Arrange
        var result = Result<decimal>.Failure(ErrorCode.CalculationFailed, "Failed");

        // Act
        var success = result.TryGetValue(out var value);

        // Assert
        await Assert.That(success).IsFalse();
        await Assert.That(value).IsEqualTo(default(decimal));
    }

    [Test]
    public async Task GetValueOrDefault_Should_Return_Value_When_Successful()
    {
        // Arrange
        var result = Result<decimal>.Success(50_000m);

        // Act
        var value = result.GetValueOrDefault(-1m);

        // Assert
        await Assert.That(value).IsEqualTo(50_000m);
    }

    [Test]
    public async Task GetValueOrDefault_Should_Return_Default_When_Failed()
    {
        // Arrange
        var result = Result<decimal>.Failure(ErrorCode.CalculationFailed, "Failed");

        // Act
        var value = result.GetValueOrDefault(-1m);

        // Assert
        await Assert.That(value).IsEqualTo(-1m);
    }

    [Test]
    public async Task GetValueOrThrow_Should_Return_Value_When_Successful()
    {
        // Arrange
        var result = Result<decimal>.Success(50_000m);

        // Act
        var value = result.GetValueOrThrow();

        // Assert
        await Assert.That(value).IsEqualTo(50_000m);
    }

    [Test]
    public async Task GetValueOrThrow_Should_Throw_When_Failed()
    {
        // Arrange
        var result = Result<decimal>.Failure(ErrorCode.CalculationFailed, "Failed");

        // Act & Assert
        await Assert.That(result.GetValueOrThrow).Throws<InvalidOperationException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Generic Result<T> - Implicit Conversions
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ImplicitConversion_From_Error_Should_Create_Failed_GenericResult()
    {
        // Act
        Result<decimal> result = new Error(ErrorCode.YearNotSpecified, "Year is required");

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task ImplicitConversion_From_ErrorArray_Should_Create_Failed_GenericResult()
    {
        // Act
        Result<decimal> result = new Error[]
        {
            new(ErrorCode.YearNotSpecified, "Year is required"),
            new(ErrorCode.InvalidSalaryAmount, "Invalid salary"),
        };

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).Count().IsEqualTo(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // Generic Result<T> - Deconstruction Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Deconstruct_Should_Return_Success_Values()
    {
        // Act
        var (isSuccess, value, errors) = Result<decimal>.Success(50_000m);

        // Assert
        await Assert.That(isSuccess).IsTrue();
        await Assert.That(value).IsEqualTo(50_000m);
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task Deconstruct_Should_Return_Failure_Values()
    {
        // Act
        var (isSuccess, value, errors) = Result<decimal>.Failure(ErrorCode.CalculationFailed, "Failed");

        // Assert
        await Assert.That(isSuccess).IsFalse();
        await Assert.That(value).IsEqualTo(default(decimal)); // Value is default for struct types
        await Assert.That(errors).Count().IsEqualTo(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // Edge Cases and Complex Scenarios
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Result_Should_Handle_Nullable_Value_Types()
    {
        // Act
        var result = Result<int?>.Success(null);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsNull();
    }

    [Test]
    public async Task Result_Should_Handle_Reference_Types()
    {
        // Act
        var result = Result<List<string>>.Success(["one", "two"]);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).Count().IsEqualTo(2);
    }

    [Test]
    public async Task BlockingErrors_And_Warnings_Should_Sum_To_Total_Errors()
    {
        // Arrange
        var errors = new[]
        {
            new Error(ErrorCode.InvalidSalaryAmount, "Error 1", ErrorSeverity.Error),
            new Error(ErrorCode.AgiNotApplicable, "Warning 1", ErrorSeverity.Warning),
            new Error(ErrorCode.InvalidWorkedDays, "Error 2", ErrorSeverity.Error),
            new Error(ErrorCode.RnDDaysNotApplicable, "Warning 2", ErrorSeverity.Warning),
        };
        var result = Result<decimal>.Failure(errors);

        // Assert
        await Assert.That(result.BlockingErrors.Count() + result.Warnings.Count()).IsEqualTo(result.Errors.Count);
        await Assert.That(result.BlockingErrors.Count()).IsEqualTo(2);
        await Assert.That(result.Warnings.Count()).IsEqualTo(2);
    }

    [Test]
    public async Task FromError_Should_Work_Same_As_Failure()
    {
        // Arrange & Act
        var error = new Error(ErrorCode.YearNotSpecified, "Year is required");
        var result1 = Result.Failure(error);
        var result2 = Result.FromError(error);

        // Assert
        await Assert.That(result1.Errors[0]).IsEqualTo(result2.Errors[0]);
    }

    [Test]
    public async Task FromErrors_Should_Work_Same_As_Failure_Multiple()
    {
        // Arrange & Act
        var errors = new Error[]
        {
            new(ErrorCode.YearNotSpecified, "Error 1"),
            new(ErrorCode.InvalidSalaryAmount, "Error 2"),
        };
        var result1 = Result.Failure(errors);
        var result2 = Result.FromErrors(errors);

        // Assert
        await Assert.That(result1.Errors.Count).IsEqualTo(result2.Errors.Count);
    }
}
