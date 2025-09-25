# Data Model — CPR

This document describes the initial PostgreSQL schema for the CPR platform. Keep the design simple and extensible; prefer clarity over premature optimization. Use UUID primary keys and sensible indexes for queries described in the vision.

Conventions
- PK: uuid (use gen_random_uuid() / uuid_generate_v4())
- Timestamps: timestamptz with timezone
- Soft delete: boolean "is_deleted" + deleted_at when needed
- Auditing: audit_logs table for important write actions
- JSON: use jsonb for flexible snapshots and tags
- Concurrency: add a row_version bigint or use xmin for optimistic concurrency

## TABLES

### users - DONE
- Purpose: Stores application user accounts and authentication details; referenced by audit and created_by/modified_by fields across the schema.
- id uuid PRIMARY KEY
- user_name text NOT NULL UNIQUE
- password_hash text NULL -- nullable for external/SSO accounts
- email text NULL UNIQUE
- display_name text NULL
- auth_provider text NULL -- e.g., 'local','azuread','sso'
- last_login timestamptz NULL
- is_active boolean NOT NULL DEFAULT true
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

Indexes: functional index on lower(user_name), index on (email)

### employees - DONE
- Purpose: Represents people in the organization (profile and org links). Connects to `users` for login and to departments/positions for org data.
- id uuid PRIMARY KEY
- first_name text NOT NULL
- middle_name text NULL
- last_name text NOT NULL
- report_to uuid NULL REFERENCES employees(id)
- user_id uuid NOT NULL UNIQUE REFERENCES users(id)
- hire_date date NULL
- department_id uuid NOT NULL REFERENCES departments(id)
- position_id uuid NOT NULL REFERENCES positions(id)
- location_id uuid NOT NULL REFERENCES locations(id)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### employee_to_skill
- Purpose: Records observed or target skill assessments for employees, including source and effective date.
- id uuid PRIMARY KEY
- employee_id uuid NOT NULL REFERENCES employees(id)
- skill_id uuid NOT NULL REFERENCES skills(id)
- skill_level_id uuid NULL REFERENCES skill_levels(id) -- assessed / target level
- persist_value numeric(5,2) NULL -- optional numeric value persisted for the assessment (e.g., score)
- source text NULL -- e.g. 'self','manager','assessment','import'
- effective_date date NULL -- when this rating/target applies from
- is_target boolean NOT NULL DEFAULT false -- distinguishes targets from observed assessments
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### audit_logs - DONE
- Purpose: Append-only audit trail for key write actions, moderation and system events.
- id uuid PRIMARY KEY
- actor_id uuid NULL REFERENCES users(id)
- action text NOT NULL -- e.g. 'create_goal','update_feedback'
- target_type text NULL -- e.g. 'goal','feedback','review'
- target_id uuid NULL
- detail jsonb NULL -- free-form details for the audit entry
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### career_paths - DONE
- Purpose: High-level career domains (e.g., Technology, Finance) used to group career tracks and roles.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### career_tracks - DONE
- Purpose: Sub-domains within a career path that group related positions and progression ladders.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- career_path_id uuid NOT NULL REFERENCES career_paths(id)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### positions - DONE
- Purpose: Canonical job/role definitions used for hiring, leveling and mapping to project roles.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- expectations text NULL -- free-form expectations / responsibilities
- career_track_id uuid NOT NULL REFERENCES career_tracks(id)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### skill_categories - DONE
- Purpose: Top-level grouping of skills (e.g., Leadership, Technical) to organize the skills taxonomy.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### skills - DONE
- Purpose: Individual skills or competencies tied to categories, used in assessments and goal linking.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- category_id uuid NOT NULL REFERENCES skill_categories(id)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### skill_levels - DONE
- Purpose: Describes levels for a skill (e.g., Beginner..Expert) with numeric values for comparison.
- id uuid PRIMARY KEY
- title text NOT NULL
- desciption text NULL
- skill_id uuids NOT NULL REFERENCE skills(id)
- value int NOT NULL -- numeric value for the level (e.g., 1..5)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### position_to_skill
- Purpose: Maps positions to required/desired skills and target levels used for hiring and assessments.
- id uuid PRIMARY KEY
- position_id uuid NOT NULL REFERENCES positions(id)
- skill_id uuid NOT NULL REFERENCES skills(id)
- skill_level_id uuid NOT NULL REFERENCES skill_levels(id)
- weight numeric(5,2) DEFAULT 1.0 -- relative importance of this skill for the position
- is_mandatory boolean NOT NULL DEFAULT false
- rationale text NULL -- short note why this skill/level is required for the position
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### goals - DONE
- Purpose: Development or performance goals owned by employees, optionally linked to skills and projects.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- employee_id uuid NOT NULL REFERENCES employees(id)
- related_skill_id uuid NULL REFERENCES skills(id) -- optional link to a skill the goal develops
- related_skill_level_id uuid NULL REFERENCES skill_levels(id) -- optional target level
- deadline date NULL
- is_completed boolean NOT NULL DEFAULT false
- completed_at timestamptz NULL
- progress_percent numeric(5,2) NOT NULL DEFAULT 0.00 -- 0.00 .. 100.00
- priority smallint NULL -- e.g., 1 (high) .. 5 (low)
- visibility text NULL -- e.g., 'private','team','org'
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### goal_tasks - DONE
- Purpose: Break down of a goal into actionable tasks with progress and completion tracking.
- id uuid PRIMARY KEY
- goal_id uuid NOT NULL REFERENCES goals(id) ON DELETE CASCADE
- title text NOT NULL
- description text NULL
- deadline date NULL
- is_completed boolean NOT NULL DEFAULT false
- completed_at timestamptz NULL
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### departments - DONE
- Purpose: Organizational departments for reporting lines, owner/manager assignment and grouping employees.
- id uuid PRIMARY KEY
- name text NOT NULL
- code text NULL -- short code (e.g., FIN, ENG) useful for integrations and reports
- description text NULL
- parent_department_id uuid NULL REFERENCES departments(id) -- optional for hierarchical orgs
- manager_id uuid NULL REFERENCES employees(id) -- optional link to the department lead
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### locations - DONE
- Purpose: Physical or virtual workplace locations used for employee metadata and regional settings.
- id uuid PRIMARY KEY
- name text NOT NULL -- e.g., 'Headquarters', 'London Office', 'Remote'
- address text NULL
- city text NULL
- region text NULL -- state/province
- country text NULL
- postal_code text NULL
- timezone text NULL -- e.g., 'Europe/London'
- contact_phone text NULL
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### projects - DONE
- Purpose: Tracks cross-functional initiatives; provides context for team membership and project-level feedback.
- id uuid PRIMARY KEY
- code text NOT NULL UNIQUE -- short project code (e.g., PRJ-123)
- title text NOT NULL
- description text NULL
- owner_id uuid NULL REFERENCES employees(id) -- primary project owner (employee)
- sponsor_id uuid NULL REFERENCES employees(id) -- optional project sponsor (employee)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### project_roles - DONE
- Purpose: Reusable project role definitions (e.g., Tech Lead, Product Manager) that map to positions.
- id uuid PRIMARY KEY
- title text NOT NULL
- description text NULL
- position_id uuid NULL REFERENCES positions(id) -- optional link to canonical position/level
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### project_teams
- Purpose: Assignment of employees to projects with a specific project role; used for access and feedback context.
- id uuid PRIMARY KEY
- project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE
- project_role_id uuid NOT NULL REFERENCES project_roles(id)
- employee_id uuid NOT NULL REFERENCES employees(id)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### feedback - DONE
- Purpose: Peer or manager feedback tied to a goal (and optionally project context) for performance reviews.
- id uuid PRIMARY KEY
- goal_id uuid NOT NULL REFERENCES goals(id) -- link to the goal this feedback is about
- project_id uuid NULL REFERENCES projects(id) -- optional link to project context
- project_role_id uuid NULL REFERENCES project_roles(id) -- optional role context
- from_employee_id uuid NOT NULL REFERENCES employees(id) -- who provided the feedback
- to_employee_id uuid NOT NULL REFERENCES employees(id) -- who the feedback is about
- content text NOT NULL -- free-form feedback text
- rating smallint NULL -- optional numeric rating (e.g., 1..5)
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

