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
    selected_agents: list[str]
    agent_results: dict
    summary: str
    error: str


async def classify_intent(state: SupervisorState) -> SupervisorState:
    try:
        ctx = state.get("context", {})
        available_ids = [
            f"lease_id: {ctx['lease_id']}" if ctx.get("lease_id") else None,
            f"tenant_id: {ctx['tenant_id']}" if ctx.get("tenant_id") else None,
            f"maintenance_request_id: {ctx['maintenance_request_id']}" if ctx.get("maintenance_request_id") else None,
            f"inspection_id: {ctx['inspection_id']}" if ctx.get("inspection_id") else None,
        ]
        ids_text = ", ".join(x for x in available_ids if x) or "none provided"

        prompt = f"""You are a property management workflow supervisor.

Available agents and their required context:
- lease_renewal: renewal analysis and notice drafting (requires lease_id)
- maintenance: triage and contractor assignment (requires maintenance_request_id)
- rent_collection: overdue payment reminders and escalations (requires no ID)
- onboarding: welcome message and move-in checklist for a new tenant (requires tenant_id and lease_id)
- inspection: analyse inspection notes and create maintenance tickets (requires inspection_id)

User request: {state['request']}
Available context IDs: {ids_text}

Select only agents for which the required IDs are present (rent_collection needs none).
Reply in exactly this format:
AGENTS: <comma-separated agent names, or NONE>
REASON: <one sentence>"""

        llm = get_llm(fast=True)
        response = await llm.ainvoke([HumanMessage(content=prompt)])
        selected = []
        for line in response.content.strip().splitlines():
            if line.startswith("AGENTS:"):
                raw = line.replace("AGENTS:", "").strip()
                if raw.upper() != "NONE":
                    selected = [a.strip() for a in raw.split(",") if a.strip() in AVAILABLE_AGENTS]
                break

        return {**state, "selected_agents": selected}
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
Outcomes:
{chr(10).join(lines)}

Write a concise operational summary."""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    return {**state, "summary": response.content}


def has_error(state: SupervisorState) -> str:
    return "error" if state.get("error") else "continue"


def build_supervisor_graph() -> StateGraph:
    graph = StateGraph(SupervisorState)
    graph.add_node("classify_intent", classify_intent)
    graph.add_node("dispatch_agents", dispatch_agents)
    graph.add_node("aggregate_results", aggregate_results)

    graph.set_entry_point("classify_intent")
    graph.add_conditional_edges("classify_intent", has_error, {"error": END, "continue": "dispatch_agents"})
    graph.add_conditional_edges("dispatch_agents", has_error, {"error": END, "continue": "aggregate_results"})
    graph.add_edge("aggregate_results", END)

    return graph.compile()


supervisor_graph = build_supervisor_graph()
