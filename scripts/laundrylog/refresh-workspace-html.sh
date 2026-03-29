#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

dotnet build "${REPO_ROOT}/CheddarBooks.slnx"
dotnet fsi --exec "${SCRIPT_DIR}/refresh-workspace-html.fsx"
