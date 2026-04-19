from typing import TypedDict
from langgraph.graph import StateGraph, END
from langchain_core.messages import HumanMessage
from agents.base import get_llm
from tools import api_client
from datetime import date


class RentCollectionState(TypedDict):
    overdue_payments: list[dict]
    reminders: list[dict]
    escalations: list[dict]
    error: str


async def fetch_overdue(state: RentCollectionState) -> RentCollectionState:
    try:
        payments = await api_client.get_overdue_payments()
        return {**state, "overdue_payments": payments}
    except Exception as e:
        return {**state, "error": str(e)}


async def generate_reminders(state: RentCollectionState) -> RentCollectionState:
    if state.get("error") or not state["overdue_payments"]:
        return {**state, "reminders": [], "escalations": []}

    llm = get_llm(fast=True)
    today = date.today().isoformat()
    reminders, escalations = [], []

    for payment in state["overdue_payments"]:
        due = payment["dueDate"]
        amount = payment["amount"]
        lease_id = payment["leaseId"]

        prompt = f"""Generate a rent reminder notice.
Lease ID: {lease_id}
Amount due: ${amount}
Due date: {due}
Today: {today}

Write a firm but professional reminder. Include late fee warning if overdue > 5 days.
Keep it under 100 words."""
        response = await llm.ainvoke([HumanMessage(content=prompt)])
        reminders.append({"payment_id": payment["id"], "lease_id": lease_id, "message": response.content})

        # flag for escalation if overdue > 30 days
        from datetime import datetime
        days_overdue = (datetime.fromisoformat(today) - datetime.fromisoformat(due)).days
        if days_overdue > 30:
            escalations.append({"payment_id": payment["id"], "lease_id": lease_id, "days_overdue": days_overdue})

    return {**state, "reminders": reminders, "escalations": escalations}


async def handle_escalations(state: RentCollectionState) -> RentCollectionState:
    if state.get("error") or not state.get("escalations"):
        return state

    llm = get_llm()
    for esc in state["escalations"]:
        prompt = f"""Draft a formal eviction notice warning for lease {esc['lease_id']}.
Payment is {esc['days_overdue']} days overdue.
This is a final notice before legal proceedings.
Keep it professional and legally appropriate."""
        await llm.ainvoke([HumanMessage(content=prompt)])

    return state


def has_error(state: RentCollectionState) -> str:
    return "error" if state.get("error") else "continue"


def build_rent_collection_graph() -> StateGraph:
    graph = StateGraph(RentCollectionState)
    graph.add_node("fetch_overdue", fetch_overdue)
    graph.add_node("generate_reminders", generate_reminders)
    graph.add_node("handle_escalations", handle_escalations)

    graph.set_entry_point("fetch_overdue")
    graph.add_conditional_edges("fetch_overdue", has_error, {"error": END, "continue": "generate_reminders"})
    graph.add_conditional_edges("generate_reminders", has_error, {"error": END, "continue": "handle_escalations"})
    graph.add_edge("handle_escalations", END)

    return graph.compile()


rent_collection_graph = build_rent_collection_graph()
