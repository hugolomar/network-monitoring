# Technical Reference: System Codebase

Welcome to the technical heart of the Network Monitoring System. This section provides an automated, in-depth view of the codebase, generated directly from the source code and its XML documentation comments.

## System Organization

The codebase is organized into three main pillars to ensure separation of concerns and maintainability:

### 📂 Core Services (`src/`)
The active components of the monitoring system:
*   **NetworkMonitoring.Backend**: The central API and persistence layer. Handles device inventory and communication graphs.
*   **NetworkMonitoring.Probe**: The data collector. Captures network traffic (Tshark) and identifies sessions/devices.
*   **NetworkMonitoring.IntegrationConsole**: The bridge. Consumes events from Kafka and forwards them to the Backend.

### 📂 Domain Model
*   **NetworkMonitoring.Domain**: Shared business logic, entities (Device, Session), and value objects (IP, MAC) used across all services to ensure consistency.

### 📂 Test Suite (`tests/`)
Comprehensive verification of system behavior:
*   **Integration Tests**: Real-world scenarios involving databases, Kafka, and API contracts.
*   **Unit Tests**: Isolated logic checks for use cases, entities, and infrastructure mappers.

---
*Compliance: Documentation aligned with Constitution v1.7.0 (Article 29).*
