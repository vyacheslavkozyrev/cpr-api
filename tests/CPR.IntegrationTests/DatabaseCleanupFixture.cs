using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using System.IO;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace CPR.IntegrationTests
{
    [CollectionDefinition("IntegrationTestCollection")]
    public class IntegrationTestCollection : ICollectionFixture<DatabaseCleanupFixture>
    {
        // Collection fixture glue - no code here
    }

    public class DatabaseCleanupFixture : IAsyncLifetime
    {
        private string? _connString;
        private static bool _globalSetupCompleted = false;
        private static readonly SemaphoreSlim _setupLock = new SemaphoreSlim(1, 1);

        public string ConnectionString
        {
            get
            {
                if (_connString == null)
                {
                    _connString = CPR.Infrastructure.Data.DatabaseConnection.BuildConnectionString();
                    Console.WriteLine($"DatabaseCleanupFixture: Built connection string: {_connString}");
                }
                return _connString;
            }
        }

        public DatabaseCleanupFixture()
        {
            Console.WriteLine("DatabaseCleanupFixture: Constructor called");
            Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_HOST={Environment.GetEnvironmentVariable("POSTGRES_HOST")}");
            Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_PORT={Environment.GetEnvironmentVariable("POSTGRES_PORT")}");
            Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_DB={Environment.GetEnvironmentVariable("POSTGRES_DB")}");
        }

        public static bool IsGlobalSetupCompleted()
        {
            return _globalSetupCompleted;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("DatabaseCleanupFixture: InitializeAsync starting");

            // Use lock to prevent multiple test collections from seeding simultaneously
            await _setupLock.WaitAsync();
            try
            {
                // Global setup: load .env file, create database, run migrations, and seed data
                if (!_globalSetupCompleted)
                {
                    Console.WriteLine("DatabaseCleanupFixture: Running global setup");
                    await PerformGlobalDatabaseSetup();
                    _globalSetupCompleted = true;
                    Console.WriteLine("DatabaseCleanupFixture: Global setup completed");
                }
                else
                {
                    Console.WriteLine("DatabaseCleanupFixture: Global setup already completed");
                }
            }
            finally
            {
                _setupLock.Release();
            }

            // best-effort cleanup before tests run
            try
            {
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(ConnectionString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                // Remove rows commonly inserted by tests (identified by title/description markers)
                db.Goals.RemoveRange(db.Goals.Where(g => g.Title.StartsWith("Integration test") || g.Description == "verify persistence"));

                // Clean up employee skills created by tests
                var testEmployeeIds = new[]
                {
                    Guid.Parse("33333333-3333-3333-3333-333333333333"), // TestEmployeeId1
                    Guid.Parse("44444444-4444-4444-4444-444444444444"), // TestEmployeeId2
                    Guid.Parse("55555555-5555-5555-5555-555555555555"), // TestEmployeeId3
                    Guid.Parse("66666666-6666-6666-6666-666666666666"), // TestEmployeeId4
                    Guid.Parse("77777777-7777-7777-7777-777777777777"), // TestEmployeeId5
                    Guid.Parse("88888888-8888-8888-8888-888888888888")  // TestEmployeeId6
                };

                db.EmployeeSkills.RemoveRange(db.EmployeeSkills.Where(es => testEmployeeIds.Contains(es.EmployeeId)));

                // Clean up feedback data created by tests
                db.Feedback.RemoveRange(db.Feedback.Where(f => testEmployeeIds.Contains(f.FromEmployeeId) || testEmployeeIds.Contains(f.ToEmployeeId)));
                db.FeedbackRequests.RemoveRange(db.FeedbackRequests.Where(fr => testEmployeeIds.Contains(fr.RequestorId) || testEmployeeIds.Contains(fr.EmployeeId)));

                // Clean up test user role assignments
                var testUserIds = new[] { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") };
                db.UserRoles.RemoveRange(db.UserRoles.Where(ur => testUserIds.Contains(ur.UserId)));

                await db.SaveChangesAsync();

                // NOTE: EnsureManagerAndEmployeeRelationship is now called by individual tests that need it
                // instead of here, to avoid creating test employees before the main seed runs
                System.Diagnostics.Debug.WriteLine("DatabaseCleanupFixture: InitializeAsync completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: InitializeAsync failed: {ex.Message}");
                // don't fail initialization solely because cleanup couldn't run; tests should still execute
            }
        }

        private async Task PerformGlobalDatabaseSetup()
        {
            Console.WriteLine("DatabaseCleanupFixture: PerformGlobalDatabaseSetup starting");
            try
            {
                Console.WriteLine("DatabaseCleanupFixture: Loading .env.test file");
                LoadEnvFile("d:/projects/CPR/.env.test");

                Console.WriteLine("DatabaseCleanupFixture: Environment variables after loading:");
                Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_HOST={Environment.GetEnvironmentVariable("POSTGRES_HOST")}");
                Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_PORT={Environment.GetEnvironmentVariable("POSTGRES_PORT")}");
                Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_DB={Environment.GetEnvironmentVariable("POSTGRES_DB")}");
                Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_USER={Environment.GetEnvironmentVariable("POSTGRES_USER")}");
                Console.WriteLine($"DatabaseCleanupFixture: POSTGRES_PASSWORD={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}");

                Console.WriteLine("DatabaseCleanupFixture: Ensuring database exists");
                await EnsureDatabaseExistsAsync();

                Console.WriteLine("DatabaseCleanupFixture: Running migrations");
                await RunMigrationsAsync();

                Console.WriteLine("DatabaseCleanupFixture: Seeding database");
                await SeedDatabaseAsync();

                Console.WriteLine("DatabaseCleanupFixture: PerformGlobalDatabaseSetup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DatabaseCleanupFixture: PerformGlobalDatabaseSetup failed: {ex.Message}");
                throw;
            }
        }

        private void LoadEnvFile(string envFilePath)
        {
            Console.WriteLine($"DatabaseCleanupFixture: LoadEnvFile called with path: {envFilePath}");
            Console.WriteLine($"DatabaseCleanupFixture: Current directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"DatabaseCleanupFixture: File exists: {File.Exists(envFilePath)}");

            if (!File.Exists(envFilePath))
            {
                // If .env.test doesn't exist, try to load from .env file as fallback
                envFilePath = "d:/projects/CPR/.env";
                Console.WriteLine($"DatabaseCleanupFixture: Trying fallback path: {envFilePath}, exists: {File.Exists(envFilePath)}");
                if (!File.Exists(envFilePath))
                {
                    Console.WriteLine("DatabaseCleanupFixture: No .env file found");
                    return; // No .env file found, use defaults
                }
            }

            Console.WriteLine("DatabaseCleanupFixture: Reading env file");
            foreach (var line in File.ReadAllLines(envFilePath))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                var parts = trimmedLine.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    // Remove quotes if present
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2);
                    else if (value.StartsWith("'") && value.EndsWith("'"))
                        value = value.Substring(1, value.Length - 2);

                    Console.WriteLine($"DatabaseCleanupFixture: Setting {key}={value}");
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            try
            {
                // Try to connect to check if database exists
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(ConnectionString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                await db.Database.CanConnectAsync();
                System.Diagnostics.Debug.WriteLine("=== DATABASE EXISTS: cpr_test ===");
            }
            catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "3D000") // 3D000 = invalid_catalog_name (database doesn't exist)
            {
                System.Diagnostics.Debug.WriteLine("=== DATABASE DOES NOT EXIST, CREATING: cpr_test ===");

                // Extract connection string components to create database
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString);

                // Connect to postgres database to create our test database
                var postgresConnectionString = new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString)
                {
                    Database = "postgres" // Connect to default postgres database
                }.ConnectionString;

                using (var postgresConnection = new NpgsqlConnection(postgresConnectionString))
                {
                    await postgresConnection.OpenAsync();
                    using (var command = postgresConnection.CreateCommand())
                    {
                        command.CommandText = $"CREATE DATABASE {builder.Database} OWNER {builder.Username}";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                System.Diagnostics.Debug.WriteLine("=== DATABASE CREATED: cpr_test ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== DATABASE EXISTENCE CHECK ERROR: {ex.Message} ===");
                // Continue anyway - the migration will fail if database really doesn't exist
            }
        }

        private async Task RunMigrationsAsync()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);
            await db.Database.MigrateAsync();
        }

        private async Task SeedDatabaseAsync()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            // Seed data - the seeder already has logic to skip if data exists
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<CPR.Infrastructure.Services.DatabaseSeeder>();
            var seeder = new CPR.Infrastructure.Services.DatabaseSeeder(db, logger);

            await seeder.SeedAsync();
            System.Diagnostics.Debug.WriteLine("=== DATABASE SEEDING COMPLETED ===");
        }

        public async Task DisposeAsync()
        {
            // best-effort cleanup after tests run
            try
            {
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(ConnectionString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                db.Goals.RemoveRange(db.Goals.Where(g => g.Title.StartsWith("Integration test") || g.Description == "verify persistence"));

                // Clean up employee skills created by tests
                var testEmployeeIds = new[]
                {
                    Guid.Parse("33333333-3333-3333-3333-333333333333"), // TestEmployeeId1
                    Guid.Parse("44444444-4444-4444-4444-444444444444"), // TestEmployeeId2
                    Guid.Parse("55555555-5555-5555-5555-555555555555"), // TestEmployeeId3
                    Guid.Parse("66666666-6666-6666-6666-666666666666"), // TestEmployeeId4
                    Guid.Parse("77777777-7777-7777-7777-777777777777"), // TestEmployeeId5
                    Guid.Parse("88888888-8888-8888-8888-888888888888")  // TestEmployeeId6
                };

                db.EmployeeSkills.RemoveRange(db.EmployeeSkills.Where(es => testEmployeeIds.Contains(es.EmployeeId)));

                // Clean up feedback data created by tests
                db.Feedback.RemoveRange(db.Feedback.Where(f => testEmployeeIds.Contains(f.FromEmployeeId) || testEmployeeIds.Contains(f.ToEmployeeId)));
                db.FeedbackRequests.RemoveRange(db.FeedbackRequests.Where(fr => testEmployeeIds.Contains(fr.RequestorId) || testEmployeeIds.Contains(fr.EmployeeId)));

                // Clean up test user role assignments
                var testUserIds = new[] { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") };
                db.UserRoles.RemoveRange(db.UserRoles.Where(ur => testUserIds.Contains(ur.UserId)));

                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // swallow - cleanup is best-effort
            }
        }

        private async Task EnsureManagerAndEmployeeRelationship(CPR.Infrastructure.Data.CprDbContext db)
        {
            Console.WriteLine("DatabaseCleanupFixture: EnsureManagerAndEmployeeRelationship starting");

            // Check if test data already exists
            var existingEmployee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            Console.WriteLine($"DatabaseCleanupFixture: Existing employee check - found: {existingEmployee != null}");

            if (existingEmployee != null)
            {
                Console.WriteLine("DatabaseCleanupFixture: Test data already exists, checking roles...");

                // Check if roles are assigned
                var managerUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
                var employeeUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

                var existingManagerRoles = await db.UserRoles.Where(ur => ur.UserId == managerUserId && !ur.IsDeleted).Include(ur => ur.Role).ToListAsync();
                var existingEmployeeRoles = await db.UserRoles.Where(ur => ur.UserId == employeeUserId && !ur.IsDeleted).Include(ur => ur.Role).ToListAsync();

                Console.WriteLine($"DatabaseCleanupFixture: Existing manager roles count: {existingManagerRoles.Count}");
                Console.WriteLine($"DatabaseCleanupFixture: Existing employee roles count: {existingEmployeeRoles.Count}");
                foreach (var ur in existingManagerRoles)
                {
                    Console.WriteLine($"DatabaseCleanupFixture: Manager role - RoleId: {ur.RoleId}, Role: {(ur.Role != null ? ur.Role.Title : "NULL")}");
                }
                foreach (var ur in existingEmployeeRoles)
                {
                    Console.WriteLine($"DatabaseCleanupFixture: Employee role - RoleId: {ur.RoleId}, Role: {(ur.Role != null ? ur.Role.Title : "NULL")}");
                }
                Console.WriteLine($"DatabaseCleanupFixture: Existing manager roles: {string.Join(", ", existingManagerRoles.Where(ur => ur.Role != null).Select(ur => ur.Role.Title))}");
                Console.WriteLine($"DatabaseCleanupFixture: Existing employee roles: {string.Join(", ", existingEmployeeRoles.Where(ur => ur.Role != null).Select(ur => ur.Role.Title))}");

                if (existingManagerRoles.Any(ur => ur.Role.Title == "People Manager") && existingEmployeeRoles.Any(ur => ur.Role.Title == "Employee"))
                {
                    Console.WriteLine("DatabaseCleanupFixture: Roles already properly assigned, returning");
                    return; // Data already exists with proper roles
                }
                else
                {
                    Console.WriteLine("DatabaseCleanupFixture: Roles not properly assigned, cleaning up and recreating...");

                    // Clean up existing data
                    db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.UserId == managerUserId || ur.UserId == employeeUserId));
                    db.Employees.RemoveRange(db.Employees.Where(e => e.UserId == managerUserId || e.UserId == employeeUserId));
                    db.Users.RemoveRange(db.Users.Where(u => u.Id == managerUserId || u.Id == employeeUserId));
                    db.Positions.RemoveRange(db.Positions.Where(p => p.Id == Guid.Parse("99999999-9999-9999-9999-999999999999") || p.Id == Guid.Parse("88888888-8888-8888-8888-888888888888")));
                    await db.SaveChangesAsync();
                    Console.WriteLine("DatabaseCleanupFixture: Cleaned up existing test data");
                }
            }

            Console.WriteLine("DatabaseCleanupFixture: Creating fresh test data");

            // Create manager user and employee
            var managerUser = new CPR.Domain.Entities.User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                UserName = "test_manager",
                DisplayName = "Test Manager User",
                PasswordHash = "hashedpassword",
                IsDeleted = false
            };

            var managerPosition = new CPR.Domain.Entities.Position
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Title = "Manager",
                CareerTrackId = Guid.Parse("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"), // Technology track
                IsDeleted = false
            };

            var managerEmployee = new CPR.Domain.Entities.Employee
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                UserId = managerUser.Id,
                PositionId = managerPosition.Id,
                DepartmentId = Guid.Parse("fff11111-1111-1111-1111-111111111111"), // Engineering
                IsDeleted = false
            };

            // Create direct report user and employee
            var employeeUser = new CPR.Domain.Entities.User
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                UserName = "test_employee",
                DisplayName = "Test Employee User",
                PasswordHash = "hashedpassword",
                IsDeleted = false
            };

            var employeePosition = new CPR.Domain.Entities.Position
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Title = "Developer",
                CareerTrackId = Guid.Parse("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"), // Technology track
                IsDeleted = false
            };

            var employee = new CPR.Domain.Entities.Employee
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                UserId = employeeUser.Id,
                ManagerId = managerEmployee.Id,
                PositionId = employeePosition.Id,
                DepartmentId = Guid.Parse("fff11111-1111-1111-1111-111111111111"), // Engineering
                IsDeleted = false
            };

            // Add entities
            db.Users.Add(managerUser);
            db.Users.Add(employeeUser);
            db.Positions.Add(managerPosition);
            db.Positions.Add(employeePosition);
            db.Employees.Add(managerEmployee);
            db.Employees.Add(employee);

            await db.SaveChangesAsync();

            // Assign roles to test users
            var employeeRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "Employee");
            var managerRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "People Manager");

            // If roles don't exist, create them
            if (employeeRole == null)
            {
                employeeRole = new CPR.Domain.Entities.Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Employee",
                    Description = "Basic user role for individual contributors.",
                    CreatedBy = Guid.Empty,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };
                db.Roles.Add(employeeRole);
            }

            if (managerRole == null)
            {
                managerRole = new CPR.Domain.Entities.Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "People Manager",
                    Description = "Manager role for team leads.",
                    CreatedBy = Guid.Empty,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };
                db.Roles.Add(managerRole);
            }

            await db.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Roles ready - Employee: {employeeRole.Title}, Manager: {managerRole.Title}");

            var managerUserRole = new CPR.Domain.Entities.UserToRole
            {
                Id = Guid.NewGuid(),
                UserId = managerUser.Id,
                RoleId = managerRole.Id,
                CreatedBy = Guid.Empty, // System
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = false
            };

            var employeeUserRole = new CPR.Domain.Entities.UserToRole
            {
                Id = Guid.NewGuid(),
                UserId = employeeUser.Id,
                RoleId = employeeRole.Id,
                CreatedBy = Guid.Empty, // System
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = false
            };

            db.UserRoles.Add(managerUserRole);
            db.UserRoles.Add(employeeUserRole);
            await db.SaveChangesAsync();

            // Verify data was saved
            var savedManager = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            var savedEmployee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            var directReports = await db.Employees.Where(e => e.ManagerId == savedManager.Id && !e.IsDeleted).ToListAsync();

            System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Manager saved: {savedManager != null}, Employee saved: {savedEmployee != null}, Direct reports count: {directReports.Count}");
            if (savedManager != null) System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Manager ID: {savedManager.Id}, IsDeleted: {savedManager.IsDeleted}");
            if (savedEmployee != null) System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Employee ID: {savedEmployee.Id}, ManagerId: {savedEmployee.ManagerId}, IsDeleted: {savedEmployee.IsDeleted}");

            // Verify role assignments
            var managerRoles = await db.UserRoles.Where(ur => ur.UserId == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") && !ur.IsDeleted).Include(ur => ur.Role).ToListAsync();
            var employeeRoles = await db.UserRoles.Where(ur => ur.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") && !ur.IsDeleted).Include(ur => ur.Role).ToListAsync();

            System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Manager roles count: {managerRoles.Count}, Employee roles count: {employeeRoles.Count}");
            foreach (var ur in managerRoles) System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Manager role: {ur.Role.Title}");
            foreach (var ur in employeeRoles) System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Employee role: {ur.Role.Title}");
        }
    }
}
