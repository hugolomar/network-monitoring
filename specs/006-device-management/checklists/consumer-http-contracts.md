# Consumer HTTP Contracts Checklist: Device Management

**Purpose**: Unit tests for requirement writing — evaluate whether **written requirements** clearly constrain how the UI consumes existing backend HTTP contracts (`GET /devices`, `POST /devices`), without turning into execution-level API tests.

**Created**: 2026-04-28

**Feature**: [spec.md](../spec.md)

**Companion artifacts**: [contracts/backend-device-contracts.md](../contracts/backend-device-contracts.md), [contracts/ui-state-contract.md](../contracts/ui-state-contract.md)

**Depth**: Standard · **Audience**: PR reviewer / spec author

**Why this file is separate**: Focuses narrowly on **contract-consumer clarity** (payloads, outcomes, states tied to GET/POST). Broader FR/SC/story completeness lives in [requirements-quality.md](./requirements-quality.md).

---

## Boundary & stability

- [ ] CHK001 Does requirement prose explicitly forbid evolving payloads/endpoints such that reviewers can distinguish “consume as-is” from “negotiate backend changes”? [Completeness, Spec §FR-012, §contracts/backend-device-contracts §Compatibility Rules]
- [ ] CHK002 Are Kafka/topic/database coupling prohibitions stated at the same abstraction layer as HTTP-only coupling so authors cannot accidentally imply UI reads auxiliary pipelines? [Consistency, Spec §FR-009 vs §contracts preamble]

## Inventory query (GET)

- [ ] CHK003 Are successful empty-array semantics distinguished from transport/backend failure at requirement level (not only in contract appendix)? [Clarity, Spec §User Story 1 acceptance vs §contracts/backend-device-contracts Inventory Query]
- [ ] CHK004 Is mapping from HTTP-layer outcomes (200 vs unreachable vs 503) to operator-visible states spelled without embedding undocumented status-code assumptions beyond published contract behavior? [Measurability, Spec §FR-005, §contracts/backend-device-contracts §UI Behavior]

## Manual intake (POST)

- [ ] CHK005 Are Idempotency-Key semantics referenced by requirement IDs such that MAC-derived identity rules cannot drift from backend expectations without detection during review? [Traceability, Spec §FR-007–FR-008, contracts/backend-device-contracts §Headers]
- [ ] CHK006 Does requirement language distinguish POST outcomes (201 vs 200 vs 400 vs 503) with equivalent granularity to contract tables where operators need differentiated messaging? [Completeness, Spec §FR-005 vs §contracts/backend-device-contracts §UI Behavior]

## Observable UI states vs HTTP contracts

- [ ] CHK007 Does ui-state-contract documentation remain reconcilable with FR-005 state list (no unnamed intermediate states reviewers cannot trace)? [Consistency, Spec §FR-005 vs §contracts/ui-state-contract]
- [ ] CHK008 Are “refresh failure retains prior inventory” obligations expressed at requirement level consistent with GET semantics after partial failures in narrative acceptance scenarios? [Consistency, Spec §User Story 2 vs §contracts/ui-state-contract Refresh States]

## Cross-document drift

- [ ] CHK009 Where quickstart/examples include literal JSON bodies, are those examples flagged as illustrative of stable contracts rather than new authoritative requirement text diverging from backend specs? [Ambiguity, Spec §Success Criteria vs quickstart.md seed examples]

## Deferred coupling risks (requirements wording only)

- [ ] CHK010 Does any phrase imply cookies, bearer tokens, or gateway paths not covered by FR-010 configurability language for backend base URL resolution? [Gap, Spec §FR-010 vs §Assumptions authentication deferred]

## Request shape & optional fields

- [ ] CHK011 Are optional or nullable POST fields (for example `sourceEvent`) reflected consistently across contract examples and requirement narrative so authors do not invent mandatory payload complexity beyond backend intake? [Consistency, contracts/backend-device-contracts §Body vs data-model.md §ManualDeviceDraft]

## Headers & media type

- [ ] CHK012 Does requirement-level language align with documented expectations for `Content-Type` on POST without asserting headers omitted from FR scope that would contradict compatibility rules? [Completeness, contracts/backend-device-contracts §Manual Device Creation]

## Response semantics vs requirement ids

- [ ] CHK013 Are intake response fields (`status`, `reason`, `device`) in contract/data-model prose reconciled with FR-005 outcome vocabulary so “validation-error” vs “idempotent-success” naming cannot drift from backend outcome tokens? [Consistency, data-model.md §DeviceIntakeResponse, Spec §FR-005]

## Contract annex vs specification authority

- [ ] CHK014 Does `contracts/ui-state-contract.md` §Required Test Coverage read as illustrative verification themes derived from requirements rather than as new mandatory requirements absent from spec FR/SC? [Ambiguity, contracts/ui-state-contract.md §Required Test Coverage vs Spec §Requirements]

## Explicit non-requirements for HTTP surface

- [ ] CHK015 Are pagination, filtering, sorting, or bulk operations absent from GET explicitly treated as out-of-scope at requirement level where reviewers might assume REST list completeness? [Gap, contracts/backend-device-contracts §Inventory Query vs Spec §Functional Requirements]

---

## Notes

- Items deliberately avoid “call GET and expect 200” style validation; they audit whether **requirements documents** give reviewers enough precision to uphold contract-consumer discipline.
- Cross-check [requirements-quality.md](./requirements-quality.md) for FR/SC completeness before concluding consumer wording alone is sufficient.
