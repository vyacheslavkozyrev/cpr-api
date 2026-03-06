using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillAssessmentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- positions: add sort_order ---
            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "positions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_positions_career_track_sort_order",
                table: "positions",
                columns: new[] { "career_track_id", "sort_order" });

            // --- employee_to_skill: add notes column ---
            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "employee_to_skill",
                type: "text",
                nullable: true);

            // --- employee_to_skill: partial unique index enforcing one self-assessment per (employee, skill, is_target) ---
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"UX_employee_to_skill_self\" " +
                "ON employee_to_skill(employee_id, skill_id, is_target) " +
                "WHERE source = 'self'");

            // --- employee_skill_evidence: new table ---
            migrationBuilder.CreateTable(
                name: "employee_skill_evidence",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_to_skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_employee_skill_evidence", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_skill_evidence_employee_to_skill_employee_to_skill_id",
                        column: x => x.employee_to_skill_id,
                        principalTable: "employee_to_skill",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_skill_evidence_feedback_feedback_id",
                        column: x => x.feedback_id,
                        principalTable: "feedback",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_skill_evidence_employee_to_skill_id",
                table: "employee_skill_evidence",
                column: "employee_to_skill_id");

            migrationBuilder.CreateIndex(
                name: "UX_employee_skill_evidence_assessment_feedback",
                table: "employee_skill_evidence",
                columns: new[] { "employee_to_skill_id", "feedback_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback employee_skill_evidence
            migrationBuilder.DropTable(name: "employee_skill_evidence");

            // Rollback employee_to_skill partial unique index and notes
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_employee_to_skill_self\"");
            migrationBuilder.DropColumn(name: "notes", table: "employee_to_skill");

            // Rollback positions sort_order
            migrationBuilder.DropIndex(name: "IX_positions_career_track_sort_order", table: "positions");
            migrationBuilder.DropColumn(name: "sort_order", table: "positions");
        }
    }
}
