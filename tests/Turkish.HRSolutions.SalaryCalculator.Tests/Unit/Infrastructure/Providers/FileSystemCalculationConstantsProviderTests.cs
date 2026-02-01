#pragma warning disable CA1707

using System.IO.Abstractions.TestingHelpers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Providers;

public class FileSystemCalculationConstantsProviderTests
{
    private const string ValidJson = """
                                     {
                                       "CALCULATION_CONSTANTS": {
                                         "monthDayCount": 30,
                                         "stampTaxRate": 0.00759,
                                         "employee": {
                                           "SGKDeductionRate": 0.14,
                                           "SGDPDeductionRate": 0.075,
                                           "unemploymentInsuranceRate": 0.01,
                                           "pensionerUnemploymentInsuranceRate": 0
                                         },
                                         "employer": {
                                           "SGKDeductionRate": 0.205,
                                           "SGDPDeductionRate": 0.245,
                                           "employerDiscount5746": 0.05,
                                           "unemploymentInsuranceRate": 0.02,
                                           "pensionerUnemploymentInsuranceRate": 0,
                                           "SGK5746AdditionalDiscount": 0.5
                                         }
                                       },
                                       "EMPLOYEE_EDUCATION_TYPES": [],
                                       "DISABILITY_OPTIONS": [],
                                       "EMPLOYEE_TYPES": [],
                                       "AGI_OPTIONS": []
                                     }
                                     """;

    [Test]
    public async Task Constants_Should_Return_Loaded_Constants_When_FileExists()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("constants.json", new MockFileData(ValidJson));

        var provider = new FileSystemCalculationConstantsProvider("constants.json", mockFs);

        // Act
        var constants = provider.Constants;

        // Assert
        await Assert.That(constants).IsNotNull();
    }
}
