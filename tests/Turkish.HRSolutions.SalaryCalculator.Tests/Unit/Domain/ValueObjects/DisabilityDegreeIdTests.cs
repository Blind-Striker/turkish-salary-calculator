using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Domain.ValueObjects;

public class DisabilityDegreeIdTests
{
    // ═══════════════════════════════════════════════════════════════
    // Static Members Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task None_Should_Have_Value_Minus1()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.None.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task First_Should_Have_Value_1()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.First.Value).IsEqualTo(1);
    }

    [Test]
    public async Task Second_Should_Have_Value_2()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.Second.Value).IsEqualTo(2);
    }

    [Test]
    public async Task Third_Should_Have_Value_3()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.Third.Value).IsEqualTo(3);
    }

    [Test]
    public async Task KnownDegrees_Should_Contain_All_4_Options()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.KnownDegrees).Count().IsEqualTo(4);
    }

    // ═══════════════════════════════════════════════════════════════
    // HasDisability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task HasDisability_Should_Return_False_For_None()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.None.HasDisability).IsFalse();
    }

    [Test]
    public async Task HasDisability_Should_Return_True_For_First()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.First.HasDisability).IsTrue();
    }

    [Test]
    public async Task HasDisability_Should_Return_True_For_Second()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.Second.HasDisability).IsTrue();
    }

    [Test]
    public async Task HasDisability_Should_Return_True_For_Third()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.Third.HasDisability).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // Extensibility Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FromDegree_Should_Create_Custom_Degree()
    {
        // Arrange & Act
        var customDegree = DisabilityDegreeId.FromDegree(4);

        // Assert
        await Assert.That(customDegree.Value).IsEqualTo(4);
        await Assert.That(customDegree.HasDisability).IsTrue();
    }

    [Test]
    public async Task FromDegree_Should_Match_Known_Degree_When_Same_Value()
    {
        // Arrange & Act
        var fromDegree = DisabilityDegreeId.FromDegree(1);

        // Assert
        await Assert.That(fromDegree).IsEqualTo(DisabilityDegreeId.First);
    }

    // ═══════════════════════════════════════════════════════════════
    // Implicit Conversion Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ImplicitConversion_Should_Convert_To_Int()
    {
        // Act
        int value = DisabilityDegreeId.Second;

        // Assert
        await Assert.That(value).IsEqualTo(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToString_Should_Return_Name_For_Known_Degrees()
    {
        // Assert
        await Assert.That(DisabilityDegreeId.None.ToString()).IsEqualTo("None");
        await Assert.That(DisabilityDegreeId.First.ToString()).IsEqualTo("First");
        await Assert.That(DisabilityDegreeId.Second.ToString()).IsEqualTo("Second");
        await Assert.That(DisabilityDegreeId.Third.ToString()).IsEqualTo("Third");
    }

    [Test]
    public async Task ToString_Should_Return_Custom_Format_For_Unknown_Degrees()
    {
        // Arrange
        var customDegree = DisabilityDegreeId.FromDegree(5);

        // Assert
        await Assert.That(customDegree.ToString()).IsEqualTo("Custom(5)");
    }

    // ═══════════════════════════════════════════════════════════════
    // Equality Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Equals_Should_Return_True_For_Same_Value()
    {
        // Arrange
        var degree1 = DisabilityDegreeId.First;
        var degree2 = DisabilityDegreeId.FromDegree(1);

        // Assert
        await Assert.That(degree1).IsEqualTo(degree2);
    }

    [Test]
    public async Task Equals_Should_Return_False_For_Different_Values()
    {
        // Arrange
        var degree1 = DisabilityDegreeId.First;
        var degree2 = DisabilityDegreeId.Second;

        // Assert
        await Assert.That(degree1).IsNotEqualTo(degree2);
    }
}
