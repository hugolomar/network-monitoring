#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
COMPOSE_FILE="${ROOT_DIR}/docker-compose.reference-stack.yml"
COLLECTOR_CFG="${ROOT_DIR}/infrastructure/observability/otel-collector-config.yml"

required_files=(
  "${COLLECTOR_CFG}"
  "${ROOT_DIR}/infrastructure/observability/prometheus.yml"
  "${ROOT_DIR}/infrastructure/observability/alert-rules.yml"
  "${ROOT_DIR}/infrastructure/observability/alertmanager.yml"
  "${ROOT_DIR}/infrastructure/observability/fluent-bit.conf"
  "${ROOT_DIR}/infrastructure/observability/fluent-bit-parsers.conf"
  "${ROOT_DIR}/infrastructure/observability/logstash/pipeline.conf"
  "${ROOT_DIR}/infrastructure/observability/logstash/logstash.yml"
  "${ROOT_DIR}/infrastructure/observability/grafana/provisioning/datasources/datasources.yml"
  "${ROOT_DIR}/infrastructure/observability/grafana/dashboards/observability-triage.json"
  "${ROOT_DIR}/infrastructure/observability/grafana/dashboards/observability-pipeline.json"
  "${ROOT_DIR}/infrastructure/observability/elasticsearch/logs-index-template.json"
  "${ROOT_DIR}/infrastructure/observability/kibana/observability-logs.ndjson"
  "${ROOT_DIR}/specs/008-observability/contracts/observability.md"
  "${ROOT_DIR}/docs/adr/0013-definitive-observability-stack.md"
)

missing=0
for file in "${required_files[@]}"; do
  if [[ ! -f "${file}" ]]; then
    echo "[observability-gate] missing required file: ${file}" >&2
    missing=1
  fi
done

if [[ "${missing}" -ne 0 ]]; then
  echo "[observability-gate] FAIL" >&2
  exit 1
fi

if [[ -f "${ROOT_DIR}/infrastructure/observability/grafana/dashboards/observability-baseline.json" ]]; then
  echo "[observability-gate] retired baseline dashboard must not remain" >&2
  exit 1
fi

if rg -q 'jaeger' "${COMPOSE_FILE}" "${COLLECTOR_CFG}"; then
  echo "[observability-gate] Jaeger must not remain in the reference stack" >&2
  exit 1
fi

if rg -q 'filelog:' "${COLLECTOR_CFG}"; then
  echo "[observability-gate] Collector must not tail platform logs via filelog" >&2
  exit 1
fi

if ! rg -q 'endpoint:\s*0\.0\.0\.0:4317' "${COLLECTOR_CFG}"; then
  echo "[observability-gate] collector gRPC endpoint must bind 0.0.0.0:4317" >&2
  exit 1
fi

if ! rg -q 'endpoint:\s*0\.0\.0\.0:4318' "${COLLECTOR_CFG}"; then
  echo "[observability-gate] collector HTTP endpoint must bind 0.0.0.0:4318" >&2
  exit 1
fi

if ! rg -q 'cors:' "${COLLECTOR_CFG}"; then
  echo "[observability-gate] collector OTLP/HTTP must enable CORS for browser ingest" >&2
  exit 1
fi

if ! rg -q 'docker_stats:' "${COLLECTOR_CFG}"; then
  echo "[observability-gate] collector must collect container runtime metrics via docker_stats" >&2
  exit 1
fi

if ! rg -q 'otlp/elastic-apm' "${COLLECTOR_CFG}"; then
  echo "[observability-gate] traces must export to Elastic APM" >&2
  exit 1
fi

if ! rg -q 'alertmanager:' "${ROOT_DIR}/infrastructure/observability/prometheus.yml"; then
  echo "[observability-gate] prometheus must send alerts to Alertmanager" >&2
  exit 1
fi

if ! rg -q 'fluent-bit:' "${COMPOSE_FILE}"; then
  echo "[observability-gate] compose must include fluent-bit for platform logs" >&2
  exit 1
fi

if ! rg -q 'logstash:' "${COMPOSE_FILE}"; then
  echo "[observability-gate] compose must include logstash for platform log normalization" >&2
  exit 1
fi

if ! rg -q 'apm-server:' "${COMPOSE_FILE}"; then
  echo "[observability-gate] compose must include apm-server" >&2
  exit 1
fi

if ! rg -q 'Observability__OtlpEndpoint:\s*http://otel-collector:4317' "${COMPOSE_FILE}"; then
  echo "[observability-gate] backend/integration OTLP endpoint must target otel-collector:4317" >&2
  exit 1
fi

if ! rg -q 'Observability__EnableOtlpLogs:\s*"true"' "${COMPOSE_FILE}"; then
  echo "[observability-gate] backend/integration OTLP log export flag must be enabled in compose" >&2
  exit 1
fi

if ! rg -q 'VITE_OTEL_OTLP_HTTP_URL' "${COMPOSE_FILE}"; then
  echo "[observability-gate] frontend build must configure browser OTLP/HTTP export" >&2
  exit 1
fi

echo "[observability-gate] PASS"
