# LaundryLog Browser Tests

This workspace holds the formal browser harness for the tracked LaundryLog HTML artifacts.

Use it for:

- real browser behavior on the checked-in `workspace/` HTML
- path navigation proof
- lens-toggle proof
- later update-monitor proof

Current target:

- [`../../workspace/laundrylog/html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html`](../../workspace/laundrylog/html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html)

Usual local flow:

1. refresh the tracked workspace HTML after renderer changes
   - [`../../scripts/laundrylog/refresh-workspace-html.sh`](../../scripts/laundrylog/refresh-workspace-html.sh)
   - or use the full serial verify helper:
     - [`../../scripts/laundrylog/verify-workspace-html.sh`](../../scripts/laundrylog/verify-workspace-html.sh)
2. install browser-test dependencies
   - `cd tests/browser && npm install`
3. install the managed Chromium browser
   - `npm run install:browsers`
4. run the browser tests
   - `npm test`

Working rule:

- `Expecto` remains the primary runner for F# model/renderer truth
- this workspace is the formal browser harness for DOM/layout/interaction truth
- test the tracked `workspace/` artifact first, not scratch files under `tmp/`

## Playwright MCP Rule

For interactive Playwright MCP browser work:

- do not open the tracked artifact with `file://` first
- the MCP browser sandbox blocks `file:` URLs
- serve the tracked workspace HTML tree over local HTTP and target `http://127.0.0.1/...` instead

Short rule:

- manual review may still use `file://` when helpful
- formal Playwright browser work should use local HTTP
- when running the full LaundryLog verification path, prefer the checked-in verify helper so artifact refresh finishes before Playwright starts