### feedback_requests - DONE
- id uuid PRIMARY KEY
- requestor_id uuid NOT NULL REFERENCES employees(id) -- employee who requested feedback
- employee_id uuid NOT NULL REFERENCES employees(id) -- the subject of the requested feedback
- project_id uuid NULL REFERENCES projects(id) -- optional project context
- goal_id uuid NULL REFERENCES goals(id) -- optional goal context
- message text NULL -- request message to recipients
- due_date date NULL -- optional deadline for responses
- created_by uuid NULL REFERENCES users(id)
- created_at timestamptz NOT NULL DEFAULT now()
- modified_by uuid NULL REFERENCES users(id)
- modified_at timestamptz NULL
- is_deleted boolean NOT NULL DEFAULT false
- deleted_by uuid NULL REFERENCES users(id)
- deleted_at timestamptz NULL

## SEED VALUES

### users (example seed)
- { id: '<uuid>', user_name: 'admin', email: 'admin@example.com', display_name: 'Administrator', auth_provider: 'local', is_active: true, created_at }

### career_paths (suggested seed values)
- Finance — Roles related to financial planning, reporting, and analysis.
- Business Development — Revenue, partnerships, and strategic growth roles.
- People — HR, people operations, and employee development roles.
- Delivery and Transformation — Program delivery, change management, and transformation roles.
- Technology — Engineering, architecture, and platform roles.
- Legal — Legal, compliance, and contract management roles.
- Marketing — Brand, demand generation, and communications roles.
- Operations — Business operations, facilities and process roles.
- Talent Acquisition — Recruiting, employer branding, and sourcing roles.

