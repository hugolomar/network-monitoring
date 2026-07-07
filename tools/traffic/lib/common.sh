#!/usr/bin/env bash
set -euo pipefail

# Shared helpers for traffic tooling scripts.
#
# This file centralizes:
# - logging format conventions,
# - input and dependency guards, and
# - replay interface resolution behavior.

COMMON_LIB_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TRAFFIC_ROOT="$(cd "${COMMON_LIB_DIR}/.." && pwd)"

# Print informational message to stdout.
log() {
  printf '[traffic-lab] %s\n' "$*"
}

# Print warning message to stderr.
warn() {
  printf '[traffic-lab] WARN: %s\n' "$*" >&2
}

# Print fatal message and exit with status 1.
die() {
  printf '[traffic-lab] ERROR: %s\n' "$*" >&2
  exit 1
}

# Ensure required executable is available in PATH.
require_cmd() {
  local cmd="$1"
  command -v "${cmd}" >/dev/null 2>&1 || die "Required command not found: ${cmd}"
}

# Ensure required file exists.
require_file() {
  local path="$1"
  [[ -f "${path}" ]] || die "Required file not found: ${path}"
}

# Resolve replay interface using precedence:
# 1) explicit argument, 2) TRAFFIC_INTERFACE env var, 3) eth0 fallback.
resolve_iface() {
  local explicit_iface="${1:-}"
  if [[ -n "${explicit_iface}" ]]; then
    printf '%s\n' "${explicit_iface}"
    return
  fi
  if [[ -n "${TRAFFIC_INTERFACE:-}" ]]; then
    printf '%s\n' "${TRAFFIC_INTERFACE}"
    return
  fi
  printf 'eth0\n'
}
