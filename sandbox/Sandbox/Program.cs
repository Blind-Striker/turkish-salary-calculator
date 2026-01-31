using System.Globalization;
using Spectre.Console;
using Turkish.HRSolutions.SalaryCalculator;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

#pragma warning disable S1075,S1481,S125, IDE0059

var turkishCulture = CultureInfo.CreateSpecificCulture("tr-TR");

const decimal salary = 27_000m;

// Use V2 API with public static entry point
var calculator = SalaryCalculatorBuilder.Create();

var request = GrossToNetRequest.For(year: 2026)
    .WithMonths(MonthlyInput.Uniform(salary))
    .WithEmployeeType(EmployeeTypeId.Standard)
    .Build();

var result = calculator.Calculate(request);

if (result.IsFailure)
{
    AnsiConsole.MarkupLine("[red]Calculation failed:[/]");
    foreach (var error in result.Errors)
    {
        AnsiConsole.MarkupLine($"  [red]{error.Code}: {error.Message}[/]");
    }

    return;
}

var yearCalculationModel = result.Value;

var table = new Table();

table.AddColumn("Ay");
table.AddColumn("Gün Sayısı");
table.AddColumn("Ar-Ge Gün Sayısı");
table.AddColumn("Bordroya Esas Brüt");
table.AddColumn("Çalışan SGK Primi");
table.AddColumn("Çalışan SGK Primi İstinası");
table.AddColumn("Çalışan İşsizlik Sigortası");
table.AddColumn("Çalışan İşsizlik Sigortası İstinası");
table.AddColumn("Gelir Vergisi");
table.AddColumn("Gelir Vergisi İstinası");
table.AddColumn("Damga Vergisi");
table.AddColumn("Damga Vergisi İstinası");
table.AddColumn("Net Ücret");
table.AddColumn("Maaş");

foreach (var month in yearCalculationModel.MonthlyBreakdowns)
{
    _ = table.AddRow(
        month.Month.Number.ToString(turkishCulture), // Ay
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
