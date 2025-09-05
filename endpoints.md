# API Endpoints — CPR

## Persona: Jane Smith — Senior Software Engineer
- Roles: employee, project_member
- Goals: set/track goals, request/collect feedback, update profile, contribute to projects, self-assess skills

Endpoints

Common query params used in many endpoints:
- ?period=week|month|quarter|year (timeframe selection for timelines and reports)
- ?status=not_started|in_progress|achieved|archived
- pagination: ?page=1&per_page=20

# Contributor (Jane Smith) endpoints — goal & feedback focused
- GET /me
  - Return authenticated user's profile, current projects, active goals and recent feedback summary.
  - Query params: ?include=projects,goals,feedback_summary

- GET /me/goals
  - List user's goals with progress, tasks, related skill links and selected timeframe.
  - Query params: ?status=active|completed|archived&period=month
  - Response: paginated list of goal objects including progress_percent and timeframe summary

- POST /goals
  - Create a personal or project-linked development goal. Accepts optional timeframe and related_skill_id.
  - Body example:
    { "title": "Improve API test coverage", "description": "Add integration tests for service X", "related_skill_id": "<skill_uuid>", "deadline": "2025-12-31", "project_id": "<project_uuid>", "timeframe": "quarter" }
  - Response: 201 Created + created goal

- PATCH /goals/{id}
  - Update goal fields (progress, description, deadline, timeframe). Partial updates allowed.
  - Roles: owner, manager (where allowed)

- DELETE /goals/{id}
  - Remove a goal (soft-delete). Managers may confirm deletion for direct reports.
  - Response: 204 No Content

- POST /goals/{id}/archive
  - Archive a completed goal (moves to user's archive).
  - Response: 200 OK

- GET /goals/archive
  - List archived goals for the authenticated user.
  - Query params: ?page=1&per_page=20

- GET /goals/{id}/progress
  - Return detailed progress (tasks, completed steps, percent complete, timeline points).
  - Response: progress object with task list and calculated progress_percent

- POST /goals/{id}/tasks
  - Add a task under a goal (actionable step).
  - Body: { "title": "Add tests for endpoint Y", "deadline": "2025-10-01" }

- GET /skills
  - Read-only taxonomy for skills and levels; used when creating goals or self-assessing.

- POST /employee_to_skill
  - Submit a self-assessment entry for a skill (source='self').
  - Body example: { "employee_id": "<uuid>", "skill_id": "<skill_uuid>", "skill_level_id": "<level_uuid>", "persist_value": 3.5, "source": "self", "effective_date": "2025-09-01", "is_target": false }

- POST /goals/suggest
  - Suggest SMART goal(s) for a user based on current skill gaps, availability and history.
  - Body: { "employee_id": "<uuid>", "skill_id": "<skill_uuid>", "availability": { "hours_per_week": 5 }, "target_timeframe": "quarter" }
  - Response: list of suggested goal drafts (title, description, suggested_deadline)

- POST /goals/{id}/plan
  - Request an automated improvement plan (step list) for a specific goal.
  - Response: step-by-step plan with suggested tasks and estimated time per step

- POST /feedback/request
  - Request feedback from project team or specific employees (creates feedback_request records and triggers notifications).
  - Body example:
    { "goal_id": "<goal_uuid>", "project_id": "<project_uuid>", "to_employee_ids": ["<employee_uuid>"], "message": "Would you review my recent PR and give feedback on design and tests?", "due_date": "2025-10-01" }
  - Response: 200 OK + list of created feedback_request records

- POST /feedback
  - Submit feedback about a peer's work (can be tied to a goal or project).
  - Body example:
    { "goal_id": "<goal_uuid>", "project_id": "<project_uuid>", "from_employee_id": "<employee_uuid>", "to_employee_id": "<employee_uuid>", "content": "Great design and thorough tests.", "rating": 4, "visibility": "team" }
  - Response: 201 Created + feedback record

- GET /feedback/me
  - List feedback received by the authenticated user, aggregated by goal or timeframe.

- GET /feedback/reports
  - Return aggregated feedback reports for the requesting user (e.g., contributions toward goals)
  - Query params: ?period=90d

# People leader / Manager (Peter Morrison) endpoints — oversight & reviews
- GET /team
  - Return manager's team roster, open goals, and high-level feedback summary.
  - Query params: ?include=members,goals,feedback_summary

- GET /team/members/{employee_id}
  - Get profile, active goals, recent feedback and project assignments for a team member.

- GET /team/goals
  - List goals owned by direct reports with progress and overdue flags.
  - Query params: ?status=active|completed&overdue=true

- POST /goals (manager-as-owner)
  - Create or assign development goals for direct reports.
  - Body example: { "title": "Improve code review quality", "employee_id": "<employee_uuid>", "related_skill_id": "<skill_uuid>", "deadline": "2025-12-31" }

- POST /feedback/request
  - Request feedback on behalf of a direct report (creates feedback_request records).
  - Body example:
    { "employee_id": "<employee_uuid>", "goal_id": "<goal_uuid>", "project_id": "<project_uuid>", "to_employee_ids": ["<employee_uuid>"], "message": "Please provide feedback on Alex's recent design work.", "due_date": "2025-10-01" }

- GET /feedback (manager view)
  - Retrieve feedback for a specific team member (respecting visibility and policy).
  - Query params: ?employee_id={id}&from_project={id}&since={date}

- POST /performance_reviews
  - Create a performance review record for a team member (cycle-based); attach goals and feedback.
  - Body example: { "employee_id": "<employee_uuid>", "cycle": "2025-Q3", "ratings": { "communication": 4, "delivery": 3 }, "summary": "Mid-year review notes." }

- GET /performance_reviews/{employee_id}
  - List past performance reviews for a team member.

- GET /reports/feedback-summary
  - Aggregated feedback metrics across the manager's team (ratings distribution, frequent tags).
  - Query params: ?period=90d

# Director endpoints — promotions & approvals
- GET /promotions
  - List promotion requests ready for director review (with linked performance reviews and feedback).

- GET /promotions/{id}
  - View promotion request details including performance review and aggregated feedback.

- POST /promotions/{id}/approve
  - Approve a promotion request (director action).
  - Body: { "approved_by": "<employee_uuid>", "comments": "Approved for next level" }

- POST /promotions/{id}/decline
  - Decline a promotion request with comments.
  - Body: { "declined_by": "<employee_uuid>", "comments": "Not enough impact evidence" }

# Career path / taxonomy endpoints
- GET /career_paths
  - List available career paths

- GET /career_paths/{id}/tracks
  - List tracks within a path

- GET /tracks/{id}/titles
  - List titles within a track

- GET /positions/{id}
  - Get position details with requirements, expectations and skills criteria

# Admin / reporting endpoints
- GET /reports/performance-overview
  - Organization-level performance overview (directors / HR)

- GET /analytics/skills-gap
  - Aggregated skills-gap analysis across org or team (supports filters)

