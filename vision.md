# Vision — CPR (Continuous Performance Review)

## Project Vision (one-line)
Provide a small, pragmatic REST API (ASP.NET Core) that continuously collects feedback, tracks goals, and produces concise review summaries — simple, maintainable, low-friction.

## Tech stack
- Runtime: .NET 9 (fallback to .NET 8)
- Web framework: ASP.NET Core Web API
- Data access: EF Core + migrations
- Database: PostgreSQL (self-hosted container)
- Auth: minimal local accounts + JWT (optional OIDC later)
- CI/CD: GitHub Actions
- Container registry: GHCR
- Deployment: Docker containers, docker-compose for local dev

## Development principles
- KISS & YAGNI (no premature optimization)
- Clean Architecture (dependency rule, testable boundaries)
- Small PRs, mandatory code review, maintainable code
- Tests: unit + integration + contract tests
- Automate build, tests, and image publish in CI

## Project structure (Clean Architecture, minimal)
- src/
  - CPR.Api — presentation: controllers, DTOs, middleware
  - CPR.Application — use-cases, interfaces (ports), DTOs
  - CPR.Domain — entities, value objects, domain rules
  - CPR.Infrastructure — EF Core, repositories, migrations, DI wiring
- tests/
  - CPR.UnitTests
  - CPR.IntegrationTests (Postgres via container)
  - CPR.ContractTests
- infra/ — docker-compose, local infra helpers
- ci/ — GitHub Actions workflows, templates

Notes
- Infrastructure depends on Application/Domain via interfaces only.
- Keep controllers thin; implement business rules in Application/Domain.
- EF Core and SQL details live solely in Infrastructure.

## Architecture (high-level)
- Single modular monolith (one deployable image) composed of layers:
  API -> Application -> Domain -> Infrastructure
- Local dev: docker-compose with app + postgres
- Correlation ID middleware for tracing across requests (in logs)

## Architecture (diagram)

```
+-----------------+     +------------------+
| External Client | --> | API (ASP.NET)    |
+-----------------+     +------------------+
                             |
                             v
                     +------------------+
                     | Application Layer|
                     | (use-cases)      |
                     +------------------+
                             |
                             v
                     +------------------+
                     | Domain Layer     |
                     +------------------+
                             |
                             v
                     +------------------+
                     | Infrastructure   |
                     | (EF Core, Repos) |
                     +------------------+
                             |
                             v
                     +------------------+
                     | PostgreSQL (DB)  |
                     +------------------+

CI/CD: GitHub Actions -> build image -> GHCR
Local dev: docker-compose (app + postgres)
```

## Configuration & secrets
- 12‑factor: env vars for configuration
- Secrets: GitHub Secrets for CI; local .env for dev; optional secret store in prod
- Minimal feature flags (if any) via env vars

## Logging approach
- Minimal logging: structured text to console (container)
- Include timestamp, level, correlation id, request path, user id (if present)
- No metrics or tracing initially (add later if required)

## Testing & quality gates
- Unit tests for domain and app logic
- Integration tests with real Postgres in CI (containerized)
- Contract tests to protect API surface
- CI pipeline: build -> tests -> publish image to GHCR

## Security & compliance
- TLS in production, input validation, basic rate limiting
- RBAC minimal, expand per requirements
- Data retention/export capability for GDPR compliance

## Minimal roadmap / acceptance criteria
1. Local dev bootstrapped (docker-compose: app + postgres)
2. Auth and user model + EF migrations
3. Feedback & goals CRUD with unit + integration tests
4. CI: build, test, publish image to GHCR
5. README with run & local dev instructions

## Non-goals (initial)
- No microservices; no Prometheus/Grafana; no managed cloud lock-in

---
This document is a contract for future development. Confirm to keep or request edits, then I will lock the file.
