#!/usr/bin/env bash
# Bring up and initialize the full local reference stack.
# Usage:
#   bash ./infrastructure/stack/bootstrap/reference-stack-init.sh
# Optional env:
#   STACK_BUILD=0   # skip --build in docker compose up
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
COMPOSE_FILE="${ROOT}/docker-compose.reference-stack.yml"
STACK_BUILD="${STACK_BUILD:-1}"

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "error: missing compose file: $COMPOSE_FILE" >&2
  exit 1
fi

cd "$ROOT"

echo "==> Starting reference stack"
if [[ "$STACK_BUILD" = "1" ]]; then
  docker compose -f "$COMPOSE_FILE" up -d --build
else
  docker compose -f "$COMPOSE_FILE" up -d
fi

echo "==> Ensuring Kafka topics"
bash ./infrastructure/stack/bootstrap/kafka-topics-init.sh

echo "==> Verifying Kafka + Schema Registry"
bash ./infrastructure/stack/health/verify-kafka-stack.sh

echo "==> Verifying Elasticsearch + Kafka Connect"
bash ./infrastructure/stack/health/verify-elasticsearch-stack.sh

echo "==> Applying Elasticsearch template/index bootstrap"
bash ./infrastructure/stack/bootstrap/elasticsearch/apply-index-template.sh

echo "==> Registering Elasticsearch sink connector"
bash ./infrastructure/connectors/register/register-elasticsearch-sink-connector.sh

echo ""
echo "Reference stack is ready."
echo "Next step (separate runtime boundary):"
echo "  docker compose -f docker-compose.probe.yml up -d --build"
