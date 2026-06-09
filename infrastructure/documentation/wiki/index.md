# Network Monitoring System

Welcome to the official technical documentation for the Network Monitoring System. This site provides a comprehensive overview of the system architecture, domain logic, and API reference.

## Project Vision
A containerized, event-driven network monitoring pipeline built with **.NET 10**, **React**, **Kafka**, and **Elasticsearch**.

## Repository Guide 🗺️

To maintain a clean and professional workspace, the repository is organized by functional responsibility:

### 🏗️ Infrastructure (`infrastructure/`)
This directory contains the "plumbing" that makes the system run:
*   **`stack/`**: Everything needed to run the local environment.
    *   `bootstrap/`: Scripts for idempotent initialization (creating Kafka topics, ES templates).
    *   `health/`: Smoke tests to verify the status of the local stack (`verify-kafka`, `verify-elasticsearch`).
*   **`connectors/`**: Data plumbing for Kafka Connect.
    *   `configs/`: JSON configuration files for various sinks (Elasticsearch, Neo4j).
    *   `register/`: Scripts to register/update connectors via REST API.
*   **`acceptance/`**: Quality and contract validation.
    *   `contract/`: Data-level verification (e.g., sampling Elasticsearch hits to ensure schema parity).
*   **`wiki/`**: The source files for this documentation site.

### 📦 Core Folders
*   **`src/`**: The heart of the project (Backend, Frontend, Probe, Domain).
*   **`tests/`**: Unit and integration tests for all components.
*   **`specs/`**: Feature specifications, data models, and quickstart guides.
*   **`docs/`**: Persistent knowledge base (ADRs and design notes).
*   **`artifacts/`**: (Local only) Build outputs and generated documentation.

---

## Where to Start?
- [Read the Constitution](../../.specify/memory/constitution.md): Understand the laws and principles governing this repository.
- [Architecture Decisions (ADRs)](../../docs/adr/0000-technology-stack-choice.md): Explore why we chose certain technologies and patterns.
- [API Reference](../../artifacts/docs/api/toc.yml): Browse the automatically generated documentation for classes and methods.
