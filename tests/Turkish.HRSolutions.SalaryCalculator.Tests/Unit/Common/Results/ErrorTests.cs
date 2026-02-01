#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Common.Results;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Common.Results;

public class ErrorTests
{
    // ═══════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Constructor_Should_Set_All_Properties()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var error = new Error(
            ErrorCode.InvalidSalaryAmount,
            "Salary must be positive",
            ErrorSeverity.Error,
            "Salary",
            -100m,
            exception);

        // Assert
        await Assert.That(error.Code).IsEqualTo(ErrorCode.InvalidSalaryAmount);
        await Assert.That(error.Message).IsEqualTo("Salary must be positive");
        await Assert.That(error.Severity).IsEqualTo(ErrorSeverity.Error);
        await Assert.That(error.Field).IsEqualTo("Salary");
        await Assert.That(error.AttemptedValue).IsEqualTo(-100m);
        await Assert.That(error.Exception).IsEqualTo(exception);
    }

    [Test]
    public async Task Constructor_Should_Default_Severity_To_Error()
    {
        // Act
        var error = new Error(ErrorCode.YearNotSpecified, "Year is required");

        // Assert
        await Assert.That(error.Severity).IsEqualTo(ErrorSeverity.Error);
    }

    [Test]
    public async Task Constructor_Should_Default_Optional_Fields_To_Null()
    {
        // Act
        var error = new Error(ErrorCode.YearNotSpecified, "Year is required");

        // Assert
        await Assert.That(error.Field).IsNull();
        await Assert.That(error.AttemptedValue).IsNull();
        await Assert.That(error.Exception).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // IsWarning / IsError Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task IsWarning_Should_Return_True_When_Severity_Is_Warning()
    {
        // Arrange
        var warning = new Error(ErrorCode.SalaryBelowMinimumWage, "Below minimum", ErrorSeverity.Warning);

        // Assert
        await Assert.That(warning.IsWarning).IsTrue();
        await Assert.That(warning.IsError).IsFalse();
    }

    [Test]
    public async Task IsError_Should_Return_True_When_Severity_Is_Error()
    {
        // Arrange
        var error = new Error(ErrorCode.InvalidSalaryAmount, "Invalid salary", ErrorSeverity.Error);

        // Assert
        await Assert.That(error.IsError).IsTrue();
        await Assert.That(error.IsWarning).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Factory Method Tests - Validation
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Validation_Should_Create_Error_With_Field_Info()
    {
        // Act
        var error = Error.Validation(
            ErrorCode.InvalidWorkedDays,
            "Worked days must be between 0 and 30",
            "WorkedDays",
            35);

        // Assert
        await Assert.That(error.Code).IsEqualTo(ErrorCode.InvalidWorkedDays);
        await Assert.That(error.Severity).IsEqualTo(ErrorSeverity.Error);
        await Assert.That(error.Field).IsEqualTo("WorkedDays");
        await Assert.That(error.AttemptedValue).IsEqualTo(35);
    }

    [Test]
    public async Task ValidationWarning_Should_Create_Warning_With_Field_Info()
    {
        // Act
        var warning = Error.ValidationWarning(
            ErrorCode.SalaryBelowMinimumWage,
            "Salary is below minimum wage",
            "Salary");

        // Assert
        await Assert.That(warning.Severity).IsEqualTo(ErrorSeverity.Warning);
        await Assert.That(warning.Field).IsEqualTo("Salary");
    }

    [Test]
    public async Task ValidationWarning_Should_Allow_Null_Field()
    {
        // Act
        var warning = Error.ValidationWarning(
            ErrorCode.SalaryBelowMinimumWage,
            "General warning");

        // Assert
        await Assert.That(warning.Field).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // Factory Method Tests - Configuration
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Configuration_Should_Create_Error()
    {
        // Act
        var error = Error.Configuration(
            ErrorCode.AgiNotApplicableForYear,
            "AGI is not available for years 2022+");

        // Assert
        await Assert.That(error.Code).IsEqualTo(ErrorCode.AgiNotApplicableForYear);
        await Assert.That(error.Severity).IsEqualTo(ErrorSeverity.Error);
        await Assert.That(error.Field).IsNull();
    }

    [Test]
    public async Task ConfigurationWarning_Should_Create_Warning()
    {
        // Act
        var warning = Error.ConfigurationWarning(
            ErrorCode.Discount5746NotApplicableForEmployeeType,
            "5746 discount ignored for this employee type");

        // Assert
        await Assert.That(warning.Severity).IsEqualTo(ErrorSeverity.Warning);
    }

    // ═══════════════════════════════════════════════════════════════
    // Factory Method Tests - Infrastructure
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Infrastructure_Should_Create_Error_With_Exception()
    {
        // Arrange
        var exception = new FileNotFoundException("Config not found", "config.json");

        // Act
        var error = Error.Infrastructure(
            ErrorCode.FileNotFound,
            "Configuration file not found",
            exception);

        // Assert
        await Assert.That(error.Code).IsEqualTo(ErrorCode.FileNotFound);
        await Assert.That(error.Severity).IsEqualTo(ErrorSeverity.Error);
        await Assert.That(error.Exception).IsEqualTo(exception);
    }

    [Test]
    public async Task Infrastructure_Should_Allow_Null_Exception()
    {
        // Act
        var error = Error.Infrastructure(
            ErrorCode.EmbeddedResourceNotFound,
            "Resource not found");

        // Assert
        await Assert.That(error.Exception).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToString_Should_Format_Error_Without_Field()
    {
        // Arrange
        var error = new Error(ErrorCode.YearNotSupported, "Year 2015 not supported");

        // Act
        var str = error.ToString();

        // Assert
        await Assert.That(str).IsEqualTo("[Error:YearNotSupported] Year 2015 not supported");
    }

    [Test]
    public async Task ToString_Should_Include_Field_When_Present()
    {
        // Arrange
        var error = Error.Validation(
            ErrorCode.InvalidWorkedDays,
            "Must be 0-30",
            "WorkedDays",
            45);

        // Act
        var str = error.ToString();

        // Assert
        await Assert.That(str).IsEqualTo("[Error:InvalidWorkedDays] WorkedDays: Must be 0-30");
    }

    [Test]
    public async Task ToString_Should_Show_Warning_Severity()
    {
        // Arrange
        var warning = new Error(ErrorCode.SalaryBelowMinimumWage, "Below minimum", ErrorSeverity.Warning);

        // Act
        var str = warning.ToString();

        // Assert
        await Assert.That(str).Contains("[Warning:");
    }

    // ═══════════════════════════════════════════════════════════════
    // Record Equality Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Errors_With_Same_Values_Should_Be_Equal()
    {
        // Arrange
        var error1 = Error.Validation(ErrorCode.InvalidSalaryAmount, "Invalid", "Salary", -100m);
        var error2 = Error.Validation(ErrorCode.InvalidSalaryAmount, "Invalid", "Salary", -100m);

        // Assert
        await Assert.That(error1).IsEqualTo(error2);
        await Assert.That(error1.GetHashCode()).IsEqualTo(error2.GetHashCode());
    }

    [Test]
    public async Task Errors_With_Different_Values_Should_Not_Be_Equal()
    {
        // Arrange
        var error1 = Error.Validation(ErrorCode.InvalidSalaryAmount, "Invalid", "Salary", -100m);
        var error2 = Error.Validation(ErrorCode.InvalidWorkedDays, "Invalid", "WorkedDays", 45);

        // Assert
        await Assert.That(error1).IsNotEqualTo(error2);
    }
}
