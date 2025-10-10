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
- `GET /me/skills` - Any authenticated user
- `POST /me/skills` - Any authenticated user
- `PUT /me/skills/{skillId}` - Any authenticated user
- `POST /goals` - Any authenticated user
- `GET /me/goals` - Any authenticated user
- `GET /goals/{id}` - Employee+
- `PATCH /goals/{id}` - Employee+
- `DELETE /goals/{id}` - Administrator only
- `POST /goals/{id}/tasks` - Employee+
- `PATCH /goals/{id}/tasks/{taskId}` - Employee+

#### **Manager Endpoints** (People Manager, Solution Owner, Director, Administrator)
- `GET /team` - People Manager+
- `GET /team/members/{employee_id}` - People Manager+
- `GET /team/goals` - People Manager+

#### **Feedback Endpoints** (Any authenticated user)
- `POST /feedback` - Any authenticated user
- `POST /feedback/request` - Any authenticated user
- `GET /me/feedback` - Any authenticated user
- `GET /me/feedback/request` - Any authenticated user
- `GET /me/feedback/request/todo` - Any authenticated user

#### **Taxonomy Endpoints (Read)** (Public - no authentication required)
- `GET /career` - Public
- `GET /career_track` - Public
- `GET /positions` - Public
- `GET /skills` - Public
- `GET /skill_levels` - Public
- `GET /skill_categories` - Public
- `GET /positions/{id}/skills` - Public

#### **Taxonomy Management Endpoints (Write)** (Administrator role required)
**Career Path Management**:
- `POST /api/career` - Administrator only
- `PUT /api/career/{id}` - Administrator only
- `DELETE /api/career/{id}` - Administrator only (soft delete)

**Career Track Management**:
- `POST /api/career_track` - Administrator only
- `PUT /api/career_track/{id}` - Administrator only
- `DELETE /api/career_track/{id}` - Administrator only (soft delete)

**Position Management**:
- `POST /api/positions` - Administrator only
- `PUT /api/positions/{id}` - Administrator only
- `DELETE /api/positions/{id}` - Administrator only (soft delete)

**Skill Category Management**:
- `POST /api/skill_categories` - Administrator only
- `PUT /api/skill_categories/{id}` - Administrator only
- `DELETE /api/skill_categories/{id}` - Administrator only (soft delete)

**Skill Management**:
- `POST /api/skills` - Administrator only
- `PUT /api/skills/{id}` - Administrator only
- `DELETE /api/skills/{id}` - Administrator only (soft delete)

**Skill Level Management**:
- `POST /api/skill_levels` - Administrator only
- `PUT /api/skill_levels/{id}` - Administrator only
- `DELETE /api/skill_levels/{id}` - Administrator only (soft delete)

**Position-to-Skill Mapping**:
- `POST /api/positions/{id}/skills` - Administrator only
- `DELETE /api/positions/{id}/skills/{skillId}` - Administrator only

#### **Project Management Endpoints**
**Read Operations** (Any authenticated user):
- `GET /api/projects` - Any authenticated user
- `GET /api/projects/{id}` - Any authenticated user
- `GET /api/projects/{id}/roles` - Any authenticated user
- `GET /api/projects/{id}/team` - Any authenticated user

**Write Operations** (Solution Owner role required):
- `POST /api/projects` - Solution Owner only
- `PUT /api/projects/{id}` - Solution Owner only
- `DELETE /api/projects/{id}` - Solution Owner only
- `POST /api/projects/{id}/roles` - Solution Owner only
- `PUT /api/projects/{id}/roles/{roleId}` - Solution Owner only
- `DELETE /api/projects/{id}/roles/{roleId}` - Solution Owner only
- `POST /api/projects/{id}/team` - Solution Owner only
- `DELETE /api/projects/{id}/team/{teamMemberId}` - Solution Owner only

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

## Project Management (Project Owner Role)

### Overview
Project management endpoints allow Project Owners to create and manage projects, define project-specific roles, and assign employees to projects. All authenticated users can view project information, but only users with the Project Owner role can create, update, or delete projects and their related resources.

### Role Requirements
- **Read Operations**: Any authenticated user can view projects, roles, and team members
- **Write Operations**: Only users with the **Project Owner** role can create, update, or delete projects, roles, and team assignments

### Projects

