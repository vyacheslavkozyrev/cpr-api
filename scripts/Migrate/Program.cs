using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CPR.Infrastructure.Data;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json", optional: true).Build();
var conn = config.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres";

var options = new DbContextOptionsBuilder<CprDbContext>().UseNpgsql(conn).Options;

using var ctx = new CprDbContext(options);
Console.WriteLine("Applying migrations...");
ctx.Database.Migrate();
Console.WriteLine("Migrations applied.");
