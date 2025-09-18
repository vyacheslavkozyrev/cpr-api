# Tasklist — CPR

Progress (update after each iteration)
- Iteration: 11
- Status: Complete
- Notes: Iteration 11 completed (2025-09-18) — Feedback submit & read fully implemented with POST /api/feedback and GET /api/me/feedback. Includes project association, streamlined payload, optimized response format, proper visibility rules, comprehensive validation, and full test coverage (45/45 unit tests, 48/48 integration tests, 5/5 contract tests passing).

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

- [ ] Iteration 12 — Manager views
  - Scope: GET /team, GET /team/members/{employee_id}, GET /team/goals
  - Acceptance: Manager RBAC tests and integration smoke.

- [ ] Iteration 13 — Performance reviews
  - Scope: POST /performance_reviews, GET /performance_reviews/{employee_id}
  - Acceptance: Persist reviews and link to goals/feedback; integration test for create/read.

- [ ] Iteration 14 — Promotions (director)
  - Scope: GET /promotions, GET /promotions/{id}, POST /promotions/{id}/approve|decline
  - Acceptance: Director flows test (read request -> approve/decline transitions).

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
