#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Domain.ValueObjects;

public class EmployeeTypeIdTests
{
    // ═══════════════════════════════════════════════════════════════
    // Static Members Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Standard_Should_Have_Value_1()
    {
        // Assert
        await Assert.That(EmployeeTypeId.Standard.Value).IsEqualTo(1);
    }

    [Test]
    public async Task Teknokent4691_Should_Have_Value_2()
    {
        // Assert
        await Assert.That(EmployeeTypeId.Teknokent4691.Value).IsEqualTo(2);
    }

    [Test]
    public async Task RnD5746_Should_Have_Value_3()
    {
        // Assert
        await Assert.That(EmployeeTypeId.RnD5746.Value).IsEqualTo(3);
    }

    [Test]
    public async Task KnownTypes_Should_Contain_All_10_Types()
    {
        // Assert
        await Assert.That(EmployeeTypeId.KnownTypes).Count().IsEqualTo(10);
    }

    // ═══════════════════════════════════════════════════════════════
    // Extensibility Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FromId_Should_Create_Custom_Type()
    {
        // Arrange & Act
        var customType = EmployeeTypeId.FromId(99);

        // Assert
        await Assert.That(customType.Value).IsEqualTo(99);
    }

    [Test]
    public async Task FromId_Should_Match_Known_Type_When_Same_Value()
    {
        // Arrange & Act
        var fromId = EmployeeTypeId.FromId(1);

        // Assert
        await Assert.That(fromId).IsEqualTo(EmployeeTypeId.Standard);
    }

    // ═══════════════════════════════════════════════════════════════
    // Implicit Conversion Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ImplicitConversion_Should_Convert_To_Int()
    {
        // Act
        int value = EmployeeTypeId.Teknokent4691;

        // Assert
        await Assert.That(value).IsEqualTo(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToString_Should_Return_Name_For_Known_Types()
    {
        // Assert
        await Assert.That(EmployeeTypeId.Standard.ToString()).IsEqualTo("Standard");
        await Assert.That(EmployeeTypeId.Teknokent4691.ToString()).IsEqualTo("Teknokent4691");
        await Assert.That(EmployeeTypeId.DomesticServices.ToString()).IsEqualTo("DomesticServices");
    }

    [Test]
    public async Task ToString_Should_Return_Custom_Format_For_Unknown_Types()
    {
        // Arrange
        var customType = EmployeeTypeId.FromId(99);

        // Assert
        await Assert.That(customType.ToString()).IsEqualTo("Custom(99)");
    }

    // ═══════════════════════════════════════════════════════════════
    // Equality Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task Equals_Should_Return_True_For_Same_Value()
    {
        // Arrange
        var type1 = EmployeeTypeId.Standard;
        var type2 = EmployeeTypeId.FromId(1);

        // Assert
        await Assert.That(type1).IsEqualTo(type2);
    }

    [Test]
    public async Task Equals_Should_Return_False_For_Different_Values()
    {
        // Arrange
        var type1 = EmployeeTypeId.Standard;
        var type2 = EmployeeTypeId.Teknokent4691;

        // Assert
        await Assert.That(type1).IsNotEqualTo(type2);
    }

    [Test]
    public async Task GetHashCode_Should_Be_Same_For_Equal_Values()
    {
        // Arrange
        var type1 = EmployeeTypeId.RnD5746;
        var type2 = EmployeeTypeId.FromId(3);

        // Assert
        await Assert.That(type1.GetHashCode()).IsEqualTo(type2.GetHashCode());
    }
}
