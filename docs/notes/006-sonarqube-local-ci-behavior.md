# 006 - SonarQube in Local CI (Operational Behavior)

**Date:** 2026-06-23  
**Status:** Active  
**Context:** How SonarQube integrates with the local Jenkins pipeline (`docker-compose.ci.yml`), and
non-obvious UI or checkout behavior observed while operating the lab stack.

Related: [002 - CI Pipeline Architecture](./002-ci-pipeline-architecture.md), [005 - Security Hardening Backlog](./005-security-hardening-backlog.md) **SEC-001**.

## Token and authentication

- The pipeline uses project key **`network-monitoring`** and reaches Sonar at **`http://sonarqube:9000`** from inside the CI Docker network (host UI: `http://localhost:9000`).
- **`SONAR_TOKEN`** comes from the Jenkins credential store: Secret text **`sonarqube-token`** in **Manage Jenkins → Credentials** (`credentials('sonarqube-token')` in `Jenkinsfile`). The token must not be committed to git.
- Prefer a Sonar **Project Analysis Token** for project `network-monitoring`. A **User Token** also works for lab troubleshooting.
- Tokens can **expire** or become invalid after **`docker compose … down -v`** on Sonar volumes (DB reset). Symptom: *Authentication with the server has failed* at **Backend - Prepare Analysis**. Fix: generate a new token in Sonar, **edit** the Jenkins credential (not `Jenkinsfile`), re-run the build.
- After `down -v` on **`jenkins-home`**, recreate the **`sonarqube-token`** credential in Jenkins.

### Tokens in git history

Older commits may still contain a hardcoded Sonar token from before the Jenkins credential migration. An **expired or revoked** token in history is not usable for authentication; confirm it is dead in Sonar (**My Account** → **Security**). No history rewrite is required for a **private local lab**. Before publishing the repository, treat any past secret in git as exposure and revoke tokens plus consider history cleanup.

## Jenkins checkout vs working tree

- Jenkins reads the repo from **`file:///workspace`** (bind mount of the project). **`checkout scm` uses committed refs**, not uncommitted edits on disk.
- If the build log still shows an old commit or an old hardcoded token, the change was not committed yet (or the branch tip Jenkins sees has not moved). Commit and re-run; no compose restart required.

## Stage View vs Build History

Two different widgets on the job page:

| Widget | What it shows |
|--------|----------------|
| **Builds** (sidebar) | Full build history (#1, #2, …). |
| **Stage View** (grid) | Recent runs of the **current pipeline definition**, with per-stage timings (~11–15 rows, not the full history). |

While a build is running, the UI emphasises the active pipeline; stages appear one by one. That is normal.

## Stage View after pipeline changes

Changing `Jenkinsfile` can change the **internal pipeline graph** even when stage **names** stay the same (for example, binding `SONAR_TOKEN` via `credentials()` adds a `withCredentials` wrapper). The Stage View then shows only builds that match the **new** definition; older runs remain in **Builds** but may disappear from the grid. This is not a counter reset (build #26 does not wipe #1–#25) and is unrelated to which Sonar token was used.

## What is *not* affected by the token

- Whether a build appears in the **Builds** sidebar.
- Jenkins build numbering (#26, #27, …).
- Stage View row limits or “reset” behaviour — those follow pipeline definition and Jenkins UI rules above, not token rotation.
