# LaundryLog HTML NM Path Renderer Proving Ground

This note tracks the first broader `NM` path surface beside the existing screen-path and AEM path pages.

Primary tracked review artifact:

- [`../../workspace/laundrylog/html/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html`](../../workspace/laundrylog/html/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html)

Current purpose:

- broaden the AEM slice-shell idea into a first ATLAS/NM modeling surface
- keep one ordered horizontal column per explicit state change
- allow multiple bounded contexts to appear in one ordered path
- keep the existing screen-path page as a fallback surface while the NM shape is explored

Current `PATH1 NM` carries:

- `ApplicationLifecycle`
- `RuntimeOrchestration`
- `ScreenPath`
- `EventModeling`

Current header controls:

- `Summary`
- `Detailed`
- `Lifecycle`
- `Runtime`
- `Screen`
- `AEM`
- `Thumbnail`
- `Full`
- `Notify Me`
- `Auto Refresh`

Current surface rule:

- the top surface slot defaults to `Thumbnail`
- `Full` expands the slot height across the page
- clicking a surface preview opens a full overlay for that individual column

Important renderer guidance learned here:

- do not wrap a full interactive DOM surface preview in a literal `<button>`
- the preview content already contains buttons, inputs, and other controls, so a button wrapper creates invalid nested interactive HTML
- when that happened in the NM page, the browser restructured the DOM and the preview escaped the intended thumbnail shell, which made the scale and movement look broken
- the correct seam is:
  - use a non-button activator wrapper with `role="button"` and keyboard handling
  - keep the live surface preview inside a clipped preview frame
  - absolutely position the scaled preview so the slot height stays deterministic

Verification rule:

- refresh and verify with [`../../scripts/laundrylog/verify-workspace-html.sh`](../../scripts/laundrylog/verify-workspace-html.sh)
- do not verify the NM page ad hoc when the checked-in helper already exists

Formal browser proof:

- [`../../tests/browser/specs/path1-nm-path.spec.ts`](../../tests/browser/specs/path1-nm-path.spec.ts)

The NM page is intentionally additive right now.

It should evolve without removing the existing screen-path artifact until the broader NM shape proves itself useful.