#### GET /api/projects
- **Purpose**: Get all projects
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Any authenticated user
- **Response 200** (JSON):
  ```json
  [
    {
      "id": "GUID - Project identifier",
      "code": "string - Project code (e.g., PRJ-001)",
      "title": "string - Project title",
      "description": "string - Project description (optional)",
      "ownerId": "GUID - Project owner employee ID (optional)",
      "sponsorId": "GUID - Project sponsor employee ID (optional)",
      "createdAt": "DateTime - Creation timestamp",
      "modifiedAt": "DateTime - Last modification timestamp (optional)"
    }
  ]
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required

#### GET /api/projects/{id}
- **Purpose**: Get a specific project by ID
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Any authenticated user
- **Response 200** (JSON):
  ```json
  {
    "id": "GUID - Project identifier",
    "code": "string - Project code",
    "title": "string - Project title",
    "description": "string - Project description (optional)",
    "ownerId": "GUID - Project owner employee ID (optional)",
    "sponsorId": "GUID - Project sponsor employee ID (optional)",
    "createdAt": "DateTime - Creation timestamp",
    "modifiedAt": "DateTime - Last modification timestamp (optional)"
  }
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `404 Not Found`: Project not found

#### POST /api/projects
- **Purpose**: Create a new project
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Request Body** (JSON):
  ```json
  {
    "code": "string - Project code (required, max 50 characters)",
    "title": "string - Project title (required, max 250 characters)",
    "description": "string - Project description (optional, max 2000 characters)",
    "ownerId": "GUID - Project owner employee ID (optional)",
    "sponsorId": "GUID - Project sponsor employee ID (optional)"
  }
  ```
- **Response 201** (JSON): Returns created project (same structure as GET)
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required

#### PUT /api/projects/{id}
- **Purpose**: Update an existing project
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Request Body** (JSON):
  ```json
  {
    "code": "string - Project code (optional, max 50 characters)",
    "title": "string - Project title (optional, max 250 characters)",
    "description": "string - Project description (optional, max 2000 characters)",
    "ownerId": "GUID - Project owner employee ID (optional)",
    "sponsorId": "GUID - Project sponsor employee ID (optional)"
  }
  ```
- **Response 200** (JSON): Returns updated project
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project not found

#### DELETE /api/projects/{id}
- **Purpose**: Delete a project (soft delete)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project not found

### Project Roles

#### GET /api/projects/{id}/roles
- **Purpose**: Get all roles for a specific project
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Any authenticated user
- **Response 200** (JSON):
  ```json
  [
    {
      "id": "GUID - Role identifier",
      "projectId": "GUID - Project identifier",
      "title": "string - Role title",
      "description": "string - Role description (optional)",
      "createdAt": "DateTime - Creation timestamp",
      "modifiedAt": "DateTime - Last modification timestamp (optional)"
    }
  ]
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `404 Not Found`: Project not found

#### POST /api/projects/{id}/roles
- **Purpose**: Create a new role for a project
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Role title (required, max 250 characters)",
    "description": "string - Role description (optional, max 2000 characters)"
  }
  ```
- **Response 201** (JSON): Returns created role
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project not found

