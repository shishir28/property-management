import asyncio
import uuid
from contextlib import asynccontextmanager
from fastapi import FastAPI, BackgroundTasks, HTTPException
from pydantic import BaseModel
from redis.asyncio import Redis

from agents.lease_renewal import lease_renewal_graph, LeaseRenewalState
from agents.maintenance import maintenance_graph, MaintenanceState
from agents.rent_collection import rent_collection_graph, RentCollectionState
from agents.onboarding import onboarding_graph, OnboardingState
from agents.inspection import inspection_graph, InspectionState
from agents.supervisor import supervisor_graph, SupervisorState
from config import settings
from job_store import JobStore


@asynccontextmanager
async def lifespan(app: FastAPI):
    redis = Redis.from_url(settings.redis_url, decode_responses=True)
    await redis.ping()
    app.state.job_store = JobStore(redis, settings.job_ttl_seconds)
    print("Orchestration service ready.")
    try:
        yield
    finally:
        await redis.aclose()

app = FastAPI(title="Property Management Orchestration", lifespan=lifespan)


# ── Request models ────────────────────────────────────────────────────────

class LeaseRenewalRequest(BaseModel):
    lease_id: str

class MaintenanceRequest(BaseModel):
    request_id: str

class RentCollectionRequest(BaseModel):
    triggered_at: str | None = None

class OnboardingRequest(BaseModel):
    tenant_id: str
    lease_id: str

class InspectionRequest(BaseModel):
    inspection_id: str

class SupervisorRequest(BaseModel):
    request: str
    context: dict = {}


async def _store_result(app: FastAPI, job_id: str, result: dict) -> None:
    await app.state.job_store.set_result(job_id, result)


async def _store_error(app: FastAPI, job_id: str, error: str) -> None:
    await app.state.job_store.set_error(job_id, error)


# ── Background runners ────────────────────────────────────────────────────

async def _run_lease_renewal(app: FastAPI, job_id: str, lease_id: str):
    try:
        state: LeaseRenewalState = {"lease_id": lease_id, "lease": {}, "tenant": {},
                                    "compliance_notes": "", "renewal_notice": "",
                                    "suggested_rent": 0.0, "error": ""}
        result = await lease_renewal_graph.ainvoke(state)
        await _store_result(app, job_id, {"renewal_notice": result.get("renewal_notice"),
                                          "suggested_rent": result.get("suggested_rent"),
                                          "compliance_notes": result.get("compliance_notes")})
    except Exception as e:
        await _store_error(app, job_id, str(e))


async def _run_maintenance(app: FastAPI, job_id: str, request_id: str):
    try:
        state: MaintenanceState = {"request_id": request_id, "request": {},
                                   "triage_result": "", "recommended_contractor": "",
                                   "assigned": False, "error": ""}
        result = await maintenance_graph.ainvoke(state)
        await _store_result(app, job_id, {"triage_result": result.get("triage_result"),
                                          "contractor": result.get("recommended_contractor"),
                                          "assigned": result.get("assigned")})
    except Exception as e:
        await _store_error(app, job_id, str(e))


async def _run_rent_collection(app: FastAPI, job_id: str):
    try:
        state: RentCollectionState = {"overdue_payments": [], "reminders": [],
                                      "escalations": [], "error": ""}
        result = await rent_collection_graph.ainvoke(state)
        await _store_result(app, job_id, {"reminders_sent": len(result.get("reminders", [])),
                                          "escalations": len(result.get("escalations", []))})
    except Exception as e:
        await _store_error(app, job_id, str(e))


async def _run_onboarding(app: FastAPI, job_id: str, tenant_id: str, lease_id: str):
    try:
        state: OnboardingState = {"tenant_id": tenant_id, "lease_id": lease_id,
                                  "tenant": {}, "lease": {}, "welcome_message": "",
                                  "move_in_checklist": "", "error": ""}
        result = await onboarding_graph.ainvoke(state)
        await _store_result(app, job_id, {"welcome_message": result.get("welcome_message"),
                                          "checklist": result.get("move_in_checklist")})
    except Exception as e:
        await _store_error(app, job_id, str(e))


