# Tasklist — CPR

Progress (update after each iteration)
- Iteration: 14
- Status: **Complete** ✅
- Notes: Iteration 14 Project Management (Solution Owner Role) — Fully implemented and tested (2025-10-09). Successfully implemented project management system with major database schema refactoring. Created 8 DTOs with validation, refactored project_roles and project_teams schema (ProjectRole now has project_id instead of position_id, ProjectTeam no longer has project_id), implemented IProjectService with 11 methods, created ProjectsController with 11 RESTful endpoints (read operations open to all authenticated users, write operations require Solution Owner role), enhanced seed data with 10 diverse projects and 100 project roles. **All tests passing: Unit: 22/22 (ProjectService), Integration: 52/52 (ProjectsController), Contract: 16/16 (11 Projects + 5 existing)**. Database successfully recreated with new schema. Note: Solution Owner role (not "Project Owner") is used for authorization. Created run-api-as-solution-owner.cmd script for testing. **Next**: Iteration 15 - Positions & position->skill mapping.

Database Enhancement (2025-09-18): Successfully implemented comprehensive seed data system with:
- Created SeedData.cs with hierarchical data generation methods
- 10 career paths, 100 career tracks, 50 positions, 5 skill categories, 250 skills, 1250 skill levels
- 10 departments with 100 employees and associated users
- Fixed DbContext design-time factory for migration generation
- Created clean database schema migration (CreateDatabaseSchema)
- Resolved GUID format validation issues for EF Core migrations
- All seed data methods ready for manual migration insertion when needed

Guidelines
- Each iteration is incremental: implement, test, and deploy the listed items.
- Keep each step small and verifiable (unit/integration test + simple manual smoke test).

Iterations

- [x] Iteration 1 — Repository & CI
  - Goal: Create repo scaffold, CI pipeline, and test runner using .NET 9 + EF Core (Postgres).
  - Acceptance criteria / tests:
    - GitHub Actions run: dotnet build, dotnet test, and lint (optional) on push to main.
    - README contains run/test instructions and Docker Compose usage.
  - Deployable: CI only (no runtime service). Include docker-compose.dev.yml with Postgres for local testing.

- [x] Iteration 2 — Database & schema
  - Goal: Add PostgreSQL connection, create core tables (users, employees, goals) and migrations.
  - Acceptance criteria / tests:
    - Migrations apply successfully in a local dev DB.
    - Integration test covers creating a user and a goal via DB layer.
  - Status: Complete
  - Notes / verification:
    - Initial and subsequent migrations updated to emit final snake_case audit columns (created_by, created_at, etc.).
    - Removed `RenameAuditColumns` migration files and cleaned project files.
    - Updated `CprDbContext` property-to-column mappings to match migrations.
    - Applied migrations to local Docker Postgres (container `cpr` / `cpr_db_1`) and confirmed tables exist with correct column names.
    - Ran integration tests against the local DB: 4/4 tests passed.
  - Deployable: Database migrations applied in staging.

- [x] Iteration 3 — Authorization (JWT stub)
  - Goal: Implement application-level authorization only (JWT stub) and auth plumbing.
  - Acceptance criteria / tests:
    - Authentication scheme registered and configurable via environment variable (no secrets in files).
    - Unit tests for token validation (valid/invalid token scenarios).
    - Swagger Authorize button usable with the stub token.
  - Status: Complete
  - Notes: Auth middleware, token generator, protected `/me` endpoint, tests, scripts, and CI guard implemented. Endpoints implemented as minimal protected surface (e.g., `/me`) for verification.
  - Deployable: Auth middleware and test token generator available for local and CI runs.

