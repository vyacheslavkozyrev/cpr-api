using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabaseSchema : Migration
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
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "employee_to_skill",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_level_id = table.Column<Guid>(type: "uuid", nullable: true),
                    persist_value = table.Column<decimal>(type: "numeric", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_target = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_to_skill", x => x.id);
                });
            // Indexes and unique constraints for employee_to_skill
            migrationBuilder.CreateIndex(
                name: "IX_employee_to_skill_employee_id",
                table: "employee_to_skill",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_to_skill_skill_id",
                table: "employee_to_skill",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_to_skill_skill_level_id",
                table: "employee_to_skill",
                column: "skill_level_id");

            migrationBuilder.CreateIndex(
                name: "UX_employee_to_skill_employee_skill_effective",
                table: "employee_to_skill",
                columns: new[] { "employee_id", "skill_id", "effective_date" },
                unique: true);


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
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_employees_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_employees_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "id");
                });
            // Indexes for employees
            migrationBuilder.CreateIndex(
                name: "IX_employees_user_id",
                table: "employees",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_manager_id",
                table: "employees",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_department_id",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_position_id",
                table: "employees",
                column: "position_id");

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
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    related_skill_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_skill_level_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "open"),
                    deadline = table.Column<DateTime>(type: "date", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    progress_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0.00m),
                    priority = table.Column<short>(type: "smallint", nullable: true),
                    visibility = table.Column<string>(type: "text", nullable: true),
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

            // index + FK for employee_id -> employees(id)
            migrationBuilder.CreateIndex(
                name: "IX_goals_employee_id",
                table: "goals",
                column: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_employees_employee_id",
                table: "goals",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "position_to_skill",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_level_id = table.Column<Guid>(type: "uuid", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    rationale = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_position_to_skill", x => x.id);
                });
            // Indexes and unique constraints for position_to_skill
            migrationBuilder.CreateIndex(
                name: "IX_position_to_skill_position_id",
                table: "position_to_skill",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_position_to_skill_skill_id",
                table: "position_to_skill",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_position_to_skill_skill_level_id",
                table: "position_to_skill",
                column: "skill_level_id");

            migrationBuilder.CreateIndex(
                name: "UX_position_to_skill_position_skill",
                table: "position_to_skill",
                columns: new[] { "position_id", "skill_id" },
                unique: true);

            // Create projects table first (before project_roles)
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

            // Indexes for projects
            migrationBuilder.CreateIndex(
                name: "IX_projects_owner_id",
                table: "projects",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_sponsor_id",
                table: "projects",
                column: "sponsor_id");

            migrationBuilder.CreateIndex(
                name: "UX_projects_code",
                table: "projects",
                column: "code",
                unique: true);

            migrationBuilder.CreateTable(
                name: "project_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_project_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_roles_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_project_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_teams_project_roles_project_role_id",
                        column: x => x.project_role_id,
                        principalTable: "project_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_teams_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
            // Indexes and unique constraints for project_teams
            migrationBuilder.CreateIndex(
                name: "IX_project_teams_project_role_id",
                table: "project_teams",
                column: "project_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_teams_employee_id",
                table: "project_teams",
                column: "employee_id");

            // Index for project_roles.project_id
            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id",
                table: "project_roles",
                column: "project_id");

            // Unique constraint: one employee per project role
            migrationBuilder.CreateIndex(
                name: "UX_project_teams_project_role_employee",
                table: "project_teams",
                columns: new[] { "project_role_id", "employee_id" },
                unique: true);

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
            // Indexes for skill_levels
            migrationBuilder.CreateIndex(
                name: "IX_skill_levels_skill_id",
                table: "skill_levels",
                column: "skill_id");

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
            // Indexes for skills
            migrationBuilder.CreateIndex(
                name: "IX_skills_category_id",
                table: "skills",
                column: "category_id");

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
            // Indexes and unique constraints for users
            migrationBuilder.CreateIndex(
                name: "UX_users_user_name",
                table: "users",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_to_role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_to_role", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_to_role_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_to_role_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes for roles table
            migrationBuilder.CreateIndex(
                name: "IX_roles_title",
                table: "roles",
                column: "title",
                unique: true);

            // Indexes for user_to_role table
            migrationBuilder.CreateIndex(
                name: "IX_user_to_role_user_id",
                table: "user_to_role",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_to_role_role_id",
                table: "user_to_role",
                column: "role_id");

            // Unique constraint for user-role combinations (excluding soft-deleted records)
            migrationBuilder.CreateIndex(
                name: "UX_user_to_role_user_id_role_id",
                table: "user_to_role",
                columns: new[] { "user_id", "role_id" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "is_deleted" })
                .Annotation("Npgsql:IndexFilter", "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_to_role");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "career_paths");

            migrationBuilder.DropTable(
                name: "career_tracks");

            migrationBuilder.DropTable(
                name: "departments");

            // Drop indexes for employee_to_skill
            migrationBuilder.DropIndex(
                name: "UX_employee_to_skill_employee_skill_effective",
                table: "employee_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_employee_to_skill_skill_level_id",
                table: "employee_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_employee_to_skill_skill_id",
                table: "employee_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_employee_to_skill_employee_id",
                table: "employee_to_skill");

            migrationBuilder.DropTable(
                name: "employee_to_skill");

            // Drop indexes for employees
            migrationBuilder.DropIndex(
                name: "IX_employees_manager_id",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_user_id",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_position_id",
                table: "employees");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "positions");

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
                name: "position_to_skill");

            // Drop indexes for position_to_skill
            migrationBuilder.DropIndex(
                name: "UX_position_to_skill_position_skill",
                table: "position_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_position_to_skill_skill_level_id",
                table: "position_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_position_to_skill_skill_id",
                table: "position_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_position_to_skill_position_id",
                table: "position_to_skill");


            migrationBuilder.DropTable(
                name: "project_roles");

            // Drop indexes for project_teams
            migrationBuilder.DropIndex(
                name: "UX_project_teams_project_role_employee",
                table: "project_teams");

            migrationBuilder.DropIndex(
                name: "IX_project_teams_employee_id",
                table: "project_teams");

            migrationBuilder.DropIndex(
                name: "IX_project_teams_project_role_id",
                table: "project_teams");

            migrationBuilder.DropIndex(
                name: "IX_project_teams_project_id",
                table: "project_teams");

            migrationBuilder.DropTable(
                name: "project_teams");

            // Drop indexes for projects
            migrationBuilder.DropIndex(
                name: "UX_projects_code",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_sponsor_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_owner_id",
                table: "projects");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "skill_categories");

            // Drop indexes for skill_levels
            migrationBuilder.DropIndex(
                name: "IX_skill_levels_skill_id",
                table: "skill_levels");

            migrationBuilder.DropTable(
                name: "skill_levels");

            // Drop indexes for skills
            migrationBuilder.DropIndex(
                name: "IX_skills_category_id",
                table: "skills");

            migrationBuilder.DropTable(
                name: "skills");

            // Drop indexes for users
            migrationBuilder.DropIndex(
                name: "UX_users_user_name",
                table: "users");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
