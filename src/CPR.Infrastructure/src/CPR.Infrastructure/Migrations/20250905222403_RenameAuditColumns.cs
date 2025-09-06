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
            var renames = new (string Table, string Old, string New)[]
            {
                                ("users", "CreatedBy", "create_by"),
                                ("users", "CreatedAt", "create_at"),
                                ("users", "ModifiedBy", "modified_by"),
                                ("users", "ModifiedAt", "modified_at"),
                                ("users", "IsDeleted", "is_deleted"),
                                ("users", "DeletedBy", "deleted_by"),
                                ("users", "DeletedAt", "deleted_at"),

                                ("goals", "CreatedBy", "create_by"),
                                ("goals", "CreatedAt", "create_at"),
                                ("goals", "ModifiedBy", "modified_by"),
                                ("goals", "ModifiedAt", "modified_at"),
                                ("goals", "IsDeleted", "is_deleted"),
                                ("goals", "DeletedBy", "deleted_by"),
                                ("goals", "DeletedAt", "deleted_at"),

                                ("employees", "CreatedBy", "create_by"),
                                ("employees", "CreatedAt", "create_at"),
                                ("employees", "ModifiedBy", "modified_by"),
                                ("employees", "ModifiedAt", "modified_at"),
                                ("employees", "IsDeleted", "is_deleted"),
                                ("employees", "DeletedBy", "deleted_by"),
                                ("employees", "DeletedAt", "deleted_at"),
            };

            foreach (var r in renames)
            {
                var sql = $@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name='{r.Table}' AND column_name='{r.Old}'
    ) THEN
        EXECUTE 'ALTER TABLE {r.Table} RENAME COLUMN ""{r.Old}"" TO ""{r.New}""';
    END IF;
END
$$;";

                migrationBuilder.Sql(sql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var renames = new (string Table, string Old, string New)[]
            {
                                ("users", "create_by", "CreatedBy"),
                                ("users", "create_at", "CreatedAt"),
                                ("users", "modified_by", "ModifiedBy"),
                                ("users", "modified_at", "ModifiedAt"),
                                ("users", "is_deleted", "IsDeleted"),
                                ("users", "deleted_by", "DeletedBy"),
                                ("users", "deleted_at", "DeletedAt"),

                                ("goals", "create_by", "CreatedBy"),
                                ("goals", "create_at", "CreatedAt"),
                                ("goals", "modified_by", "ModifiedBy"),
                                ("goals", "modified_at", "ModifiedAt"),
                                ("goals", "is_deleted", "IsDeleted"),
                                ("goals", "deleted_by", "DeletedBy"),
                                ("goals", "deleted_at", "DeletedAt"),

                                ("employees", "create_by", "CreatedBy"),
                                ("employees", "create_at", "CreatedAt"),
                                ("employees", "modified_by", "ModifiedBy"),
                                ("employees", "modified_at", "ModifiedAt"),
                                ("employees", "is_deleted", "IsDeleted"),
                                ("employees", "deleted_by", "DeletedBy"),
                                ("employees", "deleted_at", "DeletedAt"),
            };

            foreach (var r in renames)
            {
                var sql = $@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name='{r.Table}' AND column_name='{r.Old}'
    ) THEN
        EXECUTE 'ALTER TABLE {r.Table} RENAME COLUMN ""{r.Old}"" TO ""{r.New}""';
    END IF;
END
$$;";

                migrationBuilder.Sql(sql);
            }
        }
    }
}
