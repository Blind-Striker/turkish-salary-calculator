#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Providers;

public class EmbeddedCalculationConstantsProviderTests
{
    [Test]
    public async Task Constants_Should_Return_Loaded_Constants()
    {
        // Arrange
        var provider = new EmbeddedCalculationConstantsProvider();

        // Act
        var constants = provider.Constants;

        // Assert
        await Assert.That(constants).IsNotNull();
        await Assert.That(provider.AllEmployeeTypes).IsNotEmpty();
        await Assert.That(provider.AllEducationTypes).IsNotEmpty();
    }
}
