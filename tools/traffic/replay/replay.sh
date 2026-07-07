#!/usr/bin/env bash
set -euo pipefail

# Single-PCAP replay runner around tcpreplay.
#
# This script validates CLI parameters and environment,
# resolves target interface, and executes a replay command.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "${SCRIPT_DIR}/../lib/common.sh"

# Print replay mode usage help.
usage() {
  cat <<'EOF'
Usage:
  replay.sh --pcap <file> [--iface <name>] [--pps <n>] [--loop <n>] [--dry-run]

Options:
  --pcap <file>   Capture file to replay (.pcap/.pcapng/.cap)
  --iface <name>  Interface to replay on (default: TRAFFIC_INTERFACE or eth0)
  --pps <n>       Packets per second throttle (default: 200)
  --loop <n>      Number of replay iterations (default: 1)
  --dry-run       Print resolved command without executing
EOF
}

PCAP_FILE=""
IFACE=""
PPS="${TRAFFIC_REPLAY_PPS:-200}"
LOOP_COUNT="${TRAFFIC_REPLAY_LOOP:-1}"
DRY_RUN=0

# Parse CLI options.
while [[ $# -gt 0 ]]; do
  case "$1" in
    --pcap)
      PCAP_FILE="${2:-}"
      shift 2
      ;;
    --iface)
      IFACE="${2:-}"
      shift 2
      ;;
    --pps)
      PPS="${2:-}"
      shift 2
      ;;
    --loop)
      LOOP_COUNT="${2:-}"
      shift 2
      ;;
    --dry-run)
      DRY_RUN=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      die "Unknown argument: $1"
      ;;
  esac
done

[[ -n "${PCAP_FILE}" ]] || die "--pcap is required"
require_file "${PCAP_FILE}"

# Resolve interface precedence (arg -> env -> default).
IFACE="$(resolve_iface "${IFACE}")"

[[ "${PPS}" =~ ^[0-9]+$ ]] || die "--pps must be a positive integer"
[[ "${LOOP_COUNT}" =~ ^[0-9]+$ ]] || die "--loop must be a positive integer"
(( PPS > 0 )) || die "--pps must be greater than 0"
(( LOOP_COUNT > 0 )) || die "--loop must be greater than 0"

TCREPLAY_CMD=(
  tcpreplay
  --intf1 "${IFACE}"
  --pps "${PPS}"
  --loop "${LOOP_COUNT}"
  "${PCAP_FILE}"
)

if (( DRY_RUN == 1 )); then
  log "Dry run command:"
  printf '[traffic-lab] NOTE: tcpreplay binary check skipped in dry-run mode\n'
  printf '  %q' "${TCREPLAY_CMD[@]}"
  printf '\n'
  exit 0
fi

require_cmd tcpreplay

log "Replaying PCAP file: ${PCAP_FILE}"
log "Interface: ${IFACE} | PPS: ${PPS} | Loop: ${LOOP_COUNT}"
"${TCREPLAY_CMD[@]}"
log "Replay finished"
