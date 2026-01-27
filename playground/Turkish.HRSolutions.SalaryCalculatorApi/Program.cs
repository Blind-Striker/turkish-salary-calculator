using System.Security.Cryptography;
using Turkish.HRSolutions.SalaryCalculatorApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

app.MapGet("/weatherforecast", () =>
        Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                    RandomNumberGenerator.GetInt32(-20, 55),
                    summaries[RandomNumberGenerator.GetInt32(summaries.Length)]
                ))
            .ToArray())
    .WithName("GetWeatherForecast");

await app.RunAsync().ConfigureAwait(false);

namespace Turkish.HRSolutions.SalaryCalculatorApi
{
    internal sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
