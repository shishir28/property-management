import asyncio
from typing import TypedDict
from langgraph.graph import StateGraph, END
from langchain_core.messages import HumanMessage
from agents.base import get_llm
from agents.lease_renewal import lease_renewal_graph, LeaseRenewalState
from agents.maintenance import maintenance_graph, MaintenanceState
from agents.rent_collection import rent_collection_graph, RentCollectionState
from agents.onboarding import onboarding_graph, OnboardingState
from agents.inspection import inspection_graph, InspectionState

AVAILABLE_AGENTS = ["lease_renewal", "maintenance", "rent_collection", "onboarding", "inspection"]


class SupervisorState(TypedDict):
    request: str
    context: dict
    discovered_context: dict
    selected_agents: list[str]
    agent_results: dict
    summary: str
    error: str


async def discover_context(state: SupervisorState) -> SupervisorState:
    try:
        ctx = dict(state.get("context", {}))

        expiring_task = api_client.get_expiring_leases()
        leases_task = api_client.get_leases()
        urgent_task = api_client.get_open_maintenance("Urgent")
        emergency_task = api_client.get_open_maintenance("Emergency")
        inspections_task = api_client.get_inspections()
        overdue_task = api_client.get_overdue_payments()

        expiring_leases, leases, urgent_requests, emergency_requests, inspections, overdue_payments = await asyncio.gather(
            expiring_task,
            leases_task,
            urgent_task,
            emergency_task,
            inspections_task,
            overdue_task
        )

        draft_leases = [lease for lease in leases if lease.get("status") == "Draft"]
        actionable_inspections = [
            inspection for inspection in inspections
            if inspection.get("notes") or inspection.get("leaseId")
        ]
        high_priority_requests = emergency_requests + [
            request for request in urgent_requests
            if request.get("id") not in {item.get("id") for item in emergency_requests}
        ]

        discovered = {
            "expiring_leases": expiring_leases[:5],
            "draft_leases": draft_leases[:5],
            "maintenance_requests": high_priority_requests[:5],
            "actionable_inspections": actionable_inspections[:5],
            "overdue_payments": overdue_payments[:5]
        }

        if not ctx.get("lease_id") and expiring_leases:
            ctx["lease_id"] = expiring_leases[0].get("id")
        if not ctx.get("maintenance_request_id") and high_priority_requests:
            ctx["maintenance_request_id"] = high_priority_requests[0].get("id")
        if not ctx.get("inspection_id") and actionable_inspections:
            ctx["inspection_id"] = actionable_inspections[0].get("id")
        if not ctx.get("tenant_id") and draft_leases:
            ctx["tenant_id"] = draft_leases[0].get("tenantId")
            ctx.setdefault("lease_id", draft_leases[0].get("id"))

        return {**state, "context": ctx, "discovered_context": discovered}
    except Exception as e:
        return {**state, "error": str(e)}


