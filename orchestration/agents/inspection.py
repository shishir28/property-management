from typing import TypedDict
from langgraph.graph import StateGraph, END
from langchain_core.messages import HumanMessage
from agents.base import get_llm
from tools import api_client


class InspectionState(TypedDict):
    inspection_id: str
    inspection: dict
    checklist: str
    findings: list[str]
    maintenance_tickets_created: int
    error: str


async def fetch_inspection(state: InspectionState) -> InspectionState:
    try:
        inspection = await api_client.get_inspection(state["inspection_id"])

        if inspection.get("leaseId"):
            try:
                lease = await api_client.get_lease(inspection["leaseId"])
                inspection["unitNumber"] = lease.get("unitNumber")
            except Exception:
                inspection["unitNumber"] = None

        return {**state, "inspection": inspection}
    except Exception as e:
        return {**state, "error": str(e)}


async def generate_checklist(state: InspectionState) -> InspectionState:
    if state.get("error"):
        return state
    inspection = state["inspection"]
    llm = get_llm(fast=True)
    prompt = f"""Create a property inspection checklist.
Inspection type: {inspection['type']}
Unit: {inspection.get('unitNumber', 'N/A')}

Include sections for:
1. Exterior (roof, walls, windows, doors, parking)
2. Interior common areas
3. Unit interior (rooms, kitchen, bathrooms, appliances)
4. Safety systems (smoke detectors, CO detectors, fire extinguisher)
5. Utilities (electrical panel, plumbing, HVAC)

Format as a checklist with PASS/FAIL/N/A options."""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    return {**state, "checklist": response.content}


async def process_findings(state: InspectionState) -> InspectionState:
    if state.get("error"):
        return state

    inspection = state["inspection"]
    notes = inspection.get("notes", "")
    if not notes:
        return {**state, "findings": [], "maintenance_tickets_created": 0}

    llm = get_llm(fast=True)
    prompt = f"""Analyse these inspection notes and extract maintenance issues that need tickets.
Notes: {notes}

List each issue on a new line prefixed with ISSUE:
Example:
ISSUE: Leaking kitchen faucet - Priority: Urgent
ISSUE: Broken smoke detector in bedroom - Priority: Emergency"""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    findings = [l.replace("ISSUE:", "").strip() for l in response.content.splitlines() if l.startswith("ISSUE:")]
    return {**state, "findings": findings}


async def create_tickets(state: InspectionState) -> InspectionState:
    if state.get("error") or not state.get("findings"):
        return {**state, "maintenance_tickets_created": 0}

    lease_id = state["inspection"].get("leaseId")
    if not lease_id:
        return {**state, "maintenance_tickets_created": 0}

    count = 0
    for finding in state["findings"]:
        priority = "Emergency" if "emergency" in finding.lower() else \
                   "Urgent" if "urgent" in finding.lower() else "Routine"
        try:
            await api_client.create_maintenance_request(lease_id, "Inspection finding", finding, priority)
            count += 1
        except Exception:
            pass

    return {**state, "maintenance_tickets_created": count}


def has_error(state: InspectionState) -> str:
    return "error" if state.get("error") else "continue"


def build_inspection_graph() -> StateGraph:
    graph = StateGraph(InspectionState)
    graph.add_node("fetch_inspection", fetch_inspection)
    graph.add_node("generate_checklist", generate_checklist)
    graph.add_node("process_findings", process_findings)
    graph.add_node("create_tickets", create_tickets)

    graph.set_entry_point("fetch_inspection")
    graph.add_conditional_edges("fetch_inspection", has_error, {"error": END, "continue": "generate_checklist"})
    graph.add_conditional_edges("generate_checklist", has_error, {"error": END, "continue": "process_findings"})
    graph.add_conditional_edges("process_findings", has_error, {"error": END, "continue": "create_tickets"})
    graph.add_edge("create_tickets", END)

    return graph.compile()


inspection_graph = build_inspection_graph()
