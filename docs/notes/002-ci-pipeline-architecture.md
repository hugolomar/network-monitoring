# Note 002: CI/CD Pipeline Architecture

This note documents the current state of the CI pipeline in this project, the differences compared to a professional environment, and the options for simulating real triggers locally.

## 1. Current State (Local Development Environment)
Currently, the pipeline is designed to be autonomous and reproducible on a local machine using Docker:
- **Execution**: Started with `docker-compose -f docker-compose.ci.yml up -d`.
- **Code Passing**: Uses a **shared volume** (`.:/workspace:ro`) in the Jenkins container. This allows Jenkins to see the host code without needing to clone it from the Internet.
- **Trigger**: Manual. The developer accesses Jenkins (`localhost:8085`) and triggers the build.
- **Infrastructure**: Everything runs in local containers (Jenkins, SonarQube, Database, Nginx for docs).

## 2. Professional Environment (Production)
In a real-world scenario, the architecture evolves toward scalability and total automation:
- **Jenkins Master/Agents**: The master manages logic, but builds run on **ephemeral agents** (containers that are spawned for a build and destroyed upon completion).
- **Webhooks**: The pipeline is not launched manually. GitHub/GitLab sends an event (Push/Merge) to the Jenkins server.
- **Persistence**: Tools like SonarQube are typically hosted on dedicated servers with persistent databases to track quality over time.

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
