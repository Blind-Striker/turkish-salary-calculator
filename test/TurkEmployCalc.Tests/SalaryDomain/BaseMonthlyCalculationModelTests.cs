using TurkHRSolutions.TurkEmployCalc.Domain.Salary.Base;

namespace TurkHRSolutions.TurkEmployCalc.Tests.SalaryDomain;

public class BaseMonthlyCalculationModelTests
{
    [Fact]
    public void CalcGrossSalary_Should_CalculateCorrectly()
    {
        const double salary = 5000;
        const uint dayCount = 30;
        const uint monthDayCount = 30;

        var grossSalary = BaseMonthlyCalculationModel.CalcGrossSalary(salary, dayCount, monthDayCount);

        grossSalary.Should().Be(5000);
    }
}
