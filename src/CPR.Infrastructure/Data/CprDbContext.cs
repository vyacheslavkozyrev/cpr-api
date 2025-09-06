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
        }
    }
}
