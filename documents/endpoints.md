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

## Authorization & Roles

The API implements comprehensive Role-Based Access Control (RBAC) with the following role hierarchy:

### **Role Definitions**
- **Employee**: Basic user role for individual contributors. Can manage their own goals, feedback, and profile.
- **People Manager**: Extends Employee role. Can view and manage their direct reports' goals, feedback, and team information.
- **Solution Owner**: Extends People Manager role. Can manage projects and oversee solution-level initiatives.
- **Director**: Extends Solution Owner role. Can approve promotions and access director-level reports.
- **Administrator**: Full system access. Can manage users, roles, positions, and all system data.

### **Role-Based Authorization**
All endpoints require appropriate roles. Authorization is enforced through:
- `[RequireRole]` attributes on controllers and methods
- JWT token validation with role claims
- Database-backed role assignments via `user_to_role` junction table

### **Role Requirements by Endpoint Group**

#### **Employee Endpoints** (Any authenticated user)
- `GET /me` - Any authenticated user
- `PATCH /me` - Any authenticated user
- `POST /goals` - Any authenticated user
- `GET /me/goals` - Any authenticated user
- `GET /goals/{id}` - Owner only (or higher roles)
- `PATCH /goals/{id}` - Owner only (or higher roles)
- `DELETE /goals/{id}` - Owner only (or Administrator)
- `POST /goals/{id}/tasks` - Owner only (or higher roles)

#### **Manager Endpoints** (People Manager, Solution Owner, Director, Administrator)
- `GET /team` - People Manager+
- `GET /team/members/{employee_id}` - People Manager+
- `GET /team/goals` - People Manager+
- `GET /employees/{id}` - People Manager+
- `GET /employees` - People Manager+

#### **HR/Admin Endpoints** (Administrator only)
- `GET /users` - Administrator
- `POST /users` - Administrator
- `PATCH /users/{id}` - Administrator
- `DELETE /users/{id}` - Administrator
- `GET /employees` - Administrator
- `POST /employees` - Administrator
- `PATCH /employees/{id}` - Administrator
- `DELETE /employees/{id}` - Administrator
- `CRUD /positions` - Administrator
- `CRUD /career_tracks` - Administrator
- `CRUD /career_paths` - Administrator
- `CRUD /skills` - Administrator
- `POST /position_to_skill` - Administrator

#### **Director Endpoints** (Director, Administrator)
- `GET /promotions` - Director+
- `GET /promotions/{id}` - Director+
- `POST /promotions/{id}/approve` - Director+
- `POST /promotions/{id}/decline` - Director+

#### **System Endpoints** (Administrator only)
- `POST /internal/import/users` - Administrator
- `POST /internal/import/skills` - Administrator
- `POST /webhook/feedback` - Administrator
- `POST /jobs/retention/run` - Administrator

### **Authorization Implementation Notes**
- Users can have multiple roles (many-to-many relationship)
- Higher-level roles inherit permissions from lower-level roles
- Role assignments are managed through the `user_to_role` table
- All authorization failures return `403 Forbidden` with appropriate error messages
- Authentication is required for all endpoints except health checks

---

## Employee (self)
Profile / Users
- GET /me — return current user's profile (auth)
- PATCH /me — update own profile (auth)

Employees
- GET /me/employee — employee record for signed-in user
- GET /employees/{id} — read employee (manager/admin)
- GET /employees?department=&manager_id=&page=&size= — list / filters (manager/admin)