These can be inserted as seed rows during initial migration or managed via an admin UI. Example seed structure: { id, title, description, created_by, created_at }

### career_tracks (suggested seed values)
- Finance:
  - Accounting — Manage financial records, ledgers, month-end close, and statutory reporting.
  - Accounts Payable and Accounts Receivable — Process invoices, payments, collections, and vendor/customer reconciliations.
  - Engagement Finance Operations — Finance operations supporting client engagements, billing and project costing.
  - Finance Risk Management — Identify, assess and mitigate financial risks and controls.
  - Financial Planning — Budgeting, forecasting, and long-range financial planning and analysis.
  - Procurement — Source suppliers, negotiate contracts and manage purchasing processes.
  - Tax — Manage tax compliance, reporting and planning across jurisdictions.

- Business Development:
  - Partner Development — Build and manage strategic partner relationships and alliances.
  - Inside Sales — Qualify leads, drive pipeline and close smaller accounts via inside sales channels.
  - Partner Sales — Enable and manage partner-led sales motions and joint opportunities.
  - Sales — Field and enterprise sales focused on closing revenue and customer acquisition.
  - Sales Engineering — Provide technical pre-sales expertise, demos and solution architecture support.

- People:
  - Benefits and Mobility — Design and operate employee benefits, relocation and mobility programs.
  - Compensation — Design pay structures, job leveling and total rewards strategies.
  - Equity Program — Manage employee equity plans, grants and reporting.
  - Learning Experience Design — Create learning programs, curricula and career development paths.
  - Payroll — Calculate and run payroll, tax withholdings and related reporting.
  - People Business Partnership — Strategic HR partners embedded with business units to drive people plans.
  - People Experience — Employee engagement, well-being and internal communications initiatives.
  - People Services — HR operations, case management and HR service delivery.

- Delivery and Transformation:
  - Change Management — Lead organizational change, adoption plans and stakeholder engagement.
  - Experience Design — Design user and employee experiences across products and services.
  - Project Management — Plan, execute and govern projects to deliver outcomes on time and budget.

