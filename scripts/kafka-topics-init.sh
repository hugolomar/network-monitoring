#!/usr/bin/env bash
# Idempotent topic creation for reference stack (docker-compose.reference-stack.yml).
# Run from repo root after brokers are healthy: ./scripts/kafka-topics-init.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="${ROOT}/docker-compose.reference-stack.yml"
BOOTSTRAP_INTERNAL="${KAFKA_BOOTSTRAP_INTERNAL:-kafka-1:29092,kafka-2:29092,kafka-3:29092}"
TOPIC="${KAFKA_SESSION_TOPIC:-sessions.detected}"
PARTITIONS="${SESSIONS_DETECTED_PARTITIONS:-3}"
REPLICATION="${SESSIONS_DETECTED_REPLICATION_FACTOR:-3}"

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "error: missing $COMPOSE_FILE" >&2
  exit 1
fi

cd "$ROOT"

docker compose -f "$COMPOSE_FILE" exec -T kafka-1 kafka-topics \
  --bootstrap-server "$BOOTSTRAP_INTERNAL" \
  --create \
  --topic "$TOPIC" \
  --partitions "$PARTITIONS" \
  --replication-factor "$REPLICATION" \
  --if-not-exists

echo "Topic ensured: $TOPIC (partitions=$PARTITIONS replication=$REPLICATION)"
