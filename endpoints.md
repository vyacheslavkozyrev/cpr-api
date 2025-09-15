# API Endpoints — CPR (persona-grouped)

Common query params (used across endpoints):
- pagination: ?page=1&per_page=20
- filters: ?status=&department=&manager_id=&owner_id=
- period: ?period=week|month|quarter|year

## Personas
- Employee (self)
- Manager (team lead)
- HR / Admin
- Director
- System / Integration (service)

---

## Employee (self)
Profile / Users
- GET /me — return current user's profile (auth)
- PATCH /me — update own profile (auth)

Employees
- GET /me/employee — employee record for signed-in user
- GET /employees/{id} — read employee (manager/admin)
- GET /employees?department=&manager_id=&page=&size= — list / filters (manager/admin)

Goals
- POST /goals — create goal (auth)
- GET /me/goals — list my goals (auth) (filters: status, page, sort)
- GET /goals/{id} — read goal (owner/manager/admin)
- PATCH /goals/{id} — update goal (owner)
- DELETE /goals/{id} — soft-delete goal (owner/admin)
- POST /goals/{id}/tasks — add task to goal

### Contracts & purposes (Goals)

POST /goals
- Purpose: create a new goal owned by the authenticated user. The API will map the authenticated user's employee id as the owner unless `employeeId` is explicitly supplied and allowed.
- Request (JSON):
  - {
  -   "title": "string (required, 1..250)",
  -   "description": "string (optional, max 2000)",
  -   "deadline": "date (optional, ISO8601 date or datetime)",
  -   "relatedSkillId": "GUID (optional)",
  -   "relatedSkillLevelId": "GUID (optional)",
  -   "employeeId": "GUID (optional) - override owner; must be a valid GUID",
  -   "priority": "integer (optional, 0..100)",
  -   "visibility": "string (optional, one of: private|team|org)"
  - }
- Response 201 (JSON):
  - {
  -   "id": "GUID",
  -   "employeeId": "GUID",
  -   "title": "string",
  -   "description": "string|null",
  -   "status": "open",
  -   "deadline": "ISO8601|null",
  -   "priority": "integer|null",
  -   "visibility": "string|null",
  -   "createdAt": "ISO8601",
  -   "createdBy": "GUID"
  - }
- Errors: 400 validation (ProblemDetails with `errors` dictionary), 401 unauthorized.

Validation rules (server-side - DataAnnotations):
- title: required, string length 1..250
- description: max length 2000
- deadline: must be a parseable date/time (invalid format results in 400)
- relatedSkillId / relatedSkillLevelId: GUID if supplied
- employeeId: if supplied, must be a valid GUID (regex validated); otherwise the authenticated user's employee id is used
- priority: optional integer between 0 and 100 inclusive
- visibility: optional string; allowed values: "private", "team", "org"

Example request (JSON):
{
  "title": "Improve onboarding experience",
  "description": "Coordinate with product and design to reduce time to first value.",
  "deadline": "2025-12-01",
  "relatedSkillId": "11111111-2222-3333-4444-555555555555",
  "relatedSkillLevelId": "22222222-3333-4444-5555-666666666666",
  "priority": 50,
  "visibility": "team"
}

Example error (400 ProblemDetails) — validation errors live under `errors`:
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "EmployeeId": ["EmployeeId must be a valid GUID"],
    "Priority": ["Priority must be between 0 and 100"]
  }
}

GET /me/goals
- Purpose: return paged list of goals for the authenticated user.
- Query params: ?status=&page=&per_page=
- Response 200 (JSON):
  - {
  -   "items": [{ "id":"GUID","title":"string","status":"string","createdAt":"ISO8601" }],
  -   "total": 123,
  -   "page": 1,
  -   "per_page": 20
  - }

GET /goals/{id}
- Purpose: read a single goal with tasks and metadata. Access: owner, manager, admin.
- Response 200 (JSON):
  - {
  -   "id": "GUID",
  -   "ownerId": "GUID",
  -   "title": "string",
  -   "description": "string|null",
  -   "status": "open|in_progress|completed",
  -   "tasks": [{ "id":"GUID","title":"string","isCompleted":false }],
  -   "createdAt": "ISO8601",
  -   "updatedAt": "ISO8601"
  - }
- Errors: 401, 403, 404.

PATCH /goals/{id}
- Purpose: partial update (owner or manager). Accepts fields to change.
- Request (JSON): { "title": "string?", "description": "string?", "status": "open|in_progress|completed" }
- Response 200: updated Goal object (same shape as GET).
- Errors: 400, 401, 403, 404.

DELETE /goals/{id}
- Purpose: soft-delete or archive a goal (owner/admin). Implementation should mark as deleted or archived.
- Response: 204 No Content.
- Errors: 401, 403, 404.

POST /goals/{id}/tasks
- Purpose: add a task under a goal (owner or manager).
- Request (JSON):
  - { "title": "string (required)", "description": "string?", "deadline": "ISO8601?" }
- Response 201 (JSON): task object:
  - { "id":"GUID", "goalId":"GUID", "title":"string", "deadline":"ISO8601?", "isCompleted": false, "createdAt":"ISO8601" }
- Errors: 400, 401, 403, 404.

