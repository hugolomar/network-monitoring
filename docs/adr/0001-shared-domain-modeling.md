# ADR 0001: Shared Domain Modeling and .NET Baseline

- Status: Accepted
- Date: 2026-04-06

## Context

The constitution principle `I. Clean/Hexagonal Shared Domain Core` defines mandatory constraints for domain modeling:

- Framework baseline is .NET 10.
- Domain entities in scope (`Session`, `Device`) must inherit from `SeedWork.Entity`.
- `Device` must implement `SeedWork.IAggregateRoot`.
- Shared domain abstractions in `src/NetworkMonitoring.Domain/SeedWork` are authoritative and mostly immutable.

The platform is composed of multiple components (probe, backend, integration flows) that must exchange and process the same domain concepts with minimal semantic drift.

## Decision

We adopt the following domain and technology decisions as a single coherent baseline:

1. **.NET 10 as project baseline**
   - Use .NET 10 across services/modules to maximize consistency in language/runtime behavior and packaging.
   - Prioritize straightforward domain sharing through common C# assemblies where justified by scope.

2. **Shared authoritative SeedWork**
   - Treat `src/NetworkMonitoring.Domain/SeedWork` as the core domain contract.
   - Keep `SeedWork` immutable except for minimal build/wiring needs explicitly allowed by constitution.

3. **Entity lineage through `Entity` base class**
   - `Session` and `Device` inherit from `Entity`, standardizing identity semantics and domain-event support.

4. **`Device` as aggregate root**
   - `Device` is marked with `IAggregateRoot` because it is the consistency boundary for device lifecycle data (identity via MAC, observed IPs, first/last seen, hostname, discovery source).
   - Event-driven ingestion and manual creation flows converge on the same business validation path; aggregate-root semantics make that boundary explicit and enforceable.
   - `Session` is intentionally **not** marked as aggregate root because it represents an observed, immutable network fact once recorded; it does not own an evolving consistency boundary in this model.

5. **Value Objects for protocol/domain invariants**
   - Represent critical network values as Value Objects to centralize validation, normalization, and equality by value.
   - Primary intent is to prevent:
     - Invalid data reaching entities (for example malformed IPs/MACs or out-of-range ports).
     - Multiple textual representations of the same value (normalization drift such as different MAC/IP formats).
     - Repeated validation/parsing logic across probe, backend, and integration flows.
     - Hidden bugs from primitive misuse (`string`/`int` passed without semantic guarantees).
     - Contract drift where each component interprets protocol/source fields differently.
   - Current Value Objects:
     - `IpAddress`: validates and normalizes IPv4/IPv6 textual representation.
     - `MacAddress`: validates/normalizes MAC format into canonical uppercase colon-separated form.
     - `Port`: enforces valid range (1..65535).
     - `ProtocolType`: normalizes known protocol names/numbers to a bounded set (`TCP`, `UDP`, `ICMP`, `OTHER`).
     - `DiscoverySource`: normalizes source origin to bounded set (`ARP`, `LLDP`, `CDP`, `TRAFFIC`, `OTHER`).

## Alternatives Considered

1. **Primitive obsession (`string`/`int` everywhere)**
   - Lower initial coding effort.
   - Rejected due to duplicated validation, inconsistent normalization, and higher cross-service drift risk.

2. **`Device` as plain entity (no aggregate root marker)**
   - Simpler modeling.
   - Rejected because device state is modified through multiple input channels and needs a clear transactional/consistency boundary.

3. **No shared SeedWork / duplicated local abstractions per service**
   - Better team autonomy in larger, independent organizations.
   - Rejected for current stage because it increases mapping overhead and weakens semantic alignment in a strongly shared domain.

4. **Alternative runtime baseline (non-.NET) for domain core**
   - Potentially broader ecosystem choices.
   - Rejected because it complicates shared domain distribution and violates constitutional baseline.

## Consequences

### Positive

- Strong and explicit domain invariants near the model.
- Consistent identity and equality behavior for entities and value objects.
- Lower ambiguity when probe/backend/integration components exchange domain data.
- Clear aggregate boundary for device lifecycle and validation rules.
- Faster onboarding and implementation coherence under a unified runtime/toolchain.

### Negative / Trade-offs

- Shared assembly and immutable SeedWork increase coupling between components.
- Runtime baseline and shared abstractions reduce technology flexibility.
- Evolving core model requires coordinated changes and version discipline.

## Scope Note

This ADR intentionally groups baseline runtime + shared domain modeling because these are tightly coupled by constitution principle I and are implemented together in the current codebase. If coupling pressure increases, future ADRs may split concerns (for example, runtime baseline vs contract/distribution strategy).

Validation policy for probe ingestion is documented in `ADR 0003`.
