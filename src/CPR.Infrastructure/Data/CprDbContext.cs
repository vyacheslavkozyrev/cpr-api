using System;
using Microsoft.EntityFrameworkCore;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data.Configurations;

namespace CPR.Infrastructure.Data
{
    public class CprDbContext : DbContext
    {
        public CprDbContext(DbContextOptions<CprDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

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
        public DbSet<FeedbackRequestRecipient> FeedbackRequestRecipients { get; set; }
        public DbSet<EmployeeToSkill> EmployeeSkills { get; set; }
        public DbSet<PositionToSkill> PositionToSkills { get; set; }
        public DbSet<ProjectTeam> ProjectTeams { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserToRole> UserRoles { get; set; }
        public DbSet<ReviewCycle> ReviewCycles { get; set; }
        public DbSet<ReviewNominee> ReviewNominees { get; set; }
        public DbSet<ReviewResponse> ReviewResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users");
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).HasColumnName("id");
                b.Property(u => u.UserName).HasColumnName("user_name").IsRequired();
                b.Property(u => u.EntraExternalId).HasColumnName("entra_external_id");
                b.Property(u => u.DisplayName).HasColumnName("display_name");

                b.Property(u => u.CreatedBy).HasColumnName("created_by");
                b.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(u => u.ModifiedBy).HasColumnName("modified_by");
                b.Property(u => u.ModifiedAt).HasColumnName("modified_at");
                b.Property(u => u.IsDeleted).HasColumnName("is_deleted");
                b.Property(u => u.DeletedBy).HasColumnName("deleted_by");
                b.Property(u => u.DeletedAt).HasColumnName("deleted_at");

                // Indexes
                b.HasIndex(u => u.UserName).IsUnique().HasDatabaseName("UX_users_user_name");
                b.HasIndex(u => u.EntraExternalId).IsUnique().HasDatabaseName("IX_users_entra_external_id");
            });

