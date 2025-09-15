using CPR.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hellang.Middleware.ProblemDetails;
using CPR.Infrastructure.Data;
using System;
using System.IO;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Reduce noisy framework logging during test runs
// (tests and TestServer pick up this configuration via Program)
// Apply broader Microsoft/System filters to suppress verbose EF Core and ASP.NET logs
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Npgsql", LogLevel.Warning);

// Add minimal services
builder.Services.AddControllers().AddNewtonsoftJson();

// Register stub authentication scheme (reads signing key from env var JWT_SIGNING_KEY)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Stub";
    options.DefaultChallengeScheme = "Stub";
}).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, CPR.Api.Auth.JwtStubAuthenticationHandler>("Stub", options => { });

builder.Services.AddAuthorization();
// Register app services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CPR.Api.Services.IUserService, CPR.Api.Services.UserService>();
// Register Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CPR API",
        Version = "v1",
        Description = "Career progress & feedback API - development",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "CPR Team",
            Email = "devops@example.com",
        }
    });

    // JWT Bearer (placeholder) for Authorize button in Swagger
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });

    // Only apply the security requirement to endpoints decorated with [Authorize]
    options.OperationFilter<CPR.Api.Swagger.AuthorizeCheckOperationFilter>();

    // Include XML comments if generated
    try
    {
        // Include XML comments from all referenced assemblies (so DTO XML in CPR.Application appears)
        var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
        foreach (var xmlPath in xmlFiles)
        {
            try { options.IncludeXmlComments(xmlPath); } catch { /* ignore malformed or unrelated xml */ }
        }
    }
    catch
    {
        // ignore if XML file cannot be found or loaded
    }
    // Register example providers from this assembly (no-op here; AddSwaggerExamplesFromAssemblyOf registers provider services)
});

// register Swashbuckle example provider services
builder.Services.AddSwaggerExamplesFromAssemblyOf<CPR.Api.Swagger.Examples.PositionsExample>();
builder.Services.AddInfrastructure();

var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "cpr_dev";
var defaultConn = $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres";
var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["DATABASE_URL"] ?? defaultConn;

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
});

var app = builder.Build();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPR API v1"));
}

// Use the ProblemDetails middleware so exceptions are mapped to RFC7807 responses
app.UseProblemDetails();

// Test-only endpoints for integration tests that intentionally throw so ProblemDetails
// middleware can be validated. Using MapGet ensures the TestHost routing matches
// the paths reliably.
app.MapGet("/__test/throw/argnull", (Microsoft.AspNetCore.Http.HttpContext _) => throw new ArgumentNullException("dto"));
app.MapGet("/__test/throw/argout", (Microsoft.AspNetCore.Http.HttpContext _) => throw new ArgumentOutOfRangeException("page"));

app.MapGet("/", () => Results.Ok(new { message = "CPR API - running" }));
app.MapControllers();

app.Run();
