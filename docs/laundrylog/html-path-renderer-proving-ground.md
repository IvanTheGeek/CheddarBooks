# LaundryLog HTML Path Renderer Proving Ground

This note captures the first deterministic HTML/CSS path-renderer direction for LaundryLog.

It exists because the recent Penpot exploration was useful research, but not reliable enough to be the primary working engine for path and slice rendering.

## Current Working Rule

For the near-term proving ground:

- the semantic source stays in repo docs and typed state
- the first deterministic projection surface is self-contained HTML/CSS
- SVG can be used later when vector output is actually needed
- Penpot becomes optional reference material, not the primary path engine

This keeps the current work reviewable and reproducible while `FnHCI` and later `FnUI` continue to take shape.

## Why HTML/CSS First

HTML/CSS is the preferred current canvas because it gives us:

- deterministic text and layout behavior
- straightforward inspection
- easy file-based artifacts for review
- a direct path toward web, WASM, server-hosted, and desktop-hosted UI surfaces

That makes it a better proving ground for the Bolero-replacement direction than continuing to fight Penpot rendering quirks.

## Current Local Boundary

The first renderer lives locally in:

- `CheddarBooks.LaundryLog.UI`

It is still a proving ground, not yet the final shared `FnTools` surface.

That means:

- keep it concrete
- keep it aligned with LaundryLog `PATH 1`
- pressure the shape until the right shared seams are obvious

## Current Rendered Model

The first local renderer introduces explicit path-renderer state for:

- `CommandSlice`
- `ViewSlice`
- rendered slice blocks such as `Screen`, `Command`, `Event`, and `View`
- `PathRow`
- rendering options for different lenses

It now also uses explicit CSS grid row contracts for the outer path row and shared slice shells, so:

- `CommandSlice` and `ViewSlice` can stay in horizontal rhythm
- screen, detail, and future supporting bands can share stable heights
- `GWT` can live as a deliberate row inside each slice frame instead of floating outside the slice

The current first path is:

- manual location capture
- washer expense
- dryer expense
- one embedded per-slice `GWT` section inside each slice frame

The current rule is:

- `GWT` is per slice, not per command/view pair
- `CommandSlice` and `ViewSlice` use `GWT` differently
- command-side `GWT` expresses command rules and outcomes
- view-side `GWT` expresses projection logic from facts into the visible `View`
- command-side `GWT` should prefer mini representations of the participating command and event surfaces when that improves readability
- view-side `GWT` can mix fact/view representations with short projection text
- the slice frame itself should include that slice's `GWT`, not push it into a separate board-wide band

## Lens Rule

The current HTML renderer already supports the start of lens selection:

- classic Event Modeling
  - green-box `View` only in `ViewSlice`
- UI-enriched
  - allows the resultant screen to be shown beside the `View`

This matters because the fuller model direction should not force every audience to read the same projection.

## Relationship To Existing LaundryLog Notes

This renderer sits on top of:

- [`event-modeling/path-1-first-entry.md`](event-modeling/path-1-first-entry.md)
- [`event-modeling/views.md`](event-modeling/views.md)
- [`command-view-primitive-seam.md`](command-view-primitive-seam.md)
- [`view-contracts.md`](view-contracts.md)
- [`fnhci-primitive-state-shapes.md`](fnhci-primitive-state-shapes.md)

So the current order is:

1. Event Modeling and path semantics
2. command/view to primitive seam
3. deterministic HTML/CSS projection
4. later shared `FnHCI` and `FnUI` extraction

## Near-Term Direction

The next likely moves are:

- refine the slice-card visual language
- keep shared row contracts strong enough for later `GWT` and scenario bands that remain local to the slice frame
- decide which `GWT` bands are always visible versus lens-controlled
- decide how much property detail a compact `GWT` block reference should show by default
- keep the projection deterministic and self-contained
- pressure which slice and badge concepts should later migrate into shared `FnTools`
- keep the current LaundryLog proving ground honest before extracting a wider reusable engine
