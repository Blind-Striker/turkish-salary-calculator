using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.DependencyInjection;
using Turkish.HRSolutions.SalaryCalculatorApi;
using Turkish.HRSolutions.SalaryCalculatorApi.Contracts;
using Turkish.HRSolutions.SalaryCalculatorApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════════
// Service Registration
// ═══════════════════════════════════════════════════════════════════════════════
builder.AddServiceDefaults();
builder.Services.AddOpenApi();

// Register salary calculator services
builder.Services.AddSalaryCalculator();

// Configure JSON serialization for AOT safety
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonSerializerContext.Default));

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════════════════════
// Middleware Pipeline
// ═══════════════════════════════════════════════════════════════════════════════

// Global exception handler — converts framework exceptions to RFC 7807 JSON.
// Catches BadHttpRequestException (bad JSON, missing params, binding failures),
// ArgumentNullException (null DTOs), and any other unhandled exceptions.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        var (statusCode, title, detail) = exception switch
        {
            BadHttpRequestException bad => (StatusCodes.Status400BadRequest, "Bad Request", bad.Message),
            ArgumentNullException arg => (StatusCodes.Status400BadRequest, "Bad Request", $"A required value was missing: {arg.ParamName}"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred."),
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var errorResponse = new ErrorResponse(
            Type: statusCode == 400
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                : "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title: title,
            Status: statusCode,
            Detail: detail);

        await context.Response.WriteAsJsonAsync(errorResponse, ApiJsonSerializerContext.Default.ErrorResponse).ConfigureAwait(false);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Scalar UI at /scalar/v1
}

app.UseHttpsRedirection();

// ═══════════════════════════════════════════════════════════════════════════════
// API Routes - Version 1
// ═══════════════════════════════════════════════════════════════════════════════
var v1 = app.MapGroup("/api/v1");

// ─────────────────────────────────────────────────────────────────────────────────
// Metadata Endpoints
// ─────────────────────────────────────────────────────────────────────────────────

v1.MapGet("/meta/years", (ISalaryCalculatorMetadata metadata) =>
{
    var years = metadata.GetAvailableYears();
    return Results.Ok(new AvailableYearsResponse(years));
})
.WithName("GetAvailableYears")
.WithTags("Metadata")
.WithSummary("Get available calculation years")
.WithDescription("Returns a list of years that have calculation parameters configured.");

v1.MapGet("/meta/employee-types", (ISalaryCalculatorMetadata metadata) =>
{
    var types = metadata.GetEmployeeTypes();
    return Results.Ok(new EmployeeTypesResponse(types));
})
.WithName("GetEmployeeTypes")
.WithTags("Metadata")
.WithSummary("Get employee types")
.WithDescription("Returns all employee types with IDs, names (Turkish), and descriptions.");

v1.MapGet("/meta/education-types", (ISalaryCalculatorMetadata metadata) =>
{
    var types = metadata.GetEducationTypes();
    return Results.Ok(new EducationTypesResponse(types));
})
.WithName("GetEducationTypes")
.WithTags("Metadata")
.WithSummary("Get education types")
.WithDescription("Returns education types for R&D exemption calculations (Law 5746).");

v1.MapGet("/meta/disability-degrees", (ISalaryCalculatorMetadata metadata) =>
{
    var degrees = metadata.GetDisabilityDegrees();
    return Results.Ok(new DisabilityDegreesResponse(degrees));
})
.WithName("GetDisabilityDegrees")
.WithTags("Metadata")
.WithSummary("Get disability degrees")
.WithDescription("Returns disability degree options for tax exemption calculations.");

v1.MapGet("/meta/capabilities", (
    ISalaryCalculatorMetadata metadata,
    int year,
    int? employeeTypeId = null,
    bool? isPensioner = null) =>
{
    var employeeType = employeeTypeId.HasValue
        ? EmployeeTypeId.FromId(employeeTypeId.Value)
        : (EmployeeTypeId?)null;

    var result = metadata.GetCapabilities(year, employeeType, isPensioner ?? false);
    return result.ToCapabilitiesHttpResult();
})
.WithName("GetCapabilities")
.WithTags("Metadata")
.WithSummary("Get available capabilities")
.WithDescription("Returns which features are enabled/disabled for a given year, employee type, and pensioner status. Use this to dynamically configure UI controls.");

// ─────────────────────────────────────────────────────────────────────────────────
// Calculation Endpoints
// ─────────────────────────────────────────────────────────────────────────────────

v1.MapPost("/calculate", (
    CalculationRequestDto request,
    ISalaryCalculator calculator) =>
{
    // Map API DTO to library request
    var mapResult = request.ToLibraryRequest();
    if (mapResult.IsFailure)
    {
        return mapResult.ToHttpResult(); // Returns 400 with error details
    }

    // Perform calculation
    var calcResult = calculator.Calculate(mapResult.Value);
    return calcResult.ToHttpResult();
})
.WithName("Calculate")
.WithTags("Calculation")
.WithSummary("Calculate salary")
.WithDescription("Performs salary calculation with full monthly breakdown. Supports gross-to-net, net-to-gross, and total-to-gross modes.");

v1.MapPost("/calculate/uniform", (
    UniformCalculationRequestDto request,
    ISalaryCalculator calculator) =>
{
    // Map API DTO to library request (creates 12 identical months)
    var mapResult = request.ToLibraryRequest();
    if (mapResult.IsFailure)
    {
        return mapResult.ToHttpResult(); // Returns 400 with error details
    }

    // Perform calculation
    var calcResult = calculator.Calculate(mapResult.Value);
    return calcResult.ToHttpResult();
})
.WithName("CalculateUniform")
.WithTags("Calculation")
.WithSummary("Calculate salary (uniform)")
.WithDescription("Shorthand for calculating with the same salary for all 12 months. Useful for quick calculations.");

await app.RunAsync().ConfigureAwait(false);
