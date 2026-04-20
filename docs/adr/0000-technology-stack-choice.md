# ADR 0000: Technology Stack Choice

- Status: Accepted
- Date: 2026-04-06

## Context

The network monitoring platform needs to capture traffic passively, process sessions and devices at scale, expose data through APIs and UI, and secure both machine-to-machine and user access.

The architecture also needs:

- Decoupled ingestion and processing pipelines.
- Device discovery and device management from both automated and manual flows.
- A consistent domain model across probe, backend, and integration console.
- Fast search capabilities for session exploration.
- Strong security for internal communications and user authorization.

## Decision

Adopt an event-driven microservices architecture with this stack:

- **Probe**: `tshark` (Wireshark CLI) for packet capture/decoding, wrapped by a thin service that parses output and publishes domain events.
- **Event backbone**: Apache Kafka as the messaging platform.
- **Core storage and API**: Backend service with relational persistence (`PostgreSQL` / `TimescaleDB`) for canonical session/device data and business operations.
- **Search and analytics index**: Elasticsearch fed from Kafka through Kafka Connect sink.
- **Security (service-to-service)**: EJBCA-issued certificates with mandatory mTLS.
- **Security (users and roles)**: Keycloak as IdP using OIDC/OAuth2 with RBAC in backend APIs.
- **Shared domain model**: a common domain library for `Session` and `Device` entities reused by probe, backend, and integration console.
- **UI**: web application consuming backend APIs with Keycloak tokens.

## Alternatives Considered

1. **Direct HTTP from probe to backend instead of Kafka**
   - Simpler initial flow.
   - Rejected because it increases coupling, reduces resilience under bursts, and limits independent consumers.

2. **Custom Elasticsearch writer in backend instead of Kafka Connect**
   - More direct control in application code.
   - Rejected because it duplicates integration responsibilities and increases maintenance overhead.

3. **No shared domain library (contract-only integration)**
   - Better service autonomy.
   - Rejected for now due to higher mapping/serialization friction and increased risk of schema drift during early development.

4. **Single security layer only (mTLS or IdP, not both)**
   - Lower operational complexity.
   - Rejected because system requires both internal zero-trust service identity and user-level authorization.

## Consequences

### Positive

- Better scalability and decoupling through asynchronous pub/sub.
- Clear separation of concerns across probe, ingestion, processing, and presentation.
- Strong security posture by combining mTLS for services and RBAC for users.
- Better search and dashboard capabilities through Elasticsearch indexing.
- Reduced model mismatch by reusing shared domain entities.

### Negative / Trade-offs

- Higher operational complexity (Kafka, Connect, PKI, Keycloak, Elasticsearch).
- Shared domain library introduces coupling between services.
- More infrastructure and observability requirements for reliable operations.
- Team needs multi-stack expertise for runtime, security, and data pipelines.

## Notes

This ADR establishes the baseline stack for initial implementation. Future ADRs should refine specific choices (e.g., exact backend framework/runtime, schema format strategy, and long-term approach to shared contracts vs shared library).
