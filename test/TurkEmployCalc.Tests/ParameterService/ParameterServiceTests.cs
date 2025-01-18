namespace TurkHRSolutions.TurkEmployCalc.Tests.ParameterService;

public class ParameterServiceTests
{
    [Fact]
    public void Load_Parameters_Should_Throw_Argument_Exception_For_Null_Or_Empty_Path()
    {
        var service = new Services.Parameter.ParameterService();

        service.Invoking(parameterService => parameterService.LoadParameters(string.Empty))
            .Should()
            .Throw<ArgumentException>()
            .And.ParamName.Should().Be("path");
    }

    [Fact]
    public void Load_Parameters_Should_Throw_File_Not_Found_Exception_For_Invalid_Path()
    {
        var service = new Services.Parameter.ParameterService(new MockFileSystem());

        const string invalidPathJson = "invalidPath.json";

        service.Invoking(parameterService => parameterService.LoadParameters(invalidPathJson))
            .Should()
            .Throw<FileNotFoundException>()
            .And.FileName.Should().Be(invalidPathJson);
    }

    [Fact]
    [SuppressMessage("Design", "MA0051:Method is too long")]
    public void Load_Parameters_Should_Return_Turk_Employ_Calc_Parameters_For_Valid_Json()
    {
        const string validPathJson = "validPath.json";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(StringComparer.Ordinal)
            { { validPathJson, new MockFileData(ParameterData.ValidJson) } });

        var service = new Services.Parameter.ParameterService(mockFileSystem);

        var turkEmployCalcParameters = service.LoadParameters(validPathJson);

        turkEmployCalcParameters.Should().NotBeNull();
        turkEmployCalcParameters.YearParameters.Should().NotBeEmpty();
        turkEmployCalcParameters.MinimumLivingAllowanceParameters.Should().NotBeEmpty();
        turkEmployCalcParameters.EmployeeTypeParameters.Should().NotBeEmpty();
        turkEmployCalcParameters.CalculationConstantParameters.Should().NotBeNull();
        turkEmployCalcParameters.EmployeeEducationTypeParameters.Should().NotBeEmpty();
        turkEmployCalcParameters.DisabilityTypeParameters.Should().NotBeEmpty();

        var year2023 = turkEmployCalcParameters.YearParameters.ToList();

        year2023[0].Year.Should().Be(2023);

        year2023[0].MinGrossWages.Should().NotBeEmpty();

        var minGrossWageParameters = year2023[0].MinGrossWages.ToList();
        minGrossWageParameters[0].StartMonth.Should().Be(7);
        minGrossWageParameters[0].Amount.Should().Be(13414.5);
        minGrossWageParameters[0].SgkCeil.Should().Be(100608.9);
        minGrossWageParameters[1].StartMonth.Should().Be(1);
        minGrossWageParameters[1].Amount.Should().Be(10008);
        minGrossWageParameters[1].SgkCeil.Should().Be(75060);

        year2023[0].MinWageEmployeeTaxExemption.Should().BeTrue();

        year2023[0].TaxSlices.Should().NotBeEmpty();

        var taxSliceParameters = year2023[0].TaxSlices.ToList();
        taxSliceParameters[0].Rate.Should().Be(0.15);
        taxSliceParameters[0].Ceil.Should().Be(70000);
        taxSliceParameters[1].Rate.Should().Be(0.2);
        taxSliceParameters[1].Ceil.Should().Be(150000);
        taxSliceParameters[2].Rate.Should().Be(0.27);
        taxSliceParameters[2].Ceil.Should().Be(550000);
        taxSliceParameters[3].Rate.Should().Be(0.35);
        taxSliceParameters[3].Ceil.Should().Be(1900000);
        taxSliceParameters[4].Rate.Should().Be(0.4);
        taxSliceParameters[4].Ceil.Should().BeNull();

        year2023[0].DisabledMonthlyIncomeTaxDiscountBases.Should().NotBeEmpty();

        var disabledMonthlyIncomeTaxDiscountBaseParameter = year2023[0].DisabledMonthlyIncomeTaxDiscountBases.ToList();
        disabledMonthlyIncomeTaxDiscountBaseParameter[0].Degree.Should().Be(1);
        disabledMonthlyIncomeTaxDiscountBaseParameter[0].Amount.Should().Be(4400);
        disabledMonthlyIncomeTaxDiscountBaseParameter[1].Degree.Should().Be(2);
        disabledMonthlyIncomeTaxDiscountBaseParameter[1].Amount.Should().Be(2600);
        disabledMonthlyIncomeTaxDiscountBaseParameter[2].Degree.Should().Be(3);
        disabledMonthlyIncomeTaxDiscountBaseParameter[2].Amount.Should().Be(1100);