- [x] Iteration 4 — EF Core: Entity tables (core domain entities)
  - Goal: Implement the core entity tables in EF Core and migrations: `users`, `employees`, `goals`, `skills` (core domain entities).
  - Status: Complete (2025-09-10)
  - Acceptance criteria / tests:
    - Migrations scaffolded and applied successfully in a local dev DB.
    - Basic seed data present for core entities to exercise integration tests later.
  - Notes: Seed data inserted; integration tests executed against dev DB.
  - Deployable: migrations ready for staging.
  - [x] Iteration 4 — EF Core: Entity tables (all entity tables)
  - Goal: Implement all entity tables from `data.md` in EF Core and migrations. This iteration will create the canonical domain tables (not junction tables):
    - users, employees, audit_logs, career_paths, career_tracks, positions, skill_categories, skills, skill_levels, departments, locations, projects, project_roles, goals, goal_tasks, feedback, feedback_requests
  - Status: Complete (2025-09-10)
  - Acceptance criteria / tests:
    - One or more EF Core migrations added that create the listed tables with snake_case column names and constraints.
    - `dotnet ef database update` succeeds against the local Docker dev DB (docker-compose.dev.yml) without manual SQL edits.
    - Minimal seed data inserted (example: admin user, one employee, one goal, a couple of skills) so integration tests can run against real rows.
    - Unit tests for repository/service layer (happy path) for at least `users`, `employees`, and `goals`.
    - Integration tests that: create a user -> create an employee -> create a goal -> read goals for the employee.
  - Notes: Consolidated migration scaffolded and applied; dev DB dropped and recreated to apply new schema; integration tests passed. Work merged to `main`.
  - Implementation notes / plan:
    1. Add domain entity classes under `CPR.Domain/Entities` for each table above.
    2. Add `DbSet<>` properties and Fluent API mapping in `CprDbContext` (snake_case naming and audit columns) in `CPR.Infrastructure`.
    3. Scaffold and review EF migration(s) (e.g., `AddEntities_v1`). Keep migrations small and readable.
    4. Add a dev-only seeder (executed at startup in Development) to insert minimal seed rows for tests.
    5. Add repository and service methods for basic CRUD for `users`, `employees`, and `goals` and unit tests.
    6. Add integration tests (use Docker Postgres from `docker-compose.dev.yml`) that run migrations, seed, and exercise the API paths.
    7. Update CI to run migrations and tests against a disposable Postgres instance (service container or testcontainers).
  - Risk & mitigation:
    - Risk: Schema drift or long migrations. Mitigation: keep migration steps incremental and test locally against the Docker DB before pushing.
    - Risk: Flaky integration tests due to shared DB. Mitigation: use a fresh database per CI job or clear state between tests.
  - Deployable: Full schema for core entities ready for staging; relation/junction tables (e.g., `employee_to_skill`, `position_to_skill`, `project_teams`) can be added in Iteration 5 if needed.

- [x] Iteration 5 — EF Core: Relation tables
  - Goal: Implement relation and join tables in EF Core and migrations (remaining): `employee_to_skill`, `position_to_skill`, `project_teams`.
  - Acceptance criteria / tests:
    - Referential integrity enforced by migrations (foreign keys configured).
    - Migrations apply cleanly and seeded relation data present for integration tests.
  - Deployable: full schema (entities + relations) ready for API development.
  - Notes: Completed (2025-09-10) — relation entities, migrations, indexes added; dev DB recreated and integration tests passed. Changes merged to `main`.

 - [x] Iteration 6 — Goals CRUD
  - Scope: POST /goals, GET /me/goals, GET /goals/{id}, PATCH /goals/{id}, DELETE /goals/{id}
  - Acceptance: Integration tests for create/read/update/delete (happy path).

 - [x] Iteration 7 — Goal Tasks
  - Scope: POST /goals/{id}/tasks, GET /goals/{id}/tasks, PATCH /goals/{id}/tasks/{taskId}/complete
  - Acceptance: Integration test to add and complete a task.

 - [x] Iteration 8 — Skills taxonomy (read-only)
  - Scope: GET /career, GET /career_track?career_path_id=, GET /positions?career_track_id=
  - Acceptance: Seeded career paths, career tracks and positions visible in staging; integration tests read lists and filter by query params.

 - [x] Iteration 8.1 — Skills taxonomy (part 2)
  - Scope: GET /skills?position_id=, GET /skill_levels?skill_id=
  - Acceptance: Seeded skills and skill_levels visible in staging; integration tests verify listing and filtering by position_id and skill_id respectively.
  - Status: Complete (2025-09-16)
  - Notes: Implemented DTOs, service methods with LINQ joins, controller endpoints, seed data in migration, and integration tests. PositionToSkill relationships properly seeded and filtering working correctly.

