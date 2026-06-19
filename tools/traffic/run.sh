#!/usr/bin/env bash
set -euo pipefail

# Entry-point wrapper for traffic tooling.
#
# Responsibilities:
# - route command to replay or composer mode,
# - keep CLI stable in one script, and
# - delegate heavy logic to specialized scripts.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"

# Print command usage summary.
usage() {
  cat <<'EOF'
Usage:
  run.sh replay --pcap <file> [--iface <name>] [--pps <n>] [--loop <n>] [--dry-run]
  run.sh scenario --file <scenario.yaml> [--validate|--dry-run|--run]
  run.sh help
EOF
}

if [[ $# -lt 1 ]]; then
  usage
  exit 1
fi

# First token selects execution mode.
cmd="$1"
shift

case "${cmd}" in
  replay)
    # Thin pass-through for one-off PCAP runs.
    "${TRAFFIC_ROOT}/replay/replay.sh" "$@"
    ;;
  scenario)
    # Scenario mode delegates orchestration to the Python composer.
    action="run"
    scenario_file=""
    while [[ $# -gt 0 ]]; do
      case "$1" in
        --file)
          scenario_file="${2:-}"
          shift 2
          ;;
        --validate)
          action="validate"
          shift
          ;;
        --dry-run)
          action="dry-run"
          shift
          ;;
        --run)
          action="run"
          shift
          ;;
        *)
          die "Unknown scenario argument: $1"
          ;;
      esac
    done
    [[ -n "${scenario_file}" ]] || die "scenario requires --file <path>"
    python3 "${TRAFFIC_ROOT}/composer/traffic_compose.py" "${action}" --file "${scenario_file}"
    ;;
  help|-h|--help)
    usage
    ;;
  *)
    die "Unknown command: ${cmd}"
    ;;
esac