        var minimumLivingAllowanceParameters = turkEmployCalcParameters.MinimumLivingAllowanceParameters.ToList();
        minimumLivingAllowanceParameters[0].SpouseWorkStatus.Should().Be(SpouseWorkStatus.Unmarried);
        minimumLivingAllowanceParameters[0].ChildrenCompareType.Should().Be(ChildrenCompareType.Eq);
        minimumLivingAllowanceParameters[0].NumberOfChildren.Should().Be(0);
        minimumLivingAllowanceParameters[0].Rate.Should().Be(0.5);
        minimumLivingAllowanceParameters[1].SpouseWorkStatus.Should().Be(SpouseWorkStatus.Working);
        minimumLivingAllowanceParameters[1].ChildrenCompareType.Should().Be(ChildrenCompareType.Eq);
        minimumLivingAllowanceParameters[1].NumberOfChildren.Should().Be(0);
        minimumLivingAllowanceParameters[1].Rate.Should().Be(0.5);

        var employeeTypeParameters = turkEmployCalcParameters.EmployeeTypeParameters.ToList();
        employeeTypeParameters[0].Code.Should().Be("SC");
        employeeTypeParameters[0].Description.Should().Be("Standart Çalışan");
        employeeTypeParameters[0].SgkApplicable.Should().BeTrue();
        employeeTypeParameters[0].SgkMinWageBasedExemption.Should().BeFalse();
        employeeTypeParameters[0].SgkMinWageBased17103Exemption.Should().BeFalse();
        employeeTypeParameters[0].UnemploymentInsuranceApplicable.Should().BeTrue();
        employeeTypeParameters[0].AgiApplicable.Should().BeTrue();
        employeeTypeParameters[0].IncomeTaxApplicable.Should().BeTrue();
        employeeTypeParameters[0].TaxMinWageBasedExemption.Should().BeFalse();
        employeeTypeParameters[0].ResearchAndDevelopmentTaxExemption.Should().BeFalse();
        employeeTypeParameters[0].StampTaxApplicable.Should().BeTrue();
        employeeTypeParameters[0].EmployerSgkApplicable.Should().BeTrue();
        employeeTypeParameters[0].EmployerUnemploymentInsuranceApplicable.Should().BeTrue();
        employeeTypeParameters[0].EmployerIncomeTaxApplicable.Should().BeTrue();
        employeeTypeParameters[0].EmployerStampTaxApplicable.Should().BeTrue();
        employeeTypeParameters[0].EmployerEducationIncomeTaxExemption.Should().BeFalse();
        employeeTypeParameters[0].EmployerSgkShareTotalExemption.Should().BeFalse();
        employeeTypeParameters[0].EmployerSgkDiscount5746Applicable.Should().BeTrue();
        employeeTypeParameters[0].Employer5746AdditionalDiscountApplicable.Should().BeFalse();

        turkEmployCalcParameters.CalculationConstantParameters.MonthDayCount.Should().Be(30);
        turkEmployCalcParameters.CalculationConstantParameters.StampTaxRate.Should().Be(0.00759);

        var employeeConstants = turkEmployCalcParameters.CalculationConstantParameters.Employee;
        var employerConstants = turkEmployCalcParameters.CalculationConstantParameters.Employer;
        employeeConstants.Should().NotBeNull();
        employerConstants.Should().NotBeNull();

        employeeConstants.SgkDeductionRate.Should().Be(0.14);
        employeeConstants.SgdpDeductionRate.Should().Be(0.075);
        employeeConstants.UnemploymentInsuranceRate.Should().Be(0.01);
        employeeConstants.PensionerUnemploymentInsuranceRate.Should().Be(0);

        employerConstants.SgkDeductionRate.Should().Be(0.205);
        employerConstants.SgdpDeductionRate.Should().Be(0.245);
        employerConstants.EmployerDiscount5746.Should().Be(0.05);
        employerConstants.UnemploymentInsuranceRate.Should().Be(0.02);
        employerConstants.PensionerUnemploymentInsuranceRate.Should().Be(0);
        employerConstants.Sgk5746AdditionalDiscount.Should().Be(0.5);

        var employeeEducationTypes = turkEmployCalcParameters.EmployeeEducationTypeParameters.ToList();
        employeeEducationTypes[0].Code.Should().Be("DARP");
        employeeEducationTypes[0].Description.Should().Be("Diğer AR-GE Personeli");
        employeeEducationTypes[0].ExemptionRate.Should().Be(0.8);

        var disabilityTypes = turkEmployCalcParameters.DisabilityTypeParameters.ToList();
        disabilityTypes[0].Code.Should().Be("ED0");
        disabilityTypes[0].Description.Should().Be("Engelli Değil");
        disabilityTypes[0].Degree.Should().Be(-1);
    }
}