- [x] Iteration 9 — Self-assessments
  - Scope: POST /me/skills, GET /me/skills, PUT /me/skills/{skillId}
  - Acceptance: Integration tests for creating, reading, and updating self-assessments with proper authentication and validation.
  - Status: Complete (2025-09-17)
  - Notes: Fully implemented self-assessment system with:
    - POST /me/skills - Create new skill assessment (with upsert logic for existing assessments)
    - GET /me/skills - List user's skill assessments with skill and level details
    - PUT /me/skills/{skillId} - Update existing skill assessment
    - JWT authentication and employee ID extraction from claims
    - PostgreSQL DateTime compatibility (UTC conversions)
    - Comprehensive error handling and validation
    - 35/35 integration tests passing, 31/31 unit tests passing
    - Repository/Service pattern with proper separation of concerns

- [x] Iteration 10 — Feedback requests
  - Scope: POST /feedback/request, GET /me/feedback/request, GET /me/feedback/request/todo
  - Acceptance: Creates feedback_request rows and returns requests sent/received by current user; unit test for validation.
  - Status: Complete (2025-09-17)
  - Notes: Fully implemented feedback request system with:
    - POST /feedback/request - Create new feedback request with validation and duplicate prevention
    - GET /me/feedback/request - List feedback requests sent by current user with recipient details
    - GET /me/feedback/request/todo - List feedback requests received by current user with sender details
    - JWT authentication with employee ID extraction from claims
    - Comprehensive validation (recipient exists, not self-request, duplicate prevention)
    - EF Core navigation properties for efficient queries (User.DisplayName)
    - DTO pattern with CreateFeedbackRequestDto and FeedbackRequestDto
    - Service layer with IFeedbackService and FeedbackService implementation
    - Repository/Service pattern with proper separation of concerns
    - Database schema with FeedbackRequest entity and navigation properties
    - Seed data for test users and employees in migrations
    - Comprehensive error handling and HTTP status codes
    - 40/40 integration tests passing (including 5 new feedback controller tests)
    - 31/31 unit tests passing with no regressions
    - Clean architecture principles maintained throughout implementation

- [x] Iteration 11 — Feedback submit & read
  - Scope: POST /api/feedback, GET /api/me/feedback
  - Acceptance: Submit feedback and retrieve it (visibility rules respected).
  - Status: Complete (2025-09-18)
  - Notes: Fully implemented feedback submission and retrieval system with:
    - POST /api/feedback - Submit feedback with project association, streamlined payload (removed fromEmployeeId, renamed toEmployeeId to employeeId), self-feedback prevention, comprehensive validation, and automatic content sanitization
    - GET /api/me/feedback - Retrieve feedback addressed to current user with optimized MyFeedbackDto (excludes redundant toEmployee information), proper visibility rules ensuring users only see feedback addressed to them
    - Enhanced validation and error handling with ProblemDetails format
    - JWT authentication with employee ID extraction from claims
    - EF Core navigation properties for efficient queries
    - Repository/Service pattern with proper separation of concerns
    - Database schema with Feedback entity and navigation properties
    - Automatic input sanitization to prevent XSS attacks
    - Comprehensive error handling and HTTP status codes
    - 45/45 unit tests passing, 48/48 integration tests passing, 5/5 contract tests passing
    - Clean architecture principles maintained throughout implementation
    - API documentation updated to reflect current structure