- Technology:
  - Architecture — Define solution and enterprise architecture, technical strategy and standards.
  - Business Intelligence and Analytics — Build analytics, dashboards and insights to support decisions.
  - Data Engineering — Design and maintain data pipelines, warehouses and ETL processes.
  - Information Security — Protect systems and data, run security controls and incident response.
  - Infrastructure Engineering — Build and operate cloud/on-prem infrastructure and platform services.
  - Product Delivery — Manage product lifecycle, roadmaps and cross-functional delivery.
  - Software Engineering — Develop, test and maintain application software and services.
  - Technology Support and Operations — Provide day-to-day support, incident management and runbook operations.

- Legal:
  - Compliance and Risk Management — Oversee regulatory compliance, policy and enterprise risk programs.
  - Legal Counsel — Provide legal advice, contracts and corporate governance support.
  - Legal Support — Paralegal and legal operations support for contracts and filings.

- Marketing:
  - Communications — Corporate and internal communications, PR and media relations.
  - Content — Create written and multimedia content for demand and brand initiatives.
  - Creative — Design and creative production for brand and campaign assets.
  - Event Management — Plan and execute events, webinars and experiential marketing.
  - Marketing — Demand generation, brand, product marketing and campaign execution.

- Operations:
  - Business Operations — Core business processes, vendor management and operational analytics.
  - Business Strategy — Strategic planning, market analysis and corporate initiatives.
  - Global Resilience and Safety — Risk, continuity planning and workplace safety programs.
  - Operations Programs — Cross-functional operational programs and process improvement.
  - Spaces and Workspaces — Manage offices, facilities and workplace experience.
  - Team Operations — People and team-level operational support and tooling.

- Talent Acquisition:
  - Talent Acquisition — Recruit, source and hire talent; manage employer branding and candidate experience.

These can be inserted as seed rows during initial migration or managed via an admin UI. Example seed structure: { id, title, description, created_by, created_at }

### positions (suggested seed values)
- Architecture:
  - Architect — Designs solution and system architecture across products. expectations: define architecture patterns, create design docs, perform design reviews, mentor engineers.
  - Sr Architect — Leads complex, cross-team architecture initiatives. expectations: drive technical direction, stakeholder alignment, mentor architects.
  - Director — Manages architecture teams and roadmaps. expectations: craft strategy, prioritize architecture investments, represent architecture to leadership.
  - Sr Director — Oversees multiple architecture domains and long-term strategy. expectations: lead cross-organization initiatives, set standards and governance.
  - Managing Director — Executive responsibility for architecture and technology strategy. expectations: align architecture with business goals, drive large-scale transformation.

- Business Intelligence and Analytics:
  - Associate BI Developer — Entry-level BI work building reports and data views. expectations: support dashboard builds, learn ETL patterns, validate data quality.
  - BI Developer — Build BI solutions and dashboards. expectations: implement ETL, SQL models, deliver reports to stakeholders.
  - Sr BI Developer — Lead BI development and data modelling. expectations: design data models, optimize queries, mentor juniors.
  - Lead — Technical lead for BI projects. expectations: coordinate BI efforts, ensure data accuracy and performance.
  - Manager — Manages BI team and delivery. expectations: prioritize analytics roadmap, liaise with product owners.
  - Sr Manager — Oversees multiple BI squads and strategy. expectations: lead analytics programs, stakeholder engagement.
  - Director — Owns analytics strategy and outcomes. expectations: set vision, measure impact and ROI.
  - Sr Director — Senior leader for analytics across business domains. expectations: executive reporting, cross-org alignment.
  - Managing Director — Executive owner of data engineering capabilities. expectations: champion data-driven decision-making at enterprise scale.

