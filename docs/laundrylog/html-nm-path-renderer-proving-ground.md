# LaundryLog HTML NM Path Renderer Proving Ground

This note tracks the active broader `NM` path surface for LaundryLog.

Primary tracked review artifact:

- [`../../workspace/laundrylog/html/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html`](../../workspace/laundrylog/html/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html)

Current purpose:

- broaden the AEM slice-shell idea into a first ATLAS/NM modeling surface
- keep one ordered horizontal column per explicit state change
- allow multiple bounded contexts to appear in one ordered path
- treat NM as the only active path/modeling surface

Current slice law:

- `COMMAND` slices model change
- `VIEW` slices model read / interpretation
- `EVENT` is the backbone element inside the model, not a whole-column slice type
- `COMMAND` slices visibly carry:
  - optional attachment
  - `COMMAND`
  - `EVENT`
  - `COMMAND GWT`
- `VIEW` slices visibly carry:
  - optional attachment
  - `VIEW`
  - `VIEW GWT`

Presentation rule for humans and AI:

- do not describe the NM path as a fixed linear `COMMAND -> EVENT -> VIEW` triplet
- describe it as:
  - a `COMMAND` slice that produces an `EVENT`
  - later `VIEW` slice(s) that consume prior event(s) through `VIEW GWT`
- the consumed event does not need to come from the immediately previous slice
- multiple `VIEW` slices may consume the same prior event once it exists in the store

Current startup naming read:

- `Step 01-launch-app`
  - slice title: `LaunchApp`
  - produced event: `AppStarted`
- later startup views such as `SplashVisible` should be described as view consumers of prior events, not as the third link in a forced one-step chain

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

Scenario disclosure rule:

- the scenario chip/disclosure is independent from `Summary` and `Detailed`
- `Detailed` can show richer column metadata without forcing the scenario open
- opening or closing the scenario remains a direct user action on the scenario disclosure itself

Current surface rule:

- the top surface slot defaults to `Thumbnail`
- `Full` expands the slot height across the page
- clicking a surface preview opens a full overlay for that individual column
- `Thumbnail` should render as a small static schematic preview, not as a live mini-app
- keep the live DOM surface for `Full` mode and the overlay instead of trying to make one shrunken live render serve both jobs
- use human-first screen titles like `Splash Screen`, `Set Location Screen`, and `Laundry Entry Screen` in the `SCREEN` compartment; keep technical renderer ids as secondary detail only

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

Legacy note:

- the older screen-path artifact remains tracked as archived reference only
- it is no longer the active renderer/test contract