async def classify_intent(state: SupervisorState) -> SupervisorState:
    try:
        ctx = state.get("context", {})
        discovered = state.get("discovered_context", {})
        available_ids = [
            f"lease_id: {ctx['lease_id']}" if ctx.get("lease_id") else None,
            f"tenant_id: {ctx['tenant_id']}" if ctx.get("tenant_id") else None,
            f"maintenance_request_id: {ctx['maintenance_request_id']}" if ctx.get("maintenance_request_id") else None,
            f"inspection_id: {ctx['inspection_id']}" if ctx.get("inspection_id") else None,
        ]
        ids_text = ", ".join(x for x in available_ids if x) or "none provided"

        expiring_text = _format_records(
            discovered.get("expiring_leases", []),
            lambda lease: f"{lease['id']} | unit {lease['unitNumber']} | end {lease['endDate']} | rent {lease['monthlyRent']}"
        )
        draft_text = _format_records(
            discovered.get("draft_leases", []),
            lambda lease: f"{lease['id']} | tenant {lease['tenantId']} | unit {lease['unitNumber']} | status {lease['status']}"
        )
        maintenance_text = _format_records(
            discovered.get("maintenance_requests", []),
            lambda request: f"{request['id']} | {request['priority']} | {request['title']} | status {request['status']}"
        )
        inspection_text = _format_records(
            discovered.get("actionable_inspections", []),
            lambda inspection: f"{inspection['id']} | {inspection['type']} | lease {inspection.get('leaseId')} | status {inspection['status']}"
        )
        overdue_text = _format_records(
            discovered.get("overdue_payments", []),
            lambda payment: f"{payment['id']} | lease {payment['leaseId']} | amount {payment['amount']} | due {payment['dueDate']}"
        )

        prompt = f"""You are a property management workflow supervisor.

Available agents and their required context:
- lease_renewal: renewal analysis and notice drafting (requires lease_id)
- maintenance: triage and contractor assignment (requires maintenance_request_id)
- rent_collection: overdue payment reminders and escalations (requires no ID)
- onboarding: welcome message and move-in checklist for a new tenant (requires tenant_id and lease_id)
- inspection: analyse inspection notes and create maintenance tickets (requires inspection_id)

User request: {state['request']}
Available context IDs: {ids_text}

Discovered candidates from the API:
Expiring leases:
{expiring_text}

Draft leases:
{draft_text}

High priority maintenance requests:
{maintenance_text}

Actionable inspections:
{inspection_text}

Overdue payments:
{overdue_text}

Use the discovered candidates to fill in missing IDs when they clearly match the request.
Select only agents that are relevant to the request.
Reply in exactly this format:
AGENTS: <comma-separated agent names, or NONE>
LEASE_ID: <id or NONE>
TENANT_ID: <id or NONE>
MAINTENANCE_REQUEST_ID: <id or NONE>
INSPECTION_ID: <id or NONE>
REASON: <one sentence>"""

        llm = get_llm(fast=True)
        response = await llm.ainvoke([HumanMessage(content=prompt)])
        selected = []
        resolved_ids = {}
        for line in response.content.strip().splitlines():
            if line.startswith("AGENTS:"):
                raw = line.replace("AGENTS:", "").strip()
                if raw.upper() != "NONE":
                    selected = [a.strip() for a in raw.split(",") if a.strip() in AVAILABLE_AGENTS]
            elif line.startswith("LEASE_ID:"):
                value = line.replace("LEASE_ID:", "").strip()
                if value.upper() != "NONE":
                    resolved_ids["lease_id"] = value
            elif line.startswith("TENANT_ID:"):
                value = line.replace("TENANT_ID:", "").strip()
                if value.upper() != "NONE":
                    resolved_ids["tenant_id"] = value
            elif line.startswith("MAINTENANCE_REQUEST_ID:"):
                value = line.replace("MAINTENANCE_REQUEST_ID:", "").strip()
                if value.upper() != "NONE":
                    resolved_ids["maintenance_request_id"] = value
            elif line.startswith("INSPECTION_ID:"):
                value = line.replace("INSPECTION_ID:", "").strip()
                if value.upper() != "NONE":
                    resolved_ids["inspection_id"] = value

        merged_context = {**ctx, **resolved_ids}
        return {**state, "context": merged_context, "selected_agents": selected}
    except Exception as e:
        return {**state, "error": str(e)}


async def dispatch_agents(state: SupervisorState) -> SupervisorState:
    if state.get("error") or not state.get("selected_agents"):
        return {**state, "agent_results": {}}

    ctx = state.get("context", {})
    tasks: dict[str, object] = {}

    for agent in state["selected_agents"]:
        if agent == "lease_renewal" and ctx.get("lease_id"):
            s: LeaseRenewalState = {"lease_id": ctx["lease_id"], "lease": {}, "tenant": {},
                                    "compliance_notes": "", "renewal_notice": "", "suggested_rent": 0.0, "error": ""}
            tasks[agent] = lease_renewal_graph.ainvoke(s)

        elif agent == "maintenance" and ctx.get("maintenance_request_id"):
            s: MaintenanceState = {"request_id": ctx["maintenance_request_id"], "request": {},
                                   "triage_result": "", "recommended_contractor": "", "assigned": False, "error": ""}
            tasks[agent] = maintenance_graph.ainvoke(s)

        elif agent == "rent_collection":
            s: RentCollectionState = {"overdue_payments": [], "reminders": [], "escalations": [], "error": ""}
            tasks[agent] = rent_collection_graph.ainvoke(s)

        elif agent == "onboarding" and ctx.get("tenant_id") and ctx.get("lease_id"):
            s: OnboardingState = {"tenant_id": ctx["tenant_id"], "lease_id": ctx["lease_id"],
                                  "tenant": {}, "lease": {}, "welcome_message": "", "move_in_checklist": "", "error": ""}
            tasks[agent] = onboarding_graph.ainvoke(s)

        elif agent == "inspection" and ctx.get("inspection_id"):
            s: InspectionState = {"inspection_id": ctx["inspection_id"], "inspection": {},
                                  "checklist": "", "findings": [], "maintenance_tickets_created": 0, "error": ""}
            tasks[agent] = inspection_graph.ainvoke(s)

    if not tasks:
        return {**state, "agent_results": {}}

    results = await asyncio.gather(*tasks.values(), return_exceptions=True)
    agent_results = {}
    for name, result in zip(tasks.keys(), results):
        if isinstance(result, Exception):
            agent_results[name] = {"error": str(result)}
        else:
            agent_results[name] = _extract_result(name, result)

    return {**state, "agent_results": agent_results}