#### PUT /api/projects/{id}/roles/{roleId}
- **Purpose**: Update an existing project role
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Role title (optional, max 250 characters)",
    "description": "string - Role description (optional, max 2000 characters)"
  }
  ```
- **Response 200** (JSON): Returns updated role
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project or role not found

#### DELETE /api/projects/{id}/roles/{roleId}
- **Purpose**: Delete a project role (soft delete)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project or role not found

### Project Team

#### GET /api/projects/{id}/team
- **Purpose**: Get all team members for a specific project
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Any authenticated user
- **Response 200** (JSON):
  ```json
  [
    {
      "id": "GUID - Team member assignment identifier",
      "projectRoleId": "GUID - Project role identifier",
      "employeeId": "GUID - Employee identifier",
      "createdAt": "DateTime - Assignment timestamp",
      "modifiedAt": "DateTime - Last modification timestamp (optional)"
    }
  ]
  ```
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `404 Not Found`: Project not found

#### POST /api/projects/{id}/team
- **Purpose**: Assign an employee to a project role
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Request Body** (JSON):
  ```json
  {
    "projectRoleId": "GUID - Project role identifier (required)",
    "employeeId": "GUID - Employee identifier (required)"
  }
  ```
- **Response 201** (JSON): Returns created team assignment
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project, role, or employee not found

#### DELETE /api/projects/{id}/team/{teamMemberId}
- **Purpose**: Remove an employee from a project (soft delete)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Project Owner role required
- **Response 204**: No content (successful removal)
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Project Owner role required
  - `404 Not Found`: Project or team member not found

### Project Management Authorization Notes
- **Project Owner** role is required for all create, update, and delete operations
- All authenticated users can view projects, roles, and team members (read operations)
- Role assignments are managed through the `user_to_role` table in the database
- Users without the Project Owner role receive `403 Forbidden` responses for write operations
- All deletions are soft deletes (sets `is_deleted = true` without removing from database)
- Project roles are project-specific and independent from organization positions
- Employees can be assigned to multiple project roles across different projects

### Database Schema Notes
- **projects** table: Stores project information with code, title, description, owner, and sponsor
- **project_roles** table: Stores project-specific roles with project_id FK (not position_id)
- **project_teams** table: Junction table linking employees to project roles (project_role_id + employee_id)
- Relationship: Project → ProjectRole → ProjectTeam → Employee
- Projects are seeded with 10 sample projects and 100 project roles (10 roles per project)

---

## HR / Admin (Administrator Role Required)
Users & Employees
- GET /users; POST /users; PATCH /users/{id}; DELETE /users/{id}
- GET /employees; POST /employees; PATCH /employees/{id}; DELETE /employees/{id}

Career Paths & Career Tracks
- POST /api/career — Create career path
- PUT /api/career/{id} — Update career path
- DELETE /api/career/{id} — Soft delete career path
- POST /api/career_track — Create career track
- PUT /api/career_track/{id} — Update career track
- DELETE /api/career_track/{id} — Soft delete career track

Positions
- POST /api/positions — Create position
- PUT /api/positions/{id} — Update position
- DELETE /api/positions/{id} — Soft delete position
- POST /api/positions/{id}/skills — Map skill to position
- DELETE /api/positions/{id}/skills/{skillId} — Remove skill from position

Skills Taxonomy
- POST /api/skill_categories — Create skill category
- PUT /api/skill_categories/{id} — Update skill category
- DELETE /api/skill_categories/{id} — Soft delete skill category
- POST /api/skills — Create skill
- PUT /api/skills/{id} — Update skill
- DELETE /api/skills/{id} — Soft delete skill
- POST /api/skill_levels — Create skill level
- PUT /api/skill_levels/{id} — Update skill level
- DELETE /api/skill_levels/{id} — Soft delete skill level

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

---

## Iteration 15 — Taxonomy Management (Administrator CUD Operations)

### Overview
Iteration 15 extends the read-only taxonomy endpoints (career paths, career tracks, positions, skill categories, skills, skill levels) with full Create, Update, and Delete operations. All write operations require the **Administrator** role.

### Endpoints Summary

#### Career Path Management (Administrator role required)
- **POST /api/career** — Create a new career path
- **PUT /api/career/{id}** — Update an existing career path
- **DELETE /api/career/{id}** — Soft delete a career path

#### Career Track Management (Administrator role required)
- **POST /api/career_track** — Create a new career track
- **PUT /api/career_track/{id}** — Update an existing career track
- **DELETE /api/career_track/{id}** — Soft delete a career track

#### Position Management (Administrator role required)
- **POST /api/positions** — Create a new position
- **PUT /api/positions/{id}** — Update an existing position
- **DELETE /api/positions/{id}** — Soft delete a position

#### Skill Category Management (Administrator role required)
- **POST /api/skill_categories** — Create a new skill category
- **PUT /api/skill_categories/{id}** — Update an existing skill category
- **DELETE /api/skill_categories/{id}** — Soft delete a skill category

#### Skill Management (Administrator role required)
- **POST /api/skills** — Create a new skill
- **PUT /api/skills/{id}** — Update an existing skill
- **DELETE /api/skills/{id}** — Soft delete a skill

#### Skill Level Management (Administrator role required)
- **POST /api/skill_levels** — Create a new skill level
- **PUT /api/skill_levels/{id}** — Update an existing skill level
- **DELETE /api/skill_levels/{id}** — Soft delete a skill level

#### Position-to-Skill Mapping (Administrator role required)
- **POST /api/positions/{id}/skills** — Map a skill to a position with required skill level
- **DELETE /api/positions/{id}/skills/{skillId}** — Remove skill mapping from a position

### Endpoint Details

#### Career Path Management

##### POST /api/career
- **Purpose**: Create a new career path
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Career path title (required, 1-250 characters)",
    "description": "string - Career path description (optional, max 2000 characters)"
  }
  ```
- **Response 201** (JSON):
  ```json
  {
    "id": "GUID - Career path identifier",
    "title": "string - Career path title",
    "description": "string - Career path description (optional)"
  }
  ```
- **Error Responses**:
  - `400 Bad Request`: Validation errors (title required, duplicate title, length constraints)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required

