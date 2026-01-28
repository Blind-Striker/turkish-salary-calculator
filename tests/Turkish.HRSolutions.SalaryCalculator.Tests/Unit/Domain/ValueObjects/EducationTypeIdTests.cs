using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Domain.ValueObjects;

public class EducationTypeIdTests
{
    // ═══════════════════════════════════════════════════════════════
    // Static Members Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task OtherRnDPersonnel_Should_Have_Value_1()
    {
        // Assert
        await Assert.That(EducationTypeId.OtherRnDPersonnel.Value).IsEqualTo(1);
    }

    [Test]
    public async Task Doctorate_Should_Have_Value_2()
    {
        // Assert
        await Assert.That(EducationTypeId.Doctorate.Value).IsEqualTo(2);
    }

    [Test]
    public async Task MastersOrFundamentalSciences_Should_Have_Value_3()
    {
        // Assert
        await Assert.That(EducationTypeId.MastersOrFundamentalSciences.Value).IsEqualTo(3);
    }

    [Test]
    public async Task KnownTypes_Should_Contain_All_3_Types()
    {
        // Assert
        await Assert.That(EducationTypeId.KnownTypes).Count().IsEqualTo(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // Extensibility Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task FromId_Should_Create_Custom_Type()
    {
        // Arrange & Act
        var customType = EducationTypeId.FromId(99);

        // Assert
        await Assert.That(customType.Value).IsEqualTo(99);
    }

    [Test]
    public async Task FromId_Should_Match_Known_Type_When_Same_Value()
    {
        // Arrange & Act
        var fromId = EducationTypeId.FromId(1);

        // Assert
        await Assert.That(fromId).IsEqualTo(EducationTypeId.OtherRnDPersonnel);
    }

    // ═══════════════════════════════════════════════════════════════
    // Implicit Conversion Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ImplicitConversion_Should_Convert_To_Int()
    {
        // Act
        int value = EducationTypeId.Doctorate;

        // Assert
        await Assert.That(value).IsEqualTo(2);
    }

    [Test]
    public async Task ToInt32_Should_Return_Value()
    {
        // Assert
        await Assert.That(EducationTypeId.MastersOrFundamentalSciences.ToInt32()).IsEqualTo(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ToString_Should_Return_Name_For_Known_Types()
    {
        // Assert
        await Assert.That(EducationTypeId.OtherRnDPersonnel.ToString()).IsEqualTo("OtherRnDPersonnel");
        await Assert.That(EducationTypeId.Doctorate.ToString()).IsEqualTo("Doctorate");
        await Assert.That(EducationTypeId.MastersOrFundamentalSciences.ToString()).IsEqualTo("MastersOrFundamentalSciences");
    }

    [Test]
    public async Task ToString_Should_Return_Custom_Format_For_Unknown_Types()
    {
        // Arrange
        var customType = EducationTypeId.FromId(99);

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
        var type1 = EducationTypeId.OtherRnDPersonnel;
        var type2 = EducationTypeId.FromId(1);

        // Assert
        await Assert.That(type1).IsEqualTo(type2);
    }

    [Test]
    public async Task Equals_Should_Return_False_For_Different_Values()
    {
        // Arrange
        var type1 = EducationTypeId.OtherRnDPersonnel;
        var type2 = EducationTypeId.Doctorate;

        // Assert
        await Assert.That(type1).IsNotEqualTo(type2);
    }

    [Test]
    public async Task GetHashCode_Should_Be_Same_For_Equal_Values()
    {
        // Arrange
        var type1 = EducationTypeId.Doctorate;
        var type2 = EducationTypeId.FromId(2);

        // Assert
        await Assert.That(type1.GetHashCode()).IsEqualTo(type2.GetHashCode());
    }
}