### GET /me
- **Purpose**: Return the current authenticated user's profile information
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Any authenticated user
- **Response 200** (JSON):
  ```json
  {
    "userId": "GUID - Unique user identifier",
    "userName": "string - User's login/username",
    "displayName": "string - User's display name",
    "employeeId": "GUID - Associated employee record ID",
    "position": "string - User's job position/title"
  }
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `404 Not Found`: User profile not found

### Notes
- `userName` and `displayName` are sourced from the database, not JWT claims
- `userId` is the unique identifier for the user account
- `employeeId` links to the employee's detailed record
- `position` reflects the user's current job title

Goals
- POST /goals — create goal (any authenticated user)
- GET /me/goals — list my goals (any authenticated user) (filters: status, page, sort)
- GET /goals/{id} — read goal (owner or People Manager+)
- PATCH /goals/{id} — update goal (owner or People Manager+)
- DELETE /goals/{id} — soft-delete goal (owner or Administrator)
- POST /goals/{id}/tasks — add task to goal (owner or People Manager+)

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
- POST /api/feedback — submit feedback to an employee
- POST /api/feedback/request — request feedback from people
- GET /api/me/feedback — get feedback addressed to current user
- GET /api/me/feedback/request — feedback requests I sent
- GET /api/me/feedback/request/todo — feedback requests addressed to me (to respond to)

### Contracts & purposes (Feedback)

#### POST /api/feedback
- **Purpose**: Submit feedback from the authenticated user to another employee regarding a specific goal. Content is automatically sanitized to prevent XSS attacks and malicious input.
- **Authentication**: Required (JWT Bearer token)
- **Request Body** (JSON):
  ```json
  {
    "projectId": "GUID (optional) - The project this feedback is for",
    "goalId": "GUID (required) - The goal this feedback is for",
    "employeeId": "GUID (required) - Employee receiving feedback",
    "content": "string (required, 10-2000 chars) - Feedback content",
    "rating": "integer (required, 1-5) - Rating on 1-5 scale"
  }
  ```
- **Validation Rules**:
  - `projectId`: Optional, must be a valid project if provided
  - `goalId`: Must be a valid, non-deleted goal
  - `employeeId`: Must be a valid, non-deleted employee, cannot be the same as the authenticated user
  - `content`: 10-2000 characters, automatically sanitized (HTML tags removed, scripts filtered)
  - `rating`: Must be between 1 and 5
- **Input Sanitization**: Content is automatically cleaned of HTML tags, script elements, and suspicious patterns
- **Response 201** (JSON):
  ```json
  {
    "id": "GUID - Feedback identifier",
    "projectId": "GUID (nullable) - Associated project",
    "goalId": "GUID - Associated goal",
    "fromEmployeeId": "GUID - Feedback giver (derived from authentication)",
    "toEmployeeId": "GUID - Feedback receiver",
    "content": "string - Sanitized feedback content",
    "rating": "integer - Rating value",
    "createdAt": "DateTime - Creation timestamp",
    "project": {
      "id": "GUID (nullable)",
      "title": "string (nullable) - Project title"
    },
    "goal": {
      "id": "GUID",
      "title": "string - Goal title"
    },
    "fromEmployee": {
      "id": "GUID",
      "displayName": "string - Employee name"
    },
    "toEmployee": {
      "id": "GUID",
      "displayName": "string - Employee name"
    }
  }
  ```
- **Error Responses**:
  - `400 Bad Request`: Validation failed
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7807",
      "title": "Validation failed",
      "detail": "One or more validation errors occurred",
      "status": 400,
      "instance": "/api/feedback",
      "errors": {
        "Content": ["Feedback content must be between 10 and 2000 characters"],
        "Rating": ["Rating must be between 1 and 5"],
        "EmployeeId": ["Cannot submit feedback to yourself"]
      }
    }
    ```
  - `400 Bad Request`: Sanitization/content validation failed
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7807",
      "title": "Validation failed",
      "detail": "Feedback content contains invalid or malicious content",
      "status": 400,
      "instance": "/api/feedback"
    }
    ```
  - `401 Unauthorized`: Authentication required
  - `404 Not Found`: Goal or employee not found

#### POST /api/feedback/request
- **Purpose**: Create a feedback request from the current user to another employee
- **Authentication**: Required (JWT Bearer token)
- **Request Body** (JSON):
  ```json
  {
    "employeeId": "GUID (required) - Employee to request feedback from",
    "projectId": "GUID (optional) - Associated project context",
    "goalId": "GUID (optional) - Associated goal context",
    "message": "string (optional) - Request message",
    "dueDate": "DateTimeOffset (optional) - When feedback is due"
  }
  ```
- **Response 201** (JSON):
  ```json
  {
    "id": "GUID - Request identifier",
    "requestorId": "GUID - User who made the request",
    "employeeId": "GUID - Employee to provide feedback",
    "projectId": "GUID (nullable)",
    "goalId": "GUID (nullable)",
    "message": "string (nullable)",
    "dueDate": "DateTimeOffset (nullable)",
    "createdAt": "DateTimeOffset",
    "requestor": {
      "id": "GUID",
      "displayName": "string"
    },
    "employee": {
      "id": "GUID",
      "displayName": "string"
    },
    "project": {
      "id": "GUID (nullable)",
      "title": "string (nullable)"
    },
    "goal": {
      "id": "GUID (nullable)",
      "title": "string (nullable)"
    }
  }
  ```

#### GET /api/me/feedback
- **Purpose**: Get all feedback addressed to the current user (optimized response format)
- **Authentication**: Required (JWT Bearer token)
- **Response 200** (JSON Array):
  ```json
  [
    {
      "id": "GUID",
      "projectId": "GUID (nullable)",
      "goalId": "GUID",
      "fromEmployeeId": "GUID",
      "content": "string",
      "rating": "integer",
      "createdAt": "DateTime",
      "project": {
        "id": "GUID (nullable)",
        "title": "string (nullable)"
      },
      "goal": {
        "id": "GUID",
        "title": "string"
      },
      "fromEmployee": {
        "id": "GUID",
        "displayName": "string"
      }
    }
  ]
  ```

#### GET /api/me/feedback/request
- **Purpose**: Get feedback requests sent by the current user
- **Authentication**: Required (JWT Bearer token)
- **Response 200** (JSON Array): Array of feedback request objects (same format as POST response)

#### GET /api/me/feedback/request/todo
- **Purpose**: Get feedback requests addressed to the current user (requests to respond to)
- **Authentication**: Required (JWT Bearer token)
- **Response 200** (JSON Array): Array of feedback request objects (same format as POST response)

### Security & Validation Notes
- **Input Sanitization**: All feedback content is automatically sanitized to prevent XSS attacks
- **Self-Feedback Prevention**: Users cannot submit feedback to themselves
- **Content Validation**: Feedback content must be 10-2000 characters and contain valid text patterns
- **Rating Validation**: Ratings must be integers between 1 and 5
- **Error Format**: All errors follow RFC7807 Problem Details format for consistent API responses

---

## Manager
Team & Reports
- GET /team — list direct reports (People Manager role required)
- GET /team/members/{employee_id} — profile + goals + feedback (People Manager role required)
- GET /team/goals?status=&overdue= — team goals overview (People Manager role required)

### GET /team
- **Purpose**: List all direct reports for the authenticated manager
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: People Manager role required (users without People Manager role receive 403 Forbidden)
- **Response 200** (JSON Array):
  ```json
  [
    {
      "id": "GUID - Employee ID",
      "userId": "GUID - Associated user ID",
      "userName": "string - Employee username",
      "displayName": "string - Employee display name",
      "position": "string - Job position/title",
      "department": "string - Department name",
      "managerId": "GUID - Manager's employee ID"
    }
  ]
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: User is not a manager or has no direct reports

