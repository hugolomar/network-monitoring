# 002 - Observability (High-level)

```mermaid
%%{init: {"theme":"base","themeVariables":{"fontSize":"44px"},"flowchart":{"curve":"basis","nodeSpacing":30,"rankSpacing":80,"padding":20,"useMaxWidth":false}}}%%
flowchart TB
  subgraph APP["APPLICATION EMITTERS"]
    Probe["Probe"]
    Console["Integration Console"]
    Backend["Backend API"]
    DeviceUI["Device UI (browser)"]
  end

  subgraph PLATFORM["PLATFORM EMITTERS"]
    Kafka["Kafka brokers"]
    Connect["Kafka Connect"]
    Postgres["PostgreSQL"]
    Neo4j["Neo4j"]
    ElasticCore["Elasticsearch"]
  end

  subgraph OBS["OBSERVABILITY PLATFORM"]
    OTel["OpenTelemetry Collector"]
    Prom[("Prometheus")]
    Grafana["Grafana"]
    AlertMgr["Alertmanager"]
    FluentBit["Fluent Bit"]
    Logstash["Logstash"]
    APM["Elastic APM Server"]
    ElasticObs[("Elasticsearch\n(logs + traces)")]
    Kibana["Kibana"]
  end

  %% Application telemetry path
  APP -->|"app logs+metrics+traces"| OTel
  DeviceUI -.->|"browser traces"| OTel

  %% Platform metrics path (collector receivers)
  Kafka -->|"platform metrics"| OTel
  Connect -->|"health metrics"| OTel
  Postgres -->|"platform metrics"| OTel
  Neo4j -->|"health metrics"| OTel
  ElasticCore -->|"platform metrics only"| OTel

  %% Platform logs path
  PLATFORM -->|"stdout logs"| FluentBit


  %% Metrics leg
  OTel -->|"metrics"| Prom
  Prom --> Grafana
  Prom -->|"alerts"| AlertMgr

  %% Logs and traces leg
  OTel -->|"application logs"| FluentBit
  OTel -->|"traces"| APM
  FluentBit --> Logstash
  Logstash -->|"platform logs"| ElasticObs
  APM -->|"apm traces"| ElasticObs
  ElasticObs --> Kibana
```

## Key flows

Edge labels are intentionally short for readability. At this level, the diagram captures these
observability paths:

- Application telemetry is emitted through OpenTelemetry instrumentation in authored services; browser telemetry contributes client-side traces to the same collector entry point.
- Platform telemetry is split by signal type: selected platform components expose metrics to Collector receivers, while selected platform logs are collected from container standard output through `Fluent Bit -> Logstash`.
- Elasticsearch is intentionally excluded from centralized platform log collection to avoid a self-referential logging cycle; its health is observed through metrics instead.
- Metrics are centralized through `OpenTelemetry Collector -> Prometheus`, then consumed in `Grafana`; alert evaluation in Prometheus is routed through `Alertmanager`.
- Logs and traces converge in Elasticsearch through two routes: platform logs through `Fluent Bit -> Logstash`, and traces through `Elastic APM Server`; Kibana is the read surface for investigation.

## Scope note

This is a high-level topology view. It intentionally omits low-level parsing/enrichment details
(for example multiline handling, redaction rules, index templates, and receiver-specific tuning).
For definitive implementation rationale and trade-offs, see `docs/adr/0013-definitive-observability-stack.md`.

