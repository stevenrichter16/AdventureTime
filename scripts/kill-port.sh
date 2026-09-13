#!/usr/bin/env bash
# Kill whatever process is listening on a TCP port, without needing to look up a PID.
#
# Usage:
#   scripts/kill-port.sh          # kills whatever is on port 5256 (the API's default port)
#   scripts/kill-port.sh 5173     # kills whatever is on a different port instead
#
# Common cause: Ctrl+Z suspends `dotnet run` instead of stopping it, which leaves
# the process alive in the background still holding the port. Use Ctrl+C to stop
# it cleanly in the first place; this script is the fallback for when that ship
# has already sailed.

set -euo pipefail

PORT="${1:-5256}"

if ! command -v lsof >/dev/null 2>&1; then
  echo "error: lsof is required but not found on PATH" >&2
  exit 1
fi

pids="$(lsof -ti "tcp:${PORT}" || true)"

if [[ -z "$pids" ]]; then
  echo "nothing is listening on port ${PORT}"
  exit 0
fi

echo "port ${PORT} is in use by: $(echo "$pids" | tr '\n' ' ')"
echo "$pids" | xargs kill

# Give it a moment to shut down gracefully, then force anything still holding the port.
sleep 1
remaining="$(lsof -ti "tcp:${PORT}" || true)"
if [[ -n "$remaining" ]]; then
  echo "still listening after a graceful kill, forcing: $(echo "$remaining" | tr '\n' ' ')"
  echo "$remaining" | xargs kill -9
  sleep 1
fi

if lsof -ti "tcp:${PORT}" >/dev/null 2>&1; then
  echo "error: port ${PORT} is still in use" >&2
  exit 1
fi

echo "port ${PORT} is free"
