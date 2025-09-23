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
        /// Seeds initial data if the database is empty
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting database seeding...");

            await SeedDepartmentsAsync();
            await SeedUsersAndEmployeesAsync();
            await SeedCareerPathsAndTracksAsync();
            await SeedSkillsAndCategoriesAsync();
            await SeedProjectsAsync();

            _logger.LogInformation("Database seeding completed.");
        }

        private async Task SeedDepartmentsAsync()
        {
            if (await _context.Departments.AnyAsync())
            {
                _logger.LogInformation("Departments already exist, skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding departments...");

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

        private async Task SeedUsersAndEmployeesAsync()
        {
            if (await _context.Users.AnyAsync() || await _context.Employees.AnyAsync())
            {
                _logger.LogInformation("Users and employees already exist, skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding users and employees...");

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

            // Create users
            var users = new[]
            {
                new User
                {
                    Id = new Guid("679add6e-6c29-4e00-b6a5-b69c8e0f3445"),
                    UserName = "john.doe",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "John Doe"
                },
                new User
                {
                    Id = new Guid("c6874b28-e2fa-4835-8e8f-159bd5067091"),
                    UserName = "jane.smith",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Jane Smith"
                },
                new User
                {
                    Id = new Guid("45f0eaae-b3eb-4261-a430-4d9e94ec8e0d"),
                    UserName = "bob.johnson",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Bob Johnson"
                },
                new User
                {
                    Id = new Guid("e9741b9b-3c66-4462-af46-297810b29403"),
                    UserName = "alice.wilson",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Alice Wilson"
                },
                new User
                {
                    Id = new Guid("d670f2cf-66a6-4cb6-947f-062c7b089c8d"),
                    UserName = "charlie.brown",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Charlie Brown"
                },
                new User
                {
                    Id = new Guid("bf428236-361c-4ade-995d-21a62feec86f"),
                    UserName = "diana.prince",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Diana Prince"
                },
                new User
                {
                    Id = new Guid("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"),
                    UserName = "eve.adams",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Eve Adams"
                },
                new User
                {
                    Id = new Guid("7567ad7a-174e-461c-bd88-e7489db10317"),
                    UserName = "frank.miller",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Frank Miller"
                },
                new User
                {
                    Id = new Guid("5d70d7d5-570e-46fe-91ce-2d6081b365aa"),
                    UserName = "grace.lee",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Grace Lee"
                },
                new User
                {
                    Id = new Guid("977f4f1f-b3ce-4244-98fc-2c0d0248de88"),
                    UserName = "henry.wilson",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Henry Wilson"
                },
                new User
                {
                    Id = new Guid("20c78aa6-077a-4d3b-a7c5-84d561ec3975"),
                    UserName = "iris.davis",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Iris Davis"
                },
                new User
                {
                    Id = new Guid("87896109-9ac7-444c-aa79-dfe1dc908a5d"),
                    UserName = "jack.thompson",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Jack Thompson"
                },
                new User
                {
                    Id = new Guid("81694c14-a96a-4625-b5e5-a9fd034021af"),
                    UserName = "kate.garcia",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Kate Garcia"
                },
                new User
                {
                    Id = new Guid("7522f469-e697-4e02-bb2f-f7f055c9aacb"),
                    UserName = "liam.anderson",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Liam Anderson"
                },
                new User
                {
                    Id = new Guid("bf694c99-cfa6-4fba-be98-efba28bb4f31"),
                    UserName = "mia.rodriguez",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Mia Rodriguez"
                },
                new User
                {
                    Id = new Guid("879c8ae6-c1c0-4d16-85fb-5dfe698fd108"),
                    UserName = "noah.martinez",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Noah Martinez"
                },
                new User
                {
                    Id = new Guid("88b6f0d3-298e-4083-84cf-fa7893a0c846"),
                    UserName = "olivia.lopez",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Olivia Lopez"
                },
                new User
                {
                    Id = new Guid("ad92cd5e-5599-4f3c-b2e1-39a49dd6b5bc"),
                    UserName = "peter.gonzalez",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Peter Gonzalez"
                },
                new User
                {
                    Id = new Guid("cc230776-dc23-4996-9383-15fee9688215"),
                    UserName = "quinn.hernandez",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Quinn Hernandez"
                },
                new User
                {
                    Id = new Guid("5950a2be-bdfb-4dcb-9913-1e3e0e022a5c"),
                    UserName = "ryan.king",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Ryan King"
                },
                new User
                {
                    Id = new Guid("a22e8c3a-4ae0-4b59-b766-89a226fa82f0"),
                    UserName = "sara.wright",
                    PasswordHash = "hashed_password_placeholder",
                    DisplayName = "Sara Wright"
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Create employees linked to users
            var employees = new[]
            {
                new Employee
                {
                    Id = new Guid("004e1f8b-1ea3-4e27-a373-ed82f85147cc"),
                    UserId = users[0].Id,
                    Title = "Employee Role 43",
                    DepartmentId = financeDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0353f880-f993-4b3a-a7c2-41e7c58f0aa6"),
                    UserId = users[1].Id,
                    Title = "Employee Role 55",
                    DepartmentId = operationsDept?.Id
                },
                new Employee
                {
                    Id = new Guid("03c21dc8-d3cf-4d69-91fa-6e85c99c26e3"),
                    UserId = users[2].Id,
                    Title = "Employee Role 45",
                    DepartmentId = operationsDept?.Id
                },
                new Employee
                {
                    Id = new Guid("05532608-3d3f-4e0a-8a18-39565f10df9c"),
                    UserId = users[3].Id,
                    Title = "Employee Role 50",
                    DepartmentId = qaDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0a0bcaa0-7069-45e5-859d-86c0ac3c6804"),
                    UserId = users[4].Id,
                    Title = "Employee Role 69",
                    DepartmentId = securityDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0b1b37d7-776b-4564-8be1-0b3a38ef22e4"),
                    UserId = users[5].Id,
                    Title = "Employee Role 20",
                    DepartmentId = qaDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0b7715cf-b9ad-4da2-b4ab-b2c681ad078b"),
                    UserId = users[6].Id,
                    Title = "Employee Role 90",
                    DepartmentId = qaDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0b797477-9c18-4a58-ab94-b888a67db592"),
                    UserId = users[7].Id,
                    Title = "Employee Role 35",
                    DepartmentId = operationsDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0bdca7e8-6a57-4bc2-aeaa-4eea19fb6a3c"),
                    UserId = users[8].Id,
                    Title = "Employee Role 82",
                    DepartmentId = hrDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0c8b8b8b-8b8b-4b8b-8b8b-8b8b8b8b8b8b"),
                    UserId = users[9].Id,
                    Title = "Senior Software Engineer",
                    DepartmentId = engineeringDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0d9d9d9d-9d9d-4d9d-9d9d-9d9d9d9d9d9d"),
                    UserId = users[10].Id,
                    Title = "HR Manager",
                    DepartmentId = hrDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0e0e0e0e-0e0e-4e0e-0e0e-0e0e0e0e0e0e"),
                    UserId = users[11].Id,
                    Title = "Product Manager",
                    DepartmentId = productDept?.Id
                },
                new Employee
                {
                    Id = new Guid("0f0f0f0f-0f0f-4f0f-0f0f-0f0f0f0f0f0f"),
                    UserId = users[12].Id,
                    Title = "Legal Counsel",
                    DepartmentId = legalDept?.Id
                },
                new Employee
                {
                    Id = new Guid("10101010-1010-4101-0101-010101010101"),
                    UserId = users[13].Id,
                    Title = "Finance Analyst",
                    DepartmentId = financeDept?.Id
                },
                new Employee
                {
                    Id = new Guid("11111111-1111-4111-1111-111111111111"),
                    UserId = users[14].Id,
                    Title = "Marketing Specialist",
                    DepartmentId = marketingDept?.Id
                },
                new Employee
                {
                    Id = new Guid("12121212-1212-4121-2121-212121212121"),
                    UserId = users[15].Id,
                    Title = "Sales Representative",
                    DepartmentId = salesDept?.Id
                },
                new Employee
                {
                    Id = new Guid("13131313-1313-4131-3131-313131313131"),
                    UserId = users[16].Id,
                    Title = "Operations Manager",
                    DepartmentId = operationsDept?.Id
                },
                new Employee
                {
                    Id = new Guid("14141414-1414-4141-4141-414141414141"),
                    UserId = users[17].Id,
                    Title = "Security Analyst",
                    DepartmentId = securityDept?.Id
                },
                new Employee
                {
                    Id = new Guid("15151515-1515-4151-5151-515151515151"),
                    UserId = users[18].Id,
                    Title = "QA Engineer",
                    DepartmentId = qaDept?.Id
                },
                new Employee
                {
                    Id = new Guid("16161616-1616-4161-6161-616161616161"),
                    UserId = users[19].Id,
                    Title = "Software Engineer",
                    DepartmentId = engineeringDept?.Id,
                    ManagerId = users[9].Id // Reports to Senior Software Engineer
                }
            };

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {UserCount} users and {EmployeeCount} employees.", users.Length, employees.Length);
        }

        private async Task SeedCareerPathsAndTracksAsync()
        {
            if (await _context.CareerPaths.AnyAsync() || await _context.CareerTracks.AnyAsync())
            {
                _logger.LogInformation("Career paths and tracks already exist, skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding career paths and tracks...");

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

            // Add some key career tracks from the migration
            var careerTracks = new[]
            {
                new CareerTrack
                {
                    Id = new Guid("18fe3ad0-e6ea-4ad6-a18b-64ae97995443"),
                    Title = "Technology Track 1",
                    Description = "Specialized track 1 within technology career path",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("446aa37c-aa90-41a7-9f76-2dec7b5d3dfc"),
                    Title = "Technology Track 2",
                    Description = "Specialized track 2 within technology career path",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new CareerTrack
                {
                    Id = new Guid("4f86052f-9bc3-48e4-96a1-4b14b4284678"),
                    Title = "Technology Track 3",
                    Description = "Specialized track 3 within technology career path",
                    CareerPathId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                }
            };

            await _context.CareerTracks.AddRangeAsync(careerTracks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {PathCount} career paths and {TrackCount} career tracks.", careerPaths.Length, careerTracks.Length);
        }

        private async Task SeedSkillsAndCategoriesAsync()
        {
            if (await _context.SkillCategories.AnyAsync() || await _context.Skills.AnyAsync())
            {
                _logger.LogInformation("Skill categories and skills already exist, skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding skill categories and skills...");

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
                    Title = "Unit Testing",
                    Description = "Writing and maintaining unit tests",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("07f11bdd-82cf-4b79-ad78-70eb1e772e19"),
                    Title = "Technical Skill 12",
                    Description = "Description for technical skill 12",
                    CategoryId = technicalCategory.Id
                },
                new Skill
                {
                    Id = new Guid("18adb648-5c95-4c45-a8a3-c5174c385215"),
                    Title = "Technical Skill 28",
                    Description = "Description for technical skill 28",
                    CategoryId = technicalCategory.Id
                },
                // Leadership Skills
                new Skill
                {
                    Id = new Guid("08a22cea-ab17-4a3f-9645-23305553152c"),
                    Title = "Leadership Skill 12",
                    Description = "Description for leadership skill 12",
                    CategoryId = leadershipCategory.Id
                },
                new Skill
                {
                    Id = new Guid("199f28ba-4b05-42bd-b0d8-edda9cef4181"),
                    Title = "Leadership Skill 23",
                    Description = "Description for leadership skill 23",
                    CategoryId = leadershipCategory.Id
                },
                // Business Skills
                new Skill
                {
                    Id = new Guid("087eca69-033b-4542-aef1-8f2756ccc562"),
                    Title = "Business Skill 7",
                    Description = "Description for business skill 7",
                    CategoryId = businessCategory.Id
                },
                new Skill
                {
                    Id = new Guid("31676cb5-e435-49f5-b0ea-99db874aa3fa"),
                    Title = "Business Skill 6",
                    Description = "Description for business skill 6",
                    CategoryId = businessCategory.Id
                },
                // Soft Skills
                new Skill
                {
                    Id = new Guid("084c859d-23f8-4de3-b020-6399d2ed6623"),
                    Title = "Soft Skills Skill 8",
                    Description = "Description for soft skills skill 8",
                    CategoryId = softSkillsCategory.Id
                },
                new Skill
                {
                    Id = new Guid("189d66ee-95e1-4672-b154-c19dccdcaf4c"),
                    Title = "Soft Skills Skill 2",
                    Description = "Description for soft skills skill 2",
                    CategoryId = softSkillsCategory.Id
                },
                // Domain Specific Skills
                new Skill
                {
                    Id = new Guid("10c1293d-63e7-43ac-8933-7e7e611577a9"),
                    Title = "Domain Specific Skill 1",
                    Description = "Description for domain specific skill 1",
                    CategoryId = domainSpecificCategory.Id
                },
                new Skill
                {
                    Id = new Guid("0480319f-144f-4758-ad4d-22091680a0bb"),
                    Title = "Domain Specific Skill 2",
                    Description = "Description for domain specific skill 2",
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
                        Description = "Basic understanding and limited experience",
                        SkillId = skill.Id,
                        Value = 1
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Intermediate",
                        Description = "Solid understanding with practical experience",
                        SkillId = skill.Id,
                        Value = 2
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Advanced",
                        Description = "Deep expertise and extensive experience",
                        SkillId = skill.Id,
                        Value = 3
                    },
                    new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Expert",
                        Description = "Recognized authority with innovative contributions",
                        SkillId = skill.Id,
                        Value = 4
                    }
                });
            }

            await _context.SkillLevels.AddRangeAsync(skillLevels);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {CategoryCount} skill categories, {SkillCount} skills, and {LevelCount} skill levels.",
                categories.Length, skills.Length, skillLevels.Count);
        }

        private async Task SeedProjectsAsync()
        {
            if (await _context.Projects.AnyAsync())
            {
                _logger.LogInformation("Projects already exist, skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding projects...");

            var projects = new[]
            {
                new Project
                {
                    Id = Guid.NewGuid(),
                    Code = "PRJ-001",
                    Title = "Career Progression System",
                    Description = "Development of the career progression and review system"
                }
            };

            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} projects.", projects.Length);
        }
    }
}