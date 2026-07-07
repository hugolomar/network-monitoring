# Architecture Decision Records (ADR)

Records of significant architectural decisions for the Network Monitoring platform. An ADR captures
**why** a choice was made, its context, and its consequences — not step-by-step procedures (see
`docs/guides/`) or short operational context (see `docs/notes/`).

## When to write an ADR

Create an ADR when a decision is:

- Hard to reverse or expensive to change later
- Shared across modules or runtime boundaries
- Likely to be questioned by future contributors

Use a **guide** for repeatable how-to work. Use a **note** for local setup context that may evolve quickly.

## Format

- **Filename:** `NNNN-short-title-kebab.md` (four-digit index, for example `0011-graph-database-for-device-communications.md`)
- **Title:** `# ADR NNNN: Human-readable title`
- **Body:** Status, date, context, decision, consequences (see existing ADRs for examples)

Add the file in this directory. Individual ADRs appear in the published documentation sidebar after
the DocFX build (see `infrastructure/documentation/README.md`).

## Status

Existing records use **Accepted** unless a later ADR explicitly supersedes them. When superseding, link
to the replacing ADR and update the older record's status if the team agrees to maintain history that way.
