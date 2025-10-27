using CPR.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hellang.Middleware.ProblemDetails;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration is automatically loaded from appsettings.json and appsettings.{Environment}.json

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

// CORS: get allowed origins from configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevCors", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Configure authentication based on configuration
var authenticationMode = builder.Configuration["Authentication:Mode"] ?? "Stub";

// Debug output to see what authentication mode is being used
Console.WriteLine($"DEBUG: Authentication mode from configuration: '{authenticationMode}'");
Console.WriteLine($"DEBUG: AzureAd:TenantId: '{builder.Configuration["AzureAd:TenantId"]}'");

if (authenticationMode.Equals("EntraExternalId", StringComparison.OrdinalIgnoreCase))
{
    // Use Microsoft Entra External ID authentication with JWT Bearer validation
    // Get configuration values from appsettings
    var tenantId = builder.Configuration["AzureAd:TenantId"];
    var clientId = builder.Configuration["AzureAd:ClientId"];
    var apiAudience = builder.Configuration["AzureAd:Audience"];

    // Use standard JWT Bearer authentication with complete bypass for testing
    builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Configure token validation parameters to bypass signature validation for development
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,

                // Use SignatureValidator to bypass signature validation - return JsonWebToken for .NET 8+ compatibility
                SignatureValidator = (token, parameters) =>
                {
                    // Parse the token using JsonWebToken for .NET 8+ compatibility
                    return new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token);
                }
            };


        });
}
else
{
    // Use stub authentication (default) - reads signing key from JWT_SIGNING_KEY env var
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Stub";
        options.DefaultChallengeScheme = "Stub";
    }).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, CPR.Api.Auth.JwtStubAuthenticationHandler>("Stub", options => { });
}

builder.Services.AddAuthorization();

// Register custom authorization policy provider and handler
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, CPR.Api.Auth.RoleAuthorizationPolicyProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, CPR.Api.Auth.RoleAuthorizationHandler>();
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

var connectionString = CPR.Infrastructure.Data.DatabaseConnection.GetConnectionString(builder.Configuration);

builder.Services.AddDbContext<CprDbContext>(options =>
    options.UseNpgsql(connectionString)
        .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
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

// Seed the database on startup (but not in Test environment - tests handle their own seeding)
if (!app.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CprDbContext>();

        // Run migrations to ensure schema is up to date
        // Note: Migrate() will automatically create the database if it doesn't exist
        dbContext.Database.Migrate();

        // Seed the database
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}

// Enable Swagger UI in development and test environments
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPR API v1"));
}

// Use the ProblemDetails middleware so exceptions are mapped to RFC7807 responses
app.UseProblemDetails();

// Apply CORS policy for local development before authentication
app.UseCors("LocalDevCors");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Test-only endpoints for integration tests that intentionally throw so ProblemDetails
// middleware can be validated. Using MapGet ensures the TestHost routing matches
// the paths reliably.
app.MapGet("/__test/throw/argnull", (Microsoft.AspNetCore.Http.HttpContext _) => throw new ArgumentNullException("dto"));
app.MapGet("/__test/throw/argout", (Microsoft.AspNetCore.Http.HttpContext _) => throw new ArgumentOutOfRangeException("page"));

app.MapGet("/", () => Results.Ok(new { message = "CPR API - running" }));

// Configure routing with /api prefix for all controllers
app.MapControllers();

app.Run();


