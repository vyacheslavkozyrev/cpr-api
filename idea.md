### PROMT 1: I want to describe my idea here. Please help me to structure my idea in the open file idea.md so we can use it in the future. My project is set of REST API endpoints to communicate with SQL data base. Main purpose of this project is to build a platform to help organizations with employee performance review process. To minimize workload on people leaders during mid year and end year season this platform will collect feedback about employee continuesly, help people leader to set employee goals and help track that goals. During performance review this platform will help people leader to summarize all feedbacks, goals results and summurize performance review results.

### PROMT 2: Please limit this file to overall project idea description listing general functionalities and flows. Structure of the file should be Overview of the project, Objectives, Stakeholders, TechStack, NFR, Rists and Open Questions

<!-- filepath: d:\projects\CPR\idea.md -->

# CPR — Continuous Performance Review (project idea)

## Overview
A lightweight platform of REST API endpoints backed by a SQL database to support employee performance reviews. The system continuously collects feedback, enables managers to create and track goals, and aggregates feedback and goal outcomes into concise review summaries to reduce manual work during mid-year and end-of-year review cycles.

## Objectives
- Continuously collect peer and manager feedback
- Enable goal creation, assignment, progress updates, and completion tracking
- Aggregate feedback and goal snapshots into review-ready summaries
- Provide exportable reports and audit trails for HR and compliance

## Stakeholders
- People leaders (managers)
- Employees
- HR administrators
- Solution owners

## TechStack (suggested)
- REST API: ASP.NET Core (C#) — Web API using ASP.NET Core, recommended use of EF Core for data access and migrations
- Database: PostgreSQL
- Auth: OAuth2 / OpenID Connect + JWT for service APIs
- Deployment: Docker containers, CI/CD pipeline (e.g., GitHub Actions)

## Non-functional requirements
- Authentication & RBAC
- Encryption in transit and at rest
- Input validation and rate limiting
- Pagination and indexes for performance
- Configurable data retention and export for compliance (e.g., GDPR)
- Observability: logs, metrics, error tracking, and backups

## Risks & Open Questions
- Anonymous feedback abuse and moderation strategy
- Access boundaries for cross-team or skip-level feedback
- How to weight qualitative feedback vs. goal outcomes in final summaries
- Retention policy, legal requirements, and export formats