using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service for seeding initial data into the database
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly CprDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(CprDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeds initial data if the database is empty (only in Development environment)
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting database seeding...");

            // Check environment to determine seeding behavior
            var aspnetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevOrTestEnvironment = aspnetcoreEnvironment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true
                || aspnetcoreEnvironment?.Equals("Test", StringComparison.OrdinalIgnoreCase) == true;
            _logger.LogInformation("ASP.NET Core Environment: '{AspNetCoreEnvironment}', IsDevOrTestEnvironment: {IsDevOrTestEnvironment}", aspnetcoreEnvironment, isDevOrTestEnvironment);

            if (isDevOrTestEnvironment)
            {
                // Check if seeding has already been completed
                var seedingCompleted = false; // Force seeding in development/test environment
                if (seedingCompleted)
                {
                    _logger.LogInformation("Database already seeded for {Environment} environment.", aspnetcoreEnvironment);
                    return;
                }

                _logger.LogInformation("{Environment} environment detected - seeding all data...", aspnetcoreEnvironment);
                await SeedRolesAsync();
                await SeedDepartmentsAsync();
                await SeedLocationsAsync();
                await SeedUsersAsync();
                await SeedCareerPathsAndTracksAsync();
                await SeedPositionsAsync();
                await SeedEmployeesAsync();
                await SeedSkillsAndCategoriesAsync();
                await SeedProjectsAsync();
                await SeedProjectRolesAsync();
                await SeedPositionToSkillsAsync();
                await SeedEmployeeToSkillsAsync();
            }
            else
            {
                _logger.LogInformation("Non-development environment detected - skipping seeding.");
            }

            _logger.LogInformation("Database seeding completed.");
        }

        private async Task SeedRolesAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Roles.AnyAsync(r => !r.IsDeleted))
            {
                _logger.LogInformation("Roles already exist, skipping seeding.");
                return;
            }

            var roles = new[]
            {
                new Role
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Title = "Employee",
                    Description = "Basic user role for individual contributors. Can manage their own goals, feedback, and profile."
                },
                new Role
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    Title = "People Manager",
                    Description = "Extends Employee role. Can view and manage their direct reports' goals, feedback, and team information."
                },
                new Role
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    Title = "Solution Owner",
                    Description = "Extends People Manager role. Can manage projects and oversee solution-level initiatives."
                },
                new Role
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    Title = "Director",
                    Description = "Extends Solution Owner role. Can approve promotions and access director-level reports."
                },
                new Role
                {
                    Id = new Guid("55555555-5555-5555-5555-555555555555"),
                    Title = "Administrator",
                    Description = "Full system access. Can manage users, roles, positions, and all system data."
                }
            };

            // Set audit fields for roles
            var systemUserId = Guid.Empty; // System user for seeding
            foreach (var role in roles)
            {
                role.CreatedBy = systemUserId;
                role.CreatedAt = DateTimeOffset.UtcNow;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding roles...");
                // Delete existing user-role assignments first due to foreign key constraints
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM user_to_role");
                // Delete all existing roles
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM roles");
                // Add all roles
                await _context.Roles.AddRangeAsync(roles);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("Seeding roles...");
                // Only add roles that don't already exist
                var existingRoleIds = await _context.Roles.Where(r => !r.IsDeleted).Select(r => r.Id).ToListAsync();
                var rolesToAdd = roles.Where(r => !existingRoleIds.Contains(r.Id)).ToArray();
                if (rolesToAdd.Length > 0)
                {
                    await _context.Roles.AddRangeAsync(rolesToAdd);
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Seeded {RoleCount} roles.", roles.Length);
        }

        private async Task SeedDepartmentsAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Departments.AnyAsync())
            {
                _logger.LogInformation("Departments already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding departments...");
                // Note: Employees are deleted in SeedUsersAndEmployeesAsync
                await _context.Positions.ExecuteDeleteAsync();
                // Remove existing departments
                await _context.Departments.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding departments...");
            }

            var departments = new[]
            {
                new Department
                {
                    Id = new Guid("fff00000-0000-0000-0000-000000000000"),
                    Name = "Quality Assurance",
                    Code = "QA",
                    Description = "Quality assurance and testing"
                },
                new Department
                {
                    Id = new Guid("fff11111-1111-1111-1111-111111111111"),
                    Name = "Engineering",
                    Code = "ENG",
                    Description = "Software development and engineering"
                },
                new Department
                {
                    Id = new Guid("fff22222-2222-2222-2222-222222222222"),
                    Name = "Human Resources",
                    Code = "HR",
                    Description = "People operations and employee development"
                },
                new Department
                {
                    Id = new Guid("fff33333-3333-3333-3333-333333333333"),
                    Name = "Finance",
                    Code = "FIN",
                    Description = "Financial planning and reporting"
                },
                new Department
                {
                    Id = new Guid("fff44444-4444-4444-4444-444444444444"),
                    Name = "Marketing",
                    Code = "MKT",
                    Description = "Brand management and customer acquisition"
                },
                new Department
                {
                    Id = new Guid("fff55555-5555-5555-5555-555555555555"),
                    Name = "Operations",
                    Code = "OPS",
                    Description = "Business operations and logistics"
                },
                new Department
                {
                    Id = new Guid("fff66666-6666-6666-6666-666666666666"),
                    Name = "Sales",
                    Code = "SAL",
                    Description = "Sales and business development"
                },
                new Department
                {
                    Id = new Guid("fff77777-7777-7777-7777-777777777777"),
                    Name = "Product",
                    Code = "PRD",
                    Description = "Product management and design"
                },
                new Department
                {
                    Id = new Guid("fff88888-8888-8888-8888-888888888888"),
                    Name = "Legal",
                    Code = "LEG",
                    Description = "Legal affairs and compliance"
                },
                new Department
                {
                    Id = new Guid("fff99999-9999-9999-9999-999999999999"),
                    Name = "Security",
                    Code = "SEC",
                    Description = "Information security and data protection"
                }
            };

            await _context.Departments.AddRangeAsync(departments);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} departments.", departments.Length);
        }

        private async Task SeedUsersAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Users.AnyAsync(u => !u.IsDeleted))
            {
                _logger.LogInformation("Users already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding users...");
                // Clean up existing data in dependency order
                await _context.GoalTasks.ExecuteDeleteAsync();
                await _context.Goals.ExecuteDeleteAsync();
                await _context.FeedbackRequests.ExecuteDeleteAsync();
                await _context.Feedback.ExecuteDeleteAsync();
                await _context.Set<EmployeeToSkill>().ExecuteDeleteAsync();
                await _context.ProjectTeams.ExecuteDeleteAsync();
                await _context.UserRoles.ExecuteDeleteAsync();
                await _context.Employees.ExecuteDeleteAsync();
                await _context.Users.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding users...");
            }

            // Create users
            var users = new[]
            {
                new User
                {
                    Id = new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"),
                    UserName = "john.doe",
                    DisplayName = "John Doe"
                },
                new User
                {
                    Id = new Guid("c6874b28-e2fa-4835-8e8f-159bd5067091"),
                    UserName = "jane.smith",
                    DisplayName = "Jane Smith"
                },
                new User
                {
                    Id = new Guid("45f0eaae-b3eb-4261-a430-4d9e94ec8e0d"),
                    UserName = "bob.johnson",
                    DisplayName = "Bob Johnson"
                },
                new User
                {
                    Id = new Guid("e9741b9b-3c66-4462-af46-297810b29403"),
                    UserName = "alice.wilson",
                    DisplayName = "Alice Wilson"
                },
                new User
                {
                    Id = new Guid("d670f2cf-66a6-4cb6-947f-062c7b089c8d"),
                    UserName = "charlie.brown",
                    DisplayName = "Charlie Brown"
                },
                new User
                {
                    Id = new Guid("bf428236-361c-4ade-995d-21a62feec86f"),
                    UserName = "diana.prince",
                    DisplayName = "Diana Prince"
                },
                new User
                {
                    Id = new Guid("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"),
                    UserName = "eve.adams",
                    DisplayName = "Eve Adams"
                },
                new User
                {
                    Id = new Guid("7567ad7a-174e-461c-bd88-e7489db10317"),
                    UserName = "frank.miller",
                    DisplayName = "Frank Miller"
                },
                new User
                {
                    Id = new Guid("5d70d7d5-570e-46fe-91ce-2d6081b365aa"),
                    UserName = "grace.lee",
                    DisplayName = "Grace Lee"
                },
                new User
                {
                    Id = new Guid("977f4f1f-b3ce-4244-98fc-2c0d0248de88"),
                    UserName = "henry.wilson",
                    DisplayName = "Henry Wilson"
                },
                new User
                {
                    Id = new Guid("20c78aa6-077a-4d3b-a7c5-84d561ec3975"),
                    UserName = "iris.davis",
                    DisplayName = "Iris Davis"
                },
                new User
                {
                    Id = new Guid("87896109-9ac7-444c-aa79-dfe1dc908a5d"),
                    UserName = "jack.thompson",
                    DisplayName = "Jack Thompson"
                },
                new User
                {
                    Id = new Guid("81694c14-a96a-4625-b5e5-a9fd034021af"),
                    UserName = "kate.garcia",
                    DisplayName = "Kate Garcia"
                },
                new User
                {
                    Id = new Guid("7522f469-e697-4e02-bb2f-f7f055c9aacb"),
                    UserName = "liam.anderson",
                    DisplayName = "Liam Anderson"
                },
                new User
                {
                    Id = new Guid("bf694c99-cfa6-4fba-be98-efba28bb4f31"),
                    UserName = "mia.rodriguez",
                    DisplayName = "Mia Rodriguez"
                },
                new User
                {
                    Id = new Guid("879c8ae6-c1c0-4d16-85fb-5dfe698fd108"),
                    UserName = "noah.martinez",
                    DisplayName = "Noah Martinez"
                },
                new User
                {
                    Id = new Guid("88b6f0d3-298e-4083-84cf-fa7893a0c846"),
                    UserName = "olivia.lopez",
                    DisplayName = "Olivia Lopez"
                },
                new User
                {
                    Id = new Guid("ad92cd5e-5599-4f3c-b2e1-39a49dd6b5bc"),
                    UserName = "peter.gonzalez",
                    DisplayName = "Peter Gonzalez"
                },
                new User
                {
                    Id = new Guid("cc230776-dc23-4996-9383-15fee9688215"),
                    UserName = "quinn.hernandez",
                    DisplayName = "Quinn Hernandez"
                },
                new User
                {
                    Id = new Guid("5950a2be-bdfb-4dcb-9913-1e3e0e022a5c"),
                    UserName = "ryan.king",
                    DisplayName = "Ryan King"
                },
                new User
                {
                    Id = new Guid("a22e8c3a-4ae0-4b59-b766-89a226fa82f0"),
                    UserName = "sara.wright",
                    DisplayName = "Sara Wright"
                },
                new User
                {
                    Id = new Guid("b1111111-1111-4111-1111-111111111111"),
                    UserName = "tom.richards",
                    DisplayName = "Tom Richards"
                },
                new User
                {
                    Id = new Guid("b2222222-2222-4222-2222-222222222222"),
                    UserName = "uma.patel",
                    DisplayName = "Uma Patel"
                },
                new User
                {
                    Id = new Guid("b3333333-3333-4333-3333-333333333333"),
                    UserName = "victor.chen",
                    DisplayName = "Victor Chen"
                },
                new User
                {
                    Id = new Guid("b4444444-4444-4444-4444-444444444444"),
                    UserName = "wendy.clark",
                    DisplayName = "Wendy Clark"
                },
                new User
                {
                    Id = new Guid("b5555555-5555-4555-5555-555555555555"),
                    UserName = "xavier.ross",
                    DisplayName = "Xavier Ross"
                },
                new User
                {
                    Id = new Guid("b6666666-6666-4666-6666-666666666666"),
                    UserName = "yara.hassan",
                    DisplayName = "Yara Hassan"
                },
                new User
                {
                    Id = new Guid("b7777777-7777-4777-7777-777777777777"),
                    UserName = "zack.powell",
                    DisplayName = "Zack Powell"
                },
                new User
                {
                    Id = new Guid("b8888888-8888-4888-8888-888888888888"),
                    UserName = "amy.foster",
                    DisplayName = "Amy Foster"
                },
                new User
                {
                    Id = new Guid("b9999999-9999-4999-9999-999999999999"),
                    UserName = "ben.cooper",
                    DisplayName = "Ben Cooper"
                },
                new User
                {
                    Id = new Guid("c1111111-1111-4111-1111-111111111111"),
                    UserName = "clara.morgan",
                    DisplayName = "Clara Morgan"
                },
                new User
                {
                    Id = new Guid("c2222222-2222-4222-2222-222222222222"),
                    UserName = "david.bell",
                    DisplayName = "David Bell"
                },
                new User
                {
                    Id = new Guid("c3333333-3333-4333-3333-333333333333"),
                    UserName = "emma.murphy",
                    DisplayName = "Emma Murphy"
                },
                new User
                {
                    Id = new Guid("c4444444-4444-4444-4444-444444444444"),
                    UserName = "felix.rivera",
                    DisplayName = "Felix Rivera"
                },
                new User
                {
                    Id = new Guid("c5555555-5555-4555-5555-555555555555"),
                    UserName = "gina.coleman",
                    DisplayName = "Gina Coleman"
                },
                new User
                {
                    Id = new Guid("c6666666-6666-4666-6666-666666666666"),
                    UserName = "hugo.sanders",
                    DisplayName = "Hugo Sanders"
                },
                new User
                {
                    Id = new Guid("c7777777-7777-4777-7777-777777777777"),
                    UserName = "isla.perry",
                    DisplayName = "Isla Perry"
                },
                new User
                {
                    Id = new Guid("c8888888-8888-4888-8888-888888888888"),
                    UserName = "jason.butler",
                    DisplayName = "Jason Butler"
                },
                new User
                {
                    Id = new Guid("c9999999-9999-4999-9999-999999999999"),
                    UserName = "kara.simmons",
                    DisplayName = "Kara Simmons"
                },
                new User
                {
                    Id = new Guid("d1111111-1111-4111-1111-111111111111"),
                    UserName = "leo.barnes",
                    DisplayName = "Leo Barnes"
                },
                new User
                {
                    Id = new Guid("d2222222-2222-4222-2222-222222222222"),
                    UserName = "maya.ross",
                    DisplayName = "Maya Ross"
                },
                new User
                {
                    Id = new Guid("d3333333-3333-4333-3333-333333333333"),
                    UserName = "nathan.jenkins",
                    DisplayName = "Nathan Jenkins"
                },
                new User
                {
                    Id = new Guid("d4444444-4444-4444-4444-444444444444"),
                    UserName = "olivia.ward",
                    DisplayName = "Olivia Ward"
                },
                new User
                {
                    Id = new Guid("d5555555-5555-4555-5555-555555555555"),
                    UserName = "paul.hayes",
                    DisplayName = "Paul Hayes"
                },
                new User
                {
                    Id = new Guid("d6666666-6666-4666-6666-666666666666"),
                    UserName = "rachel.myers",
                    DisplayName = "Rachel Myers"
                },
                new User
                {
                    Id = new Guid("d7777777-7777-4777-7777-777777777777"),
                    UserName = "samuel.long",
                    DisplayName = "Samuel Long"
                },
                new User
                {
                    Id = new Guid("d8888888-8888-4888-8888-888888888888"),
                    UserName = "tina.watson",
                    DisplayName = "Tina Watson"
                },
                new User
                {
                    Id = new Guid("d9999999-9999-4999-9999-999999999999"),
                    UserName = "ulysses.brooks",
                    DisplayName = "Ulysses Brooks"
                },
                new User
                {
                    Id = new Guid("e1111111-1111-4111-1111-111111111111"),
                    UserName = "veronica.kelly",
                    DisplayName = "Veronica Kelly"
                },
                new User
                {
                    Id = new Guid("e2222222-2222-4222-2222-222222222222"),
                    UserName = "william.sanders",
                    DisplayName = "William Sanders"
                },
                new User
                {
                    Id = new Guid("e3333333-3333-4333-3333-333333333333"),
                    UserName = "xena.price",
                    DisplayName = "Xena Price"
                },
                new User
                {
                    Id = new Guid("e4444444-4444-4444-4444-444444444444"),
                    UserName = "yale.bennett",
                    DisplayName = "Yale Bennett"
                },
                new User
                {
                    Id = new Guid("e5555555-5555-4555-5555-555555555555"),
                    UserName = "zoe.wood",
                    DisplayName = "Zoe Wood"
                },
                new User
                {
                    Id = new Guid("e6666666-6666-4666-6666-666666666666"),
                    UserName = "aaron.gray",
                    DisplayName = "Aaron Gray"
                },
                new User
                {
                    Id = new Guid("e7777777-7777-4777-7777-777777777777"),
                    UserName = "bella.james",
                    DisplayName = "Bella James"
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Assign roles to users when forced (for data consistency)
            if (forceSeed)
            {
                await AssignRolesToUsersAsync(true);
            }
            else
            {
                await AssignRolesToUsersAsync();
            }

            _logger.LogInformation("Seeded {UserCount} users.", users.Length);
        }

        private async Task SeedEmployeesAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Employees.AnyAsync(e => !e.IsDeleted))
            {
                _logger.LogInformation("Employees already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding employees...");
                // Clean up existing employee data in dependency order
                await _context.GoalTasks.ExecuteDeleteAsync();
                await _context.Goals.ExecuteDeleteAsync();
                await _context.FeedbackRequests.ExecuteDeleteAsync();
                await _context.Feedback.ExecuteDeleteAsync();
                await _context.Set<EmployeeToSkill>().ExecuteDeleteAsync();
                await _context.ProjectTeams.ExecuteDeleteAsync();
                await _context.Employees.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding employees...");
            }

            // Get departments for reference
            var engineeringDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "ENG");
            var hrDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "HR");
            var financeDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "FIN");
            var marketingDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "MKT");
            var operationsDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "OPS");
            var salesDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "SAL");
            var productDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "PRD");
            var legalDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "LEG");
            var securityDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "SEC");
            var qaDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "QA");

            // Get all users - Use specific GUIDs instead of relying on array index order
            // This ensures consistent employee-to-user mapping regardless of database query order
            var johnDoeUserId = new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"); // user 0
            var janeSmithUserId = new Guid("c6874b28-e2fa-4835-8e8f-159bd5067091"); // user 1
            var bobJohnsonUserId = new Guid("45f0eaae-b3eb-4261-a430-4d9e94ec8e0d"); // user 2
            var aliceWilsonUserId = new Guid("e9741b9b-3c66-4462-af46-297810b29403"); // user 3
            var charlieBrownUserId = new Guid("d670f2cf-66a6-4cb6-947f-062c7b089c8d"); // user 4
            var dianaPrinceUserId = new Guid("bf428236-361c-4ade-995d-21a62feec86f"); // user 5
            var eveAdamsUserId = new Guid("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"); // user 6
            var frankMillerUserId = new Guid("7567ad7a-174e-461c-bd88-e7489db10317"); // user 7
            var graceLeeUserId = new Guid("5d70d7d5-570e-46fe-91ce-2d6081b365aa"); // user 8
            var henryWilsonUserId = new Guid("977f4f1f-b3ce-4244-98fc-2c0d0248de88"); // user 9
            var irisDavisUserId = new Guid("20c78aa6-077a-4d3b-a7c5-84d561ec3975"); // user 10

            // For remaining employees, userIds array is populated from database query
            // Order doesn't matter for them as they aren't referenced in tests
            var allUsers = await _context.Users.Where(u => !u.IsDeleted).ToListAsync();
            var userIds = allUsers.Select(u => u.Id).ToList();

            // Create employees linked to users with proper organizational hierarchy
            // VP level - reports to CEO (not in system)
            var vpEngId = new Guid("00000000-0000-0000-0000-000000000001");

            // Senior Directors - report to VP
            var seniorDirDevOpsId = new Guid("00000000-0000-0000-0000-000000000002");

            // Directors - report to Senior Directors or VP
            var dirFrontendId = new Guid("00000000-0000-0000-0000-000000000003");
            var dirBackendId = new Guid("00000000-0000-0000-0000-000000000004");
            var dirMobileId = new Guid("00000000-0000-0000-0000-000000000005");
            var dirInfraId = new Guid("00000000-0000-0000-0000-000000000006");
            var dirSecurityId = new Guid("00000000-0000-0000-0000-000000000007");
            var dirPlatformId = new Guid("00000000-0000-0000-0000-000000000008");
            var dirMLId = new Guid("00000000-0000-0000-0000-000000000009");
            var dirSupportId = new Guid("00000000-0000-0000-0000-00000000000a");
            var dirDevOpsId = new Guid("00000000-0000-0000-0000-00000000000b");

            // Engineering Managers/Senior Managers - report to Directors
            var engMgrId = new Guid("00000000-0000-0000-0000-00000000000c");
            var seniorEngMgrId = new Guid("00000000-0000-0000-0000-00000000000d");
            var engDirId = new Guid("00000000-0000-0000-0000-00000000000e");
            var seniorEngDirId = new Guid("00000000-0000-0000-0000-00000000000f");

            var employees = new[]
            {
                // VP of Engineering - Top of Engineering org (John Doe)
                new Employee
                {
                    Id = vpEngId,
                    UserId = johnDoeUserId,
                    PositionId = new Guid("aa00c283-1af1-4397-bc34-2f674bda4852"), // VP of Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = null // Reports to CEO (not in system)
                },
                
                // Senior Director of DevOps Engineering - reports to VP (Jane Smith)
                new Employee
                {
                    Id = seniorDirDevOpsId,
                    UserId = janeSmithUserId,
                    PositionId = new Guid("aeb133ad-e366-4b7a-be19-a86659e423f8"), // Senior Director of DevOps Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of Frontend Engineering - reports to VP (Bob Johnson)
                new Employee
                {
                    Id = dirFrontendId,
                    UserId = bobJohnsonUserId,
                    PositionId = new Guid("06091429-d5c5-47f7-9f85-3034618325c5"), // Director of Frontend Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of Backend Engineering - reports to VP (Alice Wilson)
                new Employee
                {
                    Id = dirBackendId,
                    UserId = aliceWilsonUserId,
                    PositionId = new Guid("6f33e923-834d-4639-a88a-9fdec61215f4"), // Director of Backend Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of Mobile Engineering - reports to VP (Charlie Brown)
                new Employee
                {
                    Id = dirMobileId,
                    UserId = charlieBrownUserId,
                    PositionId = new Guid("f640cd3f-6e9d-4dc8-9c4f-acbbcd34ad24"), // Director of Mobile Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of Infrastructure Engineering - reports to Senior Dir DevOps (Diana Prince)
                new Employee
                {
                    Id = dirInfraId,
                    UserId = dianaPrinceUserId,
                    PositionId = new Guid("948cd97b-7376-45ee-91f3-b511a456bb58"), // Director of Infrastructure Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = seniorDirDevOpsId
                },
                
                // Director of Security Engineering - reports to VP (Eve Adams)
                new Employee
                {
                    Id = dirSecurityId,
                    UserId = eveAdamsUserId,
                    PositionId = new Guid("80d23551-e51e-405a-9339-8a6f52a0c6bd"), // Director of Security Engineering
                    DepartmentId = securityDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of Platform Engineering - reports to VP (Frank Miller)
                new Employee
                {
                    Id = dirPlatformId,
                    UserId = frankMillerUserId,
                    PositionId = new Guid("5284d661-4c72-40d1-8cc5-56e08a6138d0"), // Director of Platform Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of ML Engineering - reports to VP (Grace Lee)
                new Employee
                {
                    Id = dirMLId,
                    UserId = graceLeeUserId,
                    PositionId = new Guid("b707fc9a-2442-4ad7-b8bd-0567f87ef5a1"), // Director of ML Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of Technical Support - reports to VP (Henry Wilson)
                new Employee
                {
                    Id = dirSupportId,
                    UserId = henryWilsonUserId,
                    PositionId = new Guid("d38d1988-8914-4453-826f-063b5bc016c0"), // Director of Technical Support
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Director of DevOps Engineering - reports to Senior Dir DevOps (Iris Davis)
                new Employee
                {
                    Id = dirDevOpsId,
                    UserId = userIds[10],
                    PositionId = new Guid("f75f7943-1667-4438-a27d-93408f39f49d"), // Director of DevOps Engineering
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = seniorDirDevOpsId
                },
                
                // Engineering Manager - reports to Dir Frontend (user 11)
                new Employee
                {
                    Id = engMgrId,
                    UserId = userIds[11],
                    PositionId = new Guid("570d12e2-911e-4adc-a98a-373e4c8aab53"), // Engineering Manager
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirFrontendId
                },
                
                // Senior Engineering Manager - reports to Dir Backend (user 12)
                new Employee
                {
                    Id = seniorEngMgrId,
                    UserId = userIds[12],
                    PositionId = new Guid("ab42e7b3-caf3-417d-80f9-62422e089596"), // Senior Engineering Manager
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirBackendId
                },
                
                // Engineering Director - reports to VP (user 13)
                new Employee
                {
                    Id = engDirId,
                    UserId = userIds[13],
                    PositionId = new Guid("8ef2121e-e2c5-4514-b16a-dda651fb1c50"), // Engineering Director
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Senior Engineering Director - reports to VP (user 14)
                new Employee
                {
                    Id = seniorEngDirId,
                    UserId = userIds[14],
                    PositionId = new Guid("f2208214-d7c6-4db4-98ea-99e097576ef8"), // Senior Engineering Director
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = vpEngId
                },
                
                // Principal Frontend Engineer - reports to Dir Frontend (user 15)
                new Employee
                {
                    Id = new Guid("15151515-1515-4151-5151-515151515151"),
                    UserId = userIds[15],
                    PositionId = new Guid("ad711f5b-08c2-43b3-b503-d9e25c545119"), // Principal Frontend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirFrontendId
                },
                
                // Principal Backend Engineer - reports to Dir Backend (user 16)
                new Employee
                {
                    Id = new Guid("16161616-1616-4161-6161-616161616161"),
                    UserId = userIds[16],
                    PositionId = new Guid("901c89ea-a073-48c6-ab32-c414914b8f2e"), // Principal Backend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirBackendId
                },
                
                // Principal Mobile Engineer - reports to Dir Mobile (user 17)
                new Employee
                {
                    Id = new Guid("17171717-1717-4171-7171-717171717171"),
                    UserId = userIds[17],
                    PositionId = new Guid("b28f0718-03af-4a80-9d0b-6034e52a2ef0"), // Principal Mobile Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirMobileId
                },
                
                // Principal Infrastructure Engineer - reports to Dir Infrastructure (user 18)
                new Employee
                {
                    Id = new Guid("18181818-1818-4181-8181-818181818181"),
                    UserId = userIds[18],
                    PositionId = new Guid("37ea7352-b7f8-43f0-a377-138fb40a6d0d"), // Principal Infrastructure Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirInfraId
                },
                
                // Principal Security Engineer - reports to Dir Security (user 19)
                new Employee
                {
                    Id = new Guid("19191919-1919-4191-9191-919191919191"),
                    UserId = userIds[19],
                    PositionId = new Guid("a35f6bd9-652e-42d6-b137-929c3b1bd791"), // Principal Security Engineer
                    DepartmentId = securityDept?.Id,
                    ManagerId = dirSecurityId
                },
                
                // Principal Platform Engineer - reports to Dir Platform (user 20)
                new Employee
                {
                    Id = new Guid("20202020-2020-4202-0202-202020202020"),
                    UserId = userIds[20],
                    PositionId = new Guid("24582967-5515-4000-a390-bce4aa59d460"), // Principal Platform Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirPlatformId
                },
                
                // Principal ML Engineer - reports to Dir ML (user 21)
                new Employee
                {
                    Id = new Guid("21212121-2121-4212-1212-212121212121"),
                    UserId = userIds[21],
                    PositionId = new Guid("eec819f7-a4f5-4b2a-a1f6-328bb889c6c5"), // Principal ML Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirMLId
                },
                
                // Principal Support Engineer - reports to Dir Support (user 22)
                new Employee
                {
                    Id = new Guid("22222222-2222-4222-2222-222222222222"),
                    UserId = userIds[22],
                    PositionId = new Guid("bf563729-a2eb-4716-ab53-9b1497f2fa2a"), // Principal Support Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirSupportId
                },
                
                // Staff Frontend Engineer - reports to Eng Manager (user 23)
                new Employee
                {
                    Id = new Guid("23232323-2323-4232-3232-323232323232"),
                    UserId = userIds[23],
                    PositionId = new Guid("6073c789-4512-499c-84c6-0195e87c60bc"), // Staff Frontend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = engMgrId
                },
                
                // Staff Backend Engineer - reports to Senior Eng Manager (user 24)
                new Employee
                {
                    Id = new Guid("24242424-2424-4242-4242-424242424242"),
                    UserId = userIds[24],
                    PositionId = new Guid("ac69cf26-aa02-481c-ac06-5ccd9dfec7f0"), // Staff Backend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = seniorEngMgrId
                },
                
                // Staff Mobile Engineer - reports to Dir Mobile (user 25)
                new Employee
                {
                    Id = new Guid("25252525-2525-4252-5252-525252525252"),
                    UserId = userIds[25],
                    PositionId = new Guid("0a3ddc39-af6b-487d-9bac-2d9dd0bee554"), // Staff Mobile Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirMobileId
                },
                
                // Staff Infrastructure Engineer - reports to Dir Infrastructure (user 26)
                new Employee
                {
                    Id = new Guid("26262626-2626-4262-6262-626262626262"),
                    UserId = userIds[26],
                    PositionId = new Guid("4fea9a06-63a6-4f4e-bc81-e89ba6709295"), // Staff Infrastructure Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirInfraId
                },
                
                // Staff Security Engineer - reports to Dir Security (user 27)
                new Employee
                {
                    Id = new Guid("27272727-2727-4272-7272-727272727272"),
                    UserId = userIds[27],
                    PositionId = new Guid("da04a0f8-bf20-45f7-af85-3595cc2f7f15"), // Staff Security Engineer
                    DepartmentId = securityDept?.Id,
                    ManagerId = dirSecurityId
                },
                
                // Staff Platform Engineer - reports to Dir Platform (user 28)
                new Employee
                {
                    Id = new Guid("28282828-2828-4282-8282-828282828282"),
                    UserId = userIds[28],
                    PositionId = new Guid("e3e2e045-ae6e-4629-810d-2720e750eb9c"), // Staff Platform Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirPlatformId
                },
                
                // Staff ML Engineer - reports to Dir ML (user 29)
                new Employee
                {
                    Id = new Guid("29292929-2929-4292-9292-929292929292"),
                    UserId = userIds[29],
                    PositionId = new Guid("b4c4445e-2bc2-441b-8fb9-ef5a9e6e0696"), // Staff ML Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirMLId
                },
                
                // Staff Support Engineer - reports to Dir Support (user 30)
                new Employee
                {
                    Id = new Guid("30303030-3030-4303-0303-303030303030"),
                    UserId = userIds[30],
                    PositionId = new Guid("148802ce-fe15-42a7-b6ca-33d9ea320448"), // Staff Support Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirSupportId
                },
                
                // Lead DevOps Engineer - reports to Dir DevOps (user 31)
                new Employee
                {
                    Id = new Guid("31313131-3131-4313-1313-313131313131"),
                    UserId = userIds[31],
                    PositionId = new Guid("7b018a08-132c-46ee-bbe1-d4a4320eab2a"), // Lead DevOps Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = dirDevOpsId
                },
                
                // Senior Frontend Engineer - reports to Staff Frontend (user 32)
                new Employee
                {
                    Id = new Guid("32323232-3232-4323-2323-323232323232"),
                    UserId = userIds[32],
                    PositionId = new Guid("0b5e0e9d-6920-4037-8312-0f3e78d0088a"), // Senior Frontend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("23232323-2323-4232-3232-323232323232") // Reports to Staff Frontend
                },
                
                // Senior Backend Engineer - reports to Staff Backend (user 33)
                new Employee
                {
                    Id = new Guid("33333333-3333-4333-3333-333333333333"),
                    UserId = userIds[33],
                    PositionId = new Guid("4c7ded1f-7dd8-47b7-8b63-35020100ba19"), // Senior Backend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("24242424-2424-4242-4242-424242424242") // Reports to Staff Backend
                },
                
                // Senior Mobile Engineer - reports to Staff Mobile (user 34)
                new Employee
                {
                    Id = new Guid("34343434-3434-4343-4343-434343434343"),
                    UserId = userIds[34],
                    PositionId = new Guid("4152e8ef-9424-49f3-8b86-3dd87b89c8bf"), // Senior Mobile Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("25252525-2525-4252-5252-525252525252") // Reports to Staff Mobile
                },
                
                // Senior Infrastructure Engineer - reports to Staff Infrastructure (user 35)
                new Employee
                {
                    Id = new Guid("35353535-3535-4353-5353-535353535353"),
                    UserId = userIds[35],
                    PositionId = new Guid("835c2609-0c27-446a-9fe7-779326c9f1f6"), // Senior Infrastructure Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("26262626-2626-4262-6262-626262626262") // Reports to Staff Infrastructure
                },
                
                // Senior Security Engineer - reports to Staff Security (user 36)
                new Employee
                {
                    Id = new Guid("36363636-3636-4363-6363-636363636363"),
                    UserId = userIds[36],
                    PositionId = new Guid("cf1ac4b7-4c13-4a72-8772-211bb1832ae0"), // Senior Security Engineer
                    DepartmentId = securityDept?.Id,
                    ManagerId = new Guid("27272727-2727-4272-7272-727272727272") // Reports to Staff Security
                },
                
                // Senior Platform Engineer - reports to Staff Platform (user 37)
                new Employee
                {
                    Id = new Guid("37373737-3737-4373-7373-737373737373"),
                    UserId = userIds[37],
                    PositionId = new Guid("7ad2dbb8-acc3-43bc-a9e5-48c166713822"), // Senior Platform Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("28282828-2828-4282-8282-828282828282") // Reports to Staff Platform
                },
                
                // Senior ML Engineer - reports to Staff ML (user 38)
                new Employee
                {
                    Id = new Guid("38383838-3838-4383-8383-838383838383"),
                    UserId = userIds[38],
                    PositionId = new Guid("0ecfa16b-8d67-4298-be82-06fb0be2c1a7"), // Senior ML Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("29292929-2929-4292-9292-929292929292") // Reports to Staff ML
                },
                
                // Senior Support Engineer - reports to Staff Support (user 39)
                new Employee
                {
                    Id = new Guid("39393939-3939-4393-9393-939393939393"),
                    UserId = userIds[39],
                    PositionId = new Guid("d8f594a3-1e3c-466f-a178-1e16ddb9a2ca"), // Senior Support Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("30303030-3030-4303-0303-303030303030") // Reports to Staff Support
                },
                
                // Senior DevOps Engineer - reports to Lead DevOps (user 40)
                new Employee
                {
                    Id = new Guid("40404040-4040-4404-0404-404040404040"),
                    UserId = userIds[40],
                    PositionId = new Guid("4e064c81-2d4d-4dfa-b95d-419fc13b72ca"), // Senior DevOps Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("31313131-3131-4313-1313-313131313131") // Reports to Lead DevOps
                },
                
                // Frontend Engineer - reports to Senior Frontend (user 41)
                new Employee
                {
                    Id = new Guid("41414141-4141-4414-1414-414141414141"),
                    UserId = userIds[41],
                    PositionId = new Guid("b56b213e-fcc1-4e1c-8653-e24a52a7e28d"), // Frontend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("32323232-3232-4323-2323-323232323232") // Reports to Senior Frontend
                },
                
                // Backend Engineer - reports to Senior Backend (user 42)
                new Employee
                {
                    Id = new Guid("42424242-4242-4424-2424-424242424242"),
                    UserId = userIds[42],
                    PositionId = new Guid("15e1ba17-e786-4a86-bbc1-1ada3f47b6df"), // Backend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("33333333-3333-4333-3333-333333333333") // Reports to Senior Backend
                },
                
                // Mobile Engineer - reports to Senior Mobile (user 43)
                new Employee
                {
                    Id = new Guid("43434343-4343-4434-3434-434343434343"),
                    UserId = userIds[43],
                    PositionId = new Guid("e9fa3feb-cf51-411f-adc0-8244e61a831d"), // Mobile Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("34343434-3434-4343-4343-434343434343") // Reports to Senior Mobile
                },
                
                // Infrastructure Engineer - reports to Senior Infrastructure (user 44)
                new Employee
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    UserId = userIds[44],
                    PositionId = new Guid("7573b182-5813-411a-91ee-b5beeef80e1a"), // Infrastructure Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("35353535-3535-4353-5353-535353535353") // Reports to Senior Infrastructure
                },
                
                // Security Engineer - reports to Senior Security (user 45)
                new Employee
                {
                    Id = new Guid("45454545-4545-4454-5454-454545454545"),
                    UserId = userIds[45],
                    PositionId = new Guid("fab1f5ce-59b0-4261-a359-b4ec60e9f606"), // Security Engineer
                    DepartmentId = securityDept?.Id,
                    ManagerId = new Guid("36363636-3636-4363-6363-636363636363") // Reports to Senior Security
                },
                
                // Platform Engineer - reports to Senior Platform (user 46)
                new Employee
                {
                    Id = new Guid("46464646-4646-4464-6464-464646464646"),
                    UserId = userIds[46],
                    PositionId = new Guid("78d6f12b-d42c-4af5-991b-f2b069305f2a"), // Platform Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("37373737-3737-4373-7373-737373737373") // Reports to Senior Platform
                },
                
                // ML Engineer - reports to Senior ML (user 47)
                new Employee
                {
                    Id = new Guid("47474747-4747-4474-7474-474747474747"),
                    UserId = userIds[47],
                    PositionId = new Guid("ca70ee84-c88f-4352-b80d-914a9cd33674"), // ML Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("38383838-3838-4383-8383-838383838383") // Reports to Senior ML
                },
                
                // Support Engineer - reports to Senior Support (user 48)
                new Employee
                {
                    Id = new Guid("48484848-4848-4484-8484-484848484848"),
                    UserId = userIds[48],
                    PositionId = new Guid("40f80e0b-f128-493a-99ce-5e1c765cc87f"), // Support Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("39393939-3939-4393-9393-939393939393") // Reports to Senior Support
                },
                
                // DevOps Engineer - reports to Senior DevOps (user 49)
                new Employee
                {
                    Id = new Guid("49494949-4949-4494-9494-494949494949"),
                    UserId = userIds[49],
                    PositionId = new Guid("48316a63-4812-4a04-ae3c-97248556d3f8"), // DevOps Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("40404040-4040-4404-0404-404040404040") // Reports to Senior DevOps
                },
                
                // Additional Frontend Engineer - reports to Eng Manager (user 50)
                new Employee
                {
                    Id = new Guid("50505050-5050-4505-0505-505050505050"),
                    UserId = userIds[50],
                    PositionId = new Guid("b56b213e-fcc1-4e1c-8653-e24a52a7e28d"), // Frontend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = engMgrId
                },
                
                // Additional Backend Engineer - reports to Senior Eng Manager (user 51)
                new Employee
                {
                    Id = new Guid("51515151-5151-4515-1515-515151515151"),
                    UserId = userIds[51],
                    PositionId = new Guid("15e1ba17-e786-4a86-bbc1-1ada3f47b6df"), // Backend Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = seniorEngMgrId
                },
                
                // Additional Mobile Engineer - reports to Principal Mobile (user 52)
                new Employee
                {
                    Id = new Guid("52525252-5252-4525-2525-525252525252"),
                    UserId = userIds[52],
                    PositionId = new Guid("e9fa3feb-cf51-411f-adc0-8244e61a831d"), // Mobile Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("17171717-1717-4171-7171-717171717171") // Reports to Principal Mobile
                },
                
                // Additional Infrastructure Engineer - reports to Principal Infrastructure (user 53)
                new Employee
                {
                    Id = new Guid("53535353-5353-4535-3535-535353535353"),
                    UserId = userIds[53],
                    PositionId = new Guid("7573b182-5813-411a-91ee-b5beeef80e1a"), // Infrastructure Engineer
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = new Guid("18181818-1818-4181-8181-818181818181") // Reports to Principal Infrastructure
                },
                
                // Additional Security Engineer - reports to Principal Security (user 54)
                new Employee
                {
                    Id = new Guid("54545454-5454-4545-4545-545454545454"),
                    UserId = userIds[54],
                    PositionId = new Guid("fab1f5ce-59b0-4261-a359-b4ec60e9f606"), // Security Engineer
                    DepartmentId = securityDept?.Id,
                    ManagerId = new Guid("19191919-1919-4191-9191-919191919191") // Reports to Principal Security
                }
            };

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {EmployeeCount} employees.", employees.Length);
        }

        private async Task AssignRolesToUsersAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.UserRoles.AnyAsync())
            {
                _logger.LogInformation("User roles already exist, skipping assignment.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force assigning roles to users...");
                // Remove existing user roles first
                await _context.UserRoles.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Assigning roles to users...");
            }

            // Get role IDs
            var employeeRole = await _context.Roles.FirstAsync(r => r.Title == "Employee");
            var peopleManagerRole = await _context.Roles.FirstAsync(r => r.Title == "People Manager");
            var administratorRole = await _context.Roles.FirstAsync(r => r.Title == "Administrator");
            var solutionOwnerRole = await _context.Roles.FirstAsync(r => r.Title == "Solution Owner");

            // Get user IDs - all users from the seed data
            var userIds = new[]
            {
                new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"), // John Doe
                new Guid("c6874b28-e2fa-4835-8e8f-159bd5067091"), // Jane Smith
                new Guid("45f0eaae-b3eb-4261-a430-4d9e94ec8e0d"), // Bob Johnson
                new Guid("e9741b9b-3c66-4462-af46-297810b29403"), // Alice Wilson
                new Guid("d670f2cf-66a6-4cb6-947f-062c7b089c8d"), // Charlie Brown
                new Guid("bf428236-361c-4ade-995d-21a62feec86f"), // Diana Prince
                new Guid("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"), // Eve Adams
                new Guid("7567ad7a-174e-461c-bd88-e7489db10317"), // Frank Miller
                new Guid("5d70d7d5-570e-46fe-91ce-2d6081b365aa"), // Grace Lee
                new Guid("977f4f1f-b3ce-4244-98fc-2c0d0248de88"), // Henry Wilson (People Manager)
                new Guid("20c78aa6-077a-4d3b-a7c5-84d561ec3975"), // Iris Davis
                new Guid("87896109-9ac7-444c-aa79-dfe1dc908a5d"), // Jack Thompson
                new Guid("81694c14-a96a-4625-b5e5-a9fd034021af"), // Kate Garcia
                new Guid("7522f469-e697-4e02-bb2f-f7f055c9aacb"), // Liam Anderson
                new Guid("bf694c99-cfa6-4fba-be98-efba28bb4f31"), // Mia Rodriguez
                new Guid("879c8ae6-c1c0-4d16-85fb-5dfe698fd108"), // Noah Martinez
                new Guid("88b6f0d3-298e-4083-84cf-fa7893a0c846"), // Olivia Lopez
                new Guid("ad92cd5e-5599-4f3c-b2e1-39a49dd6b5bc"), // Peter Gonzalez
                new Guid("cc230776-dc23-4996-9383-15fee9688215"), // Quinn Hernandez
                new Guid("5950a2be-bdfb-4dcb-9913-1e3e0e022a5c"), // Ryan King
                new Guid("a22e8c3a-4ae0-4b59-b766-89a226fa82f0")  // Sara Wright
            };

            var systemUserId = Guid.Empty; // System user for seeding

            // Create role assignments - all users get Employee role by default
            var userRoles = new List<UserToRole>();

            // Assign Employee role to all users
            foreach (var userId in userIds)
            {
                userRoles.Add(new UserToRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = employeeRole.Id,
                    CreatedBy = systemUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            // Override specific users with higher roles
            // Henry Wilson - People Manager (he has direct reports)
            var henryWilsonIndex = userRoles.FindIndex(ur => ur.UserId == new Guid("977f4f1f-b3ce-4244-98fc-2c0d0248de88"));
            if (henryWilsonIndex >= 0)
            {
                userRoles[henryWilsonIndex] = new UserToRole
                {
                    Id = Guid.NewGuid(),
                    UserId = new Guid("977f4f1f-b3ce-4244-98fc-2c0d0248de88"),
                    RoleId = peopleManagerRole.Id,
                    CreatedBy = systemUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            }

            // John Doe - Administrator and Solution Owner (for testing purposes)
            var johnDoeIndex = userRoles.FindIndex(ur => ur.UserId == new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"));
            if (johnDoeIndex >= 0)
            {
                // Add Administrator role in addition to Employee role
                userRoles.Add(new UserToRole
                {
                    Id = Guid.NewGuid(),
                    UserId = new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"),
                    RoleId = administratorRole.Id,
                    CreatedBy = systemUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                // Add Solution Owner role in addition to Employee role
                userRoles.Add(new UserToRole
                {
                    Id = Guid.NewGuid(),
                    UserId = new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"),
                    RoleId = solutionOwnerRole.Id,
                    CreatedBy = systemUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned roles to {UserRoleCount} user-role relationships.", userRoles.Count);
        }

        private async Task SeedCareerPathsAndTracksAsync(bool forceSeed = false)
        {
            if (!forceSeed && (await _context.CareerPaths.AnyAsync() || await _context.CareerTracks.AnyAsync()))
            {
                _logger.LogInformation("Career paths and tracks already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding career paths and tracks...");
                // Remove existing data first
                await _context.CareerTracks.ExecuteDeleteAsync();
                await _context.CareerPaths.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding career paths and tracks...");
            }

            var careerPaths = new[]
            {
                new CareerPath
                {
                    Id = new Guid("aaa11111-1111-1111-1111-111111111111"),
                    Title = "Product",
                    Description = "Product management, design and user experience"
                },
                new CareerPath
                {
                    Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Title = "Technology",
                    Description = "Engineering, architecture and platform roles"
                },
                new CareerPath
                {
                    Id = new Guid("bbb22222-2222-2222-2222-222222222222"),
                    Title = "Legal",
                    Description = "Legal affairs, compliance and risk management"
                },
                new CareerPath
                {
                    Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Title = "People",
                    Description = "HR, people operations and employee development"
                },
                new CareerPath
                {
                    Id = new Guid("ccc33333-3333-3333-3333-333333333333"),
                    Title = "Security",
                    Description = "Information security, cybersecurity and data protection"
                },
                new CareerPath
                {
                    Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Title = "Finance",
                    Description = "Financial planning, reporting and analysis"
                },
                new CareerPath
                {
                    Id = new Guid("ddd44444-4444-4444-4444-444444444444"),
                    Title = "Quality Assurance",
                    Description = "Quality assurance, testing and process improvement"
                },
                new CareerPath
                {
                    Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Title = "Marketing",
                    Description = "Brand management, digital marketing and customer acquisition"
                },
                new CareerPath
                {
                    Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Title = "Operations",
                    Description = "Business operations, process optimization and logistics"
                },
                new CareerPath
                {
                    Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    Title = "Sales",
                    Description = "Sales, business development and account management"
                }
            };

            await _context.CareerPaths.AddRangeAsync(careerPaths);
            await _context.SaveChangesAsync();

            // Add all career tracks referenced by positions from the migration
            var careerTracks = new[]
            {
                // Technology Career Path Tracks
                new CareerTrack
                {
                    Id = new Guid("18fe3ad0-e6ea-4ad6-a18b-64ae97995443"),
                    Title = "Software Engineering",
                    Description = "Application and software development, full-stack engineering",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("446aa37c-aa90-41a7-9f76-2dec7b5d3dfc"),
                    Title = "Architecture",
                    Description = "System architecture, solution design, and technical strategy",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("4f86052f-9bc3-48e4-96a1-4b14b4284678"),
                    Title = "Data Engineering",
                    Description = "Data pipelines, analytics platforms, and data infrastructure",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("d7556322-667b-4062-96e1-c6773eccfc04"),
                    Title = "Security Engineering",
                    Description = "Application security, security architecture, and secure development",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("d533dd42-c223-4b34-9ddd-6e257f8016b8"),
                    Title = "Infrastructure Engineering",
                    Description = "Cloud infrastructure, platform engineering, and systems reliability",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("7875db90-71b6-4e06-a7cf-53a3a77f10a0"),
                    Title = "Software Delivery",
                    Description = "DevOps, CI/CD, release engineering, and deployment automation",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("e4441805-32e1-4714-9ae7-5688c9115ef4"),
                    Title = "Technical Support",
                    Description = "Production support, technical troubleshooting, and customer assistance",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("c5409a70-d866-465e-9d56-3da4684c54d3"),
                    Title = "Mobile Engineering",
                    Description = "iOS, Android, and cross-platform mobile application development",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("0c60eaad-dab3-426e-92f4-eafcd91db208"),
                    Title = "Frontend Engineering",
                    Description = "Web frontend development, UI engineering, and user interfaces",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f"),
                    Title = "Backend Engineering",
                    Description = "Server-side development, APIs, microservices, and distributed systems",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("43a12f91-e50a-480d-a865-1085f53adeb6"),
                    Title = "Machine Learning Engineering",
                    Description = "ML systems, AI model development, and MLOps",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"),
                    Title = "Engineering Management",
                    Description = "Technical leadership, team management, and engineering strategy",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c"),
                    Title = "Platform Engineering",
                    Description = "Developer platforms, tooling, and infrastructure services",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },

                // Product Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Product Management",
                    Description = "Product strategy, roadmap planning, and feature prioritization",
                    CareerPathId = new Guid("aaa11111-1111-1111-1111-111111111111")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Product Design",
                    Description = "UX/UI design, user research, and design systems",
                    CareerPathId = new Guid("aaa11111-1111-1111-1111-111111111111")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Product Analytics",
                    Description = "Product metrics, user behavior analysis, and data-driven insights",
                    CareerPathId = new Guid("aaa11111-1111-1111-1111-111111111111")
                },

                // Legal Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Corporate Law",
                    Description = "Corporate governance, contracts, and business transactions",
                    CareerPathId = new Guid("bbb22222-2222-2222-2222-222222222222")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Compliance",
                    Description = "Regulatory compliance, risk management, and policy enforcement",
                    CareerPathId = new Guid("bbb22222-2222-2222-2222-222222222222")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Privacy & Data Protection",
                    Description = "Data privacy law, GDPR/CCPA compliance, and privacy programs",
                    CareerPathId = new Guid("bbb22222-2222-2222-2222-222222222222")
                },

                // People Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Talent Acquisition",
                    Description = "Recruiting, talent sourcing, and candidate experience",
                    CareerPathId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "People Operations",
                    Description = "HR operations, employee relations, and people programs",
                    CareerPathId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Learning & Development",
                    Description = "Employee training, career development, and organizational learning",
                    CareerPathId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Compensation & Benefits",
                    Description = "Compensation strategy, benefits administration, and total rewards",
                    CareerPathId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                },

                // Security Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Cybersecurity",
                    Description = "Threat detection, incident response, and security operations",
                    CareerPathId = new Guid("ccc33333-3333-3333-3333-333333333333")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Security Architecture",
                    Description = "Security design, threat modeling, and security frameworks",
                    CareerPathId = new Guid("ccc33333-3333-3333-3333-333333333333")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Security Compliance",
                    Description = "Security audits, compliance certifications, and governance",
                    CareerPathId = new Guid("ccc33333-3333-3333-3333-333333333333")
                },

                // Finance Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Financial Planning & Analysis",
                    Description = "Budgeting, forecasting, and financial modeling",
                    CareerPathId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Accounting",
                    Description = "Financial reporting, accounts management, and audit",
                    CareerPathId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Corporate Finance",
                    Description = "Treasury, M&A, capital structure, and investor relations",
                    CareerPathId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc")
                },

                // Quality Assurance Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Engineering",
                    Description = "Test automation, framework development, and test strategy",
                    CareerPathId = new Guid("ddd44444-4444-4444-4444-444444444444")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Quality Assurance",
                    Description = "Manual testing, test case design, and quality processes",
                    CareerPathId = new Guid("ddd44444-4444-4444-4444-444444444444")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Performance Testing",
                    Description = "Load testing, performance optimization, and scalability testing",
                    CareerPathId = new Guid("ddd44444-4444-4444-4444-444444444444")
                },

                // Marketing Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Digital Marketing",
                    Description = "SEO, SEM, social media, and digital campaigns",
                    CareerPathId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Content Marketing",
                    Description = "Content strategy, copywriting, and content production",
                    CareerPathId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Brand Management",
                    Description = "Brand strategy, positioning, and brand communications",
                    CareerPathId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Marketing Analytics",
                    Description = "Marketing metrics, attribution modeling, and ROI analysis",
                    CareerPathId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd")
                },

                // Operations Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Business Operations",
                    Description = "Process optimization, operational excellence, and efficiency",
                    CareerPathId = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Supply Chain Management",
                    Description = "Logistics, vendor management, and supply chain optimization",
                    CareerPathId = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Program Management",
                    Description = "Program planning, coordination, and delivery management",
                    CareerPathId = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")
                },

                // Sales Career Path Tracks
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Enterprise Sales",
                    Description = "Large account sales, strategic partnerships, and enterprise deals",
                    CareerPathId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Sales Engineering",
                    Description = "Technical pre-sales, solution architecture, and customer demos",
                    CareerPathId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Account Management",
                    Description = "Customer success, account growth, and relationship management",
                    CareerPathId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")
                },
                new CareerTrack
                {
                    Id = Guid.NewGuid(),
                    Title = "Sales Operations",
                    Description = "Sales processes, CRM management, and sales analytics",
                    CareerPathId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")
                }
            };

            await _context.CareerTracks.AddRangeAsync(careerTracks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {PathCount} career paths and {TrackCount} career tracks.", careerPaths.Length, careerTracks.Length);
        }

        private async Task SeedPositionsAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Positions.AnyAsync())
            {
                _logger.LogInformation("Positions already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding positions...");
                // Remove existing positions first
                await _context.Positions.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding positions...");
            }

            var positions = new[]
            {
                // Software Delivery Track (7875db90-71b6-4e06-a7cf-53a3a77f10a0) - 5 positions
                new Position
                {
                    Id = new Guid("48316a63-4812-4a04-ae3c-97248556d3f8"),
                    CareerTrackId = new Guid("7875db90-71b6-4e06-a7cf-53a3a77f10a0"),
                    Title = "DevOps Engineer",
                    Description = "Entry-level DevOps focusing on CI/CD, deployment automation, and infrastructure basics",
                    Expectations = "Build and maintain CI/CD pipelines, assist with deployments, learn infrastructure as code"
                },
                new Position
                {
                    Id = new Guid("4e064c81-2d4d-4dfa-b95d-419fc13b72ca"),
                    CareerTrackId = new Guid("7875db90-71b6-4e06-a7cf-53a3a77f10a0"),
                    Title = "Senior DevOps Engineer",
                    Description = "Experienced DevOps engineer managing complex deployments and infrastructure architecture",
                    Expectations = "Lead complex infrastructure projects, mentor junior engineers, optimize deployment pipelines"
                },
                new Position
                {
                    Id = new Guid("7b018a08-132c-46ee-bbe1-d4a4320eab2a"),
                    CareerTrackId = new Guid("7875db90-71b6-4e06-a7cf-53a3a77f10a0"),
                    Title = "Lead DevOps Engineer",
                    Description = "Technical lead for DevOps practices, mentoring team and driving automation strategy",
                    Expectations = "Define DevOps strategy, lead team initiatives, establish best practices and standards"
                },
                new Position
                {
                    Id = new Guid("f75f7943-1667-4438-a27d-93408f39f49d"),
                    CareerTrackId = new Guid("7875db90-71b6-4e06-a7cf-53a3a77f10a0"),
                    Title = "Director of DevOps Engineering",
                    Description = "Leadership role managing DevOps teams and engineering infrastructure strategy",
                    Expectations = "Manage DevOps organization, set technical direction, align with business objectives"
                },
                new Position
                {
                    Id = new Guid("aeb133ad-e366-4b7a-be19-a86659e423f8"),
                    CareerTrackId = new Guid("7875db90-71b6-4e06-a7cf-53a3a77f10a0"),
                    Title = "Senior Director of DevOps Engineering",
                    Description = "Executive leadership for DevOps organization and technical operations",
                    Expectations = "Executive oversight of DevOps, strategic planning, cross-organizational leadership"
                },

                // Mobile Engineering Track (c5409a70-d866-465e-9d56-3da4684c54d3) - 5 positions
                new Position
                {
                    Id = new Guid("e9fa3feb-cf51-411f-adc0-8244e61a831d"),
                    CareerTrackId = new Guid("c5409a70-d866-465e-9d56-3da4684c54d3"),
                    Title = "Mobile Engineer",
                    Description = "Developing mobile applications for iOS and Android platforms",
                    Expectations = "Build mobile features, write tests, collaborate on mobile architecture"
                },
                new Position
                {
                    Id = new Guid("4152e8ef-9424-49f3-8b86-3dd87b89c8bf"),
                    CareerTrackId = new Guid("c5409a70-d866-465e-9d56-3da4684c54d3"),
                    Title = "Senior Mobile Engineer",
                    Description = "Leading mobile feature development and architecture decisions",
                    Expectations = "Lead complex mobile features, mentor engineers, drive mobile best practices"
                },
                new Position
                {
                    Id = new Guid("0a3ddc39-af6b-487d-9bac-2d9dd0bee554"),
                    CareerTrackId = new Guid("c5409a70-d866-465e-9d56-3da4684c54d3"),
                    Title = "Staff Mobile Engineer",
                    Description = "Setting technical direction for mobile platform and cross-platform solutions",
                    Expectations = "Define mobile architecture, solve complex technical challenges, influence mobile strategy"
                },
                new Position
                {
                    Id = new Guid("b28f0718-03af-4a80-9d0b-6034e52a2ef0"),
                    CareerTrackId = new Guid("c5409a70-d866-465e-9d56-3da4684c54d3"),
                    Title = "Principal Mobile Engineer",
                    Description = "Defining mobile strategy, performance optimization, and developer experience",
                    Expectations = "Strategic mobile platform decisions, performance leadership, cross-team collaboration"
                },
                new Position
                {
                    Id = new Guid("f640cd3f-6e9d-4dc8-9c4f-acbbcd34ad24"),
                    CareerTrackId = new Guid("c5409a70-d866-465e-9d56-3da4684c54d3"),
                    Title = "Director of Mobile Engineering",
                    Description = "Leading mobile engineering teams and mobile product strategy",
                    Expectations = "Manage mobile teams, set mobile technology direction, align with product goals"
                },

                // Frontend Engineering Track (0c60eaad-dab3-426e-92f4-eafcd91db208) - 5 positions
                new Position
                {
                    Id = new Guid("b56b213e-fcc1-4e1c-8653-e24a52a7e28d"),
                    CareerTrackId = new Guid("0c60eaad-dab3-426e-92f4-eafcd91db208"),
                    Title = "Frontend Engineer",
                    Description = "Building user interfaces and implementing responsive web applications",
                    Expectations = "Implement UI components, write clean code, collaborate with designers"
                },
                new Position
                {
                    Id = new Guid("0b5e0e9d-6920-4037-8312-0f3e78d0088a"),
                    CareerTrackId = new Guid("0c60eaad-dab3-426e-92f4-eafcd91db208"),
                    Title = "Senior Frontend Engineer",
                    Description = "Leading frontend architecture and component library development",
                    Expectations = "Lead frontend projects, establish component patterns, mentor junior engineers"
                },
                new Position
                {
                    Id = new Guid("6073c789-4512-499c-84c6-0195e87c60bc"),
                    CareerTrackId = new Guid("0c60eaad-dab3-426e-92f4-eafcd91db208"),
                    Title = "Staff Frontend Engineer",
                    Description = "Defining frontend standards, performance optimization, and accessibility",
                    Expectations = "Set frontend technical direction, optimize performance, drive accessibility initiatives"
                },
                new Position
                {
                    Id = new Guid("ad711f5b-08c2-43b3-b503-d9e25c545119"),
                    CareerTrackId = new Guid("0c60eaad-dab3-426e-92f4-eafcd91db208"),
                    Title = "Principal Frontend Engineer",
                    Description = "Strategic frontend architecture, design system leadership, and tooling",
                    Expectations = "Define frontend strategy, lead design system evolution, establish tooling standards"
                },
                new Position
                {
                    Id = new Guid("06091429-d5c5-47f7-9f85-3034618325c5"),
                    CareerTrackId = new Guid("0c60eaad-dab3-426e-92f4-eafcd91db208"),
                    Title = "Director of Frontend Engineering",
                    Description = "Managing frontend teams and driving frontend technology strategy",
                    Expectations = "Manage frontend organization, align frontend with product strategy, set technical vision"
                },
                // Backend Engineering Track (bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f) - 5 positions
                new Position
                {
                    Id = new Guid("15e1ba17-e786-4a86-bbc1-1ada3f47b6df"),
                    CareerTrackId = new Guid("bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f"),
                    Title = "Backend Engineer",
                    Description = "Building APIs, microservices, and server-side business logic",
                    Expectations = "Develop backend services, write tests, design database schemas"
                },
                new Position
                {
                    Id = new Guid("4c7ded1f-7dd8-47b7-8b63-35020100ba19"),
                    CareerTrackId = new Guid("bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f"),
                    Title = "Senior Backend Engineer",
                    Description = "Designing scalable backend systems and data models",
                    Expectations = "Lead backend architecture, optimize performance, mentor engineers"
                },
                new Position
                {
                    Id = new Guid("ac69cf26-aa02-481c-ac06-5ccd9dfec7f0"),
                    CareerTrackId = new Guid("bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f"),
                    Title = "Staff Backend Engineer",
                    Description = "Architecture for distributed systems and cross-service integration",
                    Expectations = "Design distributed systems, establish integration patterns, solve complex problems"
                },
                new Position
                {
                    Id = new Guid("901c89ea-a073-48c6-ab32-c414914b8f2e"),
                    CareerTrackId = new Guid("bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f"),
                    Title = "Principal Backend Engineer",
                    Description = "Strategic backend platform decisions and technical standards",
                    Expectations = "Define backend strategy, lead platform evolution, set technical standards"
                },
                new Position
                {
                    Id = new Guid("6f33e923-834d-4639-a88a-9fdec61215f4"),
                    CareerTrackId = new Guid("bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f"),
                    Title = "Director of Backend Engineering",
                    Description = "Leading backend teams and service architecture strategy",
                    Expectations = "Manage backend teams, set architectural direction, align with business needs"
                },

                // Machine Learning Engineering Track (43a12f91-e50a-480d-a865-1085f53adeb6) - 5 positions
                new Position
                {
                    Id = new Guid("ca70ee84-c88f-4352-b80d-914a9cd33674"),
                    CareerTrackId = new Guid("43a12f91-e50a-480d-a865-1085f53adeb6"),
                    Title = "ML Engineer",
                    Description = "Developing and deploying machine learning models and pipelines",
                    Expectations = "Build ML models, implement training pipelines, deploy to production"
                },
                new Position
                {
                    Id = new Guid("0ecfa16b-8d67-4298-be82-06fb0be2c1a7"),
                    CareerTrackId = new Guid("43a12f91-e50a-480d-a865-1085f53adeb6"),
                    Title = "Senior ML Engineer",
                    Description = "Leading ML model development and ML infrastructure",
                    Expectations = "Lead ML projects, optimize model performance, establish MLOps practices"
                },
                new Position
                {
                    Id = new Guid("b4c4445e-2bc2-441b-8fb9-ef5a9e6e0696"),
                    CareerTrackId = new Guid("43a12f91-e50a-480d-a865-1085f53adeb6"),
                    Title = "Staff ML Engineer",
                    Description = "ML platform architecture and MLOps strategy",
                    Expectations = "Design ML infrastructure, establish MLOps patterns, solve complex ML challenges"
                },
                new Position
                {
                    Id = new Guid("eec819f7-a4f5-4b2a-a1f6-328bb889c6c5"),
                    CareerTrackId = new Guid("43a12f91-e50a-480d-a865-1085f53adeb6"),
                    Title = "Principal ML Engineer",
                    Description = "Strategic ML research, model architecture, and AI strategy",
                    Expectations = "Define ML strategy, lead research initiatives, drive AI innovation"
                },
                new Position
                {
                    Id = new Guid("b707fc9a-2442-4ad7-b8bd-0567f87ef5a1"),
                    CareerTrackId = new Guid("43a12f91-e50a-480d-a865-1085f53adeb6"),
                    Title = "Director of ML Engineering",
                    Description = "Leading ML teams and AI product strategy",
                    Expectations = "Manage ML organization, set AI strategy, align with business objectives"
                },

                // Technical Support Track (e4441805-32e1-4714-9ae7-5688c9115ef4) - 5 positions
                new Position
                {
                    Id = new Guid("40f80e0b-f128-493a-99ce-5e1c765cc87f"),
                    CareerTrackId = new Guid("e4441805-32e1-4714-9ae7-5688c9115ef4"),
                    Title = "Support Engineer",
                    Description = "Providing technical support and troubleshooting production issues",
                    Expectations = "Respond to support tickets, troubleshoot issues, document solutions"
                },
                new Position
                {
                    Id = new Guid("d8f594a3-1e3c-466f-a178-1e16ddb9a2ca"),
                    CareerTrackId = new Guid("e4441805-32e1-4714-9ae7-5688c9115ef4"),
                    Title = "Senior Support Engineer",
                    Description = "Leading complex technical investigations and customer escalations",
                    Expectations = "Handle escalations, lead incident response, improve support processes"
                },
                new Position
                {
                    Id = new Guid("148802ce-fe15-42a7-b6ca-33d9ea320448"),
                    CareerTrackId = new Guid("e4441805-32e1-4714-9ae7-5688c9115ef4"),
                    Title = "Staff Support Engineer",
                    Description = "Driving support tooling, processes, and knowledge management",
                    Expectations = "Design support systems, establish best practices, mentor support team"
                },
                new Position
                {
                    Id = new Guid("bf563729-a2eb-4716-ab53-9b1497f2fa2a"),
                    CareerTrackId = new Guid("e4441805-32e1-4714-9ae7-5688c9115ef4"),
                    Title = "Principal Support Engineer",
                    Description = "Strategic support operations and customer success programs",
                    Expectations = "Define support strategy, optimize customer experience, lead technical programs"
                },
                new Position
                {
                    Id = new Guid("d38d1988-8914-4453-826f-063b5bc016c0"),
                    CareerTrackId = new Guid("e4441805-32e1-4714-9ae7-5688c9115ef4"),
                    Title = "Director of Technical Support",
                    Description = "Managing support teams and customer experience strategy",
                    Expectations = "Lead support organization, set customer success vision, manage SLAs"
                },

                // Infrastructure Engineering Track (d533dd42-c223-4b34-9ddd-6e257f8016b8) - 5 positions
                new Position
                {
                    Id = new Guid("7573b182-5813-411a-91ee-b5beeef80e1a"),
                    CareerTrackId = new Guid("d533dd42-c223-4b34-9ddd-6e257f8016b8"),
                    Title = "Infrastructure Engineer",
                    Description = "Managing cloud infrastructure, networking, and systems reliability",
                    Expectations = "Maintain infrastructure, respond to incidents, optimize systems"
                },
                new Position
                {
                    Id = new Guid("835c2609-0c27-446a-9fe7-779326c9f1f6"),
                    CareerTrackId = new Guid("d533dd42-c223-4b34-9ddd-6e257f8016b8"),
                    Title = "Senior Infrastructure Engineer",
                    Description = "Designing scalable infrastructure and platform reliability",
                    Expectations = "Lead infrastructure projects, improve reliability, mentor engineers"
                },
                new Position
                {
                    Id = new Guid("4fea9a06-63a6-4f4e-bc81-e89ba6709295"),
                    CareerTrackId = new Guid("d533dd42-c223-4b34-9ddd-6e257f8016b8"),
                    Title = "Staff Infrastructure Engineer",
                    Description = "Infrastructure architecture, capacity planning, and site reliability",
                    Expectations = "Design infrastructure architecture, establish SRE practices, optimize costs"
                },
                new Position
                {
                    Id = new Guid("37ea7352-b7f8-43f0-a377-138fb40a6d0d"),
                    CareerTrackId = new Guid("d533dd42-c223-4b34-9ddd-6e257f8016b8"),
                    Title = "Principal Infrastructure Engineer",
                    Description = "Strategic infrastructure decisions and cloud platform strategy",
                    Expectations = "Define infrastructure strategy, lead cloud migration, set technical direction"
                },
                new Position
                {
                    Id = new Guid("948cd97b-7376-45ee-91f3-b511a456bb58"),
                    CareerTrackId = new Guid("d533dd42-c223-4b34-9ddd-6e257f8016b8"),
                    Title = "Director of Infrastructure Engineering",
                    Description = "Leading infrastructure teams and operations strategy",
                    Expectations = "Manage infrastructure organization, set operations strategy, ensure reliability"
                },
                // Security Engineering Track (d7556322-667b-4062-96e1-c6773eccfc04) - 5 positions
                new Position
                {
                    Id = new Guid("fab1f5ce-59b0-4261-a359-b4ec60e9f606"),
                    CareerTrackId = new Guid("d7556322-667b-4062-96e1-c6773eccfc04"),
                    Title = "Security Engineer",
                    Description = "Implementing security controls and conducting security assessments",
                    Expectations = "Implement security measures, perform assessments, respond to incidents"
                },
                new Position
                {
                    Id = new Guid("cf1ac4b7-4c13-4a72-8772-211bb1832ae0"),
                    CareerTrackId = new Guid("d7556322-667b-4062-96e1-c6773eccfc04"),
                    Title = "Senior Security Engineer",
                    Description = "Leading security projects and vulnerability management",
                    Expectations = "Lead security initiatives, manage vulnerabilities, conduct threat analysis"
                },
                new Position
                {
                    Id = new Guid("da04a0f8-bf20-45f7-af85-3595cc2f7f15"),
                    CareerTrackId = new Guid("d7556322-667b-4062-96e1-c6773eccfc04"),
                    Title = "Staff Security Engineer",
                    Description = "Security architecture, threat modeling, and security tooling",
                    Expectations = "Design security architecture, establish threat models, build security tools"
                },
                new Position
                {
                    Id = new Guid("a35f6bd9-652e-42d6-b137-929c3b1bd791"),
                    CareerTrackId = new Guid("d7556322-667b-4062-96e1-c6773eccfc04"),
                    Title = "Principal Security Engineer",
                    Description = "Strategic security architecture and security program leadership",
                    Expectations = "Define security strategy, lead security programs, set security standards"
                },
                new Position
                {
                    Id = new Guid("80d23551-e51e-405a-9339-8a6f52a0c6bd"),
                    CareerTrackId = new Guid("d7556322-667b-4062-96e1-c6773eccfc04"),
                    Title = "Director of Security Engineering",
                    Description = "Managing security teams and enterprise security strategy",
                    Expectations = "Lead security organization, set enterprise security vision, manage risk"
                },

                // Platform Engineering Track (0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c) - 5 positions
                new Position
                {
                    Id = new Guid("78d6f12b-d42c-4af5-991b-f2b069305f2a"),
                    CareerTrackId = new Guid("0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c"),
                    Title = "Platform Engineer",
                    Description = "Building developer platforms, tooling, and internal services",
                    Expectations = "Build platform services, develop tooling, support internal teams"
                },
                new Position
                {
                    Id = new Guid("7ad2dbb8-acc3-43bc-a9e5-48c166713822"),
                    CareerTrackId = new Guid("0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c"),
                    Title = "Senior Platform Engineer",
                    Description = "Designing platform services and developer experience",
                    Expectations = "Lead platform projects, improve developer experience, optimize platform"
                },
                new Position
                {
                    Id = new Guid("e3e2e045-ae6e-4629-810d-2720e750eb9c"),
                    CareerTrackId = new Guid("0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c"),
                    Title = "Staff Platform Engineer",
                    Description = "Platform architecture, API design, and platform strategy",
                    Expectations = "Design platform architecture, establish API standards, drive platform vision"
                },
                new Position
                {
                    Id = new Guid("24582967-5515-4000-a390-bce4aa59d460"),
                    CareerTrackId = new Guid("0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c"),
                    Title = "Principal Platform Engineer",
                    Description = "Strategic platform decisions and infrastructure abstraction",
                    Expectations = "Define platform strategy, lead infrastructure abstraction, set technical direction"
                },
                new Position
                {
                    Id = new Guid("5284d661-4c72-40d1-8cc5-56e08a6138d0"),
                    CareerTrackId = new Guid("0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c"),
                    Title = "Director of Platform Engineering",
                    Description = "Leading platform teams and internal platform strategy",
                    Expectations = "Manage platform organization, set platform vision, align with business needs"
                },

                // Engineering Management Track (22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1) - 5 positions
                new Position
                {
                    Id = new Guid("570d12e2-911e-4adc-a98a-373e4c8aab53"),
                    CareerTrackId = new Guid("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"),
                    Title = "Engineering Manager",
                    Description = "Managing engineering team and delivery execution",
                    Expectations = "Manage team performance, deliver projects, develop engineers, align with goals"
                },
                new Position
                {
                    Id = new Guid("ab42e7b3-caf3-417d-80f9-62422e089596"),
                    CareerTrackId = new Guid("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"),
                    Title = "Senior Engineering Manager",
                    Description = "Managing multiple teams and cross-functional initiatives",
                    Expectations = "Lead multiple teams, drive cross-functional work, scale engineering practices"
                },
                new Position
                {
                    Id = new Guid("8ef2121e-e2c5-4514-b16a-dda651fb1c50"),
                    CareerTrackId = new Guid("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"),
                    Title = "Engineering Director",
                    Description = "Managing managers and department strategy",
                    Expectations = "Manage managers, set department strategy, align with company objectives"
                },
                new Position
                {
                    Id = new Guid("f2208214-d7c6-4db4-98ea-99e097576ef8"),
                    CareerTrackId = new Guid("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"),
                    Title = "Senior Engineering Director",
                    Description = "Managing multiple departments and organizational strategy",
                    Expectations = "Lead multiple departments, set organizational strategy, drive transformation"
                },
                new Position
                {
                    Id = new Guid("aa00c283-1af1-4397-bc34-2f674bda4852"),
                    CareerTrackId = new Guid("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"),
                    Title = "VP of Engineering",
                    Description = "Executive leadership for entire engineering organization",
                    Expectations = "Executive leadership, set engineering vision, align with business strategy"
                }
            };

            await _context.Positions.AddRangeAsync(positions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {PositionCount} positions.", positions.Length);
        }

        private async Task SeedSkillsAndCategoriesAsync(bool forceSeed = false)
        {
            if (!forceSeed && (await _context.SkillCategories.AnyAsync() || await _context.Skills.AnyAsync()))
            {
                _logger.LogInformation("Skill categories and skills already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding skill categories and skills...");
                // Remove existing data first
                await _context.Skills.ExecuteDeleteAsync();
                await _context.SkillCategories.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding skill categories and skills...");
            }

            var categories = new[]
            {
                new SkillCategory
                {
                    Id = new Guid("ccc11111-1111-1111-1111-111111111111"),
                    Title = "Technical",
                    Description = "Technical skills and competencies"
                },
                new SkillCategory
                {
                    Id = new Guid("ccc22222-2222-2222-2222-222222222222"),
                    Title = "Leadership",
                    Description = "Leadership and communication skills"
                },
                new SkillCategory
                {
                    Id = new Guid("ccc33333-3333-3333-3333-333333333333"),
                    Title = "Business",
                    Description = "Business acumen and domain knowledge"
                },
                new SkillCategory
                {
                    Id = new Guid("ccc44444-4444-4444-4444-444444444444"),
                    Title = "Soft Skills",
                    Description = "Interpersonal and behavioral skills"
                },
                new SkillCategory
                {
                    Id = new Guid("ccc55555-5555-5555-5555-555555555555"),
                    Title = "Domain Specific",
                    Description = "Industry or domain-specific knowledge"
                }
            };

            await _context.SkillCategories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            var technicalCategory = categories[0];
            var leadershipCategory = categories[1];
            var businessCategory = categories[2];
            var softSkillsCategory = categories[3];
            var domainSpecificCategory = categories[4];

            var skills = new[]
            {
                // Technical Skills
                new Skill
                {
                    Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"),
                    Title = "Software Development",
                    Description = "Ability to design, develop, and maintain software applications",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("07f11bdd-82cf-4b79-ad78-70eb1e772e19"),
                    Title = "System Architecture",
                    Description = "Designing scalable and maintainable system architectures",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("18adb648-5c95-4c45-a8a3-c5174c385215"),
                    Title = "DevOps & CI/CD",
                    Description = "Implementing and managing continuous integration and deployment pipelines",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("3f4e5d6c-7b8a-9c0d-1e2f-3a4b5c6d7e8f"),
                    Title = "Cloud Technologies",
                    Description = "Expertise in cloud platforms (AWS, Azure, GCP) and cloud-native architectures",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("4a5b6c7d-8e9f-0a1b-2c3d-4e5f6a7b8c9d"),
                    Title = "Security Engineering",
                    Description = "Implementing security controls, conducting assessments, and managing vulnerabilities",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("5b6c7d8e-9f0a-1b2c-3d4e-5f6a7b8c9d0e"),
                    Title = "Data Engineering",
                    Description = "Building and maintaining data pipelines, ETL processes, and data infrastructure",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"),
                    Title = "Machine Learning",
                    Description = "Developing ML models, training algorithms, and deploying AI solutions",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"),
                    Title = "Testing & Quality Assurance",
                    Description = "Creating test strategies, writing tests, and ensuring software quality",
                    CategoryId = technicalCategory.Id
                },
                
                // Leadership Skills
                new Skill
                {
                    Id = new Guid("08a22cea-ab17-4a3f-9645-23305553152c"),
                    Title = "Communication and Influence",
                    Description = "Effectively communicating ideas and influencing stakeholders at all levels",
                    CategoryId = leadershipCategory.Id
                },
                new Skill
                {
                    Id = new Guid("199f28ba-4b05-42bd-b0d8-edda9cef4181"),
                    Title = "Stakeholder Management",
                    Description = "Managing relationships with stakeholders and aligning expectations",
                    CategoryId = leadershipCategory.Id
                },
                new Skill
                {
                    Id = new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"),
                    Title = "Team Leadership",
                    Description = "Leading, mentoring, and developing high-performing teams",
                    CategoryId = leadershipCategory.Id
                },
                new Skill
                {
                    Id = new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"),
                    Title = "Strategic Thinking",
                    Description = "Developing long-term strategies and aligning them with organizational goals",
                    CategoryId = leadershipCategory.Id
                },
                new Skill
                {
                    Id = new Guid("0a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"),
                    Title = "Change Management",
                    Description = "Leading organizational change and managing transformation initiatives",
                    CategoryId = leadershipCategory.Id
                },
                
                // Business Skills
                new Skill
                {
                    Id = new Guid("087eca69-033b-4542-aef1-8f2756ccc562"),
                    Title = "Decision Making",
                    Description = "Making informed decisions based on data, analysis, and business context",
                    CategoryId = businessCategory.Id
                },
                new Skill
                {
                    Id = new Guid("31676cb5-e435-49f5-b0ea-99db874aa3fa"),
                    Title = "Business Acumen",
                    Description = "Understanding business operations, market dynamics, and financial impact",
                    CategoryId = businessCategory.Id
                },
                new Skill
                {
                    Id = new Guid("1b2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"),
                    Title = "Product Management",
                    Description = "Defining product vision, roadmap, and prioritizing features based on user needs",
                    CategoryId = businessCategory.Id
                },
                new Skill
                {
                    Id = new Guid("2c3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f"),
                    Title = "Project Management",
                    Description = "Planning, executing, and delivering projects on time and within budget",
                    CategoryId = businessCategory.Id
                },
                
                // Soft Skills
                new Skill
                {
                    Id = new Guid("084c859d-23f8-4de3-b020-6399d2ed6623"),
                    Title = "Problem Solving",
                    Description = "Analyzing complex problems and developing effective solutions",
                    CategoryId = softSkillsCategory.Id
                },
                new Skill
                {
                    Id = new Guid("189d66ee-95e1-4672-b154-c19dccdcaf4c"),
                    Title = "Personal Accountability",
                    Description = "Taking ownership of responsibilities and delivering on commitments",
                    CategoryId = softSkillsCategory.Id
                },
                new Skill
                {
                    Id = new Guid("3d4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a"),
                    Title = "Collaboration",
                    Description = "Working effectively with cross-functional teams and diverse stakeholders",
                    CategoryId = softSkillsCategory.Id
                },
                new Skill
                {
                    Id = new Guid("4e5f6a7b-8c9d-0e1f-2a3b-4c5d6e7f8a9b"),
                    Title = "Adaptability",
                    Description = "Adjusting to changing priorities and thriving in dynamic environments",
                    CategoryId = softSkillsCategory.Id
                },
                new Skill
                {
                    Id = new Guid("5f6a7b8c-9d0e-1f2a-3b4c-5d6e7f8a9b0c"),
                    Title = "Continuous Learning",
                    Description = "Commitment to ongoing professional development and skill enhancement",
                    CategoryId = softSkillsCategory.Id
                },
                
                // Domain Specific Skills
                new Skill
                {
                    Id = new Guid("10c1293d-63e7-43ac-8933-7e7e611577a9"),
                    Title = "Industry Knowledge",
                    Description = "Deep understanding of industry trends, regulations, and best practices",
                    CategoryId = domainSpecificCategory.Id
                },
                new Skill
                {
                    Id = new Guid("0480319f-144f-4758-ad4d-22091680a0bb"),
                    Title = "Domain Expertise",
                    Description = "Specialized knowledge in specific business domains or technical areas",
                    CategoryId = domainSpecificCategory.Id
                },
                new Skill
                {
                    Id = new Guid("6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c1d"),
                    Title = "Regulatory Compliance",
                    Description = "Understanding and ensuring compliance with relevant regulations and standards",
                    CategoryId = domainSpecificCategory.Id
                }
            };

            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Seed skill levels for each skill
            var skillLevels = new List<SkillLevel>();
            foreach (var skill in skills)
            {
                skillLevels.AddRange(new[]
                {
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Beginner",
                        Description = "Basic awareness and initial exposure to the skill",
                        SkillId = skill.Id,
                        Value = 1
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Foundational",
                        Description = "Developing competency with guided practice and support",
                        SkillId = skill.Id,
                        Value = 2
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Intermediate",
                        Description = "Solid understanding with practical application and experience",
                        SkillId = skill.Id,
                        Value = 3
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Advanced",
                        Description = "Deep expertise with ability to solve complex problems independently",
                        SkillId = skill.Id,
                        Value = 4
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Expert",
                        Description = "Recognized authority with innovative contributions and thought leadership",
                        SkillId = skill.Id,
                        Value = 5
                    }
                });
            }

            await _context.SkillLevels.AddRangeAsync(skillLevels);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {CategoryCount} skill categories, {SkillCount} skills, and {LevelCount} skill levels.",
                categories.Length, skills.Length, skillLevels.Count);
        }

        private async Task SeedProjectsAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Projects.AnyAsync())
            {
                _logger.LogInformation("Projects already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding projects...");
                // Remove existing projects first
                await _context.Projects.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding projects...");
            }

            var projects = new[]
            {
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777701"),
                    Code = "PRJ-001",
                    Title = "Career Progression System",
                    Description = "Development of the career progression and review system for employee growth"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777702"),
                    Code = "PRJ-002",
                    Title = "Customer Portal Redesign",
                    Description = "Modernization of the customer-facing portal with improved UX"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777703"),
                    Code = "PRJ-003",
                    Title = "Mobile App Development",
                    Description = "Native mobile applications for iOS and Android platforms"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777704"),
                    Code = "PRJ-004",
                    Title = "Data Analytics Platform",
                    Description = "Enterprise data warehouse and analytics infrastructure"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777705"),
                    Code = "PRJ-005",
                    Title = "Cloud Migration Initiative",
                    Description = "Migration of legacy systems to cloud infrastructure"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777706"),
                    Code = "PRJ-006",
                    Title = "API Gateway Implementation",
                    Description = "Centralized API management and security platform"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777707"),
                    Code = "PRJ-007",
                    Title = "E-Commerce Platform",
                    Description = "Full-featured online shopping and payment processing system"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777708"),
                    Code = "PRJ-008",
                    Title = "DevOps Automation",
                    Description = "CI/CD pipeline automation and infrastructure as code"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777709"),
                    Code = "PRJ-009",
                    Title = "Security Compliance Framework",
                    Description = "Enterprise security and compliance monitoring system"
                },
                new Project
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777710"),
                    Code = "PRJ-010",
                    Title = "AI/ML Research Platform",
                    Description = "Machine learning experimentation and model deployment platform"
                }
            };

            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} projects.", projects.Length);
        }

        private async Task SeedLocationsAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Locations.AnyAsync())
            {
                _logger.LogInformation("Locations already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding locations...");
                // Remove existing locations first
                await _context.Locations.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding locations...");
            }

            var locations = new[]
            {
                new Location
                {
                    Id = new Guid("eee11111-1111-1111-1111-111111111111"),
                    Name = "Headquarters",
                    Address = "123 Main Street",
                    City = "San Francisco",
                    Region = "California",
                    Country = "USA",
                    PostalCode = "94105",
                    Timezone = "America/Los_Angeles",
                    ContactPhone = "+1-415-555-0100"
                },
                new Location
                {
                    Id = new Guid("eee22222-2222-2222-2222-222222222222"),
                    Name = "Remote",
                    Country = "Various",
                    Timezone = "UTC"
                },
                new Location
                {
                    Id = new Guid("eee33333-3333-3333-3333-333333333333"),
                    Name = "New York Office",
                    Address = "456 5th Avenue",
                    City = "New York",
                    Region = "New York",
                    Country = "USA",
                    PostalCode = "10001",
                    Timezone = "America/New_York",
                    ContactPhone = "+1-212-555-0200"
                },
                new Location
                {
                    Id = new Guid("eee44444-4444-4444-4444-444444444444"),
                    Name = "London Office",
                    Address = "789 Oxford Street",
                    City = "London",
                    Region = "Greater London",
                    Country = "United Kingdom",
                    PostalCode = "W1D 1NN",
                    Timezone = "Europe/London",
                    ContactPhone = "+44-20-7946-0958"
                },
                new Location
                {
                    Id = new Guid("eee55555-5555-5555-5555-555555555555"),
                    Name = "Tokyo Office",
                    Address = "101 Shibuya Crossing",
                    City = "Tokyo",
                    Region = "Kanto",
                    Country = "Japan",
                    PostalCode = "150-0043",
                    Timezone = "Asia/Tokyo",
                    ContactPhone = "+81-3-5555-0300"
                }
            };

            await _context.Locations.AddRangeAsync(locations);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} locations.", locations.Length);
        }

        private async Task SeedProjectRolesAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.ProjectRoles.AnyAsync())
            {
                _logger.LogInformation("Project roles already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding project roles...");
                // Remove existing project roles first
                await _context.ProjectRoles.ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding project roles...");
            }

            // Create project roles for all 10 projects
            var projectRoles = new List<ProjectRole>();

            // Define project IDs
            var projectIds = new[]
            {
                new Guid("77777777-7777-7777-7777-777777777701"),
                new Guid("77777777-7777-7777-7777-777777777702"),
                new Guid("77777777-7777-7777-7777-777777777703"),
                new Guid("77777777-7777-7777-7777-777777777704"),
                new Guid("77777777-7777-7777-7777-777777777705"),
                new Guid("77777777-7777-7777-7777-777777777706"),
                new Guid("77777777-7777-7777-7777-777777777707"),
                new Guid("77777777-7777-7777-7777-777777777708"),
                new Guid("77777777-7777-7777-7777-777777777709"),
                new Guid("77777777-7777-7777-7777-777777777710")
            };

            // Common role templates
            var roleTemplates = new[]
            {
                ("Tech Lead", "Technical leadership and architecture decisions"),
                ("Product Manager", "Product vision, roadmap, and stakeholder management"),
                ("Software Engineer", "Software development and implementation"),
                ("QA Engineer", "Quality assurance, testing, and validation"),
                ("UX Designer", "User experience design and research"),
                ("DevOps Engineer", "Infrastructure, deployment, and operations"),
                ("Data Analyst", "Data analysis, reporting, and insights"),
                ("Business Analyst", "Requirements gathering and business process analysis"),
                ("Scrum Master", "Agile process facilitation and team coordination"),
                ("Security Engineer", "Security assessment, implementation, and compliance")
            };

            // Create 10 roles for each project (100 total)
            int counter = 1;
            foreach (var projectId in projectIds)
            {
                foreach (var (title, description) in roleTemplates)
                {
                    projectRoles.Add(new ProjectRole
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        Title = title,
                        Description = description
                    });
                    counter++;
                }
            }

            await _context.ProjectRoles.AddRangeAsync(projectRoles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} project roles.", projectRoles.Count);
        }

        private async Task SeedPositionToSkillsAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Set<PositionToSkill>().AnyAsync())
            {
                _logger.LogInformation("Position-to-skill mappings already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding position-to-skill mappings...");
                await _context.Set<PositionToSkill>().ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding position-to-skill mappings...");
            }

            // Get all positions and skills
            var positions = await _context.Positions.ToListAsync();
            var skills = await _context.Skills.ToListAsync();
            var skillLevels = await _context.SkillLevels.ToListAsync();

            // Helper function to get skill level by skill and level title
            Guid GetSkillLevelId(Guid skillId, string levelTitle)
            {
                var skillLevel = skillLevels.FirstOrDefault(sl => sl.SkillId == skillId && sl.Title == levelTitle);
                if (skillLevel == null)
                {
                    var skill = skills.FirstOrDefault(s => s.Id == skillId);
                    throw new InvalidOperationException($"Skill level not found: skill='{skill?.Title ?? skillId.ToString()}', level='{levelTitle}'. Available levels: {string.Join(", ", skillLevels.Where(sl => sl.SkillId == skillId).Select(sl => sl.Title))}");
                }
                return skillLevel.Id;
            }

            // Helper function to get skill by title
            Guid GetSkillId(string skillTitle)
            {
                var skill = skills.FirstOrDefault(s => s.Title == skillTitle);
                if (skill == null)
                {
                    throw new InvalidOperationException($"Skill not found: '{skillTitle}'. Available skills: {string.Join(", ", skills.Select(s => s.Title).OrderBy(t => t))}");
                }
                return skill.Id;
            }

            var positionToSkills = new List<PositionToSkill>();

            // Core skills required for ALL positions with varying levels
            foreach (var position in positions)
            {
                var positionTitle = position.Title.ToLower();

                // Determine seniority level based on position title
                bool isEntry = positionTitle.Contains("engineer") && !positionTitle.Contains("senior") &&
                              !positionTitle.Contains("staff") && !positionTitle.Contains("principal") &&
                              !positionTitle.Contains("director") && !positionTitle.Contains("lead") &&
                              !positionTitle.Contains("manager");
                bool isSenior = positionTitle.Contains("senior") && !positionTitle.Contains("director");
                bool isStaff = positionTitle.Contains("staff") || positionTitle.Contains("lead");
                bool isPrincipal = positionTitle.Contains("principal");
                bool isDirector = positionTitle.Contains("director") || positionTitle.Contains("vp") ||
                                 positionTitle.Contains("manager");

                // Decision Making - required for all, level increases with seniority
                string decisionMakingLevel = isEntry ? "Foundational" :
                                            isSenior ? "Intermediate" :
                                            isStaff ? "Advanced" :
                                            isPrincipal || isDirector ? "Expert" : "Beginner";

                positionToSkills.Add(new PositionToSkill
                {
                    Id = Guid.NewGuid(),
                    PositionId = position.Id,
                    SkillId = GetSkillId("Decision Making"),
                    SkillLevelId = GetSkillLevelId(GetSkillId("Decision Making"), decisionMakingLevel),
                    Weight = 1.0m,
                    IsMandatory = true,
                    Rationale = "Critical for making sound technical and business decisions"
                });

                // Communication and Influence - required for all, increases with seniority
                string communicationLevel = isEntry ? "Foundational" :
                                          isSenior ? "Intermediate" :
                                          isStaff ? "Advanced" :
                                          isPrincipal || isDirector ? "Expert" : "Beginner";

                positionToSkills.Add(new PositionToSkill
                {
                    Id = Guid.NewGuid(),
                    PositionId = position.Id,
                    SkillId = GetSkillId("Communication and Influence"),
                    SkillLevelId = GetSkillLevelId(GetSkillId("Communication and Influence"), communicationLevel),
                    Weight = 1.0m,
                    IsMandatory = true,
                    Rationale = "Essential for effective collaboration and stakeholder engagement"
                });

                // Problem Solving - required for all technical roles
                string problemSolvingLevel = isEntry ? "Foundational" :
                                            isSenior ? "Intermediate" :
                                            isStaff ? "Advanced" :
                                            isPrincipal || isDirector ? "Expert" : "Beginner";

                positionToSkills.Add(new PositionToSkill
                {
                    Id = Guid.NewGuid(),
                    PositionId = position.Id,
                    SkillId = GetSkillId("Problem Solving"),
                    SkillLevelId = GetSkillLevelId(GetSkillId("Problem Solving"), problemSolvingLevel),
                    Weight = 1.0m,
                    IsMandatory = true,
                    Rationale = "Core ability to analyze and solve complex challenges"
                });

                // Stakeholder Management - more important at higher levels
                if (!isEntry)
                {
                    string stakeholderLevel = isSenior ? "Intermediate" :
                                             isStaff ? "Advanced" :
                                             isPrincipal || isDirector ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Stakeholder Management"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Stakeholder Management"), stakeholderLevel),
                        Weight = 0.9m,
                        IsMandatory = true,
                        Rationale = "Managing relationships and aligning expectations with stakeholders"
                    });
                }

                // Personal Accountability - required for all
                string accountabilityLevel = isEntry ? "Foundational" :
                                            isSenior ? "Intermediate" :
                                            isStaff || isPrincipal || isDirector ? "Advanced" : "Beginner";

                positionToSkills.Add(new PositionToSkill
                {
                    Id = Guid.NewGuid(),
                    PositionId = position.Id,
                    SkillId = GetSkillId("Personal Accountability"),
                    SkillLevelId = GetSkillLevelId(GetSkillId("Personal Accountability"), accountabilityLevel),
                    Weight = 1.0m,
                    IsMandatory = true,
                    Rationale = "Taking ownership and delivering on commitments"
                });

                // Collaboration - required for all
                string collaborationLevel = isEntry ? "Foundational" :
                                          isSenior ? "Intermediate" :
                                          isStaff ? "Advanced" :
                                          isPrincipal || isDirector ? "Expert" : "Beginner";

                positionToSkills.Add(new PositionToSkill
                {
                    Id = Guid.NewGuid(),
                    PositionId = position.Id,
                    SkillId = GetSkillId("Collaboration"),
                    SkillLevelId = GetSkillLevelId(GetSkillId("Collaboration"), collaborationLevel),
                    Weight = 0.9m,
                    IsMandatory = true,
                    Rationale = "Working effectively with cross-functional teams"
                });

                // Add role-specific technical skills
                if (positionTitle.Contains("devops") || positionTitle.Contains("software delivery"))
                {
                    // DevOps & CI/CD
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("DevOps & CI/CD"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("DevOps & CI/CD"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core competency for DevOps and software delivery roles"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Cloud Technologies"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Cloud Technologies"), techLevel),
                        Weight = 0.95m,
                        IsMandatory = true,
                        Rationale = "Essential for modern DevOps practices"
                    });
                }
                else if (positionTitle.Contains("mobile"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Software Development"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Software Development"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core development skills for mobile applications"
                    });
                }
                else if (positionTitle.Contains("frontend"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Software Development"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Software Development"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core development skills for frontend engineering"
                    });
                }
                else if (positionTitle.Contains("backend"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Software Development"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Software Development"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core development skills for backend engineering"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("System Architecture"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("System Architecture"),
                                                       isSenior ? "Intermediate" :
                                                       isStaff ? "Advanced" :
                                                       isPrincipal ? "Expert" : "Foundational"),
                        Weight = 0.9m,
                        IsMandatory = isSenior || isStaff || isPrincipal,
                        Rationale = "Important for designing scalable backend systems"
                    });
                }
                else if (positionTitle.Contains("ml") || positionTitle.Contains("machine learning"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Machine Learning"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Machine Learning"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core competency for ML engineering roles"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Data Engineering"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Data Engineering"),
                                                       isSenior ? "Intermediate" :
                                                       isStaff || isPrincipal ? "Advanced" : "Foundational"),
                        Weight = 0.9m,
                        IsMandatory = true,
                        Rationale = "Essential for ML data pipeline and model training"
                    });
                }
                else if (positionTitle.Contains("support"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Testing & Quality Assurance"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Testing & Quality Assurance"), techLevel),
                        Weight = 0.9m,
                        IsMandatory = true,
                        Rationale = "Important for diagnosing and resolving technical issues"
                    });
                }
                else if (positionTitle.Contains("infrastructure"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Cloud Technologies"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Cloud Technologies"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core competency for infrastructure engineering"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("System Architecture"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("System Architecture"), techLevel),
                        Weight = 0.95m,
                        IsMandatory = true,
                        Rationale = "Essential for designing infrastructure architecture"
                    });
                }
                else if (positionTitle.Contains("security"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Security Engineering"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Security Engineering"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core competency for security engineering roles"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Regulatory Compliance"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Regulatory Compliance"),
                                                       isSenior ? "Intermediate" :
                                                       isStaff || isPrincipal ? "Advanced" : "Foundational"),
                        Weight = 0.85m,
                        IsMandatory = true,
                        Rationale = "Important for security compliance and risk management"
                    });
                }
                else if (positionTitle.Contains("platform"))
                {
                    string techLevel = isEntry ? "Intermediate" :
                                      isSenior ? "Advanced" :
                                      isStaff || isPrincipal ? "Expert" : "Foundational";

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("System Architecture"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("System Architecture"), techLevel),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Core competency for platform engineering"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Cloud Technologies"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Cloud Technologies"), techLevel),
                        Weight = 0.95m,
                        IsMandatory = true,
                        Rationale = "Essential for building modern platform services"
                    });
                }

                // Add leadership skills for management positions
                if (isDirector || positionTitle.Contains("manager"))
                {
                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Team Leadership"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Team Leadership"),
                                                       positionTitle.Contains("vp") ? "Expert" : "Advanced"),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Critical for leading and developing teams"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Strategic Thinking"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Strategic Thinking"),
                                                       positionTitle.Contains("vp") || positionTitle.Contains("senior director") ? "Expert" : "Advanced"),
                        Weight = 1.0m,
                        IsMandatory = true,
                        Rationale = "Essential for setting direction and strategy"
                    });

                    positionToSkills.Add(new PositionToSkill
                    {
                        Id = Guid.NewGuid(),
                        PositionId = position.Id,
                        SkillId = GetSkillId("Business Acumen"),
                        SkillLevelId = GetSkillLevelId(GetSkillId("Business Acumen"), "Advanced"),
                        Weight = 0.9m,
                        IsMandatory = true,
                        Rationale = "Important for aligning technical work with business objectives"
                    });
                }

                // Add continuous learning for all
                positionToSkills.Add(new PositionToSkill
                {
                    Id = Guid.NewGuid(),
                    PositionId = position.Id,
                    SkillId = GetSkillId("Continuous Learning"),
                    SkillLevelId = GetSkillLevelId(GetSkillId("Continuous Learning"),
                                                   isDirector ? "Advanced" : "Intermediate"),
                    Weight = 0.8m,
                    IsMandatory = false,
                    Rationale = "Important for staying current with technology and practices"
                });
            }

            await _context.Set<PositionToSkill>().AddRangeAsync(positionToSkills);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} position-to-skill mappings for {PositionCount} positions.",
                                  positionToSkills.Count, positions.Count);
        }

        private async Task SeedEmployeeToSkillsAsync(bool forceSeed = false)
        {
            if (!forceSeed && await _context.Set<EmployeeToSkill>().AnyAsync())
            {
                _logger.LogInformation("Employee-to-skill mappings already exist, skipping seeding.");
                return;
            }

            if (forceSeed)
            {
                _logger.LogInformation("Force seeding employee-to-skill mappings...");
                await _context.Set<EmployeeToSkill>().ExecuteDeleteAsync();
            }
            else
            {
                _logger.LogInformation("Seeding employee-to-skill mappings...");
            }

            // Get all employees with their positions
            var employees = await _context.Employees
                .Include(e => e.Position)
                .Where(e => !e.IsDeleted)
                .ToListAsync();

            // Get all skills and skill levels
            var skills = await _context.Skills.ToListAsync();
            var skillLevels = await _context.SkillLevels.ToListAsync();

            // Get position-to-skill mappings to determine what skills each position requires
            var positionToSkills = await _context.Set<PositionToSkill>().ToListAsync();

            var employeeToSkills = new List<EmployeeToSkill>();
            var random = new Random(42); // Fixed seed for reproducibility

            foreach (var employee in employees)
            {
                // Get required skills for this employee's position
                var requiredSkills = positionToSkills
                    .Where(pts => pts.PositionId == employee.PositionId)
                    .ToList();

                foreach (var requiredSkill in requiredSkills)
                {
                    // Determine employee's current skill level (simulate realistic skill development)
                    // Most employees will be at or slightly below the required level for their position
                    var requiredLevel = skillLevels.First(sl => sl.Id == requiredSkill.SkillLevelId);
                    var requiredLevelValue = requiredLevel.Value;

                    // 60% chance at required level, 30% one level below, 10% one level above
                    var rnd = random.NextDouble();
                    int actualLevelValue;

                    if (rnd < 0.6)
                    {
                        // At required level
                        actualLevelValue = requiredLevelValue;
                    }
                    else if (rnd < 0.9)
                    {
                        // One level below (but not below 1)
                        actualLevelValue = Math.Max(1, requiredLevelValue - 1);
                    }
                    else
                    {
                        // One level above (but not above 5)
                        actualLevelValue = Math.Min(5, requiredLevelValue + 1);
                    }

                    // Find the skill level entity for this skill and value
                    var actualSkillLevel = skillLevels
                        .FirstOrDefault(sl => sl.SkillId == requiredSkill.SkillId && sl.Value == actualLevelValue);

                    if (actualSkillLevel != null)
                    {
                        employeeToSkills.Add(new EmployeeToSkill
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = employee.Id,
                            SkillId = requiredSkill.SkillId,
                            SkillLevelId = actualSkillLevel.Id,
                            Source = "System Seeded",
                            EffectiveDate = DateTime.UtcNow.AddMonths(-random.Next(1, 12)), // Skill acquired within last year
                            IsTarget = false // Current skill level, not a target
                        });
                    }
                }

                // Add some additional skills (not required for position) for well-rounded employees
                // Select 2-3 random skills from their skill category
                var positionTitle = employee.Position?.Title?.ToLower() ?? "";
                var additionalSkillCount = random.Next(2, 4);

                // Identify relevant additional skills based on position
                var additionalSkillTitles = new List<string>();

                if (positionTitle.Contains("engineer") || positionTitle.Contains("developer"))
                {
                    additionalSkillTitles.AddRange(new[] { "Adaptability", "Continuous Learning", "Business Acumen" });
                }
                else if (positionTitle.Contains("manager") || positionTitle.Contains("director"))
                {
                    additionalSkillTitles.AddRange(new[] { "Change Management", "Business Acumen", "Product Management" });
                }
                else
                {
                    additionalSkillTitles.AddRange(new[] { "Adaptability", "Business Acumen", "Continuous Learning" });
                }

                var additionalSkills = skills
                    .Where(s => additionalSkillTitles.Contains(s.Title))
                    .OrderBy(s => random.Next())
                    .Take(additionalSkillCount)
                    .ToList();

                foreach (var additionalSkill in additionalSkills)
                {
                    // Check if we already added this skill
                    if (employeeToSkills.Any(ets => ets.EmployeeId == employee.Id && ets.SkillId == additionalSkill.Id))
                        continue;

                    // Additional skills are usually at Foundational or Intermediate level
                    var levelValue = random.Next(2, 4); // 2=Foundational, 3=Intermediate
                    var skillLevel = skillLevels
                        .FirstOrDefault(sl => sl.SkillId == additionalSkill.Id && sl.Value == levelValue);

                    if (skillLevel != null)
                    {
                        employeeToSkills.Add(new EmployeeToSkill
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = employee.Id,
                            SkillId = additionalSkill.Id,
                            SkillLevelId = skillLevel.Id,
                            Source = "System Seeded",
                            EffectiveDate = DateTime.UtcNow.AddMonths(-random.Next(3, 24)),
                            IsTarget = false
                        });
                    }
                }

                // Add target skills (aspirational growth areas) - 1-2 skills at higher levels
                var targetSkillCount = random.Next(1, 3);
                var existingSkillIds = employeeToSkills
                    .Where(ets => ets.EmployeeId == employee.Id)
                    .Select(ets => ets.SkillId)
                    .ToList();

                var targetSkills = requiredSkills
                    .Where(rs => rs.IsMandatory)
                    .OrderBy(rs => random.Next())
                    .Take(targetSkillCount)
                    .ToList();

                foreach (var targetSkill in targetSkills)
                {
                    // Target level is 1-2 levels above current required level
                    var currentSkillLevel = skillLevels.First(sl => sl.Id == targetSkill.SkillLevelId);
                    var targetLevelValue = Math.Min(5, currentSkillLevel.Value + random.Next(1, 3));

                    var targetSkillLevel = skillLevels
                        .FirstOrDefault(sl => sl.SkillId == targetSkill.SkillId && sl.Value == targetLevelValue);

                    if (targetSkillLevel != null)
                    {
                        employeeToSkills.Add(new EmployeeToSkill
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = employee.Id,
                            SkillId = targetSkill.SkillId,
                            SkillLevelId = targetSkillLevel.Id,
                            Source = "Career Development Plan",
                            EffectiveDate = DateTime.UtcNow.AddMonths(6), // Target 6 months from now
                            IsTarget = true // This is a growth target
                        });
                    }
                }
            }

            await _context.Set<EmployeeToSkill>().AddRangeAsync(employeeToSkills);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} employee-to-skill mappings for {EmployeeCount} employees.",
                                  employeeToSkills.Count, employees.Count);
        }
    }
}
