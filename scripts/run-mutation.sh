#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────────────
# run-mutation.sh — Run Stryker mutation tests for all core projects
#
# Usage:
#   ./scripts/run-mutation.sh
#
# Requirements:
#   - .NET SDK installed
#   - dotnet-stryker global tool (auto-installed if missing)
# ──────────────────────────────────────────────────────────────────────────────
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CORE_PROJECTS=(
  "src/core/MarcusPrado.Platform.Abstractions"
  "src/core/MarcusPrado.Platform.Application"
  "src/core/MarcusPrado.Platform.BackgroundJobs"
  "src/core/MarcusPrado.Platform.Contracts"
  "src/core/MarcusPrado.Platform.Domain"
  "src/core/MarcusPrado.Platform.Resilience"
  "src/core/MarcusPrado.Platform.Security"
)

# ─── Install dotnet-stryker if not already installed ─────────────────────────
echo "──────────────────────────────────────────────────────────────────────────"
echo "  Checking dotnet-stryker installation..."
echo "──────────────────────────────────────────────────────────────────────────"
dotnet tool install -g dotnet-stryker 2>/dev/null || true

# ─── Run Stryker for each core project ───────────────────────────────────────
declare -A RESULTS
FAILED_PROJECTS=()

for PROJECT_REL in "${CORE_PROJECTS[@]}"; do
  PROJECT_DIR="${REPO_ROOT}/${PROJECT_REL}"
  PROJECT_NAME="$(basename "${PROJECT_DIR}")"

  echo ""
  echo "══════════════════════════════════════════════════════════════════════════"
  echo "  Running mutation tests: ${PROJECT_NAME}"
  echo "  Directory: ${PROJECT_DIR}"
  echo "══════════════════════════════════════════════════════════════════════════"

  if [ ! -f "${PROJECT_DIR}/stryker-config.json" ]; then
    echo "  [SKIP] No stryker-config.json found in ${PROJECT_REL}"
    RESULTS["${PROJECT_NAME}"]="SKIPPED"
    continue
  fi

  pushd "${PROJECT_DIR}" > /dev/null

  if dotnet stryker; then
    RESULTS["${PROJECT_NAME}"]="PASSED"
  else
    RESULTS["${PROJECT_NAME}"]="FAILED"
    FAILED_PROJECTS+=("${PROJECT_NAME}")
  fi

  popd > /dev/null
done

# ─── Summary ─────────────────────────────────────────────────────────────────
echo ""
echo "══════════════════════════════════════════════════════════════════════════"
echo "  Mutation Testing Summary"
echo "══════════════════════════════════════════════════════════════════════════"
for PROJECT_NAME in "${!RESULTS[@]}"; do
  STATUS="${RESULTS[${PROJECT_NAME}]}"
  case "${STATUS}" in
    PASSED)  ICON="✔" ;;
    FAILED)  ICON="✘" ;;
    SKIPPED) ICON="–" ;;
    *)       ICON="?" ;;
  esac
  printf "  %s  %-60s %s\n" "${ICON}" "${PROJECT_NAME}" "${STATUS}"
done
echo "──────────────────────────────────────────────────────────────────────────"

if [ "${#FAILED_PROJECTS[@]}" -gt 0 ]; then
  echo ""
  echo "  FAILED projects:"
  for P in "${FAILED_PROJECTS[@]}"; do
    echo "    - ${P}"
  done
  echo ""
  echo "  One or more projects did not meet mutation score thresholds."
  echo "  HTML reports are saved in each project's StrykerOutput/ directory."
  exit 1
else
  echo ""
  echo "  All mutation tests passed (or were skipped)."
  echo "  HTML reports are saved in each project's StrykerOutput/ directory."
fi
