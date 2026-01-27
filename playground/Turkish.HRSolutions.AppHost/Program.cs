var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Turkish_HRSolutions_SalaryCalculatorApi>("salary-calculator-api");

await builder.Build().RunAsync().ConfigureAwait(false);
