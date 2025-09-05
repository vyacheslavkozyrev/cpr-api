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

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { message = "CPR API - running" }));
app.MapControllers();

app.Run();