- [x] Iteration 12 — Manager views
  - Scope: GET /team, GET /team/members/{employee_id}, GET /team/goals
  - Acceptance: Manager RBAC tests and integration smoke.
  - Status: Complete (2025-09-30)
  - Notes: Fully implemented and tested team management system with comprehensive fixes:
    - Team endpoints: GET /api/team (team members), GET /api/team/members/{employeeId}, GET /api/team/goals
    - Manager authorization: Proper RBAC with IsManagerAsync checks (403 Forbidden for non-managers)
    - Data seeding: Fixed DatabaseSeeder.cs with proper manager-reporter relationships (Henry Wilson has 3 direct reports)
    - Test fixes: Updated integration tests to use valid seeded employee IDs and dynamic goal creation
    - Migration cleanup: Removed conflicting SeedData migration, keeping seeding in DatabaseSeeder.cs
    - User profile fixes: Updated /api/me endpoint to return proper userId, userName, and displayName from database
    - Full test coverage: All unit tests (31/31) and integration tests (48/48) passing
    - Authorization validation: Managers access team data (200 OK), employees get 403 Forbidden

- [x] Iteration 13 — Role-Based Access Control (RBAC) System
  - [x] Phase 1: Database Schema Changes ✅
    - Created Role and UserToRole entities with full audit columns
    - Updated CprDbContext with proper configurations and relationships
    - Modified CreateDatabaseSchema migration to include roles and user_to_role tables
    - Added foreign key constraints, unique indexes, and soft-delete filtering
  - [x] Phase 2: Domain Models & Entities ✅
    - Created IRoleRepository and RoleRepository implementations
    - Created IUserRoleRepository and UserRoleRepository implementations
    - Created IRoleService and RoleService implementations
    - Created RoleDtos.cs with DTOs for role operations
    - Registered all services and repositories in DI container
  - [x] Phase 4: Authorization Infrastructure ✅
    - Created RequireRoleAttribute extending AuthorizeAttribute for role-based authorization
    - Created RoleAuthorizationHandler implementing IAuthorizationHandler to process role requirements
    - Created RoleAuthorizationPolicyProvider to parse RequireRole policies
    - Registered authorization services in Program.cs DI container
    - Build verification: All components compile successfully with proper role checking logic
  - [x] Phase 5: Update Controllers ✅
    - Applied RequireRoleAttribute to GoalsController endpoints (Employee+ for most operations, Administrator for DELETE)
    - Applied RequireRoleAttribute to TeamController endpoints (People Manager+ for all operations)
    - Verified MeController and FeedbackController already have correct authorization (any authenticated user)
    - Confirmed TaxonomyController endpoints are public (no authentication required)
    - Updated endpoints.md documentation with actual role requirements implemented
    - Build verification: All controllers compile successfully with role attributes
    - Test verification: All existing tests pass with new authorization requirements
  - Scope: Implement comprehensive RBAC with roles table, user-to-role mapping, authorization attributes
  - Database: roles table (id, title, description, created_at, modified_at, created_by, modified_by, is_deleted, deleted_by, deleted_at), user_to_role junction table (id, user_id, role_id, created_at, modified_at, created_by, modified_by, is_deleted, deleted_by, deleted_at)
  - Authorization: Role-based attributes on all controllers
  - Roles: Employee, People Manager, Solution Owner, Director, Administrator
  - Acceptance: All endpoints properly secured with role-based access control; integration tests for role authorization.

