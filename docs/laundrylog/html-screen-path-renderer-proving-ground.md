# LaundryLog HTML Screen Path Renderer Proving Ground

This note captures the first deterministic HTML/CSS screen-path renderer for LaundryLog.

It is separate from the current Event Modeling path renderer on purpose.

## Tracked Workspace Artifact

The current checked-in screen-path artifact lives at:

- [`../../workspace/laundrylog/html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html`](../../workspace/laundrylog/html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html)
- [`../../workspace/laundrylog/html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.update.js`](../../workspace/laundrylog/html/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.update.js)

Usual refresh helper:

- [`../../scripts/laundrylog/refresh-workspace-html.sh`](../../scripts/laundrylog/refresh-workspace-html.sh)

Formal browser harness:

- [`../../tests/browser/README.md`](../../tests/browser/README.md)

Scratch copies may still be regenerated under `tmp/`, but the workspace copy is the checked-in current path surface.

## Lens Split

For LaundryLog, there are now at least two useful path lenses:

- `AEM`
  Adam/Event Modeling as the business-flow lens
- `screen path lens`
  the ordered user-visible and app-visible screen-state flow

The current screen-path renderer is not a replacement for `AEM`.

It is a neighboring lens that helps model:

- app startup and route transitions
- visible screen states in sequence
- UI/system mechanics that are real but not well expressed as only business events
- how screen state changes across one real path

Later, these lenses should be able to pressure each other and be woven together.

## Shared Row Direction

The current proving ground is still mostly a horizontal strip of stacked step documents.

That is good enough for the first path, but the stronger direction is:

- one continuous horizontal path
- shared row bands across all step columns
- rows that can be turned on and off by lens
- row heights that stay coherent across the whole path whenever a lens is visible

That means later path surfaces should be able to add rows like:

- runtime/app-mechanics row
- screen row
- `AEM` row
- `ADM` row
- other future lens rows

without losing the shared path rhythm.

## Current Working Rule

For the current screen-path work:

- use actual typed screen states rather than fresh mockups where possible
- reuse the current screen renderer for real screen surfaces
- add local app/system surfaces only where the path needs something not yet represented by the normal app screens
- keep one self-contained HTML file per path in the tracked workspace area
- let scratch regeneration in `tmp/` remain optional and local

## Current First Path

The first screen-path proving ground is:

- `PATH1`
  `Fresh First Launch -> Need Location -> First Entry`

For this path, the scenario is explicit:

- fresh first launch with no known local data
- no saved location
- no active local session or pending draft
- runtime should resolve the app to `Need Location` before the first entry

It currently walks through:

1. `AppStarted`
2. `Runtime Checks`
3. `No Local Session`
4. `Route Resolved`
5. `Need Location`
6. `Ready To Set Location`
7. `Entry Form Ready`
8. `Washer Draft`
9. `Logged Success`

This is intentionally still small.

It is enough to pressure:

- application lifecycle visibility
- app runtime orchestration before the first usable screen
- app/system startup
- route-to-screen flow
- screen-by-screen state changes
- where app mechanics are different from business `AEM` slices

Current working rule for this path:

- each rendered step should state what changed from the previous step
- those change summaries belong to the path surface itself, not only to chat or external notes
- `Summary` view may hide that detail, but `Standard` and `Detailed` should keep the change list available

## View Modes

The current page now has first-pass viewer controls:

- `Summary`
  hides some supporting metadata and keeps the path focused on the visible columns
- `Standard`
  the default working mode
- `Detailed`
  shows the scenario assumptions beside the path

This is only a first step, but it establishes that a path surface should not assume one permanently fixed verbosity level.

## Lens Visibility

The current page now also has first-pass lens visibility controls in the header.

That means the page can turn lens families on and off, instead of only printing the lens label inside every step.

Current lens families:

- `Application Lifecycle Lens`
- `App Runtime Lens`
- `Screen Path Lens`

Working rule:

- the header controls decide which lens families are currently visible
- each visible step still states its own bounded context and lens explicitly
- the page should not force one permanently fixed lens mix
- when a path step uses authored layout like `.ll-path-step { display: flex; ... }`, do not rely on the browser's default `[hidden]` behavior alone; the renderer should emit an explicit author rule such as `.ll-path-step[hidden] { display: none !important; }` so filtered lens steps are actually removed from layout

## Step Metadata Hierarchy

For the current screen-path page, step metadata should distinguish at least:

