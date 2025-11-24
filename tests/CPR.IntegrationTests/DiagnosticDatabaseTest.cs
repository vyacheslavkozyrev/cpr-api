using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CPR.IntegrationTests;

[Collection("SequentialIntegrationTestCollection")]
public class DiagnosticDatabaseTest : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseCleanupFixture>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public DiagnosticDatabaseTest(CustomWebApplicationFactory factory, DatabaseCleanupFixture dbFixture, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task CheckDatabaseState()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

        var johnUserId = Guid.Parse("679add6e-6c29-4e00-b6a5-b69c8e0f3445");

        // Check if user exists
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == johnUserId);
        _output.WriteLine($"User {johnUserId}: {(user != null ? $"EXISTS - {user.UserName}" : "NOT FOUND")}");

        // Check if employee with that UserId exists
        var johnEmployee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == johnUserId);
        _output.WriteLine($"Employee with UserId {johnUserId}: {(johnEmployee != null ? $"EXISTS - Id={johnEmployee.Id}, UserId={johnEmployee.UserId}" : "NOT FOUND")}");

        // Check total employees
        var totalEmployees = await db.Employees.Where(e => !e.IsDeleted).CountAsync();
        _output.WriteLine($"Total active employees: {totalEmployees}");

        // Get first 5 employees
        var employees = await db.Employees.Where(e => !e.IsDeleted).Take(5).ToListAsync();
        foreach (var emp in employees)
        {
            _output.WriteLine($"  Employee Id={emp.Id}, UserId={emp.UserId}");
        }

        // Check employees excluding John's UserId
        var availableEmployees = await db.Employees
            .Where(e => !e.IsDeleted && e.UserId != johnUserId)
            .Take(5)
            .ToListAsync();
        _output.WriteLine($"Employees excluding UserId {johnUserId}: {availableEmployees.Count}");
        foreach (var emp in availableEmployees)
        {
            _output.WriteLine($"  Available Employee Id={emp.Id}, UserId={emp.UserId}");
        }
    }
}
