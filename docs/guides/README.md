# Guides

Task-oriented documentation for running and validating the platform locally. Guides answer **how to
do something**; they are not architecture decision records (see `docs/adr/`) and are not feature
specifications (see `specs/README.md`).

## When to use what

| Kind | Location | Purpose |
|------|----------|---------|
| **Guide** | `docs/guides/` | Repeatable procedures, local E2E flows, operational caveats |
| **ADR** | `docs/adr/` | Why a design or technology choice was made |
| **Feature quickstart** | `specs/*/quickstart.md` | Validate one feature slice against its spec (see `specs/README.md`) |
| **Note** | `docs/notes/` | Short-lived context or topics not yet promoted to a guide |

## Naming

Use `NNN-short-title.md` and a heading such as `# NNN - Title` (for example `001-traffic-deterministic-probe.md`).

## Guides

| Guide | Summary |
|-------|---------|
| [001 - Traffic simulator and deterministic probe](./001-traffic-deterministic-probe.md) | Build scenario PCAPs, run the probe in `DeterministicTest` mode, choose playback speed, validate downstream |

## Adding a guide

Each guide should include:

- **Audience** and **Prerequisites**
- **Goal** (one sentence)
- Numbered **steps**
- **Troubleshooting** or operational caveats where relevant
- **Related** links to ADRs, specs, and tool READMEs

Mature content in `docs/notes/` can be promoted here; leave a short redirect in the note if the
filename is already referenced elsewhere.
