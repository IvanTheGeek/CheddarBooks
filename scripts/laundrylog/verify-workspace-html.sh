#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../.." && pwd)"
browser_tests_dir="$repo_root/tests/browser"

echo "==> Refreshing tracked LaundryLog workspace HTML"
"$script_dir/refresh-workspace-html.sh"

echo
echo "==> Running Expecto model/renderer tests"
dotnet run --project "$repo_root/tests/CheddarBooks.Tests/CheddarBooks.Tests.fsproj"

echo
echo "==> Running Playwright browser tests"
if [[ ! -d "$browser_tests_dir/node_modules" ]]; then
  echo "Playwright npm dependencies missing; installing them first..."
  (cd "$browser_tests_dir" && npm install)
fi

(cd "$browser_tests_dir" && npm test)
