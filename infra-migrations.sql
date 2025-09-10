CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905211423_InitialCreate', '8.0.0');

COMMIT;

START TRANSACTION;

INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
VALUES ('11111111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', NULL, NULL, NULL, 'Jane Smith', FALSE, NULL, NULL, '$2b$12$.........................', 'jane.smith');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905211614_SeedUsers', '8.0.0');

COMMIT;

START TRANSACTION;

CREATE TABLE goals (
    id uuid NOT NULL,
    owner_id uuid NOT NULL,
    title text NOT NULL,
    description text,
    status text NOT NULL DEFAULT 'open',
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_goals" PRIMARY KEY (id)
);

INSERT INTO goals (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, owner_id, status, title)
VALUES ('22222222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', NULL, NULL, NULL, 'Add tests for critical services', FALSE, NULL, NULL, '11111111-1111-1111-1111-111111111111', 'open', 'Improve unit test coverage');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905214737_AddGoals', '8.0.0');

COMMIT;

START TRANSACTION;

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

INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
VALUES ('33333333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', NULL, NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Senior Software Engineer', '11111111-1111-1111-1111-111111111111');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905215334_AddEmployees', '8.0.0');

COMMIT;

START TRANSACTION;

CREATE TABLE audit_logs (
    id uuid NOT NULL,
    "ActorId" uuid,
    action text NOT NULL,
    "TargetType" text,
    "TargetId" uuid,
    "Detail" text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
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
    "Description" text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
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
    "Description" text,
    career_path_id uuid NOT NULL,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
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
    "Description" text,
    "ParentDepartmentId" uuid,
    "ManagerId" uuid,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_departments" PRIMARY KEY (id)
);

CREATE TABLE feedback (
    id uuid NOT NULL,
    goal_id uuid NOT NULL,
    "ProjectId" uuid,
    "FromEmployeeId" uuid NOT NULL,
    "ToEmployeeId" uuid NOT NULL,
    content text NOT NULL,
    "Rating" integer,
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
    "ProjectId" uuid,
    "GoalId" uuid,
    message text,
    "DueDate" timestamp with time zone,
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
    "Description" text,
    "Deadline" timestamp with time zone,
    "IsCompleted" boolean NOT NULL,
    "CompletedAt" timestamp with time zone,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_goal_tasks" PRIMARY KEY (id)
);

CREATE TABLE locations (
    id uuid NOT NULL,
    name text NOT NULL,
    "Address" text,
    "City" text,
    "Region" text,
    "Country" text,
    "PostalCode" text,
    "Timezone" text,
    "ContactPhone" text,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_locations" PRIMARY KEY (id)
);

CREATE TABLE positions (
    id uuid NOT NULL,
    title text NOT NULL,
    "Description" text,
    "Expectations" text,
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
    "Description" text,
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

CREATE TABLE projects (
    id uuid NOT NULL,
    code text NOT NULL,
    title text NOT NULL,
    "Description" text,
    "OwnerId" uuid,
    "SponsorId" uuid,
    created_by uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    modified_by uuid,
    modified_at timestamp with time zone,
    is_deleted boolean NOT NULL,
    deleted_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT "PK_projects" PRIMARY KEY (id)
);

CREATE TABLE skill_categories (
    id uuid NOT NULL,
    title text NOT NULL,
    "Description" text,
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
    "Description" text,
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

CREATE TABLE skills (
    id uuid NOT NULL,
    title text NOT NULL,
    "Description" text,
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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250909233636_AddEntities_v1', '8.0.0');

COMMIT;

START TRANSACTION;

INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering, architecture and platform roles', FALSE, NULL, NULL, 'Technology');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'HR, people operations and employee development', FALSE, NULL, NULL, 'People');
INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Financial planning, reporting and analysis', FALSE, NULL, NULL, 'Finance');

INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Software development and engineering roles', FALSE, NULL, NULL, 'Software Engineering');

INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, "ManagerId", modified_at, modified_by, name, "ParentDepartmentId")
VALUES ('99999999-9999-9999-9999-999999999901', 'ENG', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, NULL, 'Engineering', NULL);

INSERT INTO locations (id, "Address", "City", "ContactPhone", "Country", created_at, created_by, deleted_at, deleted_by, is_deleted, modified_at, modified_by, name, "PostalCode", "Region", "Timezone")
VALUES ('88888888-8888-8888-8888-888888888801', NULL, 'Remote', NULL, 'Global', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, FALSE, NULL, NULL, 'Headquarters', NULL, NULL, NULL);

INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, "Description", "Expectations", is_deleted, modified_at, modified_by, title)
VALUES ('cccccccc-cccc-cccc-cccc-cccccccc0001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 'Senior Software Engineer');

INSERT INTO project_roles (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, position_id, title)
VALUES ('66666666-6666-6666-6666-666666666601', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'cccccccc-cccc-cccc-cccc-cccccccc0001', 'Tech Lead');

INSERT INTO projects (id, code, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, "OwnerId", "SponsorId", title)
VALUES ('77777777-7777-7777-7777-777777777701', 'PRJ-001', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, NULL, NULL, 'Sample Project');

INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('dddddddd-dddd-dddd-dddd-dddddddddd01', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Technical skills and competencies', FALSE, NULL, NULL, 'Technical');
INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('dddddddd-dddd-dddd-dddd-dddddddddd02', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Leadership and communication skills', FALSE, NULL, NULL, 'Leadership');

INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ffffffff-ffff-ffff-ffff-ffffffff0001', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'Beginner', 1);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ffffffff-ffff-ffff-ffff-ffffffff0002', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'Intermediate', 3);
INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, skill_id, title, value)
VALUES ('ffffffff-ffff-ffff-ffff-ffffffff0003', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'Advanced', 5);

INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'dddddddd-dddd-dddd-dddd-dddddddddd01', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Writing unit and integration tests', FALSE, NULL, NULL, 'Unit Testing');
INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, "Description", is_deleted, modified_at, modified_by, title)
VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeee0002', 'dddddddd-dddd-dddd-dddd-dddddddddd02', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Verbal and written communication skills', FALSE, NULL, NULL, 'Communication');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250910002037_AddSeedData_v1', '8.0.0');

COMMIT;

