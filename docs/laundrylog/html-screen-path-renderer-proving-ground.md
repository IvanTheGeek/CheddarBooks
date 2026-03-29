# LaundryLog HTML Screen Path Renderer Proving Ground

This note captures the first deterministic HTML/CSS screen-path renderer for LaundryLog.

It is separate from the current Event Modeling path renderer on purpose.

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
- keep one self-contained HTML file per path in the scratch work area for now

## Current First Path

The first screen-path proving ground is:

- `PATH1`
  `App Started -> Need Location -> First Entry`

It currently walks through:

1. `AppStarted`
2. `Need Location`
3. `Ready To Set Location`
4. `Entry Form Ready`
5. `Washer Draft`
6. `Logged Success`

This is intentionally still small.

It is enough to pressure:

- app/system startup
- route-to-screen flow
- screen-by-screen state changes
- where app mechanics are different from business `AEM` slices

## Relationship To The Screen Renderer

The current screen-path renderer reuses the actual LaundryLog screen renderer for the app screens.

That means:

- the screen proving ground remains the source of current screen composition
- the screen-path proving ground is the ordered path projection over those screens
- the path page can now expose a sticky horizontal scroll rail under the header so wide path rows can be panned without dropping to the browser's bottom scrollbar

## Near-Term Direction

The next likely moves are:

- refine the order and notes of `PATH1`
- add more path-specific screen states where the path exposes real transitions
- clarify the app/system lens beside `AEM`
- later decide how these screen-path steps should weave with the existing Event Modeling path surfaces