##### PUT /api/career/{id}
- **Purpose**: Update an existing career path
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Career path title (optional, 1-250 characters)",
    "description": "string - Career path description (optional, max 2000 characters)"
  }
  ```
- **Response 200** (JSON): Returns updated career path
- **Error Responses**:
  - `400 Bad Request`: Validation errors (duplicate title, length constraints)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Career path not found

##### DELETE /api/career/{id}
- **Purpose**: Soft delete a career path (checks for dependent career tracks)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `400 Bad Request`: Cannot delete - has dependent career tracks
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Career path not found

#### Career Track Management

##### POST /api/career_track
- **Purpose**: Create a new career track
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Career track title (required, 1-250 characters)",
    "description": "string - Career track description (optional, max 2000 characters)",
    "careerPathId": "GUID - Parent career path identifier (required)"
  }
  ```
- **Response 201** (JSON): Returns created career track with `id`, `title`, `description`, `careerPathId`
- **Error Responses**:
  - `400 Bad Request`: Validation errors (title required, duplicate title within career path, invalid careerPathId)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required

##### PUT /api/career_track/{id}
- **Purpose**: Update an existing career track
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Career track title (optional, 1-250 characters)",
    "description": "string - Career track description (optional, max 2000 characters)",
    "careerPathId": "GUID - Parent career path identifier (optional)"
  }
  ```
- **Response 200** (JSON): Returns updated career track
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Career track not found

##### DELETE /api/career_track/{id}
- **Purpose**: Soft delete a career track (checks for dependent positions)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `400 Bad Request`: Cannot delete - has dependent positions
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Career track not found

#### Position Management

##### POST /api/positions
- **Purpose**: Create a new position
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Position title (required, 1-250 characters)",
    "description": "string - Position description (optional, max 2000 characters)",
    "expectations": "string - Position expectations (optional, max 2000 characters)",
    "careerTrackId": "GUID - Parent career track identifier (required)"
  }
  ```
- **Response 201** (JSON): Returns created position with `id`, `title`, `description`, `expectations`, `careerTrackId`
- **Error Responses**:
  - `400 Bad Request`: Validation errors (title required, duplicate title within career track, invalid careerTrackId)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required

##### PUT /api/positions/{id}
- **Purpose**: Update an existing position
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Position title (optional, 1-250 characters)",
    "description": "string - Position description (optional, max 2000 characters)",
    "expectations": "string - Position expectations (optional, max 2000 characters)",
    "careerTrackId": "GUID - Parent career track identifier (optional)"
  }
  ```
- **Response 200** (JSON): Returns updated position
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Position not found

##### DELETE /api/positions/{id}
- **Purpose**: Soft delete a position (checks for employee assignments and skill mappings)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `400 Bad Request`: Cannot delete - has employee assignments or skill mappings
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Position not found

#### Skill Category Management

##### POST /api/skill_categories
- **Purpose**: Create a new skill category
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Category title (required, 1-250 characters)",
    "description": "string - Category description (optional, max 2000 characters)"
  }
  ```
- **Response 201** (JSON): Returns created skill category with `id`, `title`, `description`
- **Error Responses**:
  - `400 Bad Request`: Validation errors (title required, duplicate title)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required

##### PUT /api/skill_categories/{id}
- **Purpose**: Update an existing skill category
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Category title (optional, 1-250 characters)",
    "description": "string - Category description (optional, max 2000 characters)"
  }
  ```
- **Response 200** (JSON): Returns updated skill category
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Skill category not found

##### DELETE /api/skill_categories/{id}
- **Purpose**: Soft delete a skill category (checks for dependent skills)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `400 Bad Request`: Cannot delete - has dependent skills
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Skill category not found

#### Skill Management

##### POST /api/skills
- **Purpose**: Create a new skill
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Skill title (required, 1-250 characters)",
    "description": "string - Skill description (optional, max 2000 characters)",
    "categoryId": "GUID - Parent skill category identifier (required)"
  }
  ```
- **Response 201** (JSON): Returns created skill with `id`, `title`, `description`, `categoryId`
- **Error Responses**:
  - `400 Bad Request`: Validation errors (title required, duplicate title within category, invalid categoryId)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required