### Minimal DTO names (suggested, C#)
- CreateGoalDto { string Title; string? Description; DateTimeOffset? Deadline; Guid? RelatedSkillId; Guid? EmployeeId; int? Priority; string? Visibility }
- GoalDto { Guid Id; Guid EmployeeId; string Title; string? Description; string Status; DateTimeOffset CreatedAt; DateTimeOffset? UpdatedAt; List<TaskDto> Tasks }
- CreateGoalTaskDto { string Title; string? Description; DateTimeOffset? Deadline }
- TaskDto { Guid Id; Guid GoalId; string Title; string? Description; DateTimeOffset? Deadline; bool IsCompleted; DateTimeOffset? CompletedAt }

---

## Iteration 8 — Skills taxonomy (read-only)

Endpoints

- GET /career
  - Purpose: return list of career paths (id, title, description)
  - Response 200 (JSON): [ { "id": "GUID", "title": "string", "description": "string|null" } ]

- GET /career_track?career_path_id={guid}
  - Purpose: list career tracks; optional filter by career_path_id
  - Query params: career_path_id (GUID)
  - Response 200 (JSON): [ { "id": "GUID", "title": "string", "description": "string|null", "careerPathId": "GUID" } ]

- GET /positions?career_track_id={guid}
  - Purpose: list positions; optional filter by career_track_id
  - Query params: career_track_id (GUID)
  - Response 200 (JSON): [ { "id": "GUID", "title": "string", "description": "string|null", "expectations": "string|null", "careerTrackId": "GUID" } ]

Notes
- The `expectations` field is a free-form text field describing responsibilities and success expectations for the position.
- Responses are camelCased in JSON (e.g., `careerPathId`, `careerTrackId`, `expectations`).
- All endpoints are read-only and return 200 with an empty array when no rows matched.

Example response for GET /positions?career_track_id=cccccccc-cccc-cccc-cccc-cccccccc0001

[
  {
    "id": "33333333-3333-3333-3333-333333333333",
    "title": "Senior Software Engineer",
    "description": "Senior member of engineering team",
    "expectations": "Deliver high-quality code, mentor peers, drive architecture decisions",
    "careerTrackId": "cccccccc-cccc-cccc-cccc-cccccccc0001"
  }
]

OpenAPI & example responses
---------------------------
The API publishes an OpenAPI document at `/swagger/v1/swagger.json` when running in Development. Example responses for taxonomy endpoints are included in the OpenAPI document and visible in Swagger UI.

Quick client generation (PowerShell):

```powershell
# fetch swagger.json after starting the API locally
Invoke-WebRequest -Uri http://localhost:5000/swagger/v1/swagger.json -OutFile .\swagger.json

# generate a C# client with NSwag (install once)
dotnet tool install --global NSwag.ConsoleCore
nswag openapi2csclient /input:swagger.json /output:src\clients\CprApiClient.cs /namespace:CprApi.Client
```

Example response (C#) from generated client:

```csharp
var http = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var client = new CprApi.Client.CprApiClient(http);
var positions = await client.GetPositionsAsync();
foreach(var p in positions) Console.WriteLine($"{p.Id} {p.Title}");
```


Authentication & authorization notes
- OwnerId should be sourced from the authenticated user's subject claim.
- RBAC: owner & manager & admin roles map to update/delete privileges. Read allowed to owner/manager/admin and any project-member if goal is project-linked.

Validation highlights
- Title required (1..250 characters).
- Description length limit (e.g., 2000 chars).
- Deadline optional; policy: allow future dates (or accept backdated if domain requires).

Skills / Self-assessment
- GET /skills — list skills and taxonomy
- GET /skill_levels?skill_id=
- GET /me/skills
- POST /me/skills — submit self-assessment

Projects & Teams
- GET /projects
- GET /projects/{id}
- GET /projects/{id}/team

Feedback
- POST /feedback/request — request feedback from people
- POST /feedback — submit feedback
- GET /feedback/me — feedback received by me

---

## Manager
Team & Reports
- GET /team — list direct reports
- GET /team/members/{employee_id} — profile + goals + feedback
- GET /team/goals?status=&overdue=

Approvals & Reviews
- GET /reviews/pending
- POST /performance_reviews — create review for team member
- GET /performance_reviews/{employee_id}

Feedback moderation
- GET /team/feedback — feedback for team (visibility rules apply)

---

## HR / Admin
Users & Employees
- GET /users; POST /users; PATCH /users/{id}; DELETE /users/{id}
- GET /employees; POST /employees; PATCH /employees/{id}; DELETE /employees/{id}

Positions & Career
- CRUD /positions, /career_tracks, /career_paths
- GET /positions/{id}/skills

Skills taxonomy
- CRUD /skills, /skill_levels, /skill_categories
- POST /position_to_skill — map position → skill

Audit & housekeeping
- GET /audit_logs?entity_type=&entity_id=&from=&to=
- POST /seeds/run (dev-only)

---

## Director
- GET /promotions — list promotion requests
- GET /promotions/{id}
- POST /promotions/{id}/approve
- POST /promotions/{id}/decline
- GET /reports/skills-gap

---

## System / Integration
- POST /internal/import/users
- POST /internal/import/skills
- POST /webhook/feedback
- POST /jobs/retention/run

---

## Cross-cutting
- GET /health, GET /ready, GET /metrics
- Pagination and filter conventions: page, per_page, sort, q
- Soft-delete: default filter is_deleted=false (use include_deleted=true to override)
- Auth: role claims (admin|manager|director) + resource-owner checks

