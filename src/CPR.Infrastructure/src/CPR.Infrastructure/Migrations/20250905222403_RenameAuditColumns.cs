using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.src.CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // users
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "users",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "create_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "users",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "users",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "users",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "users",
                newName: "deleted_by");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "users",
                newName: "deleted_at");

            // goals
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "goals",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "goals",
                newName: "create_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "goals",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "goals",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "goals",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "goals",
                newName: "deleted_by");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "goals",
                newName: "deleted_at");

            // employees
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "employees",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "employees",
                newName: "create_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "employees",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "employees",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "employees",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "employees",
                newName: "deleted_by");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "employees",
                newName: "deleted_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // users
            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "users",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "create_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                table: "users",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "users",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "users",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deleted_by",
                table: "users",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "users",
                newName: "DeletedAt");

            // goals
            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "goals",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "create_at",
                table: "goals",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                table: "goals",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "goals",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "goals",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deleted_by",
                table: "goals",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "goals",
                newName: "DeletedAt");

            // employees
            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "employees",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "create_at",
                table: "employees",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                table: "employees",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "employees",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "employees",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deleted_by",
                table: "employees",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "employees",
                newName: "DeletedAt");
        }
    }
}
