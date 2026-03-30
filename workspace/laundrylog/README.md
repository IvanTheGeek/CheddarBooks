# LaundryLog Workspace

This workspace holds checked-in, human-viewable LaundryLog artifacts that should be reviewable directly from the repo and traceable through git history.

This is the primary review surface for current LaundryLog HTML artifacts.

If you want to open and review the current renderer output, start here rather than in `tmp/`.

Use this area for:

- current self-contained HTML artifacts that represent the latest meaningful renderer state
- local design/reference HTML that should remain visible in repo history
- companion files that the checked-in HTML artifacts depend on, such as update manifests

Current structure:

- `html/aem-paths/`
  Adam/Event Modeling (`AEM`) HTML path artifacts
- `html/nm-paths/`
  broader `NM` / ATLAS-style modeling path artifacts
- `html/screen-paths/`
  ordered screen-path HTML artifacts
- `html/screens/`
  current screen-surface HTML artifacts
- `html/reference/`
  tracked design/reference HTML inputs that still matter

Refresh helper:

- [`../../scripts/laundrylog/refresh-workspace-html.sh`](../../scripts/laundrylog/refresh-workspace-html.sh)
- [`../../scripts/laundrylog/refresh-workspace-html.fsx`](../../scripts/laundrylog/refresh-workspace-html.fsx)

Verification helper:

- [`../../scripts/laundrylog/verify-workspace-html.sh`](../../scripts/laundrylog/verify-workspace-html.sh)

Formal browser harness:

- [`../../tests/browser/README.md`](../../tests/browser/README.md)

Working rule:

- review the checked-in artifacts in this workspace first
- treat this workspace as the durable current artifact surface
- `tmp/` remains scratch and should only be used when there is a specific reason to investigate local regeneration details, experiments, screenshots, or transient outputs
- this workspace holds the checked-in current artifacts worth preserving in git history
- refresh the tracked HTML with the checked-in helper under `scripts/laundrylog/` rather than ad hoc shell snippets
- when you want the full current LaundryLog verification flow, prefer the checked-in verify helper so refresh, Expecto, and Playwright run in the right serial order
- when renderer or artifact behavior changes materially, refresh the relevant workspace artifact in the same commit as the code/docs change
- do not force separate artifact-only commits just to keep the workspace current
- for formal browser proof, target these tracked workspace artifacts through the Playwright workspace under `tests/browser/`
- for recurring LaundryLog verification, AI should default to the checked-in helper scripts in `scripts/laundrylog/` rather than reconstructing the flow by hand

Current first review targets:

- [`html/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html`](html/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html)
- [`html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html`](html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html)
- [`html/screens/LaundryLog_ScreenComponents.html`](html/screens/LaundryLog_ScreenComponents.html)
- [`html/aem-paths/LaundryLog_PATH1_CommandSlice_ViewSlice.html`](html/aem-paths/LaundryLog_PATH1_CommandSlice_ViewSlice.html)