async def _run_supervisor(app: FastAPI, job_id: str, request: str, context: dict):
    try:
        state: SupervisorState = {"request": request, "context": context,
                                  "discovered_context": {},
                                  "selected_agents": [], "agent_results": {},
                                  "summary": "", "error": ""}
        result = await supervisor_graph.ainvoke(state)
        await _store_result(app, job_id, {"selected_agents": result.get("selected_agents"),
                                          "resolved_context": result.get("context"),
                                          "agent_results": result.get("agent_results"),
                                          "summary": result.get("summary")})
    except Exception as e:
        await _store_error(app, job_id, str(e))


async def _run_inspection(app: FastAPI, job_id: str, inspection_id: str):
    try:
        state: InspectionState = {"inspection_id": inspection_id, "inspection": {},
                                  "checklist": "", "findings": [],
                                  "maintenance_tickets_created": 0, "error": ""}
        result = await inspection_graph.ainvoke(state)
        await _store_result(app, job_id, {"checklist": result.get("checklist"),
                                          "findings": result.get("findings"),
                                          "tickets_created": result.get("maintenance_tickets_created")})
    except Exception as e:
        await _store_error(app, job_id, str(e))


# ── Endpoints ─────────────────────────────────────────────────────────────

@app.post("/workflows/lease-renewal", status_code=202)
async def trigger_lease_renewal(req: LeaseRenewalRequest, bg: BackgroundTasks):
    job_id = str(uuid.uuid4())
    await app.state.job_store.set_running(job_id)
    bg.add_task(_run_lease_renewal, app, job_id, req.lease_id)
    return {"job_id": job_id, "status": "accepted"}


@app.post("/workflows/maintenance", status_code=202)
async def trigger_maintenance(req: MaintenanceRequest, bg: BackgroundTasks):
    job_id = str(uuid.uuid4())
    await app.state.job_store.set_running(job_id)
    bg.add_task(_run_maintenance, app, job_id, req.request_id)
    return {"job_id": job_id, "status": "accepted"}


@app.post("/workflows/rent-collection", status_code=202)
async def trigger_rent_collection(req: RentCollectionRequest, bg: BackgroundTasks):
    job_id = str(uuid.uuid4())
    await app.state.job_store.set_running(job_id)
    bg.add_task(_run_rent_collection, app, job_id)
    return {"job_id": job_id, "status": "accepted"}


@app.post("/workflows/onboarding", status_code=202)
async def trigger_onboarding(req: OnboardingRequest, bg: BackgroundTasks):
    job_id = str(uuid.uuid4())
    await app.state.job_store.set_running(job_id)
    bg.add_task(_run_onboarding, app, job_id, req.tenant_id, req.lease_id)
    return {"job_id": job_id, "status": "accepted"}


@app.post("/workflows/inspection", status_code=202)
async def trigger_inspection(req: InspectionRequest, bg: BackgroundTasks):
    job_id = str(uuid.uuid4())
    await app.state.job_store.set_running(job_id)
    bg.add_task(_run_inspection, app, job_id, req.inspection_id)
    return {"job_id": job_id, "status": "accepted"}


@app.post("/workflows/supervisor", status_code=202)
async def trigger_supervisor(req: SupervisorRequest, bg: BackgroundTasks):
    job_id = str(uuid.uuid4())
    await app.state.job_store.set_running(job_id)
    bg.add_task(_run_supervisor, app, job_id, req.request, req.context)
    return {"job_id": job_id, "status": "accepted"}


@app.get("/jobs/{job_id}")
async def get_job(job_id: str):
    job = await app.state.job_store.get(job_id)
    if not job:
        raise HTTPException(status_code=404, detail="Job not found")
    return job


@app.get("/health")
async def health():
    return {"status": "ok"}