- Data Engineering:
  - Intern — Support data pipeline tasks and learning. expectations: assist ETL jobs, write tests, learn tooling.
  - Engineer — Build and maintain data pipelines. expectations: implement reliable ETL, monitor pipelines, perform data validation.
  - Sr Engineer — Lead complex pipeline development. expectations: design scalable data systems, optimize performance.
  - Architect — Design data platform architecture. expectations: define data platform patterns, ensure reliability and security.
  - Sr Architect — Lead data platform strategy. expectations: set long-term platform roadmap and standards.
  - Principal — Technical authority for data engineering. expectations: drive innovation, mentor senior engineers.
  - Director — Manage data engineering organization. expectations: deliver platform roadmap, resource planning.
  - Sr Director — Lead multiple data engineering domains. expectations: executive alignment and strategy.
  - Managing Director — Enterprise owner of data engineering capabilities. expectations: champion data platform investments.

- Information Security:
  - Analyst — Perform security monitoring and assessments. expectations: triage alerts, perform vulnerability scans, support audits.
  - Sr Analyst — Lead investigations and security improvements. expectations: incident response, threat hunting, remediation guidance.
  - Lead — Technical lead for security initiatives. expectations: architect security controls, run security reviews.
  - Manager — Manage security team and operations. expectations: prioritize security programs, liaise with stakeholders.
  - Sr Manager — Oversee broader security operations and programs. expectations: strategy, vendor/third-party risk oversight.
  - Director — Own security strategy and compliance. expectations: set security roadmap, report to execs.
  - Sr Director — Senior leader for enterprise security. expectations: governance, cross-org risk management.
  - Managing Director — Executive responsibility for security and risk at enterprise level. expectations: define risk appetite, oversee major incidents.

- Infrastructure Engineering:
  - Engineer — Build and operate infrastructure and platform services. expectations: deploy infrastructure, automate runbooks, support incidents.
  - Sr Engineer — Lead infrastructure projects and automation. expectations: design resilient systems, performance tuning.
  - Architect — Define infrastructure and platform architecture. expectations: capacity planning, platform design.
  - Sr Architect — Lead multi-domain infrastructure architecture. expectations: guide infrastructure strategy and standards.
  - Principal — Senior technical leader for infrastructure. expectations: mentor, drive platform improvements.
  - Director — Manage infrastructure organization. expectations: roadmap, budgeting, vendor management.
  - Sr Director — Oversee global infrastructure programs. expectations: strategy, cross-region resilience.
  - Managing Director — Executive owner of infrastructure and platform strategy. expectations: align infrastructure with business continuity and growth.

- Product Delivery:
  - Associate Business Analyst — Support requirements gathering and analysis. expectations: produce user stories, assist stakeholders, document acceptance criteria.
  - Business Analyst — Drive requirements and process discovery. expectations: define workflows, validate solutions, coordinate UAT.
  - Sr Business Analyst — Lead complex analysis and stakeholder negotiations. expectations: shape product requirements, mentor analysts.
  - Product Lead — Lead delivery for a product area. expectations: coordinate teams, ensure timely delivery and quality.
  - Product Manager — Own product roadmap and outcomes. expectations: prioritize features, measure KPIs, engage stakeholders.
  - Sr Product Manager — Lead product strategy and multiple product lines. expectations: set vision, measure business impact.
  - Director — Manage product delivery organization. expectations: delivery strategy, resourcing and governance.
  - Sr Director — Oversee multiple product portfolios. expectations: strategic alignment and cross-product initiatives.
  - Managing Director — Executive responsible for product delivery across the company. expectations: deliver business outcomes and product-market fit.

- Software Engineering:
  - Intern — Assist engineering tasks and learn processes. expectations: complete small tickets, write tests, get familiar with codebase.
  - Engineer — Implement features and fix defects. expectations: deliver code, write tests, participate in reviews.
  - Sr Engineer — Lead technical features and mentoring. expectations: design components, guide engineers, improve quality.
  - Architect — Define solution-level designs and patterns. expectations: design systems, conduct design reviews.
  - Sr Architect — Lead architecture across teams. expectations: ensure technical consistency and scalability.
  - Principal — Senior technical expert driving complex solutions. expectations: lead critical initiatives, coach senior engineers.
  - Director — Manage engineering teams and delivery. expectations: people development, delivery metrics, hiring.
  - Sr Director — Oversee large engineering organizations. expectations: strategy, cross-org alignment, budgeting.
  - Managing Director — Executive owner of engineering and technology delivery. expectations: technical vision and business alignment.

