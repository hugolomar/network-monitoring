# 000 - Architecture Overview (High-level)

## Goal

Provide a single high-level view of the platform structure, showing clear layers and the
event-driven backbone.

## High-level Architecture

```mermaid
%%{init: {"theme":"base","themeVariables":{"fontSize":"52px"},"flowchart":{"curve":"basis","nodeSpacing":30,"rankSpacing":80,"padding":20,"useMaxWidth":false}}}%%
flowchart TB
  subgraph EDGE["PROBE"]
    Probe["Probe Service\n(tshark capture)"]
  end

  subgraph CICD["CI/CD PIPELINE"]
    Jenkins["Jenkins"]
    Sonar["SonarQube"]
    Jenkins -->|"scan"| Sonar
  end

  subgraph BACKBONE["EVENT BACKBONE"]
    Kafka["Kafka Topics\nsessions.detected + devices.detected"]
    Schema["Schema Registry\nAvro contracts"]
  end

  subgraph APP["APPLICATION SERVICES"]
    Console["Integration Console"]
    Backend["Backend API"]
    DeviceUI["Device Management UI"]
    Connect["Kafka Connect"]
  end

  subgraph OBS["OBSERVABILITY"]
    OTel["OpenTelemetry Collector"]
    Prom[("Prometheus")]
    Grafana["Grafana"]
    AlertMgr["Alertmanager"]
    FluentBit["Fluent Bit"]
    Logstash["Logstash"]
    APM["Elastic APM Server"]
    Kibana["Kibana"]
  end

  subgraph DATA["DATA LAYER"]
    Postgres[("PostgreSQL\nauthoritative inventory")]
    Neo4j[("Neo4j\ncommunication graph")]
    Elastic[("Elasticsearch\nbusiness + observability")]
  end

  %% Event backbone
  Probe -->|"events"| Kafka
  Probe -->|"schemas"| Schema
  Kafka -->|"consume"| Console
  Kafka -->|"consume"| Connect

  %% Application services: direction + protocol
  Console -->|"REST"| Backend
  DeviceUI -->|"REST"| Backend
  Backend -->|"SQL"| Postgres
  Backend -->|"Bolt"| Neo4j
  Connect -->|"bulk"| Elastic

  %% CI/CD
  Jenkins -->|"deploy"| APP

  %% Telemetry per component
  Probe -->|"logs+metrics+traces"| OTel
  Kafka -->|"logs+metrics+traces"| OTel
  Schema -->|"logs+metrics+traces"| OTel
  Console -->|"logs+metrics+traces"| OTel
  Backend -->|"logs+metrics+traces"| OTel
  Connect -->|"logs+metrics+traces"| OTel
  DeviceUI -->|"metrics+traces"| OTel

  %% Metrics leg
  OTel -->|"metrics"| Prom
  Prom --> Grafana
  Prom -->|"alerts"| AlertMgr

  %% Logs and traces leg
  OTel -->|"logs"| FluentBit
  OTel -->|"traces"| APM
  FluentBit --> Logstash
  Logstash --> Elastic
  APM --> Elastic
  Elastic --> Kibana
```

## Key flows

Edge labels stay deliberately short so the diagram renders narrow and legible. The full
detail behind each label is:

- Probe publishes `sessions.detected` / `devices.detected` as Avro and validates them against Schema Registry; Backend publishes its own domain events the same way.
- Integration Console and Kafka Connect consume those topics as independent consumer groups.
- `REST` edges are HTTPS with JSON payloads. `SQL` is PostgreSQL over TCP 5432, `Bolt` is the Neo4j driver protocol, and `bulk` is the Elasticsearch bulk HTTP API.
- Every runtime component exports OTLP telemetry, including Probe and Kafka. Backend, Console, Connect, Probe, Kafka, and Schema Registry emit all three signals over OTLP/gRPC; the browser UI emits metrics and traces over OTLP/HTTP.
- Observability splits into two legs from the Collector: metrics through `Prometheus -> Grafana` with `Alertmanager` for alerting, and logs/traces through `Logstash` and `APM Server` into Elasticsearch, with Kibana as the read surface.
- Jenkins builds and deploys the application services and runs SonarQube for code quality.

See `xxx-application-observability-stack.md` for the detailed service-level version.

## Event-driven characteristics

- **Asynchronous core**: business flow is decoupled through Kafka topics.
- **Contract-first events**: Avro schemas in Schema Registry govern event compatibility.
- **Independent consumers**: Integration Console and Connect evolve independently from producers.
- **Resilient ingestion**: capture, publication, consumption, and persistence are split into separate runtime responsibilities.

## Related

- `docs/adr/0013-definitive-observability-stack.md`
- `specs/007-device-communication-graph/spec.md`
- `specs/008-observability/spec.md`
