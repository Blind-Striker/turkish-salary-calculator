using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Domain.ValueObjects;

public class MonthsOfYearTests
{
    // ═══════════════════════════════════════════════════════════════
    // Static Members Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task January_Should_Have_Number_1()
    {
        // Assert
        await Assert.That((int)MonthsOfYear.January.Number).IsEqualTo(1);
        await Assert.That(MonthsOfYear.January.Name).IsEqualTo("January");
        await Assert.That(MonthsOfYear.January.Season).IsEqualTo("Winter");
    }

    [Test]
    public async Task December_Should_Have_Number_12()
    {
        // Assert
        await Assert.That((int)MonthsOfYear.December.Number).IsEqualTo(12);
        await Assert.That(MonthsOfYear.December.Name).IsEqualTo("December");
        await Assert.That(MonthsOfYear.December.Season).IsEqualTo("Winter");
    }

    [Test]
    public async Task AllMonths_Should_Contain_12_Months()
    {
        // Assert
        await Assert.That(MonthsOfYear.AllMonths).Count().IsEqualTo(12);
    }

    [Test]
    public async Task AllMonths_Should_Be_In_Order()
    {
        // Arrange
        var allMonths = MonthsOfYear.AllMonths;

        // Assert
        for (var i = 0; i < 12; i++)
        {
            await Assert.That((int)allMonths[i].Number).IsEqualTo(i + 1);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Comparison Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task LessThan_Should_Return_True_When_Earlier_Month()
    {
        // Assert
        await Assert.That(MonthsOfYear.January < MonthsOfYear.February).IsTrue();
        await Assert.That(MonthsOfYear.June < MonthsOfYear.December).IsTrue();
    }

    [Test]
    public async Task LessThan_Should_Return_False_When_Later_Month()
    {
        // Assert
        await Assert.That(MonthsOfYear.December < MonthsOfYear.January).IsFalse();
    }

    [Test]
    public async Task GreaterThan_Should_Return_True_When_Later_Month()
    {
        // Assert
        await Assert.That(MonthsOfYear.December > MonthsOfYear.January).IsTrue();
        await Assert.That(MonthsOfYear.July > MonthsOfYear.March).IsTrue();
    }

    [Test]
    public async Task LessThanOrEqual_Should_Return_True_When_Same_Month()
    {
        // Arrange
        var june1 = MonthsOfYear.June;
        var june2 = MonthsOfYear.AllMonths[5]; // Also June

        // Assert
        await Assert.That(june1 <= june2).IsTrue();
    }

    [Test]
    public async Task GreaterThanOrEqual_Should_Return_True_When_Same_Month()
    {
        // Arrange
        var june1 = MonthsOfYear.June;
        var june2 = MonthsOfYear.AllMonths[5]; // Also June

        // Assert
        await Assert.That(june1 >= june2).IsTrue();
    }

    [Test]
    public async Task CompareTo_Should_Return_Negative_When_Earlier()
    {
        // Act
        var result = MonthsOfYear.January.CompareTo(MonthsOfYear.December);

        // Assert
        await Assert.That(result).IsLessThan(0);
    }

    [Test]
    public async Task CompareTo_Should_Return_Positive_When_Later()
    {
        // Act
        var result = MonthsOfYear.December.CompareTo(MonthsOfYear.January);

        // Assert
        await Assert.That(result).IsGreaterThan(0);
    }

    [Test]
    public async Task CompareTo_Should_Return_Zero_When_Same()
    {
        // Act
        var result = MonthsOfYear.July.CompareTo(MonthsOfYear.July);

        // Assert
        await Assert.That(result).IsEqualTo(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // Equality Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Equals_Should_Return_True_For_Same_Month()
    {
        // Arrange
        var jan1 = MonthsOfYear.January;
        var jan2 = MonthsOfYear.AllMonths[0]; // Also January

        // Assert
        await Assert.That(jan1 == jan2).IsTrue();
        await Assert.That(jan1.Equals(jan2)).IsTrue();
    }

    [Test]
    public async Task Equals_Should_Return_False_For_Different_Months()
    {
        // Assert
        await Assert.That(MonthsOfYear.January == MonthsOfYear.February).IsFalse();
        await Assert.That(MonthsOfYear.January != MonthsOfYear.February).IsTrue();
    }

    [Test]
    public async Task GetHashCode_Should_Be_Same_For_Same_Month()
    {
        // Arrange
        var month1 = MonthsOfYear.March;
        var month2 = MonthsOfYear.AllMonths[2]; // March

        // Assert
        await Assert.That(month1.GetHashCode()).IsEqualTo(month2.GetHashCode());
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToString_Should_Return_Month_Name()
    {
        // Assert
        await Assert.That(MonthsOfYear.January.ToString()).IsEqualTo("January");
        await Assert.That(MonthsOfYear.July.ToString()).IsEqualTo("July");
        await Assert.That(MonthsOfYear.December.ToString()).IsEqualTo("December");
    }

    // ═══════════════════════════════════════════════════════════════
    // IsDefault Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task IsDefault_Should_Return_False_For_Valid_Months()
    {
        // Assert
        await Assert.That(MonthsOfYear.January.IsDefault).IsFalse();
        await Assert.That(MonthsOfYear.December.IsDefault).IsFalse();
    }

    [Test]
    public async Task IsDefault_Should_Return_True_For_Default_Struct()
    {
        // Arrange
        var defaultMonth = default(MonthsOfYear);

        // Assert
        await Assert.That(defaultMonth.IsDefault).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // Season Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task WinterMonths_Should_Have_Winter_Season()
    {
        // Assert
        await Assert.That(MonthsOfYear.December.Season).IsEqualTo("Winter");
        await Assert.That(MonthsOfYear.January.Season).IsEqualTo("Winter");
        await Assert.That(MonthsOfYear.February.Season).IsEqualTo("Winter");
    }

    [Test]
    public async Task SpringMonths_Should_Have_Spring_Season()
    {
        // Assert
        await Assert.That(MonthsOfYear.March.Season).IsEqualTo("Spring");
        await Assert.That(MonthsOfYear.April.Season).IsEqualTo("Spring");
        await Assert.That(MonthsOfYear.May.Season).IsEqualTo("Spring");
    }

    [Test]
    public async Task SummerMonths_Should_Have_Summer_Season()
    {
        // Assert
        await Assert.That(MonthsOfYear.June.Season).IsEqualTo("Summer");
        await Assert.That(MonthsOfYear.July.Season).IsEqualTo("Summer");
        await Assert.That(MonthsOfYear.August.Season).IsEqualTo("Summer");
    }

    [Test]
    public async Task AutumnMonths_Should_Have_Autumn_Season()
    {
        // Assert
        await Assert.That(MonthsOfYear.September.Season).IsEqualTo("Autumn");
        await Assert.That(MonthsOfYear.October.Season).IsEqualTo("Autumn");
        await Assert.That(MonthsOfYear.November.Season).IsEqualTo("Autumn");
    }
}
