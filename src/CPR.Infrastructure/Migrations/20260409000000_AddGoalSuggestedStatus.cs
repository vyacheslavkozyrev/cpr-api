using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalSuggestedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Extend status CHECK constraint to allow 'suggested' value
            migrationBuilder.Sql(
                "ALTER TABLE goals DROP CONSTRAINT IF EXISTS chk_goals_status;");

            migrationBuilder.Sql(
                "ALTER TABLE goals ADD CONSTRAINT chk_goals_status " +
                "CHECK (status IN ('not_started', 'in_progress', 'completed', 'on_hold', 'cancelled', 'suggested', 'open'));");

            // Add suggested_by_id column if it does not already exist
            migrationBuilder.Sql(
                "ALTER TABLE goals ADD COLUMN IF NOT EXISTS suggested_by_id UUID NULL REFERENCES users(id);");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS IX_goals_suggested_by_id ON goals (suggested_by_id) " +
                "WHERE suggested_by_id IS NOT NULL;");

            // Add timeframe column if it does not already exist
            migrationBuilder.Sql(
                "ALTER TABLE goals ADD COLUMN IF NOT EXISTS timeframe VARCHAR(20) NULL;");

            // Add skill_category_id column if it does not already exist
            migrationBuilder.Sql(
                "ALTER TABLE goals ADD COLUMN IF NOT EXISTS skill_category_id UUID NULL REFERENCES skill_categories(id);");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS IX_goals_skill_category_id ON goals (skill_category_id) " +
                "WHERE skill_category_id IS NOT NULL;");

            // Create goal_deletion_requests table if it does not already exist
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS goal_deletion_requests (
    id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    goal_id          UUID         NOT NULL REFERENCES goals(id),
    requested_by_id  UUID         NOT NULL REFERENCES users(id),
    status           VARCHAR(20)  NOT NULL DEFAULT 'pending',
    reason           TEXT         NULL,
    reviewed_by_id   UUID         NULL REFERENCES users(id),
    reviewed_at      TIMESTAMPTZ  NULL,
    created_by       UUID         NOT NULL,
    created_at       TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by      UUID         NULL,
    modified_at      TIMESTAMPTZ  NULL,
    is_deleted       BOOLEAN      NOT NULL DEFAULT FALSE,
    deleted_by       UUID         NULL,
    deleted_at       TIMESTAMPTZ  NULL
);");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS IX_goal_deletion_requests_goal_id " +
                "ON goal_deletion_requests (goal_id);");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS UQ_goal_deletion_requests_pending " +
                "ON goal_deletion_requests (goal_id) " +
                "WHERE status = 'pending' AND is_deleted = FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP TABLE IF EXISTS goal_deletion_requests;");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS IX_goals_suggested_by_id;");

            migrationBuilder.Sql(
                "ALTER TABLE goals DROP COLUMN IF EXISTS suggested_by_id;");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS IX_goals_skill_category_id;");

            migrationBuilder.Sql(
                "ALTER TABLE goals DROP COLUMN IF EXISTS skill_category_id;");

            migrationBuilder.Sql(
                "ALTER TABLE goals DROP COLUMN IF EXISTS timeframe;");

            migrationBuilder.Sql(
                "ALTER TABLE goals DROP CONSTRAINT IF EXISTS chk_goals_status;");

            migrationBuilder.Sql(
                "ALTER TABLE goals ADD CONSTRAINT chk_goals_status " +
                "CHECK (status IN ('not_started', 'in_progress', 'completed', 'on_hold', 'cancelled', 'open'));");
        }
    }
}