- Technology Support and Operations:
  - Intern — Support day-to-day operations and learn triage. expectations: assist in tickets, document fixes, learn tooling.
  - Specialist — Provide operational or technical subject-matter expertise. expectations: own incidents, improve runbooks.
  - Sr Specialist — Lead on complex support cases and improvements. expectations: resolve escalations, mentor specialists.
  - Lead — Coordinate support teams and major incidents. expectations: incident commander duties, process improvements.
  - Manager — Manage support operations and SLAs. expectations: staffing, SLA reporting, continuous improvement.
  - Sr Manager — Oversee multiple support functions and strategy. expectations: service strategy, cross-team coordination.
  - Director — Own support strategy and large-scale operations. expectations: vendor management, operational KPIs.
  - Sr Director — Senior leader for support and operations across regions. expectations: strategy, resilience and scaling.
  - Managing Director — Executive responsibility for global support and operational excellence. expectations: drive organizational maturity and outcomes.

### project_roles (example seed)
- { id: '<uuid>', title: 'Product Manager', description: 'Drives product decisions and backlog prioritization', position_id: '<position_uuid>', created_by: '<user_uuid>', created_at: '<timestamp>' }
- { id: '<uuid>', title: 'Tech Lead', description: 'Leads technical direction for the project', position_id: '<position_uuid>', created_by: '<user_uuid>', created_at: '<timestamp>' }

### project_teams (example seed)
- { id: '<uuid>', project_id: '<project_uuid>', project_role_id: '<project_role_uuid>', employee_id: '<employee_uuid>', created_by: '<user_uuid>', created_at: '<timestamp>' }

### feedback (example seed)
- { id: '<uuid>', goal_id: '<goal_uuid>', project_id: '<project_uuid>', from_employee_id: '<employee_uuid>', to_employee_id: '<employee_uuid>', content: 'Provided helpful code review and mentored on API design.', rating: 4, visibility: 'team', created_by: '<user_uuid>', created_at: '<timestamp>' }

### feedback_requests (example seed)
- { id: '<uuid>', requestor_id: '<employee_uuid>', employee_id: '<employee_uuid>', project_id: '<project_uuid>', goal_id: '<goal_uuid>', message: 'Please review my PR and give feedback on design and tests.', due_date: '2025-10-01', created_by: '<user_uuid>', created_at: '<timestamp>' }

## INDEXES

- users: functional index on lower(user_name), index on (email)
- employee_to_skill: indexes on (employee_id), (skill_id), (skill_level_id), (effective_date); UNIQUE(employee_id, skill_id, effective_date)
- audit_logs: indexes on (actor_id), (target_type), (target_id), (created_at)
- career_paths: index on lower(title) for case-insensitive searches
- career_tracks: indexes on (career_path_id), functional index on lower(title)
- positions: indexes on (career_track_id), functional index on lower(title)
- skill_categories: functional index on lower(title)
- skills: indexes on (category_id), functional index on lower(title)
- position_to_skill: indexes on (position_id), (skill_id), (skill_level_id), UNIQUE(position_id, skill_id)
- goals: indexes on (employee_id), (related_skill_id), (deadline), (is_completed), composite (employee_id, is_completed, deadline)
- goal_tasks: indexes on (goal_id), (assigned_to), (is_completed), (deadline), composite (goal_id, order)
- departments: indexes on lower(name), code (consider unique), parent_department_id, manager_id
- locations: indexes on (name), (city), (country)
- projects: UNIQUE(code), functional index on lower(title), index on (owner_id), index on (sponsor_id)
- project_roles: functional index on lower(title), index on (position_id)
- project_teams: index on (project_id), index on (project_role_id), index on (employee_id), UNIQUE(project_id, project_role_id, employee_id)
- feedback: indexes on (goal_id), (project_id), (from_employee_id), (to_employee_id), (created_at); consider composite (to_employee_id, created_at) for recent feedback listing
- feedback_requests: index on (requestor_id), index on (employee_id), index on (project_id), index on (goal_id), index on (due_date)

