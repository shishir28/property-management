from typing import TypedDict
from langgraph.graph import StateGraph, END
from langchain_core.messages import HumanMessage
from agents.base import get_llm
from tools import api_client


class LeaseRenewalState(TypedDict):
    lease_id: str
    lease: dict
    tenant: dict
    compliance_notes: str
    renewal_notice: str
    suggested_rent: float
    error: str


async def fetch_lease(state: LeaseRenewalState) -> LeaseRenewalState:
    try:
        lease = await api_client.get_lease(state["lease_id"])
        tenant = await api_client.get_tenant(lease["tenantId"])
        return {**state, "lease": lease, "tenant": tenant}
    except Exception as e:
        return {**state, "error": str(e)}


async def check_compliance(state: LeaseRenewalState) -> LeaseRenewalState:
    if state.get("error"):
        return state
    lease = state["lease"]
    llm = get_llm(fast=True)
    prompt = f"""You are a property compliance expert.
Lease details:
- Jurisdiction: California, United States
- Current monthly rent: ${lease['monthlyRent']}
- Lease end date: {lease['endDate']}

What is the maximum legal rent increase percentage for this jurisdiction?
What notice period is required?
Suggest a reasonable new monthly rent (max legal increase).
Reply in this format:
MAX_INCREASE: <percent>
NOTICE_DAYS: <days>
SUGGESTED_RENT: <amount>
NOTES: <brief compliance summary>"""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    lines = response.content.strip().splitlines()
    suggested_rent = lease["monthlyRent"]
    for line in lines:
        if line.startswith("SUGGESTED_RENT:"):
            try:
                suggested_rent = float(line.split(":")[1].strip())
            except ValueError:
                pass
    return {**state, "compliance_notes": response.content, "suggested_rent": suggested_rent}


async def draft_notice(state: LeaseRenewalState) -> LeaseRenewalState:
    if state.get("error"):
        return state
    lease, tenant = state["lease"], state["tenant"]
    llm = get_llm()
    prompt = f"""Draft a professional lease renewal notice letter.
Tenant: {tenant['fullName']}
Unit: {lease['unitNumber']}
Current rent: ${lease['monthlyRent']}/month
Proposed rent: ${state['suggested_rent']}/month
Current end date: {lease['endDate']}
Compliance context: {state['compliance_notes']}

Write a complete, professional renewal notice. Be concise and clear."""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    return {**state, "renewal_notice": response.content}


def has_error(state: LeaseRenewalState) -> str:
    return "error" if state.get("error") else "continue"


def build_lease_renewal_graph() -> StateGraph:
    graph = StateGraph(LeaseRenewalState)
    graph.add_node("fetch_lease", fetch_lease)
    graph.add_node("check_compliance", check_compliance)
    graph.add_node("draft_notice", draft_notice)

    graph.set_entry_point("fetch_lease")
    graph.add_conditional_edges("fetch_lease", has_error, {"error": END, "continue": "check_compliance"})
    graph.add_conditional_edges("check_compliance", has_error, {"error": END, "continue": "draft_notice"})
    graph.add_edge("draft_notice", END)

    return graph.compile()


lease_renewal_graph = build_lease_renewal_graph()
