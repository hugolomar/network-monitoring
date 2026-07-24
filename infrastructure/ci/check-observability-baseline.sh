#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
COMPOSE_FILE="${ROOT_DIR}/docker-compose.reference-stack.yml"

required_files=(
  "${ROOT_DIR}/infrastructure/observability/otel-collector-config.yml"
  "${ROOT_DIR}/infrastructure/observability/prometheus.yml"
  "${ROOT_DIR}/infrastructure/observability/alert-rules.yml"
  "${ROOT_DIR}/infrastructure/observability/grafana/provisioning/datasources/datasources.yml"
  "${ROOT_DIR}/infrastructure/observability/elasticsearch/logs-index-template.json"
  "${ROOT_DIR}/infrastructure/observability/kibana/observability-logs.ndjson"
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

if ! rg -q 'endpoint:\s*0\.0\.0\.0:4317' "${ROOT_DIR}/infrastructure/observability/otel-collector-config.yml"; then
  echo "[observability-gate] collector gRPC endpoint must bind 0.0.0.0:4317" >&2
  exit 1
fi

if ! rg -q 'endpoint:\s*0\.0\.0\.0:4318' "${ROOT_DIR}/infrastructure/observability/otel-collector-config.yml"; then
  echo "[observability-gate] collector HTTP endpoint must bind 0.0.0.0:4318" >&2
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

echo "[observability-gate] PASS"
