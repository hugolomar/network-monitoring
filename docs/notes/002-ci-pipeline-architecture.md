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
- **Sonar analysis token:** injected from Jenkins credential `sonarqube-token` (not stored in git). See [§1.1](#11-sonar-token--jenkins-credential-setup) below. Target hardening context: [§2.2](#22-security-and-secrets), [005](./005-security-hardening-backlog.md) **SEC-001**.

### 1.1 Sonar token — Jenkins credential setup

One-time setup (and again after `docker compose … down -v` on **`jenkins-home`**, or when Sonar invalidates the token). **Order matters:** create the credential **before** running a build that expects it.

1. **Ensure a valid Sonar token**
   - Open `http://localhost:9000` → **My Account** → **Security** → **Generate Tokens**.
   - **Type:** **Project Analysis Token** (project `network-monitoring`), or **User Token** for a quick lab fix.
   - **Expiration:** none on a single-developer machine.
   - Copy the token (shown once).

2. **Store it in Jenkins**
   - Open `http://localhost:8085` → **Manage Jenkins** → **Credentials**.
   - Open **(global)** → **Add Credentials**.
   - **Kind:** Secret text  
   - **Secret:** paste the Sonar token  
   - **ID:** `sonarqube-token` (must match `Jenkinsfile`)  
   - **Description:** e.g. `Sonar project analysis token (network-monitoring)`  
   - Save. No compose restart required.

3. **Confirm `Jenkinsfile`**
   - `environment { SONAR_TOKEN = credentials('sonarqube-token') }` — no literal token in the file.

4. **Run the pipeline**
   - Jenkins → your job → **Build Now**.
   - Sonar stages **Backend - Prepare Analysis** and **SonarQube - End Analysis** use the credential automatically.

5. **Commit safely**
   - Commit the `Jenkinsfile` change only (no token in git). Old tokens may still exist in git history; rotate in Sonar if the repo is or will be shared.

**When the token expires or Sonar is reset (`down -v` on Sonar volumes):** generate a new token in Sonar (step 1), update the existing Jenkins credential **Secret** (step 2, edit instead of add), re-run the build. Do not put the new token in `Jenkinsfile`.

## 2. Professional Environment (Production)
In a real-world scenario, the CI stack evolves on two axes that are kept separate below: **how builds run**
(automation) and **how secrets and access are handled** (security).

### 2.1 Automation and orchestration
- **Jenkins Master/Agents**: The master manages logic, but builds run on **ephemeral agents** (containers that are spawned for a build and destroyed upon completion).
- **Webhooks**: The pipeline is not launched manually. GitHub/GitLab sends an event (Push/Merge) to the Jenkins server.
- **Persistence**: Tools like SonarQube are typically hosted on dedicated servers with persistent databases to track quality over time.
- **Source checkout**: The pipeline clones from the remote repository (`checkout scm`) with deploy keys or OAuth—not a host bind-mount of working tree.

### 2.2 Security and secrets
- **No secrets in git**: Tokens and passwords (Sonar, registry, deploy keys) live in the CI **credential store** or secret manager, injected at runtime (e.g. Jenkins `withCredentials`), never in `Jenkinsfile` or compose files committed to the repo.
- **Sonar analysis token**: Use a **Project Analysis Token** scoped to `network-monitoring`, stored as a Jenkins Secret text credential (e.g. `sonarqube-token`). See **SEC-001** in [005](./005-security-hardening-backlog.md).
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
