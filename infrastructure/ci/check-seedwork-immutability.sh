#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SEEDWORK_DIR="${ROOT_DIR}/src/NetworkMonitoring.Domain/SeedWork"

if [[ ! -d "${SEEDWORK_DIR}" ]]; then
  echo "[seedwork-check] SeedWork directory not found, skipping."
  exit 0
fi

changed="$(git -C "${ROOT_DIR}" status --porcelain -- "${SEEDWORK_DIR}" | wc -l | tr -d ' ')"
if [[ "${changed}" -gt 0 ]]; then
  echo "[seedwork-check] SeedWork contains modified files." >&2
  git -C "${ROOT_DIR}" status --short -- "${SEEDWORK_DIR}" >&2
  exit 1
fi

echo "[seedwork-check] PASS"
