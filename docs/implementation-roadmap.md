# Implementation Roadmap

This roadmap turns the suggested build order into an execution plan for the property-management platform.

## Principles

- Build foundational capabilities first
- Prefer end-to-end usability over isolated backend work
- Sequence features so later work can assume earlier contracts exist
- Keep infrastructure changes small and demonstrable

## Recommended Order

### 1. JWT Authentication

Why first:

- The frontend workflow UI should assume authenticated access exists
- CI and future environment promotion will be easier if auth is already part of the API contract
- It establishes the security model for everything that follows

Scope:

- Add JWT authentication to the .NET API
- Protect workflow-triggering and sensitive data endpoints
- Add login or token bootstrap flow for local development
- Update frontend API client to send bearer tokens
- Document auth setup in `README.md`

Definition of done:

- API endpoints require valid tokens where appropriate
- Frontend can authenticate and call protected endpoints
- Local Docker workflow supports acquiring a usable token

### 2. Frontend Workflow UI

Why second:

- The system becomes usable end-to-end without `curl`
- It provides a visible place to trigger workflows and inspect results
- It becomes the proving ground for later supervisor and error-handling work

Scope:

- Add a dedicated Workflows page in the Angular app
- Allow users to trigger lease renewal, maintenance, rent collection, onboarding, and inspection jobs
- Show job submission state and returned `job_id`
- Add polling for job completion using `/jobs/{job_id}`

Definition of done:

- A user can trigger every workflow from the frontend
- A user can see job status transitions without using the command line

### 3. Supervisor Context Discovery

Why third:

- Once a workflow UI exists, supervisor-driven context becomes visible and testable
- It upgrades orchestration quality without blocking the UI itself

Scope:

- Enhance the supervisor agent to query the .NET API directly for context
- Use domain context to decide routing or enrich downstream prompts
- Avoid hardcoded assumptions when building workflow state

Definition of done:

- Supervisor can fetch relevant domain data before delegating
- Routing or enrichment decisions use live backend data

### 4. Inspection Trigger On Complete

Why fourth:

- Small change, high demo value
- Helps show a complete inspection lifecycle from scheduling through follow-up actions

Scope:

- Trigger downstream orchestration or maintenance follow-up when an inspection is completed
- Ensure completion notes can flow into the inspection workflow automatically

Definition of done:

- Completing an inspection can automatically kick off the next useful workflow step

### 5. Persistent Job Store With Redis

Why fifth:

- The current in-memory job store is simple but fragile
- Redis is self-contained and easy to add to Docker Compose
- It makes workflow status durable across orchestration restarts

Scope:

- Add Redis to `docker-compose.yml`
- Replace the in-memory job dictionary in the orchestration service
- Store job status, results, and failures in Redis

Definition of done:

- Jobs survive orchestration service restarts
- Polling still works with the same API contract

### 6. Error Surfacing In Frontend

Why sixth:

- Depends on the workflow UI existing first
- Becomes more valuable once background jobs are durable and asynchronous

Scope:

- Show failed job states clearly in the frontend
- Display useful error messages and next actions
- Differentiate validation failures, orchestration failures, and API failures

Definition of done:

- A failed workflow is obvious in the UI
- Users can see what failed without checking container logs

### 7. Pagination

Why seventh:

- Important for production readiness, but not on the critical path for demonstrating workflow orchestration
- Can be implemented incrementally without blocking earlier milestones

Scope:

- Add pagination parameters to list endpoints in the .NET API
- Update frontend list views to handle paged responses
- Preserve sorting and filtering behavior where needed

Definition of done:

- Large datasets do not require full-table fetches in the UI

### 8. Unit Tests

Why eighth:

- Valuable throughout the project, but most effective after the main behavior has stabilized a bit
- Should focus on domain rules and MediatR handlers first

Scope:

- Add domain tests for lease, payment, maintenance, inspection, and tenant rules
- Add handler tests for application-layer commands and queries
- Focus on business rules before UI tests

Definition of done:

- Core business rules have repeatable automated coverage
- Main application handlers have baseline regression protection

### 9. GitHub Actions CI

Why ninth:

- CI provides the most value once there are tests to run
- Avoid shipping a pipeline that only proves the repo compiles

Scope:

- Add GitHub Actions workflow for `dotnet build` and `dotnet test`
- Optionally add frontend build validation
- Run on push and pull request

Definition of done:

- Every push validates the backend build and tests automatically

## Suggested Milestones

### Milestone 1: Secure And Usable

- JWT Authentication
- Frontend Workflow UI

Outcome:

- Users can sign in and run workflows from the browser

### Milestone 2: Smarter Orchestration

- Supervisor context discovery
- Inspection trigger on complete

Outcome:

- The orchestration layer becomes more autonomous and more connected to the business lifecycle

### Milestone 3: Durable Operations

- Persistent job store with Redis
- Error surfacing in frontend

Outcome:

- Long-running workflows become more reliable and easier to operate

### Milestone 4: Hardening

- Pagination
- Unit tests
- GitHub Actions CI

Outcome:

- The platform becomes easier to maintain and safer to evolve

## Immediate Next Action

Start with JWT authentication and treat it as the foundation for the next several changes.

The practical implementation sequence should be:

1. Add JWT auth to the .NET API
2. Add frontend login/token handling
3. Protect workflow-relevant endpoints
4. Update Docker-based local developer instructions
5. Then begin the Workflows page
