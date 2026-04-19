import httpx
from config import settings

_client = httpx.AsyncClient(base_url=settings.dotnet_api_base_url, timeout=30)


async def get_lease(lease_id: str) -> dict:
    r = await _client.get(f"/api/leases/{lease_id}")
    r.raise_for_status()
    return r.json()


async def get_leases() -> list[dict]:
    r = await _client.get("/api/leases")
    r.raise_for_status()
    return r.json()


async def get_expiring_leases(within_days: int = 60) -> list[dict]:
    r = await _client.get(f"/api/leases/expiring?withinDays={within_days}")
    r.raise_for_status()
    return r.json()


async def get_inspection(inspection_id: str) -> dict:
    r = await _client.get(f"/api/inspections/{inspection_id}")
    r.raise_for_status()
    return r.json()


async def get_inspections() -> list[dict]:
    r = await _client.get("/api/inspections")
    r.raise_for_status()
    return r.json()


async def get_tenant(tenant_id: str) -> dict:
    r = await _client.get(f"/api/tenants/{tenant_id}")
    r.raise_for_status()
    return r.json()


async def get_maintenance_request(request_id: str) -> dict:
    r = await _client.get(f"/api/maintenance/{request_id}")
    r.raise_for_status()
    return r.json()


async def get_open_maintenance(priority: str) -> list[dict]:
    r = await _client.get(f"/api/maintenance/open/{priority}")
    r.raise_for_status()
    return r.json()


async def get_overdue_payments() -> list[dict]:
    r = await _client.get("/api/payments/overdue")
    r.raise_for_status()
    return r.json()


async def assign_maintenance(request_id: str, contractor: str) -> None:
    r = await _client.post(f"/api/maintenance/{request_id}/assign", json={"contractor": contractor})
    r.raise_for_status()


async def mark_payment_overdue_notice_sent(payment_id: str) -> None:
    """Log that an overdue notice was sent (marks in workflow_events via .NET API)."""
    await _client.post(f"/api/payments/{payment_id}/pay", json={"paymentMethod": "pending_notice_sent"})


async def renew_lease(lease_id: str, new_end_date: str, new_monthly_rent: float) -> None:
    r = await _client.post(f"/api/leases/{lease_id}/renew",
                           json={"newEndDate": new_end_date, "newMonthlyRent": new_monthly_rent})
    r.raise_for_status()


async def create_maintenance_request(lease_id: str, title: str, description: str, priority: str) -> dict:
    r = await _client.post("/api/maintenance",
                           json={"leaseId": lease_id, "title": title, "description": description, "priority": priority})
    r.raise_for_status()
    return r.json()
