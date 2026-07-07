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
  run.sh build-pcap --file <scenario.yaml> [--output <file.pcap>] [--dry-run]
  run.sh build-all-pcaps [--dry-run]
  run.sh validate-pcaps [--file <scenario.yaml>] [--pcap <file.pcap>]
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
  build-pcap)
    scenario_file=""
    output_file=""
    dry_run=0
    while [[ $# -gt 0 ]]; do
      case "$1" in
        --file)
          scenario_file="${2:-}"
          shift 2
          ;;
        --output)
          output_file="${2:-}"
          shift 2
          ;;
        --dry-run)
          dry_run=1
          shift
          ;;
        *)
          die "Unknown build-pcap argument: $1"
          ;;
      esac
    done
    [[ -n "${scenario_file}" ]] || die "build-pcap requires --file <path>"
    build_args=(--file "${scenario_file}")
    if [[ -n "${output_file}" ]]; then
      build_args+=(--output "${output_file}")
    fi
    if (( dry_run == 1 )); then
      build_args+=(--dry-run)
    fi
    python3 "${TRAFFIC_ROOT}/composer/traffic_scenario_pcap.py" "${build_args[@]}"
    ;;
  build-all-pcaps)
    dry_run=0
    while [[ $# -gt 0 ]]; do
      case "$1" in
        --dry-run)
          dry_run=1
          shift
          ;;
        *)
          die "Unknown build-all-pcaps argument: $1"
          ;;
      esac
    done
    shopt -s nullglob
    scenario_files=("${TRAFFIC_ROOT}/scenarios/"*.yaml)
    shopt -u nullglob
    [[ ${#scenario_files[@]} -gt 0 ]] || die "No scenario files found under scenarios/"
    for scenario_file in "${scenario_files[@]}"; do
      echo "==> build-pcap ${scenario_file}"
      build_args=(--file "${scenario_file}")
      if (( dry_run == 1 )); then
        build_args+=(--dry-run)
      fi
      python3 "${TRAFFIC_ROOT}/composer/traffic_scenario_pcap.py" "${build_args[@]}"
    done
    ;;
  validate-pcaps)
    validate_args=()
    while [[ $# -gt 0 ]]; do
      case "$1" in
        --file)
          validate_args+=(--file "${2:-}")
          shift 2
          ;;
        --pcap)
          validate_args+=(--pcap "${2:-}")
          shift 2
          ;;
        *)
          die "Unknown validate-pcaps argument: $1"
          ;;
      esac
    done
    python3 "${TRAFFIC_ROOT}/composer/traffic_scenario_pcap_validate.py" "${validate_args[@]}"
    ;;
  help|-h|--help)
    usage
    ;;
  *)
    die "Unknown command: ${cmd}"
    ;;
esac
