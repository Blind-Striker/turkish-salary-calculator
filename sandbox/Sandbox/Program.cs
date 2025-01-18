using System.Globalization;
using System.IO.Abstractions;
using Spectre.Console;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure;

#pragma warning disable S1075,S1481,S125, IDE0059

var parameterProvider = new JsonParameterProvider(new FileSystem());

var yearParameters = await parameterProvider
    .LoadYearParametersAsync(@"E:\repos\my-projects\turkish-employee-financial-utils\assets\year-parameters.json");

var fixtures = await parameterProvider
    .LoadFixtureAsync(@"E:\repos\my-projects\turkish-employee-financial-utils\assets\fixtures.json");

var monthlySalaries = new List<MonthlySalary>()
{
    new(MonthsOfYear.January, 140_831),
    new(MonthsOfYear.February, 140_831),
    new(MonthsOfYear.March, 140_831),
    new(MonthsOfYear.April, 140_831),
    new(MonthsOfYear.May, 140_831),
    new(MonthsOfYear.June, 140_831),
    new(MonthsOfYear.July, 140_831),
    new(MonthsOfYear.August, 140_831),
    new(MonthsOfYear.September, 140_831),
    new(MonthsOfYear.October, 140_831),
    new(MonthsOfYear.November, 140_831),
    new(MonthsOfYear.December, 140_831),
};

var calculateSalaryRequest = new CalculateSalaryRequest(2025, monthlySalaries);

var service = new SalaryCalculationService(yearParameters.Value, fixtures.Value);
var yearCalculationModel = service.CalculateSalary(calculateSalaryRequest);

var table = new Table();

table.AddColumn("Ay");
table.AddColumn("Gün Sayısı");
table.AddColumn("Ar-Ge Gün Sayısı");
table.AddColumn("Bordroya Esas Brüt");
table.AddColumn("Çalışan SGK Primi");
table.AddColumn("Çalışan SGK Primi İstinası");
table.AddColumn("Çalışan İşsizlik Sigortası");
table.AddColumn("Çalışan İşsizlik Sigortası İstinası");
// table.AddColumn("Vergi Dilimi");
table.AddColumn("Gelir Vergisi");
table.AddColumn("Gelir Vergisi İstinası");
table.AddColumn("Damga Vergisi");
table.AddColumn("Damga Vergisi İstinası");
// table.AddColumn("Asgari Ücret Vergi İstisnası");
table.AddColumn("Net Ücret");
table.AddColumn("Maaş");

var turkishCulture = CultureInfo.CreateSpecificCulture("tr-TR");

foreach (var month in yearCalculationModel.Months)
{
    _ = table.AddRow(
        "1", // Ay
        month.WorkedDays.ToString("N", turkishCulture), // Gün Sayısı
        month.ResearchAndDevelopmentWorkedDays.ToString("N", turkishCulture), // Ar-Ge Gün Sayısı
        month.CalculatedGrossSalary.ToString("N2", turkishCulture), // Bordroya Esas Brüt
        month.EmployeeSgkDeduction.ToString("N2", turkishCulture), // Çalışan SGK Primi
        month.EmployeeSgkExemption.ToString("N2", turkishCulture), // Çalışan SGK Primi İstinası
        month.EmployeeUnemploymentInsuranceDeduction.ToString("N2", turkishCulture), // Çalışan İşsizlik Sigortası
        month.EmployeeUnemploymentInsuranceExemption.ToString("N2", turkishCulture), // Çalışan İşsizlik Sigortası İstinası
        month.EmployeeIncomeTax.ToString("N2", turkishCulture), // Gelir Vergisi
        month.EmployerIncomeTaxExemptionAmount.ToString("N2", turkishCulture), // Gelir Vergisi İstinası
        month.StampTax.ToString("N2", turkishCulture), // Damga Vergisi
        month.TotalStampTaxExemption.ToString("N2", turkishCulture), // Damga Vergisi İstinası
        month.NetSalary.ToString("N2", turkishCulture), //  Net Ücret
        month.FinalNetSalary.ToString("N2", turkishCulture)); // Maaş
}

AnsiConsole.Write(table);

// var parameterService = new ParameterService();
//
// var path = @"E:\repos\my-projects\turkish-employee-financial-utils\assets\example_parameters.json";
// var parameters = parameterService.LoadParameters(path);
//
// var salaryService = new SalaryService(parameters);
// var monthlySalaries = new List<MonthlySalary>()
// {
//     new(MonthsOfYear.January, 140_831),
//     new(MonthsOfYear.February, 140_831),
//     new(MonthsOfYear.March, 140_831),
//     new(MonthsOfYear.April, 140_831),
//     new(MonthsOfYear.May, 140_831),
//     new(MonthsOfYear.June, 140_831),
//     new(MonthsOfYear.July, 140_831),
//     new(MonthsOfYear.August, 140_831),
//     new(MonthsOfYear.September, 140_831),
//     new(MonthsOfYear.October, 140_831),
//     new(MonthsOfYear.November, 140_831),
//     new(MonthsOfYear.December, 140_831),
// };
//
// var request = new CalculateSalaryRequest(2025, "SC", "DARP", "ED0", monthlySalaries);
//
// // var grossToNetYearlyCalcModel = salaryService.CalculateGrossToNetMonthly(request);
// //
// // grossToNetYearlyCalcModel.Calculate();
//
// var netToGrossYearlyCalcModel = salaryService.CalculateNetToGrossMonthly(request);
// netToGrossYearlyCalcModel.IsAgiCalculationEnabled = true;
// netToGrossYearlyCalcModel.IsAgiIncludedTax = true;
// netToGrossYearlyCalcModel.ApplyMinWageTaxExemption = true;
// netToGrossYearlyCalcModel.Calculate();
//
// Console.ReadLine();