- [x] Iteration 14 — Project Management (Solution Owner Role)
  - Goal: Implement project management APIs for Solution Owner role to manage projects, project roles, and employee assignments.
  - Status: **Complete** ✅ - Implementation and all tests passing (2025-10-09)
  - Scope:
    - **Solution Owner APIs (Solution Owner role required)**:
      - POST /api/projects — Create a new project ✅
      - PUT /api/projects/{id} — Edit project details ✅
      - DELETE /api/projects/{id} — Delete project (soft delete) ✅
      - POST /api/projects/{id}/roles — Create/define a project role ✅
      - PUT /api/projects/{id}/roles/{roleId} — Edit project role ✅
      - DELETE /api/projects/{id}/roles/{roleId} — Delete project role (soft delete) ✅
      - POST /api/projects/{id}/team — Assign employee to project with role ✅
      - DELETE /api/projects/{id}/team/{teamMemberId} — Remove employee from project ✅
    - **Public/Shared APIs (All authenticated users)**:
      - GET /api/projects — List all projects ✅
      - GET /api/projects/{id} — Get project details ✅
      - GET /api/projects/{id}/roles — Get project roles ✅
      - GET /api/projects/{id}/team — Get project team members ✅
  - Implementation:
    - **Phase 1: DTOs** ✅
      - Created 8 DTOs: ProjectDto, CreateProjectDto, UpdateProjectDto, ProjectRoleDto, CreateProjectRoleDto, UpdateProjectRoleDto, ProjectTeamDto, CreateProjectTeamDto
      - Added comprehensive validation attributes (Required, MaxLength, etc.)
      - Renamed ProjectRoleDto in TeamDtos.cs to TeamMemberProjectDto to avoid naming conflicts
    - **Phase 2: Database Schema Refactoring** ✅
      - **Major Schema Change**: Refactored project_roles and project_teams tables
      - Changed project_roles.position_id → project_id (required FK to projects)
      - Removed project_teams.project_id (now gets project through ProjectRole)
      - Fixed table creation ordering: projects → project_roles → project_teams
      - Fixed unique constraint: project_teams now uses (project_role_id, employee_id)
      - Updated CreateDatabaseSchema migration, Designer, and ModelSnapshot
      - Updated domain entities: ProjectRole.cs and ProjectTeam.cs
      - Updated all DTOs to reflect new schema
      - Updated CprDbContext configuration
    - **Phase 3: Seed Data** ✅
      - Enhanced DatabaseSeeder to create **10 diverse projects** (PRJ-001 through PRJ-010)
      - Created **100 project roles** (10 roles per project)
      - Role templates: Tech Lead, Product Manager, Software Engineer, QA Engineer, UX Designer, DevOps Engineer, Data Analyst, Business Analyst, Scrum Master, Security Engineer
      - Projects: Career Progression System, Customer Portal Redesign, Mobile App Development, Data Analytics Platform, Cloud Migration Initiative, API Gateway Implementation, E-Commerce Platform, DevOps Automation, Security Compliance Framework, AI/ML Research Platform
    - **Phase 4: Repository** ✅
      - Created IProjectRepository interface with 15 methods for CRUD operations
      - Implemented ProjectRepository with proper EF Core queries
      - Updated ProjectRepository queries for new schema relationships
      - Updated TeamService.GetEmployeeProjectsAsync() with proper joins
      - Registered IProjectRepository → ProjectRepository in DI container
    - **Phase 5: Services** ✅
      - Created IProjectService interface with 11 business methods
      - Implemented ProjectService with full CRUD logic for projects, roles, and team
      - Added validation (project exists, role belongs to project, etc.)
      - Implemented soft delete pattern (IsDeleted, DeletedBy, DeletedAt)
      - Added partial update support (only modifies provided fields)
      - Entity-to-DTO mapping helpers
      - Registered IProjectService → ProjectService in DI container
    - **Phase 6: Controllers** ✅
      - Created ProjectsController with 11 RESTful endpoints
      - Authorization: Read operations open to all authenticated users, write operations require "Solution Owner" role
      - Proper HTTP status codes (200 OK, 201 Created, 204 No Content, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found)
      - User context handling with IUserService.GetCurrentUserProfileAsync()
      - Error handling with try-catch for InvalidOperationException
      - XML documentation comments for Swagger generation
      - ProducesResponseType attributes for all endpoints
    - **Phase 7: Database Recreation** ✅
      - Dropped and recreated cpr_dev and cpr_test databases
      - Applied updated migrations successfully
      - Verified 10 projects and 100 project roles seeded correctly
      - Database running with new schema
    - **Phase 8: Testing** ✅
      - **Integration Tests (52 tests)**: Created ProjectsControllerIntegrationTests.cs with comprehensive tests for all 11 endpoints including authorization, validation, soft delete, and error scenarios. All passing ✅
      - **Unit Tests (22 tests)**: Created ProjectServiceTests.cs testing business logic including validation, entity-to-DTO mapping, soft delete behavior, partial updates, and null handling. All passing ✅
      - **Contract Tests (16 tests)**: Created ProjectsContractTests.cs with 11 new tests validating JSON schemas for all project management endpoints. Implemented CreateSolutionOwnerClientAsync() using dependency injection to ensure Solution Owner role after database initialization (avoiding constructor timing issues). All passing ✅
      - Test Pattern: Used DI-based async helper method for role assignment in test methods, not constructors
      - Created scripts/run-api-as-solution-owner.cmd for manual testing with Solution Owner role
  - Test Results:
    - **Unit Tests**: 22/22 passing ✅ (ProjectService business logic)
    - **Integration Tests**: 52/52 passing ✅ (all 11 ProjectsController endpoints)
    - **Contract Tests**: 16/16 passing ✅ (11 Projects + 5 existing)
    - **Total New Tests**: 90 tests created and passing ✅
    - **No Regressions**: All existing tests continue to pass ✅
  - Acceptance Criteria:
    - ✅ Solution Owner can create/edit/delete projects
    - ✅ Solution Owner can define project-specific roles
    - ✅ Solution Owner can assign/remove employees to projects with roles
    - ✅ All authenticated users can view projects list and details
    - ✅ Proper authorization with RequireRole("Solution Owner") for write operations
    - ✅ All existing tests pass with new schema
    - ✅ Database schema properly refactored and seeded
    - ✅ Integration tests for all 11 new endpoints (52 tests)
    - ✅ Unit tests for ProjectService business logic (22 tests)
    - ✅ Contract tests for new API endpoints (11 tests)
    - ✅ Swagger documentation with examples
  - Notes:
    - Major database schema refactoring completed successfully
    - ProjectRole now correctly references projects (not positions)
    - ProjectTeam simplified (removed redundant project_id)
    - Rich seed data with 10 diverse projects and 100 roles
    - Clean separation: read operations public, write operations require Solution Owner role
    - Test isolation solved using dependency injection pattern for database operations
    - Solution Owner role assignment handled dynamically in tests (not seeded by default)
    - All 90 new tests pass reliably when run individually or together

- [ ] Iteration 15 — Positions & position->skill mapping (admin)
  - Scope: CRUD /positions, POST /position_to_skill
  - Acceptance: Admin CRUD tests and mapping persisted.

- [ ] Iteration 16 — Reporting & analytics (basic)
  - Scope: GET /reports/feedback-summary, GET /analytics/skills-gap
  - Acceptance: Aggregation job or query and sample integration tests.

- [ ] Iteration 17 — System / integrations
  - Scope: POST /internal/import/users, POST /webhook/feedback
  - Acceptance: Import endpoints accept payload and create records; integration tests.

- [ ] Iteration 18 — Cross-cutting & monitoring
  - Scope: GET /health, GET /ready, GET /metrics, GET /audit_logs
  - Acceptance: Health checks, metrics endpoint and audit retrieval working in staging.

Notes
- Keep iterations small; split further if a step becomes large.
- Prefer automated tests and a green CI before deploying to staging/production.
- Use environment variables for configuration as per `conventions.md`.
