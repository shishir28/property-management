from typing import TypedDict
from langgraph.graph import StateGraph, END
from langchain_core.messages import HumanMessage
from agents.base import get_llm
from tools import api_client


class MaintenanceState(TypedDict):
    request_id: str
    request: dict
    triage_result: str
    recommended_contractor: str
    assigned: bool
    error: str


async def fetch_request(state: MaintenanceState) -> MaintenanceState:
    try:
        request = await api_client.get_maintenance_request(state["request_id"])
        return {**state, "request": request}
    except Exception as e:
        return {**state, "error": str(e)}


async def triage(state: MaintenanceState) -> MaintenanceState:
    if state.get("error"):
        return state
    req = state["request"]
    llm = get_llm(fast=True)
    prompt = f"""You are a property maintenance coordinator.
Issue: {req['title']}
Description: {req['description']}
Reported priority: {req['priority']}

1. Confirm or adjust the priority (Emergency/Urgent/Routine). Explain why.
2. What type of contractor is needed? (plumber, electrician, HVAC, general handyman, etc.)
3. What is the SLA deadline? (Emergency=24h, Urgent=72h, Routine=30 days)
4. Any safety concerns?

Reply in this format:
CONFIRMED_PRIORITY: <priority>
CONTRACTOR_TYPE: <type>
SLA: <deadline>
SAFETY: <yes/no - reason>
NOTES: <brief notes>"""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    contractor_type = "general handyman"
    for line in response.content.splitlines():
        if line.startswith("CONTRACTOR_TYPE:"):
            contractor_type = line.split(":", 1)[1].strip()
    return {**state, "triage_result": response.content, "recommended_contractor": contractor_type}


async def assign_contractor(state: MaintenanceState) -> MaintenanceState:
    if state.get("error"):
        return state
    try:
        await api_client.assign_maintenance(state["request_id"], state["recommended_contractor"])
        return {**state, "assigned": True}
    except Exception as e:
        return {**state, "error": str(e)}


def has_error(state: MaintenanceState) -> str:
    return "error" if state.get("error") else "continue"


def build_maintenance_graph() -> StateGraph:
    graph = StateGraph(MaintenanceState)
    graph.add_node("fetch_request", fetch_request)
    graph.add_node("triage", triage)
    graph.add_node("assign_contractor", assign_contractor)

    graph.set_entry_point("fetch_request")
    graph.add_conditional_edges("fetch_request", has_error, {"error": END, "continue": "triage"})
    graph.add_conditional_edges("triage", has_error, {"error": END, "continue": "assign_contractor"})
    graph.add_edge("assign_contractor", END)

    return graph.compile()


maintenance_graph = build_maintenance_graph()
