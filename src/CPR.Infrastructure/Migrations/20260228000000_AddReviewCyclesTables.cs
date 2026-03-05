using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCyclesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "review_cycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    subject_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "draft"),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_review_cycles", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_cycles_employees_subject_employee_id",
                        column: x => x.subject_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_cycles_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "review_nominees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nominated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
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
                    table.PrimaryKey("PK_review_nominees", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_nominees_review_cycles_cycle_id",
                        column: x => x.cycle_id,
                        principalTable: "review_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_nominees_employees_reviewer_employee_id",
                        column: x => x.reviewer_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_nominees_employees_nominated_by",
                        column: x => x.nominated_by,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "review_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nominee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    overall_rating = table.Column<short>(type: "smallint", nullable: false),
                    comments = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_review_responses", x => x.id);
                    table.CheckConstraint("CK_review_responses_overall_rating", "overall_rating BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_review_responses_review_cycles_cycle_id",
                        column: x => x.cycle_id,
                        principalTable: "review_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_responses_review_nominees_nominee_id",
                        column: x => x.nominee_id,
                        principalTable: "review_nominees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_responses_employees_reviewer_employee_id",
                        column: x => x.reviewer_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indexes for review_cycles
            migrationBuilder.CreateIndex(
                name: "IX_review_cycles_subject_employee_id",
                table: "review_cycles",
                column: "subject_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_cycles_department_id",
                table: "review_cycles",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_cycles_status",
                table: "review_cycles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_review_cycles_is_deleted",
                table: "review_cycles",
                column: "is_deleted");

            // Indexes for review_nominees
            migrationBuilder.CreateIndex(
                name: "IX_review_nominees_cycle_id",
                table: "review_nominees",
                column: "cycle_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_nominees_reviewer_employee_id",
                table: "review_nominees",
                column: "reviewer_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_nominees_is_deleted",
                table: "review_nominees",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "UX_review_nominees_cycle_reviewer",
                table: "review_nominees",
                columns: new[] { "cycle_id", "reviewer_employee_id" },
                unique: true,
                filter: "is_deleted = FALSE");

            // Indexes for review_responses
            migrationBuilder.CreateIndex(
                name: "IX_review_responses_cycle_id",
                table: "review_responses",
                column: "cycle_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_responses_nominee_id",
                table: "review_responses",
                column: "nominee_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_responses_is_deleted",
                table: "review_responses",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "UX_review_responses_nominee",
                table: "review_responses",
                column: "nominee_id",
                unique: true,
                filter: "is_deleted = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "review_responses");
            migrationBuilder.DropTable(name: "review_nominees");
            migrationBuilder.DropTable(name: "review_cycles");
        }
    }
}
