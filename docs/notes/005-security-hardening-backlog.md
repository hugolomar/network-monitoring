# 005 - Security Hardening Backlog

**Date:** 2026-06-22  
**Status:** Active  
**Context:** Local and reference-stack setups prioritize reproducibility over production-grade security.
This note tracks known gaps, accepted lab risk, and the target hardening path. CI-specific Sonar
operational detail lives in [002 - CI Pipeline Architecture](./002-ci-pipeline-architecture.md).

## How to use this note

Add an entry when we discover a security gap that is **not yet fixed** but should not be forgotten.
When an item is resolved, move it to **Resolved** (with date and PR/commit reference) or delete it if
the fix is self-explanatory in code.

| Field | Meaning |
|-------|---------|
| **Risk (lab)** | Impact if the repo or local ports are exposed beyond a single developer machine |
| **Status** | `Accepted (lab)` = intentional for now; `Pending` = should be fixed before shared/production use |
| **Target** | Professional baseline we aim for |

## Open items

| ID | Area | What | Risk (lab) | Status | Target |
|----|------|------|------------|--------|--------|
| SEC-001 | CI / Sonar | Sonar token must not live in git; historical commits may still contain old tokens | Medium (high if repo is public) | Pending | `Jenkinsfile` uses `credentials('sonarqube-token')`; one-time **Secret text** in Jenkins UI; **Project Analysis Token** for `network-monitoring`. Rotate/revoke old tokens in Sonar if repo is shared. Setup: note 002 §1.1. |
| SEC-002 | CI / Jenkins | Jenkins container runs **privileged**, as **root**, with **`/var/run/docker.sock`** mounted | Medium | Accepted (lab) | Non-privileged agent, no host Docker socket, or dedicated ephemeral agents with minimal mounts |
| SEC-003 | CI / Jenkins | Jenkins on `:8085` with setup wizard skipped; no documented strong auth baseline | Medium | Accepted (lab) | Admin password + RBAC; do not expose port without reverse proxy and auth |
| SEC-004 | CI / Sonar | PostgreSQL for Sonar uses default credentials (`sonar` / `sonar`) in `docker-compose.ci.yml` | Low (localhost only) | Accepted (lab) | Strong generated passwords; secrets via env file not committed |
| SEC-005 | CI / Git | `GitSCM.ALLOW_LOCAL_CHECKOUT=true` — pipeline reads host workspace mount instead of authenticated clone | Low (local only) | Accepted (lab) | `checkout scm` from remote with deploy keys or OAuth; no host bind-mount of source |
| SEC-006 | Reference stack | Default passwords in `docker-compose.reference-stack.yml` (Postgres, Neo4j, etc.) | Low (localhost) | Accepted (lab) | Externalized secrets; different values per environment; never default in shared deployments |
| SEC-007 | Probe | Probe requires `NET_ADMIN` / `NET_RAW` and host networking (see note 003) | Low (scoped) | Accepted (lab) | Keep probe isolated; document capability boundary; avoid merging into main app compose |
| SEC-008 | CI / exposure | Sonar `:9000`, Jenkins `:8085`, wiki `:8090` bound to localhost without TLS or gateway auth | Low (local) | Accepted (lab) | TLS termination, SSO or basic auth at edge; never port-forward Jenkins to Internet (see note 002) |

## Resolved

*(None yet.)*

## Related notes

- [002 - CI Pipeline Architecture](./002-ci-pipeline-architecture.md) — Sonar token operations and CI vs production layout
- [003 - Why Probe Runs Separately](./003-probe-separated-stack.md) — elevated capabilities scoped to probe only
- [004 - CI/CD Delivery Path](./004-ci-cd-local-delivery-path.md) — delivery gates and promotion (future CD security belongs there)
