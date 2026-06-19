#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5090}"
UI_URL="${UI_URL:-http://localhost:3000}"
GRAPH_AUTH_HEADER="${GRAPH_AUTH_HEADER:-Authorization: Bearer test}"
GRAPH_ROLE_HEADER="${GRAPH_ROLE_HEADER:-X-Role: analyst}"

check_status() {
  local url="$1"
  local name="$2"
  shift 2
  local status
  status="$(curl -sS -o /tmp/traffic_lab_check.out -w "%{http_code}" "$@" "${url}")"
  if [[ "${status}" != "200" ]]; then
    echo "[traffic-lab] FAIL ${name}: ${url} -> ${status}" >&2
    return 1
  fi
  echo "[traffic-lab] OK   ${name}: ${url}"
}

echo "[traffic-lab] Running smoke checks..."
check_status "${UI_URL}" "frontend-root"
check_status "${BASE_URL}/devices" "devices-api"
check_status "${BASE_URL}/api/graph/devices/all?limit=200" "graph-snapshot-api" \
  -H "${GRAPH_AUTH_HEADER}" \
  -H "${GRAPH_ROLE_HEADER}"
echo "[traffic-lab] Smoke checks passed"
