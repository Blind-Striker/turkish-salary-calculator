using System.Globalization;
using System.IO.Abstractions;
using Spectre.Console;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure;

#pragma warning disable S1075,S1481,S125, IDE0059

var turkishCulture = CultureInfo.CreateSpecificCulture("tr-TR");

// var parsed = decimal.TryParse(args[0], NumberStyles.Number, turkishCulture, out var salary);
const decimal salary = 27_000m;

var fileSystem = new FileSystem();
var parameterProvider = new JsonParameterProvider(fileSystem);

var currentDirectory = fileSystem.Directory.GetCurrentDirectory();

var yearConstants = fileSystem.Path.Combine(currentDirectory, "Constants", "year-constants.json");
var calcConstants = fileSystem.Path.Combine(currentDirectory, "Constants", "calculation-constants.json");

var yearParameters = await parameterProvider.LoadYearParametersAsync(yearConstants);
var fixtures = await parameterProvider.LoadFixtureAsync(calcConstants);

var monthlySalaries = new List<MonthlySalary>()
{
    new(MonthsOfYear.January, salary),
    new(MonthsOfYear.February, salary),
    new(MonthsOfYear.March, salary),
    new(MonthsOfYear.April, salary),
    new(MonthsOfYear.May, salary),
    new(MonthsOfYear.June, salary),
    new(MonthsOfYear.July, salary),
    new(MonthsOfYear.August, salary),
    new(MonthsOfYear.September, salary),
    new(MonthsOfYear.October, salary),
    new(MonthsOfYear.November, salary),
    new(MonthsOfYear.December, salary),
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
