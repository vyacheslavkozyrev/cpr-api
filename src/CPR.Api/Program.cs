using CPR.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CPR.Infrastructure.Data;
using System;
using System.IO;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

    // Include XML comments if generated
    try
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
    }
    catch
    {
        // ignore if XML file cannot be found or loaded
    }
});
builder.Services.AddInfrastructure();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["DATABASE_URL"] ?? "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

builder.Services.AddDbContext<CprDbContext>(options =>
    options.UseNpgsql(connectionString)
);

var app = builder.Build();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPR API v1"));
}

app.MapGet("/", () => Results.Ok(new { message = "CPR API - running" }));
app.MapControllers();

app.Run();
