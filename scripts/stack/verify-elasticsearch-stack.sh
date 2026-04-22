#!/usr/bin/env bash
# Smoke-check Elasticsearch and Kafka Connect in the reference stack.
# Run from repo root after: docker compose -f docker-compose.reference-stack.yml up -d
# Usage: ./scripts/stack/verify-elasticsearch-stack.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

ES_URL="${ES_URL:-http://localhost:9200}"
CONNECT_URL="${CONNECT_URL:-http://localhost:8083}"

echo "== docker compose (reference) services of interest =="
docker compose -f docker-compose.reference-stack.yml ps elasticsearch kafka-connect 2>/dev/null || true

echo ""
echo "== Elasticsearch cluster health =="
es_ok=0
for _ in $(seq 1 40); do
  if code=$(curl -sS -o /tmp/_es_h.json -w "%{http_code}" "$ES_URL/_cluster/health?wait_for_status=yellow&timeout=5s" 2>/dev/null) && [ "$code" = "200" ]; then
    es_ok=1
    break
  fi
  sleep 2
done
if [[ "$es_ok" -ne 1 ]]; then
  echo "FAIL: $ES_URL not healthy after wait (is the elasticsearch service up?)" >&2
  exit 1
fi
head -c 400 /tmp/_es_h.json; echo; rm -f /tmp/_es_h.json
echo "OK: Elasticsearch"

echo ""
echo "== Kafka Connect REST =="
echo "Waiting for $CONNECT_URL (first container start: confluent-hub + JVM can take several minutes)..."
kc_ok=0
for i in $(seq 1 150); do
  if curl -fsS -o /tmp/_kc_root.json "$CONNECT_URL/" 2>/dev/null; then
    kc_ok=1
    break
  fi
  if (( i % 15 == 0 )); then
    echo "  ... still waiting (${i} attempts, ~$((i * 2))s)"
  fi
  sleep 2
done
if [[ "$kc_ok" -ne 1 ]]; then
  echo "FAIL: $CONNECT_URL not reachable after ~5m (is kafka-connect running? check: docker compose logs kafka-connect --tail 80)" >&2
  exit 1
fi
head -c 200 /tmp/_kc_root.json; echo; rm -f /tmp/_kc_root.json

if ! curl -fsS -o /tmp/_kc_c.json "$CONNECT_URL/connectors" 2>/dev/null; then
  echo "FAIL: $CONNECT_URL/connectors" >&2
  exit 1
fi
echo "Connect connectors:"
cat /tmp/_kc_c.json; echo; rm -f /tmp/_kc_c.json

echo ""
echo "All Elasticsearch + Connect checks passed."
