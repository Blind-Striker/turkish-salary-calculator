namespace TurkHRSolutions.TurkEmployCalc.Tests.ParameterService;

public class ParameterServiceIntegrationTests
{
    private readonly FileSystem _fileSystem = new();

    [Fact]
    public void Load_Parameters_Should_Return_Turk_Employ_Calc_Parameters_From_Json_File()
    {
        var jsonFile = _fileSystem.FileInfo.New("./example_parameters.json");
        jsonFile.Exists.Should().BeTrue();

        var parameterService = new Services.Parameter.ParameterService(_fileSystem);

        var turkEmployCalcParameters = parameterService.LoadParameters(jsonFile.FullName);

        turkEmployCalcParameters.Should().NotBeNull();
    }
}
