#!/usr/bin/env bash
# Idempotent topic creation for reference stack (docker-compose.reference-stack.yml).
# Run from repo root after brokers are healthy: ./scripts/bootstrap/kafka-topics-init.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="${ROOT}/docker-compose.reference-stack.yml"
BOOTSTRAP_INTERNAL="${KAFKA_BOOTSTRAP_INTERNAL:-kafka-1:29092,kafka-2:29092,kafka-3:29092}"
SESSION_TOPIC="${KAFKA_SESSION_TOPIC:-sessions.detected}"
DEVICE_TOPIC="${KAFKA_DEVICE_TOPIC:-devices.detected}"
PARTITIONS="${DETECTED_EVENTS_PARTITIONS:-3}"
REPLICATION="${DETECTED_EVENTS_REPLICATION_FACTOR:-3}"

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "error: missing $COMPOSE_FILE" >&2
  exit 1
fi

cd "$ROOT"

for TOPIC in "$SESSION_TOPIC" "$DEVICE_TOPIC"; do
  docker compose -f "$COMPOSE_FILE" exec -T kafka-1 kafka-topics \
    --bootstrap-server "$BOOTSTRAP_INTERNAL" \
    --create \
    --topic "$TOPIC" \
    --partitions "$PARTITIONS" \
    --replication-factor "$REPLICATION" \
    --if-not-exists

  echo "Topic ensured: $TOPIC (partitions=$PARTITIONS replication=$REPLICATION)"
done
