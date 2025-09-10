using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CPR.Infrastructure.src.CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatedInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: true),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    detail = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "career_paths",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_paths", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "career_tracks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    career_path_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_tracks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    parent_department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feedback_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requestor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goal_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "open"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    region = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    timezone = table.Column<string>(type: "text", nullable: true),
                    contact_phone = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    expectations = table.Column<string>(type: "text", nullable: true),
                    career_track_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sponsor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill_levels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "career_paths",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "title" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Engineering, architecture and platform roles", false, null, null, "Technology" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "HR, people operations and employee development", false, null, null, "People" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Financial planning, reporting and analysis", false, null, null, "Finance" }
                });

            migrationBuilder.InsertData(
                table: "career_tracks",
                columns: new[] { "id", "career_path_id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "title" },
                values: new object[] { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Software development and engineering roles", false, null, null, "Software Engineering" });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "id", "code", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "manager_id", "modified_at", "modified_by", "name", "parent_department_id" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999901"), "ENG", new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, false, null, null, null, "Engineering", null });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "department", "is_deleted", "manager_id", "modified_at", "modified_by", "title", "user_id" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Engineering", false, null, null, null, "Senior Software Engineer", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "goals",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "owner_id", "status", "title" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Add tests for critical services", false, null, null, new Guid("11111111-1111-1111-1111-111111111111"), "open", "Improve unit test coverage" });

            migrationBuilder.InsertData(
                table: "locations",
                columns: new[] { "id", "address", "city", "contact_phone", "country", "created_at", "created_by", "deleted_at", "deleted_by", "is_deleted", "modified_at", "modified_by", "name", "postal_code", "region", "timezone" },
                values: new object[] { new Guid("88888888-8888-8888-8888-888888888801"), null, "Remote", null, "Global", new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, false, null, null, "Headquarters", null, null, null });

            migrationBuilder.InsertData(
                table: "positions",
                columns: new[] { "id", "career_track_id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "expectations", "is_deleted", "modified_at", "modified_by", "title" },
                values: new object[] { new Guid("cccccccc-cccc-cccc-cccc-cccccccc0001"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, null, false, null, null, "Senior Software Engineer" });

            migrationBuilder.InsertData(
                table: "project_roles",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "position_id", "title" },
                values: new object[] { new Guid("66666666-6666-6666-6666-666666666601"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, false, null, null, new Guid("cccccccc-cccc-cccc-cccc-cccccccc0001"), "Tech Lead" });

            migrationBuilder.InsertData(
                table: "projects",
                columns: new[] { "id", "code", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "owner_id", "sponsor_id", "title" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777701"), "PRJ-001", new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, false, null, null, null, null, "Sample Project" });

            migrationBuilder.InsertData(
                table: "skill_categories",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "title" },
                values: new object[,]
                {
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Technical skills and competencies", false, null, null, "Technical" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Leadership and communication skills", false, null, null, "Leadership" }
                });

            migrationBuilder.InsertData(
                table: "skill_levels",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "skill_id", "title", "value" },
                values: new object[,]
                {
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffff0001"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, false, null, null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"), "Beginner", 1 },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffff0002"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, false, null, null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"), "Intermediate", 3 },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffff0003"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, false, null, null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"), "Advanced", 5 }
                });

            migrationBuilder.InsertData(
                table: "skills",
                columns: new[] { "id", "category_id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "modified_at", "modified_by", "title" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeee0001"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Writing unit and integration tests", false, null, null, "Unit Testing" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeee0002"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Verbal and written communication skills", false, null, null, "Communication" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "display_name", "is_deleted", "modified_at", "modified_by", "password_hash", "user_name" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTimeOffset(new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Jane Smith", false, null, null, "$2b$12$.........................", "jane.smith" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "career_paths");

            migrationBuilder.DropTable(
                name: "career_tracks");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "feedback");

            migrationBuilder.DropTable(
                name: "feedback_requests");

            migrationBuilder.DropTable(
                name: "goal_tasks");

            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropTable(
                name: "project_roles");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "skill_categories");

            migrationBuilder.DropTable(
                name: "skill_levels");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
