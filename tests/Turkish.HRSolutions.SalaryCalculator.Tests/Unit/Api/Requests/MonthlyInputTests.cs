using Turkish.HRSolutions.SalaryCalculator.Api.Requests;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Api.Requests;

public class MonthlyInputTests
{
    // ═══════════════════════════════════════════════════════════════
    // FillForward Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FillForward_Should_Return_Failure_When_No_Entries_Provided()
    {
        // Arrange
        var entries = Array.Empty<(MonthsOfYear, decimal)>();

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors).HasSingleItem();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.FillNoEntries);
    }

    [Test]
    public async Task FillForward_Should_Return_Failure_When_First_Entry_Not_January()
    {
        // Arrange
        var entries = new[] { (MonthsOfYear.March, 30_000m) };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.FillForwardMustStartWithJanuary);
        await Assert.That(result.Errors[0].Field).IsEqualTo("entries[0].month");
    }

    [Test]
    public async Task FillForward_Should_Return_Failure_When_Entries_Not_In_Chronological_Order()
    {
        // Arrange
        var entries = new[]
        {
            (MonthsOfYear.January, 30_000m),
            (MonthsOfYear.July, 40_000m),
            (MonthsOfYear.March, 35_000m), // Out of order!
        };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.FillForwardNonSequentialMonths);
    }

    [Test]
    public async Task FillForward_Should_Return_Success_When_Single_January_Entry()
    {
        // Arrange
        var entries = new[] { (MonthsOfYear.January, 30_000m) };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).Count().IsEqualTo(12);
        await Assert.That(result.Value.All(m => m.Salary == 30_000m)).IsTrue();
    }

    [Test]
    public async Task FillForward_Should_Fill_Gaps_When_Sparse_Entries_Provided()
    {
        // Arrange - Raise in July
        var entries = new[]
        {
            (MonthsOfYear.January, 30_000m),
            (MonthsOfYear.July, 40_000m),
        };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).Count().IsEqualTo(12);

        // Jan-Jun should be 30,000
        await Assert.That(result.Value[0].Salary).IsEqualTo(30_000m); // Jan
        await Assert.That(result.Value[5].Salary).IsEqualTo(30_000m); // Jun

        // Jul-Dec should be 40,000
        await Assert.That(result.Value[6].Salary).IsEqualTo(40_000m);  // Jul
        await Assert.That(result.Value[11].Salary).IsEqualTo(40_000m); // Dec
    }

    [Test]
    public async Task FillForward_Should_Preserve_Month_Sequence()
    {
        // Arrange
        var entries = new[] { (MonthsOfYear.January, 50_000m) };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();

        var allMonths = MonthsOfYear.AllMonths;
        for (var i = 0; i < 12; i++)
        {
            await Assert.That(result.Value[i].Month).IsEqualTo(allMonths[i]);
        }
    }

    [Test]
    public async Task FillForward_Should_Use_Default_WorkedDays_And_RnDDays_When_Simple_Overload()
    {
        // Arrange
        var entries = new[] { (MonthsOfYear.January, 30_000m) };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.All(m => m.WorkedDays == 30)).IsTrue();
        await Assert.That(result.Value.All(m => m.RnDDays == 0)).IsTrue();
    }

    [Test]
    public async Task FillForward_Should_Fill_Custom_WorkedDays_And_RnDDays()
    {
        // Arrange
        var entries = new[]
        {
            (MonthsOfYear.January, 30_000m, 25, 10),
            (MonthsOfYear.July, 40_000m, 30, 15),
        };

        // Act
        var result = MonthlyInput.FillForward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();

        // Jan-Jun values
        await Assert.That(result.Value[0].WorkedDays).IsEqualTo(25);
        await Assert.That(result.Value[0].RnDDays).IsEqualTo(10);
        await Assert.That(result.Value[5].WorkedDays).IsEqualTo(25);

        // Jul-Dec values
        await Assert.That(result.Value[6].WorkedDays).IsEqualTo(30);
        await Assert.That(result.Value[6].RnDDays).IsEqualTo(15);
    }

    // ═══════════════════════════════════════════════════════════════
    // FillBackward Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FillBackward_Should_Return_Failure_When_No_Entries_Provided()
    {
        // Arrange
        var entries = Array.Empty<(MonthsOfYear, decimal)>();

        // Act
        var result = MonthlyInput.FillBackward(entries);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.FillNoEntries);
    }

    [Test]
    public async Task FillBackward_Should_Return_Failure_When_Last_Entry_Not_December()
    {
        // Arrange
        var entries = new[] { (MonthsOfYear.July, 40_000m) };

        // Act
        var result = MonthlyInput.FillBackward(entries);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.FillBackwardMustEndWithDecember);
    }

    [Test]
    public async Task FillBackward_Should_Return_Failure_When_Entries_Not_In_Chronological_Order()
    {
        // Arrange
        var entries = new[]
        {
            (MonthsOfYear.July, 40_000m),
            (MonthsOfYear.March, 35_000m), // Out of order!
            (MonthsOfYear.December, 50_000m),
        };

        // Act
        var result = MonthlyInput.FillBackward(entries);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors[0].Code).IsEqualTo(ErrorCode.FillBackwardNonSequentialMonths);
    }

    [Test]
    public async Task FillBackward_Should_Return_Success_When_Single_December_Entry()
    {
        // Arrange
        var entries = new[] { (MonthsOfYear.December, 50_000m) };

        // Act
        var result = MonthlyInput.FillBackward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).Count().IsEqualTo(12);
        await Assert.That(result.Value.All(m => m.Salary == 50_000m)).IsTrue();
    }

    [Test]
    public async Task FillBackward_Should_Fill_Gaps_Backward_When_Sparse_Entries_Provided()
    {
        // Arrange - Know July and December values, fill backward
        var entries = new[]
        {
            (MonthsOfYear.July, 40_000m),
            (MonthsOfYear.December, 50_000m),
        };

        // Act
        var result = MonthlyInput.FillBackward(entries);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).Count().IsEqualTo(12);

        // Jan-Jun should be filled backward from July (40,000)
        await Assert.That(result.Value[0].Salary).IsEqualTo(40_000m); // Jan
        await Assert.That(result.Value[5].Salary).IsEqualTo(40_000m); // Jun

        // Jul should be 40,000
        await Assert.That(result.Value[6].Salary).IsEqualTo(40_000m); // Jul

        // Aug-Dec should be filled backward from December (50,000)
        await Assert.That(result.Value[7].Salary).IsEqualTo(50_000m);  // Aug
        await Assert.That(result.Value[11].Salary).IsEqualTo(50_000m); // Dec
    }

    // ═══════════════════════════════════════════════════════════════
    // Uniform Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Uniform_Should_Return_12_Months_With_Same_Salary()
    {
        // Arrange & Act
        var result = MonthlyInput.Uniform(45_000m);

        // Assert
        await Assert.That(result).Count().IsEqualTo(12);
        await Assert.That(result.All(m => m.Salary == 45_000m)).IsTrue();
    }

    [Test]
    public async Task Uniform_Should_Use_Default_30_WorkedDays()
    {
        // Arrange & Act
        var result = MonthlyInput.Uniform(45_000m);

        // Assert
        await Assert.That(result.All(m => m.WorkedDays == 30)).IsTrue();
    }

    [Test]
    public async Task Uniform_Should_Use_Default_Zero_RnDDays()
    {
        // Arrange & Act
        var result = MonthlyInput.Uniform(45_000m);

        // Assert
        await Assert.That(result.All(m => m.RnDDays == 0)).IsTrue();
    }

    [Test]
    public async Task Uniform_Should_Apply_Custom_WorkedDays_And_RnDDays()
    {
        // Arrange & Act
        var result = MonthlyInput.Uniform(45_000m, workedDays: 25, rndDays: 20);

        // Assert
        await Assert.That(result.All(m => m.WorkedDays == 25)).IsTrue();
        await Assert.That(result.All(m => m.RnDDays == 20)).IsTrue();
    }

    [Test]
    public async Task Uniform_Should_Have_Correct_Month_Sequence()
    {
        // Arrange & Act
        var result = MonthlyInput.Uniform(45_000m);

        // Assert
        var allMonths = MonthsOfYear.AllMonths;
        for (var i = 0; i < 12; i++)
        {
            await Assert.That(result[i].Month).IsEqualTo(allMonths[i]);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Constructor_Should_Set_All_Properties()
    {
        // Arrange & Act
        var input = new MonthlyInput(MonthsOfYear.March, 50_000m, 28, 15);

        // Assert
        await Assert.That(input.Month).IsEqualTo(MonthsOfYear.March);
        await Assert.That(input.Salary).IsEqualTo(50_000m);
        await Assert.That(input.WorkedDays).IsEqualTo(28);
        await Assert.That(input.RnDDays).IsEqualTo(15);
    }

    [Test]
    public async Task Constructor_Should_Use_Default_WorkedDays_When_Not_Specified()
    {
        // Arrange & Act
        var input = new MonthlyInput(MonthsOfYear.January, 30_000m);

        // Assert
        await Assert.That(input.WorkedDays).IsEqualTo(30);
        await Assert.That(input.RnDDays).IsEqualTo(0);
    }
}
