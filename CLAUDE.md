# cpr-api — Backend Context

.NET 9 Web API following Clean Architecture. See root `CLAUDE.md` for project-wide rules (naming, RBAC, SDD phases).

## Solution Structure

```
src/
├── CPR.Api/             # Entry point — controllers, middleware, DI, auth, Swagger
│   ├── Auth/            # JwtStubAuthenticationHandler, RoleAuthorizationHandler, RoleAuthorizationPolicyProvider
│   ├── Controllers/     # DashboardController, EmployeesController, FeedbackController,
│   │                    #   FeedbackRequestController, FeedbackRequestManagerController,
│   │                    #   GoalsController, MeController, ProjectsController,
│   │                    #   TaxonomyController, TeamController
│   ├── Json/            # SnakeCaseNamingStrategy (Newtonsoft)
│   ├── Middleware/       # FeedbackRequestRateLimitMiddleware
│   ├── Services/        # IUserService / UserService (resolves current user from claims)
│   ├── Swagger/         # AuthorizeCheckOperationFilter, example providers
│   └── Program.cs       # App bootstrap — auth mode selection, DI, middleware pipeline
├── CPR.Application/     # Use-case layer — no framework dependencies
│   ├── DTOs/            # Request/response record types
│   ├── Services/        # Service interfaces (IGoalService, IFeedbackService, …)
│   └── Validators/      # FluentValidation validators
├── CPR.Domain/          # Pure domain — entities, repository interfaces, no EF references
│   ├── Entities/        # ~22 entity classes (User, Goal, Feedback, FeedbackRequest, …)
│   └── Repositories/    # IGoalsRepository, IFeedbackRepository, ITeamRepository, …
└── CPR.Infrastructure/  # EF Core, repository implementations, external services, jobs
    ├── Data/            # CprDbContext, DatabaseConnection, DatabaseSeeder
    ├── Jobs/            # FeedbackRequestReminderJob (Hangfire)
    ├── Migrations/      # EF Core migrations
    ├── Repositories/    # Concrete repository implementations
    └── Services/        # Concrete service implementations, InfrastructureRegistrar

tests/
├── CPR.UnitTests/       # 84 tests — xUnit, Moq, in-memory EF
├── CPR.IntegrationTests/# 100 tests — WebApplicationFactory against real PostgreSQL
├── CPR.ContractTests/   # 16 tests — Swagger schema + contract validation
└── CPR.SeedTestData/    # Shared test fixture helpers
```

## Architecture Rules

Clean Architecture dependency direction — **never violate layer boundaries**:

```
Domain ← Application ← Infrastructure
                ↑            ↑
              CPR.Api ───────┘
```

- **Domain**: no external dependencies; pure C# entities and repository interfaces.
- **Application**: depends only on Domain; holds service interfaces and DTOs.
- **Infrastructure**: implements Application interfaces; owns EF Core, Hangfire, MailKit.
- **Api**: depends on all layers; owns HTTP concerns (controllers, middleware, auth).

## C# Standards

- `<Nullable>enable</Nullable>` — nullable warnings treated as errors.
- Annotate every C# property with `[JsonPropertyName("snake_case_name")]`.
- Use `record` types for DTOs/request bodies.
- Register all new services/repositories in `InfrastructureRegistrar.cs`.
- XML doc comments (`///`) required on all public API surface (enables Swagger descriptions).

## Authentication

Controlled by `appsettings.json → Authentication:Mode`.

| Mode | Handler | Token format | When used |
|------|---------|-------------|-----------|
| `Stub` (default) | `JwtStubAuthenticationHandler` | `{userId}.{base64 HMAC-SHA256}` | Local dev & tests |
| `EntraExternalId` | JWT Bearer (Microsoft.Identity.Web) | Standard JWT from Entra | QA / Production |

**Generating a stub token** (dev/test):
```powershell
scripts/generate-token.ps1 -UserId <guid>
# Or: JWT_SIGNING_KEY env var must match the running API's key (default: "local-test-key")
```

**Pre-seeded test user IDs**:
- `679add6e-6c29-4e00-b6a5-b69c8e0f3445` — Administrator + SolutionOwner (John Doe)
- `977f4f1f-b3ce-4244-98fc-2c0d0248de88` — PeopleManager (Henry Wilson)
- `c7746e91-a5e8-4f8b-9f22-f48374ffa2a4` — Employee (Eve Adams)

**Authorization**: Use `[RequireRole("RoleName")]` on controllers/actions. The custom `RoleAuthorizationHandler` resolves roles from the database (not from JWT claims).

## API Standards

