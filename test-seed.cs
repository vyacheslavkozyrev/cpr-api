using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;

class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<CprDbContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres"))
            .BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CprDbContext>();

            // Ensure database is created and seed data is applied
            context.Database.EnsureCreated();

            // Check if seed data exists
            var userCount = context.Users.Count();
            var employeeCount = context.Employees.Count();
            var careerPathCount = context.CareerPaths.Count();

            Console.WriteLine($"Users: {userCount}");
            Console.WriteLine($"Employees: {employeeCount}");
            Console.WriteLine($"Career Paths: {careerPathCount}");

            if (userCount > 0)
            {
                Console.WriteLine("Seed data found!");
                var firstUser = context.Users.First();
                Console.WriteLine($"First user: {firstUser.UserName} - {firstUser.DisplayName}");
            }
            else
            {
                Console.WriteLine("No seed data found!");
            }
        }
    }
}