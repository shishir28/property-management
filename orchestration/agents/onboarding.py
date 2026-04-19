from typing import TypedDict
from langgraph.graph import StateGraph, END
from langchain_core.messages import HumanMessage
from agents.base import get_llm
from tools import api_client


class OnboardingState(TypedDict):
    tenant_id: str
    lease_id: str
    tenant: dict
    lease: dict
    welcome_message: str
    move_in_checklist: str
    error: str


async def fetch_tenant_lease(state: OnboardingState) -> OnboardingState:
    try:
        tenant = await api_client.get_tenant(state["tenant_id"])
        lease = await api_client.get_lease(state["lease_id"])
        return {**state, "tenant": tenant, "lease": lease}
    except Exception as e:
        return {**state, "error": str(e)}


async def generate_welcome(state: OnboardingState) -> OnboardingState:
    if state.get("error"):
        return state
    tenant, lease = state["tenant"], state["lease"]
    llm = get_llm()
    prompt = f"""Write a warm, professional welcome message for a new tenant.
Tenant name: {tenant['fullName']}
Unit: {lease['unitNumber']}
Move-in date: {lease['startDate']}
Monthly rent: ${lease['monthlyRent']}

Include: welcome, key collection instructions, emergency contact reminder, and portal login info.
Keep it friendly and under 200 words."""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    return {**state, "welcome_message": response.content}


async def generate_checklist(state: OnboardingState) -> OnboardingState:
    if state.get("error"):
        return state
    lease = state["lease"]
    llm = get_llm(fast=True)
    prompt = f"""Create a detailed move-in checklist for a tenant.
Unit number: {lease['unitNumber']}
Move-in date: {lease['startDate']}

Include sections for:
1. Before move-in (documents, payments, keys)
2. Day of move-in (inspection, utilities, meter readings)
3. First week (setup, reporting issues, portal registration)

Format as a numbered checklist."""
    response = await llm.ainvoke([HumanMessage(content=prompt)])
    return {**state, "move_in_checklist": response.content}


def has_error(state: OnboardingState) -> str:
    return "error" if state.get("error") else "continue"


def build_onboarding_graph() -> StateGraph:
    graph = StateGraph(OnboardingState)
    graph.add_node("fetch_tenant_lease", fetch_tenant_lease)
    graph.add_node("generate_welcome", generate_welcome)
    graph.add_node("generate_checklist", generate_checklist)

    graph.set_entry_point("fetch_tenant_lease")
    graph.add_conditional_edges("fetch_tenant_lease", has_error, {"error": END, "continue": "generate_welcome"})
    graph.add_conditional_edges("generate_welcome", has_error, {"error": END, "continue": "generate_checklist"})
    graph.add_edge("generate_checklist", END)

    return graph.compile()


onboarding_graph = build_onboarding_graph()
