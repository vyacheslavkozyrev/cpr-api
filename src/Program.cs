using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CPR.Infrastructure.Data;
using CPR.Api;

var builder = WebApplication.CreateBuilder(args);

// Add minimal services
builder.Services.AddControllers();
builder.Services.AddInfrastructure();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["DATABASE_URL"] ?? "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

builder.Services.AddDbContext<CprDbContext>(options =>
    options.UseNpgsql(connectionString)
);
// Configure ProblemDetails (Hellang middleware) - register before building the app
builder.Services.AddProblemDetails(options =>
{
    // Map ArgumentNullException to 400
    options.Map<ArgumentNullException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Title = "Missing required value",
        Detail = ex.Message,
        Status = StatusCodes.Status400BadRequest
    });

    // Map ArgumentOutOfRangeException to 400
    options.Map<ArgumentOutOfRangeException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Title = "Invalid argument",
        Detail = ex.Message,
        Status = StatusCodes.Status400BadRequest
    });

    // Let the middleware handle other exceptions as 500 with ProblemDetails
});

var app = builder.Build();

app.UseProblemDetails();

app.MapGet("/", () => Results.Ok(new { message = "CPR API - running" }));
// Test-only middleware: if the test runner requests these paths, throw exceptions so
// ProblemDetails middleware can be validated reliably in integration tests.
// Test-only endpoints for integration tests that intentionally throw so ProblemDetails
// middleware can be validated. Using MapGet ensures the TestHost routing matches
// the paths reliably (previous PathString.Equals check was returning 404).
app.MapGet("/__test/throw/argnull", () => throw new ArgumentNullException("dto"));
app.MapGet("/__test/throw/argout", () => throw new ArgumentOutOfRangeException("page"));
app.MapControllers();

app.Run();
