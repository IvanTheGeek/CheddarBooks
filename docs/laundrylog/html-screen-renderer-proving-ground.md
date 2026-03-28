# LaundryLog HTML Screen Renderer Proving Ground

This note captures the first deterministic HTML/CSS screen-renderer direction for LaundryLog.

It sits beside the HTML path renderer, but it serves a different purpose:

- the path renderer proves slice and path semantics
- the screen renderer proves reusable page and component composition for the actual app screens

## Current Working Rule

For the current LaundryLog screen work:

- screen meaning still comes from the repo docs and typed primitive state
- Penpot remains real design evidence, not the automatic generator
- the first deterministic page surface is self-contained HTML/CSS
- the screen renderer should stay built from reusable component blocks, not one-off page markup

## Current Source Pressure

The current first screen renderer is grounded in:

- [`screens.md`](screens.md)
- [`concept-ui-page.md`](concept-ui-page.md)
- [`claude-design-nuances.md`](claude-design-nuances.md)
- [`penpot-screen-evidence.md`](penpot-screen-evidence.md)
- [`fnhci-primitive-map.md`](fnhci-primitive-map.md)
- [`fnhci-primitive-state-shapes.md`](fnhci-primitive-state-shapes.md)

That means the current screen renderer is:

- Penpot-informed
- historical-chat-informed
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

- `Screen.NewSession - Awaiting Location`
- `Screen.NewSession - Ready To Set`
- `Screen.EntryForm - Washer Draft`

This is intentionally small.

It is enough to pressure:

- location-first flow
- command-centered screen layout
- mobile-first component sizing
- summary visibility
- repeated-entry screen composition

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