##### PUT /api/skills/{id}
- **Purpose**: Update an existing skill
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "title": "string - Skill title (optional, 1-250 characters)",
    "description": "string - Skill description (optional, max 2000 characters)",
    "categoryId": "GUID - Parent skill category identifier (optional)"
  }
  ```
- **Response 200** (JSON): Returns updated skill
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Skill not found

##### DELETE /api/skills/{id}
- **Purpose**: Soft delete a skill (checks for skill levels, position mappings, employee assessments)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `400 Bad Request`: Cannot delete - has skill levels, position mappings, or employee assessments
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Skill not found

#### Skill Level Management

##### POST /api/skill_levels
- **Purpose**: Create a new skill level
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "skillId": "GUID - Parent skill identifier (required)",
    "value": "integer - Level value 1-5 (required)",
    "title": "string - Level title (required, 1-250 characters)",
    "description": "string - Level description (optional, max 2000 characters)"
  }
  ```
- **Response 201** (JSON): Returns created skill level with `id`, `skillId`, `value`, `title`, `description`
- **Error Responses**:
  - `400 Bad Request`: Validation errors (value must be 1-5, duplicate value for skill, invalid skillId)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required

##### PUT /api/skill_levels/{id}
- **Purpose**: Update an existing skill level
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "skillId": "GUID - Parent skill identifier (optional)",
    "value": "integer - Level value 1-5 (optional)",
    "title": "string - Level title (optional, 1-250 characters)",
    "description": "string - Level description (optional, max 2000 characters)"
  }
  ```
- **Response 200** (JSON): Returns updated skill level
- **Error Responses**:
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Skill level not found

##### DELETE /api/skill_levels/{id}
- **Purpose**: Soft delete a skill level (checks for position mappings and employee assessments)
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful deletion)
- **Error Responses**:
  - `400 Bad Request`: Cannot delete - has position mappings or employee assessments
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Skill level not found

#### Position-to-Skill Mapping

##### POST /api/positions/{id}/skills
- **Purpose**: Map a skill to a position with required skill level
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Request Body** (JSON):
  ```json
  {
    "skillId": "GUID - Skill identifier (required)",
    "skillLevelId": "GUID - Required skill level identifier (required)"
  }
  ```
- **Response 201** (JSON):
  ```json
  {
    "id": "GUID - Mapping identifier",
    "positionId": "GUID - Position identifier",
    "skillId": "GUID - Skill identifier",
    "skillTitle": "string - Skill title",
    "skillLevelId": "GUID - Skill level identifier",
    "skillLevelTitle": "string - Skill level title",
    "createdAt": "DateTime - Creation timestamp"
  }
  ```
- **Error Responses**:
  - `400 Bad Request`: Validation errors (skill level doesn't belong to skill, duplicate mapping, invalid skillId or skillLevelId)
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Position not found

##### DELETE /api/positions/{id}/skills/{skillId}
- **Purpose**: Remove skill mapping from a position
- **Authentication**: Required (JWT Bearer token)
- **Authorization**: Administrator role required
- **Response 204**: No content (successful removal)
- **Error Responses**:
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Administrator role required
  - `404 Not Found`: Position or skill mapping not found

### Authorization Notes
- All write operations require the **Administrator** role
- Read operations (`GET /career`, `GET /skill_categories`, etc.) remain public and require no authentication
- Users without the Administrator role receive `403 Forbidden` for all write operations
- Role assignments are managed through the `user_to_role` table

### Validation & Business Rules
- **Duplicate Detection**: Titles must be unique within their parent context (e.g., career track titles unique within career path)
- **Referential Integrity**: Parent entities must exist before creating child entities
- **Soft Delete Protection**: Cannot delete entities with dependent children (e.g., career path with career tracks)
- **Skill Level Values**: Must be integers 1-5 (Beginner to Expert)
- **Skill Level Validation**: When mapping skills to positions, the skill level must belong to the specified skill
- **Length Constraints**: Titles max 250 characters, descriptions max 2000 characters
- **Partial Updates**: PUT endpoints support partial updates (only provided fields are modified)

### Database Implementation
- All deletions are **soft deletes** (sets `is_deleted = true`, `deleted_at = timestamp`, `deleted_by = user_id`)
- Soft-deleted entities are automatically filtered from read queries
- `modified_by` and `modified_at` fields are updated on every modification
- All timestamps use `DateTimeOffset` for timezone support

### Test Coverage
- **Unit Tests**: 64 new tests in `TaxonomyControllerTests.cs` (148 total unit tests)
- **Integration Tests**: 85 new tests in `TaxonomyControllerIntegrationTests.cs` (~285 total integration tests)
- **Contract Tests**: 13 new tests in `TaxonomyContractTests.cs` with 8 JSON schema files (28 total contract tests)
- All tests validate authorization, validation, soft delete behavior, and referential integrity

