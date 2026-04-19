# Property Management Platform

This repository contains a multi-service property management platform made up of:

- A `.NET 10` backend API
- A `FastAPI` orchestration service for workflow automation
- An `Angular` frontend
- A local `TiDB` cluster running in Docker
- A `Redis` job store for persistent workflow status

The easiest way to run the full stack locally is with Docker Compose.

## Repository Layout

- `backend/`: .NET API and domain/application/infrastructure projects
- `frontend/property-mgmt-app/`: Angular application served by nginx in Docker
- `orchestration/`: FastAPI orchestration service and workflow agents
- `docker-compose.yml`: local development stack
- `docs/architecture.md`: overall system architecture and Mermaid diagram

## Prerequisites

Install the following before you start:

- `Git`
- `Docker Desktop` or Docker Engine with Compose support
- `Ollama` running on your machine if you want the orchestration workflows to execute end-to-end

Recommended checks:

```bash
git --version
docker --version
docker compose version
ollama --version
```

## Clone The Repository

If you do not have the code yet:

```bash
git clone <your-repository-url>
cd property-management
```

If you already have the repo and want the latest changes:

```bash
git pull origin <your-branch>
```

Replace `<your-repository-url>` and `<your-branch>` with the values used by your team.

## Local Services And Ports

When the stack is running, these endpoints are exposed locally:

- Frontend: `http://localhost:4200`
- .NET API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- Orchestration API: `http://localhost:8000`
- TiDB SQL port: `localhost:4000`
- TiDB status page: `http://localhost:10080/status`
- Redis: `localhost:6379`

## Run Locally With Docker

From the repository root:

```bash
docker compose up --build
```

This command:

- Builds the backend, orchestration, and frontend images
- Starts a local TiDB stack
- Starts the .NET API on port `5000`
- Starts the orchestration service on port `8000`
- Starts the Angular frontend behind nginx on port `4200`

To run in the background:

```bash
docker compose up --build -d
```

To stop everything:

```bash
docker compose down
```

## Ollama Requirement

The orchestration container is configured to call Ollama on your host using:

- `OLLAMA_BASE_URL=http://host.docker.internal:11434`

The default models in [`orchestration/config.py`](orchestration/config.py) are:

- `qwen2.5:14b`
- `qwen2.5:3b`

Before using workflow endpoints, make sure Ollama is running locally and the models are available:

```bash
ollama serve
ollama pull qwen2.5:14b
ollama pull qwen2.5:3b
```

If Ollama is not running, the frontend and backend can still start, but workflow execution in the orchestration service may fail.

## Authentication

The API now uses JWT bearer authentication for controller endpoints. For local development, the repository includes a development-only login bootstrap.

Default local credentials:

- Username: `admin@property.local`
- Password: `Passw0rd!`

Login endpoint:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@property.local","password":"Passw0rd!"}'
```

The Angular app uses the same credentials on the `/login` page by default. You can override them for Docker runs using the values in `.env.example`.

## First-Run Notes

The .NET API initializes the database schema on startup. If startup fails during schema creation and leaves TiDB in a partial state, reset the local volumes and start again:

```bash
docker compose down -v
docker compose up --build
```

Use `-v` only when you are comfortable deleting local TiDB data for this project.

## Useful Commands

View logs for all services:

```bash
docker compose logs -f
```

View logs for one service:

```bash
docker compose logs -f dotnet-api
docker compose logs -f orchestration
docker compose logs -f frontend
docker compose logs -f tidb
```

Rebuild only one service:

```bash
docker compose build dotnet-api
docker compose build orchestration
docker compose build frontend
```

Start only one service after rebuilding:

```bash
docker compose up dotnet-api
```

## Developer Workflow

Typical local workflow:

1. Pull the latest code with `git pull`.
2. Make sure Docker and Ollama are running.
3. Start the stack with `docker compose up --build`.
4. Open the frontend at `http://localhost:4200`.
5. Use Swagger at `http://localhost:5000/swagger` to inspect or test API endpoints.
6. Use the orchestration API at `http://localhost:8000` for workflow jobs.

## Sample Workflow Test Data

The database seed creates enough records to exercise each workflow, but the IDs are generated at runtime, so they will be different on every fresh database.

Use the following API queries to find valid seeded records before triggering workflows.

