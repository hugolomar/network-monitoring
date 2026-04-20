#!/usr/bin/env bash
# Smoke-check the reference Kafka + Schema Registry stack (from repo root).
# Usage: ./scripts/verify-kafka-stack.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "== docker compose ps =="
docker compose -f docker-compose.kafka.yml ps

echo ""
echo "== Schema Registry (subjects) =="
registry_ok=0
for _ in $(seq 1 30); do
  if curl -fsS "http://localhost:8081/subjects" >/dev/null 2>&1; then
    registry_ok=1
    break
  fi
  sleep 2
done
if [[ "$registry_ok" -ne 1 ]]; then
  echo "FAIL: http://localhost:8081 not reachable after ~60s (is schema-registry starting?)" >&2
  exit 1
fi
curl -fsS "http://localhost:8081/subjects" | head -c 500 || true
echo ""

echo ""
echo "== Kafka broker API (kafka-1 internal) =="
docker compose -f docker-compose.kafka.yml exec -T kafka-1 \
  kafka-broker-api-versions --bootstrap-server kafka-1:29092 >/dev/null
echo "OK: kafka-1:29092"

echo ""
echo "== Topic sessions.detected (if created) =="
if docker compose -f docker-compose.kafka.yml exec -T kafka-1 \
  kafka-topics --bootstrap-server kafka-1:29092 --list 2>/dev/null | grep -qx 'sessions.detected'; then
  docker compose -f docker-compose.kafka.yml exec -T kafka-1 \
    kafka-topics --bootstrap-server kafka-1:29092 --describe --topic sessions.detected
else
  echo "(topic not present yet — run ./scripts/kafka-topics-init.sh)"
fi

echo ""
echo "All checks passed."
