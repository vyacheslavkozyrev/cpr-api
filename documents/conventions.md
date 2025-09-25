# Conventions — CPR

Purpose
- Define allowed actions, forbidden actions, and operational conventions for code, data, APIs and deployments.

Scope
- Applies to all contributors working in this repository and to services built from it.

Branching model
- Primary workflow: work on a `develop` branch and merge into `main` when an iteration is complete and verified.
- For single-developer workflows you may work locally on `develop` and push focused commits; merge `develop` -> `main` when the iteration is ready. If you prefer to work directly on `main`, keep commits small and self-contained and ensure CI passes before merging significant changes.


Allowed actions
- Contributors may create, read, update and soft-delete their own data (goals, tasks, self-assessments).
- Managers may act on behalf of direct reports for goal assignment, feedback requests, and performance reviews within RBAC policies.
- Admins may perform organization-level reporting and administrative tasks, with approvals for destructive actions.
- Developers may propose schema or API changes via pull requests and automated migrations; all changes require review and CI checks.

Forbidden actions
- Directly modify audit fields (created_at/created_by/modified_at/modified_by) except via migration scripts with approved justification.
- Bypass role-based access control (RBAC) or impersonate users in production.
- Export or publish personally identifiable information (PII) or raw identifiable feedback without explicit consent or approved legal/HR request.
- Attempt to re-identify anonymized/pseudonymized feedback without an approved business and legal process.
- Run destructive SQL (DROP/ALTER) on production databases outside of reviewed migrations and scheduled maintenance windows.

Data handling & privacy
- Use UUIDs for primary keys and store timestamps in UTC (timestamptz).
- Store personal data minimised and only when required for functionality.
- For anonymous feedback flows, persist only a pseudonymization token and redact any direct identifier fields in user-facing outputs.
- Use strong hashing for passwords (bcrypt or Argon2) and never store plaintext secrets in repository.
- Redact PII in logs and metrics; only store identifiers necessary for troubleshooting with access controls.

Database conventions
- Primary keys: UUID (gen_random_uuid()/uuid_generate_v4()).
- Audit columns: created_by, created_at, modified_by, modified_at, is_deleted, deleted_by, deleted_at.
- Soft-delete semantics: set is_deleted=true and set deleted_at; do not physically remove rows without an approved purge policy.
- Indexing: use GIN for jsonb, functional indexes for case-insensitive search (lower(col)), and partial indexes for active rows (WHERE is_deleted = false).

API & integration conventions
- Use clear HTTP status codes: 200 (OK), 201 (Created), 204 (No Content), 400 (Bad Request), 401 (Unauthorized), 403 (Forbidden), 404 (Not Found), 429 (Too Many Requests), 500 (Server Error).
- Pagination: use ?page & ?per_page; include total counts in responses when feasible.
- Timeframes and filters: support ?period=week|month|quarter|year and ?status=not_started|in_progress|achieved|archived consistently.
- Idempotency: POST that may be retried (e.g., create feedback_request) must support idempotency keys when called by clients.
- Rate limits: enforce per-user and per-IP limits on sensitive endpoints (feedback creation, requests).
- Validation: server-side input validation for all endpoints; return helpful error messages and codes.
- Error Format: Use RFC7807 Problem Details format for all error responses to ensure consistency across the API.
- Input Sanitization: Automatically sanitize user-generated content to prevent XSS attacks and malicious input.
- Model Validation: Use DataAnnotations and custom validation attributes for comprehensive request validation.
- Business Rule Validation: Implement custom validators for domain-specific rules (e.g., preventing self-feedback).

Security & secrets
- Store secrets in secured vaults (Azure Key Vault, or equivalent). Never commit secrets to VCS.
- Use OAuth2 / OpenID Connect for authentication and short-lived tokens for services.
- Enforce least privilege for service identities and RBAC roles.
- Do not use repository or checked-in config files. All runtime configuration should be provided via environment variables. Secrets must still be stored in secured vaults and referenced from environment variables or secure config services at deployment time.

Testing & CI/CD
- All changes require automated unit and integration tests where applicable.
- Migrations must be created and applied via CI pipelines; no manual schema edits in production.
- Code review: at least one approving review for non-trivial changes; security-sensitive changes require a security reviewer.

Retention & compliance
- Follow data retention policies defined by HR/legal; soft-deleted rows may be purged after an approved retention period.
- Maintain an audit trail for deletions and data exports; require approvals for export of identifiable data.

Operations & incident response
- Monitor services and set alerts for error rates, latency and data pipeline failures.
- In case of data breach or PII exposure, follow the incident response playbook and notify stakeholders per policy.

Governance
- Changes to conventions must be proposed via a PR and approved by product owner and engineering lead.
- Exceptions require documented justification and recorded approval.

Contact
- For questions about these conventions, contact the engineering lead or product owner listed in the repository README.
