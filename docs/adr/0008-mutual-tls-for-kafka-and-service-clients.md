# ADR 0008: Mutual TLS (mTLS) for Kafka and Probe Clients

- Status: Accepted
- Date: 2026-04-20

## Context

The platform architecture assumes an internal **PKI (EJBCA)** and **mutual TLS** between services:
producers and consumers present **client certificates**, brokers validate them, and authorization can
bind to **certificate identity** (for example Kafka ACLs). This is stronger than **TLS with server
authentication only**, where the client does not prove its identity with a certificate.

Feature specification `001-session-detection` (FR-016) stated **TLS as a minimum** and **mTLS as the
target** for enterprise alignment. We are now **committing to mTLS** for Kafka access from the probe
(and, by extension, the same posture for other internal clients as they appear), rather than
stopping at one-way TLS.

## Decision

Adopt **mutual TLS (mTLS)** for **Kafka** client connections from the probe and for other
first-party services that publish or consume platform topics, **in environments that represent
integration, staging, or production**.

- **Broker expectation:** Kafka is configured so that **client certificate authentication is
  required** (analogous to `ssl.client.auth=required` in the reference architecture) and access
  control can use **client identities** derived from certificates.
- **Certificate source:** Internal **X.509** certificates issued under the project PKI story
  (EJBCA in the reference case); exact issuance automation is out of scope for this ADR but **must**
  exist before non-development mTLS rollout.
- **Development:** A **non-production** stack **may** temporarily use weaker transport **only** if
  documented explicitly (e.g. PLAINTEXT or TLS without client certs) to reduce friction; such modes
  **must not** be promoted to staging/production without mTLS.

## Rationale

- **Alignment** with the documented network-monitoring architecture (service-to-service trust via
  mTLS, not only channel encryption).
- **Strong identity** for clients enables **least-privilege** Kafka ACLs and auditability.
- **Consistency:** Avoids a “TLS now, mTLS later” refactor on the same client libraries and broker
  listeners for serious environments.

## Alternatives Considered

1. **TLS with server authentication only (no client certs)**
   - **Pros:** Simpler bootstrap; fewer certificates to issue and rotate.
   - **Cons:** Does not match the reference security model; weaker client identity for ACLs;
     migration to mTLS still required for parity.
   - **Rejected** as the **target** posture for integration and above.

2. **SASL/SCRAM or OAuth over TLS instead of mTLS for Kafka**
   - **Pros:** Familiar in some orgs; no client keystores in some setups.
   - **Cons:** Diverges from the **EJBCA + mTLS** narrative for this platform; adds a parallel
     identity system unless unified.
   - **Deferred** unless a future ADR revisits enterprise IdP constraints for Kafka specifically.

## Consequences

- **Positive:** Security posture matches the **architecture case study**; ready for **ACLs** and
  clear service identity on the wire.
- **Negative:** **Operational cost:** keystores/truststores (or equivalents), rotation, expiry
  monitoring, and developer friction unless local dev exceptions are clearly documented.
- **Follow-up:** Provide **certificate** layout (CA, broker certs, client cert for probe), wire
  Confluent/.NET client **SSL** settings, and update **compose/Kubernetes** examples for mTLS
  listeners; keep **FR-016** and deployment docs in sync with this ADR.
