# Feature specifications

Product and feature design for Network Monitoring. Each numbered folder under `specs/` is one feature
slice: requirements, plan, tasks, contracts, and a local validation quickstart.

Specifications answer **what** we are building and **how we validate it** for that feature. They are
not architecture decision records (see `docs/adr/`), operational how-to guides (see `docs/guides/`), or
short-lived delivery notes (see `docs/notes/`).

## Spec Kit workflow

This repository uses **[Spec Kit](https://github.com/github/spec-kit)** (`.specify/` at the repo root)
to create and evolve feature folders in a consistent way.

- **New or updated features:** use the Spec Kit commands in Cursor (for example `/speckit.specify`,
  `/speckit.plan`, `/speckit.tasks`, `/speckit.implement`) rather than inventing ad hoc folder layouts.
- **Numbering:** feature directories are `NNN-short-name/` (for example `003-device-discovery/`).
  Spec Kit assigns the next index when creating a feature; avoid renumbering existing folders by hand.
- **Constitution:** cross-cutting principles live in `.specify/memory/constitution.md` (also linked from
  the published wiki sidebar).

Spec Kit writes and updates artifacts **inside** each feature directory. This `README.md` is orientation
only; it is not part of the Spec Kit template set.

## When to use what

| Kind | Location | Purpose |
|------|----------|---------|
| **Feature spec** | `specs/NNN-…/spec.md` | User stories, requirements, acceptance criteria |
| **Plan / tasks** | `specs/NNN-…/plan.md`, `tasks.md` | Implementation design and ordered work (Spec Kit) |
| **Feature quickstart** | `specs/NNN-…/quickstart.md` | Validate that feature slice locally against its spec |
| **Guide** | `docs/guides/` | Cross-feature procedures (E2E flows, traffic lab, etc.) |
| **ADR** | `docs/adr/` | Durable *why* for architecture and technology choices |
| **Note** | `docs/notes/` | Local CI, delivery context, operational boundaries |

## Typical layout per feature

```text
specs/NNN-feature-name/
  spec.md          # Feature specification (start here)
  plan.md          # Technical plan (Spec Kit)
  tasks.md         # Actionable task list (Spec Kit)
  quickstart.md    # Local validation steps
  research.md      # Spikes and decisions during planning (optional)
  data-model.md    # Entities and persistence (when applicable)
  contracts/       # API, Avro, HTTP, UI contracts
  checklists/      # Requirements or readiness checklists
```

## Features

| ID | Folder | Specification | Quickstart | Status |
|----|--------|---------------|------------|--------|
| 001 | [session-detection](./001-session-detection/) | [Probe session detection](./001-session-detection/spec.md) | [quickstart](./001-session-detection/quickstart.md) | Delivered |
| 002 | [session-indexing](./002-session-indexing/) | [Session indexing](./002-session-indexing/spec.md) | [quickstart](./002-session-indexing/quickstart.md) | Delivered |
| 003 | [device-discovery](./003-device-discovery/) | [Device discovery](./003-device-discovery/spec.md) | [quickstart](./003-device-discovery/quickstart.md) | Delivered |
| 004 | [device-ingestion](./004-device-ingestion/) | [Device ingestion](./004-device-ingestion/spec.md) | [quickstart](./004-device-ingestion/quickstart.md) | Delivered |
| 005 | [device-inventory](./005-device-inventory/) | [Device inventory](./005-device-inventory/spec.md) | [quickstart](./005-device-inventory/quickstart.md) | Delivered |
| 006 | [device-management](./006-device-management/) | [Device management UI](./006-device-management/spec.md) | [quickstart](./006-device-management/quickstart.md) | Delivered |
| 007 | [device-communication-graph](./007-device-communication-graph/) | [Communication graph](./007-device-communication-graph/spec.md) | [quickstart](./007-device-communication-graph/quickstart.md) | Delivered |
| 008 | [observability](./008-observability/) | [Production observability](./008-observability/spec.md) | [quickstart](./008-observability/quickstart.md) | Delivered |

Promote stable cross-feature procedures to `docs/guides/` when they outgrow a feature quickstart.
