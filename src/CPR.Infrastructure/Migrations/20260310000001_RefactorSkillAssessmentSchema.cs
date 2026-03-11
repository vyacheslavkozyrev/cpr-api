using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSkillAssessmentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Remove target-assessment rows (is_target = TRUE) — no longer needed
            migrationBuilder.Sql("DELETE FROM employee_to_skill WHERE is_target = TRUE");

            // Step 2: Default NULL persist_value to 0 before enforcing NOT NULL
            migrationBuilder.Sql("UPDATE employee_to_skill SET persist_value = 0 WHERE persist_value IS NULL");

            // Step 3: Rename persist_value → self_assessment_value
            migrationBuilder.RenameColumn(
                name: "persist_value",
                table: "employee_to_skill",
                newName: "self_assessment_value");

            // Enforce NOT NULL with default 0
            migrationBuilder.AlterColumn<decimal>(
                name: "self_assessment_value",
                table: "employee_to_skill",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            // Step 4: Add manager_assessment_value (nullable)
            migrationBuilder.AddColumn<decimal>(
                name: "manager_assessment_value",
                table: "employee_to_skill",
                type: "numeric",
                nullable: true);

            // Step 5: Drop the 0007 partial index that references source and is_target
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_employee_to_skill_self\"");

            // Step 5b: Drop deprecated columns
            migrationBuilder.DropColumn(
                name: "source",
                table: "employee_to_skill");

            migrationBuilder.DropColumn(
                name: "is_target",
                table: "employee_to_skill");

            // Step 6: Replace the compound unique index with a simpler per-employee-skill index
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_employee_to_skill_employee_skill_effective\"");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"UX_employee_to_skill_employee_skill\" " +
                "ON employee_to_skill(employee_id, skill_id) " +
                "WHERE is_deleted = FALSE");

            // Step 7: Drop weight from position_to_skill
            migrationBuilder.DropColumn(
                name: "weight",
                table: "position_to_skill");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore weight on position_to_skill
            migrationBuilder.AddColumn<decimal>(
                name: "weight",
                table: "position_to_skill",
                type: "numeric(5,2)",
                nullable: true);

            // Drop new simple unique index
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_employee_to_skill_employee_skill\"");

            // Restore the original compound unique index
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"UX_employee_to_skill_employee_skill_effective\" " +
                "ON employee_to_skill(employee_id, skill_id, effective_date) " +
                "WHERE is_deleted = FALSE");

            // Restore is_target and source columns
            migrationBuilder.AddColumn<bool>(
                name: "is_target",
                table: "employee_to_skill",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "employee_to_skill",
                type: "text",
                nullable: true);

            // Drop manager_assessment_value
            migrationBuilder.DropColumn(
                name: "manager_assessment_value",
                table: "employee_to_skill");

            // Rename self_assessment_value back to persist_value and make nullable
            migrationBuilder.RenameColumn(
                name: "self_assessment_value",
                table: "employee_to_skill",
                newName: "persist_value");

            migrationBuilder.AlterColumn<decimal>(
                name: "persist_value",
                table: "employee_to_skill",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: false,
                oldDefaultValue: 0m);
        }
    }
}
