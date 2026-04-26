#!/usr/bin/env bash
# SC-006: sample returned Elasticsearch documents and fail if _source drifts from
# session-detected-value.avsc field semantics (presence of required business fields).
# Opt-in: set RUN_ES_INTEGRATION=1 to execute checks (default: skip to avoid false failures in CI without ES).
# Usage: RUN_ES_INTEGRATION=1 ./scripts/acceptance/verify-sc006-elasticsearch-sampling.sh
# Needs: python3 to parse _search JSON.
set -euo pipefail

if [[ "${RUN_ES_INTEGRATION:-0}" != "1" ]]; then
  echo "SKIP: set RUN_ES_INTEGRATION=1 to run SC-006 Elasticsearch sampling (needs stack + data)."
  exit 0
fi

if ! command -v python3 >/dev/null 2>&1; then
  echo "error: python3 is required" >&2
  exit 1
fi

ES_URL="${ES_URL:-http://localhost:9200}"
INDEX="${ES_SESSIONS_INDEX:-sessions-detected}"

if ! curl -fsS -o /dev/null "$ES_URL" 2>/dev/null; then
  echo "error: cannot reach $ES_URL" >&2
  exit 1
fi

echo "== SC-006 sample query ($INDEX) =="
body='{"size":20,"query":{"match_all":{}},"sort":[{"occurredAtUtc":{"order":"desc"}}]}'
res=$(curl -sS -H "Content-Type: application/json" -X POST "$ES_URL/$INDEX/_search" -d "$body" || true)

export ES_SC006_INDEX="$INDEX"
# stdin to Python must be the _search body; do not use `| python3 <<'PY'`
# (the heredoc replaces stdin and breaks json.loads).
printf '%s' "$res" | python3 -c "$(cat <<'PY'
import json, os, sys

required = [
    "eventType",
    "occurredAtUtc",
    "source",
    "schemaVersion",
    "sourceIp",
    "destinationIp",
    "protocol",
    "firstSeenUtc",
    "lastSeenUtc",
    "bytesObserved",
]
index = os.environ.get("ES_SC006_INDEX", "sessions-detected")
raw = sys.stdin.read()
try:
    res = json.loads(raw)
except json.JSONDecodeError:
    print("FAIL: not valid JSON from _search", file=sys.stderr)
    print(raw, file=sys.stderr)
    sys.exit(1)

hits = res.get("hits", {}).get("hits", [])
n = len(hits)
if n == 0:
    print(
        f"No hits in {index} (no data to sample). Pass when index empty is acceptable for a dry check; "
        "ingest + probe data required for full SC-006."
    )
    sys.exit(0)

failed = False
for i, hit in enumerate(hits[:20]):
    src = hit.get("_source", {})
    for key in required:
        if key not in src:
            print(f"FAIL: hit {i} missing field: {key}", file=sys.stderr)
            print(json.dumps(src, indent=2), file=sys.stderr)
            failed = True

if failed:
    print("SC-006 sampling FAILED", file=sys.stderr)
    sys.exit(1)
print(
    f"SC-006: sampled {n} hit(s) — required field keys present (contract parity check, sample-based)."
)
PY
)"
