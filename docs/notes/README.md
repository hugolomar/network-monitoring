# Technical Notes

Short operational or project-context documents. Notes answer **why something is set up this way** or
capture delivery context; repeatable procedures belong in `docs/guides/`.

## When to use what

| Kind | Location | Purpose |
|------|----------|---------|
| **Note** | `docs/notes/` | Context, delivery setup, operational boundaries |
| **Guide** | `docs/guides/` | Step-by-step procedures and local E2E validation |
| **ADR** | `docs/adr/` | Architectural decisions with long-lived rationale |

## Naming

Use `NNN-short-title.md` and a heading such as `# NNN - Title` (for example `003-probe-separated-stack.md`).

## Notes

| Note | Topic |
|------|-------|
| [001 - Scalar API Access](./001-scalar-access.md) | Enable Scalar/OpenAPI in local Docker |
| [002 - CI Pipeline Architecture](./002-ci-pipeline-architecture.md) | Jenkins pipeline layout |
| [003 - Why Probe Runs Separately](./003-probe-separated-stack.md) | Probe compose boundary and host networking |
| [004 - CI/CD Delivery Path](./004-ci-cd-local-delivery-path.md) | Local bootstrap vs CD gaps |
| [005 - Security Hardening Backlog](./005-security-hardening-backlog.md) | Known security gaps and target hardening |
| [006 - SonarQube in Local CI](./006-sonarqube-local-ci-behavior.md) | Sonar token, checkout, and Stage View behavior |
| [007 - Elasticsearch Tenancy](./007-elasticsearch-tenancy.md) | One instance for business search, logs and traces; shared fate |

Promote stable procedural content to `docs/guides/` when it outgrows a note.