### Lease Renewal

Find leases expiring soon (defaults to within 60 days; adjust with `?withinDays=N`):

```bash
curl http://localhost:5000/api/leases/expiring
curl http://localhost:5000/api/leases/expiring?withinDays=30
```

Pick any returned lease `id`, then trigger the workflow:

```bash
curl -X POST http://localhost:8000/workflows/lease-renewal \
  -H "Content-Type: application/json" \
  -d '{"lease_id":"<lease-id>"}'
```

### Maintenance

Find urgent open maintenance requests:

```bash
curl http://localhost:5000/api/maintenance/open/Urgent
```

Or emergency requests:

```bash
curl http://localhost:5000/api/maintenance/open/Emergency
```

Pick any returned maintenance request `id`, then trigger the workflow:

```bash
curl -X POST http://localhost:8000/workflows/maintenance \
  -H "Content-Type: application/json" \
  -d '{"request_id":"<request-id>"}'
```

### Rent Collection

Check the seeded overdue payments:

```bash
curl http://localhost:5000/api/payments/overdue
```

Then run the workflow:

```bash
curl -X POST http://localhost:8000/workflows/rent-collection \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Onboarding

List leases and look for a draft lease, then note its `tenantId` and lease `id`:

```bash
curl http://localhost:5000/api/leases
```

You can also list tenants if you want to confirm the tenant details:

```bash
curl http://localhost:5000/api/tenants
```

Trigger the workflow with the lease `id` and matching `tenantId`:

```bash
curl -X POST http://localhost:8000/workflows/onboarding \
  -H "Content-Type: application/json" \
  -d '{"tenant_id":"<tenant-id>","lease_id":"<lease-id>"}'
```

### Inspection

List inspections and pick one with useful `notes` and a non-null `leaseId` if you want maintenance tickets to be created:

```bash
curl http://localhost:5000/api/inspections
```

Trigger the workflow:

```bash
curl -X POST http://localhost:8000/workflows/inspection \
  -H "Content-Type: application/json" \
  -d '{"inspection_id":"<inspection-id>"}'
```

### Supervisor (Multi-Agent)

The supervisor accepts a plain-language request and a context object containing any relevant IDs. It uses Ollama to decide which specialist agents to invoke, runs them in parallel, and returns a combined summary.

Find the IDs you need from the API first:

```bash
# get a lease expiring soon
curl http://localhost:5000/api/leases/expiring

# get an open maintenance request
curl http://localhost:5000/api/maintenance/open/Urgent

# get a scheduled inspection
curl http://localhost:5000/api/inspections/scheduled
```

Trigger the supervisor with a natural-language request and any available IDs:

```bash
curl -X POST http://localhost:8000/workflows/supervisor \
  -H "Content-Type: application/json" \
  -d '{
    "request": "A new tenant is moving in and there is an urgent maintenance issue to resolve",
    "context": {
      "tenant_id": "<tenant-id>",
      "lease_id": "<lease-id>",
      "maintenance_request_id": "<maintenance-request-id>"
    }
  }'
```

The supervisor will select the appropriate agents (`onboarding` and `maintenance` in this case), run them in parallel, and return a combined summary. You can omit IDs for agents you do not need — the supervisor will only invoke agents for which the required IDs are present.

### Check Workflow Status

Every orchestration workflow returns a `job_id`. Use it to poll the result:

```bash
curl http://localhost:8000/jobs/<job-id>
```

## Architecture

See [`docs/architecture.md`](docs/architecture.md) for the overall project architecture and Mermaid diagram.

## Roadmap

See [`docs/implementation-roadmap.md`](docs/implementation-roadmap.md) for the prioritized implementation plan and suggested build order.

## Troubleshooting

`dotnet-api` fails during startup:

- Check `docker compose logs -f dotnet-api`
- Check TiDB health with `http://localhost:10080/status`
- If schema creation partially succeeded, reset local volumes with `docker compose down -v`

`orchestration` cannot complete jobs:

- Confirm Ollama is running on the host
- Confirm the required models are pulled
- Check `docker compose logs -f orchestration`

Frontend is up but API calls fail:

- Confirm `dotnet-api` is healthy on `http://localhost:5000/swagger`
- Confirm CORS is serving the frontend from `http://localhost:4200`
