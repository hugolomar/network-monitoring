#!/usr/bin/env bash
# Dev-only APM traces bootstrap when Kibana security/Fleet is disabled.
# Without the Fleet "apm" package, APM Server can ingest but Kibana APM UI stays empty
# or fails to render trace samples (missing processor.event / duration.us / keyword mappings).
#
# Usage:
#   bash ./infrastructure/stack/bootstrap/elasticsearch/apply-apm-traces-template.sh
set -euo pipefail

ES_URL="${ES_URL:-http://localhost:9200}"

echo "==> Upserting ingest pipeline apm-traces-processor-event"
curl -fsS -X PUT "${ES_URL}/_ingest/pipeline/apm-traces-processor-event" \
  -H 'Content-Type: application/json' \
  -d @- <<'EOF'
{
  "description": "Dev shim: processor.event + duration.us for Kibana APM without Fleet package",
  "processors": [
    {
      "set": {
        "field": "processor.event",
        "value": "transaction",
        "if": "ctx.transaction != null",
        "override": false
      }
    },
    {
      "set": {
        "field": "processor.event",
        "value": "span",
        "if": "ctx.transaction == null && ctx.span != null",
        "override": false
      }
    },
    {
      "script": {
        "description": "Derive transaction.duration.us from event.duration (ns)",
        "if": "ctx.event?.duration != null && ctx.transaction != null",
        "source": "if (ctx.transaction.duration == null) { ctx.transaction.duration = new HashMap(); } if (ctx.transaction.duration.us == null) { ctx.transaction.duration.us = (long)(ctx.event.duration / 1000L); }"
      }
    },
    {
      "script": {
        "description": "Derive span.duration.us from event.duration (ns)",
        "if": "ctx.event?.duration != null && ctx.span != null && ctx.transaction == null",
        "source": "if (ctx.span.duration == null) { ctx.span.duration = new HashMap(); } if (ctx.span.duration.us == null) { ctx.span.duration.us = (long)(ctx.event.duration / 1000L); }"
      }
    }
  ]
}
EOF
echo

echo "==> Upserting index template traces-apm-local"
curl -fsS -X PUT "${ES_URL}/_index_template/traces-apm-local" \
  -H 'Content-Type: application/json' \
  -d @- <<'EOF'
{
  "index_patterns": ["traces-apm*"],
  "data_stream": {},
  "priority": 500,
  "template": {
    "settings": {
      "index.default_pipeline": "apm-traces-processor-event",
      "index.number_of_replicas": 0
    },
    "mappings": {
      "dynamic_templates": [
        {
          "strings_as_keyword": {
            "match_mapping_type": "string",
            "mapping": { "type": "keyword", "ignore_above": 1024 }
          }
        }
      ],
      "properties": {
        "@timestamp": { "type": "date" },
        "processor": {
          "properties": { "event": { "type": "keyword" } }
        },
        "service": {
          "properties": {
            "name": { "type": "keyword" },
            "environment": { "type": "keyword" }
          }
        },
        "transaction": {
          "properties": {
            "name": { "type": "keyword" },
            "type": { "type": "keyword" },
            "id": { "type": "keyword" },
            "duration": { "properties": { "us": { "type": "long" } } }
          }
        },
        "span": {
          "properties": {
            "id": { "type": "keyword" },
            "duration": { "properties": { "us": { "type": "long" } } }
          }
        },
        "event": {
          "properties": {
            "duration": { "type": "long" },
            "outcome": { "type": "keyword" }
          }
        },
        "trace": { "properties": { "id": { "type": "keyword" } } }
      }
    }
  }
}
EOF
echo

# If a legacy plain index exists (created before the template), recreate as a data stream.
if curl -fsS "${ES_URL}/_cat/indices/traces-apm-default?h=index" 2>/dev/null | grep -qx 'traces-apm-default'; then
  echo "==> Recreating legacy traces-apm-default index as a data stream"
  curl -fsS -X DELETE "${ES_URL}/traces-apm-default" >/dev/null
  curl -fsS -X PUT "${ES_URL}/_data_stream/traces-apm-default" >/dev/null
  echo
elif ! curl -fsS "${ES_URL}/_data_stream/traces-apm-default" >/dev/null 2>&1; then
  echo "==> Creating data stream traces-apm-default"
  curl -fsS -X PUT "${ES_URL}/_data_stream/traces-apm-default" >/dev/null || true
  echo
else
  echo "==> Data stream traces-apm-default already present"
fi

# Backfill duration.us on existing docs (safe no-op when already populated).
if curl -fsS "${ES_URL}/_data_stream/traces-apm-default" >/dev/null 2>&1; then
  echo "==> Backfilling transaction.duration.us on existing trace docs"
  curl -fsS -X POST "${ES_URL}/traces-apm-*/_update_by_query?conflicts=proceed" \
    -H 'Content-Type: application/json' \
    -d @- <<'EOF' >/dev/null
{
  "script": {
    "lang": "painless",
    "source": "if (ctx._source.event != null && ctx._source.event.duration != null) { long us = (long)(ctx._source.event.duration / 1000L); if (ctx._source.transaction != null) { if (ctx._source.transaction.duration == null) { ctx._source.transaction.duration = new HashMap(); } if (ctx._source.transaction.duration.us == null) { ctx._source.transaction.duration.us = us; } } else if (ctx._source.span != null) { if (ctx._source.span.duration == null) { ctx._source.span.duration = new HashMap(); } if (ctx._source.span.duration.us == null) { ctx._source.span.duration.us = us; } } }"
  },
  "query": {
    "bool": {
      "must": [ { "exists": { "field": "event.duration" } } ],
      "must_not": [ { "exists": { "field": "transaction.duration.us" } } ]
    }
  }
}
EOF
  echo
fi

echo "APM traces local template is ready."
