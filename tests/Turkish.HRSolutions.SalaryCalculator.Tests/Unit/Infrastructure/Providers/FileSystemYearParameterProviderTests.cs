using System.IO.Abstractions.TestingHelpers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Providers;

public class FileSystemYearParameterProviderTests
{
    private const string ValidJson = """
                                     {
                                       "yearParameters": [
                                         {
                                           "year": 2026,
                                           "minGrossWages": [
                                             { "startMonth": 1, "amount": 30000.0, "SGKCeil": 270000.0 }
                                           ],
                                           "minWageEmployeeTaxExemption": true,
                                           "taxSlices": [
                                             { "rate": 0.15, "ceil": 190000 },
                                             { "rate": 0.2, "ceil": null }
                                           ],
                                           "disabledMonthlyIncomeTaxDiscountBases": []
                                         }
                                       ]
                                     }
                                     """;

    [Test]
    public async Task GetParameter_Should_ReturnParameter_When_FileExistsAndIsValid()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("years.json", new MockFileData(ValidJson));

        var provider = new FileSystemYearParameterProvider("years.json", mockFs);

        // Act
        var param = provider.GetParameter(2026);

        // Assert
        await Assert.That(param).IsNotNull();
        await Assert.That(param!.Year).IsEqualTo(2026);
    }

    [Test]
    public async Task Constructor_Should_ThrowArgumentException_When_PathIsInvalid()
    {
        await Assert.That(() => new FileSystemYearParameterProvider("")).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_Should_ThrowArgumentNullException_When_FileSystemIsNull()
    {
        await Assert.That(() => new FileSystemYearParameterProvider("test.json", null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetParameter_Should_ReturnNull_When_FileDoesNotExist()
    {
        // Arrange
        var mockFs = new MockFileSystem(); // Empty fs to simulate missing file
        var provider = new FileSystemYearParameterProvider("missing.json", mockFs);

        // Act
        var param = provider.GetParameter(2026);

        // Assert
        await Assert.That(param).IsNull();
    }
}
