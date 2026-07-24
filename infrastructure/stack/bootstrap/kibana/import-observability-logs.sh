#!/usr/bin/env bash
# Import baseline observability Kibana saved objects (idempotent with overwrite=true).
# Run from repo root or any location:
#   bash ./infrastructure/stack/bootstrap/kibana/import-observability-logs.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../../.." && pwd)"
KIBANA_URL="${KIBANA_URL:-http://localhost:5601}"
OBJECTS_FILE="${KIBANA_OBJECTS_FILE:-$ROOT/infrastructure/observability/kibana/observability-logs.ndjson}"

if [[ ! -f "$OBJECTS_FILE" ]]; then
  echo "error: missing Kibana objects file: $OBJECTS_FILE" >&2
  exit 1
fi

echo "Waiting for Kibana at $KIBANA_URL ..."
ready=0
for i in $(seq 1 90); do
  if curl -fsS "$KIBANA_URL/api/status" -o /tmp/_kibana_status.json 2>/dev/null; then
    ready=1
    break
  fi

  if (( i % 15 == 0 )); then
    echo "  ... still waiting (${i} attempts, ~$((i * 2))s)"
  fi
  sleep 2
done

if [[ "$ready" -ne 1 ]]; then
  echo "error: Kibana is not reachable at $KIBANA_URL after wait" >&2
  exit 1
fi

echo "Importing Kibana saved objects from: $OBJECTS_FILE"
code=$(curl -sS -o /tmp/_kibana_import.json -w "%{http_code}" \
  -X POST "$KIBANA_URL/api/saved_objects/_import?overwrite=true" \
  -H "kbn-xsrf: true" \
  --form "file=@${OBJECTS_FILE}" || true)

if [[ "$code" != "200" ]]; then
  echo "error: Kibana import returned HTTP $code" >&2
  cat /tmp/_kibana_import.json >&2
  rm -f /tmp/_kibana_import.json /tmp/_kibana_status.json
  exit 1
fi

cat /tmp/_kibana_import.json
rm -f /tmp/_kibana_import.json /tmp/_kibana_status.json
echo "Kibana observability objects imported."
