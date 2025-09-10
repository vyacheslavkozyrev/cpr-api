using System;
using Microsoft.EntityFrameworkCore;
using CPR.Domain.Entities;

namespace CPR.Infrastructure.Data
{
    public class CprDbContext : DbContext
    {
        public CprDbContext(DbContextOptions<CprDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CareerPath> CareerPaths { get; set; }
        public DbSet<CareerTrack> CareerTracks { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<SkillCategory> SkillCategories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<SkillLevel> SkillLevels { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectRole> ProjectRoles { get; set; }
        public DbSet<GoalTask> GoalTasks { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<FeedbackRequest> FeedbackRequests { get; set; }
        public DbSet<EmployeeToSkill> EmployeeSkills { get; set; }
        public DbSet<PositionToSkill> PositionToSkills { get; set; }
        public DbSet<ProjectTeam> ProjectTeams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users");
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).HasColumnName("id");
                b.Property(u => u.UserName).HasColumnName("user_name").IsRequired();
                b.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
                b.Property(u => u.DisplayName).HasColumnName("display_name");

                b.Property(u => u.CreatedBy).HasColumnName("created_by");
                b.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(u => u.ModifiedBy).HasColumnName("modified_by");
                b.Property(u => u.ModifiedAt).HasColumnName("modified_at");
                b.Property(u => u.IsDeleted).HasColumnName("is_deleted");
                b.Property(u => u.DeletedBy).HasColumnName("deleted_by");
                b.Property(u => u.DeletedAt).HasColumnName("deleted_at");

                // Seed: sample user
                b.HasData(new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    UserName = "jane.smith",
                    PasswordHash = "$2b$12$.........................", // placeholder
                    DisplayName = "Jane Smith",
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<Employee>(b =>
            {
                b.ToTable("employees");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).HasColumnName("id");
                b.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                b.Property(e => e.ManagerId).HasColumnName("manager_id");
                b.Property(e => e.Title).HasColumnName("title");
                b.Property(e => e.Department).HasColumnName("department");

                b.Property(e => e.CreatedBy).HasColumnName("created_by");
                b.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(e => e.ModifiedBy).HasColumnName("modified_by");
                b.Property(e => e.ModifiedAt).HasColumnName("modified_at");
                b.Property(e => e.IsDeleted).HasColumnName("is_deleted");
                b.Property(e => e.DeletedBy).HasColumnName("deleted_by");
                b.Property(e => e.DeletedAt).HasColumnName("deleted_at");

                // Seed: sample employee linked to seeded user
                b.HasData(new Employee
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    ManagerId = null,
                    Title = "Senior Software Engineer",
                    Department = "Engineering",
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<Goal>(b =>
            {
                b.ToTable("goals");
                b.HasKey(g => g.Id);
                b.Property(g => g.Id).HasColumnName("id");
                b.Property(g => g.OwnerId).HasColumnName("owner_id").IsRequired();
                b.Property(g => g.Title).HasColumnName("title").IsRequired();
                b.Property(g => g.Description).HasColumnName("description");
                b.Property(g => g.Status).HasColumnName("status").HasDefaultValue("open");

                b.Property(g => g.CreatedBy).HasColumnName("created_by");
                b.Property(g => g.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(g => g.ModifiedBy).HasColumnName("modified_by");
                b.Property(g => g.ModifiedAt).HasColumnName("modified_at");
                b.Property(g => g.IsDeleted).HasColumnName("is_deleted");
                b.Property(g => g.DeletedBy).HasColumnName("deleted_by");
                b.Property(g => g.DeletedAt).HasColumnName("deleted_at");

                // Seed: sample goal
                b.HasData(new Goal
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Improve unit test coverage",
                    Description = "Add tests for critical services",
                    Status = "open",
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            // Basic mappings for additional entities (table name + created_at default)
            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("audit_logs");
                b.HasKey(a => a.Id);
                b.Property(a => a.Id).HasColumnName("id");
                b.Property(a => a.Action).HasColumnName("action").IsRequired();
                b.Property(a => a.ActorId).HasColumnName("actor_id");
                b.Property(a => a.Detail).HasColumnName("detail");
                b.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(a => a.CreatedBy).HasColumnName("created_by");
                b.Property(a => a.ModifiedBy).HasColumnName("modified_by");
                b.Property(a => a.ModifiedAt).HasColumnName("modified_at");
                b.Property(a => a.IsDeleted).HasColumnName("is_deleted");
                b.Property(a => a.DeletedBy).HasColumnName("deleted_by");
                b.Property(a => a.DeletedAt).HasColumnName("deleted_at");
                b.Property(a => a.TargetId).HasColumnName("target_id");
                b.Property(a => a.TargetType).HasColumnName("target_type");
            });

            modelBuilder.Entity<CareerPath>(b =>
            {
                b.ToTable("career_paths");
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).HasColumnName("id");
                b.Property(c => c.Title).HasColumnName("title").IsRequired();
                b.Property(c => c.Description).HasColumnName("description");
                b.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(c => c.CreatedBy).HasColumnName("created_by");
                b.Property(c => c.ModifiedBy).HasColumnName("modified_by");
                b.Property(c => c.ModifiedAt).HasColumnName("modified_at");
                b.Property(c => c.IsDeleted).HasColumnName("is_deleted");
                b.Property(c => c.DeletedBy).HasColumnName("deleted_by");
                b.Property(c => c.DeletedAt).HasColumnName("deleted_at");
                // Seed: minimal career paths
                b.HasData(
                    new CareerPath
                    {
                        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                        Title = "Technology",
                        Description = "Engineering, architecture and platform roles",
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    },
                    new CareerPath
                    {
                        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                        Title = "People",
                        Description = "HR, people operations and employee development",
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    },
                    new CareerPath
                    {
                        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                        Title = "Finance",
                        Description = "Financial planning, reporting and analysis",
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    }
                );
            });

            modelBuilder.Entity<CareerTrack>(b =>
            {
                b.ToTable("career_tracks");
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).HasColumnName("id");
                b.Property(c => c.Title).HasColumnName("title").IsRequired();
                b.Property(c => c.Description).HasColumnName("description");
                b.Property(c => c.CareerPathId).HasColumnName("career_path_id");
                b.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(c => c.CreatedBy).HasColumnName("created_by");
                b.Property(c => c.ModifiedBy).HasColumnName("modified_by");
                b.Property(c => c.ModifiedAt).HasColumnName("modified_at");
                b.Property(c => c.IsDeleted).HasColumnName("is_deleted");
                b.Property(c => c.DeletedBy).HasColumnName("deleted_by");
                b.Property(c => c.DeletedAt).HasColumnName("deleted_at");
                // Seed: minimal career track for Technology
                b.HasData(new CareerTrack
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001"),
                    Title = "Software Engineering",
                    Description = "Software development and engineering roles",
                    CareerPathId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                    CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<Position>(b =>
            {
                b.ToTable("positions");
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).HasColumnName("id");
                b.Property(p => p.Title).HasColumnName("title").IsRequired();
                b.Property(p => p.CareerTrackId).HasColumnName("career_track_id");
                b.Property(p => p.Description).HasColumnName("description");
                b.Property(p => p.Expectations).HasColumnName("expectations");
                b.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(p => p.CreatedBy).HasColumnName("created_by");
                b.Property(p => p.ModifiedBy).HasColumnName("modified_by");
                b.Property(p => p.ModifiedAt).HasColumnName("modified_at");
                b.Property(p => p.IsDeleted).HasColumnName("is_deleted");
                b.Property(p => p.DeletedBy).HasColumnName("deleted_by");
                b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
                // Seed: sample position
                b.HasData(new Position
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0001"),
                    Title = "Senior Software Engineer",
                    CareerTrackId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001"),
                    CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<SkillCategory>(b =>
            {
                b.ToTable("skill_categories");
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).HasColumnName("id");
                b.Property(s => s.Title).HasColumnName("title").IsRequired();
                b.Property(s => s.Description).HasColumnName("description");
                b.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(s => s.CreatedBy).HasColumnName("created_by");
                b.Property(s => s.ModifiedBy).HasColumnName("modified_by");
                b.Property(s => s.ModifiedAt).HasColumnName("modified_at");
                b.Property(s => s.IsDeleted).HasColumnName("is_deleted");
                b.Property(s => s.DeletedBy).HasColumnName("deleted_by");
                b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
                // Seed: skill categories
                b.HasData(
                    new SkillCategory
                    {
                        Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                        Title = "Technical",
                        Description = "Technical skills and competencies",
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    },
                    new SkillCategory
                    {
                        Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                        Title = "Leadership",
                        Description = "Leadership and communication skills",
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    }
                );
            });

            modelBuilder.Entity<Skill>(b =>
            {
                b.ToTable("skills");
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).HasColumnName("id");
                b.Property(s => s.Title).HasColumnName("title").IsRequired();
                b.Property(s => s.Description).HasColumnName("description");
                b.Property(s => s.CategoryId).HasColumnName("category_id");
                b.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(s => s.CreatedBy).HasColumnName("created_by");
                b.Property(s => s.ModifiedBy).HasColumnName("modified_by");
                b.Property(s => s.ModifiedAt).HasColumnName("modified_at");
                b.Property(s => s.IsDeleted).HasColumnName("is_deleted");
                b.Property(s => s.DeletedBy).HasColumnName("deleted_by");
                b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
                // Seed: sample skills
                b.HasData(
                    new Skill
                    {
                        Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"),
                        Title = "Unit Testing",
                        Description = "Writing unit and integration tests",
                        CategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    },
                    new Skill
                    {
                        Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0002"),
                        Title = "Communication",
                        Description = "Verbal and written communication skills",
                        CategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    }
                );
            });

            modelBuilder.Entity<SkillLevel>(b =>
            {
                b.ToTable("skill_levels");
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).HasColumnName("id");
                b.Property(s => s.Title).HasColumnName("title").IsRequired();
                b.Property(s => s.Description).HasColumnName("description");
                b.Property(s => s.SkillId).HasColumnName("skill_id");
                b.Property(s => s.Value).HasColumnName("value");
                b.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(s => s.CreatedBy).HasColumnName("created_by");
                b.Property(s => s.ModifiedBy).HasColumnName("modified_by");
                b.Property(s => s.ModifiedAt).HasColumnName("modified_at");
                b.Property(s => s.IsDeleted).HasColumnName("is_deleted");
                b.Property(s => s.DeletedBy).HasColumnName("deleted_by");
                b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
                // Seed: skill levels for Unit Testing
                b.HasData(
                    new SkillLevel
                    {
                        Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0001"),
                        Title = "Beginner",
                        SkillId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"),
                        Value = 1,
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    },
                    new SkillLevel
                    {
                        Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0002"),
                        Title = "Intermediate",
                        SkillId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"),
                        Value = 3,
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    },
                    new SkillLevel
                    {
                        Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0003"),
                        Title = "Advanced",
                        SkillId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"),
                        Value = 5,
                        CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                    }
                );
            });

            modelBuilder.Entity<Department>(b =>
            {
                b.ToTable("departments");
                b.HasKey(d => d.Id);
                b.Property(d => d.Id).HasColumnName("id");
                b.Property(d => d.Name).HasColumnName("name").IsRequired();
                b.Property(d => d.Code).HasColumnName("code");
                b.Property(d => d.Description).HasColumnName("description");
                b.Property(d => d.ManagerId).HasColumnName("manager_id");
                b.Property(d => d.ParentDepartmentId).HasColumnName("parent_department_id");
                b.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(d => d.CreatedBy).HasColumnName("created_by");
                b.Property(d => d.ModifiedBy).HasColumnName("modified_by");
                b.Property(d => d.ModifiedAt).HasColumnName("modified_at");
                b.Property(d => d.IsDeleted).HasColumnName("is_deleted");
                b.Property(d => d.DeletedBy).HasColumnName("deleted_by");
                b.Property(d => d.DeletedAt).HasColumnName("deleted_at");
                // Seed: sample department
                b.HasData(new Department
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999901"),
                    Name = "Engineering",
                    Code = "ENG",
                    CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<Location>(b =>
            {
                b.ToTable("locations");
                b.HasKey(l => l.Id);
                b.Property(l => l.Id).HasColumnName("id");
                b.Property(l => l.Name).HasColumnName("name").IsRequired();
                b.Property(l => l.Address).HasColumnName("address");
                b.Property(l => l.City).HasColumnName("city");
                b.Property(l => l.ContactPhone).HasColumnName("contact_phone");
                b.Property(l => l.Country).HasColumnName("country");
                b.Property(l => l.PostalCode).HasColumnName("postal_code");
                b.Property(l => l.Region).HasColumnName("region");
                b.Property(l => l.Timezone).HasColumnName("timezone");
                b.Property(l => l.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(l => l.CreatedBy).HasColumnName("created_by");
                b.Property(l => l.ModifiedBy).HasColumnName("modified_by");
                b.Property(l => l.ModifiedAt).HasColumnName("modified_at");
                b.Property(l => l.IsDeleted).HasColumnName("is_deleted");
                b.Property(l => l.DeletedBy).HasColumnName("deleted_by");
                b.Property(l => l.DeletedAt).HasColumnName("deleted_at");
                // Seed: sample location
                b.HasData(new Location
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888801"),
                    Name = "Headquarters",
                    City = "Remote",
                    Country = "Global",
                    CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<Project>(b =>
            {
                b.ToTable("projects");
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).HasColumnName("id");
                b.Property(p => p.Code).HasColumnName("code").IsRequired();
                b.Property(p => p.Title).HasColumnName("title").IsRequired();
                b.Property(p => p.Description).HasColumnName("description");
                b.Property(p => p.OwnerId).HasColumnName("owner_id");
                b.Property(p => p.SponsorId).HasColumnName("sponsor_id");
                b.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(p => p.CreatedBy).HasColumnName("created_by");
                b.Property(p => p.ModifiedBy).HasColumnName("modified_by");
                b.Property(p => p.ModifiedAt).HasColumnName("modified_at");
                b.Property(p => p.IsDeleted).HasColumnName("is_deleted");
                b.Property(p => p.DeletedBy).HasColumnName("deleted_by");
                b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
                // Seed: sample project
                b.HasData(new Project
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777701"),
                    Code = "PRJ-001",
                    Title = "Sample Project",
                    CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<ProjectRole>(b =>
            {
                b.ToTable("project_roles");
                b.HasKey(r => r.Id);
                b.Property(r => r.Id).HasColumnName("id");
                b.Property(r => r.Title).HasColumnName("title").IsRequired();
                b.Property(r => r.PositionId).HasColumnName("position_id");
                b.Property(r => r.Description).HasColumnName("description");
                b.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(r => r.CreatedBy).HasColumnName("created_by");
                b.Property(r => r.ModifiedBy).HasColumnName("modified_by");
                b.Property(r => r.ModifiedAt).HasColumnName("modified_at");
                b.Property(r => r.IsDeleted).HasColumnName("is_deleted");
                b.Property(r => r.DeletedBy).HasColumnName("deleted_by");
                b.Property(r => r.DeletedAt).HasColumnName("deleted_at");
                // Seed: sample project role
                b.HasData(new ProjectRole
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666601"),
                    Title = "Tech Lead",
                    PositionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0001"),
                    CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = DateTimeOffset.Parse("2025-09-05T00:00:00Z")
                });
            });

            modelBuilder.Entity<GoalTask>(b =>
            {
                b.ToTable("goal_tasks");
                b.HasKey(t => t.Id);
                b.Property(t => t.Id).HasColumnName("id");
                b.Property(t => t.GoalId).HasColumnName("goal_id");
                b.Property(t => t.Title).HasColumnName("title").IsRequired();
                b.Property(t => t.Description).HasColumnName("description");
                b.Property(t => t.CompletedAt).HasColumnName("completed_at");
                b.Property(t => t.Deadline).HasColumnName("deadline");
                b.Property(t => t.IsCompleted).HasColumnName("is_completed");
                b.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(t => t.CreatedBy).HasColumnName("created_by");
                b.Property(t => t.ModifiedBy).HasColumnName("modified_by");
                b.Property(t => t.ModifiedAt).HasColumnName("modified_at");
                b.Property(t => t.IsDeleted).HasColumnName("is_deleted");
                b.Property(t => t.DeletedBy).HasColumnName("deleted_by");
                b.Property(t => t.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<Feedback>(b =>
            {
                b.ToTable("feedback");
                b.HasKey(f => f.Id);
                b.Property(f => f.Id).HasColumnName("id");
                b.Property(f => f.GoalId).HasColumnName("goal_id");
                b.Property(f => f.FromEmployeeId).HasColumnName("from_employee_id");
                b.Property(f => f.ToEmployeeId).HasColumnName("to_employee_id");
                b.Property(f => f.ProjectId).HasColumnName("project_id");
                b.Property(f => f.Rating).HasColumnName("rating");
                b.Property(f => f.Content).HasColumnName("content").IsRequired();
                b.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(f => f.CreatedBy).HasColumnName("created_by");
                b.Property(f => f.ModifiedBy).HasColumnName("modified_by");
                b.Property(f => f.ModifiedAt).HasColumnName("modified_at");
                b.Property(f => f.IsDeleted).HasColumnName("is_deleted");
                b.Property(f => f.DeletedBy).HasColumnName("deleted_by");
                b.Property(f => f.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<FeedbackRequest>(b =>
            {
                b.ToTable("feedback_requests");
                b.HasKey(fr => fr.Id);
                b.Property(fr => fr.Id).HasColumnName("id");
                b.Property(fr => fr.RequestorId).HasColumnName("requestor_id");
                b.Property(fr => fr.EmployeeId).HasColumnName("employee_id");
                b.Property(fr => fr.Message).HasColumnName("message");
                b.Property(fr => fr.DueDate).HasColumnName("due_date");
                b.Property(fr => fr.ProjectId).HasColumnName("project_id");
                b.Property(fr => fr.GoalId).HasColumnName("goal_id");
                b.Property(fr => fr.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(fr => fr.CreatedBy).HasColumnName("created_by");
                b.Property(fr => fr.ModifiedBy).HasColumnName("modified_by");
                b.Property(fr => fr.ModifiedAt).HasColumnName("modified_at");
                b.Property(fr => fr.IsDeleted).HasColumnName("is_deleted");
                b.Property(fr => fr.DeletedBy).HasColumnName("deleted_by");
                b.Property(fr => fr.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<EmployeeToSkill>(b =>
            {
                b.ToTable("employee_to_skill");
                b.HasKey(es => es.Id);
                b.Property(es => es.Id).HasColumnName("id");
                b.Property(es => es.EmployeeId).HasColumnName("employee_id").IsRequired();
                b.Property(es => es.SkillId).HasColumnName("skill_id").IsRequired();
                b.Property(es => es.SkillLevelId).HasColumnName("skill_level_id");
                b.Property(es => es.PersistValue).HasColumnName("persist_value");
                b.Property(es => es.Source).HasColumnName("source");
                b.Property(es => es.EffectiveDate).HasColumnName("effective_date");
                b.Property(es => es.IsTarget).HasColumnName("is_target");
                b.Property(es => es.CreatedBy).HasColumnName("created_by");
                b.Property(es => es.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(es => es.ModifiedBy).HasColumnName("modified_by");
                b.Property(es => es.ModifiedAt).HasColumnName("modified_at");
                b.Property(es => es.IsDeleted).HasColumnName("is_deleted");
                b.Property(es => es.DeletedBy).HasColumnName("deleted_by");
                b.Property(es => es.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<PositionToSkill>(b =>
            {
                b.ToTable("position_to_skill");
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).HasColumnName("id");
                b.Property(p => p.PositionId).HasColumnName("position_id").IsRequired();
                b.Property(p => p.SkillId).HasColumnName("skill_id").IsRequired();
                b.Property(p => p.SkillLevelId).HasColumnName("skill_level_id").IsRequired();
                b.Property(p => p.Weight).HasColumnName("weight");
                b.Property(p => p.IsMandatory).HasColumnName("is_mandatory");
                b.Property(p => p.Rationale).HasColumnName("rationale");
                b.Property(p => p.CreatedBy).HasColumnName("created_by");
                b.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(p => p.ModifiedBy).HasColumnName("modified_by");
                b.Property(p => p.ModifiedAt).HasColumnName("modified_at");
                b.Property(p => p.IsDeleted).HasColumnName("is_deleted");
                b.Property(p => p.DeletedBy).HasColumnName("deleted_by");
                b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<ProjectTeam>(b =>
            {
                b.ToTable("project_teams");
                b.HasKey(pt => pt.Id);
                b.Property(pt => pt.Id).HasColumnName("id");
                b.Property(pt => pt.ProjectId).HasColumnName("project_id").IsRequired();
                b.Property(pt => pt.ProjectRoleId).HasColumnName("project_role_id").IsRequired();
                b.Property(pt => pt.EmployeeId).HasColumnName("employee_id").IsRequired();
                b.Property(pt => pt.CreatedBy).HasColumnName("created_by");
                b.Property(pt => pt.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(pt => pt.ModifiedBy).HasColumnName("modified_by");
                b.Property(pt => pt.ModifiedAt).HasColumnName("modified_at");
                b.Property(pt => pt.IsDeleted).HasColumnName("is_deleted");
                b.Property(pt => pt.DeletedBy).HasColumnName("deleted_by");
                b.Property(pt => pt.DeletedAt).HasColumnName("deleted_at");
            });
        }
    }
}
