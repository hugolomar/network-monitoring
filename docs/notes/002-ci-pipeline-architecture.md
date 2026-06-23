# 002 - CI Pipeline Architecture

This note documents the current state of the CI pipeline in this project, the differences compared to a professional environment, and the options for simulating real triggers locally.

## 1. Current State (Local Development Environment)
Currently, the pipeline is designed to be autonomous and reproducible on a local machine using Docker:
- **Execution**: Started with `docker compose -f docker-compose.ci.yml up -d`.
- **Code passing**: Uses a **shared volume** (`.:/workspace:ro`) in the Jenkins container. This allows Jenkins to see the host code without needing to clone it from the Internet.
- **Trigger**: Manual. The developer accesses Jenkins and triggers the build.
- **CI stack endpoints**:
  - **Jenkins** — `http://localhost:8085`
  - **SonarQube** — `http://localhost:9000` (after the Sonar stage)
  - **Technical wiki** — `http://localhost:8090` (after a successful docs stage; see `infrastructure/documentation/README.md`)
- **Secrets (local CI):** stored in the **Jenkins credential store** — **Manage Jenkins → Credentials** (Secret text, e.g. `sonarqube-token`). Values persist in the `jenkins-home` volume; the `Jenkinsfile` only references credential IDs via `credentials('…')`. Not a separate tool beyond Jenkins.
- **Sonar analysis token:** Jenkins credential `sonarqube-token` (not in git). Operational detail: [006 - SonarQube in Local CI](./006-sonarqube-local-ci-behavior.md). Hardening: [§2.2](#22-security-and-secrets); **SEC-001** closed in [005](./005-security-hardening-backlog.md) (residual: expired token in old commits).

## 2. Professional Environment (Production)
In a real-world scenario, the CI stack evolves on two axes that are kept separate below: **how builds run**
(automation) and **how secrets and access are handled** (security).

### 2.1 Automation and orchestration
- **Jenkins Master/Agents**: The master manages logic, but builds run on **ephemeral agents** (containers that are spawned for a build and destroyed upon completion).
- **Webhooks**: The pipeline is not launched manually. GitHub/GitLab sends an event (Push/Merge) to the Jenkins server.
- **Persistence**: Tools like SonarQube are typically hosted on dedicated servers with persistent databases to track quality over time.
- **Source checkout**: The pipeline clones from the remote repository (`checkout scm`) with deploy keys or OAuth—not a host bind-mount of working tree.

### 2.2 Security and secrets
- **No secrets in git**: Tokens and passwords (Sonar, registry, deploy keys) are injected at runtime, never in `Jenkinsfile` or compose files committed to the repo.
- **Local lab — Jenkins credential store**: In this project the CI credential store **is Jenkins Credentials** (`http://localhost:8085` → **Manage Jenkins** → **Credentials**). Use **Secret text** entries (for example `sonarqube-token`) and bind them in the pipeline with `credentials('id')` or `withCredentials`. Secrets live in the **`jenkins-home`** Docker volume, not in git.
- **Production**: The same pattern applies; the store may be Jenkins Credentials plus an external **secret manager** (Vault, cloud provider secrets) or the hosting platform’s secret store (GitHub Actions Secrets, GitLab CI variables), depending on where CI runs.
- **Sonar analysis token**: **Project Analysis Token** scoped to `network-monitoring`, in Jenkins Secret text (`sonarqube-token`). **SEC-001** resolved — see [005](./005-security-hardening-backlog.md).
- **Least privilege**: Agents without host `docker.sock`, non-root where possible, strong Jenkins auth, TLS and auth at the edge if services are reachable beyond localhost.
- **Broader backlog**: Other known lab gaps (default compose passwords, privileged Jenkins, etc.) are tracked in [005 - Security Hardening Backlog](./005-security-hardening-backlog.md).

## 3. Simulating Real Triggers Locally
For GitHub to "call" our local Jenkins, we need a public URL since `localhost` is private.

### Option A: ngrok / Localtunnel (Recommended for testing)
Tools that create a secure tunnel from the Internet to our local port.
- **Pros**: Very easy to set up. Does not require router configuration.
- **Cons**: In the free plan, the URL changes every time you restart the service, forcing you to update the Webhook in GitHub constantly.

### Option B: Port Forwarding (Port Exposure)
Opening a port on the router pointing to the PC's local IP.
- **Status**: **DISCARDED** for security reasons.
- **Reason**: Directly exposing the Jenkins port to the Internet without a prior security audit, robust authentication, or a Reverse Proxy (like Nginx/Cloudflare) is a critical risk. It could allow unauthorized access to the host file system through service vulnerabilities.

## 4. Future Workflow Summary
If automation is desired, the workflow would be:
1. `git push` -> 2. Webhook (via ngrok) -> 3. Local Jenkins wakes up -> 4. `checkout scm` (clones from GitHub) -> 5. Executes tests and analysis.
