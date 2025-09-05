# Tasklist — CPR

Progress (update after each iteration)
- Iteration: 1
- Status: Complete
- Notes: Iteration 1 completed — repo scaffold, CI, test projects, docker-compose, and initial migrations applied locally.

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

- [ ] Iteration 2 — Database & schema
  - Goal: Add PostgreSQL connection, create core tables (users, employees, goals) and migrations.
  - Acceptance criteria / tests:
    - Migrations apply successfully in a local dev DB.
    - Integration test covers creating a user and a goal via DB layer.
  - Deployable: Database migrations applied in staging.

- [ ] Iteration 3 — Authentication & users API
  - Goal: Implement auth (JWT/OIDC stub) and basic `GET /me` and `PATCH /me` endpoints.
  - Acceptance criteria / tests:
    - Authenticated request to `GET /me` returns the user profile.
    - Unit tests for token validation and profile update.
  - Deployable: Backend service with auth enabled.

- [ ] Iteration 4 — Goals CRUD + tasks
  - Goal: Implement `POST /goals`, `GET /me/goals`, `PATCH /goals/{id}`, `DELETE /goals/{id}`, and `POST /goals/{id}/tasks`.
  - Acceptance criteria / tests:
    - API integration tests for create/read/update/delete and adding tasks.
    - Manual smoke: create a goal, add a task, mark progress.
  - Deployable: Backend APIs and DB migrations.

- [ ] Iteration 5 — Skill taxonomy & self-assessments
  - Goal: Add `skills`, `skill_levels`, and `employee_to_skill` endpoints and data exposure via `GET /skills`.
  - Acceptance criteria / tests:
    - Seed skills available in a staging DB.
    - Integration test for submitting and reading a self-assessment.
  - Deployable: API and seeded data.

- [ ] Iteration 6 — Feedback request flow
  - Goal: Implement `POST /feedback/request`, `feedback_requests` table, notifications stub, and recipient listing.
  - Acceptance criteria / tests:
    - Creating a feedback request inserts rows and returns recipients list.
    - Unit test for idempotency handling and due_date enforcement.
  - Deployable: API with notifications disabled or stubbed.

- [ ] Iteration 7 — Feedback submit & visibility
  - Goal: Implement `POST /feedback`, visibility rules, and `GET /feedback/me`.
  - Acceptance criteria / tests:
    - Submit feedback tied to goal/project and retrieve it via `GET /feedback/me`.
    - Authorization tests for visibility (employee vs manager).
  - Deployable: API with backend rules enforced.

- [ ] Iteration 8 — Manager features & reviews
  - Goal: Manager endpoints (`GET /team`, `/team/goals`, `POST /performance_reviews`) and review storage.
  - Acceptance criteria / tests:
    - Manager can view direct reports' goals and create a performance review.
    - Integration tests for manager RBAC.
  - Deployable: API with manager role enabled.

- [ ] Iteration 9 — Promotions & director flows
  - Goal: Implement promotions listing and approve/decline endpoints with links to reviews/feedback.
  - Acceptance criteria / tests:
    - Director endpoints return promotions ready for review.
    - E2E test: submit promotion request -> director approves/declines.
  - Deployable: API and small UI or API client scripts.

- [ ] Iteration 10 — Reporting & analytics
  - Goal: Add reporting endpoints (`/reports/feedback-summary`, `/analytics/skills-gap`) and background jobs for aggregation.
  - Acceptance criteria / tests:
    - Aggregation jobs run and produce expected summaries in staging.
    - API returns analytics objects matching sample assertions.
  - Deployable: Backend job runner and reporting APIs.

- [ ] Iteration 11 — Monitoring, retention & governance
  - Goal: Add monitoring (metrics, health checks), retention/purge job, and enforce conventions (conventions.md checks in CI).
  - Acceptance criteria / tests:
    - Alerts configured in staging; retention job can be run manually.
    - CI verifies conventions (no checked-in configs, secrets usage).
  - Deployable: Observability and maintenance jobs.

Notes
- Keep iterations small; split further if a step becomes large.
- Prefer automated tests and a green CI before deploying to staging/production.
- Use environment variables for configuration as per `conventions.md`.
