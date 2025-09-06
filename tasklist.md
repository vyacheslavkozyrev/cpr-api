# Tasklist — CPR

Progress (update after each iteration)
- Iteration: 2
- Status: Complete
- Notes: Iteration 2 completed — database & schema finalized: initial migration rewritten to produce snake_case audit columns, RenameAuditColumns migration removed, DbContext mappings updated, migrations applied to local Docker DB (`cpr`), and integration tests passed locally.

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

 - [ ] Iteration 3 — Authorization (JWT stub)
  - Goal: Implement application-level authorization only (JWT stub) and auth plumbing.
  - Acceptance criteria / tests:
    - Authentication scheme registered and configurable via environment variable (no secrets in files).
    - Unit tests for token validation (valid/invalid token scenarios).
    - Swagger Authorize button usable with the stub token.
  - Notes: Do not implement API endpoints in this iteration; endpoints come after DB tables are in place.
  - Deployable: Auth middleware and test token generator available for local and CI runs.

 - [ ] Iteration 4 — EF Core: Entity tables (core domain entities)
  - Goal: Implement the core entity tables in EF Core and migrations: `users`, `employees`, `goals`, `skills` (core domain entities).
  - Acceptance criteria / tests:
    - Migrations scaffolded and applied successfully in a local dev DB.
    - Basic seed data present for core entities to exercise integration tests later.
  - Deployable: migrations ready for staging.

 - [ ] Iteration 5 — EF Core: Relation tables
  - Goal: Implement relation and join tables in EF Core and migrations: `employee_to_skill`, `goal_tasks` (or `tasks`), `feedback_requests`, `feedback` and any necessary junction tables.
  - Acceptance criteria / tests:
    - Referential integrity enforced by migrations (foreign keys configured).
    - Migrations apply cleanly and seeded relation data present for integration tests.
  - Deployable: full schema (entities + relations) ready for API development.

 - [ ] Iteration 6 — Goals CRUD + tasks
  - Goal: Implement `POST /goals`, `GET /me/goals`, `PATCH /goals/{id}`, `DELETE /goals/{id}`, and `POST /goals/{id}/tasks`.
  - Acceptance criteria / tests:
    - API integration tests for create/read/update/delete and adding tasks.
    - Manual smoke: create a goal, add a task, mark progress.
  - Deployable: Backend APIs and DB migrations.

 - [ ] Iteration 7 — Skill taxonomy & self-assessments
  - Goal: Add `skills`, `skill_levels`, and `employee_to_skill` endpoints and data exposure via `GET /skills`.
  - Acceptance criteria / tests:
    - Seed skills available in a staging DB.
    - Integration test for submitting and reading a self-assessment.
  - Deployable: API and seeded data.

 - [ ] Iteration 8 — Feedback request flow
  - Goal: Implement `POST /feedback/request`, `feedback_requests` table, notifications stub, and recipient listing.
  - Acceptance criteria / tests:
    - Creating a feedback request inserts rows and returns recipients list.
    - Unit test for idempotency handling and due_date enforcement.
  - Deployable: API with notifications disabled or stubbed.

 - [ ] Iteration 9 — Feedback submit & visibility
  - Goal: Implement `POST /feedback`, visibility rules, and `GET /feedback/me`.
  - Acceptance criteria / tests:
    - Submit feedback tied to goal/project and retrieve it via `GET /feedback/me`.
    - Authorization tests for visibility (employee vs manager).
  - Deployable: API with backend rules enforced.

 - [ ] Iteration 10 — Manager features & reviews
  - Goal: Manager endpoints (`GET /team`, `/team/goals`, `POST /performance_reviews`) and review storage.
  - Acceptance criteria / tests:
    - Manager can view direct reports' goals and create a performance review.
    - Integration tests for manager RBAC.
  - Deployable: API with manager role enabled.

 - [ ] Iteration 11 — Promotions & director flows
  - Goal: Implement promotions listing and approve/decline endpoints with links to reviews/feedback.
  - Acceptance criteria / tests:
    - Director endpoints return promotions ready for review.
    - E2E test: submit promotion request -> director approves/declines.
  - Deployable: API and small UI or API client scripts.

 - [ ] Iteration 12 — Reporting & analytics
  - Goal: Add reporting endpoints (`/reports/feedback-summary`, `/analytics/skills-gap`) and background jobs for aggregation.
  - Acceptance criteria / tests:
    - Aggregation jobs run and produce expected summaries in staging.
    - API returns analytics objects matching sample assertions.
  - Deployable: Backend job runner and reporting APIs.

 - [ ] Iteration 13 — Monitoring, retention & governance
  - Goal: Add monitoring (metrics, health checks), retention/purge job, and enforce conventions (conventions.md checks in CI).
  - Acceptance criteria / tests:
    - Alerts configured in staging; retention job can be run manually.
    - CI verifies conventions (no checked-in configs, secrets usage).
  - Deployable: Observability and maintenance jobs.

Notes
- Keep iterations small; split further if a step becomes large.
- Prefer automated tests and a green CI before deploying to staging/production.
- Use environment variables for configuration as per `conventions.md`.