- **Error format**: RFC 7807 ProblemDetails — `{ type, title, status, detail }`.
  - Mapped in `Program.cs` via `Hellang.Middleware.ProblemDetails`.
  - Add new exception mappings in `builder.Services.AddProblemDetails(...)`.
- **Dates**: ISO 8601 UTC with milliseconds — `"2025-11-05T10:30:00.000Z"`.
- **JSON field names**: snake_case (enforced globally by `SnakeCaseNamingStrategy`).
- **Query params**: snake_case — `?page=1&per_page=20&sort_by=created_at`.
- **Pagination**: default 20, max 100; include total count in responses.
- **Rate limiting**: `FeedbackRequestRateLimitMiddleware` enforces 50 requests / 24 h per user.
- **Validation**: all inputs validated server-side via FluentValidation + DataAnnotations;
  `InputSanitizer` strips HTML/SQL injection from user-generated content.
- **HTTP status codes**: 200 OK, 201 Created, 204 No Content, 400 Bad Request,
  401 Unauthorized, 403 Forbidden, 404 Not Found, 429 Too Many Requests, 500 Server Error.

## Database Standards

- **Primary keys**: UUID only — `gen_random_uuid()` / `Guid.NewGuid()`.
- **Audit columns** (required on every table):
  `created_by`, `created_at`, `modified_by`, `modified_at`,
  `is_deleted`, `deleted_by`, `deleted_at`.
- **Soft delete only** — set `is_deleted = true`; never hard-delete user data.
- **Timestamps**: `timestamptz` (UTC); always store in UTC, never local time.
- **Indexes**: on all FKs and filter/sort columns; partial indexes use `WHERE is_deleted = FALSE`.
- **Migrations**: EF Core with Up/Down methods.
  Always create a migration for schema changes — never edit the DB directly.

## Build Commands

```bash
# Build
dotnet build src/CPR.sln

# Run (dev — uses Stub auth, port 5000)
dotnet run --project src/CPR.Api

# Migrations
dotnet ef migrations add <MigrationName> --project src/CPR.Infrastructure --startup-project src/CPR.Api
dotnet ef database update --project src/CPR.Infrastructure --startup-project src/CPR.Api

# All tests (requires Docker db_test running on port 5433)
scripts/run-tests.cmd

# Individual test suites
dotnet test tests/CPR.UnitTests/CPR.UnitTests.csproj
dotnet test tests/CPR.IntegrationTests/CPR.IntegrationTests.csproj
dotnet test tests/CPR.ContractTests/CPR.ContractTests.csproj
```

Local dev setup (Docker, run options, Swagger): see `README.md` Quick Start.
Hangfire dashboard (dev only): `http://localhost:5000/hangfire`.

## Testing

| Suite | Count | Approach |
|-------|-------|----------|
| Unit | 84 | xUnit + Moq; in-memory EF Core |
| Integration | 100 | `WebApplicationFactory`; real PostgreSQL on port 5433 |
| Contract | 16 | Swagger schema validation; existing endpoint contracts |

- Coverage target: **80% overall**; **100%** for auth, validation, and business-rule paths.
- Integration tests use `ASPNETCORE_ENVIRONMENT=Test` — Hangfire and DB seeding are skipped.
- New endpoints require at least one integration test covering the happy path and key error cases.

## Adding a New Feature

1. Add entity to `CPR.Domain/Entities/` and repository interface to `CPR.Domain/Repositories/`.
2. Add DTOs to `CPR.Application/DTOs/` and service interface to `CPR.Application/Services/`.
3. Add FluentValidation validator to `CPR.Application/Validators/`.
4. Implement repository in `CPR.Infrastructure/Repositories/` and service in `CPR.Infrastructure/Services/`.
5. Register both in `InfrastructureRegistrar.cs`.
6. Create an EF Core migration (`dotnet ef migrations add ...`).
7. Add controller in `CPR.Api/Controllers/` with `[RequireRole]` on protected actions.
8. Add unit tests (service logic) and integration tests (endpoint behaviour).

## Key Config Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Defaults (Stub auth, CORS origins) |
| `appsettings.Development.json` | Full dev config (EntraExternalId, DB, SMTP, logging) |
| `appsettings.Test.json` | Test environment overrides |
| `.env.dev` / `.env.test` | Docker Compose DB credentials |
| `docker/docker-compose.yml` | Dev DB (5432), test DB (5433), smtp4dev (1025/3333) |
| `scripts/generate-token.ps1` | Generate stub auth token for a given user ID |
| `scripts/run-tests.cmd` | Run all test suites sequentially |