### GET /team/members/{employee_id}
- **Purpose**: Get detailed profile, goals, and feedback for a specific team member
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Manager role required, and employee must be a direct report
- **Path Parameters**:
  - `employee_id`: GUID of the employee to retrieve
- **Response 200** (JSON):
  ```json
  {
    "profile": {
      "id": "GUID",
      "userId": "GUID",
      "userName": "string",
      "displayName": "string",
      "position": "string",
      "department": "string"
    },
    "goals": [
      {
        "id": "GUID",
        "title": "string",
        "status": "string",
        "deadline": "DateTime (nullable)",
        "createdAt": "DateTime"
      }
    ],
    "recentFeedback": [
      {
        "id": "GUID",
        "fromEmployeeId": "GUID",
        "content": "string",
        "rating": "integer",
        "createdAt": "DateTime"
      }
    ]
  }
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: User is not a manager or employee is not a direct report
  - `404 Not Found`: Employee not found

### Authorization Notes
- People Manager role authorization is required for all team management endpoints
- Role assignments are managed through the `user_to_role` table in the database
- Users without the People Manager role receive 403 Forbidden responses
- All team endpoints require the authenticated user to have the People Manager role assigned

Approvals & Reviews
- GET /reviews/pending
- POST /performance_reviews — create review for team member
- GET /performance_reviews/{employee_id}

Feedback moderation
- GET /team/feedback — feedback for team (visibility rules apply)

---

## HR / Admin (Administrator Role Required)
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

## Director (Director Role Required)
- GET /promotions — list promotion requests
- GET /promotions/{id}
- POST /promotions/{id}/approve
- POST /promotions/{id}/decline
- GET /reports/skills-gap

---

## System / Integration (Administrator Role Required)
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

