#!/usr/bin/env bash
# Register (or update) the Elasticsearch Sink connector in Kafka Connect. Idempotent: create
# on 404, PUT config when the connector name already exists.
# Prerequisites: connect healthy; sessions.detected topic; registry; elasticsearch.
# Needs: jq **or** python3 to read the connector JSON.
# Usage: ./scripts/connectors/register-elasticsearch-sink-connector.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
CONNECT_URL="${CONNECT_URL:-http://localhost:8083}"
CFILE="${ELASTICSEARCH_SINK_CONNECTOR_JSON:-$ROOT/scripts/connectors/elasticsearch-sink-sessions-detected.json}"

if ! command -v jq >/dev/null 2>&1 && ! command -v python3 >/dev/null 2>&1; then
  echo "error: need jq or python3 to parse $CFILE" >&2
  exit 1
fi
if [[ ! -f "$CFILE" ]]; then
  echo "error: missing $CFILE" >&2
  exit 1
fi

read_name() {
  if command -v jq >/dev/null 2>&1; then
    jq -r .name "$1"
  else
    python3 -c "import json,sys; print(json.load(open(sys.argv[1]))['name'])" "$1"
  fi
}

write_config_compact() {
  local out="$1"
  if command -v jq >/dev/null 2>&1; then
    jq -c .config "$CFILE" >"$out"
  else
    python3 -c "import json,sys; c=json.load(open(sys.argv[1]))['config']; sys.stdout.write(json.dumps(c,separators=(',',':')))" "$CFILE" >"$out"
  fi
}

pretty() {
  if command -v jq >/dev/null 2>&1; then
    jq .
  elif command -v python3 >/dev/null 2>&1; then
    python3 -m json.tool
  else
    cat
  fi
}

NAME=$(read_name "$CFILE")
if [[ -z "$NAME" || "$NAME" = "null" ]]; then
  echo "error: connector JSON must have .name" >&2
  exit 1
fi

code=$(curl -sS -o /dev/null -w "%{http_code}" "$CONNECT_URL/connectors/$NAME" 2>/dev/null || echo "000")

curl_post_json() {
  # POST JSON; on failure print HTTP code and body (not JSON) to stderr, exit 1
  local url="$1" file="$2"
  local resp
  resp="$(mktemp)"
  local code
  code=$(curl -sS -o "$resp" -w "%{http_code}" -X POST -H "Content-Type: application/json" --data-binary "@$file" "$url")
  if [[ "$code" =~ ^2[0-9][0-9]$ ]]; then
    pretty <"$resp" || true
  else
    echo "error: Connect returned HTTP $code for POST $url" >&2
    cat "$resp" >&2
    rm -f "$resp"
    return 1
  fi
  rm -f "$resp"
}

curl_put_json() {
  local url="$1" file="$2"
  local resp
  resp="$(mktemp)"
  local code
  code=$(curl -sS -o "$resp" -w "%{http_code}" -X PUT -H "Content-Type: application/json" --data-binary "@$file" "$url")
  if [[ "$code" =~ ^2[0-9][0-9]$ ]]; then
    pretty <"$resp" || true
  else
    echo "error: Connect returned HTTP $code for PUT $url" >&2
    cat "$resp" >&2
    rm -f "$resp"
    return 1
  fi
  rm -f "$resp"
}

if [[ "$code" = "200" ]]; then
  echo "Connector '$NAME' exists; updating config..."
  tmp="$(mktemp)"
  write_config_compact "$tmp"
  curl_put_json "$CONNECT_URL/connectors/$NAME/config" "$tmp" || { rm -f "$tmp"; exit 1; }
  rm -f "$tmp"
  echo "OK: connector $NAME updated"
  exit 0
fi

echo "Creating connector '$NAME'..."
curl_post_json "$CONNECT_URL/connectors" "$CFILE" || exit 1
echo "OK: connector $NAME created"
