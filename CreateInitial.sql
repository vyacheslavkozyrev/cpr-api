CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    CREATE TABLE audit_logs (
        id uuid NOT NULL,
        actor_id uuid,
        action text NOT NULL,
        target_type text,
        target_id uuid,
        detail text,
        created_by uuid,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        modified_by uuid,
        modified_at timestamp with time zone,
        is_deleted boolean NOT NULL,
        deleted_by uuid,
        deleted_at timestamp with time zone,
        CONSTRAINT "PK_audit_logs" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    CREATE TABLE career_paths (
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
        CONSTRAINT "PK_career_paths" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    CREATE TABLE career_tracks (
        id uuid NOT NULL,
        title text NOT NULL,
        description text,
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    CREATE TABLE departments (
        id uuid NOT NULL,
        name text NOT NULL,
        code text,
        description text,
        parent_department_id uuid,
        manager_id uuid,
        created_by uuid,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        modified_by uuid,
        modified_at timestamp with time zone,
        is_deleted boolean NOT NULL,
        deleted_by uuid,
        deleted_at timestamp with time zone,
        CONSTRAINT "PK_departments" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        modified_by uuid,
        modified_at timestamp with time zone,
        is_deleted boolean NOT NULL,
        deleted_by uuid,
        deleted_at timestamp with time zone,
        CONSTRAINT "PK_employee_to_skill" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    CREATE TABLE goals (
        id uuid NOT NULL,
        -- primary owner reference moved from owner_id to employee_id
        employee_id uuid NOT NULL,
        related_skill_id uuid,
        related_skill_level_id uuid,
        title text NOT NULL,
        description text,
        status text NOT NULL DEFAULT 'open',
        deadline date,
        is_completed boolean NOT NULL DEFAULT FALSE,
        completed_at timestamp with time zone,
        progress_percent numeric(5,2) NOT NULL DEFAULT 0.00,
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Engineering, architecture and platform roles', FALSE, NULL, NULL, 'Technology');
    INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'HR, people operations and employee development', FALSE, NULL, NULL, 'People');
    INSERT INTO career_paths (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Financial planning, reporting and analysis', FALSE, NULL, NULL, 'Finance');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO career_tracks (id, career_path_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Software development and engineering roles', FALSE, NULL, NULL, 'Software Engineering');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO departments (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, manager_id, modified_at, modified_by, name, parent_department_id)
    VALUES ('99999999-9999-9999-9999-999999999901', 'ENG', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, NULL, 'Engineering', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO employees (id, created_at, created_by, deleted_at, deleted_by, department, is_deleted, manager_id, modified_at, modified_by, title, user_id)
    VALUES ('33333333-3333-3333-3333-333333333333', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', NULL, NULL, NULL, 'Engineering', FALSE, NULL, NULL, NULL, 'Senior Software Engineer', '11111111-1111-1111-1111-111111111111');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO goals (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, employee_id, status, title)
    VALUES ('22222222-2222-2222-2222-222222222222', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', NULL, NULL, NULL, 'Add tests for critical services', FALSE, NULL, NULL, '33333333-3333-3333-3333-333333333333', 'open', 'Improve unit test coverage');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO locations (id, address, city, contact_phone, country, created_at, created_by, deleted_at, deleted_by, is_deleted, modified_at, modified_by, name, postal_code, region, timezone)
    VALUES ('88888888-8888-8888-8888-888888888801', NULL, 'Remote', NULL, 'Global', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, FALSE, NULL, NULL, 'Headquarters', NULL, NULL, NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO positions (id, career_track_id, created_at, created_by, deleted_at, deleted_by, description, expectations, is_deleted, modified_at, modified_by, title)
    VALUES ('cccccccc-cccc-cccc-cccc-cccccccc0001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 'Senior Software Engineer');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO project_roles (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, position_id, title)
    VALUES ('66666666-6666-6666-6666-666666666601', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'cccccccc-cccc-cccc-cccc-cccccccc0001', 'Tech Lead');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO projects (id, code, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, owner_id, sponsor_id, title)
    VALUES ('77777777-7777-7777-7777-777777777701', 'PRJ-001', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, NULL, NULL, 'Sample Project');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('dddddddd-dddd-dddd-dddd-dddddddddd01', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Technical skills and competencies', FALSE, NULL, NULL, 'Technical');
    INSERT INTO skill_categories (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('dddddddd-dddd-dddd-dddd-dddddddddd02', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Leadership and communication skills', FALSE, NULL, NULL, 'Leadership');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
    VALUES ('ffffffff-ffff-ffff-ffff-ffffffff0001', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'Beginner', 1);
    INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
    VALUES ('ffffffff-ffff-ffff-ffff-ffffffff0002', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'Intermediate', 3);
    INSERT INTO skill_levels (id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, skill_id, title, value)
    VALUES ('ffffffff-ffff-ffff-ffff-ffffffff0003', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, NULL, FALSE, NULL, NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'Advanced', 5);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeee0001', 'dddddddd-dddd-dddd-dddd-dddddddddd01', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Writing unit and integration tests', FALSE, NULL, NULL, 'Unit Testing');
    INSERT INTO skills (id, category_id, created_at, created_by, deleted_at, deleted_by, description, is_deleted, modified_at, modified_by, title)
    VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeee0002', 'dddddddd-dddd-dddd-dddd-dddddddddd02', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', '11111111-1111-1111-1111-111111111111', NULL, NULL, 'Verbal and written communication skills', FALSE, NULL, NULL, 'Communication');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO users (id, created_at, created_by, deleted_at, deleted_by, display_name, is_deleted, modified_at, modified_by, password_hash, user_name)
    VALUES ('11111111-1111-1111-1111-111111111111', TIMESTAMPTZ '2025-09-05T00:00:00+00:00', NULL, NULL, NULL, 'Jane Smith', FALSE, NULL, NULL, '$2b$12$.........................', 'jane.smith');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250910193406_CreateInitial') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250910193406_CreateInitial', '8.0.0');
    END IF;
END $EF$;
COMMIT;

