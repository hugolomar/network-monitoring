# 004 - CI/CD Delivery Path (Local Project Context)

**Date:** 2026-06-19  
**Status:** Active  
**Context:** The project currently has a working CI pipeline in Jenkins and operational bootstrap scripts
for local stack initialization. This note clarifies what those scripts are (and are not), what is still
missing for CD, and a practical path to implement CD in this repository without over-engineering.

## Current state

- `Jenkinsfile` is focused on **CI** stages (checkout, build/test, lint/build frontend, Sonar, docs).
- `infrastructure/stack/bootstrap/reference-stack-init.sh` automates local stack setup and validation:
  - `docker compose up`,
  - Kafka topic bootstrap,
  - Kafka/Schema Registry health checks,
  - Elasticsearch/Connect health checks,
  - Elasticsearch template/index bootstrap,
  - Elasticsearch sink connector registration.

## Important distinction

`reference-stack-init.sh` is an **automated runbook**, not a complete CD pipeline by itself.

- It defines *what operational steps must happen* to make the reference stack ready.
- A CD pipeline defines *when/how those steps run automatically* with gates, environment rules,
  promotion logic, and rollback policy.

## Why this matters

Without CD orchestration:
- bootstrap can still be executed manually and inconsistently,
- promotion rules are implicit,
- rollback is ad-hoc,
- there is no controlled progression from validated build to deployed release.

## What CD should add (professional baseline)

Inside Jenkins, after CI success, CD should include:

1. **Deploy stage** (target environment aware).
2. **Post-deploy smoke checks** (service and contract health).
3. **Promotion gates** (manual approval or policy checks where required).
4. **Rollback strategy** (previous image/config known and quickly restorable).
5. **Environment-aware configuration** (dev/staging/prod values, no hidden local assumptions).

For production-grade setups this usually means rolling/canary/blue-green strategies and strong
observability gates. For this project's current local context, we can keep it simpler.

## Practical CD approach for this repository (local-first)

Use Jenkins to orchestrate the existing runbook:

- Keep `reference-stack-init.sh` as the single source of truth for stack bootstrap.
- Add Jenkins stages after current CI stages:
  - `CD - Local Deploy`: run `reference-stack-init.sh`.
  - `CD - Local Probe Up` (optional based on job type): start `docker-compose.probe.yml`.
  - `CD - Local Smoke`: run `tools/traffic/validation/smoke-checks.sh`.
- Add optional manual gate for long-running traffic scenarios.

This gives consistent local delivery semantics while reusing the existing operational scripts.

## Suggested local pipeline shape

1. CI: checkout -> build/test -> lint/build frontend -> analysis -> docs  
2. CD-local:
   - stack bootstrap (`reference-stack-init.sh`)
   - optional probe startup
   - smoke checks
   - optional soak/traffic job trigger

## Scope guidance

- Keep long traffic simulations (for example 1-hour scenarios) out of the main CI path.
- Run them as separate scheduled/manual jobs (soak/regression profile).
- Keep CD-local deterministic and fast enough for frequent execution.

## Future-state ("full" CI/CD) roadmap

If the project later moves from local-first delivery to a production-grade platform, a common target
architecture is:

- **Jenkins** for CI and release artifact creation.
- **Kubernetes** as the runtime platform.
- **Argo CD** for GitOps synchronization (desired state from Git -> cluster state).
- **Argo Rollouts** for progressive delivery (rolling/canary/blue-green) with rollback support.

### Role split in that model

- Jenkins:
  - build/test/scan,
  - container image publish,
  - update deploy manifests/version refs in Git.
- Argo CD + Kubernetes:
  - detect manifest updates,
  - apply deployment changes in cluster.
- Argo Rollouts:
  - progressive traffic shifting,
  - automated rollback on failed analysis gates.

### Event model (recommended)

- PR / feature push -> CI only.
- Merge to `main` -> CI + optional non-production deployment.
- Version tag (for example `v1.4.0`) -> CI + release CD stages.

### Why keep both sections (local-first + future-state)

- Local-first is the fastest and simplest way to operate this repository today.
- Future-state provides a clear migration path when higher automation, safer releases, and multi-env
  promotion become necessary.

## Summary

- Today: CI is present; CD orchestration is still missing in Jenkins.
- `reference-stack-init.sh` is the correct operational foundation (automated runbook).
- Next step: wire that runbook into Jenkins CD stages with smoke gates and clear rollback behavior.