- `bounded context`
- `lens`
- `surface`

Example:

- `Context · RuntimeOrchestration`
- `app runtime lens`
- `Screen.AppStart - Runtime Checks`

Current rule:

- `domain` is broader and usually implicit at the page/doc level here
- `bounded context` is the stronger per-step semantic label
- `lens` is the current viewing/projection perspective
- `surface` is the actual rendered screen/app surface being shown

So for this page:

- `SoftwareDevelopment` / interaction work is the broader domain pressure
- `ApplicationLifecycle`, `RuntimeOrchestration`, and `ScreenPath` are the bounded contexts being surfaced
- `Application Lifecycle Lens`, `App Runtime Lens`, and `Screen Path Lens` are the current viewer-selectable lenses
- `Screen.AppStart - Runtime Checks` and similar names are the rendered surfaces

## Local Update Monitor

The current `file://` path artifact now uses a small companion update manifest:

- the HTML artifact remains self-contained for the visible page
- a sibling `.update.js` file is regenerated alongside it
- the open page polls that sidecar script for a newer `version`
- when a newer build exists, the page can either:
  - notify that a refresh is available
  - or auto-refresh if the user chose that sticky mode

This pattern exists because a local `file://` page cannot be treated like a normal served web app:

- `fetch(window.location.href)` is not reliable enough in the current browser/file setup
- but loading a cache-busted sibling script file does work
- so the page can detect updates without needing a server
- the sibling update manifest should be checked in beside the tracked workspace HTML artifact whenever that artifact is refreshed

## Relationship To The Screen Renderer

The current screen-path renderer reuses the actual LaundryLog screen renderer for the app screens.

That means:

- the screen proving ground remains the source of current screen composition
- the screen-path proving ground is the ordered path projection over those screens
- the path page can now behave more like an app viewport:
  - browser-level scrollbars stay hidden
  - the header and top horizontal controls stay fixed
  - the path rows scroll inside an internal stage
  - first/previous/next/end controls can move the visible screen columns by whole-step boundaries instead of partial drift

## Browser Verification

The formal browser proof for this page now belongs to the local Playwright workspace:

- [`../../tests/browser/README.md`](../../tests/browser/README.md)

Working split:

- `Expecto`
  keeps covering typed state, renderer output invariants, and deterministic artifact generation
- `JS/TS Playwright Test`
  covers browser-only truth for the tracked workspace artifact, such as:
  - whole-column path navigation
  - start/end rail alignment
  - lens-toggle visibility behavior
  - local-storage-backed UI persistence

Current working rule:

- the tracked `workspace/` HTML is the browser-test target
- `file://` loading is still kept as a secondary smoke path
- the formal browser suite should primarily serve the tracked workspace artifact over a tiny local HTTP server instead of relying only on `file://`
- Playwright MCP should follow the same rule, because its browser sandbox blocks `file:` URLs even though manual local browser review may still open the artifact directly from disk

## Horizontal Navigation Guidance

For this path surface, "move one column" should mean:

- the next rendered screen column becomes the new leftmost visible column
- not "scroll by a guessed pixel amount"
- and not "stop at the browser's native content edge if that prevents the next whole column from aligning"

The current working rule is:

- measure the actual rendered step starts
- normalize them against the first rendered column
- compute the last logical left-edge target from what remaining content can still fit inside the viewport
- add explicit trailing space when needed so that last logical target is actually reachable
- size the top rail from that logical target range rather than from the native content width, so rail start/end match the readable path start/end
- when the rail uses stable scrollbar gutters, derive that width from the rail's full box width, not only its client width

This matters because a continuous horizontal screen path may need to leave a small blank tail at the far right in order to let the next whole column align at the left edge. The desired behavior is path readability, not strict native scroll-width purity.

For investigation/debugging:

- inspect the generated HTML/JS directly
- then verify the behavior in a real browser surface, not just by source reading
- prefer measured geometry over guessed widths when snap behavior matters

One concrete finding from this path:

- native `scrollTo({ behavior: "smooth" })` was not trustworthy enough for the whole-column path buttons here
- the stable pattern was to keep measured logical targets, then drive the viewport and top rail together with an explicit `requestAnimationFrame` animation

## Near-Term Direction

The next likely moves are:

- refine the order and notes of `PATH1`
- add more path-specific screen states where the path exposes real transitions
- clarify the app/system lens beside `AEM`
- later decide how these screen-path steps should weave with the existing Event Modeling path surfaces
