# LaundryLog HTML Screen Renderer Proving Ground

This note captures the first deterministic HTML/CSS screen-renderer direction for LaundryLog.

It sits beside the HTML path renderer, but it serves a different purpose:

- the path renderer proves slice and path semantics
- the screen renderer proves reusable page and component composition for the actual app screens

## Tracked Workspace Artifact

The current checked-in screen artifact lives at:

- [`../../workspace/laundrylog/html/screens/LaundryLog_ScreenComponents.html`](../../workspace/laundrylog/html/screens/LaundryLog_ScreenComponents.html)

Scratch or quick-regeneration copies may still appear under `tmp/`, but the repo-tracked workspace artifact is the one that should stay reviewable through git history.

## Current Working Rule

For the current LaundryLog screen work:

- screen meaning still comes from the repo docs and typed primitive state
- Penpot remains real design evidence, not the automatic generator
- the first deterministic page surface is self-contained HTML/CSS
- the screen renderer should stay built from reusable component blocks, not one-off page markup
- when refining the HTML/CSS renderer, inspect the actual source and generated artifact first rather than guessing from memory or screenshots alone
- if a visual/layout bug is fixed, add or update a regression test for the renderer contract that caused it

## Current Source Pressure

The current first screen renderer is grounded in:

- [`screens.md`](screens.md)
- [`concept-ui-page.md`](concept-ui-page.md)
- [`claude-design-nuances.md`](claude-design-nuances.md)
- tracked reference artifact: [`../../workspace/laundrylog/html/reference/laundrylog-v7.html`](../../workspace/laundrylog/html/reference/laundrylog-v7.html)
- [`penpot-screen-evidence.md`](penpot-screen-evidence.md)
- [`fnhci-primitive-map.md`](fnhci-primitive-map.md)
- [`fnhci-primitive-state-shapes.md`](fnhci-primitive-state-shapes.md)

That means the current screen renderer is:

- Penpot-informed
- historical-chat-informed
- anchored to the recovered `laundrylog-v7.html` mobile visual direction
- primitive-state-driven
- reviewable
- deterministic

## Current Reusable Block Set

The first HTML screen renderer is now using these reusable block ideas:

- `HeaderBar`
- `Panel`
- `TextInput`
- `ActionButton`
- `OptionGroup`
- `Stepper`
- `MoneyInput`
- `SummaryBar`
- `EntryCard`

These are still local proving-ground names and shapes.

The goal is to make the right seams obvious before wider extraction into shared `FnTools`.

## Current Screen Set

The current screen proving ground renders:

- `Screen.EntryForm - v7 Primary`
- `Screen.NewSession - Awaiting Location`
- `Screen.NewSession - Ready To Set`
- `Screen.EntryForm - Washer Draft`
- `Screen.EntryForm - Card Details Expanded`
- `Screen.EntryForm - Logged Success`

This is intentionally small.

It is enough to pressure:

- location-first flow
- command-centered screen layout
- mobile-first component sizing
- summary visibility
- repeated-entry screen composition
- payment progressive disclosure
- post-submit feedback state

## Restored Claude-Era Nuances

The current screen renderer now restores a first pass of several LaundryLog nuances that were previously flattened:

- top readiness chips for `Location`, `Type`, and `Payment`
- lighter session-total bar placed near the primary action area
- quarter-style `±25¢` price-adjust buttons
- payment progressive disclosure when card details are needed
- visible logged-state feedback through toast plus `✓ Logged!`

These are still proving-ground implementations, but they are now structural rather than one-off styling.

## Current Visual Anchor

For the mobile LaundryLog screen renderer, the current visual source of truth is:

- [`../../workspace/laundrylog/html/reference/laundrylog-v7.html`](../../workspace/laundrylog/html/reference/laundrylog-v7.html)

Future refinements should treat the current HTML renderer as successful only insofar as it converges toward that `v7` look-and-feel while still staying typed, deterministic, and built from reusable seams.

That now has a concrete presentation rule:

- the `Screen.EntryForm - v7 Primary` surface is the primary mobile app screen
- the other current screens render as supporting state variants beneath it
- the proving ground should read like `the app first, variant references second`, not like a flat gallery of equal-weight screens

## Still Not Fully Restored

The current screen renderer still does not fully recover everything from the older LaundryLog design work.

The main remaining gaps are:

- the exact spacing and tonal nuance of the status row
- richer meaning in the price helper chips
- the final placement and behavior of session total versus recent entries on shorter screens
- the fuller repeated-entry feel after logging multiple items in one session

## Relationship To The Path Renderer

The current intended layering is:

1. path and Event Modeling semantics
2. primitive-state seam
3. deterministic path renderer
4. deterministic screen renderer
5. later extraction into wider `FnHCI` / `FnUI` surfaces

The path renderer and the screen renderer should influence each other, but they should not collapse into one artifact.

## Near-Term Direction

The next likely moves are:

- refine the reusable screen blocks against the current LaundryLog screens
- connect the screen renderer more directly to current mapped view state
- keep the screen component shapes mobile-first while still reviewable on laptop and desktop
- pressure which blocks should later become shared `FnHCI` / `FnUI` components