def _extract_result(agent: str, result: dict) -> dict:
    if result.get("error"):
        return {"error": result["error"]}
    if agent == "lease_renewal":
        return {"renewal_notice": result.get("renewal_notice"),
                "suggested_rent": result.get("suggested_rent"),
                "compliance_notes": result.get("compliance_notes")}
    if agent == "maintenance":
        return {"triage_result": result.get("triage_result"),
                "recommended_contractor": result.get("recommended_contractor"),
                "assigned": result.get("assigned")}
    if agent == "rent_collection":
        return {"reminders_sent": len(result.get("reminders", [])),
                "escalations": len(result.get("escalations", []))}
    if agent == "onboarding":
        return {"welcome_message": result.get("welcome_message"),
                "move_in_checklist": result.get("move_in_checklist")}
    if agent == "inspection":
        return {"checklist": result.get("checklist"),
                "findings": result.get("findings"),
                "tickets_created": result.get("maintenance_tickets_created")}
    return result


async def aggregate_results(state: SupervisorState) -> SupervisorState:
    if state.get("error"):
        return state

    results = state.get("agent_results", {})
    if not results:
        return {**state, "summary": "No agents were dispatched. Ensure the required IDs are present in context."}

    lines = []
    for agent, result in results.items():
        if result.get("error"):
            lines.append(f"{agent}: FAILED — {result['error']}")
        elif agent == "lease_renewal":
            lines.append(f"lease_renewal: renewal notice drafted, suggested rent ${result.get('suggested_rent')}")
        elif agent == "maintenance":
            lines.append(f"maintenance: triaged, contractor '{result.get('recommended_contractor')}', assigned={result.get('assigned')}")
        elif agent == "rent_collection":
            lines.append(f"rent_collection: {result.get('reminders_sent')} reminders, {result.get('escalations')} escalations")
        elif agent == "onboarding":
            lines.append("onboarding: welcome message and move-in checklist generated")
        elif agent == "inspection":
            lines.append(f"inspection: checklist generated, {result.get('tickets_created')} maintenance tickets created")

    llm = get_llm(fast=True)
    prompt = f"""You are a property management supervisor. Summarise these workflow outcomes in 2-3 sentences.

Original request: {state['request']}
Resolved context: {state.get('context', {})}
Outcomes:
{chr(10).join(lines)}

Write a concise operational summary."""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    return {**state, "summary": response.content}


def has_error(state: SupervisorState) -> str:
    return "error" if state.get("error") else "continue"


def _format_records(records: list[dict], formatter) -> str:
    if not records:
        return "- none"
    return "\n".join(f"- {formatter(record)}" for record in records)


def build_supervisor_graph() -> StateGraph:
    graph = StateGraph(SupervisorState)
    graph.add_node("discover_context", discover_context)
    graph.add_node("classify_intent", classify_intent)
    graph.add_node("dispatch_agents", dispatch_agents)
    graph.add_node("aggregate_results", aggregate_results)

    graph.set_entry_point("discover_context")
    graph.add_conditional_edges("discover_context", has_error, {"error": END, "continue": "classify_intent"})
    graph.add_conditional_edges("classify_intent", has_error, {"error": END, "continue": "dispatch_agents"})
    graph.add_conditional_edges("dispatch_agents", has_error, {"error": END, "continue": "aggregate_results"})
    graph.add_edge("aggregate_results", END)

    return graph.compile()


supervisor_graph = build_supervisor_graph()
