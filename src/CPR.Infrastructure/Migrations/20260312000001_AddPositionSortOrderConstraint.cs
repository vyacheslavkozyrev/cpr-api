using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionSortOrderConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE positions ADD CONSTRAINT chk_positions_sort_order_positive CHECK (sort_order > 0);");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX uq_positions_track_sort_order ON positions (career_track_id, sort_order) WHERE is_deleted = FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS uq_positions_track_sort_order;");

            migrationBuilder.Sql(
                "ALTER TABLE positions DROP CONSTRAINT IF EXISTS chk_positions_sort_order_positive;");
        }
    }
}