            modelBuilder.Entity<Employee>(b =>
            {
                b.ToTable("employees");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).HasColumnName("id");
                b.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                b.Property(e => e.ManagerId).HasColumnName("manager_id");
                b.Property(e => e.PositionId).HasColumnName("position_id");
                b.Property(e => e.DepartmentId).HasColumnName("department_id");

                b.Property(e => e.CreatedBy).HasColumnName("created_by");
                b.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(e => e.ModifiedBy).HasColumnName("modified_by");
                b.Property(e => e.ModifiedAt).HasColumnName("modified_at");
                b.Property(e => e.IsDeleted).HasColumnName("is_deleted");
                b.Property(e => e.DeletedBy).HasColumnName("deleted_by");
                b.Property(e => e.DeletedAt).HasColumnName("deleted_at");

                // Configure navigation properties
                b.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship to Department
                b.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Configure self-referencing relationship for Manager
                b.HasOne(e => e.Manager)
                    .WithMany(e => e.DirectReports)
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Goal>(b =>
            {
                b.ToTable("goals");
                b.HasKey(g => g.Id);
                b.Property(g => g.Id).HasColumnName("id");
                // OwnerId removed; owner is represented by employee_id
                b.Property(g => g.Title).HasColumnName("title").IsRequired();
                b.Property(g => g.Description).HasColumnName("description");
                b.Property(g => g.Status).HasColumnName("status").HasDefaultValue("open");

                // New columns from data.md
                b.Property(g => g.EmployeeId).HasColumnName("employee_id");
                b.Property(g => g.RelatedSkillId).HasColumnName("related_skill_id");
                b.Property(g => g.RelatedSkillLevelId).HasColumnName("related_skill_level_id");
                b.Property(g => g.Deadline).HasColumnName("deadline").HasColumnType("date");
                b.Property(g => g.IsCompleted).HasColumnName("is_completed").HasDefaultValue(false);
                b.Property(g => g.CompletedAt).HasColumnName("completed_at");
                b.Property(g => g.ProgressPercent).HasColumnName("progress_percent").HasColumnType("numeric(5,2)").HasDefaultValue(0.00m);
                b.Property(g => g.Priority).HasColumnName("priority").HasColumnType("smallint");
                b.Property(g => g.Visibility).HasColumnName("visibility");

                b.Property(g => g.CreatedBy).HasColumnName("created_by");
                b.Property(g => g.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(g => g.ModifiedBy).HasColumnName("modified_by");
                b.Property(g => g.ModifiedAt).HasColumnName("modified_at");
                b.Property(g => g.IsDeleted).HasColumnName("is_deleted");
                b.Property(g => g.DeletedBy).HasColumnName("deleted_by");
                b.Property(g => g.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
                b.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(c => c.CreatedBy).HasColumnName("created_by");
                b.Property(c => c.ModifiedBy).HasColumnName("modified_by");
                b.Property(c => c.ModifiedAt).HasColumnName("modified_at");
                b.Property(c => c.IsDeleted).HasColumnName("is_deleted");
                b.Property(c => c.DeletedBy).HasColumnName("deleted_by");
                b.Property(c => c.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<CareerTrack>(b =>
            {
                b.ToTable("career_tracks");
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).HasColumnName("id");
                b.Property(c => c.Title).HasColumnName("title").IsRequired();
                b.Property(c => c.Description).HasColumnName("description");
                b.Property(c => c.CareerPathId).HasColumnName("career_path_id");
                b.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(c => c.CreatedBy).HasColumnName("created_by");
                b.Property(c => c.ModifiedBy).HasColumnName("modified_by");
                b.Property(c => c.ModifiedAt).HasColumnName("modified_at");
                b.Property(c => c.IsDeleted).HasColumnName("is_deleted");
                b.Property(c => c.DeletedBy).HasColumnName("deleted_by");
                b.Property(c => c.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(p => p.CreatedBy).HasColumnName("created_by");
                b.Property(p => p.ModifiedBy).HasColumnName("modified_by");
                b.Property(p => p.ModifiedAt).HasColumnName("modified_at");
                b.Property(p => p.IsDeleted).HasColumnName("is_deleted");
                b.Property(p => p.DeletedBy).HasColumnName("deleted_by");
                b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<SkillCategory>(b =>
            {
                b.ToTable("skill_categories");
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).HasColumnName("id");
                b.Property(s => s.Title).HasColumnName("title").IsRequired();
                b.Property(s => s.Description).HasColumnName("description");
                b.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(s => s.CreatedBy).HasColumnName("created_by");
                b.Property(s => s.ModifiedBy).HasColumnName("modified_by");
                b.Property(s => s.ModifiedAt).HasColumnName("modified_at");
                b.Property(s => s.IsDeleted).HasColumnName("is_deleted");
                b.Property(s => s.DeletedBy).HasColumnName("deleted_by");
                b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<Skill>(b =>
            {
                b.ToTable("skills");
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).HasColumnName("id");
                b.Property(s => s.Title).HasColumnName("title").IsRequired();
                b.Property(s => s.Description).HasColumnName("description");
                b.Property(s => s.CategoryId).HasColumnName("category_id");
                b.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(s => s.CreatedBy).HasColumnName("created_by");
                b.Property(s => s.ModifiedBy).HasColumnName("modified_by");
                b.Property(s => s.ModifiedAt).HasColumnName("modified_at");
                b.Property(s => s.IsDeleted).HasColumnName("is_deleted");
                b.Property(s => s.DeletedBy).HasColumnName("deleted_by");
                b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(s => s.CreatedBy).HasColumnName("created_by");
                b.Property(s => s.ModifiedBy).HasColumnName("modified_by");
                b.Property(s => s.ModifiedAt).HasColumnName("modified_at");
                b.Property(s => s.IsDeleted).HasColumnName("is_deleted");
                b.Property(s => s.DeletedBy).HasColumnName("deleted_by");
                b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(d => d.CreatedBy).HasColumnName("created_by");
                b.Property(d => d.ModifiedBy).HasColumnName("modified_by");
                b.Property(d => d.ModifiedAt).HasColumnName("modified_at");
                b.Property(d => d.IsDeleted).HasColumnName("is_deleted");
                b.Property(d => d.DeletedBy).HasColumnName("deleted_by");
                b.Property(d => d.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(l => l.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(l => l.CreatedBy).HasColumnName("created_by");
                b.Property(l => l.ModifiedBy).HasColumnName("modified_by");
                b.Property(l => l.ModifiedAt).HasColumnName("modified_at");
                b.Property(l => l.IsDeleted).HasColumnName("is_deleted");
                b.Property(l => l.DeletedBy).HasColumnName("deleted_by");
                b.Property(l => l.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(p => p.CreatedBy).HasColumnName("created_by");
                b.Property(p => p.ModifiedBy).HasColumnName("modified_by");
                b.Property(p => p.ModifiedAt).HasColumnName("modified_at");
                b.Property(p => p.IsDeleted).HasColumnName("is_deleted");
                b.Property(p => p.DeletedBy).HasColumnName("deleted_by");
                b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<ProjectRole>(b =>
            {
                b.ToTable("project_roles");
                b.HasKey(r => r.Id);
                b.Property(r => r.Id).HasColumnName("id");
                b.Property(r => r.ProjectId).HasColumnName("project_id").IsRequired();
                b.Property(r => r.Title).HasColumnName("title").IsRequired();
                b.Property(r => r.Description).HasColumnName("description");
                b.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(r => r.CreatedBy).HasColumnName("created_by");
                b.Property(r => r.ModifiedBy).HasColumnName("modified_by");
                b.Property(r => r.ModifiedAt).HasColumnName("modified_at");
                b.Property(r => r.IsDeleted).HasColumnName("is_deleted");
                b.Property(r => r.DeletedBy).HasColumnName("deleted_by");
                b.Property(r => r.DeletedAt).HasColumnName("deleted_at");
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
                b.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
                b.Property(f => f.FeedbackRequestId).HasColumnName("feedback_request_id");

                b.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(f => f.CreatedBy).HasColumnName("created_by");
                b.Property(f => f.ModifiedBy).HasColumnName("modified_by");
                b.Property(f => f.ModifiedAt).HasColumnName("modified_at");
                b.Property(f => f.IsDeleted).HasColumnName("is_deleted");
                b.Property(f => f.DeletedBy).HasColumnName("deleted_by");
                b.Property(f => f.DeletedAt).HasColumnName("deleted_at");

                // Relationship to FeedbackRequest
                b.HasOne(f => f.FeedbackRequest)
                    .WithMany()
                    .HasForeignKey(f => f.FeedbackRequestId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Index for feedback_request_id
                b.HasIndex(f => f.FeedbackRequestId)
                    .HasDatabaseName("IX_feedback_feedback_request_id");
            });

            modelBuilder.Entity<FeedbackRequest>(b =>
            {
                b.ToTable("feedback_requests");
                b.HasKey(fr => fr.Id);
                b.Property(fr => fr.Id).HasColumnName("id");
                b.Property(fr => fr.RequestorId).HasColumnName("requestor_id").IsRequired();
                b.Property(fr => fr.ProjectId).HasColumnName("project_id");
                b.Property(fr => fr.GoalId).HasColumnName("goal_id");
                b.Property(fr => fr.Message).HasColumnName("message").HasMaxLength(500);
                b.Property(fr => fr.DueDate).HasColumnName("due_date").HasColumnType("date");

                // Audit properties
                b.Property(fr => fr.CreatedBy).HasColumnName("created_by");
                b.Property(fr => fr.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(fr => fr.ModifiedBy).HasColumnName("modified_by");
                b.Property(fr => fr.ModifiedAt).HasColumnName("modified_at");
                b.Property(fr => fr.IsDeleted).HasColumnName("is_deleted");
                b.Property(fr => fr.DeletedBy).HasColumnName("deleted_by");
                b.Property(fr => fr.DeletedAt).HasColumnName("deleted_at");

                // Relationships
                b.HasOne(fr => fr.Requestor)
                    .WithMany()
                    .HasForeignKey(fr => fr.RequestorId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(fr => fr.Project)
                    .WithMany()
                    .HasForeignKey(fr => fr.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne(fr => fr.Goal)
                    .WithMany()
                    .HasForeignKey(fr => fr.GoalId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(fr => fr.Recipients)
                    .WithOne(r => r.FeedbackRequest)
                    .HasForeignKey(r => r.FeedbackRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes (with filters matching migration)
                b.HasIndex(fr => fr.RequestorId)
                    .HasDatabaseName("IX_feedback_requests_requestor_id")
                    .HasFilter("is_deleted = false");

                b.HasIndex(fr => fr.DueDate)
                    .HasDatabaseName("IX_feedback_requests_due_date")
                    .HasFilter("is_deleted = false");

                b.HasIndex(fr => fr.ProjectId)
                    .HasDatabaseName("IX_feedback_requests_project_id")
                    .HasFilter("is_deleted = false AND project_id IS NOT NULL");

                b.HasIndex(fr => fr.GoalId)
                    .HasDatabaseName("IX_feedback_requests_goal_id")
                    .HasFilter("is_deleted = false AND goal_id IS NOT NULL");

                b.HasIndex(fr => fr.CreatedAt)
                    .HasDatabaseName("IX_feedback_requests_created_at")
                    .IsDescending()
                    .HasFilter("is_deleted = false");
            });

            modelBuilder.Entity<FeedbackRequestRecipient>(b =>
            {
                b.ToTable("feedback_request_recipients");
                b.HasKey(r => r.Id);
                b.Property(r => r.Id).HasColumnName("id");
                b.Property(r => r.FeedbackRequestId).HasColumnName("feedback_request_id").IsRequired();
                b.Property(r => r.EmployeeId).HasColumnName("employee_id").IsRequired();
                b.Property(r => r.IsCompleted).HasColumnName("is_completed").HasDefaultValue(false);
                b.Property(r => r.RespondedAt).HasColumnName("responded_at");
                b.Property(r => r.LastReminderAt).HasColumnName("last_reminder_at");
                b.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(r => r.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                // Relationships
                b.HasOne(r => r.FeedbackRequest)
                    .WithMany(fr => fr.Recipients)
                    .HasForeignKey(r => r.FeedbackRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(r => r.Employee)
                    .WithMany()
                    .HasForeignKey(r => r.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: one recipient per request per employee
                b.HasIndex(r => new { r.FeedbackRequestId, r.EmployeeId })
                    .IsUnique()
                    .HasDatabaseName("UX_feedback_request_recipients_request_employee");

                // Indexes
                b.HasIndex(r => r.FeedbackRequestId)
                    .HasDatabaseName("IX_feedback_request_recipients_feedback_request_id");

                b.HasIndex(r => r.EmployeeId)
                    .HasDatabaseName("IX_feedback_request_recipients_employee_id");

                b.HasIndex(r => r.IsCompleted)
                    .HasDatabaseName("IX_feedback_request_recipients_is_completed");

                b.HasIndex(r => new { r.EmployeeId, r.IsCompleted })
                    .HasDatabaseName("IX_feedback_request_recipients_pending")
                    .HasFilter("is_completed = false");
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
                b.Property(es => es.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
                b.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
                b.Property(pt => pt.ProjectRoleId).HasColumnName("project_role_id").IsRequired();
                b.Property(pt => pt.EmployeeId).HasColumnName("employee_id").IsRequired();
                b.Property(pt => pt.CreatedBy).HasColumnName("created_by");
                b.Property(pt => pt.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(pt => pt.ModifiedBy).HasColumnName("modified_by");
                b.Property(pt => pt.ModifiedAt).HasColumnName("modified_at");
                b.Property(pt => pt.IsDeleted).HasColumnName("is_deleted");
                b.Property(pt => pt.DeletedBy).HasColumnName("deleted_by");
                b.Property(pt => pt.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("roles");
                b.HasKey(r => r.Id);
                b.Property(r => r.Id).HasColumnName("id");
                b.Property(r => r.Title).HasColumnName("title").IsRequired();
                b.Property(r => r.Description).HasColumnName("description");
                b.Property(r => r.CreatedBy).HasColumnName("created_by");
                b.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(r => r.ModifiedBy).HasColumnName("modified_by");
                b.Property(r => r.ModifiedAt).HasColumnName("modified_at");
                b.Property(r => r.IsDeleted).HasColumnName("is_deleted");
                b.Property(r => r.DeletedBy).HasColumnName("deleted_by");
                b.Property(r => r.DeletedAt).HasColumnName("deleted_at");
            });

            modelBuilder.ApplyConfiguration(new ReviewCycleConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewNomineeConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewResponseConfiguration());

            modelBuilder.Entity<UserToRole>(b =>
            {
                b.ToTable("user_to_role");
                b.HasKey(ur => ur.Id);
                b.Property(ur => ur.Id).HasColumnName("id");
                b.Property(ur => ur.UserId).HasColumnName("user_id").IsRequired();
                b.Property(ur => ur.RoleId).HasColumnName("role_id").IsRequired();
                b.Property(ur => ur.CreatedBy).HasColumnName("created_by");
                b.Property(ur => ur.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(ur => ur.ModifiedBy).HasColumnName("modified_by");
                b.Property(ur => ur.ModifiedAt).HasColumnName("modified_at");
                b.Property(ur => ur.IsDeleted).HasColumnName("is_deleted");
                b.Property(ur => ur.DeletedBy).HasColumnName("deleted_by");
                b.Property(ur => ur.DeletedAt).HasColumnName("deleted_at");

                // Foreign key relationships
                b.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent duplicate user-role assignments
                b.HasIndex(ur => new { ur.UserId, ur.RoleId })
                    .IsUnique()
                    .HasFilter("is_deleted = false");
            });
        }
    }
}
