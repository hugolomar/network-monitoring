#!/usr/bin/env bash
# Generate conceptual TOCs and build the DocFX site.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

python3 ./infrastructure/documentation/generate-conceptual-tocs.py
dotnet docfx metadata
dotnet docfx build

echo "DocFX site: $ROOT/artifacts/docs/site"
