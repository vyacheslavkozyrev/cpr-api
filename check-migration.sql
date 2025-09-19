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

INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product management, design and user experience', FALSE, NULL, NULL, 'Product');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering, architecture and platform roles', FALSE, NULL, NULL, 'Technology');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal affairs, compliance and risk management', FALSE, NULL, NULL, 'Legal');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'HR, people operations and employee development', FALSE, NULL, NULL, 'People');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Information security, cybersecurity and data protection', FALSE, NULL, NULL, 'Security');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Financial planning, reporting and analysis', FALSE, NULL, NULL, 'Finance');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality assurance, testing and process improvement', FALSE, NULL, NULL, 'Quality Assurance');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Brand management, digital marketing and customer acquisition', FALSE, NULL, NULL, 'Marketing');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Business operations, process optimization and logistics', FALSE, NULL, NULL, 'Operations');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales, business development and account management', FALSE, NULL, NULL, 'Sales');

INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('01d80d21-b452-4762-95b0-fa3ba40ca3eb', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('057ae87d-f059-47cb-80fa-b2a2a09a5208', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within security career path', FALSE, NULL, NULL, 'Security Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('075d4687-8506-47fa-9024-607245b441e6', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('084e66a3-6926-4be5-a1ab-dab432dab60e', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('087f36b0-af05-4501-a70d-3493009515c8', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within people career path', FALSE, NULL, NULL, 'People Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0aa14197-4cc8-409c-8c89-2235b68864da', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within technology career path', FALSE, NULL, NULL, 'Technology Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0af3da54-4cdb-4482-b9ec-f6b6d2fcf1c4', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within security career path', FALSE, NULL, NULL, 'Security Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0b37a4b6-4d25-4613-a069-1196ccfeba17', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within operations career path', FALSE, NULL, NULL, 'Operations Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0c44d3ea-832e-49fc-90b7-b7a33fde83e9', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0c56c293-d35e-429c-8f9d-c1a9e15a67fd', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within legal career path', FALSE, NULL, NULL, 'Legal Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0e8d52f2-17b3-4aaa-a73b-1bc42ae91db2', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within product career path', FALSE, NULL, NULL, 'Product Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('109ee243-0ff8-4faa-be7b-51dffab543df', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within finance career path', FALSE, NULL, NULL, 'Finance Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('18fe3ad0-e6ea-4ad6-a18b-64ae97995443', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within technology career path', FALSE, NULL, NULL, 'Technology Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('19c06f1c-1266-4e50-a1e2-d9de4036d000', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within sales career path', FALSE, NULL, NULL, 'Sales Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('19c3af76-c894-4315-be0d-7e6bd51a0b45', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('1b18e36b-2074-4bb9-b3a4-914e139457b5', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('27753f71-2290-4e16-863d-df06074f903a', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2c48e691-7089-43e6-8282-755bf77cf94f', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2d1052b0-a490-4d35-8040-52ec3f622872', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within security career path', FALSE, NULL, NULL, 'Security Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2e9bf0ab-3c96-4c1c-b679-7f0510c75915', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within security career path', FALSE, NULL, NULL, 'Security Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2f8fae4b-46ee-4671-aac8-eadd697eb9d0', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within finance career path', FALSE, NULL, NULL, 'Finance Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3326d942-0e69-461e-a614-24e3090fcc6e', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within sales career path', FALSE, NULL, NULL, 'Sales Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('34e669d2-a973-4f04-b8ad-050f5c358e67', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within people career path', FALSE, NULL, NULL, 'People Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('359bdacc-a895-47f9-bfe4-1e9de7b98276', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within technology career path', FALSE, NULL, NULL, 'Technology Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('37d32f29-998f-4457-8f1e-b765282c488b', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('39f37d03-61b3-4b17-8442-ecb353866082', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within finance career path', FALSE, NULL, NULL, 'Finance Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3c59c8d5-426a-48fa-8503-e6f12e89bd78', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within technology career path', FALSE, NULL, NULL, 'Technology Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('40092886-d27b-4749-8b69-066b1a0f73db', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within operations career path', FALSE, NULL, NULL, 'Operations Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4112ad37-96d1-4dcc-ac7f-15d374b2d0af', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within finance career path', FALSE, NULL, NULL, 'Finance Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('446aa37c-aa90-41a7-9f76-2dec7b5d3dfc', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within technology career path', FALSE, NULL, NULL, 'Technology Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('46202264-44f6-4b0d-ac35-734f003fdaf3', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within operations career path', FALSE, NULL, NULL, 'Operations Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('49748a92-51b3-403a-b1ff-7a5030e4512e', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4b832ad4-eee9-40bb-a5d4-175d7e857f08', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within technology career path', FALSE, NULL, NULL, 'Technology Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4ca15725-3780-40d2-8e1e-58850bdfe7eb', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within people career path', FALSE, NULL, NULL, 'People Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4f5059c6-62b3-4d90-919c-ade9996b6453', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4f6feed0-1842-476f-87f3-276b93be8c2a', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within people career path', FALSE, NULL, NULL, 'People Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4f86052f-9bc3-48e4-96a1-4b14b4284678', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within technology career path', FALSE, NULL, NULL, 'Technology Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('535ace92-b305-4691-a0d2-f240d5ec5e0f', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within product career path', FALSE, NULL, NULL, 'Product Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5c6b67f2-582f-40c9-8768-29b2756f95bd', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within finance career path', FALSE, NULL, NULL, 'Finance Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6028bd89-f700-4191-b370-44ea96707444', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within sales career path', FALSE, NULL, NULL, 'Sales Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('631d81c0-f8cd-45ae-8437-6f1ab2cb3bf3', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within product career path', FALSE, NULL, NULL, 'Product Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('634a172c-8048-4c72-ad36-50247906774f', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within finance career path', FALSE, NULL, NULL, 'Finance Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('668cfa26-b9d7-4083-acf7-a58ad5e20bc7', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within legal career path', FALSE, NULL, NULL, 'Legal Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6ec1e453-250d-4707-8eaa-0d7daf6e5e54', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within people career path', FALSE, NULL, NULL, 'People Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6ee627f2-1800-442a-a365-397b0b554774', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within product career path', FALSE, NULL, NULL, 'Product Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7083dd82-e896-45c1-b55c-adf97e70c515', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within operations career path', FALSE, NULL, NULL, 'Operations Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('72cca803-8fa2-4021-bffa-a2e2cc5a875d', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within operations career path', FALSE, NULL, NULL, 'Operations Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('73b57b2e-cd73-4023-b127-40e3b62515eb', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within finance career path', FALSE, NULL, NULL, 'Finance Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('743e5278-bbb9-4eff-b146-52cd207da37e', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within sales career path', FALSE, NULL, NULL, 'Sales Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('77560c9e-9c9b-4663-8a97-b9e1a4090379', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within operations career path', FALSE, NULL, NULL, 'Operations Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('79f0e8d7-d15d-4d2d-be8f-cc75bd3fa4d6', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within operations career path', FALSE, NULL, NULL, 'Operations Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7cc1254e-299d-47d3-9893-833a6ed6d362', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7d3e116d-072a-4c93-ac69-cbe2f367d3dd', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within product career path', FALSE, NULL, NULL, 'Product Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8193ed82-881d-4956-a42d-4b195814ca6e', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within security career path', FALSE, NULL, NULL, 'Security Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('841ef2fb-9b6a-4878-83fc-69d34b8a82c0', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within security career path', FALSE, NULL, NULL, 'Security Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('842ace88-6d91-42f9-8ea3-e2a19295222d', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within technology career path', FALSE, NULL, NULL, 'Technology Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('88a35334-b477-4786-9c55-fc7d738021c1', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within people career path', FALSE, NULL, NULL, 'People Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8d00633a-a205-47df-ae60-010154dd2147', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8f60018e-e512-4472-809e-87093653f6df', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within legal career path', FALSE, NULL, NULL, 'Legal Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('90af1452-db9d-4106-8048-feedffe7885a', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within legal career path', FALSE, NULL, NULL, 'Legal Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('95ee864d-6280-4f6b-81ca-c5159a9bead2', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within sales career path', FALSE, NULL, NULL, 'Sales Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('96090724-fa83-41e2-914d-2a60514ab0f9', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('97bc91d6-4f7a-49b5-a227-192d580af131', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within people career path', FALSE, NULL, NULL, 'People Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9b86ecce-549d-4f9c-8b62-44fe330bb71a', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within legal career path', FALSE, NULL, NULL, 'Legal Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9bcfeabb-74ee-4de0-84be-4b2f87fd4c9c', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within product career path', FALSE, NULL, NULL, 'Product Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9d8296dd-f02f-413c-9758-3090e3ae8584', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9e316f53-5eff-4551-bd7e-7765b361b185', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within legal career path', FALSE, NULL, NULL, 'Legal Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9fc86e3b-68db-47ca-b516-b7994df96e87', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within technology career path', FALSE, NULL, NULL, 'Technology Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a4031fbe-d3e3-4004-8788-9dff4b926459', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within finance career path', FALSE, NULL, NULL, 'Finance Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a6a289fd-74a9-431e-bb1f-6c03746527aa', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within legal career path', FALSE, NULL, NULL, 'Legal Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a848c8a0-a4e7-4205-9403-bc48f487e375', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('abcd7ed5-abbc-4f9d-b57f-d2a96d204b00', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within security career path', FALSE, NULL, NULL, 'Security Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ae1ca11a-481f-4a5c-919d-eeb54be217b0', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('afd9884b-7402-48f7-baac-4d6b3b5d81e3', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within product career path', FALSE, NULL, NULL, 'Product Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b2983196-8737-42ff-a996-072d5315a331', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within security career path', FALSE, NULL, NULL, 'Security Track 1');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b2bda2ae-4f5a-4a32-b23c-7f98562f4de5', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within legal career path', FALSE, NULL, NULL, 'Legal Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b5f6b3c3-a0e5-4366-8932-3b1d159f68f4', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within operations career path', FALSE, NULL, NULL, 'Operations Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b692ab95-02b0-4013-81ce-3117c1d985d4', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within sales career path', FALSE, NULL, NULL, 'Sales Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b7e7a9bc-1016-4040-90f5-876d13972a30', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within sales career path', FALSE, NULL, NULL, 'Sales Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bb207905-df3b-4e61-b587-6a445a4451f4', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within legal career path', FALSE, NULL, NULL, 'Legal Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bbe33295-9872-4b15-8328-0317a62074e4', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within people career path', FALSE, NULL, NULL, 'People Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c15202cc-92d1-4b21-8453-166c59f5a3f2', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within operations career path', FALSE, NULL, NULL, 'Operations Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c3d7e8b4-3412-42ae-ab8a-b676aa571afe', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within security career path', FALSE, NULL, NULL, 'Security Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c6008d6f-498f-4a21-87d1-b0d60cc7a844', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within people career path', FALSE, NULL, NULL, 'People Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c90efa4b-2682-44ff-bfba-b12dd6e53a0d', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 3 within sales career path', FALSE, NULL, NULL, 'Sales Track 3');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cb25e565-399f-468e-9923-92f597ddbce0', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within finance career path', FALSE, NULL, NULL, 'Finance Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cd37decb-e727-48a3-b3d0-0a79f70e165f', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within operations career path', FALSE, NULL, NULL, 'Operations Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d1570a59-b284-4460-9c9a-3ad615da3a3e', 'dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 9 within marketing career path', FALSE, NULL, NULL, 'Marketing Track 9');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d64ded22-f468-444a-a7ba-228f2f5cfaff', 'cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within finance career path', FALSE, NULL, NULL, 'Finance Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('dd9903eb-6a23-445e-b22c-a0f5c4b756de', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 8 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 8');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e07ac01e-cd87-46e6-ae47-9cc0250cd042', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within product career path', FALSE, NULL, NULL, 'Product Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e1bde287-61b8-43bd-b7c5-423708c80d39', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within sales career path', FALSE, NULL, NULL, 'Sales Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e49bfa28-c44f-46b3-a003-9be23b5c956a', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 7 within product career path', FALSE, NULL, NULL, 'Product Track 7');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e8c0a182-de39-477d-9bc7-ce1e874a4d60', 'aaa11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within product career path', FALSE, NULL, NULL, 'Product Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ecb0c777-9c1d-492d-bb63-ff232950a849', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 4 within people career path', FALSE, NULL, NULL, 'People Track 4');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ece34dfe-fb63-46b4-ada7-bd7302a9ea7f', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 2 within security career path', FALSE, NULL, NULL, 'Security Track 2');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('efc3a689-7868-47c4-b728-62b8fbf4940b', 'bbb22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 10 within legal career path', FALSE, NULL, NULL, 'Legal Track 10');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f191c490-56a8-4aa4-8780-c98b704508ae', 'ddd44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 5 within quality assurance career path', FALSE, NULL, NULL, 'Quality Assurance Track 5');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f1fae3d0-f213-43fd-b13a-37fefbed00a8', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 6 within technology career path', FALSE, NULL, NULL, 'Technology Track 6');
INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f6d49acf-9fb2-4426-870a-57439df6072d', 'ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Specialized track 1 within sales career path', FALSE, NULL, NULL, 'Sales Track 1');

INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff00000-0000-0000-0000-000000000000', 'QA', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality assurance and testing', FALSE, NULL, NULL, NULL, 'Quality Assurance', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff11111-1111-1111-1111-111111111111', 'ENG', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Software development and engineering', FALSE, NULL, NULL, NULL, 'Engineering', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff22222-2222-2222-2222-222222222222', 'HR', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'People operations and employee development', FALSE, NULL, NULL, NULL, 'Human Resources', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff33333-3333-3333-3333-333333333333', 'FIN', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Financial planning and reporting', FALSE, NULL, NULL, NULL, 'Finance', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff44444-4444-4444-4444-444444444444', 'MKT', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Brand management and customer acquisition', FALSE, NULL, NULL, NULL, 'Marketing', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff55555-5555-5555-5555-555555555555', 'OPS', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Business operations and logistics', FALSE, NULL, NULL, NULL, 'Operations', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff66666-6666-6666-6666-666666666666', 'SAL', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales and business development', FALSE, NULL, NULL, NULL, 'Sales', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff77777-7777-7777-7777-777777777777', 'PRD', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product management and design', FALSE, NULL, NULL, NULL, 'Product', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff88888-8888-8888-8888-888888888888', 'LEG', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal affairs and compliance', FALSE, NULL, NULL, NULL, 'Legal', NULL);
INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
VALUES ('fff99999-9999-9999-9999-999999999999', 'SEC', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Information security and data protection', FALSE, NULL, NULL, NULL, 'Security', NULL);

INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('004e1f8b-1ea3-4e27-a373-ed82f85147cc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 43', '679add6e-6c29-4e00-b6a5-b69c8e0f3445');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('0353f880-f993-4b3a-a7c2-41e7c58f0aa6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 55', 'c6874b28-e2fa-4835-8e8f-159bd5067091');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('03c21dc8-d3cf-4d69-91fa-6e85c99c26e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 45', '45f0eaae-b3eb-4261-a430-4d9e94ec8e0d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('05532608-3d3f-4e0a-8a18-39565f10df9c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 50', 'e9741b9b-3c66-4462-af46-297810b29403');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('0a0bcaa0-7069-45e5-859d-86c0ac3c6804', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 69', 'd670f2cf-66a6-4cb6-947f-062c7b089c8d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('0b1b37d7-776b-4564-8be1-0b3a38ef22e4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 20', 'bf428236-361c-4ade-995d-21a62feec86f');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('0b7715cf-b9ad-4da2-b4ab-b2c681ad078b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 90', 'c7746e91-a5e8-4f8b-9f22-f48374ffa2a4');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('0b797477-9c18-4a58-ab94-b888a67db592', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 35', '7567ad7a-174e-461c-bd88-e7489db10317');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('0bdca7e8-6a57-4bc2-aeaa-4eea19fb6a3c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 82', '5d70d7d5-570e-46fe-91ce-2d6081b365aa');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('101791b3-415b-4942-b35d-f5878827b987', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 6', '59a9897a-52ce-4fe7-b33d-b0fb335f6856');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('123b2cce-7824-46c4-bab9-55c284ca03aa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 95', '23589491-285a-4dca-9548-81a8adaa8a7c');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('13c73a90-3759-4c49-af6b-04538cebb314', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 13', '8beab0bb-eb5e-4973-bfa9-08ecb892fd61');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('16121f72-74ca-450d-8ce1-2c40e3031506', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 59', 'c213543d-0ad6-44e5-b302-1f3cc7dab759');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('223493d5-a3b9-4277-b37f-adab0c77f443', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 72', '13e3f003-2aac-4499-bd46-66a4112dfdce');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('2bd724f4-8349-430f-87c5-fce64def71b1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 3', '7c65183a-c662-45aa-bcb6-cb7ff6a80b01');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('2d6dfacf-20cf-4854-add5-9f7f7d572826', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 31', '1cd3b312-e66e-48c3-851c-3179688f493d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('2ed94013-ce0c-47d6-bbc9-e61084de19c8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 79', '065e1d39-b8d4-4bed-9f39-2370546a1c84');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('318e26a5-2ad7-41c5-aef7-7b42eceded66', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 63', '3b75db0b-ffbc-4871-b49d-b16513d79757');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('32b9c245-dc6f-4e8d-9c76-51ed4ab73a7d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 14', '42d6cb94-638f-4255-8a77-ba472f132c69');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('361f3b8e-a774-4554-8424-2140035dbffd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 4', '68625ab6-f8eb-4141-a4b9-95463c12f121');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('3924bf94-4851-4c14-8546-35eb99aed336', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 89', 'ea8854b2-ea06-4d1f-872a-9495c2f7d31b');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('3f767847-3128-4b42-8b95-c9b9e9fa6abb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 51', 'abcbeac8-bd31-454b-bad3-9f1e8b749e1b');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('414aa2ea-c083-449a-bed1-ea20ca6be44f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 80', '910d0646-fb85-4460-a4d5-91473bcfb941');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('46f4f5f5-d4e0-47a6-bbdf-8da0e4093f0a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 9', 'd892e4e0-d4d7-4b53-9aba-3f1e91ac1149');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('4d2efa5e-919d-4913-bdd1-c075b511d574', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 38', '4cd3ad7d-4777-4abe-894a-84209f652c1d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('51b0ebe7-d112-4d9f-8f7e-7ec50d1751dd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 26', 'a2867868-64f3-4734-bb97-362d2d898a85');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('5298a016-efa6-4922-ab25-a3d6e9e6b9fb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 92', 'cccd7844-ee64-44c7-962b-45b42ff14212');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('53664c8e-4658-470d-8aa2-b53a5246ecf4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 36', 'bb2d30f2-b03c-43b1-b485-227ecb68bfc3');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('538d2193-7ee9-4596-b47b-2cab442241ce', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 58', 'ef8fa138-0bcf-40d4-9857-52723dff774a');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('54e1d437-1af1-4911-b20d-899ea07c1e67', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 74', '6f8cbe75-7a03-4c26-8c2f-e6e3b1d025e2');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('555ba245-4872-42b0-84f8-c5c14f921114', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 61', '0d53a27f-752d-4b51-af43-e9d370949d57');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('563e1211-7e1b-4e35-b96b-a0aec063abec', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 37', 'e4b864d9-4c49-45db-8262-c56f86e7730b');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('5732e2ee-4230-49c6-bf54-2b990011d9a8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 86', '8fe991c4-3d7b-45d4-b2b4-37a486838be0');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('59a87eda-5e6d-43e9-89eb-7cde2ea30a17', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 12', '2e2316a6-f444-48b1-9b5f-c34ef58dc3e5');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('5bb3f91c-67ae-40df-8e91-9c41ec4582f1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 84', 'c5108d70-8b77-4155-a950-46f6943afb04');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('5dee4592-4fbe-48bc-927c-34bfeb31dd64', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 23', '1125eca1-173d-482d-a7a1-3b1e24282a49');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('5e64c2c9-5abf-4c9d-9920-9d776e6d2133', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 81', 'd598d839-15af-40e9-a7f2-b480a75486f9');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('699bb7a3-33c3-48e8-8994-e02f701422d8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 66', '788b5062-ef9d-43ce-abda-2369716d7e2d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('6af87b75-d629-4217-b8f2-b2dd1338a351', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 24', '977f4f1f-b3ce-4244-98fc-2c0d0248de88');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('6b908fa9-96c2-49ad-87d0-30a03b95839e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 30', '20c78aa6-077a-4d3b-a7c5-84d561ec3975');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('6e61ae2f-af9d-4d04-99a2-1bfdd5332726', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 52', '87896109-9ac7-444c-aa79-dfe1dc908a5d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('6f1cfe6d-7d8e-4fad-8e50-410781ebb3af', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 5', '81694c14-a96a-4625-b5e5-a9fd034021af');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('6f45df01-bb10-4d50-90fe-284edd85f3d4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 29', '7522f469-e697-4e02-bb2f-f7f055c9aacb');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('6f56dd2e-ab5c-4bbe-9b65-28e0f3f7b7b0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 17', 'bf694c99-cfa6-4fba-be98-efba28bb4f31');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('7101e13e-ad36-478b-b4eb-07bbd32479a5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 88', '879c8ae6-c1c0-4d16-85fb-5dfe698fd108');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('71398e97-e966-44af-8724-2dbda6eca8a3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 83', '88b6f0d3-298e-4083-84cf-fa7893a0c846');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8531998a-4d25-45ac-a98c-5e6ecb63ef0f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 33', 'ad92cd5e-5599-4f3c-b2e1-39a49dd6b5bc');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('874fbb3e-a090-4837-8807-4cea8ca23f61', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 42', 'cc230776-dc23-4996-9383-15fee9688215');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('88053fd1-f52a-4f98-9b3d-ba6266a7356a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 11', '5950a2be-bdfb-4dcb-9913-1e3e0e022a5c');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8af10ba7-3ac0-4c96-b016-13be8271f2f3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 96', 'a22e8c3a-4ae0-4b59-b766-89a226fa82f0');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8b524197-74a8-44cc-9199-d24c8afa550e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 25', 'b7152c34-8408-4768-9436-7970f81bec0c');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8c3589b9-3557-4415-9bfe-66d4e1536902', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 99', '2af89029-e0a0-4943-880e-3acbb7eefa0a');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8d4b83e0-eb3c-4073-948c-a2dc041244c7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 28', '7649d780-254a-4ccc-8faa-42c0f62529ad');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8e8f85b7-7b64-4711-a0a4-3a92a154334a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 65', '8f6cc827-6382-4883-8aac-6529730ea89e');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8ed860f2-4ade-4537-9a4f-ad83e9a1f321', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 16', 'ce7c6812-3357-4b5e-bd7a-bdae1446396f');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('8f8500f0-29cb-4d60-a0fd-9f21ece4637a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 41', '0e9109a1-e6b7-4ae9-ba51-055d434a2d04');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('90032162-c346-4b93-bdf5-d6c14650a4e7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 62', '52e73634-f68b-4fb5-84f5-18f9c75bc732');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('9a3892bf-d2f7-440b-803d-b99843fd1468', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 46', '8a941ab4-6336-4c8d-bf3e-229b1a8d54d4');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('9a624f40-8977-4855-9f49-33c31f12c640', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 44', 'cfaf89cd-011a-426b-8a30-6e24f3abf592');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('9d5cb25d-6f0c-4bfc-907c-6d6f826feda3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 71', '3a8040f9-653a-47f2-9b4c-bcdc1753efb0');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('9eb5bf5f-0d20-4165-a992-27c461618b76', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 54', '97ab3775-8ab8-4153-940c-ceca4fe44f0f');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('9f0deb21-5cef-4ccc-91d0-b5503e3769d4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 70', '661b1c49-730b-4864-b1d2-e32a14efb767');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('a0e6ec40-b581-435a-8fab-31985268391f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 78', 'abc21110-01fc-45d5-ae0b-f16f9af89595');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('a6487cec-4497-405c-b4dc-475185c4a4c5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 75', 'fb87109f-b928-4b2d-b74e-01aab1f77912');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('a6858d7a-7e72-4f1e-ac1f-fd84654411c8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 2', '9f047b6c-3ffe-4b28-b97b-4e876d05bb9f');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('a6ed3f99-eead-48f5-be61-247a85b4f0cd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 18', '21ee6537-1838-4e51-9d6c-1494cbd54e00');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('a9e796d9-5e39-47fe-9f51-a3f30c738710', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 22', '72376c5a-f5ed-4f0e-9c57-d177336a6952');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('ab597c04-f3d3-4a83-be8b-c8bf16b97fb9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 7', '1020fe8c-f2cb-47ec-9718-c486c1e7b43a');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('b1b47aa1-becc-41a7-85f3-bf165356e538', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 57', '7f1d4ed4-cb41-413b-9e02-2f98944c8118');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('b6f887a0-cacf-4db0-a6bf-4f613dfa6159', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 76', 'd387732d-50a3-43e5-b2cf-82b3a4cc88f7');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('c017f28a-ab66-495c-99bb-c655b5ce153a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 10', 'b04f33f7-c2b2-41e2-8336-b35fe8860828');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('c468e67c-9dbd-4ca8-83ab-ba4720eba7c5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 94', '6a52bb7b-694e-4e28-a37f-e387299a4890');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('c5971978-2348-42fd-b187-02c78ac52cd3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 48', 'b286f0fa-50ed-4df0-8fc5-aafc0525b7b3');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('c6cb874e-fa8c-40a4-8962-c648e176eade', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 87', 'cfafee28-780f-4753-97fe-0de06af70213');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('c6f55d1a-7179-4430-8562-1cc4d28e4f6e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 27', '7dc6d43a-e17c-44c2-807d-4fade9a636cd');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('cbe8fd03-7ac2-4b4b-853f-472ebf20292e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 19', '09b072bb-d88b-4976-b408-0227f54de525');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('cf43e9dc-c824-4196-a635-7c92ee29eda4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Sales', FALSE, NULL, NULL, NULL, 'Employee Role 56', 'b75dfc87-e4b3-4ae1-8b97-cce9dbe4c700');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('d0b9a22b-4f4c-475f-b977-e3c721f77aa3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 97', 'ba0de2e8-7ed2-420d-b519-dc69a28d08b6');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('d4c5b83e-3eb6-45c3-9119-8ef82c2ea71b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 49', 'ab6030a0-83ef-412d-a5f2-77fa99fb6d5c');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('d5a1b0ce-f897-4e8c-b816-ebddc3b36a01', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 93', 'a38fe0db-95c7-41bb-98fb-6af13cf6a1d0');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('d8a4b509-0f46-4dab-b370-de5d53b8f7e0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 85', '0a6d469b-d6c3-4a3b-af43-21fa3142b160');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('d98abbf5-5fec-4839-9538-2cda660be240', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 34', '17cd9849-9b4b-4828-a2c2-a59e9857acd8');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('d9fd7268-0199-49d0-a833-69d666584b65', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 60', '89bf1edb-5085-4891-a332-c41017a2ad6f');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('dd3107b6-97ae-4576-a9c1-1ba0a771ad54', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Marketing', FALSE, NULL, NULL, NULL, 'Employee Role 64', 'd6c17aaa-0535-41e7-ae28-11c1ae8030bf');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('ddc6e574-b557-431f-b24c-898ce48fc738', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 53', '86e2d86b-5eaf-447c-86e8-1be9702a315a');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('de58b515-0fa3-42ed-a97e-5612078f135f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 8', '7bde6111-3017-4bf0-b62c-58ed69bef061');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('dec3bb59-6d4b-4219-9e5b-f9de28dc084e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Human Resources', FALSE, NULL, NULL, NULL, 'Employee Role 32', '50a3d3cd-fff7-4d64-96da-7149e4b72605');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('e1802b50-cb71-4ea8-9e81-65f6e81a4cd7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 47', '971a8b2d-21e8-490f-b110-ae7153f5cbdd');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('e3068b80-0f9e-47c0-8cd9-baa9d7c44b00', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Quality Assurance', FALSE, NULL, NULL, NULL, 'Employee Role 40', 'cf5c93b9-964e-470a-ad3b-73a931c81a97');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('e385f253-ede7-470f-80b1-33a288e09e79', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 68', 'f59238ac-900f-454d-9f18-855e46890a11');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('e51c9a74-f5b5-4b61-b89c-fcd98fd77bce', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 77', '575f1162-7a41-4d3c-89a3-cd1ede2e3ab2');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('f499b92e-d667-4e44-be53-0e580a4a3ab1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 91', '15ef5c19-55c3-49df-89af-f88adfab031f');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('f4c9e71e-3f49-4390-8149-97f6bd497dc3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Operations', FALSE, NULL, NULL, NULL, 'Employee Role 15', '9283b256-1fbc-4a93-88d0-fd7ea76a314d');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('f50ebe71-8658-4c75-896c-26e06a061aa1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Product', FALSE, NULL, NULL, NULL, 'Employee Role 67', '48b22548-6c93-4c5a-b0bb-84ca3e8516c7');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('f686df44-9021-4822-9173-efcdf49efa6d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 1', '7c14523e-98ba-4cd4-bcea-e68c29c19159');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('f701c5f5-9efd-4b41-b555-6e09f177f95e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Employee Role 21', '1a946808-2aac-40d3-af9f-2e98b4aeaf86');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('f828b47e-eea7-48f7-9bc1-94c255edba7c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Finance', FALSE, NULL, NULL, NULL, 'Employee Role 73', '749767c6-d663-445d-952f-1ed4b8dbfacc');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('fd02bbc1-88c1-4788-80e0-8fa7db3c7bc2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Legal', FALSE, NULL, NULL, NULL, 'Employee Role 98', '532163c3-2e39-47d9-a0ae-091cbb55aa14');
INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('fdefb96a-5c8c-474b-8a4c-743c218cc753', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Security', FALSE, NULL, NULL, NULL, 'Employee Role 39', '35b3ea92-f1fd-48fd-9996-fb22dab95f4c');

INSERT INTO locations (id, address, city, contact_phone, country, created_at, created_by, deleted_at, deleted_by, is_deleted, modified_at, modified_by, name, postal_code, region, timezone)
VALUES ('aaa11111-1111-1111-1111-111111111111', NULL, 'New York', NULL, 'USA', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, FALSE, NULL, NULL, 'Headquarters', NULL, NULL, 'America/New_York');
INSERT INTO locations (id, address, city, contact_phone, country, created_at, created_by, deleted_at, deleted_by, is_deleted, modified_at, modified_by, name, postal_code, region, timezone)
VALUES ('aaa22222-2222-2222-2222-222222222222', NULL, 'Remote', NULL, 'Global', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, FALSE, NULL, NULL, 'Remote', NULL, NULL, 'UTC');

INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('06091429-d5c5-47f7-9f85-3034618325c5', '0c60eaad-dab3-426e-92f4-eafcd91db208', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 8', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 8 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('0a3ddc39-af6b-487d-9bac-2d9dd0bee554', 'c5409a70-d866-465e-9d56-3da4684c54d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 7', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 7 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('0b5e0e9d-6920-4037-8312-0f3e78d0088a', '0c60eaad-dab3-426e-92f4-eafcd91db208', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 8', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 8 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('0ecfa16b-8d67-4298-be82-06fb0be2c1a7', '43a12f91-e50a-480d-a865-1085f53adeb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 10', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 10 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('148802ce-fe15-42a7-b6ca-33d9ea320448', 'e4441805-32e1-4714-9ae7-5688c9115ef4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 6', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 6 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('15e1ba17-e786-4a86-bbc1-1ada3f47b6df', 'bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 9', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 9 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('24582967-5515-4000-a390-bce4aa59d460', '0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 2', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 2 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('37ea7352-b7f8-43f0-a377-138fb40a6d0d', 'd533dd42-c223-4b34-9ddd-6e257f8016b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 4', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 4 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('40f80e0b-f128-493a-99ce-5e1c765cc87f', 'e4441805-32e1-4714-9ae7-5688c9115ef4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 6', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 6 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('4152e8ef-9424-49f3-8b86-3dd87b89c8bf', 'c5409a70-d866-465e-9d56-3da4684c54d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 7', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 7 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('48316a63-4812-4a04-ae3c-97248556d3f8', '7875db90-71b6-4e06-a7cf-53a3a77f10a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 5', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 5 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('4c7ded1f-7dd8-47b7-8b63-35020100ba19', 'bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 9', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 9 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('4e064c81-2d4d-4dfa-b95d-419fc13b72ca', '7875db90-71b6-4e06-a7cf-53a3a77f10a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 5', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 5 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('4fea9a06-63a6-4f4e-bc81-e89ba6709295', 'd533dd42-c223-4b34-9ddd-6e257f8016b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 4', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 4 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('5284d661-4c72-40d1-8cc5-56e08a6138d0', '0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 2', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 2 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('570d12e2-911e-4adc-a98a-373e4c8aab53', '22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 1', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 1 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('6073c789-4512-499c-84c6-0195e87c60bc', '0c60eaad-dab3-426e-92f4-eafcd91db208', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 8', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 8 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('6f33e923-834d-4639-a88a-9fdec61215f4', 'bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 9', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 9 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('7573b182-5813-411a-91ee-b5beeef80e1a', 'd533dd42-c223-4b34-9ddd-6e257f8016b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 4', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 4 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('78d6f12b-d42c-4af5-991b-f2b069305f2a', '0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 2', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 2 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('7ad2dbb8-acc3-43bc-a9e5-48c166713822', '0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 2', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 2 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('7b018a08-132c-46ee-bbe1-d4a4320eab2a', '7875db90-71b6-4e06-a7cf-53a3a77f10a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 5', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 5 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('80d23551-e51e-405a-9339-8a6f52a0c6bd', 'd7556322-667b-4062-96e1-c6773eccfc04', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 3', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 3 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('835c2609-0c27-446a-9fe7-779326c9f1f6', 'd533dd42-c223-4b34-9ddd-6e257f8016b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 4', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 4 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('8ef2121e-e2c5-4514-b16a-dda651fb1c50', '22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 1', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 1 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('901c89ea-a073-48c6-ab32-c414914b8f2e', 'bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 9', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 9 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('948cd97b-7376-45ee-91f3-b511a456bb58', 'd533dd42-c223-4b34-9ddd-6e257f8016b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 4', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 4 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('a35f6bd9-652e-42d6-b137-929c3b1bd791', 'd7556322-667b-4062-96e1-c6773eccfc04', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 3', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 3 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('aa00c283-1af1-4397-bc34-2f674bda4852', '22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 1', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 1 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('ab42e7b3-caf3-417d-80f9-62422e089596', '22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 1', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 1 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('ac69cf26-aa02-481c-ac06-5ccd9dfec7f0', 'bdb35ab2-9c08-4a4f-952f-17fbe0b04e9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 9', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 9 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('ad711f5b-08c2-43b3-b503-d9e25c545119', '0c60eaad-dab3-426e-92f4-eafcd91db208', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 8', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 8 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('aeb133ad-e366-4b7a-be19-a86659e423f8', '7875db90-71b6-4e06-a7cf-53a3a77f10a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 5', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 5 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('b28f0718-03af-4a80-9d0b-6034e52a2ef0', 'c5409a70-d866-465e-9d56-3da4684c54d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 7', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 7 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('b4c4445e-2bc2-441b-8fb9-ef5a9e6e0696', '43a12f91-e50a-480d-a865-1085f53adeb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 10', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 10 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('b56b213e-fcc1-4e1c-8653-e24a52a7e28d', '0c60eaad-dab3-426e-92f4-eafcd91db208', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 8', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 8 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('b707fc9a-2442-4ad7-b8bd-0567f87ef5a1', '43a12f91-e50a-480d-a865-1085f53adeb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 10', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 10 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('bf563729-a2eb-4716-ab53-9b1497f2fa2a', 'e4441805-32e1-4714-9ae7-5688c9115ef4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 6', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 6 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('ca70ee84-c88f-4352-b80d-914a9cd33674', '43a12f91-e50a-480d-a865-1085f53adeb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 10', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 10 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('cf1ac4b7-4c13-4a72-8772-211bb1832ae0', 'd7556322-667b-4062-96e1-c6773eccfc04', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 3', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 3 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('d38d1988-8914-4453-826f-063b5bc016c0', 'e4441805-32e1-4714-9ae7-5688c9115ef4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 6', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 6 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('d8f594a3-1e3c-466f-a178-1e16ddb9a2ca', 'e4441805-32e1-4714-9ae7-5688c9115ef4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 2 in Technology Track 6', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 6 Position 2');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('da04a0f8-bf20-45f7-af85-3595cc2f7f15', 'd7556322-667b-4062-96e1-c6773eccfc04', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 3', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 3 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('e3e2e045-ae6e-4629-810d-2720e750eb9c', '0cf603ee-4fb6-42d6-b1f3-e3057bb80c4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 2', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 2 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('e9fa3feb-cf51-411f-adc0-8244e61a831d', 'c5409a70-d866-465e-9d56-3da4684c54d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 7', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 7 Position 1');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('eec819f7-a4f5-4b2a-a1f6-328bb889c6c5', '43a12f91-e50a-480d-a865-1085f53adeb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 10', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 10 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('f2208214-d7c6-4db4-98ea-99e097576ef8', '22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 4 in Technology Track 1', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 1 Position 4');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('f640cd3f-6e9d-4dc8-9c4f-acbbcd34ad24', 'c5409a70-d866-465e-9d56-3da4684c54d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 5 in Technology Track 7', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 7 Position 5');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('f75f7943-1667-4438-a27d-93408f39f49d', '7875db90-71b6-4e06-a7cf-53a3a77f10a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 3 in Technology Track 5', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 5 Position 3');
INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
VALUES ('fab1f5ce-59b0-4261-a359-b4ec60e9f606', 'd7556322-667b-4062-96e1-c6773eccfc04', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Role description for position 1 in Technology Track 3', 'Key expectations and responsibilities for this position', FALSE, NULL, NULL, 'Technology Track 3 Position 1');

INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Technical skills and competencies', FALSE, NULL, NULL, 'Technical');
INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Leadership and communication skills', FALSE, NULL, NULL, 'Leadership');
INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Business acumen and domain knowledge', FALSE, NULL, NULL, 'Business');
INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Interpersonal and behavioral skills', FALSE, NULL, NULL, 'Soft Skills');
INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Industry or domain-specific knowledge', FALSE, NULL, NULL, 'Domain Specific');

INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('002530b1-bd41-4e08-b3df-924cade33bf9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 49', FALSE, NULL, NULL, '743217ce-074d-49c2-8105-71c0cf8cafb1', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('007e94fe-5f6f-4698-be37-6b0e2bbcdf81', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 20', FALSE, NULL, NULL, '67922510-89bd-4788-be19-8c24d2c63817', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0094082b-dcb4-498f-9a7e-93829dc09f5d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 37', FALSE, NULL, NULL, '9c586771-f8ac-4c51-ab18-9fc4210db45f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('00b2d0b5-9fea-43ea-9c59-3d9144a97abd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 17', FALSE, NULL, NULL, '48b29235-9a48-4172-9052-f16830f7cf94', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('010bd328-ce76-4fe3-abc6-98e9bb613f29', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 47', FALSE, NULL, NULL, '0f708d81-d4bb-409e-9459-841cd2b6bd38', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('016a1159-d0d5-46fd-883c-41ebab50b5c4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 19', FALSE, NULL, NULL, '880d648a-128c-417d-95ee-c51551a0e5d2', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0179b093-78d5-43f3-88c9-95da1205f0e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 38', FALSE, NULL, NULL, '852c6a86-9537-4e08-aa0c-48f76dbc82a0', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('02271212-2540-4ac6-9943-655b2da4d7ea', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 42', FALSE, NULL, NULL, '29b1fb90-3731-439c-ad24-57d3d9e532f0', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('026f259a-f9a0-4525-ba90-bba6e8311870', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 26', FALSE, NULL, NULL, 'e90397ca-786d-40c9-ba32-fab67f449831', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('028046da-153c-417d-acf5-21f62b167521', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 30', FALSE, NULL, NULL, '5313ca23-e4f7-4455-8f1e-ca88f8c6d064', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('02df7172-8d0e-4895-8cdf-598e4b92ad6d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 9', FALSE, NULL, NULL, 'f9dde99f-3afc-4322-ba9a-a757c4e8c72f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('02f22437-2fe1-46bc-8e3e-3a1791a0d425', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 9', FALSE, NULL, NULL, '1ee8a770-2a9f-4760-ad48-0cc2d469eba2', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('036e2fee-dc27-46e8-9c17-e2118ea917d2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 8', FALSE, NULL, NULL, 'fb195671-3ba6-4c67-8f4e-67b6079b9e73', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('03ac3e74-e599-43b1-9979-f79dfc851670', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 30', FALSE, NULL, NULL, '88af0997-4e0f-4647-84a4-9bf9aa5d58a3', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('03ba0d0a-030c-405d-94de-bd70b5a5a3e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 14', FALSE, NULL, NULL, 'f42a05b5-18d8-4471-8ccc-9540b146d6b4', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('03c76176-8523-4228-8ef2-ab838b50b006', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 36', FALSE, NULL, NULL, '2f830623-7758-4242-b2d6-630fcf83d0d9', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('03dd5fce-4bde-4a3c-8c97-9a183e243955', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 4', FALSE, NULL, NULL, 'c1c2325c-4801-4f5f-abd6-0ba585f1c338', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('042949ca-fdc2-448d-a999-8b94d1914571', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 3', FALSE, NULL, NULL, '89a11355-99d9-478f-ad27-8a337ef259ff', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0449725a-e737-4ff1-a623-66c5bd8430ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 47', FALSE, NULL, NULL, 'af8b9e0d-c127-47b4-8632-497fcf6c4ff4', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('044b10f8-ce59-4c5b-bcea-1bc1cdcdd469', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 40', FALSE, NULL, NULL, '6ab450b8-2fa9-4ed2-9ebd-92f629542f16', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('04856542-52dd-4301-89b4-9cd41afb37f5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 50', FALSE, NULL, NULL, '431a53a0-3609-42f8-9bf2-f3273f1d95f1', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('04bfa32b-42a6-471d-bd97-936bbadaab4f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 6', FALSE, NULL, NULL, '22a16c96-9a98-481d-bb8c-f6da7b8e3a9d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('04e0d3be-3915-4b0d-91b7-d4de853f2567', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 48', FALSE, NULL, NULL, '04dff112-bec8-4fc2-9ea4-48cd4ab055c2', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('04e34e90-bc89-4549-8524-0618d4915121', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 23', FALSE, NULL, NULL, 'bd73d7ff-4a06-4377-950d-54f7a89e3961', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('04f31fb5-1333-4def-ad87-fde48d2cf8bb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 33', FALSE, NULL, NULL, '358cf230-4e82-46b2-963d-4a41ce9bb0d8', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('05560887-cd6e-4d71-b4a3-71f251146d3b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 18', FALSE, NULL, NULL, '6ee51009-71bd-40b7-aaba-767acb0900a2', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0574f6f3-7d31-406a-acfe-d4e3227f1456', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 28', FALSE, NULL, NULL, '9e800fe9-3f75-42e8-ae66-96884def18c4', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('057bd1a6-3f75-4c59-b714-f0a468be89ef', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 8', FALSE, NULL, NULL, 'fb195671-3ba6-4c67-8f4e-67b6079b9e73', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('058a676e-87b2-4c74-b21f-76e0775b7d5b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 27', FALSE, NULL, NULL, 'e749e4ce-74b5-4106-ae09-933b730fbc1a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('05bb7326-04c6-40ce-ad4c-339b10f14853', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 7', FALSE, NULL, NULL, 'd2f88917-b564-44e1-9c30-7521f7460e76', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('05c614c4-ec84-4554-8d00-64c991133a12', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 14', FALSE, NULL, NULL, '2281c295-a679-4239-893c-9fba737a9c04', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('06132c8d-95d9-4192-afc3-27693c5f4ff3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 18', FALSE, NULL, NULL, '93065003-b808-4d3c-8f1f-6128867bb526', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('064441f5-b4e2-416a-933e-e34a698ec6b5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 36', FALSE, NULL, NULL, '37cb14dc-e7e5-4601-b03c-075de1a9a712', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('06b42289-21b2-4e35-b6a1-8b4d1bc923f1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 24', FALSE, NULL, NULL, '063f3287-9615-4d61-8fea-609c9a9438a1', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('070625ee-66a5-4b71-9771-8cb9f470a5a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 35', FALSE, NULL, NULL, 'e4a64e36-c504-413e-a950-14b20a6b2d0f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('070b3ccf-94ec-4a9f-88ff-79b88670d666', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 46', FALSE, NULL, NULL, 'df956ed0-5446-4664-a60b-f505ce84b039', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0711ddfa-8232-4038-99bc-dd8874418e74', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 16', FALSE, NULL, NULL, '83d16662-a47c-4d3d-a362-dfffd3ad0f35', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('07125db5-6fe2-4d2d-b81e-6ddd748c3064', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 2', FALSE, NULL, NULL, '7518e8f5-51f9-4520-a1e2-cf39d0dba012', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0731f3b0-09af-47df-b44c-5644616fabc9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 46', FALSE, NULL, NULL, 'd9a103d0-26df-4548-abcb-3f4c9dd2348d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('07524b83-0cd7-41a8-b22d-40560797b6c4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 11', FALSE, NULL, NULL, '69927048-8b96-4ab5-8494-1be98976d1c3', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('07827afe-726b-436b-ac14-99196e01b8e1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 33', FALSE, NULL, NULL, 'c145033c-061d-4b8e-bd50-d9dcde18c345', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0795e701-f20b-42f2-b3f9-6fb38e1b1b06', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 23', FALSE, NULL, NULL, 'abb19ae8-ccd2-4653-a0bc-6a3323f66bc8', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0820a456-2de2-4257-8f1e-a7bfdbf058ac', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 50', FALSE, NULL, NULL, '22728788-649a-4e6e-9e17-f4f6ade58a8b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0837403e-0e99-45a2-b344-45752c3b9fe3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 43', FALSE, NULL, NULL, 'dc772f2f-8632-4522-91a4-e247dfcde2ee', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('08416d0c-9b33-4216-a7ad-f7d67f1626b6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 16', FALSE, NULL, NULL, '88419019-c938-4964-a3b2-c733095b7345', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('084396c0-b585-42ab-81a7-229bb66593c6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 25', FALSE, NULL, NULL, 'c6aa7678-3585-4a11-afec-10171ba4fb3d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0868d93a-8ba3-4718-b027-5344b0eeedf2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 1', FALSE, NULL, NULL, '5a6834ac-c850-4f5e-8b44-e3eedccd308a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('088e05d9-a9f4-4567-a353-8eeb57f9795b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 28', FALSE, NULL, NULL, '6c7ffff4-12cf-4dee-a329-5259b050d0de', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('091a2f5e-7177-441d-a5d7-3fe5c33a2fb8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 14', FALSE, NULL, NULL, 'f42a05b5-18d8-4471-8ccc-9540b146d6b4', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0933282b-d2ea-40a6-b6d4-9eecb0f6de54', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 5', FALSE, NULL, NULL, '203170af-0331-470e-87f6-e768415a7a46', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('093816fa-968a-42f2-9e4a-74ffb63e9d80', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 37', FALSE, NULL, NULL, '69c8017c-3b1b-4528-8f91-e0fe0ed87d64', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('093f41af-98c4-4706-ba19-27ff1e939c7b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 31', FALSE, NULL, NULL, '8783d6c9-f7c6-4ed5-8cc8-4e64dc3f73d0', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('09599ebc-407b-410b-8d65-293bfceab28e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 28', FALSE, NULL, NULL, '9e800fe9-3f75-42e8-ae66-96884def18c4', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('097a650d-fbb9-498f-8359-f3b109c29937', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 29', FALSE, NULL, NULL, '19427640-0844-465a-ae50-9bf6e471a0fd', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('099ecf4a-413d-443c-8e7e-30ab834e7f58', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 39', FALSE, NULL, NULL, '32268f3e-ddf6-4cd7-bcf3-1b0f51ed457d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0a1e0a03-c735-406e-bccb-5b220451f480', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 34', FALSE, NULL, NULL, '8ecfce14-10a2-4827-bcd4-823b3d8f3f68', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0a6d97a8-f94a-427d-b4f4-11aed9880447', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 27', FALSE, NULL, NULL, '14406b2e-9c50-4d80-be94-08f3acdbdd97', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0a7da7db-253f-4514-a6c3-3cc2e4ac372f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 34', FALSE, NULL, NULL, '8ecfce14-10a2-4827-bcd4-823b3d8f3f68', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0bd9a010-8c06-4bce-891d-af40b7fe3105', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 21', FALSE, NULL, NULL, '7e8e0278-e829-4f4a-9003-5b9d5269fd3a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0c26cd42-120b-4d50-8160-931537f09b28', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 40', FALSE, NULL, NULL, '306b4c17-b548-4ae8-86a1-99af50efd488', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0c454176-c559-4c5f-b58e-3408e6bf0774', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 39', FALSE, NULL, NULL, 'b819a6ba-6ee1-404e-a388-311c5ce37c54', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0c4b4145-899d-40bf-8c27-201d979b33b6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 1', FALSE, NULL, NULL, '8fc7f790-2963-4fbd-8aed-24e5c9bb3293', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0c5c99d8-edb1-439f-a9bd-021be0389ef4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 29', FALSE, NULL, NULL, '56566050-5df9-4862-aa6a-c1b6b9ac7dcd', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0cb279d7-427f-4d3e-8c60-b01215f1a0e9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 27', FALSE, NULL, NULL, '695c1d0c-819c-4f3a-9978-b0f90aff7dfc', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0cd33ca4-7ede-4763-ad63-60894842064f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 20', FALSE, NULL, NULL, 'de97684d-b1f0-4226-b10d-f4493445b48b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0d05c440-1ea0-4e02-9475-8df2b03cc6cf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 17', FALSE, NULL, NULL, 'd57f2ed7-b9c3-4a69-b22e-b10177d984d0', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0d491e3a-7dc6-44cb-9466-bf6dfb7fb976', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 20', FALSE, NULL, NULL, 'de97684d-b1f0-4226-b10d-f4493445b48b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0d4a7649-3068-4c93-9a7c-9cc11cfc7bcc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 48', FALSE, NULL, NULL, '04dff112-bec8-4fc2-9ea4-48cd4ab055c2', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0dc814df-d686-4f6e-8a75-2e99b526891c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 50', FALSE, NULL, NULL, '7e960356-fef4-4fbb-841a-14c1e59c3aea', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0e04f007-2940-4fc6-acca-556a9b28bc05', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 26', FALSE, NULL, NULL, '70d314c9-5518-4cc1-9bf2-5789ad251f3d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0ed1cd6e-2e28-49bf-89b5-dc13266e2fd3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 42', FALSE, NULL, NULL, 'c2180950-cc0d-428c-bd7b-c4e17ab74ebe', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0f6c5135-da0a-4b24-92f1-b3c7542f5696', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 32', FALSE, NULL, NULL, 'e91bf542-e4b9-48fd-8df1-2d8261ffd37d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('0fa217f4-4911-4a65-8b85-aa3289d3173d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 49', FALSE, NULL, NULL, '606501a2-4493-42b7-af27-765592f2fef5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('10205d9a-4b22-4ae2-aebe-1c256e05755c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 29', FALSE, NULL, NULL, '7baac172-08b3-42d7-94a4-64622e58ea84', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1077d780-1d7f-4c56-967f-0fbfa1ccecd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 26', FALSE, NULL, NULL, 'e90397ca-786d-40c9-ba32-fab67f449831', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('108643c4-8121-4a8a-a787-7ffc0cd8d0a2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 4', FALSE, NULL, NULL, 'f6d6d5be-6640-43e6-b021-ea7464f018e7', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('10bd13f0-e1de-41d0-a12e-3425e165b7f6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 9', FALSE, NULL, NULL, '1ee8a770-2a9f-4760-ad48-0cc2d469eba2', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('10f2bfc8-ab7d-4fdc-865f-e3ab056ae850', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 11', FALSE, NULL, NULL, '54d28ef5-9273-4c94-87f8-469cedb7d54d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('11232640-aa4f-480a-9186-748d56ff41cb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 16', FALSE, NULL, NULL, '83d16662-a47c-4d3d-a362-dfffd3ad0f35', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('112c27d8-ee02-45c6-9a84-b06c4f4ba065', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 25', FALSE, NULL, NULL, '1296febe-1212-4e96-acbc-f74031902d74', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1183c195-dae6-48d0-bffa-98db4ce89240', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 41', FALSE, NULL, NULL, '449926db-cdf1-4b14-9fa8-a2b6766786b7', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('118a9fe4-58c1-4fef-9716-77d7fe68094c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 20', FALSE, NULL, NULL, '51f65c58-c42e-4c52-a251-3d80d6277e68', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('118f8e8a-be23-4e01-9d30-0bee591b53ce', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 50', FALSE, NULL, NULL, '22728788-649a-4e6e-9e17-f4f6ade58a8b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('11b187fb-785d-4b8b-ab45-4d3c664c1ab0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 7', FALSE, NULL, NULL, '086f2228-1683-4376-82a9-69148636a6aa', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('121c1459-6b9e-409d-86b5-225fc7c953c7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 16', FALSE, NULL, NULL, '88419019-c938-4964-a3b2-c733095b7345', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1294c01e-da4e-4ffe-98e2-12c14c1ad9fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 36', FALSE, NULL, NULL, 'b7bee390-307b-42b0-90d2-d0b619e2625f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('129b63b2-a5cc-4f64-b785-25aa1d1e3cd3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 25', FALSE, NULL, NULL, 'dbb082c5-6eab-4588-aabb-c4de2b71306a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('12e8c363-a611-4e61-94d0-cdc74773aecc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 31', FALSE, NULL, NULL, '2e83b1c0-9e04-47f6-a372-34f0afb6a25f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1305c5ce-85df-46db-9f82-db290ded4e76', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 39', FALSE, NULL, NULL, '207b76f8-609c-4b6b-b462-3138eaa91822', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1320dfa4-ef27-4754-bf62-767c322e05cb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 5', FALSE, NULL, NULL, '5e30a962-e6dd-4137-9b04-b85655f9a728', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1328f2f7-8666-46e4-ae25-093c46f4aedb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 46', FALSE, NULL, NULL, 'd9a103d0-26df-4548-abcb-3f4c9dd2348d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1334468b-ea46-4cab-a87f-bc9f1578249d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 10', FALSE, NULL, NULL, 'f2f809ce-ce87-4ef5-b3e3-1c6893942872', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('133cdb95-b182-4abf-a762-d0e9ba264014', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 41', FALSE, NULL, NULL, '9e12ed3e-3ba4-43ab-8c72-f0d6a266fd8b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('13701fe4-c102-42a1-9b59-dfc37ed0b129', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 6', FALSE, NULL, NULL, '41b4d323-0cd6-4d71-8aff-67f59d9c87d7', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14419b58-5cc7-4e33-9430-ee1c70047dd9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 13', FALSE, NULL, NULL, '45203b82-70ba-4466-99ac-7f37db57c051', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14b30572-0f0b-4288-8055-5055a9afa074', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 47', FALSE, NULL, NULL, '3070c143-c4b2-427f-92f5-0d1bd884240d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14c2ea6c-c89b-4f3a-a15b-8c7b140b55d2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 37', FALSE, NULL, NULL, '9c586771-f8ac-4c51-ab18-9fc4210db45f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14c4222b-a5dc-4075-8bbf-1a158c5833b6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 10', FALSE, NULL, NULL, '0b66c984-47b1-4918-945c-22c140253f66', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14e88a72-9713-45e6-bb91-b8aa5a49e2ff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 49', FALSE, NULL, NULL, '743217ce-074d-49c2-8105-71c0cf8cafb1', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14ea7382-255a-4c10-aca2-041023ca05de', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 35', FALSE, NULL, NULL, '75d17d86-f50c-48f5-a904-f74dca4acc26', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('14ec9dda-7d2f-495a-b060-3b0f882b5aab', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 7', FALSE, NULL, NULL, 'f34d8dfe-e1c8-4c68-866a-e90f6f3377c2', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('156d227f-2d28-4f7c-812c-192e2f7b1cbf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 5', FALSE, NULL, NULL, '203170af-0331-470e-87f6-e768415a7a46', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('157563c9-b444-4046-8f3d-49f4600ed520', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 27', FALSE, NULL, NULL, '695c1d0c-819c-4f3a-9978-b0f90aff7dfc', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('15b0f577-c536-4954-bb8f-dabd5bb78b31', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 34', FALSE, NULL, NULL, '2c04162e-b647-44c6-9d03-922960936c3e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1648c6c3-176e-4a1b-9efb-c1980ea1ae9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 37', FALSE, NULL, NULL, '9c586771-f8ac-4c51-ab18-9fc4210db45f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('16554f55-6100-4a04-8332-4206eb46d449', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 22', FALSE, NULL, NULL, '08e19335-c4a5-489f-890d-dec1c46d9e77', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('16f64cc2-21c4-4a53-9682-cbd504fc5f50', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 48', FALSE, NULL, NULL, '4cdd89a2-c8d5-4191-92f4-cb0aaaaf3d30', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('17114a16-8058-43eb-85a3-cc3e22b22b98', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 7', FALSE, NULL, NULL, 'b44e0c68-0e53-4e8e-ace9-917badafc578', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('17717a73-f117-492d-987a-67c7b0941e6e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 42', FALSE, NULL, NULL, '5540a03b-71cd-4f76-b011-6130c56bd67c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('17974ea8-8078-4cc3-a271-b2db942e6171', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 46', FALSE, NULL, NULL, 'd9a103d0-26df-4548-abcb-3f4c9dd2348d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('17fd7710-af48-4892-b80e-3efffffd2085', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 39', FALSE, NULL, NULL, '21e75a8e-06d2-4517-b32a-e4da644d7d0e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('18181538-9246-499c-ab34-f39ba43845f0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 17', FALSE, NULL, NULL, 'd57f2ed7-b9c3-4a69-b22e-b10177d984d0', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('182caf84-9d6e-4984-b260-2d54aaef1963', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 8', FALSE, NULL, NULL, '65c23e10-101d-4dfb-8127-b6baf0caa703', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1854639a-df62-4417-9bff-808d2bd14da2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 22', FALSE, NULL, NULL, '08e19335-c4a5-489f-890d-dec1c46d9e77', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('18cd690a-4dc9-454d-b6d6-204f8882b55c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 33', FALSE, NULL, NULL, 'f1a9c300-3185-44e6-a8f6-e50db68ca23a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('18cf10fe-47e9-4f29-bb79-e40a720901db', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 31', FALSE, NULL, NULL, 'a9db3b85-5f46-4590-8c25-24ff6ea9dd46', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('19060d95-e42f-4cc9-9983-aea469e6f0cf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 7', FALSE, NULL, NULL, '58dfc22e-55ab-4cbd-8a0d-c36fc937ad86', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('19389790-fd5d-4513-a5b0-6ddce147b15a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 28', FALSE, NULL, NULL, '077440b6-ea30-4ebc-bc36-37d4ac5381b5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('19c89a82-8229-4d30-99cb-2215a95c7f91', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 13', FALSE, NULL, NULL, '7ebf7b04-71fd-42ca-8d26-e812f84cb136', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('19f05b88-1eeb-4213-83a7-a422f6ca7cec', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 19', FALSE, NULL, NULL, 'bcf4363f-2fd8-4b68-8915-97a83f3f413c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1a072e57-64a9-4bf7-a5ab-906bfd9a2e1d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 7', FALSE, NULL, NULL, '58dfc22e-55ab-4cbd-8a0d-c36fc937ad86', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1a9b77e4-f8e2-4c00-a207-c97a9f696e84', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 19', FALSE, NULL, NULL, '880d648a-128c-417d-95ee-c51551a0e5d2', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1ab49bf1-bf8e-4f9a-b0f6-d6c3dceb6b47', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 23', FALSE, NULL, NULL, 'c51270ad-ae90-4b9d-829b-8f96ca0dd678', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1ac0863c-7abd-4920-a901-cd3a22f7777c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 15', FALSE, NULL, NULL, 'f55fa1c6-9ffb-4c37-8c58-8f9c706db98f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1acab570-c524-4f39-9339-a7e06450f0b4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 22', FALSE, NULL, NULL, 'd374ec62-adfb-4d85-a360-87359f257f95', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1b29ee1d-d6b5-4363-9faa-d000502152ba', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 32', FALSE, NULL, NULL, '8c2dcf31-0afa-461b-9cd9-d23848344ac8', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1b2ef94f-1db6-4710-bc7c-2c22d14d7f56', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 44', FALSE, NULL, NULL, '97c2369b-7b21-4278-ac0d-d8579399b235', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1b86e1f9-ead1-43b1-a530-9f3fa4bb6c73', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 32', FALSE, NULL, NULL, '5916df5b-f5a4-4255-97b4-887628b53681', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1bcf1983-338a-4bf0-95f5-f3968b1fa373', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 4', FALSE, NULL, NULL, '0b321d5e-f918-4ad6-ae22-2d4a013e9780', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1bdff08a-3107-4bb3-b6d8-22ae5763d685', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 48', FALSE, NULL, NULL, '5e888cbf-7dcd-43cf-877e-af5110e9c1f4', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1c039eb9-c489-434e-b7fa-dfbd5341d58b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 12', FALSE, NULL, NULL, '64630db3-ec72-4214-96b3-9fb904f03f2b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1c162d2b-1644-49cd-bcd8-e9eddb7ec964', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 42', FALSE, NULL, NULL, 'cb253826-8103-45fd-9fce-b569426684f1', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1c586c2f-d48e-419c-a6d1-48cfa7390730', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 27', FALSE, NULL, NULL, '841564cf-a3e4-4fba-b0a5-5d2a3b1b1d65', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1d0b206b-7744-4740-9046-2e237b33e477', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 49', FALSE, NULL, NULL, '606501a2-4493-42b7-af27-765592f2fef5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1d33eefb-49dc-416a-bd10-a6ccf2807fc9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 31', FALSE, NULL, NULL, '2e83b1c0-9e04-47f6-a372-34f0afb6a25f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1d41b72a-5a3a-46b3-b2f6-6b301d81a393', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 3', FALSE, NULL, NULL, 'b6f878dd-1118-455a-89a1-0dedaa210f68', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1d589830-403b-4fb2-9f1c-7e853375c027', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 25', FALSE, NULL, NULL, '42f2a4c4-0ce4-4eb3-862e-549a717aaf7c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1d7c5518-24f8-46d4-b76f-89d1ecf97a1f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 10', FALSE, NULL, NULL, '00c461a0-4190-4102-ae8d-285435cd9faa', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1da5e8ec-d1b2-4adc-8654-f376a341108b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 5', FALSE, NULL, NULL, 'ee5dcda4-0f10-41f1-8f9e-e74175a006cc', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1dbe5fc1-189b-4ece-bed0-d1fd8e6c9187', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 7', FALSE, NULL, NULL, 'd2f88917-b564-44e1-9c30-7521f7460e76', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1dd24562-3757-4d91-b87f-1155d7e6aa9b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 10', FALSE, NULL, NULL, '59d9bb8a-5e67-4825-ace6-332d7739a3a3', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1e874f78-eccf-4aba-8e87-135fc7b3d1a8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 41', FALSE, NULL, NULL, '78663a94-cd19-4252-a908-2ee87c8c9106', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1e8816fb-b832-4080-9977-c3a7316c79e0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 39', FALSE, NULL, NULL, '32268f3e-ddf6-4cd7-bcf3-1b0f51ed457d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1e9401da-e22c-43b2-b69a-aca01c841542', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 17', FALSE, NULL, NULL, '48b29235-9a48-4172-9052-f16830f7cf94', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1ec660a6-bae2-4e5d-b1c3-7b418239a7b4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 36', FALSE, NULL, NULL, '1b879a76-3aa1-473f-9b42-bb5df6a9ab4d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1efd901e-efcc-4c53-b136-3b2e8624a965', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 18', FALSE, NULL, NULL, 'e75f77af-8aac-4094-957f-005eacc6f1b5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1f2f4809-ea0b-454e-8644-978f307be1d1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 3', FALSE, NULL, NULL, '54a9b703-a796-4a4a-8354-791e96444c82', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1f316636-d5b6-42ea-a74a-26166d2330b0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 15', FALSE, NULL, NULL, 'bcd9353a-951d-4b99-87bd-7217343d6ddd', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1f38cbac-cb21-44d2-a188-81fbe120b434', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 23', FALSE, NULL, NULL, 'abb19ae8-ccd2-4653-a0bc-6a3323f66bc8', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('1fdad55f-14ec-4972-a7ca-e60ac19311b0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 47', FALSE, NULL, NULL, 'af8b9e0d-c127-47b4-8632-497fcf6c4ff4', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('20303210-1c48-4510-a8d3-86803dd5dcdd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 40', FALSE, NULL, NULL, '72bf85da-d581-4744-baec-9eeadfee6c99', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2055f4b2-cd7c-49f2-9eb3-02fc0318c5e0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 41', FALSE, NULL, NULL, '9e12ed3e-3ba4-43ab-8c72-f0d6a266fd8b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('206514fd-300e-46df-8401-c631343eabdd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 36', FALSE, NULL, NULL, 'b7bee390-307b-42b0-90d2-d0b619e2625f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2082f840-bc36-447a-a546-51b8de2952d4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 37', FALSE, NULL, NULL, 'c27e8f4f-1262-4bf4-86b2-36a8bc69e64d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('20c62c88-1524-496e-ab84-f126a86f5d8f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 2', FALSE, NULL, NULL, '191fa3df-fdc8-41a6-b3bb-544193cf1210', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('20d1dbbc-8706-420d-a9fc-789861ae5fd4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 40', FALSE, NULL, NULL, '306b4c17-b548-4ae8-86a1-99af50efd488', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('20d3444b-71f8-4be1-96e3-5f51b5a52820', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 45', FALSE, NULL, NULL, 'f35ca997-2bb0-4cf1-a475-2a5177ecbc28', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2151bf59-925e-427e-98b8-8753d9870286', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 42', FALSE, NULL, NULL, 'cb253826-8103-45fd-9fce-b569426684f1', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('219d073e-18ec-47e2-9126-ceeafcca124f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 17', FALSE, NULL, NULL, 'd2bff9fa-d7fc-4a0a-8f82-4885786f3999', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('21edfde1-fc0e-44c8-90d3-fb9bfae7898f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 42', FALSE, NULL, NULL, 'c873f125-8b75-4268-9529-9956d3bfa9de', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('22368718-b182-41cc-9dca-d83a49caa79f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 39', FALSE, NULL, NULL, 'b819a6ba-6ee1-404e-a388-311c5ce37c54', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('22be7254-9e91-457d-a65d-31da1cdbf122', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 9', FALSE, NULL, NULL, '4e00f153-c09c-45ff-8ded-1ffa06a338a5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('22e2e13a-4fa6-4c56-891c-726b4e7f5cfb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 44', FALSE, NULL, NULL, '127356ef-cc28-461b-95e0-622c7f96e15f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('22e8708a-d569-4fa6-b5db-318b333131fd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 38', FALSE, NULL, NULL, 'e78cf7fe-0ad6-4800-9324-e4d8e70829ef', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('22fb84b5-a323-4948-95d4-015b3789d458', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 27', FALSE, NULL, NULL, '695c1d0c-819c-4f3a-9978-b0f90aff7dfc', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('23251c2b-c14f-4105-bb19-6942aecfbe8c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 31', FALSE, NULL, NULL, '8783d6c9-f7c6-4ed5-8cc8-4e64dc3f73d0', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('23337c94-a6c7-4327-8ff6-ed32d0632770', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 38', FALSE, NULL, NULL, '90fc41bf-e41b-4d30-8d7f-4e5dfdaef3e6', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('237bc2d1-087e-4b04-b187-b60169b81785', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 16', FALSE, NULL, NULL, '83d16662-a47c-4d3d-a362-dfffd3ad0f35', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('23b097b2-020c-4c6a-8292-5ccbe09079dc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 32', FALSE, NULL, NULL, 'b2a7c2da-1311-44d9-a348-4b0c81852b3c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('23c70c1c-c509-4649-81a3-a912fb6d8941', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 32', FALSE, NULL, NULL, '8c2dcf31-0afa-461b-9cd9-d23848344ac8', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('243b8f72-6035-44f2-af4c-5d58fc003ca1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 31', FALSE, NULL, NULL, '2e83b1c0-9e04-47f6-a372-34f0afb6a25f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2442a6de-acbe-4cc7-874d-89c73f95dfc8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 19', FALSE, NULL, NULL, 'a43ab879-8377-4b94-a4a0-1458a1bc52ed', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('244baddf-9c67-4cfb-a2a5-794b9b2d77e1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 10', FALSE, NULL, NULL, '00c461a0-4190-4102-ae8d-285435cd9faa', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('247aa175-3b72-4bb3-89c5-67bdbb1fbd42', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 12', FALSE, NULL, NULL, '64630db3-ec72-4214-96b3-9fb904f03f2b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('24a538b1-011f-44d7-bbd7-0a3cae4bcb0e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 29', FALSE, NULL, NULL, '7baac172-08b3-42d7-94a4-64622e58ea84', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('24fa4b8f-fadd-4323-aa6e-da23a8ae2e1f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 46', FALSE, NULL, NULL, '612db9f9-0488-4643-a5f5-97d7323de074', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2508743a-eef3-4472-8691-490d8711fdd8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 3', FALSE, NULL, NULL, '89a11355-99d9-478f-ad27-8a337ef259ff', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('25126249-2f82-4342-8a72-a25d5f1d9385', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 27', FALSE, NULL, NULL, '841564cf-a3e4-4fba-b0a5-5d2a3b1b1d65', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2513f852-ad01-44eb-a3e2-46a185daf427', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 20', FALSE, NULL, NULL, 'de97684d-b1f0-4226-b10d-f4493445b48b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2526fcab-19e3-4c52-9d2d-f9e061d41f7a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 10', FALSE, NULL, NULL, '00c461a0-4190-4102-ae8d-285435cd9faa', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('253052f6-119a-4008-9177-c2feac2d706b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 50', FALSE, NULL, NULL, 'b88e4145-fc06-482c-a2f1-6ca07ba4b50b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('25b3cb2a-d58f-4263-972e-3d9c313a2753', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 2', FALSE, NULL, NULL, 'baeccae8-8259-4154-86e3-4b5a74c5cef7', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('25bbfd8a-875b-45be-9c83-5fe4be4799b5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 28', FALSE, NULL, NULL, '122ebea7-11c5-4c48-9109-ec853b04e048', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('25bf1d6b-4870-45f9-b66a-e8a1d6041b50', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 14', FALSE, NULL, NULL, 'f42a05b5-18d8-4471-8ccc-9540b146d6b4', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('26bd39f0-ab85-46f3-b612-9f2d027468d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 37', FALSE, NULL, NULL, 'aef4d6ad-40cb-4152-b723-de184bd58305', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('26e3932b-3712-45e3-88e1-90b784b77fd8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 24', FALSE, NULL, NULL, 'f2c6e32e-5b69-4c66-98cd-6b2689c2d5de', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('275e86e5-9ff2-4c06-ad42-36eba9fb5402', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 33', FALSE, NULL, NULL, 'd73ebd3b-9c1a-4fce-ba1f-141eb2be598f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('279b3c26-4873-43de-b544-6851e0f65d05', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 2', FALSE, NULL, NULL, '0ee337ff-d0cd-4125-a7e0-f5e1cb3abd0b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('27d34a83-7a6c-4436-8cbc-0e871a2db7cb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 33', FALSE, NULL, NULL, '358cf230-4e82-46b2-963d-4a41ce9bb0d8', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('27f188d7-de75-4b20-bc56-f76e8307e344', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 11', FALSE, NULL, NULL, 'ea917c5b-ae83-4ce9-9657-a1b232e418db', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('283c7fe8-2497-40f6-bd50-1a2fad1201bf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 40', FALSE, NULL, NULL, '72bf85da-d581-4744-baec-9eeadfee6c99', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('28981d64-f8cf-4916-98bf-9c971a42dc75', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 38', FALSE, NULL, NULL, '995e7e7a-1ac3-44a6-9c32-c71898766558', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('28a66203-efd2-4fec-99b9-5fadd1abe132', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 7', FALSE, NULL, NULL, '086f2228-1683-4376-82a9-69148636a6aa', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('28bb1835-c4f1-433d-9a48-ad3a06b0f0dc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 48', FALSE, NULL, NULL, '7abbcac4-6847-4cd3-8d05-8f706f64da18', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('28facfdc-f890-4e93-83a5-f344b96b29d8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 10', FALSE, NULL, NULL, 'f2f809ce-ce87-4ef5-b3e3-1c6893942872', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('29402167-2aa8-4ea7-989d-f785424b1286', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 43', FALSE, NULL, NULL, '596d1ee3-1e8d-4411-b14d-48a74fec4a98', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('295d1809-b4ef-4cbb-8178-cbadd2cf66b7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 12', FALSE, NULL, NULL, '2ef0a76d-e5c9-4d39-89c7-1c3ed829b887', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('29cd8279-4c77-46f7-99f3-adb7666f5510', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 30', FALSE, NULL, NULL, 'db33df4d-ebb9-45f0-a0bf-d6690bbd2661', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2a1dcc46-06ab-4558-9aca-115dca6e5867', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 46', FALSE, NULL, NULL, '99b9f78f-93e0-48d9-ad7b-15b8a374d16f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2a52fbe6-1f84-44a0-b4a9-bb36fabac48c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 23', FALSE, NULL, NULL, '705cd75f-1ced-48af-8ddd-6d3a568023d9', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2b19a010-a460-48c7-ab77-0c456bdaf206', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 23', FALSE, NULL, NULL, '42d990d2-58de-4c3b-b03c-681418a57c94', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2b39b850-53d3-4813-b83a-24f85c708ed9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 10', FALSE, NULL, NULL, 'f2f809ce-ce87-4ef5-b3e3-1c6893942872', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2b6862aa-67d2-461a-9e2d-724c348c9283', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 26', FALSE, NULL, NULL, '70d314c9-5518-4cc1-9bf2-5789ad251f3d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2b88e584-4d09-40a7-8532-92e83e2fd49b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 28', FALSE, NULL, NULL, '077440b6-ea30-4ebc-bc36-37d4ac5381b5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2b9a085f-5f39-42a7-90b6-862e0cfdccb8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 17', FALSE, NULL, NULL, 'd57f2ed7-b9c3-4a69-b22e-b10177d984d0', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2b9ce6bd-a95b-4ba0-8fd6-aa9b09178d17', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 1', FALSE, NULL, NULL, '40f75731-a006-4ad7-a410-f47a5b58bec5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2bdc99ba-a64f-41c1-b592-9d8b04bb8d23', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 28', FALSE, NULL, NULL, '077440b6-ea30-4ebc-bc36-37d4ac5381b5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2c07018d-7153-4ddd-ae0b-b0da2a39f298', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 25', FALSE, NULL, NULL, '4f4bae55-1d31-44df-acc8-08f2f34495e5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2c41976b-3be6-4682-adfb-1e3e15754358', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 33', FALSE, NULL, NULL, 'd73ebd3b-9c1a-4fce-ba1f-141eb2be598f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2c60b63b-0f5a-462d-8b80-120bb2fc4089', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 23', FALSE, NULL, NULL, '42d990d2-58de-4c3b-b03c-681418a57c94', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2c61c5b3-ad33-4ae4-bfc9-88ff7e07d6a3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 41', FALSE, NULL, NULL, '591c672f-bb0f-432c-ac4a-ea838b500f99', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2c83c371-4a85-47b0-9265-5ac2faecc58e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 42', FALSE, NULL, NULL, '29b1fb90-3731-439c-ad24-57d3d9e532f0', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2c8a15c1-9caa-4e7c-9728-a852e1a9d783', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 33', FALSE, NULL, NULL, 'f1a9c300-3185-44e6-a8f6-e50db68ca23a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2cffc616-deef-48ad-ae6f-049e083de2fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 19', FALSE, NULL, NULL, '18af0818-28cf-4bc6-8452-dcf0c2c1780e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2d1de438-5ef9-40bc-a6c3-861a50362073', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 23', FALSE, NULL, NULL, 'abb19ae8-ccd2-4653-a0bc-6a3323f66bc8', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2d22381c-3ca1-449c-bd23-82dc5ad40074', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 22', FALSE, NULL, NULL, 'e6621657-485e-4d2c-820b-0c5ef78c9139', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2d611585-e054-4869-9118-56162ef018b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 38', FALSE, NULL, NULL, 'e78cf7fe-0ad6-4800-9324-e4d8e70829ef', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2d86ca6e-ab42-4276-bf32-cc350a8186eb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 26', FALSE, NULL, NULL, 'e90397ca-786d-40c9-ba32-fab67f449831', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2de0983c-dd21-4327-8a79-29f69d9dc3bf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 28', FALSE, NULL, NULL, '077440b6-ea30-4ebc-bc36-37d4ac5381b5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2e0ba0ed-02eb-49d6-b435-b448f4133e60', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 39', FALSE, NULL, NULL, '32268f3e-ddf6-4cd7-bcf3-1b0f51ed457d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2e3c4d08-efc8-4980-95a2-35dffd5068b5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 15', FALSE, NULL, NULL, 'bcd9353a-951d-4b99-87bd-7217343d6ddd', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2e540345-d8d2-42cc-b9f0-02c459a4dbab', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 46', FALSE, NULL, NULL, '612db9f9-0488-4643-a5f5-97d7323de074', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2eca0ead-7bbf-4911-a836-f074acfea29a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 21', FALSE, NULL, NULL, 'df9bdfd3-bb10-48b6-8c18-933b97149b17', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2fb1a345-d6cc-43ce-a2b7-b4caf23af79f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 16', FALSE, NULL, NULL, '79017a9b-a6fc-4b06-984e-d594ebd461a1', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2fb24ad2-0c74-4576-8e03-ff8dc15bbf3b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 44', FALSE, NULL, NULL, '97c2369b-7b21-4278-ac0d-d8579399b235', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('2fd9af80-922e-4501-a9b3-2cee92fcadfa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 20', FALSE, NULL, NULL, '2ff40769-0a9d-414d-a243-92a3fa1a6eaf', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('301ea901-158f-4a46-935c-f78bc96e8de9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 2', FALSE, NULL, NULL, '551d1b5e-0dec-464f-9b26-f0e0abd48169', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('301ee1c2-85ea-4f3f-a182-75539dd11bc7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 30', FALSE, NULL, NULL, '5313ca23-e4f7-4455-8f1e-ca88f8c6d064', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('305ad7db-3f8a-4309-a5ae-80a7e95fe128', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 33', FALSE, NULL, NULL, '358cf230-4e82-46b2-963d-4a41ce9bb0d8', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('305e7dc2-465b-4334-baed-818e0ad10c0e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 6', FALSE, NULL, NULL, '527cfb4e-3650-4bab-b958-06224a48800f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('30eea92b-fa77-422f-b243-4313acf2e9d0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 47', FALSE, NULL, NULL, '9bc82073-4c0f-4977-aa7f-9127849e53db', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('315d179c-cf45-4993-8789-d68d8ad98bdb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 44', FALSE, NULL, NULL, '9884986e-73eb-4e0d-88e8-20f406ec9ed0', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3167449a-06e1-452c-a7b4-597584ae8c73', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 13', FALSE, NULL, NULL, '901b0932-ec60-494e-897d-d589d0ee47be', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('31dada17-869f-434b-b4b9-fdd3c374f18c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 36', FALSE, NULL, NULL, '37cb14dc-e7e5-4601-b03c-075de1a9a712', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('322bdce2-e669-4f04-a658-7a86befae3fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 7', FALSE, NULL, NULL, 'b44e0c68-0e53-4e8e-ace9-917badafc578', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('32388f58-94c3-4348-8ef0-7d2cfd9410c2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 17', FALSE, NULL, NULL, '75bd53cd-3c15-43d0-a592-074952cfeccf', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3258d833-bce1-47aa-b80b-e6d3995f5542', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 43', FALSE, NULL, NULL, '374a0c50-6f84-4575-b4a8-9c9836ce9e1b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('32ca4580-2fe5-4261-99cc-5033171cba9f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 18', FALSE, NULL, NULL, '93065003-b808-4d3c-8f1f-6128867bb526', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('331bcee6-92fd-480e-b0d8-d8848b91cdd0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 42', FALSE, NULL, NULL, 'c2180950-cc0d-428c-bd7b-c4e17ab74ebe', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('331c8e54-42f8-49cd-a6e2-e7ccfea6cbf1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 36', FALSE, NULL, NULL, '2f830623-7758-4242-b2d6-630fcf83d0d9', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('33b1f090-f828-446a-9a64-e1eafe3a2fbc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 13', FALSE, NULL, NULL, 'dfd235f0-275a-4abb-939a-afd6d1231851', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('341bf613-f01a-457e-b3b7-fb9494ac018c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 28', FALSE, NULL, NULL, '122ebea7-11c5-4c48-9109-ec853b04e048', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('345e1706-521a-4cca-8f4e-a4d6f53bfb96', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 6', FALSE, NULL, NULL, '22a16c96-9a98-481d-bb8c-f6da7b8e3a9d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3473e93b-a6ca-4d04-83ab-a4ea96dd1d76', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 49', FALSE, NULL, NULL, '969a4188-d35e-4b8f-a3c4-9a467199e36b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('34ac57c6-1b99-4614-b60c-629de1dec061', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 6', FALSE, NULL, NULL, '676f6e82-35c5-44d3-887e-7ddf62191d3e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3505c820-af53-4153-9f78-c7eccf57afd3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 24', FALSE, NULL, NULL, '063f3287-9615-4d61-8fea-609c9a9438a1', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3514cee0-5acf-4234-be78-8b7e7994a882', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 44', FALSE, NULL, NULL, '503bcc63-c633-42ed-a220-6770b4ae5d19', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('356d0684-585e-4f00-aa58-2124cce19277', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 34', FALSE, NULL, NULL, 'd33bb281-55da-4ab8-91ee-cd43d76029ef', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('35830484-c91c-4557-9daa-46b7dea44ad5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 10', FALSE, NULL, NULL, '59d9bb8a-5e67-4825-ace6-332d7739a3a3', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('362f1a85-def8-49c2-a625-fda5a597f4fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 13', FALSE, NULL, NULL, '901b0932-ec60-494e-897d-d589d0ee47be', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('367d708b-a4e5-4fcb-ab56-30d75c6b53b4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 17', FALSE, NULL, NULL, '48b29235-9a48-4172-9052-f16830f7cf94', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('369f8549-2a8a-4a5a-975c-ae8219dbda8e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 42', FALSE, NULL, NULL, '29b1fb90-3731-439c-ad24-57d3d9e532f0', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('36a23865-0b2b-4288-b7e6-8d97107e37af', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 2', FALSE, NULL, NULL, '7518e8f5-51f9-4520-a1e2-cf39d0dba012', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('36bb1af4-617e-47b3-8db7-98c930162e3c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 36', FALSE, NULL, NULL, '37cb14dc-e7e5-4601-b03c-075de1a9a712', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('36c57bd7-1b05-416f-9648-c2c3fc928096', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 32', FALSE, NULL, NULL, '8c2dcf31-0afa-461b-9cd9-d23848344ac8', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('36d9fb0a-f722-4b0c-a882-aaafeaed6f22', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 11', FALSE, NULL, NULL, 'de9ad5c1-63b3-4e61-99fe-0133f0985dcb', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('36e9cf38-ea15-49e8-ac16-37244f95aada', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 47', FALSE, NULL, NULL, '860a9902-001f-489f-9295-5a69039e7d91', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('37068814-8049-498b-b6e6-2de731d86867', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 30', FALSE, NULL, NULL, '5313ca23-e4f7-4455-8f1e-ca88f8c6d064', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('37500836-a9d2-4d18-96d7-54a50ffa99d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 21', FALSE, NULL, NULL, '692f2a3d-84db-40f0-9a1b-5da056c52fb4', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('37594294-523b-42e4-a535-2ad2b0f52c2e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 10', FALSE, NULL, NULL, '59d9bb8a-5e67-4825-ace6-332d7739a3a3', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('378aa50c-71e1-4604-91c8-56d7c7b48795', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 26', FALSE, NULL, NULL, 'c68e9452-8dfd-413b-8175-3a7d04f1e7bb', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('38133ce6-e315-40eb-9eb3-2adb3ae1af85', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 39', FALSE, NULL, NULL, '5ab5b5ca-7a9c-4277-9f9b-2466ce655d06', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('38aab3de-d87c-44a6-ab27-d04eafe9f250', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 26', FALSE, NULL, NULL, 'c68e9452-8dfd-413b-8175-3a7d04f1e7bb', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('38f182c7-d5f4-4cb0-8c5b-f619389c7014', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 17', FALSE, NULL, NULL, '75bd53cd-3c15-43d0-a592-074952cfeccf', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('394b36d1-80ef-4b54-a199-c1e4da62fea7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 7', FALSE, NULL, NULL, 'd2f88917-b564-44e1-9c30-7521f7460e76', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('398af8c7-a7e4-422c-87c3-8826848a0d96', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 3', FALSE, NULL, NULL, 'fba0139e-3e9d-4e2b-b468-aafa6345e364', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('39a270cb-563b-490c-b01d-8c689f3bf9dc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 25', FALSE, NULL, NULL, 'dbb082c5-6eab-4588-aabb-c4de2b71306a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('39c90bb3-6533-4ebd-ac75-eb29181f8a7d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 9', FALSE, NULL, NULL, 'bcecc5db-302b-4b1c-996d-fb326bfe2e68', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('39e2654f-ea7b-43f8-b721-931f0d475f28', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 8', FALSE, NULL, NULL, 'f1c1ed6b-2b3e-4879-b0b9-48c747a7390c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('39ed42ae-350e-4adb-8d0e-bbcffdcb5453', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 43', FALSE, NULL, NULL, '2693ecbc-ed7f-4fe1-a876-b3a32a2f3d59', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3a0210ef-29b8-478b-9ae6-af43e7e11c36', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 12', FALSE, NULL, NULL, '5cce1e59-c9d7-4098-b1db-ce9bc23b053f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3a41af68-9648-44e4-818a-a89e75238f1a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 32', FALSE, NULL, NULL, '5916df5b-f5a4-4255-97b4-887628b53681', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3aad6a06-0011-4ee7-89a8-ec3f7bd26742', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 8', FALSE, NULL, NULL, '2091727e-6e4e-41f7-9338-9a42829462ad', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3ac1b149-1a31-4e42-afd9-cdba3ad216f1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 34', FALSE, NULL, NULL, 'e525133f-c444-4435-a6c7-15ae65ca309d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3af13cad-97b4-42bf-a071-c2e4e392509e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 14', FALSE, NULL, NULL, '2281c295-a679-4239-893c-9fba737a9c04', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3aff1102-7978-44b0-bb4d-37d7ce266968', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 28', FALSE, NULL, NULL, '9e800fe9-3f75-42e8-ae66-96884def18c4', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3b131c07-fe76-4442-98fc-b2b221408de1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 28', FALSE, NULL, NULL, '3e7af9b7-a3b8-4198-8d86-f91d23032194', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3b2fa46e-5733-4e6b-bdbb-6d237eaab617', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 49', FALSE, NULL, NULL, '969a4188-d35e-4b8f-a3c4-9a467199e36b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3b646b8e-1146-4c0c-b976-7f3af781f7b0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 20', FALSE, NULL, NULL, '6899e753-5aa3-4604-b1ca-91b14e38217f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3c0afad2-e987-451d-942e-92ad5ca9563c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 24', FALSE, NULL, NULL, '063f3287-9615-4d61-8fea-609c9a9438a1', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3c4ef441-a420-4014-aead-d8329ec8b63f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 4', FALSE, NULL, NULL, 'd5ff567e-8d38-46f3-98b8-a20e70b5f278', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3c64e735-5d85-4b63-86ec-9821cc775fb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 44', FALSE, NULL, NULL, '97c2369b-7b21-4278-ac0d-d8579399b235', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3c8e2efe-7b78-4aa8-a6d6-8608c3b0d04b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 17', FALSE, NULL, NULL, '8136f80f-4aa7-49a0-a383-4128a17e6179', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3cc8deed-90f4-4143-b095-5c875f008b70', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 50', FALSE, NULL, NULL, 'ce59833a-0b20-4e77-ae6a-691a568f7279', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3cc9d648-5adb-41e8-bb57-bc6ebf46854d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 50', FALSE, NULL, NULL, '431a53a0-3609-42f8-9bf2-f3273f1d95f1', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3d1d8ae4-be33-435a-99ea-3af9b41875f6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 5', FALSE, NULL, NULL, '4d6f457e-b716-440b-8fa4-05e13c2d4b11', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3d407519-4405-4216-b090-7fc4d6970fcb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 24', FALSE, NULL, NULL, '1b1ce780-093d-46ef-a730-af0d0e82acf6', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3d638972-d152-4aee-b2a7-4f7363254b78', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 17', FALSE, NULL, NULL, '75bd53cd-3c15-43d0-a592-074952cfeccf', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3d9da58b-a141-4a2d-a53e-255b778c6b00', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 26', FALSE, NULL, NULL, '5ac90de7-0518-4c92-82d1-26e722be8e45', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3dcfc0b0-0d9e-4987-96f0-75438e799cd9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 6', FALSE, NULL, NULL, '527cfb4e-3650-4bab-b958-06224a48800f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3f2922bc-d77c-4e4d-ab21-ea535ec0ed6c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 40', FALSE, NULL, NULL, '306b4c17-b548-4ae8-86a1-99af50efd488', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3f36e3e6-21d8-4e63-ad47-23c984376c14', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 12', FALSE, NULL, NULL, '5cce1e59-c9d7-4098-b1db-ce9bc23b053f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3fad11f9-916b-498f-bc24-f66363622010', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 49', FALSE, NULL, NULL, '969a4188-d35e-4b8f-a3c4-9a467199e36b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3fd0ec34-7a7e-49d5-9139-bdd9ddc20d86', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 10', FALSE, NULL, NULL, '0b66c984-47b1-4918-945c-22c140253f66', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3fdb766e-83ed-4d39-8aa9-bb52ef892048', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 32', FALSE, NULL, NULL, '8c2dcf31-0afa-461b-9cd9-d23848344ac8', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('3fe5c124-d059-4c81-ba44-ab940915c6e0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 41', FALSE, NULL, NULL, '78663a94-cd19-4252-a908-2ee87c8c9106', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('40755d83-6d0b-4ddd-a0db-dd911ca35e7f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 23', FALSE, NULL, NULL, 'c51270ad-ae90-4b9d-829b-8f96ca0dd678', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('40be4a6a-cfae-4aea-aa87-855fe69df3f6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 35', FALSE, NULL, NULL, '8e3cdd81-26dc-40dc-90e5-c5614d3e984e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('40d6d2f1-e0f3-4122-8e30-a8e086ab2fe6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 39', FALSE, NULL, NULL, '5ab5b5ca-7a9c-4277-9f9b-2466ce655d06', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('418a9773-0ecb-4125-a426-57b6641b8ff7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 13', FALSE, NULL, NULL, 'dfd235f0-275a-4abb-939a-afd6d1231851', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('41a8c16a-8bed-46f7-8696-c734fda536de', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 35', FALSE, NULL, NULL, 'e4a64e36-c504-413e-a950-14b20a6b2d0f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('42b864c3-3f1e-4703-b685-b41cde73caf8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 41', FALSE, NULL, NULL, '449926db-cdf1-4b14-9fa8-a2b6766786b7', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('42ca50f8-eac4-4265-af62-76c89f55c470', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 37', FALSE, NULL, NULL, '00a4d2c6-0117-4fc1-a48a-3435af4edddf', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('430e5b09-b95f-44e3-aea6-b15a0d558ba5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 30', FALSE, NULL, NULL, 'b555654b-12ea-4dec-a524-2df524e2934e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('432d7b96-e822-429e-b1dd-4e3400ce2287', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 4', FALSE, NULL, NULL, 'f6d6d5be-6640-43e6-b021-ea7464f018e7', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('433d63d6-30b9-4d55-a1c2-6359952ddd25', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 16', FALSE, NULL, NULL, '83d16662-a47c-4d3d-a362-dfffd3ad0f35', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('438d3c38-7528-4ffc-be77-f7b419676ce2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 35', FALSE, NULL, NULL, 'e4a64e36-c504-413e-a950-14b20a6b2d0f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('43b5c88a-51f8-4eb1-aa62-4291ec3f1f98', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 5', FALSE, NULL, NULL, '6c9c437c-1510-4f93-9af6-5bc251677b04', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('43bfae9c-01b4-4e02-9719-eefd4b105351', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 41', FALSE, NULL, NULL, '71582c79-cf34-4c0d-94f0-d627c27f2bb7', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('43c89915-648a-4bdc-9120-184e6cf53902', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 16', FALSE, NULL, NULL, '88419019-c938-4964-a3b2-c733095b7345', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('43d45119-7d28-4744-9085-3fe3a28d5459', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 41', FALSE, NULL, NULL, '449926db-cdf1-4b14-9fa8-a2b6766786b7', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('44161150-ccaa-4796-9ee1-e456fa00585a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 11', FALSE, NULL, NULL, '12b51827-b1ef-4e87-8bb5-e4f91a135944', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('44764cd3-b080-4a1c-9a66-65a907964232', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 43', FALSE, NULL, NULL, '596d1ee3-1e8d-4411-b14d-48a74fec4a98', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('451f7a9a-000e-41cb-af0f-f074696ca68b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 16', FALSE, NULL, NULL, '089bd63f-2671-4da8-9115-77a2d9e1e0ff', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('45372b0b-f8d5-42c4-b75e-55d23356fb21', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 22', FALSE, NULL, NULL, '08e19335-c4a5-489f-890d-dec1c46d9e77', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('455011cc-3ded-41cf-a038-20a79e87295c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 21', FALSE, NULL, NULL, 'df9bdfd3-bb10-48b6-8c18-933b97149b17', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('45b44872-a6d2-4e64-8fc6-a7dd510d9c83', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 26', FALSE, NULL, NULL, '70d314c9-5518-4cc1-9bf2-5789ad251f3d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('45ce5e81-4bce-4033-be9a-1df7d091f430', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 31', FALSE, NULL, NULL, 'a613ebad-b529-4a56-88e9-493ad40765f8', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('45d9258b-a290-48ab-b2ee-56dc3946d026', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 22', FALSE, NULL, NULL, '08e19335-c4a5-489f-890d-dec1c46d9e77', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('461457d3-75b1-4bca-be1b-ad50630275ad', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 19', FALSE, NULL, NULL, '880d648a-128c-417d-95ee-c51551a0e5d2', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('463aedd4-b87f-45cc-8283-aa4ba17320bc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 8', FALSE, NULL, NULL, '65c23e10-101d-4dfb-8127-b6baf0caa703', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('463f2b26-356c-4842-907f-1a3004e3c0b6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 7', FALSE, NULL, NULL, '086f2228-1683-4376-82a9-69148636a6aa', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4677b7b2-1238-4a33-80e1-99ce02377d6b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 43', FALSE, NULL, NULL, 'cf7ae403-1905-4b2a-8ef9-4d77f64c7920', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('47245020-ecd6-4b34-bfad-5e4d5784747a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 36', FALSE, NULL, NULL, '2f830623-7758-4242-b2d6-630fcf83d0d9', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('476f9cd1-e151-49d9-88fa-c2c4195147be', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 18', FALSE, NULL, NULL, '522b15ff-387d-4a0e-a952-6a499c1bdb23', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('47c97983-943d-4921-af4a-0fedff901c5f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 17', FALSE, NULL, NULL, '48b29235-9a48-4172-9052-f16830f7cf94', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('48212448-62a1-4c67-9f43-66880525669b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 19', FALSE, NULL, NULL, '8e0fd2c2-f36a-42fd-8c0f-c379361d18a1', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4823f2d3-957e-4994-a326-96473ffc3302', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 37', FALSE, NULL, NULL, '00a4d2c6-0117-4fc1-a48a-3435af4edddf', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('484ce256-fcd5-4aac-92fd-3fc662e676ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 22', FALSE, NULL, NULL, '392faa72-3fcc-4a5e-b04c-5e9322800108', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('488bfb2a-ee49-4088-bd4e-0e2b8d63ae35', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 4', FALSE, NULL, NULL, 'b10c67cc-f6f7-4dbf-bd86-452949506a0a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('488f6791-a60e-40d8-b941-898c83829e98', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 40', FALSE, NULL, NULL, '306b4c17-b548-4ae8-86a1-99af50efd488', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('48904615-34f7-4897-840d-4fc83dd4a742', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 43', FALSE, NULL, NULL, '2693ecbc-ed7f-4fe1-a876-b3a32a2f3d59', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('48eb6706-4c10-467d-abd8-cd10254df7bc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 24', FALSE, NULL, NULL, '71ccafb9-a9b9-4929-978e-0b94cdf30f4e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('490dc209-cf37-444d-9141-93e4d4751701', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 40', FALSE, NULL, NULL, '11189c0b-a7a4-470b-99c0-e26508addb98', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4916be36-93ac-4515-9cdc-2fd74dd04ab5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 28', FALSE, NULL, NULL, '3e7af9b7-a3b8-4198-8d86-f91d23032194', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('49498635-a518-4d76-916f-bedf8af10d03', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 9', FALSE, NULL, NULL, '1ee8a770-2a9f-4760-ad48-0cc2d469eba2', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4951f690-b74c-44e0-8417-56341a98320e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 25', FALSE, NULL, NULL, 'dbb082c5-6eab-4588-aabb-c4de2b71306a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('497d2cb4-6dcb-40b8-8139-2bd4c07a5b33', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 25', FALSE, NULL, NULL, '1296febe-1212-4e96-acbc-f74031902d74', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4990e0a5-b8ec-40ac-a04c-f78e39fb9fb3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 35', FALSE, NULL, NULL, '8590fa79-7f6f-4ef3-83d7-8c1ea4d11245', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('49f0321d-b431-4c23-af9f-b3cd06537818', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 7', FALSE, NULL, NULL, '086f2228-1683-4376-82a9-69148636a6aa', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4a333a39-9854-428e-ab5b-76ba315a3c6b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 29', FALSE, NULL, NULL, '7baac172-08b3-42d7-94a4-64622e58ea84', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4a4cc001-e09c-46a3-9b3c-3db0ddd738f0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 2', FALSE, NULL, NULL, '191fa3df-fdc8-41a6-b3bb-544193cf1210', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4a68b9b2-a83d-4c63-98c5-b012fdbe49c6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 17', FALSE, NULL, NULL, '48b29235-9a48-4172-9052-f16830f7cf94', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4ab189a6-2364-4a2c-9330-eac1df809606', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 11', FALSE, NULL, NULL, '69927048-8b96-4ab5-8494-1be98976d1c3', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4ab64f13-f081-4912-b389-abbb035c6dc2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 1', FALSE, NULL, NULL, '5e54bed1-d4e6-41d9-a28e-d3a51e2c533c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4ac80cd6-14a4-4097-9298-1f392b68b6fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 11', FALSE, NULL, NULL, 'ea917c5b-ae83-4ce9-9657-a1b232e418db', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4b38acc5-3859-4174-bfec-11ad03c1b934', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 43', FALSE, NULL, NULL, 'dc772f2f-8632-4522-91a4-e247dfcde2ee', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4b9ce508-65ab-47ed-897f-0b5f0d516e21', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 45', FALSE, NULL, NULL, '6f37f1f2-fe08-49ee-9cf3-fb670e08401e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4bbfff6a-8e7b-4227-931b-b11f8036152e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 38', FALSE, NULL, NULL, '522da0af-7a97-4b04-91ba-16a3f80d95cb', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4bdd5dfb-57eb-494c-8aea-46cd5dacfd5c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 48', FALSE, NULL, NULL, '6cf6c573-f039-42c3-8854-043cfce6d4a3', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4c23ed0f-c758-4d7a-8b76-db4179427641', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 29', FALSE, NULL, NULL, '56566050-5df9-4862-aa6a-c1b6b9ac7dcd', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4c65eb59-301d-4d6b-936d-a8942fb5be2a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 21', FALSE, NULL, NULL, '2dcfb228-9353-42e1-8db7-23f5d56226b4', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4caccc87-0f61-4615-a87d-e9dba6109cfe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 11', FALSE, NULL, NULL, '12b51827-b1ef-4e87-8bb5-e4f91a135944', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4cbf5c58-ab47-4a57-930c-f7b53709c8d3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 12', FALSE, NULL, NULL, 'bad1d4a7-0c44-4fd1-9fa3-cf355e1c0702', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4cc3d98b-2f6b-4b8e-b71a-56a2f36117a4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 38', FALSE, NULL, NULL, '522da0af-7a97-4b04-91ba-16a3f80d95cb', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4d237a3b-2227-4008-8c47-8694a7ac7d83', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 19', FALSE, NULL, NULL, 'a43ab879-8377-4b94-a4a0-1458a1bc52ed', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4d9bc687-4030-4e2a-84ba-9c661365ebbe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 46', FALSE, NULL, NULL, '234e8d78-4de2-4e43-b6ec-8ace773ab4ad', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4df8a947-5107-4492-9bd2-4ee67f72e353', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 2', FALSE, NULL, NULL, '0ee337ff-d0cd-4125-a7e0-f5e1cb3abd0b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4e193181-79ff-43d0-bf65-e74bcfeaf693', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 25', FALSE, NULL, NULL, 'c6aa7678-3585-4a11-afec-10171ba4fb3d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4e3cebbc-ec21-4835-bfdd-662c2937a17e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 9', FALSE, NULL, NULL, '1ee8a770-2a9f-4760-ad48-0cc2d469eba2', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4e626b58-ef4f-42a8-b3bb-96e48916f1d2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 6', FALSE, NULL, NULL, '319d5049-7928-4cac-b5dc-136059e33b83', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4e98e816-adac-4860-baa8-e124356ec176', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 50', FALSE, NULL, NULL, '22728788-649a-4e6e-9e17-f4f6ade58a8b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4eb6a922-d84d-4d83-bab4-8026a25aba5b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 26', FALSE, NULL, NULL, 'c68e9452-8dfd-413b-8175-3a7d04f1e7bb', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4ed64298-f0c1-4ec0-aa24-9e5c8db8df79', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 27', FALSE, NULL, NULL, '14406b2e-9c50-4d80-be94-08f3acdbdd97', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4f077506-0de6-4e05-b9a8-26c60cbc0418', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 15', FALSE, NULL, NULL, '63696841-0700-4255-8f9a-189150ea218a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4f125743-8f52-4440-a218-c95a91f1e218', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 8', FALSE, NULL, NULL, 'fb195671-3ba6-4c67-8f4e-67b6079b9e73', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4fcb0406-9ad0-4571-8014-0c20e75a9098', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 5', FALSE, NULL, NULL, 'ee5dcda4-0f10-41f1-8f9e-e74175a006cc', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('4fcb3ab1-55b0-441c-ae1d-77ce6563d479', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 27', FALSE, NULL, NULL, 'e749e4ce-74b5-4106-ae09-933b730fbc1a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('50028af5-e286-4010-aa46-1639328045c0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 29', FALSE, NULL, NULL, 'bcfdbe3b-348a-4815-aafb-8be35ff70226', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5003eb3a-aa48-49f6-89eb-cf89cc557cf0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 40', FALSE, NULL, NULL, '11189c0b-a7a4-470b-99c0-e26508addb98', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('50441783-cbef-480d-baff-e87b2309c570', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 32', FALSE, NULL, NULL, '8c2dcf31-0afa-461b-9cd9-d23848344ac8', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('504a05ba-ad60-4d96-b7bd-8547f592889a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 44', FALSE, NULL, NULL, '127356ef-cc28-461b-95e0-622c7f96e15f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('505af2ec-75d2-43c9-acf8-d8158eb095ce', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 40', FALSE, NULL, NULL, '7c42f679-02be-41af-8d67-24fa773dea79', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('505d835f-e787-4413-859f-aa4ad587cdfe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 18', FALSE, NULL, NULL, '522b15ff-387d-4a0e-a952-6a499c1bdb23', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('505dd633-5d25-44de-b0e7-5f1672fb0141', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 7', FALSE, NULL, NULL, '58dfc22e-55ab-4cbd-8a0d-c36fc937ad86', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('506ad9f9-8917-41d3-800e-6c97fa4afd1a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 38', FALSE, NULL, NULL, '995e7e7a-1ac3-44a6-9c32-c71898766558', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('50b7237a-587b-47d5-b91f-388d951240fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 39', FALSE, NULL, NULL, 'b819a6ba-6ee1-404e-a388-311c5ce37c54', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('50d0b7e8-8544-48b9-90bb-359b422a000c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 6', FALSE, NULL, NULL, '676f6e82-35c5-44d3-887e-7ddf62191d3e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('50eee8a6-a75d-4c01-b707-d369a172801a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 50', FALSE, NULL, NULL, 'ce59833a-0b20-4e77-ae6a-691a568f7279', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('50f4012c-11f0-4694-a812-cfd9cd4df7f7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 31', FALSE, NULL, NULL, 'a9db3b85-5f46-4590-8c25-24ff6ea9dd46', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('512394cc-f7d0-45f5-9929-2de2dc2bea7a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 7', FALSE, NULL, NULL, 'f34d8dfe-e1c8-4c68-866a-e90f6f3377c2', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('513f0f8d-1dee-40d0-9da6-7a084422e671', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 16', FALSE, NULL, NULL, '79017a9b-a6fc-4b06-984e-d594ebd461a1', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('51537fd5-35ac-4218-919d-66964b946ec3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 41', FALSE, NULL, NULL, '449926db-cdf1-4b14-9fa8-a2b6766786b7', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('51a69176-b22a-4f81-966d-0cc07472d2f2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 18', FALSE, NULL, NULL, '950f1190-870e-49aa-8eca-320deff5a4e5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('51bdf72e-5471-42b5-a52f-949134e70239', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 18', FALSE, NULL, NULL, '950f1190-870e-49aa-8eca-320deff5a4e5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('51cbceb8-22c7-482c-8dab-04632867459b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 35', FALSE, NULL, NULL, '8590fa79-7f6f-4ef3-83d7-8c1ea4d11245', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('51f87d43-7873-45af-9bcf-288002fe01f6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 42', FALSE, NULL, NULL, '29b1fb90-3731-439c-ad24-57d3d9e532f0', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('52525be5-ec2b-44bb-b3a8-79f660299472', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 22', FALSE, NULL, NULL, '392faa72-3fcc-4a5e-b04c-5e9322800108', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('525a5a8f-6e19-489e-b44d-b1ca8945929f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 45', FALSE, NULL, NULL, '6f37f1f2-fe08-49ee-9cf3-fb670e08401e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5299b8c6-67fe-4a36-ba02-c25a0e079557', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 33', FALSE, NULL, NULL, 'c145033c-061d-4b8e-bd50-d9dcde18c345', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('529bbef5-09f7-4763-bf5e-5039fcbc054c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 31', FALSE, NULL, NULL, 'a4636c51-f41d-4828-aaf2-c0faa14b70ac', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('52aa2bf5-4267-4eb9-8e74-16aea3561bf3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 8', FALSE, NULL, NULL, '65c23e10-101d-4dfb-8127-b6baf0caa703', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('52ab8883-460e-43a3-bd43-4580c9cf281e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 32', FALSE, NULL, NULL, '6045ad93-b4d1-4108-9bc0-1daac8848713', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('52edfcb3-7c7e-4df6-9367-c5518349d0fa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 14', FALSE, NULL, NULL, '51599bc8-3b12-42a0-8355-faea16e988ac', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('533a473e-06a5-4f12-bf16-f8f10205dcae', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 29', FALSE, NULL, NULL, '7baac172-08b3-42d7-94a4-64622e58ea84', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5370939a-3019-4ada-8c66-87eaf9b2efe7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 38', FALSE, NULL, NULL, '90fc41bf-e41b-4d30-8d7f-4e5dfdaef3e6', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('53a72402-b2b8-4ea4-9e09-2d82e525e8aa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 5', FALSE, NULL, NULL, 'ee5dcda4-0f10-41f1-8f9e-e74175a006cc', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('53a9fc8f-962c-44fa-b96c-ac16b4083f22', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 17', FALSE, NULL, NULL, 'd57f2ed7-b9c3-4a69-b22e-b10177d984d0', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('53aa0b0d-165c-405a-8bd7-950135cf6ec3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 11', FALSE, NULL, NULL, 'ea917c5b-ae83-4ce9-9657-a1b232e418db', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('53c81f57-a18f-4b30-bb15-ab9706cbdecf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 35', FALSE, NULL, NULL, '75d17d86-f50c-48f5-a904-f74dca4acc26', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('53da52d2-1425-4797-92ca-7f8ab452e5bb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 41', FALSE, NULL, NULL, '9e12ed3e-3ba4-43ab-8c72-f0d6a266fd8b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('53dc0d08-3c47-416c-b2d7-720e47a2c418', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 1', FALSE, NULL, NULL, '40f75731-a006-4ad7-a410-f47a5b58bec5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('540cd03a-1313-4e9a-b0cb-743787caec00', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 44', FALSE, NULL, NULL, '40951a9d-e30d-4e01-b43c-8de0d76377e7', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('543332b2-b9a3-4876-9065-b64e0d34649a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 39', FALSE, NULL, NULL, '207b76f8-609c-4b6b-b462-3138eaa91822', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('54e60c40-a4b9-4872-a89d-c018d7593a66', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 6', FALSE, NULL, NULL, '22a16c96-9a98-481d-bb8c-f6da7b8e3a9d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('550693f9-1514-4c7d-a8ae-2b7dea6a529b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 31', FALSE, NULL, NULL, 'a613ebad-b529-4a56-88e9-493ad40765f8', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('554675ee-6442-4e7c-9683-7c35a23d83d2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 3', FALSE, NULL, NULL, '89a11355-99d9-478f-ad27-8a337ef259ff', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('554bffdb-81f1-4c10-9a5f-4ceaad6f4819', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 45', FALSE, NULL, NULL, 'f35ca997-2bb0-4cf1-a475-2a5177ecbc28', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('555230d8-177b-4f12-bfeb-4141ec0f4e59', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 39', FALSE, NULL, NULL, '5ab5b5ca-7a9c-4277-9f9b-2466ce655d06', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('55556ade-28b4-4fa5-956b-2dcb66e7e94f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 28', FALSE, NULL, NULL, '122ebea7-11c5-4c48-9109-ec853b04e048', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('55c25ae3-394d-40bc-a0a6-9ddfd0b43d98', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 7', FALSE, NULL, NULL, '58dfc22e-55ab-4cbd-8a0d-c36fc937ad86', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('55c98144-6aed-4b97-b118-f21e0c9d6501', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 15', FALSE, NULL, NULL, '63696841-0700-4255-8f9a-189150ea218a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('560e68d4-4761-4e8b-a9d1-731671468ea5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 39', FALSE, NULL, NULL, '21e75a8e-06d2-4517-b32a-e4da644d7d0e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('561c0a4e-35d7-4688-b445-2ac0ad35aaab', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 41', FALSE, NULL, NULL, '591c672f-bb0f-432c-ac4a-ea838b500f99', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('563758b9-422d-4433-9dbd-31d267af8042', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 39', FALSE, NULL, NULL, '207b76f8-609c-4b6b-b462-3138eaa91822', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('568a8517-2fe5-4432-83de-8ac987b0211e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 47', FALSE, NULL, NULL, 'af8b9e0d-c127-47b4-8632-497fcf6c4ff4', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('56a9d13c-5f1b-49dc-95db-a1437a538e60', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 45', FALSE, NULL, NULL, '1b63f0aa-6794-4ead-ae79-72d14b726f84', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('572bf907-eed4-449f-978e-091aaa1b8d83', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 1', FALSE, NULL, NULL, '5a6834ac-c850-4f5e-8b44-e3eedccd308a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5738489d-c8b1-4b0c-a7bf-42000b243005', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 39', FALSE, NULL, NULL, '5ab5b5ca-7a9c-4277-9f9b-2466ce655d06', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('583196c4-fd56-4f77-8be4-6fece7397d88', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 14', FALSE, NULL, NULL, '48218200-93e8-4a38-b4e6-27217d34a846', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5864d3d0-4cd8-47e3-8534-71ab86e1cf68', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 14', FALSE, NULL, NULL, 'a65cf146-2ed5-4419-8161-1ed8b425c20e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('586ec402-327c-439a-a93e-69c2451fdb4a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 41', FALSE, NULL, NULL, '9e12ed3e-3ba4-43ab-8c72-f0d6a266fd8b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('587014f9-585b-4bbd-a444-7ea82ab427ff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 46', FALSE, NULL, NULL, 'df956ed0-5446-4664-a60b-f505ce84b039', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('58749bf9-16fb-468d-92fe-22d3a6a3a143', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 31', FALSE, NULL, NULL, '8783d6c9-f7c6-4ed5-8cc8-4e64dc3f73d0', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('58e65310-245b-4d93-bfa5-de55dd97a5b2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 35', FALSE, NULL, NULL, '70c38def-4a94-4db7-bdef-422b979df781', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('58ef501a-3059-4bd8-9d9b-7a4f8d17f9e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 42', FALSE, NULL, NULL, '5540a03b-71cd-4f76-b011-6130c56bd67c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('59010213-d24a-4890-ba8c-5861f6a0105c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 34', FALSE, NULL, NULL, 'd33bb281-55da-4ab8-91ee-cd43d76029ef', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5903daa6-2d42-4529-974c-6747ef00e082', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 9', FALSE, NULL, NULL, 'bcecc5db-302b-4b1c-996d-fb326bfe2e68', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5930a25b-8991-415b-9e0a-342e4ec84b19', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 45', FALSE, NULL, NULL, 'f35ca997-2bb0-4cf1-a475-2a5177ecbc28', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('595038d5-44fe-4836-b90f-9e85b9337dbe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 27', FALSE, NULL, NULL, '2d24f583-0a55-4e10-9e6a-197506dbc960', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5a1e6c12-d6e4-4b42-97c3-eba1eff65d9d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 30', FALSE, NULL, NULL, '88af0997-4e0f-4647-84a4-9bf9aa5d58a3', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5a2902cc-81d9-4737-8268-94b986014cf7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 22', FALSE, NULL, NULL, '409f93f2-bb1d-49e2-93b5-fa38aee9aac0', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5a615d47-29ab-4e3f-b92e-afc527aeced2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 31', FALSE, NULL, NULL, 'a4636c51-f41d-4828-aaf2-c0faa14b70ac', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5a89c240-d78e-469d-bcde-212309c777cf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 45', FALSE, NULL, NULL, '6f37f1f2-fe08-49ee-9cf3-fb670e08401e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5aa820dc-9a06-4bd1-bd38-238157679fcd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 4', FALSE, NULL, NULL, '0b321d5e-f918-4ad6-ae22-2d4a013e9780', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5aa96f52-0f28-4f7f-8f02-f3b1aa852a91', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 42', FALSE, NULL, NULL, '5540a03b-71cd-4f76-b011-6130c56bd67c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5af61e3d-06db-4082-9bc1-8ad2bafa9644', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 4', FALSE, NULL, NULL, 'c1c2325c-4801-4f5f-abd6-0ba585f1c338', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b09fa43-3a1c-4f22-a6d2-3238de087436', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 44', FALSE, NULL, NULL, '503bcc63-c633-42ed-a220-6770b4ae5d19', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b4b437a-5bdb-4ff4-8fa6-6c72ad775e05', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 21', FALSE, NULL, NULL, 'df9bdfd3-bb10-48b6-8c18-933b97149b17', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b550a5d-5d62-4639-a9fe-d45f3c0f05a7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 45', FALSE, NULL, NULL, '69d8b25c-6e84-4d64-abbf-f558a5c81937', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b557938-187d-43bc-84e3-404aa3d3670e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 37', FALSE, NULL, NULL, 'aef4d6ad-40cb-4152-b723-de184bd58305', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b576e0f-728b-42be-8987-ad792bb80c82', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 12', FALSE, NULL, NULL, '5cce1e59-c9d7-4098-b1db-ce9bc23b053f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b659827-f2c1-4a3f-9b77-71a0480fc486', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 24', FALSE, NULL, NULL, 'f2c6e32e-5b69-4c66-98cd-6b2689c2d5de', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b77c799-12ca-43a2-b1e3-9bd5c25ddd9d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 36', FALSE, NULL, NULL, 'b7bee390-307b-42b0-90d2-d0b619e2625f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5b9ffc20-9732-4d85-8987-77a197fabd83', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 43', FALSE, NULL, NULL, '374a0c50-6f84-4575-b4a8-9c9836ce9e1b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5bc663b5-be54-47a8-98c2-caf33b418db5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 34', FALSE, NULL, NULL, '8ecfce14-10a2-4827-bcd4-823b3d8f3f68', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5bd121da-0a2b-463b-9845-4ab91c3099e9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 50', FALSE, NULL, NULL, '7e960356-fef4-4fbb-841a-14c1e59c3aea', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5beaeefe-379d-4770-9b9b-a2714f03f389', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 26', FALSE, NULL, NULL, '0f1e31e3-8ccd-4794-bc4a-dc3f71002b1b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5bec25bd-4638-4db6-bdd3-c14e19e63b98', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 28', FALSE, NULL, NULL, '3e7af9b7-a3b8-4198-8d86-f91d23032194', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5c1e291d-df58-4b7d-bee6-357d7e3940a6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 6', FALSE, NULL, NULL, '319d5049-7928-4cac-b5dc-136059e33b83', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5c30faaa-687c-4dee-9821-0f14e3d25d01', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 50', FALSE, NULL, NULL, 'b88e4145-fc06-482c-a2f1-6ca07ba4b50b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5ca66553-a739-40d4-b631-ab7f34d9584d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 18', FALSE, NULL, NULL, '522b15ff-387d-4a0e-a952-6a499c1bdb23', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5ccedb2b-8478-4bef-ba4a-e07f12e47a1a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 16', FALSE, NULL, NULL, '88419019-c938-4964-a3b2-c733095b7345', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5cf2e358-a552-4e34-a6c0-8fc05e4ca8c4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 12', FALSE, NULL, NULL, '37d9c504-92b4-4505-a2d3-0ed4d7a24618', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5cf75df3-55ac-4ccb-8c69-b39cd18c3ac8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 21', FALSE, NULL, NULL, '692f2a3d-84db-40f0-9a1b-5da056c52fb4', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5d1515a0-9962-4591-a703-ba5169f44531', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 49', FALSE, NULL, NULL, 'c68c918d-f860-493b-b13f-1748836c82ff', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5d3998cc-523c-4e62-b015-fe8bb9d62373', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 25', FALSE, NULL, NULL, '4f4bae55-1d31-44df-acc8-08f2f34495e5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5d41397a-c979-44ed-baa7-b159e501f139', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 34', FALSE, NULL, NULL, 'd33bb281-55da-4ab8-91ee-cd43d76029ef', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5d8570e8-ffce-46c3-b3a1-8954824643df', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 46', FALSE, NULL, NULL, 'df956ed0-5446-4664-a60b-f505ce84b039', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5d8bf72c-c3cf-460f-82d7-5eab69f2a822', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 37', FALSE, NULL, NULL, '69c8017c-3b1b-4528-8f91-e0fe0ed87d64', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5d90583e-f1b9-4038-92e0-1bec00d93d4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 4', FALSE, NULL, NULL, 'b10c67cc-f6f7-4dbf-bd86-452949506a0a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5db1bab0-6602-4f76-91f8-7e8654fb042e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 2', FALSE, NULL, NULL, '0ee337ff-d0cd-4125-a7e0-f5e1cb3abd0b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5dd37833-7511-4069-a2b8-d21612a4e020', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 16', FALSE, NULL, NULL, '79017a9b-a6fc-4b06-984e-d594ebd461a1', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5dd68bc6-e7a0-49a3-8111-1ee1d9af310c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 41', FALSE, NULL, NULL, '78663a94-cd19-4252-a908-2ee87c8c9106', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5e07eab9-d79a-4f46-bbee-fec86d7e5973', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 20', FALSE, NULL, NULL, '6899e753-5aa3-4604-b1ca-91b14e38217f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5e147bcb-ef02-4a11-8705-8ab3abca0104', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 11', FALSE, NULL, NULL, 'de9ad5c1-63b3-4e61-99fe-0133f0985dcb', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5e1818fd-a243-4521-a8f3-390a3318ab9b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 4', FALSE, NULL, NULL, 'c1c2325c-4801-4f5f-abd6-0ba585f1c338', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5e60f026-8f0b-4925-9074-77fc8151597a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 22', FALSE, NULL, NULL, 'd374ec62-adfb-4d85-a360-87359f257f95', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5e77c4af-ad55-42b8-a008-a53ae0a7165e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 42', FALSE, NULL, NULL, 'c873f125-8b75-4268-9529-9956d3bfa9de', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5fcd22e7-6696-493b-813d-2b2aa3bfe795', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 15', FALSE, NULL, NULL, '30e024ab-24b7-4c0f-82c4-155082cfe77d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('5fe915f3-c3d9-440f-8c2e-5c2c3697112b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 29', FALSE, NULL, NULL, 'bcfdbe3b-348a-4815-aafb-8be35ff70226', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('601aebe1-6041-4b8f-ab0c-d38d3a5f17ab', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 33', FALSE, NULL, NULL, 'c145033c-061d-4b8e-bd50-d9dcde18c345', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6081383d-0bd0-4e6c-9911-84dccadbe1ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 44', FALSE, NULL, NULL, '40951a9d-e30d-4e01-b43c-8de0d76377e7', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('60f0f0c7-a4f0-4141-b4d5-2eb9eac689cf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 18', FALSE, NULL, NULL, '950f1190-870e-49aa-8eca-320deff5a4e5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('610899c5-948a-4c9e-b23f-96302d106e6a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 8', FALSE, NULL, NULL, '2091727e-6e4e-41f7-9338-9a42829462ad', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('610fa60e-ba20-4d21-ac74-c6235fff75c6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 9', FALSE, NULL, NULL, 'bcecc5db-302b-4b1c-996d-fb326bfe2e68', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('61480de0-85d2-4a29-805a-b175a64b4f2b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 45', FALSE, NULL, NULL, 'f35ca997-2bb0-4cf1-a475-2a5177ecbc28', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('61555bbc-8b0a-47d4-b0ed-50604b46df58', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 39', FALSE, NULL, NULL, '32268f3e-ddf6-4cd7-bcf3-1b0f51ed457d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6182ceed-f1d9-4d76-a340-9030561e5daf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 14', FALSE, NULL, NULL, 'f42a05b5-18d8-4471-8ccc-9540b146d6b4', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6190d0fd-3eac-4d32-9f18-367964b61843', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 1', FALSE, NULL, NULL, '8fc7f790-2963-4fbd-8aed-24e5c9bb3293', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6256f42f-22c2-49d1-a241-f966dee35336', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 35', FALSE, NULL, NULL, 'e4a64e36-c504-413e-a950-14b20a6b2d0f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('62d79af1-f523-4ad5-89d7-8ca089443a72', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 17', FALSE, NULL, NULL, 'd57f2ed7-b9c3-4a69-b22e-b10177d984d0', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('62dd829d-b3e5-41d4-ae58-9aae7dfc02b9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 10', FALSE, NULL, NULL, '39d1e88e-3153-43a3-9a96-a36ff60e26f9', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('633f386d-4544-4317-ad11-343d460b986c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 22', FALSE, NULL, NULL, 'e6621657-485e-4d2c-820b-0c5ef78c9139', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('634d100b-d077-45bc-94f3-5b3e66376cfb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 26', FALSE, NULL, NULL, '70d314c9-5518-4cc1-9bf2-5789ad251f3d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('63a49eb3-e96a-4f96-9d49-61c969882455', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 12', FALSE, NULL, NULL, '2ef0a76d-e5c9-4d39-89c7-1c3ed829b887', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('63d6d783-5472-4367-9d32-32bb491a077a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 50', FALSE, NULL, NULL, '431a53a0-3609-42f8-9bf2-f3273f1d95f1', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('63e0a13c-5d76-40bc-9b6e-8359dbc7e68e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 3', FALSE, NULL, NULL, 'fba0139e-3e9d-4e2b-b468-aafa6345e364', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('63ebb044-7ed7-472e-ab31-3fa353b974b1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 19', FALSE, NULL, NULL, 'bcf4363f-2fd8-4b68-8915-97a83f3f413c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6405586d-80a1-4c42-a5f4-89cfcfcbd92e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 18', FALSE, NULL, NULL, '6ee51009-71bd-40b7-aaba-767acb0900a2', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('642f3d9b-f4c0-4a82-a467-5e8e79f9d084', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 12', FALSE, NULL, NULL, '2ef0a76d-e5c9-4d39-89c7-1c3ed829b887', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('646d206c-2a95-46ac-9d4e-418af621da62', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 16', FALSE, NULL, NULL, '83d16662-a47c-4d3d-a362-dfffd3ad0f35', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('646f271f-cb4f-4d10-9f82-d82cfaf72694', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 11', FALSE, NULL, NULL, 'ea917c5b-ae83-4ce9-9657-a1b232e418db', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('646fc55b-ee77-488b-bc6a-b4538135bc84', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 27', FALSE, NULL, NULL, '841564cf-a3e4-4fba-b0a5-5d2a3b1b1d65', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('647eb947-76b5-432a-ae79-93d7ec83a578', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 17', FALSE, NULL, NULL, 'd2bff9fa-d7fc-4a0a-8f82-4885786f3999', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('64bb2934-3765-4c91-8940-fb2696c1220e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 37', FALSE, NULL, NULL, '00a4d2c6-0117-4fc1-a48a-3435af4edddf', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('653c6837-dd97-4230-b7df-fdceb7618fb4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 46', FALSE, NULL, NULL, '99b9f78f-93e0-48d9-ad7b-15b8a374d16f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('653fa332-107e-4a6a-9342-b32a06b49371', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 32', FALSE, NULL, NULL, 'e91bf542-e4b9-48fd-8df1-2d8261ffd37d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6576bbdc-ccc4-4c80-918d-3d1dd42ffba0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 41', FALSE, NULL, NULL, '9e12ed3e-3ba4-43ab-8c72-f0d6a266fd8b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('65a564ef-c315-446c-8b55-7c41af36bea8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 28', FALSE, NULL, NULL, '6c7ffff4-12cf-4dee-a329-5259b050d0de', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('65b69a1d-8929-44e8-b465-8ccaed213be3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 13', FALSE, NULL, NULL, 'dfd235f0-275a-4abb-939a-afd6d1231851', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('65c1bfbb-3e52-408f-973d-6c713f01ea68', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 32', FALSE, NULL, NULL, 'e91bf542-e4b9-48fd-8df1-2d8261ffd37d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6600d7e8-5543-46ad-9808-425e3619c044', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 2', FALSE, NULL, NULL, 'baeccae8-8259-4154-86e3-4b5a74c5cef7', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('67039b51-37e3-46b2-aa19-604f0ba9e821', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 1', FALSE, NULL, NULL, '5a6834ac-c850-4f5e-8b44-e3eedccd308a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('676d9da2-9398-4a94-9b1b-e3f1d56fe808', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 21', FALSE, NULL, NULL, '7e8e0278-e829-4f4a-9003-5b9d5269fd3a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('67a65c2e-161a-43d5-a540-f2c771df47fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 5', FALSE, NULL, NULL, '6c9c437c-1510-4f93-9af6-5bc251677b04', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('67b3610b-7939-46b9-b0b8-ce2959a08659', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 32', FALSE, NULL, NULL, '6045ad93-b4d1-4108-9bc0-1daac8848713', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6809ee39-2ee8-4525-a3fc-c3e6a9b7a8e5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 49', FALSE, NULL, NULL, '743217ce-074d-49c2-8105-71c0cf8cafb1', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('682b7f93-eb62-4258-9e8b-a24261bbc901', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 34', FALSE, NULL, NULL, 'd77b92ea-6ed4-407c-b5c5-9aca7f704993', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('68b1d2bc-23aa-448e-954f-3e5b40fdfaf1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 45', FALSE, NULL, NULL, '69d8b25c-6e84-4d64-abbf-f558a5c81937', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6949fbd5-fa05-4c3c-88f8-479974fa4bf4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 38', FALSE, NULL, NULL, '852c6a86-9537-4e08-aa0c-48f76dbc82a0', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('69c5c5bd-1bfe-46db-9879-d846d08c430a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 29', FALSE, NULL, NULL, 'bcfdbe3b-348a-4815-aafb-8be35ff70226', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('69d89736-0297-49f6-ae07-ada4f0074142', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 49', FALSE, NULL, NULL, '743217ce-074d-49c2-8105-71c0cf8cafb1', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('69dbb627-32bf-4b62-b74d-80bc4b448635', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 4', FALSE, NULL, NULL, '0b321d5e-f918-4ad6-ae22-2d4a013e9780', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('69f5742a-85f6-4f61-b6dd-a8e2166b57bd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 13', FALSE, NULL, NULL, '45203b82-70ba-4466-99ac-7f37db57c051', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6a10474b-3019-40e0-8ef2-841ac7689b89', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 18', FALSE, NULL, NULL, 'e75f77af-8aac-4094-957f-005eacc6f1b5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6a2d023f-6004-4d00-8710-2dc97312ac36', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 12', FALSE, NULL, NULL, '37d9c504-92b4-4505-a2d3-0ed4d7a24618', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6a834357-9acf-4741-aad7-72ff4ede2ff5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 34', FALSE, NULL, NULL, 'e525133f-c444-4435-a6c7-15ae65ca309d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6aa4d357-f9a2-449b-90f5-9d01c9b4d799', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 46', FALSE, NULL, NULL, 'd9a103d0-26df-4548-abcb-3f4c9dd2348d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6abeb73d-332e-449f-ba2d-34a536e7fc98', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 50', FALSE, NULL, NULL, '22728788-649a-4e6e-9e17-f4f6ade58a8b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6adcd66b-912f-4cc8-a344-7d0c5a3d2db7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 5', FALSE, NULL, NULL, 'ee5dcda4-0f10-41f1-8f9e-e74175a006cc', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6b95272d-597b-46dd-ae47-4859cd8657c0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 49', FALSE, NULL, NULL, '7825799a-7226-402f-a99e-6312bc5962b6', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6be23873-46ca-4f73-8062-81f93320730c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 25', FALSE, NULL, NULL, 'c6aa7678-3585-4a11-afec-10171ba4fb3d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6c1072c0-3f33-4e9b-8a37-c01d21806712', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 12', FALSE, NULL, NULL, '64630db3-ec72-4214-96b3-9fb904f03f2b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6c957306-5ee1-4cf8-acb7-7fa26884a19b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 25', FALSE, NULL, NULL, '1296febe-1212-4e96-acbc-f74031902d74', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6c973f36-0b37-4e7c-8bfc-3aaee4ff182f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 44', FALSE, NULL, NULL, '127356ef-cc28-461b-95e0-622c7f96e15f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6cbc1c35-a7a1-4584-9de8-9e7a97f75671', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 50', FALSE, NULL, NULL, '431a53a0-3609-42f8-9bf2-f3273f1d95f1', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6cc44eac-a73a-445e-b46c-11e2b187a782', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 23', FALSE, NULL, NULL, 'c51270ad-ae90-4b9d-829b-8f96ca0dd678', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6cd6b742-0f3c-424c-bc08-08303dc90d91', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 35', FALSE, NULL, NULL, '8e3cdd81-26dc-40dc-90e5-c5614d3e984e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6ce67ee5-8e0c-44ab-bce8-a05e7f020c15', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 19', FALSE, NULL, NULL, '18af0818-28cf-4bc6-8452-dcf0c2c1780e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6d2777ba-5b8d-4b25-b45a-01a1d683682f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 6', FALSE, NULL, NULL, '676f6e82-35c5-44d3-887e-7ddf62191d3e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6de78580-30f2-4365-a1a9-715528fc3dc7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 19', FALSE, NULL, NULL, '18af0818-28cf-4bc6-8452-dcf0c2c1780e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6dfa5d72-4ae3-4dac-bff1-341185183abf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 30', FALSE, NULL, NULL, 'a6ea1246-14c9-44a2-9fc5-b0ccdd34229f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6e1e7a59-fe0b-49a6-944d-a80daee527a2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 38', FALSE, NULL, NULL, '90fc41bf-e41b-4d30-8d7f-4e5dfdaef3e6', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6e212a35-1fd9-4d4e-b128-cbc8be6a7916', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 6', FALSE, NULL, NULL, '319d5049-7928-4cac-b5dc-136059e33b83', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6e34b34e-7e9d-4d02-a9dd-bf351b7191b2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 1', FALSE, NULL, NULL, '8fc7f790-2963-4fbd-8aed-24e5c9bb3293', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6e386180-b278-4c47-a8d3-461970efdbc4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 15', FALSE, NULL, NULL, '63696841-0700-4255-8f9a-189150ea218a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6ed317de-6488-43c9-a293-9aac4439e69d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 23', FALSE, NULL, NULL, 'c51270ad-ae90-4b9d-829b-8f96ca0dd678', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6f0e461b-13c2-460b-85e8-59763e0a7cfd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 33', FALSE, NULL, NULL, 'f1a9c300-3185-44e6-a8f6-e50db68ca23a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6f4a60bf-8a04-4be5-b587-00865a565890', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 25', FALSE, NULL, NULL, '42f2a4c4-0ce4-4eb3-862e-549a717aaf7c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6f802e75-f072-4d01-ba27-0462c0d8a77c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 36', FALSE, NULL, NULL, '1b879a76-3aa1-473f-9b42-bb5df6a9ab4d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6fc170cc-b882-4fa4-8fe2-7ecc93b67aa4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 37', FALSE, NULL, NULL, '69c8017c-3b1b-4528-8f91-e0fe0ed87d64', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('6fdbfb7f-6416-4770-abff-89fffeb457ca', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 15', FALSE, NULL, NULL, '13e859e1-df33-4bfd-b51b-9fe3493d1a19', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('70075f45-aa7e-44e1-b1af-7f8af29a5100', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 11', FALSE, NULL, NULL, 'ea917c5b-ae83-4ce9-9657-a1b232e418db', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7013cf9d-8745-440f-b777-dd654285b6d5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 27', FALSE, NULL, NULL, 'e749e4ce-74b5-4106-ae09-933b730fbc1a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7039e7da-b024-4dd9-b506-6f76e30e8e53', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 14', FALSE, NULL, NULL, '51599bc8-3b12-42a0-8355-faea16e988ac', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('703c4d26-baa5-48a8-9ddb-ef045f00a39f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 18', FALSE, NULL, NULL, '950f1190-870e-49aa-8eca-320deff5a4e5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('706066d1-a365-44b5-8816-917e423c2016', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 35', FALSE, NULL, NULL, '75d17d86-f50c-48f5-a904-f74dca4acc26', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('70c5e36a-05cf-4d94-8297-7aa2b5e2ddd1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 37', FALSE, NULL, NULL, '9c586771-f8ac-4c51-ab18-9fc4210db45f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7152b2a3-3511-4759-84d4-a3434863439f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 3', FALSE, NULL, NULL, '9d4916f7-4621-447e-8a6d-f6095f584e09', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('71cd3aee-329e-4707-9b88-acba01973239', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 41', FALSE, NULL, NULL, '78663a94-cd19-4252-a908-2ee87c8c9106', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('71d75849-87c2-4801-b0e3-6dbafba75caa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 35', FALSE, NULL, NULL, '75d17d86-f50c-48f5-a904-f74dca4acc26', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('720d2462-c6db-43d3-af97-bf64d1862377', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 34', FALSE, NULL, NULL, 'e525133f-c444-4435-a6c7-15ae65ca309d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('723a3c00-328e-455f-8052-21e1f3d3d7e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 5', FALSE, NULL, NULL, '5e30a962-e6dd-4137-9b04-b85655f9a728', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('72507749-5701-4712-95c7-f6478929e60d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 2', FALSE, NULL, NULL, '191fa3df-fdc8-41a6-b3bb-544193cf1210', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('725dbbe0-2bfb-40ea-8ccc-6bda7f9e2134', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 14', FALSE, NULL, NULL, '51599bc8-3b12-42a0-8355-faea16e988ac', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('726918a2-14b7-45d5-9228-42c9bc795297', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 26', FALSE, NULL, NULL, '5ac90de7-0518-4c92-82d1-26e722be8e45', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('728b9dfc-bd9e-462f-9933-c74600421924', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 9', FALSE, NULL, NULL, '4e00f153-c09c-45ff-8ded-1ffa06a338a5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('72b58be7-1834-4868-9c33-3e8fac01a4d2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 14', FALSE, NULL, NULL, 'f42a05b5-18d8-4471-8ccc-9540b146d6b4', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('72d926ca-ee86-41b6-80a9-b8a6ab5c4d87', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 16', FALSE, NULL, NULL, '089bd63f-2671-4da8-9115-77a2d9e1e0ff', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('73776c8a-952d-4f8f-8894-0f813ab55d15', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 47', FALSE, NULL, NULL, '0f708d81-d4bb-409e-9459-841cd2b6bd38', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('73ca6998-b659-4fb7-9d73-de32beb33607', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 29', FALSE, NULL, NULL, 'b4aa6d10-04a5-404c-85ed-5f25b5a4bd4a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('742cd348-792e-49ae-8546-992f4f210f33', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 1', FALSE, NULL, NULL, '8fc7f790-2963-4fbd-8aed-24e5c9bb3293', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('742db28f-3d9a-4a8d-b8cd-4188a707ee80', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 7', FALSE, NULL, NULL, 'b44e0c68-0e53-4e8e-ace9-917badafc578', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('74e4c687-9529-4a7a-aee3-695676dbcfa5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 28', FALSE, NULL, NULL, '9e800fe9-3f75-42e8-ae66-96884def18c4', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('754a4fc6-0b47-488f-bb9b-d3f29a465702', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 42', FALSE, NULL, NULL, '5540a03b-71cd-4f76-b011-6130c56bd67c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('75de89bd-3785-40cb-bf26-ad16a58a9ed4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 33', FALSE, NULL, NULL, 'c145033c-061d-4b8e-bd50-d9dcde18c345', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7642e4a2-1110-47a8-a451-18eaad97574e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 27', FALSE, NULL, NULL, '841564cf-a3e4-4fba-b0a5-5d2a3b1b1d65', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('76da742f-443d-416a-9381-2d9c25edcf19', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 20', FALSE, NULL, NULL, 'de97684d-b1f0-4226-b10d-f4493445b48b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('77073e16-5e19-49c6-8584-e0104d7af08c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 48', FALSE, NULL, NULL, '4cdd89a2-c8d5-4191-92f4-cb0aaaaf3d30', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('77d3dcc9-9886-4df7-82a2-83da8fe79b24', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 19', FALSE, NULL, NULL, 'a43ab879-8377-4b94-a4a0-1458a1bc52ed', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7862d401-bea4-4b7c-bccd-6cb1b2746d70', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 43', FALSE, NULL, NULL, 'dc772f2f-8632-4522-91a4-e247dfcde2ee', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('788f1e50-9b89-4150-a9c7-4e8f95bc9af2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 11', FALSE, NULL, NULL, '54d28ef5-9273-4c94-87f8-469cedb7d54d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7892e31a-c98d-4b9b-a023-1bdb3b6480ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 49', FALSE, NULL, NULL, '7825799a-7226-402f-a99e-6312bc5962b6', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7899cf88-acf0-4218-9f00-a68f8c3febff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 37', FALSE, NULL, NULL, 'c27e8f4f-1262-4bf4-86b2-36a8bc69e64d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('78af5e61-ec06-494c-81e9-3339e2a7f6bc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 40', FALSE, NULL, NULL, '6ab450b8-2fa9-4ed2-9ebd-92f629542f16', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('78ce242a-ae3a-4500-9e0f-c6031c59febe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 45', FALSE, NULL, NULL, '1b63f0aa-6794-4ead-ae79-72d14b726f84', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7965ccb2-793e-4e66-a2cb-c1b6b83cd057', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 30', FALSE, NULL, NULL, 'db33df4d-ebb9-45f0-a0bf-d6690bbd2661', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('79d45201-b4b6-42ed-bd6a-df1838a407d7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 14', FALSE, NULL, NULL, '2281c295-a679-4239-893c-9fba737a9c04', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('79e19a20-fe1c-48d8-9886-d88974023bf5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 45', FALSE, NULL, NULL, '69d8b25c-6e84-4d64-abbf-f558a5c81937', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('79f860b3-8b54-466e-80f7-4c5c9374d0a2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 5', FALSE, NULL, NULL, '6c9c437c-1510-4f93-9af6-5bc251677b04', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7a29906b-8623-4b8f-a25a-0c6320bc445c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 23', FALSE, NULL, NULL, '42d990d2-58de-4c3b-b03c-681418a57c94', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7a87482d-05bd-490b-bc4d-d32e9b999215', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 18', FALSE, NULL, NULL, '93065003-b808-4d3c-8f1f-6128867bb526', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7aac81bb-0ee2-477b-a09e-636ab20e031b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 26', FALSE, NULL, NULL, '5ac90de7-0518-4c92-82d1-26e722be8e45', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7aaefeca-0b64-4cb8-bd64-2466cba68e1b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 29', FALSE, NULL, NULL, 'b4aa6d10-04a5-404c-85ed-5f25b5a4bd4a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7b4bd61d-966f-40d2-959a-70b2a3ceaf34', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 39', FALSE, NULL, NULL, '32268f3e-ddf6-4cd7-bcf3-1b0f51ed457d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7bb41294-74f7-43e9-b7a8-65cd2b896ce9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 12', FALSE, NULL, NULL, '37d9c504-92b4-4505-a2d3-0ed4d7a24618', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7c646832-dc8b-49b4-9952-8e44a01862e4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 31', FALSE, NULL, NULL, 'a613ebad-b529-4a56-88e9-493ad40765f8', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7c6a3fc3-c642-497e-b707-d59db20ebe50', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 24', FALSE, NULL, NULL, 'd2a16cb0-ec69-4343-834b-c288f04c6b20', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7c71cfff-5a07-47ab-adc5-e8be200d0640', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 14', FALSE, NULL, NULL, 'a65cf146-2ed5-4419-8161-1ed8b425c20e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7d1aa51c-1b90-4756-8a10-038b91d81324', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 2', FALSE, NULL, NULL, '551d1b5e-0dec-464f-9b26-f0e0abd48169', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7d2aa44c-ce13-4e90-b516-a3841a9f4cfb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 11', FALSE, NULL, NULL, '54d28ef5-9273-4c94-87f8-469cedb7d54d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7d4719c6-bd54-4ee6-835a-efd5191db26a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 24', FALSE, NULL, NULL, '71ccafb9-a9b9-4929-978e-0b94cdf30f4e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7d7393d1-d46e-4368-bf16-f8461950711a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 16', FALSE, NULL, NULL, '089bd63f-2671-4da8-9115-77a2d9e1e0ff', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7da34784-edc9-4c10-85e0-cf395a07750f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 34', FALSE, NULL, NULL, 'd77b92ea-6ed4-407c-b5c5-9aca7f704993', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7df20da1-d12e-436d-8f96-0c60d113f40c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 42', FALSE, NULL, NULL, '29b1fb90-3731-439c-ad24-57d3d9e532f0', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7e071f9d-7be3-4b4e-ad8d-5836ce2175d2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 43', FALSE, NULL, NULL, 'cf7ae403-1905-4b2a-8ef9-4d77f64c7920', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7e2ee5de-4448-48f7-969c-0acc24113d92', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 39', FALSE, NULL, NULL, '207b76f8-609c-4b6b-b462-3138eaa91822', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7e637674-78c8-41ef-b798-39b552ba1e02', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 6', FALSE, NULL, NULL, '676f6e82-35c5-44d3-887e-7ddf62191d3e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7e9dd725-7fa2-46e7-87ce-d5fb3d09271c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 10', FALSE, NULL, NULL, '39d1e88e-3153-43a3-9a96-a36ff60e26f9', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7ea51c6e-7f72-4e9a-98a3-5cf41f1b2c29', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 12', FALSE, NULL, NULL, '37d9c504-92b4-4505-a2d3-0ed4d7a24618', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7ee79506-bd1f-4e7b-bbab-67f1d9b5f9fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 5', FALSE, NULL, NULL, '4d6f457e-b716-440b-8fa4-05e13c2d4b11', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7f5ed717-6cc9-4d51-bc72-3fa90855ff53', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 48', FALSE, NULL, NULL, '7abbcac4-6847-4cd3-8d05-8f706f64da18', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7f82d9e0-caff-44b0-bde0-a3c14d155652', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 28', FALSE, NULL, NULL, '077440b6-ea30-4ebc-bc36-37d4ac5381b5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7fd88a5a-7789-4317-94dc-8c32d949c72d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 28', FALSE, NULL, NULL, '3e7af9b7-a3b8-4198-8d86-f91d23032194', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('7fdbca3c-d527-4ea1-94b6-e24565625925', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 40', FALSE, NULL, NULL, '72bf85da-d581-4744-baec-9eeadfee6c99', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8019c92d-708a-43d9-97c6-9ebca7575856', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 24', FALSE, NULL, NULL, 'f2c6e32e-5b69-4c66-98cd-6b2689c2d5de', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('80ff7387-683d-483b-accb-80ed39a5a742', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 43', FALSE, NULL, NULL, 'dc772f2f-8632-4522-91a4-e247dfcde2ee', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('81221215-bb58-41fa-8a6f-0a8ac8a61ab3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 20', FALSE, NULL, NULL, '2ff40769-0a9d-414d-a243-92a3fa1a6eaf', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('812c178d-477f-4f1c-8870-66985fba6246', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 37', FALSE, NULL, NULL, 'c27e8f4f-1262-4bf4-86b2-36a8bc69e64d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('81a28042-989a-4d92-93b1-e553ef833590', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 46', FALSE, NULL, NULL, '234e8d78-4de2-4e43-b6ec-8ace773ab4ad', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('81dadf07-c6c3-45c6-95dc-1bbe8ed2383f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 48', FALSE, NULL, NULL, '5e888cbf-7dcd-43cf-877e-af5110e9c1f4', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8210c085-15e6-46ca-8a75-7f552bebc584', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 7', FALSE, NULL, NULL, '58dfc22e-55ab-4cbd-8a0d-c36fc937ad86', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('82359a96-ea69-478a-9a6e-bbd2001044c2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 40', FALSE, NULL, NULL, '7c42f679-02be-41af-8d67-24fa773dea79', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8246427c-62b0-400f-a407-94974b9567be', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 46', FALSE, NULL, NULL, 'df956ed0-5446-4664-a60b-f505ce84b039', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8246e4f0-28a4-4579-a51e-4eaf0d460576', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 18', FALSE, NULL, NULL, '6ee51009-71bd-40b7-aaba-767acb0900a2', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('828b3c76-5f43-43a2-9215-738305fdb88c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 44', FALSE, NULL, NULL, '40951a9d-e30d-4e01-b43c-8de0d76377e7', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('82b7cc29-a44c-4f76-b98b-c5eb94607bc4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 1', FALSE, NULL, NULL, 'd65ee2f3-4b12-4ce5-84e7-5eab507d2338', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('82c32ff5-608b-4f16-acfc-5c65dc85d15e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 49', FALSE, NULL, NULL, '7825799a-7226-402f-a99e-6312bc5962b6', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('82f68dd6-a5a7-4e6b-b424-30d6d08760d6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 34', FALSE, NULL, NULL, 'e525133f-c444-4435-a6c7-15ae65ca309d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('82fe5009-0237-4329-b773-4917ef835154', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 12', FALSE, NULL, NULL, '64630db3-ec72-4214-96b3-9fb904f03f2b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('83030fe4-ea0f-44d7-950a-05a692036bc0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 49', FALSE, NULL, NULL, '969a4188-d35e-4b8f-a3c4-9a467199e36b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('830565e2-0750-4ad5-8830-ef0cbd29ebe1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 9', FALSE, NULL, NULL, 'bcecc5db-302b-4b1c-996d-fb326bfe2e68', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('83475c65-b7bf-4971-842d-92e3a647a8b9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 27', FALSE, NULL, NULL, '2d24f583-0a55-4e10-9e6a-197506dbc960', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('834ab3d3-6b0c-47e7-9c3f-d03fd1a8c072', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 49', FALSE, NULL, NULL, '606501a2-4493-42b7-af27-765592f2fef5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('835f2682-0a26-4293-b9e4-151e049c52bf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 4', FALSE, NULL, NULL, 'd5ff567e-8d38-46f3-98b8-a20e70b5f278', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('83731670-d3ad-43ab-a0f2-fcbef0b7826f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 50', FALSE, NULL, NULL, 'b88e4145-fc06-482c-a2f1-6ca07ba4b50b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('83b46421-db2f-4652-9550-54b500d00cb4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 29', FALSE, NULL, NULL, '19427640-0844-465a-ae50-9bf6e471a0fd', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('83fc05ae-474f-4382-9ce3-e70e5bbc0911', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 12', FALSE, NULL, NULL, '5cce1e59-c9d7-4098-b1db-ce9bc23b053f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('83ff2cc6-caba-4415-a2da-d766cd424463', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 48', FALSE, NULL, NULL, '5e888cbf-7dcd-43cf-877e-af5110e9c1f4', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('841cf91d-5e45-461d-a0e9-823cc382482d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 30', FALSE, NULL, NULL, 'b555654b-12ea-4dec-a524-2df524e2934e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8457897b-47a5-4093-9236-3706bca7dfda', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 8', FALSE, NULL, NULL, '2091727e-6e4e-41f7-9338-9a42829462ad', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('84682cc6-672c-47fd-866b-669704cc0302', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 20', FALSE, NULL, NULL, '67922510-89bd-4788-be19-8c24d2c63817', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('84b0ce3a-4912-4989-a67e-fc64c789b511', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 10', FALSE, NULL, NULL, 'f2f809ce-ce87-4ef5-b3e3-1c6893942872', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('84cb59ae-9c5a-45d9-b94a-8c549cbab569', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 15', FALSE, NULL, NULL, 'f55fa1c6-9ffb-4c37-8c58-8f9c706db98f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('84cc77e8-01de-490e-b124-115f2ef00f09', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 23', FALSE, NULL, NULL, '705cd75f-1ced-48af-8ddd-6d3a568023d9', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('85073bd0-cd1c-4345-b1a0-fcbb461ab9b4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 42', FALSE, NULL, NULL, '5540a03b-71cd-4f76-b011-6130c56bd67c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('851e7dd3-73c7-4432-828c-b33860b8ad8b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 1', FALSE, NULL, NULL, 'd65ee2f3-4b12-4ce5-84e7-5eab507d2338', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('854b42db-2d01-4afb-b82b-441748db12f3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 22', FALSE, NULL, NULL, '409f93f2-bb1d-49e2-93b5-fa38aee9aac0', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('855d7483-de45-497c-bf1a-f787b52c0046', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 50', FALSE, NULL, NULL, 'ce59833a-0b20-4e77-ae6a-691a568f7279', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8572aed1-dfc4-42fc-a544-acd83d11b563', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 38', FALSE, NULL, NULL, '90fc41bf-e41b-4d30-8d7f-4e5dfdaef3e6', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('857ae4ec-76dc-4e57-ab2c-028f4edd27cd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 13', FALSE, NULL, NULL, '901b0932-ec60-494e-897d-d589d0ee47be', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('858d976f-feb9-4d0b-aea6-d1a065af830c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 4', FALSE, NULL, NULL, 'd5ff567e-8d38-46f3-98b8-a20e70b5f278', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('858dc148-2bd3-41dd-a587-1a61caa6e2c4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 49', FALSE, NULL, NULL, '7825799a-7226-402f-a99e-6312bc5962b6', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('85d3c43c-7ba5-46dc-8361-c112793eaec1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 47', FALSE, NULL, NULL, '9bc82073-4c0f-4977-aa7f-9127849e53db', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('85fc126b-1cb1-457f-9a04-f1246e044403', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 33', FALSE, NULL, NULL, 'd73ebd3b-9c1a-4fce-ba1f-141eb2be598f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('86880b7c-5451-4e31-b794-626599ceab9c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 29', FALSE, NULL, NULL, '56566050-5df9-4862-aa6a-c1b6b9ac7dcd', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('86a5edf6-ebff-4eed-97a8-0ce2c55f673e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 6', FALSE, NULL, NULL, '41b4d323-0cd6-4d71-8aff-67f59d9c87d7', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('86d4bfa2-98b8-4931-bef9-d16e3c84d44a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 23', FALSE, NULL, NULL, 'abb19ae8-ccd2-4653-a0bc-6a3323f66bc8', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('86d8fc54-47de-448c-9081-72cd3cd5c944', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 8', FALSE, NULL, NULL, '63c190cb-95b6-4cad-8fa9-bf5c646006dd', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('86e13ea7-5eb0-4e6b-824e-e657101f0fde', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 30', FALSE, NULL, NULL, 'a6ea1246-14c9-44a2-9fc5-b0ccdd34229f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('86f12c58-6996-4fd0-b4bf-752fbe282fcb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 20', FALSE, NULL, NULL, '6899e753-5aa3-4604-b1ca-91b14e38217f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('87220143-c3a5-4071-819b-53fba0d45694', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 16', FALSE, NULL, NULL, '089bd63f-2671-4da8-9115-77a2d9e1e0ff', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('87311ab3-5ddd-420a-bd44-3887f5533ee0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 3', FALSE, NULL, NULL, 'fba0139e-3e9d-4e2b-b468-aafa6345e364', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('87443e72-9481-4196-99ab-2fbf32b5ac75', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 37', FALSE, NULL, NULL, '69c8017c-3b1b-4528-8f91-e0fe0ed87d64', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('875f78c3-1212-465f-8972-223f8138f8d6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 43', FALSE, NULL, NULL, '374a0c50-6f84-4575-b4a8-9c9836ce9e1b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('882a13ac-7907-452f-af70-80bd8b36bbdb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 17', FALSE, NULL, NULL, '8136f80f-4aa7-49a0-a383-4128a17e6179', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8835b71b-985c-4652-b744-8636f54dad67', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 49', FALSE, NULL, NULL, '7825799a-7226-402f-a99e-6312bc5962b6', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('884af54d-1452-47e9-99c2-0bf0fbac83e7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 41', FALSE, NULL, NULL, '71582c79-cf34-4c0d-94f0-d627c27f2bb7', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8865b1e8-0088-4b3f-b01a-ae3d2125c7c3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 36', FALSE, NULL, NULL, '7eb4887a-38b4-46e8-bb23-f9902cc5764c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('889c0f51-a94c-48e2-8cfc-a920878f9d11', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 38', FALSE, NULL, NULL, '852c6a86-9537-4e08-aa0c-48f76dbc82a0', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('88a2e5a5-f515-4a2e-98bf-fecd2db05734', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 45', FALSE, NULL, NULL, '1b63f0aa-6794-4ead-ae79-72d14b726f84', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('88bdab86-79f0-495b-91de-9ffbf9c57169', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 16', FALSE, NULL, NULL, '089bd63f-2671-4da8-9115-77a2d9e1e0ff', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('88c3a481-3595-4aef-b941-74769b91b4a5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 30', FALSE, NULL, NULL, 'b555654b-12ea-4dec-a524-2df524e2934e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('890a121b-054e-4b56-a6ed-96c6ca424aef', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 32', FALSE, NULL, NULL, 'b2a7c2da-1311-44d9-a348-4b0c81852b3c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('895593a4-6fe7-4a18-bc08-88fbf7fb63f8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 32', FALSE, NULL, NULL, '5916df5b-f5a4-4255-97b4-887628b53681', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8985e417-acef-4acf-b7b7-15f3db617249', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 18', FALSE, NULL, NULL, '93065003-b808-4d3c-8f1f-6128867bb526', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('89d98a25-6251-4550-8b45-87e78bc56ee1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 32', FALSE, NULL, NULL, '5916df5b-f5a4-4255-97b4-887628b53681', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('89e6e74c-e5dd-4683-b7c3-880d4c719668', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 2', FALSE, NULL, NULL, '191fa3df-fdc8-41a6-b3bb-544193cf1210', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('89f97958-a169-46df-ab03-9b99eefee668', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 43', FALSE, NULL, NULL, '2693ecbc-ed7f-4fe1-a876-b3a32a2f3d59', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8a0afa1b-d441-46f1-b959-fd6359af0763', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 16', FALSE, NULL, NULL, '4ad13bcc-c048-4960-9a3b-ac65ac60356e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8a5794a8-cb30-49dc-ba6a-53a9f32112f1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 30', FALSE, NULL, NULL, '88af0997-4e0f-4647-84a4-9bf9aa5d58a3', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8a912467-54fd-4bb0-86f9-ff767524f395', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 13', FALSE, NULL, NULL, '5931c01b-a29c-4302-a52e-6b92e9df920e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8b51c4fa-c9f2-41b0-93a3-edd7715c4dc7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 43', FALSE, NULL, NULL, 'cf7ae403-1905-4b2a-8ef9-4d77f64c7920', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8b7fb0a4-22b5-4bfe-9305-0b074ffcb3a1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 10', FALSE, NULL, NULL, '59d9bb8a-5e67-4825-ace6-332d7739a3a3', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8bfd5ba8-4f04-46ca-96fa-2f15b1bc9e5f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 1', FALSE, NULL, NULL, '5e54bed1-d4e6-41d9-a28e-d3a51e2c533c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8d78b961-62d4-4289-a11b-125f5be20094', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 25', FALSE, NULL, NULL, '1296febe-1212-4e96-acbc-f74031902d74', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8da82d16-604c-40ba-8bec-cfd03357e7b7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 3', FALSE, NULL, NULL, '89a11355-99d9-478f-ad27-8a337ef259ff', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8db625a7-1a7a-43eb-aa63-3281f0ec4c02', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 34', FALSE, NULL, NULL, '2c04162e-b647-44c6-9d03-922960936c3e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8dce4219-9874-4370-95c9-730c2ce6b3ba', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 13', FALSE, NULL, NULL, '7ebf7b04-71fd-42ca-8d26-e812f84cb136', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8ddc6e33-d498-402e-b270-9782fe6b9381', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 24', FALSE, NULL, NULL, '71ccafb9-a9b9-4929-978e-0b94cdf30f4e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8df920bf-d1fb-423c-ab2e-3ad14329bbf9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 45', FALSE, NULL, NULL, 'be4e0397-e949-4f1a-9bc6-776d9157a1b8', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8e2cf021-46e7-4d1f-b71a-39e1272b3a76', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 19', FALSE, NULL, NULL, 'a43ab879-8377-4b94-a4a0-1458a1bc52ed', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8e2f7664-ea04-49cd-9117-d96c02fdb23b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 48', FALSE, NULL, NULL, '4cdd89a2-c8d5-4191-92f4-cb0aaaaf3d30', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8e3b5438-2379-41f4-a008-896a66df4cb5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 46', FALSE, NULL, NULL, 'd9a103d0-26df-4548-abcb-3f4c9dd2348d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8e51ff2e-bca1-4068-956d-c9de6359388d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 20', FALSE, NULL, NULL, '6899e753-5aa3-4604-b1ca-91b14e38217f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8e9586df-ba36-46ce-a33f-1c90ad736140', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 39', FALSE, NULL, NULL, '21e75a8e-06d2-4517-b32a-e4da644d7d0e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8f1e822c-5ef9-4eb1-8d27-776864f30a42', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 45', FALSE, NULL, NULL, 'be4e0397-e949-4f1a-9bc6-776d9157a1b8', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8f4e395e-3f84-41b2-8bb2-8568da90954e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 33', FALSE, NULL, NULL, 'd73ebd3b-9c1a-4fce-ba1f-141eb2be598f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8f7610d1-ad5d-4394-a983-e3fa292de826', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 2', FALSE, NULL, NULL, '7518e8f5-51f9-4520-a1e2-cf39d0dba012', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8f7f582d-bb96-4876-89a3-369c74ffd227', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 14', FALSE, NULL, NULL, '51599bc8-3b12-42a0-8355-faea16e988ac', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8f9388cc-6741-45f6-ab2e-7a309be72ca5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 37', FALSE, NULL, NULL, '00a4d2c6-0117-4fc1-a48a-3435af4edddf', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8fe2a958-4690-4ddd-8ad3-aa2ba8b68fbe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 47', FALSE, NULL, NULL, '860a9902-001f-489f-9295-5a69039e7d91', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('8ff813b3-4a29-4e41-b93e-3431af39c749', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 19', FALSE, NULL, NULL, '8e0fd2c2-f36a-42fd-8c0f-c379361d18a1', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('905905e4-4eac-4692-8f4c-e215dbd48ccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 4', FALSE, NULL, NULL, '0b321d5e-f918-4ad6-ae22-2d4a013e9780', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('907c62ab-54f1-4d12-9509-175eafcabc9e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 15', FALSE, NULL, NULL, '13e859e1-df33-4bfd-b51b-9fe3493d1a19', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('90a4c32f-26b8-4903-aa13-01da8ff02f2c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 1', FALSE, NULL, NULL, '5e54bed1-d4e6-41d9-a28e-d3a51e2c533c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('90b5025f-b290-4ed6-a72a-7f65d32c63e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 8', FALSE, NULL, NULL, '63c190cb-95b6-4cad-8fa9-bf5c646006dd', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('90d8666c-531e-4eac-b8de-f8a0406bd0a5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 43', FALSE, NULL, NULL, '2693ecbc-ed7f-4fe1-a876-b3a32a2f3d59', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('90e64c09-51fa-48d8-9be1-f68e576aad8c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 31', FALSE, NULL, NULL, 'a4636c51-f41d-4828-aaf2-c0faa14b70ac', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('90f1b76e-0222-460d-afec-bae7ea5a9bd2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 44', FALSE, NULL, NULL, '127356ef-cc28-461b-95e0-622c7f96e15f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('90fff174-cd3b-4b45-b759-117417efa49c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 17', FALSE, NULL, NULL, '8136f80f-4aa7-49a0-a383-4128a17e6179', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('910dec0a-ae3c-4241-8d17-4b564623e413', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 35', FALSE, NULL, NULL, '8590fa79-7f6f-4ef3-83d7-8c1ea4d11245', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('91a2388e-f003-494c-9a04-12a9e26a745c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 43', FALSE, NULL, NULL, 'dc772f2f-8632-4522-91a4-e247dfcde2ee', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('91ba6494-3375-49fc-b25e-d7fd0aa5bf0b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 22', FALSE, NULL, NULL, 'e6621657-485e-4d2c-820b-0c5ef78c9139', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('91d0a7f5-c646-4be4-af5d-d29708ccf2aa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 24', FALSE, NULL, NULL, '063f3287-9615-4d61-8fea-609c9a9438a1', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('91d115da-0e75-4aa2-8374-8dabf5c10c58', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 22', FALSE, NULL, NULL, '08e19335-c4a5-489f-890d-dec1c46d9e77', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('921e5a35-43ae-4f9f-a3e8-94b3bbc419e7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 13', FALSE, NULL, NULL, '901b0932-ec60-494e-897d-d589d0ee47be', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('92e72f1a-18e0-4659-ab19-a6ba421056fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 49', FALSE, NULL, NULL, '606501a2-4493-42b7-af27-765592f2fef5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9358bba2-776c-4fab-a9d8-08c47d2728c2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 31', FALSE, NULL, NULL, 'a4636c51-f41d-4828-aaf2-c0faa14b70ac', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('936d16ef-3110-4673-8fc9-1383aaf22106', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 14', FALSE, NULL, NULL, '2281c295-a679-4239-893c-9fba737a9c04', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('93980f44-ec11-4de7-b4d8-54c30597eaf4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 21', FALSE, NULL, NULL, '7e8e0278-e829-4f4a-9003-5b9d5269fd3a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('93db75c2-7161-4398-a348-6717a52c0806', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 37', FALSE, NULL, NULL, 'c27e8f4f-1262-4bf4-86b2-36a8bc69e64d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('93dd652f-631d-45c1-9e6d-e5fd34b579d1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 40', FALSE, NULL, NULL, '6ab450b8-2fa9-4ed2-9ebd-92f629542f16', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('943f47f4-3724-48ac-ade0-9c6557235b3d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 32', FALSE, NULL, NULL, 'b2a7c2da-1311-44d9-a348-4b0c81852b3c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9479e6e9-ce2e-4b1d-99df-74dd49c286ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 19', FALSE, NULL, NULL, '8e0fd2c2-f36a-42fd-8c0f-c379361d18a1', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('94919d13-abee-4af8-b224-54de45ab589d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 24', FALSE, NULL, NULL, '1b1ce780-093d-46ef-a730-af0d0e82acf6', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9531c834-7a34-4558-8f31-d22fcc0e0bb2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 30', FALSE, NULL, NULL, '88af0997-4e0f-4647-84a4-9bf9aa5d58a3', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('95ca8523-724e-4bed-95c2-a86d4ce239a7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 16', FALSE, NULL, NULL, '79017a9b-a6fc-4b06-984e-d594ebd461a1', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('965566be-45a1-4a6b-a952-65ba44edd6a1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 36', FALSE, NULL, NULL, 'b7bee390-307b-42b0-90d2-d0b619e2625f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('96633074-42be-4aa2-9be9-0ca764a37dfd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 6', FALSE, NULL, NULL, '41b4d323-0cd6-4d71-8aff-67f59d9c87d7', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('96894bc1-c2aa-44b7-83d2-1e94ee8f5579', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 6', FALSE, NULL, NULL, '527cfb4e-3650-4bab-b958-06224a48800f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('968b0831-86a0-46e8-9913-1d2ae60813e6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 2', FALSE, NULL, NULL, 'baeccae8-8259-4154-86e3-4b5a74c5cef7', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('96ac768e-f444-4d55-b179-bc8012d07d84', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 1', FALSE, NULL, NULL, '5a6834ac-c850-4f5e-8b44-e3eedccd308a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('972a2b87-e195-4dea-bf80-3460b463c875', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 48', FALSE, NULL, NULL, '7abbcac4-6847-4cd3-8d05-8f706f64da18', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('97315fcd-e1ce-430a-b4d4-41c35c803be9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 50', FALSE, NULL, NULL, 'ce59833a-0b20-4e77-ae6a-691a568f7279', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('97581442-fdb9-44c7-9f94-1585277e108f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 11', FALSE, NULL, NULL, 'de9ad5c1-63b3-4e61-99fe-0133f0985dcb', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('97762e07-13ed-4b4d-81ed-6492eda85292', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 18', FALSE, NULL, NULL, '6ee51009-71bd-40b7-aaba-767acb0900a2', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('978226ee-f711-4198-855f-ad77e679d68d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 17', FALSE, NULL, NULL, 'd2bff9fa-d7fc-4a0a-8f82-4885786f3999', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('97bf3426-93df-4588-9e44-12ed6e1e7457', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 1', FALSE, NULL, NULL, 'd65ee2f3-4b12-4ce5-84e7-5eab507d2338', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('982839b7-5e4e-447a-9614-050ba8769a83', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 36', FALSE, NULL, NULL, '1b879a76-3aa1-473f-9b42-bb5df6a9ab4d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('986750ad-1f23-4b5e-a631-a42d56ec74df', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 43', FALSE, NULL, NULL, 'cf7ae403-1905-4b2a-8ef9-4d77f64c7920', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('98bf3de3-f40a-4f9e-979e-84a6f1bf84fd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 25', FALSE, NULL, NULL, '42f2a4c4-0ce4-4eb3-862e-549a717aaf7c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('98df892a-b1ed-42d3-802b-68baf2e7a575', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 50', FALSE, NULL, NULL, '22728788-649a-4e6e-9e17-f4f6ade58a8b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9927d6fb-a679-48f2-91b1-5267dff5629e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 46', FALSE, NULL, NULL, 'df956ed0-5446-4664-a60b-f505ce84b039', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('993be414-0160-4c69-9466-c9196d789c9e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 16', FALSE, NULL, NULL, '4ad13bcc-c048-4960-9a3b-ac65ac60356e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9941440f-fef1-443a-b540-4d730418d948', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 44', FALSE, NULL, NULL, '503bcc63-c633-42ed-a220-6770b4ae5d19', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('994b41e2-9dfb-4370-9ddc-37262532d83a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 12', FALSE, NULL, NULL, '64630db3-ec72-4214-96b3-9fb904f03f2b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('996a9458-c246-4812-850f-c23ddf1e5da8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 30', FALSE, NULL, NULL, 'b555654b-12ea-4dec-a524-2df524e2934e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('998df703-d979-49e6-814d-4b680aa13e23', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 40', FALSE, NULL, NULL, '7c42f679-02be-41af-8d67-24fa773dea79', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('999076a6-a011-4974-ab4b-651a11eeaf35', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 21', FALSE, NULL, NULL, '7e8e0278-e829-4f4a-9003-5b9d5269fd3a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('99d59d9e-6910-4ec6-a9b3-bc294f7e5415', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 5', FALSE, NULL, NULL, 'ee5dcda4-0f10-41f1-8f9e-e74175a006cc', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('99de3f96-9177-4fb3-a224-73768506e7c4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 29', FALSE, NULL, NULL, 'b4aa6d10-04a5-404c-85ed-5f25b5a4bd4a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('99eaf95b-a71e-4508-bd17-bba96da4d667', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 22', FALSE, NULL, NULL, '392faa72-3fcc-4a5e-b04c-5e9322800108', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9a46068f-c0a2-45f7-a1df-fc68f9e88216', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 32', FALSE, NULL, NULL, 'e91bf542-e4b9-48fd-8df1-2d8261ffd37d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9a871a39-1c05-47e2-8357-d8520fac3114', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 15', FALSE, NULL, NULL, '30e024ab-24b7-4c0f-82c4-155082cfe77d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9a8f2e80-8cd4-4f4b-8c94-9e5f4e5775e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 44', FALSE, NULL, NULL, '9884986e-73eb-4e0d-88e8-20f406ec9ed0', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9a8fd74e-444f-4d70-9adf-c74cd9f29e77', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 20', FALSE, NULL, NULL, '67922510-89bd-4788-be19-8c24d2c63817', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9ab4ed3b-a735-473a-a57d-1af6259e0685', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 46', FALSE, NULL, NULL, '612db9f9-0488-4643-a5f5-97d7323de074', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9afcb26d-3378-4eec-bc69-039fee70a080', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 43', FALSE, NULL, NULL, '596d1ee3-1e8d-4411-b14d-48a74fec4a98', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9b3b964f-85cf-404c-8fc7-7bd0438b2b54', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 46', FALSE, NULL, NULL, '234e8d78-4de2-4e43-b6ec-8ace773ab4ad', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9b5a07e0-d7e2-4d9b-b2fb-efbd9aae619e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 44', FALSE, NULL, NULL, '97c2369b-7b21-4278-ac0d-d8579399b235', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9b7ebd33-0c6c-4553-b874-171cfca534bf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 31', FALSE, NULL, NULL, 'a9db3b85-5f46-4590-8c25-24ff6ea9dd46', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9bbd43ce-cc85-4ab6-9df3-b4aa9ebe263d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 1', FALSE, NULL, NULL, '5a6834ac-c850-4f5e-8b44-e3eedccd308a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9bcacf6b-7db5-4411-879e-2cd225ea5c24', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 40', FALSE, NULL, NULL, '306b4c17-b548-4ae8-86a1-99af50efd488', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9bd24754-e326-4537-82ef-024473aadc52', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 20', FALSE, NULL, NULL, '2ff40769-0a9d-414d-a243-92a3fa1a6eaf', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9bd74fcd-c3c0-46fb-9627-781e9e13b1d7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 47', FALSE, NULL, NULL, '3070c143-c4b2-427f-92f5-0d1bd884240d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9bfc0ebb-073a-4c00-81a8-e6e151a3e439', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 34', FALSE, NULL, NULL, '2c04162e-b647-44c6-9d03-922960936c3e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9c617e08-e348-4095-a49e-13701e4a67a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 23', FALSE, NULL, NULL, 'abb19ae8-ccd2-4653-a0bc-6a3323f66bc8', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9c73c9e7-b580-4d4d-840a-6607a83359ea', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 3', FALSE, NULL, NULL, '89a11355-99d9-478f-ad27-8a337ef259ff', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9c98fb49-873c-4a8f-ba4c-bad8715b915a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 32', FALSE, NULL, NULL, 'b2a7c2da-1311-44d9-a348-4b0c81852b3c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9ca4f8eb-57ca-48e1-b15a-4a525b6837a4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 40', FALSE, NULL, NULL, '72bf85da-d581-4744-baec-9eeadfee6c99', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9cd7955c-32fb-4633-9af6-49f5733371e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 16', FALSE, NULL, NULL, '79017a9b-a6fc-4b06-984e-d594ebd461a1', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9d0a8f3f-24b4-4fb3-a393-7f772ff7b8df', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 45', FALSE, NULL, NULL, '69d8b25c-6e84-4d64-abbf-f558a5c81937', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9e6a38f8-50ed-4a32-9503-f9cf144874e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 15', FALSE, NULL, NULL, '30e024ab-24b7-4c0f-82c4-155082cfe77d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9ea9cd04-4623-4924-be46-19dd7906589f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 25', FALSE, NULL, NULL, '1296febe-1212-4e96-acbc-f74031902d74', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9ebacd05-069a-4efb-b822-d3a69951bdf5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 32', FALSE, NULL, NULL, '6045ad93-b4d1-4108-9bc0-1daac8848713', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9ed52769-7945-4383-9cb7-337ac1012ab8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 11', FALSE, NULL, NULL, '12b51827-b1ef-4e87-8bb5-e4f91a135944', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9efbd8aa-3d23-4731-9e2f-97ae79bba970', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 37', FALSE, NULL, NULL, '69c8017c-3b1b-4528-8f91-e0fe0ed87d64', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9f145d12-2354-46ad-ad45-82e2e5a32fd2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 42', FALSE, NULL, NULL, 'c873f125-8b75-4268-9529-9956d3bfa9de', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9f3bdbb2-c175-4be7-9948-173a2988f924', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 10', FALSE, NULL, NULL, '39d1e88e-3153-43a3-9a96-a36ff60e26f9', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9f643273-9ead-43ae-a48c-27b41531a3a6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 27', FALSE, NULL, NULL, '2d24f583-0a55-4e10-9e6a-197506dbc960', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('9f9374f7-b5e6-49e4-8695-1425565bfa20', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 44', FALSE, NULL, NULL, '9884986e-73eb-4e0d-88e8-20f406ec9ed0', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a0170255-dbdb-46a8-80ed-ee70303e1359', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 34', FALSE, NULL, NULL, 'd77b92ea-6ed4-407c-b5c5-9aca7f704993', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a02f8802-df2a-410d-8a21-f348eae01f0e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 49', FALSE, NULL, NULL, '743217ce-074d-49c2-8105-71c0cf8cafb1', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a05332c3-d143-43e9-a69f-1eb97a0294e1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 1', FALSE, NULL, NULL, '5e54bed1-d4e6-41d9-a28e-d3a51e2c533c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a0a3dddf-6f65-4e32-8cc9-3c1df182d2ea', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 44', FALSE, NULL, NULL, '40951a9d-e30d-4e01-b43c-8de0d76377e7', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a0a67133-d06a-4076-8885-340a434428a6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 23', FALSE, NULL, NULL, '705cd75f-1ced-48af-8ddd-6d3a568023d9', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a0c1869d-ec09-4b5d-858f-b3430129b5e5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 22', FALSE, NULL, NULL, 'e6621657-485e-4d2c-820b-0c5ef78c9139', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a0ef093c-dff9-4cbd-b317-b2273e995bdd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 34', FALSE, NULL, NULL, '2c04162e-b647-44c6-9d03-922960936c3e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a10c9365-507a-4d62-95df-c1e9298b8006', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 21', FALSE, NULL, NULL, '81d172f8-2904-44d3-9f4e-bb7977d7f9d8', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a12795e0-b883-42fd-9595-481f3d61f8f3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 47', FALSE, NULL, NULL, '9bc82073-4c0f-4977-aa7f-9127849e53db', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a13e0e92-583e-4335-9252-b22ad1d26128', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 17', FALSE, NULL, NULL, '75bd53cd-3c15-43d0-a592-074952cfeccf', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a18885b0-4bfe-4b72-993e-5678dbb9d7ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 17', FALSE, NULL, NULL, 'd2bff9fa-d7fc-4a0a-8f82-4885786f3999', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a198cece-5f39-4cef-9625-360971bd41af', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 18', FALSE, NULL, NULL, '93065003-b808-4d3c-8f1f-6128867bb526', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a20e679f-0012-46c0-b870-4ce5888f080e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 27', FALSE, NULL, NULL, '14406b2e-9c50-4d80-be94-08f3acdbdd97', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a21ef388-5848-4b10-8644-55cf48299bad', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 7', FALSE, NULL, NULL, 'f34d8dfe-e1c8-4c68-866a-e90f6f3377c2', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a29522ae-2579-4d46-89ca-9e222aea8144', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 1', FALSE, NULL, NULL, '40f75731-a006-4ad7-a410-f47a5b58bec5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a2d160b3-2cbf-4706-8661-b45cfa040b9e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 31', FALSE, NULL, NULL, 'a613ebad-b529-4a56-88e9-493ad40765f8', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a31855e1-4df6-42f0-9767-47d619e9996c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 41', FALSE, NULL, NULL, '449926db-cdf1-4b14-9fa8-a2b6766786b7', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a324dbe6-e33d-4c0a-bc3e-835a3976e45f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 20', FALSE, NULL, NULL, '67922510-89bd-4788-be19-8c24d2c63817', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a331d70b-23b6-4f11-8bbf-95af76e75809', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 47', FALSE, NULL, NULL, '860a9902-001f-489f-9295-5a69039e7d91', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a35823af-11a2-4f2f-ae52-458d77a8eeb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 21', FALSE, NULL, NULL, '2dcfb228-9353-42e1-8db7-23f5d56226b4', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a3641109-1f24-4776-8b98-4821701c6948', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 46', FALSE, NULL, NULL, '99b9f78f-93e0-48d9-ad7b-15b8a374d16f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a38656fd-1316-43bd-89ea-2c22cef58758', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 18', FALSE, NULL, NULL, 'e75f77af-8aac-4094-957f-005eacc6f1b5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a3ea24d1-f8a0-447e-aec8-e77dc04dbbf0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 20', FALSE, NULL, NULL, '51f65c58-c42e-4c52-a251-3d80d6277e68', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a423162c-5a5e-4723-baee-2afbdc62dc1e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 15', FALSE, NULL, NULL, '30e024ab-24b7-4c0f-82c4-155082cfe77d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a4ece8a1-e7ee-4322-816f-eb84f65146fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 31', FALSE, NULL, NULL, 'a4636c51-f41d-4828-aaf2-c0faa14b70ac', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a513bc72-b680-4698-b626-ffef8e02c74b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 21', FALSE, NULL, NULL, '2dcfb228-9353-42e1-8db7-23f5d56226b4', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a5357204-ecaf-4088-b848-7ca4a1d838a2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 44', FALSE, NULL, NULL, '503bcc63-c633-42ed-a220-6770b4ae5d19', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a565a3a3-178c-449f-affb-808e7557d43f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 24', FALSE, NULL, NULL, 'd2a16cb0-ec69-4343-834b-c288f04c6b20', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a56f600e-2724-43bb-862a-66f68745c7e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 49', FALSE, NULL, NULL, '606501a2-4493-42b7-af27-765592f2fef5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a57fba45-ff9d-47ec-b62b-5bbfab45f378', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 13', FALSE, NULL, NULL, '45203b82-70ba-4466-99ac-7f37db57c051', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a58984af-3ff4-4f42-aaf5-01c0f1a94f12', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 35', FALSE, NULL, NULL, '70c38def-4a94-4db7-bdef-422b979df781', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a5d162a3-7967-4d86-97c4-9beb6d5fa3a6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 4', FALSE, NULL, NULL, 'd5ff567e-8d38-46f3-98b8-a20e70b5f278', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a5ed658c-d348-4db1-87f6-d7bb5a40451e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 34', FALSE, NULL, NULL, 'd77b92ea-6ed4-407c-b5c5-9aca7f704993', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a5fe0ba5-e4dd-4c47-b8a0-285311449538', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 14', FALSE, NULL, NULL, '48218200-93e8-4a38-b4e6-27217d34a846', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a6574dee-c2c3-4b9f-b88b-3bf880710036', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 33', FALSE, NULL, NULL, '358cf230-4e82-46b2-963d-4a41ce9bb0d8', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a6f8c179-e2e7-40f4-b2b8-ebb12850cbc3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 39', FALSE, NULL, NULL, 'b819a6ba-6ee1-404e-a388-311c5ce37c54', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a7053c0a-b6ff-4ebf-9895-a41a83936468', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 30', FALSE, NULL, NULL, '88af0997-4e0f-4647-84a4-9bf9aa5d58a3', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a72ac4d7-0837-40e6-b03d-9ecf5d887c41', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 7', FALSE, NULL, NULL, 'd2f88917-b564-44e1-9c30-7521f7460e76', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a7380a8d-2965-4fb3-b4fd-18b24a0ac212', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 30', FALSE, NULL, NULL, 'b555654b-12ea-4dec-a524-2df524e2934e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a75b075d-ddb2-49cf-b29e-891517814fcb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 3', FALSE, NULL, NULL, '54a9b703-a796-4a4a-8354-791e96444c82', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a77c5e07-8154-4537-b3f1-c69782a71626', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 43', FALSE, NULL, NULL, '596d1ee3-1e8d-4411-b14d-48a74fec4a98', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a7a09dfa-2dc4-48f7-b5ad-ca121e4c4e0e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 40', FALSE, NULL, NULL, '11189c0b-a7a4-470b-99c0-e26508addb98', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a82151fe-11a8-42ae-946e-b2aa2547c023', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 31', FALSE, NULL, NULL, '2e83b1c0-9e04-47f6-a372-34f0afb6a25f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a8616fb1-9452-4e25-8a64-971b269511bb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 47', FALSE, NULL, NULL, '0f708d81-d4bb-409e-9459-841cd2b6bd38', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a87b8b38-6d7c-48a2-9ba5-8417b2b7fa6d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 15', FALSE, NULL, NULL, 'f55fa1c6-9ffb-4c37-8c58-8f9c706db98f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a8a6ad9a-3e0d-4e46-848b-0ac31c657612', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 26', FALSE, NULL, NULL, '5ac90de7-0518-4c92-82d1-26e722be8e45', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a8f78838-3c3f-4185-8683-1a2ca79a1f89', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 49', FALSE, NULL, NULL, 'c68c918d-f860-493b-b13f-1748836c82ff', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a9290e61-56fa-4a59-905e-abe1f3c30c57', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 18', FALSE, NULL, NULL, '522b15ff-387d-4a0e-a952-6a499c1bdb23', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a9b2b26f-af6f-488c-879f-ab1dd1601237', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 2', FALSE, NULL, NULL, '551d1b5e-0dec-464f-9b26-f0e0abd48169', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('a9d8074e-3b33-4952-a0fb-d74a72586911', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 36', FALSE, NULL, NULL, '1b879a76-3aa1-473f-9b42-bb5df6a9ab4d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('aa8c172c-6170-4cb6-814d-879ae11b06f1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 13', FALSE, NULL, NULL, 'dfd235f0-275a-4abb-939a-afd6d1231851', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('aa97dd74-4a60-40de-9757-172333d948d9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 47', FALSE, NULL, NULL, '9bc82073-4c0f-4977-aa7f-9127849e53db', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('aaba6126-87f0-4da0-bf7c-eb088e4f5b10', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 45', FALSE, NULL, NULL, '6f37f1f2-fe08-49ee-9cf3-fb670e08401e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('aad71c96-084b-46a2-97e0-fd99301c8aac', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 29', FALSE, NULL, NULL, 'bcfdbe3b-348a-4815-aafb-8be35ff70226', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ab032c0c-8aea-429e-ba70-54c24586a586', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 38', FALSE, NULL, NULL, 'e78cf7fe-0ad6-4800-9324-e4d8e70829ef', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ab4440d6-2ffb-43eb-b82f-705f896bf11e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 26', FALSE, NULL, NULL, '0f1e31e3-8ccd-4794-bc4a-dc3f71002b1b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('abdfddc6-ff69-4bc6-ac30-ceb2b4223ef7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 9', FALSE, NULL, NULL, '4e00f153-c09c-45ff-8ded-1ffa06a338a5', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('abe394f4-07b0-4f9f-81c3-458b43ec3aa5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 8', FALSE, NULL, NULL, '2091727e-6e4e-41f7-9338-9a42829462ad', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ac12cfc8-d989-4db9-b51d-506334bc484d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 26', FALSE, NULL, NULL, 'c68e9452-8dfd-413b-8175-3a7d04f1e7bb', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ac17400f-4553-4c8e-9f60-1b2199077cec', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 43', FALSE, NULL, NULL, '2693ecbc-ed7f-4fe1-a876-b3a32a2f3d59', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ac2e5040-9216-4505-9015-a4f05bea6d4f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 48', FALSE, NULL, NULL, '6cf6c573-f039-42c3-8854-043cfce6d4a3', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ac50fee9-4db4-43e4-ad2b-245607c7d4fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 33', FALSE, NULL, NULL, '28b6e6c3-579d-4236-85eb-5668d70c0032', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ac5b8ce2-fe0f-4d19-9b24-f0dbd4103c7c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 26', FALSE, NULL, NULL, '0f1e31e3-8ccd-4794-bc4a-dc3f71002b1b', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ac96d19e-50d7-481b-bb31-eb1dddb90b7d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 23', FALSE, NULL, NULL, 'bd73d7ff-4a06-4377-950d-54f7a89e3961', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('acb9595e-dd85-4ffe-803b-fe8539d80f48', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 21', FALSE, NULL, NULL, '692f2a3d-84db-40f0-9a1b-5da056c52fb4', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('acf1efa5-fcc8-4f46-9670-82422b74d155', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 25', FALSE, NULL, NULL, 'c6aa7678-3585-4a11-afec-10171ba4fb3d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ad082a49-a700-4026-b3ee-46adc07daa04', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 44', FALSE, NULL, NULL, '9884986e-73eb-4e0d-88e8-20f406ec9ed0', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ad59c082-319c-40da-bc2f-3b9673cfda5b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 19', FALSE, NULL, NULL, '18af0818-28cf-4bc6-8452-dcf0c2c1780e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ad9ce3f3-f2e1-40ab-8022-3806f1d9a2ad', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 48', FALSE, NULL, NULL, '6cf6c573-f039-42c3-8854-043cfce6d4a3', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('adad1d0d-1bc7-49cb-af5e-fa5ea1a03a46', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 5', FALSE, NULL, NULL, '203170af-0331-470e-87f6-e768415a7a46', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('adafc8d9-be10-418e-8215-34601dbf2583', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 35', FALSE, NULL, NULL, '70c38def-4a94-4db7-bdef-422b979df781', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('adfe244b-2a48-431e-8bcb-399e6bc4a002', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 2', FALSE, NULL, NULL, '191fa3df-fdc8-41a6-b3bb-544193cf1210', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ae3f8c67-05f8-46c9-be36-f68dd8831fef', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 47', FALSE, NULL, NULL, 'af8b9e0d-c127-47b4-8632-497fcf6c4ff4', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ae67acee-7f55-4c77-ab12-22dac4bb290f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 47', FALSE, NULL, NULL, '3070c143-c4b2-427f-92f5-0d1bd884240d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ae7b9cfa-d5b1-4184-9cb6-d4ee85447ad0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 33', FALSE, NULL, NULL, '28b6e6c3-579d-4236-85eb-5668d70c0032', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('aec34dc2-e978-4e0d-9b90-2e1ab8ff6611', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 17', FALSE, NULL, NULL, 'd2bff9fa-d7fc-4a0a-8f82-4885786f3999', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af356ade-17ff-4f5b-928d-a38d76adf68d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 12', FALSE, NULL, NULL, 'bad1d4a7-0c44-4fd1-9fa3-cf355e1c0702', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af483e39-ec1d-43ec-9760-f03393daea1a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 20', FALSE, NULL, NULL, '51f65c58-c42e-4c52-a251-3d80d6277e68', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af591303-f478-470d-9d65-d7ac2a7d1f71', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 15', FALSE, NULL, NULL, '30e024ab-24b7-4c0f-82c4-155082cfe77d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af675f8a-4657-4e35-a92e-4810a518afd8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 22', FALSE, NULL, NULL, '409f93f2-bb1d-49e2-93b5-fa38aee9aac0', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af6a82e5-4437-4029-b76d-1a19d50afc5b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 25', FALSE, NULL, NULL, '4f4bae55-1d31-44df-acc8-08f2f34495e5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af74dedf-5e0c-4031-9efd-deb533150672', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 29', FALSE, NULL, NULL, 'b4aa6d10-04a5-404c-85ed-5f25b5a4bd4a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('af926f4e-ddf3-4acd-95ae-2417989cac7a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 38', FALSE, NULL, NULL, 'e78cf7fe-0ad6-4800-9324-e4d8e70829ef', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('afbe8830-ef53-4dbd-a2f0-c2709524ad93', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 34', FALSE, NULL, NULL, 'e525133f-c444-4435-a6c7-15ae65ca309d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b035c5c3-b40d-4f59-84f3-8becd627d6da', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 41', FALSE, NULL, NULL, '71582c79-cf34-4c0d-94f0-d627c27f2bb7', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b059d5eb-4144-4bbf-8909-008e2331b628', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 47', FALSE, NULL, NULL, '0f708d81-d4bb-409e-9459-841cd2b6bd38', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b074f6cf-17b0-465a-90a0-1e8f9762a03c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 40', FALSE, NULL, NULL, '11189c0b-a7a4-470b-99c0-e26508addb98', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b0b803de-aa9e-4156-9d3d-75e97a3c79ed', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 26', FALSE, NULL, NULL, '70d314c9-5518-4cc1-9bf2-5789ad251f3d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b0c27c9d-e773-46ae-8a99-a4afb9cd63a5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 3', FALSE, NULL, NULL, 'fba0139e-3e9d-4e2b-b468-aafa6345e364', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b104c344-efad-44af-ab96-f2620d3b1fbd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 28', FALSE, NULL, NULL, '122ebea7-11c5-4c48-9109-ec853b04e048', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b140404a-32c2-4c50-8b07-296280e37362', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 22', FALSE, NULL, NULL, 'd374ec62-adfb-4d85-a360-87359f257f95', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b150a765-e0ad-4e7a-bbbf-63b27f1eecfb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 24', FALSE, NULL, NULL, '1b1ce780-093d-46ef-a730-af0d0e82acf6', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b1516d9f-03ae-47de-b122-dbb6e194da75', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 12', FALSE, NULL, NULL, '2ef0a76d-e5c9-4d39-89c7-1c3ed829b887', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b19cc1ff-041b-4ec2-809b-19a12d277f18', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 12', FALSE, NULL, NULL, '37d9c504-92b4-4505-a2d3-0ed4d7a24618', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b2166163-5079-449d-b32f-13c6e3f89f03', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 39', FALSE, NULL, NULL, '21e75a8e-06d2-4517-b32a-e4da644d7d0e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b224ea5c-4741-4dfa-94dd-70d4581bad4f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 14', FALSE, NULL, NULL, '48218200-93e8-4a38-b4e6-27217d34a846', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b228e7ac-d6ed-45b6-bcd9-06f02d1d87d5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 12', FALSE, NULL, NULL, 'bad1d4a7-0c44-4fd1-9fa3-cf355e1c0702', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b275eb5e-ba0f-4e44-b604-70759e52e58f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 42', FALSE, NULL, NULL, 'c2180950-cc0d-428c-bd7b-c4e17ab74ebe', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b2bf9040-f66f-4ba8-8d8a-582cde4b5a11', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 38', FALSE, NULL, NULL, '522da0af-7a97-4b04-91ba-16a3f80d95cb', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b2c955bf-01fa-481d-a44e-76e44d47f17f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 11', FALSE, NULL, NULL, '69927048-8b96-4ab5-8494-1be98976d1c3', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b309bcdf-371a-4b1a-842e-f69718398e1d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 33', FALSE, NULL, NULL, 'f1a9c300-3185-44e6-a8f6-e50db68ca23a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b330c198-813e-47da-9177-15edb3b27876', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 45', FALSE, NULL, NULL, '1b63f0aa-6794-4ead-ae79-72d14b726f84', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b33936ad-4138-432a-bf04-69a125dd01b3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 43', FALSE, NULL, NULL, '596d1ee3-1e8d-4411-b14d-48a74fec4a98', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b34735fb-d62c-4d81-8674-26c5a16df7fd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 47', FALSE, NULL, NULL, '860a9902-001f-489f-9295-5a69039e7d91', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b37c5326-1392-4d01-bafa-cfecb869950e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 46', FALSE, NULL, NULL, '99b9f78f-93e0-48d9-ad7b-15b8a374d16f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b3a55482-da44-46c8-891f-e3b7af0af301', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 16', FALSE, NULL, NULL, '4ad13bcc-c048-4960-9a3b-ac65ac60356e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b3e1329f-3a61-4eba-a48e-e2b361fff9a5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 24', FALSE, NULL, NULL, 'd2a16cb0-ec69-4343-834b-c288f04c6b20', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b412d432-935f-4d9b-8649-6ec502a0c86e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 35', FALSE, NULL, NULL, 'e4a64e36-c504-413e-a950-14b20a6b2d0f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b417f24f-43b9-464b-9f52-cc1e7d019513', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 27', FALSE, NULL, NULL, '841564cf-a3e4-4fba-b0a5-5d2a3b1b1d65', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b4362c6e-e1d2-4e85-b7d1-aa0308db477e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 37', FALSE, NULL, NULL, 'aef4d6ad-40cb-4152-b723-de184bd58305', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b436c5cb-d1ef-44a5-8bda-9c0ceca87786', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 8', FALSE, NULL, NULL, 'f1c1ed6b-2b3e-4879-b0b9-48c747a7390c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b4a4ea7f-d2bd-4ecd-a807-253847151c75', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 13', FALSE, NULL, NULL, '5931c01b-a29c-4302-a52e-6b92e9df920e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b4c94fb4-0311-4300-bf7d-2fc3202fafba', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 13', FALSE, NULL, NULL, '5931c01b-a29c-4302-a52e-6b92e9df920e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b4f765d4-1692-44e0-b443-48972222c185', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 16', FALSE, NULL, NULL, '88419019-c938-4964-a3b2-c733095b7345', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b569db1b-7d7b-4cfe-937d-68ef7dd2bae8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 31', FALSE, NULL, NULL, 'a9db3b85-5f46-4590-8c25-24ff6ea9dd46', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b574eb9d-0f36-4693-875e-cc866829dd2b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 3', FALSE, NULL, NULL, 'b6f878dd-1118-455a-89a1-0dedaa210f68', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b5b0e089-d665-4e79-afc5-da18e9e3b52e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 8', FALSE, NULL, NULL, '63c190cb-95b6-4cad-8fa9-bf5c646006dd', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b5d9ec05-be4a-46f6-b526-507574cdc426', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 29', FALSE, NULL, NULL, '56566050-5df9-4862-aa6a-c1b6b9ac7dcd', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b5ff0ccc-cd11-4d12-9200-4c4edd4ff1e7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 20', FALSE, NULL, NULL, '2ff40769-0a9d-414d-a243-92a3fa1a6eaf', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b62988be-8373-46be-b0d7-45970b3d1520', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 35', FALSE, NULL, NULL, '8e3cdd81-26dc-40dc-90e5-c5614d3e984e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b690460e-2ab8-4285-835e-63cf528152cd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 35', FALSE, NULL, NULL, '75d17d86-f50c-48f5-a904-f74dca4acc26', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b6a03998-c05e-46b3-b4db-36333e61d5a8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 21', FALSE, NULL, NULL, '7e8e0278-e829-4f4a-9003-5b9d5269fd3a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b71a8d73-bab0-4eb2-9f79-c5ccb9551adc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 30', FALSE, NULL, NULL, 'db33df4d-ebb9-45f0-a0bf-d6690bbd2661', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b745b234-1aca-4301-9f6b-97077ea2f17c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 16', FALSE, NULL, NULL, '4ad13bcc-c048-4960-9a3b-ac65ac60356e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b7565a9d-faf6-4142-979d-84d7cebe3cff', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 9', FALSE, NULL, NULL, '8d0a2f85-e0d1-4409-a390-7abe0b0dcd47', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b758b009-535c-47ea-9aea-3d19ed54b3eb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 5', FALSE, NULL, NULL, '6c9c437c-1510-4f93-9af6-5bc251677b04', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b776d26b-db85-45ec-8604-ab3412b3761d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 49', FALSE, NULL, NULL, '969a4188-d35e-4b8f-a3c4-9a467199e36b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b7b24989-9b4f-43b6-8400-65ec9cc8d692', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 8', FALSE, NULL, NULL, '2091727e-6e4e-41f7-9338-9a42829462ad', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b7bb9a54-6c46-4d5f-aa53-eb9ac9152d4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 14', FALSE, NULL, NULL, '2281c295-a679-4239-893c-9fba737a9c04', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b7c6e459-efcd-44ac-ac3e-adbaa2605c56', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 4', FALSE, NULL, NULL, 'b10c67cc-f6f7-4dbf-bd86-452949506a0a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b7eb23c2-27f9-45a5-85ba-6e18b991b685', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 25', FALSE, NULL, NULL, 'c6aa7678-3585-4a11-afec-10171ba4fb3d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b82931a0-9723-4dc4-b0c4-e071f9125641', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 5', FALSE, NULL, NULL, '4d6f457e-b716-440b-8fa4-05e13c2d4b11', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b8537774-73ec-4d00-9abe-2134ba5f14e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 18', FALSE, NULL, NULL, '522b15ff-387d-4a0e-a952-6a499c1bdb23', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b855a307-76b8-47ec-8701-dde23aaa9b83', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 20', FALSE, NULL, NULL, '67922510-89bd-4788-be19-8c24d2c63817', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b85b8ae7-8296-4fd0-9745-8aef2e6f5873', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 32', FALSE, NULL, NULL, '5916df5b-f5a4-4255-97b4-887628b53681', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b89516e9-6f2e-4c38-ac67-96272c1635ca', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 43', FALSE, NULL, NULL, '374a0c50-6f84-4575-b4a8-9c9836ce9e1b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b898c04f-0d7e-4eb9-82b9-264cc795af43', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 12', FALSE, NULL, NULL, '5cce1e59-c9d7-4098-b1db-ce9bc23b053f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b8a5c0b3-13ef-4984-9fcd-773edd4bec03', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 8', FALSE, NULL, NULL, 'fb195671-3ba6-4c67-8f4e-67b6079b9e73', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b8c1d7bf-b679-413c-a477-6aa198c49d06', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 44', FALSE, NULL, NULL, '9884986e-73eb-4e0d-88e8-20f406ec9ed0', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b8e6b910-a88b-474c-8bb3-bedf4ad28a52', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 2', FALSE, NULL, NULL, '7518e8f5-51f9-4520-a1e2-cf39d0dba012', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b96432c5-7260-4d59-89fb-dc2c3bb34731', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 10', FALSE, NULL, NULL, '0b66c984-47b1-4918-945c-22c140253f66', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b981c7ed-8aa8-4553-8dac-fa596977decd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 3', FALSE, NULL, NULL, '54a9b703-a796-4a4a-8354-791e96444c82', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b9fb95cc-216d-49c9-8a4a-0185b3e6fcde', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 8', FALSE, NULL, NULL, 'f1c1ed6b-2b3e-4879-b0b9-48c747a7390c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('b9feaf3e-bde3-44bd-92b1-78fe145360f6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 37', FALSE, NULL, NULL, '9c586771-f8ac-4c51-ab18-9fc4210db45f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ba58e99b-a346-4d15-aa48-54d0ef11c5ec', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 32', FALSE, NULL, NULL, '6045ad93-b4d1-4108-9bc0-1daac8848713', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bada8c7e-077e-4fae-9803-bafa93978877', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 3', FALSE, NULL, NULL, 'b6f878dd-1118-455a-89a1-0dedaa210f68', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bae2b29e-c2fa-4386-a27d-a9f56d744311', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 36', FALSE, NULL, NULL, '7eb4887a-38b4-46e8-bb23-f9902cc5764c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bae69fc3-f8dc-4471-8933-13dc79209baa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 8', FALSE, NULL, NULL, 'f1c1ed6b-2b3e-4879-b0b9-48c747a7390c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bb3da2c2-ce61-486b-a657-45d25b106325', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 2', FALSE, NULL, NULL, '7518e8f5-51f9-4520-a1e2-cf39d0dba012', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bb70b8e2-5782-4cbd-889d-70aec0dd3b26', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 6', FALSE, NULL, NULL, '319d5049-7928-4cac-b5dc-136059e33b83', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bbd8a7cc-11d7-402d-b97a-28a7dc5d55bc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 28', FALSE, NULL, NULL, '6c7ffff4-12cf-4dee-a329-5259b050d0de', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bc386f3d-78c9-4cac-bcbf-7bcfc2a43324', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 29', FALSE, NULL, NULL, 'b4aa6d10-04a5-404c-85ed-5f25b5a4bd4a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bc72ee71-7751-4dfe-805f-836d66cfe84a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 5', FALSE, NULL, NULL, '203170af-0331-470e-87f6-e768415a7a46', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bc891476-8fbf-4c4d-801b-f71b93b82663', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 26', FALSE, NULL, NULL, 'e90397ca-786d-40c9-ba32-fab67f449831', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bc9dc4fd-f82d-483a-8821-04f88b062d77', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 44', FALSE, NULL, NULL, '97c2369b-7b21-4278-ac0d-d8579399b235', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bce2960a-1d08-4cd5-b919-e2245a746400', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 25', FALSE, NULL, NULL, '42f2a4c4-0ce4-4eb3-862e-549a717aaf7c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bcef731d-d08f-4c04-b1be-d48ef5dac942', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 31', FALSE, NULL, NULL, 'a9db3b85-5f46-4590-8c25-24ff6ea9dd46', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bd1b70ca-d539-4247-8a7f-b0bcd2ed572f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 38', FALSE, NULL, NULL, '852c6a86-9537-4e08-aa0c-48f76dbc82a0', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bd644b7f-e47a-4246-976d-6e0d47d644fd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 19', FALSE, NULL, NULL, '880d648a-128c-417d-95ee-c51551a0e5d2', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bd83487f-097f-4111-bc40-008b3665498b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 33', FALSE, NULL, NULL, 'd73ebd3b-9c1a-4fce-ba1f-141eb2be598f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bd8da96a-d6f4-46cb-b074-ce3de5f1e5b7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 7', FALSE, NULL, NULL, 'b44e0c68-0e53-4e8e-ace9-917badafc578', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bdc57e19-73ea-42d3-9c10-afd90f883697', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 34', FALSE, NULL, NULL, '8ecfce14-10a2-4827-bcd4-823b3d8f3f68', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bdcdaeee-6a6c-47ab-81a9-0127fa650465', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 21', FALSE, NULL, NULL, '81d172f8-2904-44d3-9f4e-bb7977d7f9d8', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('be17b64c-3219-4556-8cd3-fda4eb6efa10', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 39', FALSE, NULL, NULL, '207b76f8-609c-4b6b-b462-3138eaa91822', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('be7f5ca6-3f62-4617-b86e-cd0c07c38249', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 41', FALSE, NULL, NULL, '71582c79-cf34-4c0d-94f0-d627c27f2bb7', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('be850d0e-e25b-4945-9382-5728ba8fae66', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 49', FALSE, NULL, NULL, 'c68c918d-f860-493b-b13f-1748836c82ff', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('be86b2e0-ff4d-4cc6-a462-aecf192e3e4f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 12', FALSE, NULL, NULL, 'bad1d4a7-0c44-4fd1-9fa3-cf355e1c0702', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('be8fd520-8902-4651-9879-9dd33f9ea0b3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 36', FALSE, NULL, NULL, '2f830623-7758-4242-b2d6-630fcf83d0d9', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('beadef85-51b5-419f-bba9-74afdaa0ec3d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 24', FALSE, NULL, NULL, '1b1ce780-093d-46ef-a730-af0d0e82acf6', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bed4700f-a8c4-419e-973b-fa6827995d7b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 9', FALSE, NULL, NULL, 'f9dde99f-3afc-4322-ba9a-a757c4e8c72f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bf9daa3d-8e2e-49f9-98df-ca31cd2227bd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 9', FALSE, NULL, NULL, '8d0a2f85-e0d1-4409-a390-7abe0b0dcd47', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bfa34df1-5d16-4654-8522-c909474cd270', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 36', FALSE, NULL, NULL, '37cb14dc-e7e5-4601-b03c-075de1a9a712', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bfab5a6c-ea02-49fa-b345-6f8ca762d9bb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 11', FALSE, NULL, NULL, '69927048-8b96-4ab5-8494-1be98976d1c3', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('bfcc8585-673d-4352-a9ba-5dfa18ef9dbe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 33', FALSE, NULL, NULL, 'f1a9c300-3185-44e6-a8f6-e50db68ca23a', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c0210eeb-1cf5-4b8d-9b93-518103592797', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 26', FALSE, NULL, NULL, '0f1e31e3-8ccd-4794-bc4a-dc3f71002b1b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c0a844a9-68e0-464a-9b5a-17d6d99b8cf9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 45', FALSE, NULL, NULL, 'f35ca997-2bb0-4cf1-a475-2a5177ecbc28', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c0fe88ba-dd55-45f8-b176-7346500609df', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 7', FALSE, NULL, NULL, '086f2228-1683-4376-82a9-69148636a6aa', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c12ad7ad-32ba-4a9d-a277-1e4014424abf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 22', FALSE, NULL, NULL, '409f93f2-bb1d-49e2-93b5-fa38aee9aac0', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c152e185-4f73-45a6-84bd-c47bbb995713', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 45', FALSE, NULL, NULL, '1b63f0aa-6794-4ead-ae79-72d14b726f84', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c171afc9-6d59-413a-a68d-21e2fbcfacb2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 47', FALSE, NULL, NULL, '3070c143-c4b2-427f-92f5-0d1bd884240d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c1bcb5c0-1ab4-436f-8b35-224e2cf348be', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 31', FALSE, NULL, NULL, '2e83b1c0-9e04-47f6-a372-34f0afb6a25f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c2129115-9fe2-4ce9-9f3b-725345d5c898', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 30', FALSE, NULL, NULL, 'a6ea1246-14c9-44a2-9fc5-b0ccdd34229f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c2323654-dda2-475f-9b38-6cfdf9b98835', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 3', FALSE, NULL, NULL, '9d4916f7-4621-447e-8a6d-f6095f584e09', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c25c0507-8bcf-43cf-a5a3-d8cea81e8d3a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 35', FALSE, NULL, NULL, '8e3cdd81-26dc-40dc-90e5-c5614d3e984e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c25fd667-31d0-4ef9-b461-a7b00610d58e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 33', FALSE, NULL, NULL, '358cf230-4e82-46b2-963d-4a41ce9bb0d8', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c2620d50-0cd6-4756-994d-d2a097882863', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 6', FALSE, NULL, NULL, '41b4d323-0cd6-4d71-8aff-67f59d9c87d7', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c34ff399-59a6-4808-991d-8be5376e9b1d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 1', FALSE, NULL, NULL, 'd65ee2f3-4b12-4ce5-84e7-5eab507d2338', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c40a29df-ec6b-4802-be18-18e37759d624', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 19', FALSE, NULL, NULL, '18af0818-28cf-4bc6-8452-dcf0c2c1780e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c444106e-4959-4cea-aa3b-c8f0213bf135', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 4', FALSE, NULL, NULL, 'f6d6d5be-6640-43e6-b021-ea7464f018e7', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c4507977-dfe2-414b-8a63-db1e04d1aaf9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 23', FALSE, NULL, NULL, '705cd75f-1ced-48af-8ddd-6d3a568023d9', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c45edf2a-6461-468f-a7d1-6a8f97745919', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 13', FALSE, NULL, NULL, '7ebf7b04-71fd-42ca-8d26-e812f84cb136', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c4d85cb0-3a0f-4567-a106-c71c620cccc1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 33', FALSE, NULL, NULL, '28b6e6c3-579d-4236-85eb-5668d70c0032', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c4e3814b-b266-421e-b9b1-213da2cd58dd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 28', FALSE, NULL, NULL, '9e800fe9-3f75-42e8-ae66-96884def18c4', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c51d139f-1e5f-496a-886a-e6e4137b1bcd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 3', FALSE, NULL, NULL, '54a9b703-a796-4a4a-8354-791e96444c82', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c54e8cbc-8a17-484c-a5fe-d8046eaa8698', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 34', FALSE, NULL, NULL, '8ecfce14-10a2-4827-bcd4-823b3d8f3f68', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c566eb40-503b-41bc-b8a5-c113fd5068e9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 35', FALSE, NULL, NULL, '70c38def-4a94-4db7-bdef-422b979df781', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c5ae31ca-64be-42dc-a056-1c0fb369f04f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 32', FALSE, NULL, NULL, 'b2a7c2da-1311-44d9-a348-4b0c81852b3c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c5cccf38-f207-46eb-9750-7e019c9386e4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 36', FALSE, NULL, NULL, '7eb4887a-38b4-46e8-bb23-f9902cc5764c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c5f2aefd-60e0-45e9-a9ea-81d378c9fdf6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 15', FALSE, NULL, NULL, '13e859e1-df33-4bfd-b51b-9fe3493d1a19', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c5fcfd7a-cc45-4a24-896f-0d34191ccfdd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 30', FALSE, NULL, NULL, 'a6ea1246-14c9-44a2-9fc5-b0ccdd34229f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c62c9615-bfeb-45e3-a720-f5ea3c79f092', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 30', FALSE, NULL, NULL, 'db33df4d-ebb9-45f0-a0bf-d6690bbd2661', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c64f8d4e-77df-4514-b009-4debf68f4848', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 24', FALSE, NULL, NULL, 'd2a16cb0-ec69-4343-834b-c288f04c6b20', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c677b555-d191-49e7-b859-3645bbd4ac32', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 2', FALSE, NULL, NULL, '0ee337ff-d0cd-4125-a7e0-f5e1cb3abd0b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c692d324-05c2-4bb0-839e-cdaadaef030b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 34', FALSE, NULL, NULL, 'd33bb281-55da-4ab8-91ee-cd43d76029ef', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c6b3c2a3-b1c1-439b-be2d-9b0603003e4c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 33', FALSE, NULL, NULL, 'c145033c-061d-4b8e-bd50-d9dcde18c345', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c6ec83b1-9178-4f4d-ba8f-911f1c549afa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 4', FALSE, NULL, NULL, 'b10c67cc-f6f7-4dbf-bd86-452949506a0a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c7073669-bb7e-4590-b18a-c4f36c34c677', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 21', FALSE, NULL, NULL, '81d172f8-2904-44d3-9f4e-bb7977d7f9d8', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c7086b2b-5496-44dd-b0fa-d26bc37474b2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 45', FALSE, NULL, NULL, '69d8b25c-6e84-4d64-abbf-f558a5c81937', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c738d41a-b2ca-4872-acef-057f706429ec', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 22', FALSE, NULL, NULL, '409f93f2-bb1d-49e2-93b5-fa38aee9aac0', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c74807f1-badd-4239-9d31-3292e25869be', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 38', FALSE, NULL, NULL, '522da0af-7a97-4b04-91ba-16a3f80d95cb', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c7ed2e04-3426-49d1-b6b2-61d8d44a56c1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 1', FALSE, NULL, NULL, '40f75731-a006-4ad7-a410-f47a5b58bec5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c84f7eea-0d23-4812-975b-ddcb005288b3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 37', FALSE, NULL, NULL, 'aef4d6ad-40cb-4152-b723-de184bd58305', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c8512e92-d889-4785-9f88-1ab9c028a485', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 41', FALSE, NULL, NULL, '591c672f-bb0f-432c-ac4a-ea838b500f99', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c85a6a5d-6e2b-468c-a9dc-371e79b42b02', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 6', FALSE, NULL, NULL, '527cfb4e-3650-4bab-b958-06224a48800f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c884696e-9664-4e61-88cf-59373bee3868', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 21', FALSE, NULL, NULL, 'df9bdfd3-bb10-48b6-8c18-933b97149b17', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c8e698a4-95a7-498f-931e-8689177953e9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 4', FALSE, NULL, NULL, 'f6d6d5be-6640-43e6-b021-ea7464f018e7', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c8ed1702-9d9b-4255-911e-4bfa3b1b274a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 47', FALSE, NULL, NULL, '860a9902-001f-489f-9295-5a69039e7d91', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c957c4f5-826f-4274-b024-e0659fc7b63b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 11', FALSE, NULL, NULL, '54d28ef5-9273-4c94-87f8-469cedb7d54d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c993c813-c839-4946-957f-f2e1ff282768', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 14', FALSE, NULL, NULL, 'a65cf146-2ed5-4419-8161-1ed8b425c20e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c999afe5-7f29-4697-bc37-51bf66f8d0ab', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 15', FALSE, NULL, NULL, 'f55fa1c6-9ffb-4c37-8c58-8f9c706db98f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c9b46466-f343-4401-978f-216ba35aa8f6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 20', FALSE, NULL, NULL, 'de97684d-b1f0-4226-b10d-f4493445b48b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('c9eec4ed-df0c-4be1-95d8-a6eb13921be5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 48', FALSE, NULL, NULL, '5e888cbf-7dcd-43cf-877e-af5110e9c1f4', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ca3e0472-05fd-4bd3-9318-5796c0a6c52d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 42', FALSE, NULL, NULL, 'cb253826-8103-45fd-9fce-b569426684f1', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ca54a0a9-c145-496f-9650-ee5a92578aac', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 23', FALSE, NULL, NULL, 'bd73d7ff-4a06-4377-950d-54f7a89e3961', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ca734c2e-d28b-488c-8ca8-52da11adaccd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 13', FALSE, NULL, NULL, '7ebf7b04-71fd-42ca-8d26-e812f84cb136', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ca891b45-2305-45cb-9e41-ca1320a802c7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 2', FALSE, NULL, NULL, 'baeccae8-8259-4154-86e3-4b5a74c5cef7', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cabf323c-7934-41a1-ba61-bdb9fc4b9dd5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 13', FALSE, NULL, NULL, '5931c01b-a29c-4302-a52e-6b92e9df920e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cae1bd85-3204-49cf-aee1-6295c3cda161', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 11', FALSE, NULL, NULL, '69927048-8b96-4ab5-8494-1be98976d1c3', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cafdb897-3b7e-4866-8add-48741d97115c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 22', FALSE, NULL, NULL, 'd374ec62-adfb-4d85-a360-87359f257f95', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cb0f4ec2-28d5-4396-8fc6-02cc74fd2316', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 46', FALSE, NULL, NULL, '612db9f9-0488-4643-a5f5-97d7323de074', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cb17141c-0357-4e74-94a6-0e3d12939483', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 9', FALSE, NULL, NULL, 'bcecc5db-302b-4b1c-996d-fb326bfe2e68', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cb2ec718-252a-443f-b83d-56c721088052', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 23', FALSE, NULL, NULL, '705cd75f-1ced-48af-8ddd-6d3a568023d9', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cb56d35a-5868-48fb-9c9a-bc96be851b09', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 25', FALSE, NULL, NULL, 'dbb082c5-6eab-4588-aabb-c4de2b71306a', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cb84dc9d-9433-424f-b87c-430eaf6d223d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 38', FALSE, NULL, NULL, '995e7e7a-1ac3-44a6-9c32-c71898766558', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cbb5d6e3-6dd5-4930-a777-39f8e9aeb7e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 42', FALSE, NULL, NULL, 'c873f125-8b75-4268-9529-9956d3bfa9de', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cbb6c40b-7698-452a-91e8-bb986c21f058', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 21', FALSE, NULL, NULL, '692f2a3d-84db-40f0-9a1b-5da056c52fb4', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cbd167e0-dcfd-48ca-8397-b10e48950f88', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 18', FALSE, NULL, NULL, '950f1190-870e-49aa-8eca-320deff5a4e5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cc0b5bf0-9147-47bc-83e5-1cd94ad923d6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 36', FALSE, NULL, NULL, '1b879a76-3aa1-473f-9b42-bb5df6a9ab4d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cc90bdb7-848c-4926-ac8e-0936fe0f4972', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 32', FALSE, NULL, NULL, 'e91bf542-e4b9-48fd-8df1-2d8261ffd37d', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cca14711-fc82-4ba3-9ddb-960031ab3e66', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 21', FALSE, NULL, NULL, '81d172f8-2904-44d3-9f4e-bb7977d7f9d8', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ccd304d9-b9e8-46a2-950e-033bb15f1f30', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 50', FALSE, NULL, NULL, 'b88e4145-fc06-482c-a2f1-6ca07ba4b50b', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cd1b41d7-f8ba-41a5-b937-661d1c28e759', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 39', FALSE, NULL, NULL, 'b819a6ba-6ee1-404e-a388-311c5ce37c54', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cd72a05a-69b3-4725-b141-8df57e267492', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 15', FALSE, NULL, NULL, 'bcd9353a-951d-4b99-87bd-7217343d6ddd', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cd96843f-28e4-4d23-bba9-5e84beef554f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 36', FALSE, NULL, NULL, '7eb4887a-38b4-46e8-bb23-f9902cc5764c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cdef775c-41eb-4d33-8358-433e8f1b5242', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 30', FALSE, NULL, NULL, '5313ca23-e4f7-4455-8f1e-ca88f8c6d064', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cdff8c7e-b991-4def-8269-4961e3836029', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 23', FALSE, NULL, NULL, 'bd73d7ff-4a06-4377-950d-54f7a89e3961', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ce09ec34-cebf-4623-a8a4-5833150081c3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 9', FALSE, NULL, NULL, 'f9dde99f-3afc-4322-ba9a-a757c4e8c72f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ce601841-fefb-40d8-8dee-36118969c0d7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 23', FALSE, NULL, NULL, 'c51270ad-ae90-4b9d-829b-8f96ca0dd678', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ce88fc15-c164-4399-9f70-599df1c514fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 3', FALSE, NULL, NULL, '9d4916f7-4621-447e-8a6d-f6095f584e09', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ceadfd68-d569-4f0f-b8cd-c7b142de9e3a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 15', FALSE, NULL, NULL, '63696841-0700-4255-8f9a-189150ea218a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cece2e42-4d95-402c-bc3e-16c0ec05565a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 24', FALSE, NULL, NULL, '71ccafb9-a9b9-4929-978e-0b94cdf30f4e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cef65839-17db-497d-98b4-4d16a47e8d2c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 8', FALSE, NULL, NULL, '65c23e10-101d-4dfb-8127-b6baf0caa703', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cf1f3530-734d-4f03-ad91-f91383195f31', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 2', FALSE, NULL, NULL, 'baeccae8-8259-4154-86e3-4b5a74c5cef7', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cf27032f-f74e-4c19-bf54-856a384b1917', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 45', FALSE, NULL, NULL, 'be4e0397-e949-4f1a-9bc6-776d9157a1b8', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cfb632ee-a80d-4692-88e8-05dafd6750a5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 10', FALSE, NULL, NULL, '00c461a0-4190-4102-ae8d-285435cd9faa', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cfdb0511-c5f0-4e3a-967a-4713b0516a0e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 4', FALSE, NULL, NULL, '0b321d5e-f918-4ad6-ae22-2d4a013e9780', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('cfecc046-0120-4f9d-ba80-0531e7b8f737', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 42', FALSE, NULL, NULL, 'c2180950-cc0d-428c-bd7b-c4e17ab74ebe', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d035345c-f740-44e8-b53c-a84a80d826b7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 24', FALSE, NULL, NULL, '71ccafb9-a9b9-4929-978e-0b94cdf30f4e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d0423dad-505f-48ce-a7fb-53667d644555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 3', FALSE, NULL, NULL, '9d4916f7-4621-447e-8a6d-f6095f584e09', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d06e3c5c-de76-421b-9668-bc58728342e0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 24', FALSE, NULL, NULL, '1b1ce780-093d-46ef-a730-af0d0e82acf6', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d074a604-af93-4cd5-be05-731d72734a2b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 19', FALSE, NULL, NULL, 'a43ab879-8377-4b94-a4a0-1458a1bc52ed', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d077a75a-ab52-4cad-ac2f-2733db78a5d1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 34', FALSE, NULL, NULL, '2c04162e-b647-44c6-9d03-922960936c3e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d096dce2-cc5d-41d3-b741-999b7996e154', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 41', FALSE, NULL, NULL, '71582c79-cf34-4c0d-94f0-d627c27f2bb7', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d097e9b7-9ed8-4848-88fa-5241fc4746cb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 40', FALSE, NULL, NULL, '11189c0b-a7a4-470b-99c0-e26508addb98', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d09ade26-2c15-4ca5-83a0-820d041c32a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 50', FALSE, NULL, NULL, '431a53a0-3609-42f8-9bf2-f3273f1d95f1', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d0dd8595-950d-4676-9ac1-ffb66803b5f8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 35', FALSE, NULL, NULL, '70c38def-4a94-4db7-bdef-422b979df781', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d11741b4-1a4e-4fb9-afe6-828a5e5f009e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 11', FALSE, NULL, NULL, '54d28ef5-9273-4c94-87f8-469cedb7d54d', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d12c988c-6a95-4c3a-bf67-418a04236bce', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 40', FALSE, NULL, NULL, '7c42f679-02be-41af-8d67-24fa773dea79', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d1545336-6d13-4b04-8ce5-fdf4ad0ab779', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 7', FALSE, NULL, NULL, 'f34d8dfe-e1c8-4c68-866a-e90f6f3377c2', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d1c8814e-9c0a-4475-a55e-81ef022f665a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 39', FALSE, NULL, NULL, '5ab5b5ca-7a9c-4277-9f9b-2466ce655d06', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d2130fa0-e79d-41da-93f1-87d9a3dc7423', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 9', FALSE, NULL, NULL, 'f9dde99f-3afc-4322-ba9a-a757c4e8c72f', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d25ad5b7-3115-4eae-90f3-603bd1d74261', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 5', FALSE, NULL, NULL, '6c9c437c-1510-4f93-9af6-5bc251677b04', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d26ca4cf-2c20-45c8-9ace-11fd323d60ea', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 27', FALSE, NULL, NULL, '14406b2e-9c50-4d80-be94-08f3acdbdd97', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d2703354-1a11-4e57-98c5-76ec048bf865', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 4', FALSE, NULL, NULL, 'c1c2325c-4801-4f5f-abd6-0ba585f1c338', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d2776673-4133-498a-83cb-e14290ad9b9d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 4', FALSE, NULL, NULL, 'b10c67cc-f6f7-4dbf-bd86-452949506a0a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d2ab67a9-a39c-4040-97f6-7dace9129953', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 32', FALSE, NULL, NULL, '6045ad93-b4d1-4108-9bc0-1daac8848713', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d34ae961-165c-46da-bb2d-07c499df88ca', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 50', FALSE, NULL, NULL, '7e960356-fef4-4fbb-841a-14c1e59c3aea', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d3af5433-6b22-45ef-99a4-23dfd9969929', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 8', FALSE, NULL, NULL, '63c190cb-95b6-4cad-8fa9-bf5c646006dd', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d424a2fc-22d4-4571-b1f1-51f3755e83a8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 38', FALSE, NULL, NULL, '90fc41bf-e41b-4d30-8d7f-4e5dfdaef3e6', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d44dd2f8-10c4-47d3-bb88-043d2c685515', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 40', FALSE, NULL, NULL, '6ab450b8-2fa9-4ed2-9ebd-92f629542f16', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d46c7f59-f510-472a-ad66-91ef7b71b7f0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 5', FALSE, NULL, NULL, '5e30a962-e6dd-4137-9b04-b85655f9a728', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d5085983-2399-4fe3-b63b-920bc0ce7d2e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 3', FALSE, NULL, NULL, 'b6f878dd-1118-455a-89a1-0dedaa210f68', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d533abed-5bef-4f2e-9814-355a6b0aaaee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 24', FALSE, NULL, NULL, 'd2a16cb0-ec69-4343-834b-c288f04c6b20', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d5bf645a-f682-491a-90e4-f75e2b168a09', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 29', FALSE, NULL, NULL, '7baac172-08b3-42d7-94a4-64622e58ea84', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d5cc3772-c6ad-4f4c-aa88-4882b55ed4ae', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 48', FALSE, NULL, NULL, '7abbcac4-6847-4cd3-8d05-8f706f64da18', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d6209da4-889e-48e3-82a0-5a630a111110', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 48', FALSE, NULL, NULL, '6cf6c573-f039-42c3-8854-043cfce6d4a3', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d62ecc02-5c71-4926-b684-47e003c13e8a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 42', FALSE, NULL, NULL, 'c873f125-8b75-4268-9529-9956d3bfa9de', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d6a015b2-8bc8-4ae9-bb10-70847ec4c4e1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 13', FALSE, NULL, NULL, '901b0932-ec60-494e-897d-d589d0ee47be', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d6c1feb4-0903-4e50-b902-b87dc38185d7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 50', FALSE, NULL, NULL, 'ce59833a-0b20-4e77-ae6a-691a568f7279', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d720570d-a33d-4261-a485-1cac29cc3ebd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 26', FALSE, NULL, NULL, '0f1e31e3-8ccd-4794-bc4a-dc3f71002b1b', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d7574e11-d95f-4203-828a-49ebd941ddcf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 21', FALSE, NULL, NULL, '81d172f8-2904-44d3-9f4e-bb7977d7f9d8', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d762170f-e499-4d0d-9f62-07e6eac8d344', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 30', FALSE, NULL, NULL, 'a6ea1246-14c9-44a2-9fc5-b0ccdd34229f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d769e6db-f0df-4512-b0f1-31bc84f2a6be', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 37', FALSE, NULL, NULL, '00a4d2c6-0117-4fc1-a48a-3435af4edddf', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d77e975e-3f26-46c8-ac55-4693fc18b50b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 19', FALSE, NULL, NULL, 'bcf4363f-2fd8-4b68-8915-97a83f3f413c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d78b8f9a-cc9b-4fdf-805d-5f6fecbf30b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 18', FALSE, NULL, NULL, '6ee51009-71bd-40b7-aaba-767acb0900a2', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d7c19c7c-4d18-4860-9f5c-dcc2adfee7ee', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 46', FALSE, NULL, NULL, '612db9f9-0488-4643-a5f5-97d7323de074', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d7c76943-05a5-4b1f-a27c-b07a085ff6bc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 28', FALSE, NULL, NULL, '6c7ffff4-12cf-4dee-a329-5259b050d0de', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d7ca3d58-713d-4ded-87de-7759e655be6e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 26', FALSE, NULL, NULL, 'e90397ca-786d-40c9-ba32-fab67f449831', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d7d28f39-57ea-471a-8ed6-a64f0255fb7a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 6', FALSE, NULL, NULL, '676f6e82-35c5-44d3-887e-7ddf62191d3e', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d838f39a-95eb-401d-9fc7-eb852965a14c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 9', FALSE, NULL, NULL, '4e00f153-c09c-45ff-8ded-1ffa06a338a5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d849e102-c835-4c14-9245-aebe1a6bb4a9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 9', FALSE, NULL, NULL, '8d0a2f85-e0d1-4409-a390-7abe0b0dcd47', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d86e4630-0f4b-4ed5-9849-988d213d5710', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 36', FALSE, NULL, NULL, '7eb4887a-38b4-46e8-bb23-f9902cc5764c', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d8871b40-7b13-42e7-8dfc-e1e159c373b3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 27', FALSE, NULL, NULL, '14406b2e-9c50-4d80-be94-08f3acdbdd97', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d889455d-d722-4f72-b899-e7e3fad81d23', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 8', FALSE, NULL, NULL, '63c190cb-95b6-4cad-8fa9-bf5c646006dd', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d9a9a093-35ad-4ddc-a4d4-398e639a6c7a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 14', FALSE, NULL, NULL, '48218200-93e8-4a38-b4e6-27217d34a846', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('d9c70904-3447-4fed-8563-2d47c29fb047', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 1', FALSE, NULL, NULL, '5e54bed1-d4e6-41d9-a28e-d3a51e2c533c', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('da115846-49c9-4c34-ab97-44645688f6e5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 13', FALSE, NULL, NULL, '7ebf7b04-71fd-42ca-8d26-e812f84cb136', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('da6b1999-8077-4bf2-a7d3-f6e88cb6b6fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 6', FALSE, NULL, NULL, '22a16c96-9a98-481d-bb8c-f6da7b8e3a9d', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('da6e66ee-b151-4876-b6de-43939353a7b1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 24', FALSE, NULL, NULL, 'f2c6e32e-5b69-4c66-98cd-6b2689c2d5de', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('daee2327-9c78-434e-932c-90aba7aae459', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 13', FALSE, NULL, NULL, '5931c01b-a29c-4302-a52e-6b92e9df920e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dba80930-1498-4765-bf5c-6a96dac9cca4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 10', FALSE, NULL, NULL, '00c461a0-4190-4102-ae8d-285435cd9faa', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dbba5072-22f1-4744-a7bf-710e198af8a0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 34', FALSE, NULL, NULL, 'd33bb281-55da-4ab8-91ee-cd43d76029ef', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dbfb6b63-8dca-4327-b728-051c0a4f153a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 22', FALSE, NULL, NULL, 'd374ec62-adfb-4d85-a360-87359f257f95', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dc13acb7-bae0-4206-b872-d3d60ec6419e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 13', FALSE, NULL, NULL, '45203b82-70ba-4466-99ac-7f37db57c051', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dc7f9f8d-5cad-4a46-8b41-20f4face9b20', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 34', FALSE, NULL, NULL, 'd77b92ea-6ed4-407c-b5c5-9aca7f704993', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dcc1bd1a-4b87-4551-9f9f-fbc85eae96c3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 26', FALSE, NULL, NULL, 'c68e9452-8dfd-413b-8175-3a7d04f1e7bb', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dcc89582-6574-459f-839e-e5d0423926c4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 35', FALSE, NULL, NULL, '8590fa79-7f6f-4ef3-83d7-8c1ea4d11245', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dcec4f20-3169-4816-b37b-8bf42c37468c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 40', FALSE, NULL, NULL, '72bf85da-d581-4744-baec-9eeadfee6c99', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dd77644e-2963-4260-a891-66a65c1deec3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 19', FALSE, NULL, NULL, '8e0fd2c2-f36a-42fd-8c0f-c379361d18a1', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dd7d022d-e8d9-418d-b7f2-7b86cac9fa06', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 14', FALSE, NULL, NULL, '48218200-93e8-4a38-b4e6-27217d34a846', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dd8da616-daff-460a-bb9f-c29e9e369f08', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 20', FALSE, NULL, NULL, '6899e753-5aa3-4604-b1ca-91b14e38217f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de0974bb-4701-4d6f-8cb8-e74ec5b7327b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 48', FALSE, NULL, NULL, '04dff112-bec8-4fc2-9ea4-48cd4ab055c2', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de1fda6e-228d-4cfe-be29-cacd750643a1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 14', FALSE, NULL, NULL, '51599bc8-3b12-42a0-8355-faea16e988ac', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de20456b-f007-4666-b02e-f3233f4a5ffe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 14', FALSE, NULL, NULL, 'a65cf146-2ed5-4419-8161-1ed8b425c20e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de24d41e-00a8-4e5a-9fbd-31b71f71a0a9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 47', FALSE, NULL, NULL, 'af8b9e0d-c127-47b4-8632-497fcf6c4ff4', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de4d14b6-2614-4bf5-8cfc-1cdc627b440b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 12', FALSE, NULL, NULL, '2ef0a76d-e5c9-4d39-89c7-1c3ed829b887', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de88f5c1-a863-40f9-ab10-2bdfa6861d1c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 2', FALSE, NULL, NULL, '0ee337ff-d0cd-4125-a7e0-f5e1cb3abd0b', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('de967857-dc90-417b-b8e3-429c2d38fa7e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 45', FALSE, NULL, NULL, 'be4e0397-e949-4f1a-9bc6-776d9157a1b8', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('def41b97-3467-4234-af22-eed704935da9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 6', FALSE, NULL, NULL, '22a16c96-9a98-481d-bb8c-f6da7b8e3a9d', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('df031569-e271-42ac-b6ad-a6e8b0987a48', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 21', FALSE, NULL, NULL, '692f2a3d-84db-40f0-9a1b-5da056c52fb4', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('df5f580c-c4e3-4238-9820-01798503d37d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 25', FALSE, NULL, NULL, '4f4bae55-1d31-44df-acc8-08f2f34495e5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('df8e5642-eee2-4697-aeba-c47c462e9a25', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 46', FALSE, NULL, NULL, '234e8d78-4de2-4e43-b6ec-8ace773ab4ad', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dfaeb1ff-2377-4289-a390-c3f45222b4c8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 28', FALSE, NULL, NULL, '6c7ffff4-12cf-4dee-a329-5259b050d0de', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dfe9eb64-a47f-4391-9306-e81d9bb3bc2b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 18', FALSE, NULL, NULL, 'e75f77af-8aac-4094-957f-005eacc6f1b5', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('dfea680f-5a88-4b54-87a2-e4fb2e1f079a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 12', FALSE, NULL, NULL, 'bad1d4a7-0c44-4fd1-9fa3-cf355e1c0702', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e0012b94-d007-446c-9d69-fe8fa3c2c108', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 10', FALSE, NULL, NULL, 'f2f809ce-ce87-4ef5-b3e3-1c6893942872', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e0022c3f-92de-4089-a5f1-fe8c4055abae', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 26', FALSE, NULL, NULL, '5ac90de7-0518-4c92-82d1-26e722be8e45', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e004d440-c6e4-46de-bbd8-9c005c982fac', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 6', FALSE, NULL, NULL, '319d5049-7928-4cac-b5dc-136059e33b83', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e05495da-5256-42b9-b1a1-1f849fc31ccc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 3', FALSE, NULL, NULL, '54a9b703-a796-4a4a-8354-791e96444c82', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e06210f3-25a6-4fd0-acbe-f804a7783dfe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 5', FALSE, NULL, NULL, '5e30a962-e6dd-4137-9b04-b85655f9a728', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e064d234-a48d-493a-a0c0-cc95b256f03f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 13', FALSE, NULL, NULL, '45203b82-70ba-4466-99ac-7f37db57c051', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e0a8c9c4-838a-438b-afa3-13cc791633df', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 29', FALSE, NULL, NULL, '56566050-5df9-4862-aa6a-c1b6b9ac7dcd', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e0efefaf-c289-4eed-9a23-22ee40e05e68', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 15', FALSE, NULL, NULL, 'f55fa1c6-9ffb-4c37-8c58-8f9c706db98f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e0f1d6e9-224f-42e4-914b-a134dbd8059e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 43', FALSE, NULL, NULL, '374a0c50-6f84-4575-b4a8-9c9836ce9e1b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e0fada38-ba24-4bdf-a990-344737d07be2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 5', FALSE, NULL, NULL, '4d6f457e-b716-440b-8fa4-05e13c2d4b11', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e174b016-ec58-41e5-9fc6-d0f366ffe310', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 22', FALSE, NULL, NULL, '392faa72-3fcc-4a5e-b04c-5e9322800108', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e18595c1-0e97-455b-a05e-d3d4dabe3a37', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 28', FALSE, NULL, NULL, '122ebea7-11c5-4c48-9109-ec853b04e048', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e235e772-6746-4a16-96d1-1cd84fb78522', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 7', FALSE, NULL, NULL, 'd2f88917-b564-44e1-9c30-7521f7460e76', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e255d62b-4ee2-4380-be06-595abc614837', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 38', FALSE, NULL, NULL, '995e7e7a-1ac3-44a6-9c32-c71898766558', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e2664f19-ca2b-4d22-ba5f-cdfe3bcf7cbc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 5', FALSE, NULL, NULL, '4d6f457e-b716-440b-8fa4-05e13c2d4b11', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e2de3f30-ae4d-4a13-a203-455ad4609be9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 27', FALSE, NULL, NULL, '2d24f583-0a55-4e10-9e6a-197506dbc960', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e32d28ba-a7b6-4cf8-be2a-4ae229f86c54', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 11', FALSE, NULL, NULL, '12b51827-b1ef-4e87-8bb5-e4f91a135944', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e37fa591-fcc8-4237-806d-da07e1754159', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 15', FALSE, NULL, NULL, 'bcd9353a-951d-4b99-87bd-7217343d6ddd', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e3abc0f0-6e0f-4876-a418-d06c0cf70193', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 10', FALSE, NULL, NULL, '39d1e88e-3153-43a3-9a96-a36ff60e26f9', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e3aed199-664a-4d8e-8fad-93b4fa8b7e80', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 9', FALSE, NULL, NULL, '8d0a2f85-e0d1-4409-a390-7abe0b0dcd47', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e3fd9ea3-dc89-4269-9cfb-df9e3b0bda08', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 19', FALSE, NULL, NULL, 'bcf4363f-2fd8-4b68-8915-97a83f3f413c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e4028887-21e9-4909-a729-a0c4103816c6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 30', FALSE, NULL, NULL, '5313ca23-e4f7-4455-8f1e-ca88f8c6d064', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e42f517f-ec89-4fba-bf6d-cffb55233f7f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 41', FALSE, NULL, NULL, '591c672f-bb0f-432c-ac4a-ea838b500f99', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e486a6b6-0332-497d-8652-8b98be259fe9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 39', FALSE, NULL, NULL, '21e75a8e-06d2-4517-b32a-e4da644d7d0e', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e4d1ced7-1abf-4926-a943-ebe7fdc9ba9d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 1', FALSE, NULL, NULL, 'd65ee2f3-4b12-4ce5-84e7-5eab507d2338', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e52da7bc-d023-4e6b-bf3f-c244aa7f698d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 10', FALSE, NULL, NULL, '0b66c984-47b1-4918-945c-22c140253f66', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e54cfa18-c390-4407-a720-8ce18d058741', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 50', FALSE, NULL, NULL, '7e960356-fef4-4fbb-841a-14c1e59c3aea', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e5ba23a0-b7c7-455c-bdbf-4aa9b6bae8bb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 9', FALSE, NULL, NULL, '8d0a2f85-e0d1-4409-a390-7abe0b0dcd47', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e5dc3ddd-0015-4c13-9a6f-b0e99fefac8c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 10', FALSE, NULL, NULL, '59d9bb8a-5e67-4825-ace6-332d7739a3a3', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e60959a3-91e5-48f0-aba8-59b118db9989', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 41', FALSE, NULL, NULL, '591c672f-bb0f-432c-ac4a-ea838b500f99', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e6929517-1209-4c4e-a3e5-72f84e10915e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 42', FALSE, NULL, NULL, 'c2180950-cc0d-428c-bd7b-c4e17ab74ebe', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e6eb5fd1-c4cf-4d92-8680-8d355f475392', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 24', FALSE, NULL, NULL, 'f2c6e32e-5b69-4c66-98cd-6b2689c2d5de', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e70c2eb1-b78e-43e3-bc95-6e24e4f79da8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 10', FALSE, NULL, NULL, '0b66c984-47b1-4918-945c-22c140253f66', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e71751aa-a87e-4211-abf0-0c7a45a7c3f7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 15', FALSE, NULL, NULL, '13e859e1-df33-4bfd-b51b-9fe3493d1a19', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e77aa732-8e07-4929-9b67-6dc0600086a8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 46', FALSE, NULL, NULL, '99b9f78f-93e0-48d9-ad7b-15b8a374d16f', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e77e750e-86d1-4187-b091-6da7573ed238', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 17', FALSE, NULL, NULL, '8136f80f-4aa7-49a0-a383-4128a17e6179', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e8102fcb-3b2b-4f47-9d7d-7884bcc3d540', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 8', FALSE, NULL, NULL, 'fb195671-3ba6-4c67-8f4e-67b6079b9e73', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e830533d-36eb-4483-ac1f-a79f94fee9a8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 31', FALSE, NULL, NULL, '8783d6c9-f7c6-4ed5-8cc8-4e64dc3f73d0', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e84c0567-9ca8-4983-b716-c17266d0dc8f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 29', FALSE, NULL, NULL, 'bcfdbe3b-348a-4815-aafb-8be35ff70226', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e84dd078-f038-48aa-874d-6210c0050764', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 25', FALSE, NULL, NULL, '42f2a4c4-0ce4-4eb3-862e-549a717aaf7c', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e8632f56-7005-40f0-bd02-07c988d0ddc1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 47', FALSE, NULL, NULL, '0f708d81-d4bb-409e-9459-841cd2b6bd38', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e883f68f-a7e3-4a07-90c9-856fa926e088', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 48', FALSE, NULL, NULL, '5e888cbf-7dcd-43cf-877e-af5110e9c1f4', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e89d7b1e-4159-4db3-a5c5-5ba64fb62c9a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 23', FALSE, NULL, NULL, '42d990d2-58de-4c3b-b03c-681418a57c94', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e9648028-e97f-4143-bb39-2312baeae0ed', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 11', FALSE, NULL, NULL, 'de9ad5c1-63b3-4e61-99fe-0133f0985dcb', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e99de3ee-dc04-4461-a185-1c32d064700e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 38', FALSE, NULL, NULL, 'e78cf7fe-0ad6-4800-9324-e4d8e70829ef', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('e9f6299a-85fd-4708-966f-51ac8ca654e2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 5', FALSE, NULL, NULL, '203170af-0331-470e-87f6-e768415a7a46', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ea1dd608-32ae-4304-8506-9752d31daf00', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 27', FALSE, NULL, NULL, '695c1d0c-819c-4f3a-9978-b0f90aff7dfc', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ea547019-d64f-42ad-b8fc-0c396d66d583', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 18', FALSE, NULL, NULL, 'e75f77af-8aac-4094-957f-005eacc6f1b5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('eb4fb114-815d-410f-a309-a7e3ec2766fc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 14', FALSE, NULL, NULL, 'a65cf146-2ed5-4419-8161-1ed8b425c20e', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('eb508f54-1131-489e-ac3c-73eddda04e4a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Soft Skills Skill 35', FALSE, NULL, NULL, '8e3cdd81-26dc-40dc-90e5-c5614d3e984e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('eb857837-e7db-4223-97be-4b75ab53c74c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 11', FALSE, NULL, NULL, 'de9ad5c1-63b3-4e61-99fe-0133f0985dcb', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('eb9c1b03-f315-4479-ba3d-7a5181599fe1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 19', FALSE, NULL, NULL, '8e0fd2c2-f36a-42fd-8c0f-c379361d18a1', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ec318610-9a5b-46bb-9922-07185cf30dd2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 19', FALSE, NULL, NULL, '880d648a-128c-417d-95ee-c51551a0e5d2', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ec421bb5-caf0-4693-9707-0e2f7d94a666', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 41', FALSE, NULL, NULL, '78663a94-cd19-4252-a908-2ee87c8c9106', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ed6aeb92-42c9-43d0-a235-3af8b44e5cf9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 3', FALSE, NULL, NULL, 'b6f878dd-1118-455a-89a1-0dedaa210f68', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ed7055fc-cf71-4ea4-94a6-0efdf3733dd4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 21', FALSE, NULL, NULL, '2dcfb228-9353-42e1-8db7-23f5d56226b4', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ed86ef6f-e3f8-42e9-829d-3a9305fa2815', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 15', FALSE, NULL, NULL, 'bcd9353a-951d-4b99-87bd-7217343d6ddd', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ed9f1133-3f62-499e-b104-5e9686caa974', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 36', FALSE, NULL, NULL, '37cb14dc-e7e5-4601-b03c-075de1a9a712', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('edb122ee-0e9b-4560-9691-bc5a98a771c1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 29', FALSE, NULL, NULL, '19427640-0844-465a-ae50-9bf6e471a0fd', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('eeb6136e-16eb-465e-a7e9-7059afeae064', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 4', FALSE, NULL, NULL, 'f6d6d5be-6640-43e6-b021-ea7464f018e7', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ef0fc21c-d789-4503-9d7b-db4c0cae8a1c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 20', FALSE, NULL, NULL, '51f65c58-c42e-4c52-a251-3d80d6277e68', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ef31e2a1-5864-4501-8688-354c4d1f0cbe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 5', FALSE, NULL, NULL, '5e30a962-e6dd-4137-9b04-b85655f9a728', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ef3e5d6c-7c4a-4f5a-9a56-0de3135f7fa1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 24', FALSE, NULL, NULL, '063f3287-9615-4d61-8fea-609c9a9438a1', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ef82c344-1e3d-4e60-aa0f-bdae223ff9f4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 16', FALSE, NULL, NULL, '4ad13bcc-c048-4960-9a3b-ac65ac60356e', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ef90eef2-9e70-4e3e-bd2f-bfbc792c3544', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 33', FALSE, NULL, NULL, '28b6e6c3-579d-4236-85eb-5668d70c0032', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('efb0ea66-a048-488f-bf96-5cf43e0b8425', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 25', FALSE, NULL, NULL, '4f4bae55-1d31-44df-acc8-08f2f34495e5', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('eff4aeaa-b50f-49f0-aca9-51ed0c754136', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 8', FALSE, NULL, NULL, 'f1c1ed6b-2b3e-4879-b0b9-48c747a7390c', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f002727b-3ff3-4cba-bc3b-6c73da69f88f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 17', FALSE, NULL, NULL, '8136f80f-4aa7-49a0-a383-4128a17e6179', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f047050d-b057-4e2d-80e5-d94a1769955e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Leadership Skill 48', FALSE, NULL, NULL, '7abbcac4-6847-4cd3-8d05-8f706f64da18', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f048da0f-d32c-4f73-b9f8-7f8719a84936', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 48', FALSE, NULL, NULL, '4cdd89a2-c8d5-4191-92f4-cb0aaaaf3d30', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f1cd4bed-542e-4d3c-b0c2-6dc50666d3e7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 27', FALSE, NULL, NULL, 'e749e4ce-74b5-4106-ae09-933b730fbc1a', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f1d5324c-cb08-4a87-b7b4-dc445e16a9da', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 6', FALSE, NULL, NULL, '527cfb4e-3650-4bab-b958-06224a48800f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f1dfa46f-496e-43f8-9435-3b0afa055266', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 7', FALSE, NULL, NULL, 'b44e0c68-0e53-4e8e-ace9-917badafc578', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f1f05475-7cfc-4b21-a182-f2e55b3defe4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 27', FALSE, NULL, NULL, '695c1d0c-819c-4f3a-9978-b0f90aff7dfc', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f1f84f49-2088-4c36-92bb-f5d0ccbe84f3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 40', FALSE, NULL, NULL, '7c42f679-02be-41af-8d67-24fa773dea79', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f25b7ca6-c508-47a5-b767-787da8785f73', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 4', FALSE, NULL, NULL, 'c1c2325c-4801-4f5f-abd6-0ba585f1c338', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f2601837-a0bd-44c1-a96e-0910e62d3696', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 49', FALSE, NULL, NULL, 'c68c918d-f860-493b-b13f-1748836c82ff', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f282256e-a716-4bb5-a4a8-e1d7cacaecf1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 50', FALSE, NULL, NULL, 'b88e4145-fc06-482c-a2f1-6ca07ba4b50b', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f2cf3da8-98c8-4c58-8807-ef2ca34bf942', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 28', FALSE, NULL, NULL, '3e7af9b7-a3b8-4198-8d86-f91d23032194', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f2dc448c-b05b-4060-8420-2638500ad042', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 35', FALSE, NULL, NULL, '8590fa79-7f6f-4ef3-83d7-8c1ea4d11245', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f2e51b8b-2925-4a40-b34e-fff1cd2048bc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 43', FALSE, NULL, NULL, 'cf7ae403-1905-4b2a-8ef9-4d77f64c7920', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f3449821-d9a4-4f25-a498-f9e97655a3ac', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 27', FALSE, NULL, NULL, 'e749e4ce-74b5-4106-ae09-933b730fbc1a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f3698af8-3d09-46e7-ad61-b1bf0a249811', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 7', FALSE, NULL, NULL, 'f34d8dfe-e1c8-4c68-866a-e90f6f3377c2', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f3a93fc0-015a-4e07-bbdb-84d3b3569d61', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 9', FALSE, NULL, NULL, '1ee8a770-2a9f-4760-ad48-0cc2d469eba2', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f3ac0828-d80c-4e5c-b107-a92a2e6cab00', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 25', FALSE, NULL, NULL, 'dbb082c5-6eab-4588-aabb-c4de2b71306a', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f3c771ea-1e24-4764-96a0-cd3589383752', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 38', FALSE, NULL, NULL, '995e7e7a-1ac3-44a6-9c32-c71898766558', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f3cc768e-c69e-485c-ae14-5d1d2e8b7ec9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 48', FALSE, NULL, NULL, '6cf6c573-f039-42c3-8854-043cfce6d4a3', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f436b7a6-078f-48bc-8cbe-1b1de16228e3', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 19', FALSE, NULL, NULL, 'bcf4363f-2fd8-4b68-8915-97a83f3f413c', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f4776791-2806-4c8d-9f35-5623f36593a7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 44', FALSE, NULL, NULL, '503bcc63-c633-42ed-a220-6770b4ae5d19', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f486b447-e642-4947-9a40-5628ace9143b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 46', FALSE, NULL, NULL, '234e8d78-4de2-4e43-b6ec-8ace773ab4ad', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f4a81a1d-54df-4c7c-915e-4431432d1e90', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 48', FALSE, NULL, NULL, '04dff112-bec8-4fc2-9ea4-48cd4ab055c2', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f4b06d22-85da-49e4-b1f4-74bf048919cf', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 33', FALSE, NULL, NULL, '28b6e6c3-579d-4236-85eb-5668d70c0032', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f513ee34-42e9-42bb-b588-a98643cdf90f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Business Skill 11', FALSE, NULL, NULL, '12b51827-b1ef-4e87-8bb5-e4f91a135944', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f5853b10-aeef-415a-8976-1f63c685b5df', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 15', FALSE, NULL, NULL, '13e859e1-df33-4bfd-b51b-9fe3493d1a19', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f587885c-6cc0-4c2c-a33f-9888f3ed639a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 8', FALSE, NULL, NULL, '65c23e10-101d-4dfb-8127-b6baf0caa703', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f5e115d2-8b9d-4bd4-9cde-a768eed1e0cb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 45', FALSE, NULL, NULL, 'be4e0397-e949-4f1a-9bc6-776d9157a1b8', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f60aac27-f70c-4703-9ca4-9370ede1048b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 13', FALSE, NULL, NULL, 'dfd235f0-275a-4abb-939a-afd6d1231851', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f62c8897-020d-4a38-82ca-864591c8fd18', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 29', FALSE, NULL, NULL, '19427640-0844-465a-ae50-9bf6e471a0fd', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f690a08c-6e38-402f-9d1b-ae55437c9474', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 31', FALSE, NULL, NULL, '8783d6c9-f7c6-4ed5-8cc8-4e64dc3f73d0', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f6d85300-6afb-4bd0-bdef-eaf6361ec7ad', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 40', FALSE, NULL, NULL, '6ab450b8-2fa9-4ed2-9ebd-92f629542f16', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f6fe502c-e2cd-49f8-8256-89812c169b7f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 23', FALSE, NULL, NULL, '42d990d2-58de-4c3b-b03c-681418a57c94', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f7330b7c-1097-4a81-b086-56496bfad06b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 38', FALSE, NULL, NULL, '852c6a86-9537-4e08-aa0c-48f76dbc82a0', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f7c5fa31-97e6-4380-8d3d-5f5abd751f7e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 9', FALSE, NULL, NULL, '4e00f153-c09c-45ff-8ded-1ffa06a338a5', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f7c7a917-6bc8-4906-b011-225d270fd698', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Soft Skills Skill 44', FALSE, NULL, NULL, '40951a9d-e30d-4e01-b43c-8de0d76377e7', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f7e05a7b-10d1-4ec3-9eef-7d5151e7d1de', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 2', FALSE, NULL, NULL, '551d1b5e-0dec-464f-9b26-f0e0abd48169', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f803d0eb-1d60-46f3-8ca1-e0b496d7850d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Leadership Skill 21', FALSE, NULL, NULL, 'df9bdfd3-bb10-48b6-8c18-933b97149b17', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f83f2f20-8852-44dc-9c06-7c1ff1fe2dfc', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 23', FALSE, NULL, NULL, 'bd73d7ff-4a06-4377-950d-54f7a89e3961', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f86f8609-4484-4a58-a32f-c125707675fe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 48', FALSE, NULL, NULL, '4cdd89a2-c8d5-4191-92f4-cb0aaaaf3d30', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f8e54f14-892a-4e36-9e15-d37ad6874002', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 22', FALSE, NULL, NULL, '392faa72-3fcc-4a5e-b04c-5e9322800108', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f94ef141-bf50-4e18-b37b-9a283bac0673', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 4', FALSE, NULL, NULL, 'd5ff567e-8d38-46f3-98b8-a20e70b5f278', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f9b69902-6631-4df6-8c29-2ef7c29853fd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 47', FALSE, NULL, NULL, '3070c143-c4b2-427f-92f5-0d1bd884240d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f9d1dc38-3705-436f-9037-b27c81ac23b9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 48', FALSE, NULL, NULL, '04dff112-bec8-4fc2-9ea4-48cd4ab055c2', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('f9fb9d8b-07d8-4192-931c-fa1918d816e4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Business Skill 30', FALSE, NULL, NULL, 'db33df4d-ebb9-45f0-a0bf-d6690bbd2661', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fa000841-bbc0-4de5-b0e3-7f04d3df0e39', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 3', FALSE, NULL, NULL, 'fba0139e-3e9d-4e2b-b468-aafa6345e364', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fb18ff29-c957-4c21-ad82-c1d4647a7137', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 37', FALSE, NULL, NULL, 'aef4d6ad-40cb-4152-b723-de184bd58305', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fb417853-a450-40e8-8446-7770b75d1a12', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Domain Specific Skill 36', FALSE, NULL, NULL, '2f830623-7758-4242-b2d6-630fcf83d0d9', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fb6d6a3f-adf1-4cef-8b28-17543732382f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 42', FALSE, NULL, NULL, 'cb253826-8103-45fd-9fce-b569426684f1', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fc145f66-bdde-4f28-861b-0b273bea6d5a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 2', FALSE, NULL, NULL, '551d1b5e-0dec-464f-9b26-f0e0abd48169', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fc164bc8-43cb-4629-ab89-6585c91f04b8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Business Skill 49', FALSE, NULL, NULL, 'c68c918d-f860-493b-b13f-1748836c82ff', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fc54e9b4-1f29-49b2-bb7d-3e504d8378ce', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 31', FALSE, NULL, NULL, 'a613ebad-b529-4a56-88e9-493ad40765f8', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fc7befae-dc71-4c06-a748-57e57198e69a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Technical Skill 36', FALSE, NULL, NULL, 'b7bee390-307b-42b0-90d2-d0b619e2625f', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fca24ef3-85a0-44ea-91e2-3e5913b47849', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Domain Specific Skill 3', FALSE, NULL, NULL, '9d4916f7-4621-447e-8a6d-f6095f584e09', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fcc5ddcd-db5b-41c7-bfe1-072ea1a27069', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Leadership Skill 45', FALSE, NULL, NULL, '6f37f1f2-fe08-49ee-9cf3-fb670e08401e', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fd15b603-b4ab-4f54-a1f7-7b9b91bf67f4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 22', FALSE, NULL, NULL, 'e6621657-485e-4d2c-820b-0c5ef78c9139', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fd212074-cd03-49ef-ab5c-8283ccf30964', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 29', FALSE, NULL, NULL, '19427640-0844-465a-ae50-9bf6e471a0fd', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fd6d6aa7-3147-480c-a177-e4cc11a76a5d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Domain Specific Skill 44', FALSE, NULL, NULL, '127356ef-cc28-461b-95e0-622c7f96e15f', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fdaf37e9-41ab-4c84-bf51-b9e26c6c7378', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 3 for Domain Specific Skill 27', FALSE, NULL, NULL, '2d24f583-0a55-4e10-9e6a-197506dbc960', 'Level 3', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fdec07bf-97b5-4288-aa90-beeddb95a5e6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Soft Skills Skill 20', FALSE, NULL, NULL, '2ff40769-0a9d-414d-a243-92a3fa1a6eaf', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe0bb6d2-8495-4fb8-ac33-de1203e3a3d9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Technical Skill 21', FALSE, NULL, NULL, '2dcfb228-9353-42e1-8db7-23f5d56226b4', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe27ab05-0c82-4869-a6e7-305222866ca0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 10', FALSE, NULL, NULL, '39d1e88e-3153-43a3-9a96-a36ff60e26f9', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe3f3912-92aa-4b39-813f-becc5a45a864', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 1', FALSE, NULL, NULL, '8fc7f790-2963-4fbd-8aed-24e5c9bb3293', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe51860c-42ab-4d85-824c-723d5e309583', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Soft Skills Skill 17', FALSE, NULL, NULL, '75bd53cd-3c15-43d0-a592-074952cfeccf', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe5d62ab-812e-4753-99fe-69cbab3c29ad', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 4 for Technical Skill 1', FALSE, NULL, NULL, '40f75731-a006-4ad7-a410-f47a5b58bec5', 'Level 4', 4);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe6da37b-543a-46a1-9344-5f5b7ccb4be4', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 37', FALSE, NULL, NULL, 'c27e8f4f-1262-4bf4-86b2-36a8bc69e64d', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe7b0ca4-d260-492e-917c-a89768e61cf5', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Leadership Skill 47', FALSE, NULL, NULL, '9bc82073-4c0f-4977-aa7f-9127849e53db', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fe9aa6f9-b1a8-4881-984e-37bd364fe887', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Domain Specific Skill 38', FALSE, NULL, NULL, '522da0af-7a97-4b04-91ba-16a3f80d95cb', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('fea2c5d2-ec80-467d-b111-93369481c72d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Soft Skills Skill 9', FALSE, NULL, NULL, 'f9dde99f-3afc-4322-ba9a-a757c4e8c72f', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ff02b4e0-7d2f-4855-83f2-a4b63fc58117', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Leadership Skill 6', FALSE, NULL, NULL, '41b4d323-0cd6-4d71-8aff-67f59d9c87d7', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ff4b9fad-dfaf-4e88-983c-d5880b1aec8d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Business Skill 50', FALSE, NULL, NULL, '7e960356-fef4-4fbb-841a-14c1e59c3aea', 'Level 5', 5);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ff720829-f931-450e-bb75-f1585df11029', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 2 for Business Skill 15', FALSE, NULL, NULL, '63696841-0700-4255-8f9a-189150ea218a', 'Level 2', 2);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ff982cc1-3a53-4303-ab44-e6981731423c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 1 for Technical Skill 20', FALSE, NULL, NULL, '51f65c58-c42e-4c52-a251-3d80d6277e68', 'Level 1', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ffe1fd20-1a7e-4a65-bb0c-9345674faa69', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Proficiency level 5 for Technical Skill 42', FALSE, NULL, NULL, 'cb253826-8103-45fd-9fce-b569426684f1', 'Level 5', 5);

INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('01426703-dd71-41d5-b517-202db233b4d3', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 28', FALSE, NULL, NULL, 'Domain Specific Skill 28');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0480319f-144f-4758-ad4d-22091680a0bb', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 2', FALSE, NULL, NULL, 'Domain Specific Skill 2');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('04d24b4d-ff8f-4ffd-9c64-eb6fc3da0ad5', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 41', FALSE, NULL, NULL, 'Business Skill 41');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('073adf96-2f2c-411f-a7c1-be7315c7852f', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 45', FALSE, NULL, NULL, 'Domain Specific Skill 45');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('078e7846-9132-492e-8ad9-7b3fcdcad8e7', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 25', FALSE, NULL, NULL, 'Business Skill 25');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('07f11bdd-82cf-4b79-ad78-70eb1e772e19', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 12', FALSE, NULL, NULL, 'Technical Skill 12');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('084c859d-23f8-4de3-b020-6399d2ed6623', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 8', FALSE, NULL, NULL, 'Soft Skills Skill 8');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('087eca69-033b-4542-aef1-8f2756ccc562', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 7', FALSE, NULL, NULL, 'Business Skill 7');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('08a22cea-ab17-4a3f-9645-23305553152c', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 12', FALSE, NULL, NULL, 'Leadership Skill 12');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0b325be3-52a3-4cbf-975f-fa1d8a319930', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 47', FALSE, NULL, NULL, 'Technical Skill 47');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('0f9c1416-57e3-411e-ae8e-26c87561b04f', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 40', FALSE, NULL, NULL, 'Domain Specific Skill 40');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('10c1293d-63e7-43ac-8933-7e7e611577a9', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 1', FALSE, NULL, NULL, 'Domain Specific Skill 1');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('11231ff7-f923-4385-a645-98e69b7d3009', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 33', FALSE, NULL, NULL, 'Soft Skills Skill 33');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('13632a78-c6d1-412e-9db1-341b735d14c8', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 11', FALSE, NULL, NULL, 'Soft Skills Skill 11');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('15cf0d09-b6ec-4dbd-963c-d749cf4592b5', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 31', FALSE, NULL, NULL, 'Soft Skills Skill 31');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('15ed9a34-d89e-4077-b39f-72f046248416', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 10', FALSE, NULL, NULL, 'Domain Specific Skill 10');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('16451824-4c2a-476c-92a8-8156a39159d7', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 48', FALSE, NULL, NULL, 'Domain Specific Skill 48');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('189d66ee-95e1-4672-b154-c19dccdcaf4c', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 2', FALSE, NULL, NULL, 'Soft Skills Skill 2');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('18adb648-5c95-4c45-a8a3-c5174c385215', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 28', FALSE, NULL, NULL, 'Technical Skill 28');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('199f28ba-4b05-42bd-b0d8-edda9cef4181', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 23', FALSE, NULL, NULL, 'Leadership Skill 23');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('1ae5447a-a829-4b5d-8214-1ea607837ad2', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 8', FALSE, NULL, NULL, 'Leadership Skill 8');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('1b829235-045c-4b9c-9ef0-c229e3b767c6', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 29', FALSE, NULL, NULL, 'Soft Skills Skill 29');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2198ff5d-ab6a-4156-9cb5-b1cc4604961a', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 35', FALSE, NULL, NULL, 'Soft Skills Skill 35');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2207d2ae-030d-41f9-ad95-ad7971dcd667', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 42', FALSE, NULL, NULL, 'Soft Skills Skill 42');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('222196d7-63e8-4964-aa0f-5c07d62358f9', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 45', FALSE, NULL, NULL, 'Business Skill 45');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2460eaac-85ef-4f0a-91ae-c6d088e46ba5', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 49', FALSE, NULL, NULL, 'Soft Skills Skill 49');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2addc64a-91fd-452b-954f-1d008efaba24', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 37', FALSE, NULL, NULL, 'Leadership Skill 37');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2c3a07d4-ff45-4787-a3d9-477b093ece59', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 46', FALSE, NULL, NULL, 'Leadership Skill 46');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2ca9ccb4-6657-4f9d-953f-37dbd0b23402', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 14', FALSE, NULL, NULL, 'Domain Specific Skill 14');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('2e8f6250-399c-44ca-a5fc-2088467d94e6', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 19', FALSE, NULL, NULL, 'Technical Skill 19');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('31038fa4-f668-48cb-80b5-e4837589d9d0', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 10', FALSE, NULL, NULL, 'Soft Skills Skill 10');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3118955a-a030-4604-a96c-24ec0841a554', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 21', FALSE, NULL, NULL, 'Soft Skills Skill 21');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('31676cb5-e435-49f5-b0ea-99db874aa3fa', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 6', FALSE, NULL, NULL, 'Business Skill 6');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('33014551-ed26-4b9d-a068-5a24441a1257', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 45', FALSE, NULL, NULL, 'Leadership Skill 45');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('33966360-1497-4ebf-acda-3901d4b17f86', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 7', FALSE, NULL, NULL, 'Leadership Skill 7');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3482deb4-2bc8-412b-94fb-5abfacbc5981', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 44', FALSE, NULL, NULL, 'Business Skill 44');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('366273ad-aa76-45f7-bf92-7ebcb48f81ef', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 46', FALSE, NULL, NULL, 'Business Skill 46');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('37a831dc-750c-4a58-a2a1-94d27e8475ec', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 29', FALSE, NULL, NULL, 'Leadership Skill 29');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3a657a4b-d193-4802-9ca3-8b891306135f', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 31', FALSE, NULL, NULL, 'Technical Skill 31');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3a72e89c-2a4f-4b9c-8c4e-98ced72c63c1', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 13', FALSE, NULL, NULL, 'Soft Skills Skill 13');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3ca60071-690b-4715-a8be-ce8365c543aa', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 5', FALSE, NULL, NULL, 'Business Skill 5');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3cf0ddb2-e89c-49c8-aaa3-8a8430f53d46', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 28', FALSE, NULL, NULL, 'Business Skill 28');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3d3a14a6-af31-4fc8-a5fa-cdf717554dcb', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 3', FALSE, NULL, NULL, 'Soft Skills Skill 3');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3d54ec61-4dac-45b3-8a4f-b652c12b4ccf', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 43', FALSE, NULL, NULL, 'Domain Specific Skill 43');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3dbfa887-5377-4d42-8db0-f15897fc0c5b', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 34', FALSE, NULL, NULL, 'Domain Specific Skill 34');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('3e521558-a629-4c49-a7d6-eae8b92dfed2', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 40', FALSE, NULL, NULL, 'Soft Skills Skill 40');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('41ce7dad-cedb-4bc8-87d9-a288f41c65d9', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 40', FALSE, NULL, NULL, 'Leadership Skill 40');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('438786bf-c42b-48c1-a46c-bb88c826a2fd', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 16', FALSE, NULL, NULL, 'Soft Skills Skill 16');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('44056474-f785-437e-bb87-d9b15f5c2f76', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 18', FALSE, NULL, NULL, 'Leadership Skill 18');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('452b356f-ceab-4117-affb-995b94dbcde5', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 16', FALSE, NULL, NULL, 'Leadership Skill 16');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4589bead-ecd6-487b-a107-fe967eab8947', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 27', FALSE, NULL, NULL, 'Leadership Skill 27');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('45b7f58a-5755-4a0a-a3ea-7b5b44d2af30', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 1', FALSE, NULL, NULL, 'Technical Skill 1');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('45f8c7e9-319d-45d6-88a3-5f1a794960c2', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 33', FALSE, NULL, NULL, 'Business Skill 33');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('46500da6-173f-4001-89b6-2a779c981cb5', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 17', FALSE, NULL, NULL, 'Leadership Skill 17');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4708401d-fd87-43e2-92ed-c25f43fafe79', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 19', FALSE, NULL, NULL, 'Domain Specific Skill 19');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('471da0f7-20b6-4252-bec6-bbb1ea753ec1', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 1', FALSE, NULL, NULL, 'Business Skill 1');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4bbe4c31-2a39-46ac-a0c7-71d613882d3f', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 36', FALSE, NULL, NULL, 'Soft Skills Skill 36');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4bbeab6f-743d-4956-bf68-f6bf6c10b88a', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 42', FALSE, NULL, NULL, 'Technical Skill 42');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4c4d4bae-b6e4-4f82-a73c-bb7ae5fbe044', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 24', FALSE, NULL, NULL, 'Domain Specific Skill 24');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4d3638c6-ed23-4add-aeeb-dd46db2eb2a2', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 6', FALSE, NULL, NULL, 'Leadership Skill 6');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('4e54f3e9-a1bf-474d-bbe1-39e0fb7b2a66', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 11', FALSE, NULL, NULL, 'Domain Specific Skill 11');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('508f72e7-139d-4c70-92f2-a45dbda19b02', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 9', FALSE, NULL, NULL, 'Domain Specific Skill 9');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('50c4dde1-46ff-48ca-91ea-96ca7e626a74', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 23', FALSE, NULL, NULL, 'Soft Skills Skill 23');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('50f50553-aa71-4858-965c-a8b6afeab563', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 14', FALSE, NULL, NULL, 'Business Skill 14');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('51440e0e-4a8a-4790-abae-7304ac5fb5b9', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 12', FALSE, NULL, NULL, 'Soft Skills Skill 12');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5280d7d2-4dc4-4bdd-82df-5c5af8ac3068', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 30', FALSE, NULL, NULL, 'Business Skill 30');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('54a2b825-24bf-47b6-9a87-b4a70be89249', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 6', FALSE, NULL, NULL, 'Technical Skill 6');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('54fdb0b9-74ab-47ad-8d09-402296f486a8', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 4', FALSE, NULL, NULL, 'Business Skill 4');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('57ae9786-84a5-4f11-9da3-59201a4de4e3', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 17', FALSE, NULL, NULL, 'Business Skill 17');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('58a690f0-5363-4a56-a966-7b6ad4e4aba6', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 19', FALSE, NULL, NULL, 'Business Skill 19');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5990d357-7a8e-4754-9564-e2c71d669ef4', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 38', FALSE, NULL, NULL, 'Soft Skills Skill 38');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5a32d12a-1728-4229-8f48-4a81c04d1d8d', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 4', FALSE, NULL, NULL, 'Technical Skill 4');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5a488ab6-7fa3-4687-911a-12afcdb88425', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 4', FALSE, NULL, NULL, 'Domain Specific Skill 4');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5b712b90-f547-4016-9e2b-3bcd214343a2', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 17', FALSE, NULL, NULL, 'Soft Skills Skill 17');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5c16efc1-d7e1-472c-86e0-3303dd0594c3', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 11', FALSE, NULL, NULL, 'Technical Skill 11');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5d6b961f-d97b-4f30-82ea-0de84a5af51f', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 31', FALSE, NULL, NULL, 'Business Skill 31');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('5f824139-2042-41b6-90aa-8ccc1758c6f1', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 43', FALSE, NULL, NULL, 'Soft Skills Skill 43');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6202f908-29d5-48cf-ae5c-9df1eda29cc0', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 29', FALSE, NULL, NULL, 'Business Skill 29');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('62f3ee69-6ada-4855-9ec6-b119c0852f7d', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 21', FALSE, NULL, NULL, 'Domain Specific Skill 21');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6414ae87-9517-487b-8297-2111e277d380', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 29', FALSE, NULL, NULL, 'Domain Specific Skill 29');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('676e09d4-86e6-4622-bf2f-555381076007', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 50', FALSE, NULL, NULL, 'Business Skill 50');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('68405083-d697-4f70-af59-734c3071fda2', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 44', FALSE, NULL, NULL, 'Technical Skill 44');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6a64dbb8-9db4-4e1c-b26a-7ae9c93d9997', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 50', FALSE, NULL, NULL, 'Soft Skills Skill 50');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6dae9db9-e2fc-44f6-8bc6-a32a66f1fce4', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 7', FALSE, NULL, NULL, 'Technical Skill 7');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6e5989d4-70fb-419d-b5a7-83e29ad4a872', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 18', FALSE, NULL, NULL, 'Soft Skills Skill 18');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6e7d4a78-2c75-40ad-a382-a46c782cade6', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 20', FALSE, NULL, NULL, 'Leadership Skill 20');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('6f9e6271-e464-495a-8252-a4431d36a76b', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 48', FALSE, NULL, NULL, 'Business Skill 48');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('70a203ff-68ff-4db4-90c7-750a44efc2a2', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 32', FALSE, NULL, NULL, 'Leadership Skill 32');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('73414c71-bd31-4dab-9f94-47763208a07e', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 24', FALSE, NULL, NULL, 'Business Skill 24');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('740634b6-4d62-46dd-b88a-f35200f50c9a', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 31', FALSE, NULL, NULL, 'Domain Specific Skill 31');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7664acfd-88be-4859-baa6-ea04152d4e20', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 12', FALSE, NULL, NULL, 'Domain Specific Skill 12');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('76cb5b0b-69cb-4237-a9ae-d9e4952a1be3', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 46', FALSE, NULL, NULL, 'Domain Specific Skill 46');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('76d1e7b2-b658-4fd7-b46a-0c68b81498b2', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 50', FALSE, NULL, NULL, 'Domain Specific Skill 50');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('793c4cc5-3ec3-4e15-94b8-f049dce9d327', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 13', FALSE, NULL, NULL, 'Business Skill 13');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7b15c244-4e7e-42b6-8326-7fdbf708b31a', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 36', FALSE, NULL, NULL, 'Leadership Skill 36');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7b3e6c90-055a-44cb-a003-b55d896094c3', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 37', FALSE, NULL, NULL, 'Technical Skill 37');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7b5ae153-59ef-4562-b0df-e4a10b3dec2f', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 27', FALSE, NULL, NULL, 'Business Skill 27');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7b791163-a53e-4d7a-babb-e2bddfdaf692', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 33', FALSE, NULL, NULL, 'Domain Specific Skill 33');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7b87b959-cec9-404a-848e-2b9acf38571a', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 23', FALSE, NULL, NULL, 'Domain Specific Skill 23');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7caf0db7-29b1-42e9-a726-a7a3863aa5cb', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 15', FALSE, NULL, NULL, 'Business Skill 15');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7ea92cca-3fe5-4bec-9857-18479be905e1', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 7', FALSE, NULL, NULL, 'Domain Specific Skill 7');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('7f7d6d8d-85e2-4400-924b-00f7d7934a95', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 17', FALSE, NULL, NULL, 'Domain Specific Skill 17');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8004ed80-e62d-4259-88f8-556c032cc181', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 16', FALSE, NULL, NULL, 'Business Skill 16');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('802b2c99-fc39-4fde-98d0-8561783b74ba', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 11', FALSE, NULL, NULL, 'Leadership Skill 11');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('806e9a38-7990-4091-bda8-324dbde81b94', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 49', FALSE, NULL, NULL, 'Business Skill 49');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('809612ce-897e-48c8-ad9e-0f58850d103c', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 13', FALSE, NULL, NULL, 'Technical Skill 13');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('816b93da-80b5-459b-8848-8b23405d96c3', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 16', FALSE, NULL, NULL, 'Domain Specific Skill 16');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('81cb0539-1c7c-491c-827a-3e2de0b3cd35', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 19', FALSE, NULL, NULL, 'Soft Skills Skill 19');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('82e11b3e-78f5-4242-8195-63b5cca630e4', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 10', FALSE, NULL, NULL, 'Leadership Skill 10');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8586f929-18e5-459d-866f-8ffea84b48cf', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 46', FALSE, NULL, NULL, 'Soft Skills Skill 46');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('85ed05bf-6ce5-4f2b-9c9c-d4ca27318b0b', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 36', FALSE, NULL, NULL, 'Domain Specific Skill 36');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('86138021-86a7-4915-be3d-60b4193ad508', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 35', FALSE, NULL, NULL, 'Leadership Skill 35');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8806bd3c-c0a0-4289-aac5-5e7133b2b2ba', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 15', FALSE, NULL, NULL, 'Technical Skill 15');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8818add4-2bd8-4b09-bb29-cc8ab5b7c217', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 34', FALSE, NULL, NULL, 'Soft Skills Skill 34');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('88744bad-dd62-4cb3-8319-c4afd4f2445d', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 9', FALSE, NULL, NULL, 'Soft Skills Skill 9');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8968a8d0-f56a-4b83-afef-d6fc8f58a490', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 15', FALSE, NULL, NULL, 'Leadership Skill 15');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8a8f449a-cf86-450d-b174-89c68e5b8a06', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 27', FALSE, NULL, NULL, 'Technical Skill 27');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8b56c4d8-6d6d-416e-9e22-756e089c7e45', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 38', FALSE, NULL, NULL, 'Technical Skill 38');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8bd5d6ab-5996-4e3f-9d6d-6e7f29eef478', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 30', FALSE, NULL, NULL, 'Domain Specific Skill 30');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8c2a6692-f907-4f08-a966-778aaec4672f', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 12', FALSE, NULL, NULL, 'Business Skill 12');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8cd60915-4e12-49db-a398-3eca1560aba8', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 34', FALSE, NULL, NULL, 'Business Skill 34');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8d807614-f511-487e-8fbc-d3ec97d199b0', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 35', FALSE, NULL, NULL, 'Technical Skill 35');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8f53851b-32be-463c-a03f-664af7028c6d', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 41', FALSE, NULL, NULL, 'Domain Specific Skill 41');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('8fd1ee68-7eed-416b-8986-ab8fd5662b98', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 27', FALSE, NULL, NULL, 'Soft Skills Skill 27');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('92826156-8f42-40eb-ab5e-4b33ade20935', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 8', FALSE, NULL, NULL, 'Domain Specific Skill 8');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9489283f-b487-462b-9b64-62f016d27ddd', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 5', FALSE, NULL, NULL, 'Soft Skills Skill 5');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('94b5515f-de82-4ec5-9603-f8aca4c5e7ac', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 39', FALSE, NULL, NULL, 'Domain Specific Skill 39');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('951dbf4e-7fbc-449b-b325-a37d7fb0a432', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 2', FALSE, NULL, NULL, 'Business Skill 2');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('98447ab3-380f-4e3e-a2e7-44de67fa4561', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 20', FALSE, NULL, NULL, 'Technical Skill 20');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('99140710-3c2b-4c73-83c5-0b362cc7157e', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 39', FALSE, NULL, NULL, 'Business Skill 39');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9a493d25-a0c8-43d3-9f40-2bccc0fd8ebb', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 18', FALSE, NULL, NULL, 'Domain Specific Skill 18');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9b5fea71-e386-456d-8dbc-91124a79f537', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 3', FALSE, NULL, NULL, 'Domain Specific Skill 3');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9bd49749-e8fd-4ec4-97e4-1d76b7643c17', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 25', FALSE, NULL, NULL, 'Leadership Skill 25');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9c3bdf7e-bcd6-4d9d-bd4c-2b99164c4181', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 17', FALSE, NULL, NULL, 'Technical Skill 17');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9ca43073-9c91-4ee2-9876-0c1733a10c70', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 6', FALSE, NULL, NULL, 'Soft Skills Skill 6');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9ccbd137-d0c7-451d-ac9a-609dca027bae', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 42', FALSE, NULL, NULL, 'Domain Specific Skill 42');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9e7174fd-7b2b-46be-9fa4-dfb4fd0d74e5', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 26', FALSE, NULL, NULL, 'Soft Skills Skill 26');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('9ec9cc42-aa85-4b06-b206-bbf362d7046d', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 16', FALSE, NULL, NULL, 'Technical Skill 16');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a0c09f21-5976-4198-ac56-97ae22b39d29', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 33', FALSE, NULL, NULL, 'Leadership Skill 33');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a0f3a33a-049f-4c7c-b39a-8d224e68870c', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 5', FALSE, NULL, NULL, 'Technical Skill 5');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a178967c-108f-47cd-bb5a-d8ed220c56e6', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 10', FALSE, NULL, NULL, 'Business Skill 10');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a2de3c16-302a-4e93-869e-9bd490dc9e28', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 24', FALSE, NULL, NULL, 'Soft Skills Skill 24');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a327cb91-389c-4cec-ab0d-eb17750fb26c', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 25', FALSE, NULL, NULL, 'Technical Skill 25');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a3906163-8e3e-47e4-957e-62d6c6de8ea5', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 14', FALSE, NULL, NULL, 'Soft Skills Skill 14');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a5088b6b-63da-4cd1-827e-fc4664ead959', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 14', FALSE, NULL, NULL, 'Technical Skill 14');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a5e1a29d-d9f3-407f-874b-b82c845e74a4', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 31', FALSE, NULL, NULL, 'Leadership Skill 31');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a69e4c0c-5229-4c5a-b9a4-b81f78c6dbec', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 39', FALSE, NULL, NULL, 'Leadership Skill 39');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a6eaad16-8d8a-4e9c-8457-72fa1fe5ba78', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 41', FALSE, NULL, NULL, 'Soft Skills Skill 41');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a88a468e-a637-4dca-a486-48d95af2b8e9', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 20', FALSE, NULL, NULL, 'Soft Skills Skill 20');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a93804db-12ad-49cd-801e-9f827318645d', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 28', FALSE, NULL, NULL, 'Soft Skills Skill 28');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a9dc5348-3c35-4e44-9454-48a1696d699b', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 18', FALSE, NULL, NULL, 'Business Skill 18');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('a9ddb620-cfb2-4a0d-a438-23cb41169a89', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 5', FALSE, NULL, NULL, 'Domain Specific Skill 5');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('aa69d732-a239-45ac-bfe8-26762f198f07', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 8', FALSE, NULL, NULL, 'Business Skill 8');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ab17bab1-fa7c-4463-a7f0-022b2aec5b19', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 49', FALSE, NULL, NULL, 'Domain Specific Skill 49');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('abb08051-488e-4fe8-8c42-b27df4ec3ea0', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 37', FALSE, NULL, NULL, 'Soft Skills Skill 37');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('af1caccc-3a45-43ef-9baa-264ccadac4e1', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 43', FALSE, NULL, NULL, 'Business Skill 43');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b103a70b-c6ce-4c84-9b8d-30c55bb3eb45', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 3', FALSE, NULL, NULL, 'Business Skill 3');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b1dda24e-ca29-4419-be17-7436b57a163b', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 40', FALSE, NULL, NULL, 'Business Skill 40');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b264307f-5b2e-4b0b-bebd-0c400d4bd800', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 47', FALSE, NULL, NULL, 'Domain Specific Skill 47');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b2fc18a0-ac89-42fe-b02f-2190474c3763', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 45', FALSE, NULL, NULL, 'Soft Skills Skill 45');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b2fd3c97-2dbf-4272-8b0c-b26a20d08bf9', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 22', FALSE, NULL, NULL, 'Business Skill 22');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b45e84be-6d39-47d5-9fa4-e541c811e35e', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 18', FALSE, NULL, NULL, 'Technical Skill 18');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b4956d59-0d21-455b-934d-d57db2021a85', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 13', FALSE, NULL, NULL, 'Domain Specific Skill 13');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b5056c43-4b6b-4b0f-8fd0-b6969b529e91', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 41', FALSE, NULL, NULL, 'Technical Skill 41');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b5ba53bc-848f-46b9-b02e-31abaf2171fc', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 43', FALSE, NULL, NULL, 'Leadership Skill 43');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b61d3274-f842-4350-b929-fe213599e37d', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 10', FALSE, NULL, NULL, 'Technical Skill 10');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b6dc6ae6-eaa5-4aa6-b0f2-fc670768b539', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 9', FALSE, NULL, NULL, 'Leadership Skill 9');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b7313228-9de7-422c-84aa-9c84f6b86c44', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 26', FALSE, NULL, NULL, 'Technical Skill 26');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b8773be5-0e83-41a8-bb87-9997ad066c2a', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 38', FALSE, NULL, NULL, 'Leadership Skill 38');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b8ef1522-d2ce-4405-a2ae-08d3eec7e8d5', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 13', FALSE, NULL, NULL, 'Leadership Skill 13');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('b9320bdc-01b9-4cd7-b69f-7d0ce01a9459', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 47', FALSE, NULL, NULL, 'Soft Skills Skill 47');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ba1bb156-0f9b-441a-97ff-3c10242662a7', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 47', FALSE, NULL, NULL, 'Leadership Skill 47');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bb78318c-d682-4b71-90ff-e249fa249c15', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 24', FALSE, NULL, NULL, 'Leadership Skill 24');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bc1f291d-a4e5-4942-8432-00959ea12f30', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 22', FALSE, NULL, NULL, 'Soft Skills Skill 22');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bd1dbd39-bd6f-4710-8be9-333e22280365', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 25', FALSE, NULL, NULL, 'Soft Skills Skill 25');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bdd3ddf7-f528-4dbb-8520-f809406c357b', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 48', FALSE, NULL, NULL, 'Soft Skills Skill 48');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('be14a278-ac72-496f-a4bf-714b4a349c59', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 50', FALSE, NULL, NULL, 'Leadership Skill 50');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('bf8d741c-b228-4003-918e-edfac1fc1a51', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 1', FALSE, NULL, NULL, 'Leadership Skill 1');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c075f17f-7581-40e8-863e-0e325e8bcfb9', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 27', FALSE, NULL, NULL, 'Domain Specific Skill 27');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c446da3f-12b7-46fe-ae78-8f9efe1b4092', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 1', FALSE, NULL, NULL, 'Soft Skills Skill 1');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c497f9fe-67c7-450e-9b50-ea15de3e1823', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 26', FALSE, NULL, NULL, 'Domain Specific Skill 26');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c6ccf8a5-c7f5-48aa-8499-f2d2cd181d85', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 20', FALSE, NULL, NULL, 'Domain Specific Skill 20');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c848c571-201a-4c4c-bf5b-7203c138682b', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 6', FALSE, NULL, NULL, 'Domain Specific Skill 6');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c86d31b4-f56d-4855-80d0-2ad94f7a6e60', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 48', FALSE, NULL, NULL, 'Leadership Skill 48');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c924683b-8439-405d-a5b0-cf20ce19d823', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 42', FALSE, NULL, NULL, 'Leadership Skill 42');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c949cfee-ba35-4261-affa-4964c7e30c63', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 28', FALSE, NULL, NULL, 'Leadership Skill 28');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('c94e6d70-ca4b-4f74-8b1d-10e8c905e5d5', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 32', FALSE, NULL, NULL, 'Business Skill 32');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ca8e025c-d56b-42ab-a1ce-a6ad55040968', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 4', FALSE, NULL, NULL, 'Leadership Skill 4');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cb01742e-f009-425f-8271-9aae54c03e14', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 4', FALSE, NULL, NULL, 'Soft Skills Skill 4');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cbf95d7f-f7a0-4d7a-a139-c8b5bb15d7be', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 49', FALSE, NULL, NULL, 'Technical Skill 49');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cc27519e-f443-41ce-ad45-3efc5b06cbc9', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 35', FALSE, NULL, NULL, 'Domain Specific Skill 35');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cd4028c2-9a3e-47b2-a754-84a9404fb7e7', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 11', FALSE, NULL, NULL, 'Business Skill 11');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cd58cb63-483e-499c-92cb-a3dc6c5c07d8', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 47', FALSE, NULL, NULL, 'Business Skill 47');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cdb8189a-a44a-4e94-84fe-d3c7b3c46bad', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 22', FALSE, NULL, NULL, 'Leadership Skill 22');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('cf54ad11-15f8-4580-9d99-5b466326099f', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 8', FALSE, NULL, NULL, 'Technical Skill 8');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d1c46beb-5957-4513-b3c6-a1893d23f5ae', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 23', FALSE, NULL, NULL, 'Business Skill 23');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d2749b85-d56d-47f4-8a8a-67c58779d011', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 37', FALSE, NULL, NULL, 'Domain Specific Skill 37');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d35cf831-e785-4284-a585-c8d31dcf7c62', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 43', FALSE, NULL, NULL, 'Technical Skill 43');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d409b398-4a17-49d7-8d7b-2e9bce19c223', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 39', FALSE, NULL, NULL, 'Technical Skill 39');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d472bbb2-6b33-4bfd-a232-253a047a20eb', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 50', FALSE, NULL, NULL, 'Technical Skill 50');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d5f4af46-8e55-4b0b-9fde-7eedc6e96830', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 34', FALSE, NULL, NULL, 'Technical Skill 34');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d661974a-2255-4005-a26f-c8ccf9635642', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 33', FALSE, NULL, NULL, 'Technical Skill 33');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d76fc1a3-272d-44d3-bb21-7754e1aa59c5', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 44', FALSE, NULL, NULL, 'Domain Specific Skill 44');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('d8d5d6d8-4ce4-41c1-a74b-e60f5c13c92d', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 25', FALSE, NULL, NULL, 'Domain Specific Skill 25');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('da2ab076-6e51-44b2-9e05-3a3d17d22cc5', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 45', FALSE, NULL, NULL, 'Technical Skill 45');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('dafc8927-5fea-489b-93eb-e59136f1a9aa', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 21', FALSE, NULL, NULL, 'Business Skill 21');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('db911e02-2de8-4dbf-b4d9-828bbe7c3231', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 21', FALSE, NULL, NULL, 'Technical Skill 21');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('db957e1d-a56f-4dd0-985e-6381e42f31f4', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 3', FALSE, NULL, NULL, 'Leadership Skill 3');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('de3228fc-fc58-4056-befc-6854ecaff3ed', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 37', FALSE, NULL, NULL, 'Business Skill 37');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('df2f0548-8688-40da-b7f4-2cb5c8388b67', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 19', FALSE, NULL, NULL, 'Leadership Skill 19');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('dfe1733f-3fae-4ae3-b19e-e9d2f0dde955', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 24', FALSE, NULL, NULL, 'Technical Skill 24');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e041fb45-151c-4c27-9566-d68be9f54a3a', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 30', FALSE, NULL, NULL, 'Leadership Skill 30');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e06a33a1-efe9-441f-9fc8-1c987612be17', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 36', FALSE, NULL, NULL, 'Technical Skill 36');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e1a186a5-da03-4dc3-bdf1-4d35b6c6b216', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 29', FALSE, NULL, NULL, 'Technical Skill 29');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e23c53aa-e150-4d9c-af25-304ba1aae9fd', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 23', FALSE, NULL, NULL, 'Technical Skill 23');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e31b2288-baf8-448e-b903-0d054ede2557', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 14', FALSE, NULL, NULL, 'Leadership Skill 14');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e3f10109-66ba-4743-ad47-920a6b12f70a', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 40', FALSE, NULL, NULL, 'Technical Skill 40');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e4501511-ca58-4c85-8bc7-b40cdac114d6', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 15', FALSE, NULL, NULL, 'Soft Skills Skill 15');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e46ff188-5e89-40f2-b682-e5662f80bb29', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 36', FALSE, NULL, NULL, 'Business Skill 36');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e4a06314-1429-4dca-84cb-ad6a9d01334f', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 7', FALSE, NULL, NULL, 'Soft Skills Skill 7');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e4f4498c-4447-4e4b-81b4-91b5b61c6ead', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 30', FALSE, NULL, NULL, 'Soft Skills Skill 30');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e5925927-c512-46aa-959e-ede0f47d1c01', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 20', FALSE, NULL, NULL, 'Business Skill 20');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e670155b-c6f4-494a-852d-bebb574e2c3c', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 32', FALSE, NULL, NULL, 'Technical Skill 32');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e670fa26-ad92-4c13-872e-4378505669e8', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 35', FALSE, NULL, NULL, 'Business Skill 35');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e789acdd-33d4-4ca1-a3e7-2ec40a9c11e9', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 22', FALSE, NULL, NULL, 'Technical Skill 22');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e83554af-3876-47da-ae9c-0243880c2d64', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 2', FALSE, NULL, NULL, 'Technical Skill 2');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e89d642d-0856-41cc-a002-c63c1db81ade', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 32', FALSE, NULL, NULL, 'Domain Specific Skill 32');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e94f5e0c-71c3-44e7-8f71-46a8bda9127b', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 32', FALSE, NULL, NULL, 'Soft Skills Skill 32');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('e98585d9-b20c-4cc8-8fee-2ed3cd583add', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 9', FALSE, NULL, NULL, 'Business Skill 9');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('eafcc265-677e-4e23-aad9-9c30906b13e4', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 38', FALSE, NULL, NULL, 'Domain Specific Skill 38');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('eb622da3-3d61-4d41-9492-661c3a9dd402', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 46', FALSE, NULL, NULL, 'Technical Skill 46');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('edb11f08-429c-41aa-94dd-c287bbbdf43f', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 26', FALSE, NULL, NULL, 'Business Skill 26');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f0b47c1b-6c65-47e5-934a-8779b490983d', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 49', FALSE, NULL, NULL, 'Leadership Skill 49');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f15ea936-1de9-450d-91f5-864ab4152d00', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 39', FALSE, NULL, NULL, 'Soft Skills Skill 39');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f280e4b5-68ce-4881-b30a-22ad19c64af4', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 38', FALSE, NULL, NULL, 'Business Skill 38');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f39beba5-2041-44ed-8ef9-f6e9a75eedc7', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 15', FALSE, NULL, NULL, 'Domain Specific Skill 15');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f3e55d61-2b0b-42de-9745-05a4afb768fe', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 30', FALSE, NULL, NULL, 'Technical Skill 30');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f40d9baf-cc0a-4909-90c3-b3ad7406d5ef', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 48', FALSE, NULL, NULL, 'Technical Skill 48');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f56d911e-fe4a-4680-ab4f-dac0a38b9d2d', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 41', FALSE, NULL, NULL, 'Leadership Skill 41');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f8b42584-6a14-42fa-9d7c-a7974c6d47ab', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 3', FALSE, NULL, NULL, 'Technical Skill 3');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f92fe8f0-e504-4f34-be1b-9e712ee58b7e', 'ccc11111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for technical skill 9', FALSE, NULL, NULL, 'Technical Skill 9');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('f94104cb-d258-4a14-81b6-a2cf71a4b92d', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 2', FALSE, NULL, NULL, 'Leadership Skill 2');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fa2093f7-aa3e-4c3c-80fd-e83a532e4a8b', 'ccc44444-4444-4444-4444-444444444444', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for soft skills skill 44', FALSE, NULL, NULL, 'Soft Skills Skill 44');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fac64ea6-ed1d-48ea-b2e0-62f87c39016c', 'ccc55555-5555-5555-5555-555555555555', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for domain specific skill 22', FALSE, NULL, NULL, 'Domain Specific Skill 22');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fade8277-9144-4c72-92f1-ccfeebe66d6d', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 26', FALSE, NULL, NULL, 'Leadership Skill 26');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fb6b2488-ddb5-4033-8862-7272930fdf1b', 'ccc33333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for business skill 42', FALSE, NULL, NULL, 'Business Skill 42');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fc912949-1052-4793-9071-88b107da437b', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 5', FALSE, NULL, NULL, 'Leadership Skill 5');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fd1de271-50f8-471b-a41b-a422c344b835', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 21', FALSE, NULL, NULL, 'Leadership Skill 21');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('fe37026d-cb05-42e9-8635-2df8cdbcb44a', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 44', FALSE, NULL, NULL, 'Leadership Skill 44');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
VALUES ('ffac0ff5-a4df-48b2-a0a7-70f1c44291b4', 'ccc22222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Description for leadership skill 34', FALSE, NULL, NULL, 'Leadership Skill 34');

INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('02e02fac-6036-4f2e-9eaf-f563d75ce580', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 065', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user065');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('0d813b29-c7a3-469c-a853-f1aaf6bde8ba', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 019', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user019');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('0df58091-c532-4bc2-97fe-6da1189c6875', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 041', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user041');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('0e45b14e-2770-4c8f-8351-2c93ea161a7a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 005', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user005');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('11111111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'System Administrator', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'admin');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('12d963b1-2be6-40ff-9caa-cd6103a3b62a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 003', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user003');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('1557418c-61a9-4e05-8bcd-681fd384422f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 021', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user021');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('15685b3c-e4b0-4a1a-93bb-64e4cbcac683', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 082', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user082');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('15faa43b-04d4-4259-b388-76c18e66eee2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 025', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user025');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('160d7ee3-17f8-4bf3-b0d7-3c0512fc2eed', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 089', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user089');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('16821ec1-84d1-4860-9e22-22f795795ff0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 046', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user046');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('16d432ea-d339-4691-933c-c7f07c50c0f7', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 040', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user040');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('19feb9fe-fb17-4b6b-a58c-a4bd5c3f1a6d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 083', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user083');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('1d719f7a-74d8-4eee-a1e8-7c5dcc067a54', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 060', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user060');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('1e9f7361-8fab-4b59-ae28-8438f6536ec8', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 097', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user097');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('2001b2e4-2331-4800-9f14-9064eb35e27d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 024', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user024');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('20465cd4-0b7d-4bd4-a4c9-d2ba1615cae1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 071', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user071');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('21002ac6-db14-4fc3-8600-f1db4d33bf16', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 009', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user009');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('2212f75b-daec-4a15-9864-435bdcce865f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 054', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user054');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('26f4062d-69b7-41cb-88da-578d31856b14', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 076', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user076');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('29b51c4f-1a6a-4bd4-a942-28a0d503c108', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 031', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user031');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('2c8d1616-e7f8-4137-8c6e-97150f98dd56', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 098', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user098');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('30dd6e8f-10b0-4d27-bb69-0f3e263c7048', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 008', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user008');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('33295b16-87be-411b-b6a1-5d4d3e31cb92', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 093', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user093');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('3800ddbb-1ae6-4d3a-8472-d4da54789980', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 074', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user074');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('3ae3bf60-2a2f-472b-b171-07ed1d936673', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 088', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user088');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('3bba4dd3-0eb5-475d-988b-31df07027952', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 004', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user004');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('463a93bb-0a81-4821-bf07-c255d2878afe', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 052', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user052');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('49df7672-e74a-4884-bfb5-9bc71d2419fd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 007', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user007');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('4b83f9c5-7d17-4657-b31a-1eb5cc18e148', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 029', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user029');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('4be1501e-4b2d-40e9-8fd9-ccc6ee5c6a89', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 096', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user096');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('541e8c3d-4628-40c1-9bf1-cf9767bad12d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 081', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user081');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('55aebb77-4e0e-4c0e-b69d-1040745d26e1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 079', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user079');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('5bf37912-3a39-43da-a9a5-ca336d5c9df6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 085', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user085');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('5ec0ffd2-f1f9-4719-b6e7-e2df003be098', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 006', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user006');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('64fa8b46-05d9-44ae-982c-41fffd6715d9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 064', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user064');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('672239db-2969-41ed-881a-79f57d7a37de', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 067', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user067');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('673122f4-99fb-4788-8eb7-d459c2f94333', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 036', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user036');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('6a2a3959-63cf-4684-8b18-1399832d2dd2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 023', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user023');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('6bbe9232-44da-4a5d-9d3e-4ccee556356d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 090', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user090');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('6c9c4b63-5d4d-4cbd-855b-6d99c021e544', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 070', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user070');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('6e71a733-4781-4c10-ad2b-2d1e42231f76', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 013', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user013');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('701139eb-77ae-4692-9ba2-e821a1293ac9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 053', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user053');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('783f2e37-662c-44e3-8bf6-d313d82616f0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 091', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user091');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('7ac2311b-af93-4e64-9d6f-59dd0b102c1e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 016', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user016');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('7d2f8092-199e-46c4-9a74-eceeb570b148', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 061', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user061');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('7d4cd292-fe36-4084-9105-cb7d22b11aa2', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 026', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user026');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('8134db26-0810-401d-a840-9f5448be942e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 099', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user099');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('83f205df-197b-4cd9-b8b2-ecfdbe599644', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 056', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user056');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('86847851-db9f-4c75-a7c1-60c903b9151c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 051', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user051');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('87e394ad-ad5f-4f4d-bb42-05a699967b78', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 086', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user086');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('8854759e-da02-49e6-986e-9d690bd74ed9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 001', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user001');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('8e97440c-9716-4016-aa59-993c581a5b47', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 014', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user014');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('9156c860-7316-48c7-bcce-e2243cd33543', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 027', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user027');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('928bc95e-bdda-44f8-8c13-efc801e6ebfd', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 012', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user012');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('92c5f0dc-6cee-4fef-b883-8aae907b5793', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 018', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user018');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('96b10407-7ef8-42ee-8a3a-566d72fcb2d9', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 058', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user058');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('96d68d65-48f4-4356-b755-b75a2cb0be35', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 075', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user075');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('9724e241-8ffd-420b-9d60-6101ac1b2d8e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 084', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user084');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('9a15ddc6-e1f9-44b1-84a5-c01319561677', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 037', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user037');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('9ddaf881-de6b-4efa-8733-06d6fb0e4729', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 035', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user035');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('a1191674-43e4-4069-be51-577f75052ed0', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 039', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user039');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('a20e71dc-1a62-4a67-ae35-3c3d5572e047', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 095', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user095');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('a2af2ad5-3b1f-4fe6-871f-2e6bb46dc45b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 069', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user069');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('a69e5a43-303c-480a-b205-ee05140920d1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 057', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user057');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('a80b6cd9-5a6b-4f3e-9462-b33706fe3ae1', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 059', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user059');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('aa3461a5-37cc-4eb7-802c-10b61a836442', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 002', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user002');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('aa640152-761a-4fd6-b8a5-c61825da1b09', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 073', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user073');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('ab3b439c-b84f-43d4-be8c-f465f3b67b2b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 033', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user033');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('b30aa9b5-d0aa-4300-8e90-c0d6f67166aa', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 047', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user047');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('b5b0275d-e0f0-4cc4-860d-c23fe16f155f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 043', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user043');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('b972007d-d908-489b-94e1-1b318d43a523', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 092', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user092');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('b9fcd542-3b9d-4638-9b05-bd762923855b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 080', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user080');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('ba8e9616-c7b9-4611-bf9c-29e745b1e337', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 032', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user032');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('bb058b20-ba7c-45be-adbc-4850f09d327b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 068', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user068');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('bcb9528f-2b64-4111-8290-e2e7fed824ba', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 015', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user015');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('bf8b4b63-b025-418e-8118-a82b442a678a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 010', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user010');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('c11316df-8867-446b-ba85-e5e8ee2fbb3f', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 066', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user066');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('c9f284ec-8800-4cfe-b93f-23f20e71229d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 020', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user020');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('ca918cf9-0e6c-49a9-8332-d0793bc655b6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 017', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user017');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('cdbb4de1-b93a-42f4-b7a1-5abfbd710bb6', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 045', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user045');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('ce7abbcd-dc71-46fc-b34c-93c60dd3d15a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 030', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user030');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('ceab72bb-de67-48ed-a931-3de0f3e0868b', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 063', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user063');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('cfeebeac-6a72-4b91-93d5-cb0c45ff8b70', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 044', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user044');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('d05c8fcc-4bbe-43b8-8f9e-e76f212b822d', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 078', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user078');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('d1f707eb-1668-4032-8689-42cd8558b60c', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 011', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user011');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('d575afe5-358a-41c4-8dc5-59339a252949', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 062', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user062');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('d8ecc096-71f1-494d-a0a0-05de16488edb', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 049', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user049');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('d9f753c8-1ab3-464b-95f1-149d00f88416', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 034', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user034');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('dc7c2290-c062-4450-a926-0e1889f4a002', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 072', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user072');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('de5c4ec9-a9c8-4caa-b971-db5866466050', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 050', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user050');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('df4f693c-6ef7-48e4-92d2-a4979687dc1e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 087', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user087');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('e00df4c7-8754-4865-b174-4064ba91883a', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 094', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user094');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('e00fce9b-8269-4b37-956d-e4c5d5b4538e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 038', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user038');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('e06e27d0-f84f-4aef-917c-ad6fd2f64869', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 042', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user042');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('ea2d7a0e-2d6d-4fe6-b065-a6271dac1507', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 022', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user022');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('f1657afe-5715-4b7d-a192-41a5c60a7d86', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 048', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user048');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('f4ae8439-e647-444f-8a04-11d9dca36606', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 028', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user028');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('f6a61703-7ea8-43a9-b5be-9db9c0e82b7e', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 077', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user077');
INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('f7f409f7-d281-4776-a190-87f5d5b97a40', TIMESTAMPTZ '2025-01-01T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Employee 055', FALSE, NULL, NULL, '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6fMJyHnUeO', 'user055');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250919004001_SeedData', '9.0.0');

COMMIT;

