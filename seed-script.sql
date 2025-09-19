CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE audit_logs (
    id uuid NOT NULL,
    actor_id uuid,
    action text NOT NULL,
    target_type text,
    target_id uuid,
    detail text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_audit_logs" PRIMARY KEY (id)
);

CREATE TABLE career_paths (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_career_paths" PRIMARY KEY (id)
);

CREATE TABLE career_tracks (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    career_path_id uuid NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_career_tracks" PRIMARY KEY (id)
);

CREATE TABLE departments (
    id uuid NOT NULL,
    name text NOT NULL,
    code text,
    description text,
    parent_department_id uuid,
    manager_id uuid,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_departments" PRIMARY KEY (id)
);

CREATE TABLE employee_to_skill (
    id uuid NOT NULL,
    employee_id uuid NOT NULL,
    skill_id uuid NOT NULL,
    skill_level_id uuid,
    persist_value numeric,
    source text,
    effective_date timestamp with time zone,
    is_target boolean NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_employee_to_skill" PRIMARY KEY (id)
);

CREATE INDEX "IX_employee_to_skill_employee_id" ON employee_to_skill (employee_id);

CREATE INDEX "IX_employee_to_skill_skill_id" ON employee_to_skill (skill_id);

CREATE INDEX "IX_employee_to_skill_skill_level_id" ON employee_to_skill (skill_level_id);

CREATE UNIQUE INDEX "UX_employee_to_skill_employee_skill_effective" ON employee_to_skill (employee_id, skill_id, effective_date);

CREATE TABLE employees (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    manager_id uuid,
    title text,
    department text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_employees" PRIMARY KEY (id)
);

CREATE INDEX "IX_employees_user_id" ON employees (user_id);

CREATE INDEX "IX_employees_manager_id" ON employees (manager_id);

CREATE TABLE feedback (
    id uuid NOT NULL,
    goal_id uuid NOT NULL,
    project_id uuid,
    from_employee_id uuid NOT NULL,
    to_employee_id uuid NOT NULL,
    content text NOT NULL,
    rating integer,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_feedback" PRIMARY KEY (id)
);

CREATE TABLE feedback_requests (
    id uuid NOT NULL,
    requestor_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    project_id uuid,
    goal_id uuid,
    message text,
    due_date timestamp with time zone,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_feedback_requests" PRIMARY KEY (id)
);

CREATE TABLE goal_tasks (
    id uuid NOT NULL,
    goal_id uuid NOT NULL,
    title text NOT NULL,
    description text,
    deadline timestamp with time zone,
    is_completed boolean NOT NULL,
    completed_at timestamp with time zone,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_goal_tasks" PRIMARY KEY (id)
);

CREATE TABLE goals (
    id uuid NOT NULL,
    employee_id uuid NOT NULL,
    related_skill_id uuid,
    related_skill_level_id uuid,
    title text NOT NULL,
    description text,
    status text NOT NULL DEFAULT 'open',
    deadline date,
    is_completed boolean NOT NULL DEFAULT FALSE,
    completed_at timestamp with time zone,
    progress_percent numeric(5,2) NOT NULL DEFAULT 0.0,
    priority smallint,
    visibility text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_goals" PRIMARY KEY (id)
);

CREATE INDEX "IX_goals_employee_id" ON goals (employee_id);

ALTER TABLE goals ADD CONSTRAINT "FK_goals_employees_employee_id" FOREIGN KEY (employee_id) REFERENCES employees (id) ON DELETE CASCADE;

CREATE TABLE locations (
    id uuid NOT NULL,
    name text NOT NULL,
    address text,
    city text,
    region text,
    country text,
    postal_code text,
    timezone text,
    contact_phone text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_locations" PRIMARY KEY (id)
);

CREATE TABLE position_to_skill (
    id uuid NOT NULL,
    position_id uuid NOT NULL,
    skill_id uuid NOT NULL,
    skill_level_id uuid NOT NULL,
    weight numeric,
    is_mandatory boolean NOT NULL,
    rationale text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_position_to_skill" PRIMARY KEY (id)
);

CREATE INDEX "IX_position_to_skill_position_id" ON position_to_skill (position_id);

CREATE INDEX "IX_position_to_skill_skill_id" ON position_to_skill (skill_id);

CREATE INDEX "IX_position_to_skill_skill_level_id" ON position_to_skill (skill_level_id);

CREATE UNIQUE INDEX "UX_position_to_skill_position_skill" ON position_to_skill (position_id, skill_id);

CREATE TABLE positions (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    expectations text,
    career_track_id uuid NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_positions" PRIMARY KEY (id)
);

CREATE TABLE project_roles (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    position_id uuid,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_project_roles" PRIMARY KEY (id)
);

CREATE TABLE project_teams (
    id uuid NOT NULL,
    project_id uuid NOT NULL,
    project_role_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_project_teams" PRIMARY KEY (id)
);

CREATE INDEX "IX_project_teams_project_id" ON project_teams (project_id);

CREATE INDEX "IX_project_teams_project_role_id" ON project_teams (project_role_id);

CREATE INDEX "IX_project_teams_employee_id" ON project_teams (employee_id);

CREATE UNIQUE INDEX "UX_project_teams_project_role_employee" ON project_teams (project_id, project_role_id, employee_id);

CREATE TABLE projects (
    id uuid NOT NULL,
    code text NOT NULL,
    title text NOT NULL,
    description text,
    owner_id uuid,
    sponsor_id uuid,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_projects" PRIMARY KEY (id)
);

CREATE INDEX "IX_projects_owner_id" ON projects (owner_id);

CREATE INDEX "IX_projects_sponsor_id" ON projects (sponsor_id);

CREATE UNIQUE INDEX "UX_projects_code" ON projects (code);

CREATE TABLE skill_categories (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_skill_categories" PRIMARY KEY (id)
);

CREATE TABLE skill_levels (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    skill_id uuid NOT NULL,
    value integer NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_skill_levels" PRIMARY KEY (id)
);

CREATE INDEX "IX_skill_levels_skill_id" ON skill_levels (skill_id);

CREATE TABLE skills (
    id uuid NOT NULL,
    title text NOT NULL,
    description text,
    category_id uuid NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_skills" PRIMARY KEY (id)
);

CREATE INDEX "IX_skills_category_id" ON skills (category_id);

CREATE TABLE users (
    id uuid NOT NULL,
    user_name text NOT NULL,
    password_hash text NOT NULL,
    display_name text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_users" PRIMARY KEY (id)
);

CREATE UNIQUE INDEX "UX_users_user_name" ON users (user_name);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250910193406_CreateDatabaseSchema', '9.0.0');

COMMIT;

