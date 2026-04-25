using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingGoalColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All statements use IF NOT EXISTS / DO blocks to be idempotent —
            // some objects may already exist if previous migrations ran partially.

            // --- goals columns ---
            migrationBuilder.Sql("ALTER TABLE goals ADD COLUMN IF NOT EXISTS suggested_by_id UUID NULL REFERENCES users(id);");
            migrationBuilder.Sql("ALTER TABLE goals ADD COLUMN IF NOT EXISTS timeframe VARCHAR(20) NULL;");
            migrationBuilder.Sql("ALTER TABLE goals ADD COLUMN IF NOT EXISTS skill_category_id UUID NULL REFERENCES skill_categories(id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_goals_suggested_by_id ON goals (suggested_by_id) WHERE suggested_by_id IS NOT NULL;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_goals_skill_category_id ON goals (skill_category_id) WHERE skill_category_id IS NOT NULL;");

            // --- positions.sort_order ---
            migrationBuilder.Sql("ALTER TABLE positions ADD COLUMN IF NOT EXISTS sort_order INTEGER NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_positions_career_track_sort_order ON positions (career_track_id, sort_order);");

            // --- feedback.goal_id nullable ---
            migrationBuilder.Sql("ALTER TABLE feedback ALTER COLUMN goal_id DROP NOT NULL;");

            // --- indexes that may be missing ---
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_skills_category_id ON skills (category_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_skill_levels_skill_id ON skill_levels (skill_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_position_to_skill_skill_id ON position_to_skill (skill_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_position_to_skill_skill_level_id ON position_to_skill (skill_level_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_position_to_skill_position_skill_active ON position_to_skill (position_id, skill_id) WHERE is_deleted = false;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_goal_tasks_goal_id ON goal_tasks (goal_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_career_tracks_career_path_id ON career_tracks (career_path_id);");

            // --- foreign keys that may be missing ---
            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_career_tracks_career_paths_career_path_id') THEN
    ALTER TABLE career_tracks ADD CONSTRAINT FK_career_tracks_career_paths_career_path_id FOREIGN KEY (career_path_id) REFERENCES career_paths(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_goal_tasks_goals_goal_id') THEN
    ALTER TABLE goal_tasks ADD CONSTRAINT FK_goal_tasks_goals_goal_id FOREIGN KEY (goal_id) REFERENCES goals(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_skill_levels_skills_skill_id') THEN
    ALTER TABLE skill_levels ADD CONSTRAINT FK_skill_levels_skills_skill_id FOREIGN KEY (skill_id) REFERENCES skills(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_skills_skill_categories_category_id') THEN
    ALTER TABLE skills ADD CONSTRAINT FK_skills_skill_categories_category_id FOREIGN KEY (category_id) REFERENCES skill_categories(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_positions_career_tracks_career_track_id') THEN
    ALTER TABLE positions ADD CONSTRAINT FK_positions_career_tracks_career_track_id FOREIGN KEY (career_track_id) REFERENCES career_tracks(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_position_to_skill_positions_position_id') THEN
    ALTER TABLE position_to_skill ADD CONSTRAINT FK_position_to_skill_positions_position_id FOREIGN KEY (position_id) REFERENCES positions(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_position_to_skill_skills_skill_id') THEN
    ALTER TABLE position_to_skill ADD CONSTRAINT FK_position_to_skill_skills_skill_id FOREIGN KEY (skill_id) REFERENCES skills(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_position_to_skill_skill_levels_skill_level_id') THEN
    ALTER TABLE position_to_skill ADD CONSTRAINT FK_position_to_skill_skill_levels_skill_level_id FOREIGN KEY (skill_level_id) REFERENCES skill_levels(id) ON DELETE RESTRICT;
  END IF;
END $$;");

            // --- employee_skill_evidence table ---
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS employee_skill_evidence (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_to_skill_id UUID         NOT NULL REFERENCES employee_to_skill(id) ON DELETE RESTRICT,
    feedback_id          UUID         NOT NULL REFERENCES feedback(id) ON DELETE RESTRICT,
    created_by           UUID         NULL,
    created_at           TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by          UUID         NULL,
    modified_at          TIMESTAMPTZ  NULL,
    is_deleted           BOOLEAN      NOT NULL DEFAULT FALSE,
    deleted_by           UUID         NULL,
    deleted_at           TIMESTAMPTZ  NULL
);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_employee_skill_evidence_employee_to_skill_id ON employee_skill_evidence (employee_to_skill_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_employee_skill_evidence_feedback_id ON employee_skill_evidence (feedback_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_employee_skill_evidence_assessment_feedback ON employee_skill_evidence (employee_to_skill_id, feedback_id);");

            // --- goal_deletion_requests table ---
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS goal_deletion_requests (
    id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    goal_id          UUID         NOT NULL REFERENCES goals(id) ON DELETE RESTRICT,
    requested_by_id  UUID         NOT NULL REFERENCES users(id),
    status           TEXT         NOT NULL DEFAULT 'pending',
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
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_goal_deletion_requests_goal_id ON goal_deletion_requests (goal_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UQ_goal_deletion_requests_pending ON goal_deletion_requests (goal_id) WHERE status = 'pending' AND is_deleted = FALSE;");

            // --- review_cycles table ---
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS review_cycles (
    id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    title               TEXT         NOT NULL,
    description         TEXT         NULL,
    subject_employee_id UUID         NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    department_id       UUID         NOT NULL,
    status              TEXT         NOT NULL,
    opened_at           TIMESTAMPTZ  NULL,
    started_at          TIMESTAMPTZ  NULL,
    closed_at           TIMESTAMPTZ  NULL,
    created_by          UUID         NULL,
    created_at          TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by         UUID         NULL,
    modified_at         TIMESTAMPTZ  NULL,
    is_deleted          BOOLEAN      NOT NULL DEFAULT FALSE,
    deleted_by          UUID         NULL,
    deleted_at          TIMESTAMPTZ  NULL
);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_review_cycles_subject_employee_id ON review_cycles (subject_employee_id);");

            // --- review_nominees table ---
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS review_nominees (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    cycle_id             UUID         NOT NULL REFERENCES review_cycles(id) ON DELETE CASCADE,
    reviewer_employee_id UUID         NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    nominated_by         UUID         NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    status               TEXT         NOT NULL,
    created_by           UUID         NULL,
    created_at           TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by          UUID         NULL,
    modified_at          TIMESTAMPTZ  NULL,
    is_deleted           BOOLEAN      NOT NULL DEFAULT FALSE,
    deleted_by           UUID         NULL,
    deleted_at           TIMESTAMPTZ  NULL
);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_review_nominees_nominated_by ON review_nominees (nominated_by);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_review_nominees_reviewer_employee_id ON review_nominees (reviewer_employee_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_review_nominees_cycle_reviewer ON review_nominees (cycle_id, reviewer_employee_id) WHERE is_deleted = FALSE;");

            // --- review_responses table ---
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS review_responses (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    cycle_id             UUID         NOT NULL REFERENCES review_cycles(id) ON DELETE RESTRICT,
    nominee_id           UUID         NOT NULL REFERENCES review_nominees(id) ON DELETE RESTRICT,
    reviewer_employee_id UUID         NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    overall_rating       SMALLINT     NOT NULL,
    comments             TEXT         NOT NULL,
    created_by           UUID         NULL,
    created_at           TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by          UUID         NULL,
    modified_at          TIMESTAMPTZ  NULL,
    is_deleted           BOOLEAN      NOT NULL DEFAULT FALSE,
    deleted_by           UUID         NULL,
    deleted_at           TIMESTAMPTZ  NULL
);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_review_responses_cycle_id ON review_responses (cycle_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_review_responses_reviewer_employee_id ON review_responses (reviewer_employee_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_review_responses_nominee ON review_responses (nominee_id) WHERE is_deleted = FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_career_tracks_career_paths_career_path_id",
                table: "career_tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_goal_tasks_goals_goal_id",
                table: "goal_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_position_to_skill_positions_position_id",
                table: "position_to_skill");

            migrationBuilder.DropForeignKey(
                name: "FK_position_to_skill_skill_levels_skill_level_id",
                table: "position_to_skill");

            migrationBuilder.DropForeignKey(
                name: "FK_position_to_skill_skills_skill_id",
                table: "position_to_skill");

            migrationBuilder.DropForeignKey(
                name: "FK_positions_career_tracks_career_track_id",
                table: "positions");

            migrationBuilder.DropForeignKey(
                name: "FK_skill_levels_skills_skill_id",
                table: "skill_levels");

            migrationBuilder.DropForeignKey(
                name: "FK_skills_skill_categories_category_id",
                table: "skills");

            migrationBuilder.DropTable(
                name: "employee_skill_evidence");

            migrationBuilder.DropTable(
                name: "goal_deletion_requests");

            migrationBuilder.DropTable(
                name: "review_responses");

            migrationBuilder.DropTable(
                name: "review_nominees");

            migrationBuilder.DropTable(
                name: "review_cycles");

            migrationBuilder.DropIndex(
                name: "IX_skills_category_id",
                table: "skills");

            migrationBuilder.DropIndex(
                name: "IX_skill_levels_skill_id",
                table: "skill_levels");

            migrationBuilder.DropIndex(
                name: "IX_positions_career_track_sort_order",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_position_to_skill_skill_id",
                table: "position_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_position_to_skill_skill_level_id",
                table: "position_to_skill");

            migrationBuilder.DropIndex(
                name: "UX_position_to_skill_position_skill_active",
                table: "position_to_skill");

            migrationBuilder.DropIndex(
                name: "IX_goal_tasks_goal_id",
                table: "goal_tasks");

            migrationBuilder.DropIndex(
                name: "IX_career_tracks_career_path_id",
                table: "career_tracks");

            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "skill_category_id",
                table: "goals");

            migrationBuilder.DropColumn(
                name: "suggested_by_id",
                table: "goals");

            migrationBuilder.DropColumn(
                name: "timeframe",
                table: "goals");

            migrationBuilder.AlterColumn<Guid>(
                name: "goal_id",
                table: "feedback",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
