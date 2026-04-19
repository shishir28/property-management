import json
from typing import Any

from redis.asyncio import Redis


class JobStore:
    def __init__(self, redis: Redis, ttl_seconds: int = 86400):
        self._redis = redis
        self._ttl_seconds = ttl_seconds

    def _key(self, job_id: str) -> str:
        return f"workflow-job:{job_id}"

    async def set_running(self, job_id: str) -> None:
        await self._set(job_id, {"status": "running"})

    async def set_result(self, job_id: str, result: dict[str, Any]) -> None:
        await self._set(job_id, {"status": "completed", "result": result})

    async def set_error(self, job_id: str, error: str) -> None:
        await self._set(job_id, {"status": "failed", "error": error})

    async def get(self, job_id: str) -> dict[str, Any] | None:
        payload = await self._redis.get(self._key(job_id))
        if payload is None:
            return None
        return json.loads(payload)

    async def _set(self, job_id: str, value: dict[str, Any]) -> None:
        await self._redis.set(self._key(job_id), json.dumps(value), ex=self._ttl_seconds)
