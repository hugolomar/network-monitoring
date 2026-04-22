#!/usr/bin/env bash
# Apply sessions-detected index template to local Elasticsearch (reference stack).
# Run from repo root. Usage: ./scripts/bootstrap/elasticsearch/apply-index-template.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
ES_URL="${ES_URL:-http://localhost:9200}"
TEMPLATE_NAME="${ES_INDEX_TEMPLATE_NAME:-sessions-detected}"
TEMPLATE_FILE="${ES_INDEX_TEMPLATE_FILE:-$ROOT/scripts/bootstrap/elasticsearch/index-template-sessions-detected.json}"

if [[ ! -f "$TEMPLATE_FILE" ]]; then
  echo "error: missing template $TEMPLATE_FILE" >&2
  exit 1
fi

if ! curl -fsS -o /dev/null "$ES_URL" 2>/dev/null; then
  echo "error: cannot reach $ES_URL (is Elasticsearch up?)" >&2
  exit 1
fi

code=$(curl -sS -o /dev/null -w "%{http_code}" -X PUT \
  -H "Content-Type: application/json" \
  "$ES_URL/_index_template/$TEMPLATE_NAME" \
  --data-binary "@$TEMPLATE_FILE" || true)

if [[ "$code" != "200" && "$code" != "201" ]]; then
  echo "error: PUT _index_template/$TEMPLATE_NAME returned HTTP $code" >&2
  exit 1
fi
echo "Index template applied: $TEMPLATE_NAME ($code)"

# Composable template matches sessions-detected* but does not create an index. Kafka Connect
# (elasticsearch 14.x) validates that topic.to.external.resource.mapping targets already exist.
INDEX_NAME="${ES_SESSIONS_INDEX_NAME:-sessions-detected}"
tmp_body="$(mktemp)"
idx_code=$(curl -sS -o "$tmp_body" -w "%{http_code}" -X PUT \
  -H "Content-Type: application/json" \
  "$ES_URL/$INDEX_NAME" \
  -d '{}' || true)
if [[ "$idx_code" = "200" || "$idx_code" = "201" ]]; then
  echo "Index created: $INDEX_NAME ($idx_code)"
else
  exist=$(curl -sS -o /dev/null -w "%{http_code}" "$ES_URL/$INDEX_NAME" 2>/dev/null || echo "000")
  if [[ "$exist" = "200" ]]; then
    echo "Index $INDEX_NAME already exists (ok)"
  else
    echo "error: PUT $ES_URL/$INDEX_NAME returned HTTP $idx_code" >&2
    cat "$tmp_body" >&2
    rm -f "$tmp_body"
    exit 1
  fi
fi
rm -f "$tmp_body"
exit 0