- Performance & indexing notes:
  - Add GIN index on jsonb columns used for search (tags, goal_snapshot) if querying by tags.
  - Consider composite indexes for common query patterns (e.g., (to_user_id, created_at) for listing recent feedback).

## NOTES

### Data retention and privacy
- Add a configurable retention policy table or set retention via a background job that marks rows as soft-deleted and eventually purges per legal requirements.
- For anonymous feedback, store a separate pseudonymization key or redact `from_user_id` where required, while keeping an immutable audit entry for moderation actions.
- Use soft-delete (is_deleted + deleted_at) as first step; permanently purge after retention window using scheduled jobs.
- Ensure exports and deletion operations are auditable in `audit_logs` with references to affected records and actor.
- Encrypt backups and restrict access to production databases; ensure DEV copies are scrubbed of PII.

### Migrations and EF Core
- Use EF Core migrations (dotnet-ef) in the Infrastructure project; keep migrations small and reviewable.
- Keep seed data minimal: an admin user, a sample manager and sample employees for local dev; do not seed production data.
- Use a separate schema (e.g., public) and avoid cross-schema complexity initially.
- Adopt migration checks in CI (e.g., ensure migrations build and can be applied to a disposable DB) before merging schema changes.
- Store migration history in the database using EF Core's __EFMigrationsHistory table; treat migrations as code that must be reviewed in PRs.

## TABLE RELATIONSHIPS (ER Diagram)

Below is a compact ASCII ER-style diagram showing primary relationships between the main tables. Use it as a quick reference when creating DDL, migrations or entity models.

```
[users] <--- referenced by created_by/modified_by/deleted_by across most tables
   |
   +--< users.id = employees.user_id

[departments]           [locations]            [positions]
    |                       |                      |
    +--< departments.id   +--< locations.id      +--< positions.id
           ^                     ^                    |
           |                     |                    +--< position_to_skill.position_id
           |                     |                    |
        employees               projects             |
        [employees]               |  \                |
        id                          |   \              +--< position_to_skill.skill_id --> [skills] --> [skill_levels]
        |-- department_id ----------    \              |
        |-- location_id ----------       +--< project_teams.project_id
        |-- position_id -------------        |
        |-- report_to (self)                +--< project_teams.employee_id --> [employees]
        |
        +--< employee_to_skill.employee_id --> [skills]

[projects]
  |-- owner_id --> employees.id
  |-- sponsor_id --> employees.id
  +--< project_teams.project_id --> project_roles

[project_roles]
  |-- position_id --> positions.id

[goals]
  |-- employee_id --> employees.id
  +--< goal_tasks.goal_id
  +--< feedback.goal_id

[goal_tasks] -- simple child records for goals

[feedback]
  |-- goal_id --> goals.id
  |-- project_id --> projects.id (optional)
  |-- project_role_id --> project_roles.id (optional)
  |-- from_employee_id --> employees.id
  |-- to_employee_id --> employees.id

[position_to_skill] maps positions -> skills -> skill_levels (target)
[employee_to_skill] maps employees -> skills (observed/target)

[audit_logs] references users(actor_id) and records target_type/target_id for append-only audit trail

Legend:
- Arrow direction shows FK: source_column --> referenced_table.column
- Tables not shown in full are still part of the model (skill_categories, career_paths, career_tracks, etc.) and link to the primary tables above.
- created_by / modified_by / deleted_by typically reference `users(id)` across the schema.
- Soft-delete fields (is_deleted, deleted_at) exist on most tables; consider partial indexes WHERE is_deleted = false for active-row queries.
```