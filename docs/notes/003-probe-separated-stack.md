# 003 - Why Probe Runs Separately

**Date:** 2026-06-18  
**Status:** Active  
**Context:** The probe has runtime requirements (host networking and packet-capture capabilities) that
are different from the rest of the platform services. This note documents the deployment boundary and
its operational implications.

The probe has host-level network requirements that the rest of the stack does not need:

- It captures traffic from a real host interface, so it requires `network_mode: host`.
- It needs elevated Linux capabilities (`NET_ADMIN`, `NET_RAW`) for packet-capture operations.
- It depends on environment-specific interface naming and routing (`eth0`, WSL bridge, CI runner interfaces, etc.).

Keeping the probe in `docker-compose.probe.yml` isolates these constraints from the main application stack (`docker-compose.reference-stack.yml`), which improves:

- **Portability:** Kafka/backend/UI/postgres/neo4j remain standard bridge-network services.
- **Security posture:** elevated capabilities are scoped only to the probe container.
- **Operations:** app services can be restarted/upgraded independently from capture runtime.

## Practical implication

For full end-to-end operation, launch both:

1. Main stack (receivers and persistence):  
   `docker compose -f docker-compose.reference-stack.yml up -d --build`
2. Probe stack (capture/publish):  
   `docker compose -f docker-compose.probe.yml up --build`

If the probe is not running, downstream services stay up but no capture-derived events will be published into Kafka, so backend/UI graph and inventory flows will not reflect new network observations.
