# ADR 0003: Validation Pattern for Probe Ingestion

- Status: Accepted
- Date: 2026-04-06

## Context

In the probe ingestion flow, malformed or partial traffic observations are expected in normal operation.
Using exceptions as the primary mechanism for expected validation failures reduces clarity and makes
stream behavior harder to reason about.

Additionally, this approach was explicitly requested as an architectural decision to improve quality
attributes in the current implementation.

## Decision

Adopt an explicit Validation Pattern in probe observation processing:

- Validate observations first.
- Accumulate validation errors in a structured result.
- Skip invalid observations without stopping the stream.
- Reserve exceptions for unexpected/runtime failures, not expected invalid input.

This applies to the probe use-case orchestration and related input-validation flow.

## Rationale

- Makes validation intent explicit and testable.
- Improves resilience for continuous stream processing.
- Produces richer operational diagnostics (what failed and why).
- Reduces exception-driven control flow in normal paths.

## Alternatives Considered

1. **Keep exception-driven validation**
   - Simpler short-term implementation.
   - Rejected due to lower maintainability and weaker diagnostics for expected invalid input.

2. **Validation only in domain constructors/factories**
   - Keeps domain strict.
   - Rejected as sole strategy because it still relies heavily on exceptions for normal ingestion noise.

3. **Adopt a full external validation framework immediately**
   - Could standardize validators at scale.
   - Deferred to avoid unnecessary complexity at this stage.

## Consequences

### Positive

- Better maintainability through explicit validation paths.
- Higher resilience: invalid records are dropped, processing continues.
- Better observability with structured validation errors.

### Negative / Trade-offs

- Slightly more boilerplate (validation result handling).
- Validation rules now exist both at input boundary and domain invariants (intentional layered defense).

## Scope and Follow-up

This ADR establishes the validation policy for probe ingestion. Future modules (API commands,
Kafka consumers, integration flows) should follow the same pattern where invalid input is expected.
