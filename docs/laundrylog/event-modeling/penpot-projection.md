# LaundryLog Penpot Projection

This note captures the current intended relationship between LaundryLog Event Modeling work and Penpot.

## Current Rule

Penpot should be treated as a projection surface for the Event Modeling work, not the deepest semantic source of truth.

That means:

- the durable business meaning still lives in repo memory
- the live Penpot backend is the current working state for the board
- an exported `.penpot` file is a snapshot of that state, not the live store
- Penpot is where we project that meaning into a visual board
- the board should be optimized for reading, discussion, and iteration
- the board does not need to carry every semantic concern directly inside the Penpot file

## What Stays Canonical Elsewhere

The more canonical semantic surfaces are still:

- `PATH` notes
- `CommandSlice` notes
- `Event` notes
- `ViewSlice` and `View` notes
- concrete example data
- later, code-facing models where they become stable enough

For LaundryLog, that currently means this Event Modeling area remains the deeper source:

- [README](README.md)
- [Starting Point](starting-point.md)
- [PATH 1](path-1-first-entry.md)
- [Commands And CommandSlices](commands.md)
- [Events](events.md)
- [Views, ViewSlices, And Business Screenshots](views.md)

## What Penpot Can Own Well

Penpot is a good place to hold the visual projection of:

- the slice layout
- the lane layout
- the card shapes and color language
- the current lens being shown
- the relationship between the business view and the visible screen
- screen-to-screen click-through prototypes where that helps

This makes Penpot a good home for:

- executive overview boards
- working Event Modeling boards
- UI path mockups
- visual comparison between business flow and screen flow

## Different Readers, Different Lenses

The same Penpot board will often be read differently by different people.

Examples:

- a developer may look for implementation seams
- a UX contributor may look for state and screen changes
- a designer may look for component and token pressure
- a customer or product owner may look for task completion and friction

That means one Penpot board should not be expected to carry every concern equally well at the same time.

The better rule is:

- let the board serve a chosen lens well
- and let other concerns pressure the model through other related surfaces

## Current Page Split

For LaundryLog, the current Penpot page split should be:

- `PATHS`
  primary readable projection, with each `PATH` laid out as its own row
- `Event Model`
  slice-card grammar, primitive refinement, and lens experimentation
- `Screens`
  app-page references and screenshot-oriented screen surfaces
- `Components`
  reusable component bases and token-governed design pieces

That keeps the overview board, the slice workshop, and the app screens from collapsing into one overloaded page.

## Working Interpretation

For this LaundryLog work, the likely pattern is:

1. define the business path durably in markdown
2. define the `CommandSlice`, `Event`, `ViewSlice`, and `View` meaning durably in markdown
3. project that into Penpot as a visual board
4. refine the board until the flow reads clearly
5. compare the Penpot projection back against the durable path notes
6. later bridge those same ideas into `FnHCI` / `FnUI`

That means Penpot is not replacing the semantic model.

It is the visual working surface for a selected lens over that model.

## Why This Matters

This keeps a healthier separation:

- durable meaning does not disappear into a design file
- the design file can stay pragmatic and readable
- we can create multiple projections for different lenses without pretending they are all the same thing

Examples of different projections we may want:

- CEO overview lens
- business Event Modeling lens
- screen/path lens
- component/design-system lens

## Screen Rule

Within a `PATH` row, the screen position should prefer:

- an actual app-page screenshot
- or a strong screenshot-like screen projection

That helps the reader connect the business path to the visible app surface without forcing the screen to become the source of truth.

## Early Penpot Board Guidance

The first Penpot Event Modeling board should probably aim for:

- one row or lane logic per role/lens, if that reads well visually
- one clear left-to-right path
- explicit `CommandSlice` and `ViewSlice` rhythm
- green `View` cards as business-state anchors
- blue/orange command/event pairs for action history
- light screen cards to show the visible interaction surface without letting the UI dominate the board

For the `PATHS` page specifically, the first board should aim for:

- one horizontal row per `PATH`
- enough width for desktop and tablet reading
- nearby notes or path descriptions where that improves comprehension
- an approach that can later expand to multiple actor lanes when needed

## Current Practical Rule

If the Penpot board and the durable Event Modeling notes disagree:

- fix the mismatch explicitly
- do not silently assume the Penpot board is authoritative

The best current reading is:

- markdown and code-facing notes carry the deeper meaning
- Penpot carries a chosen visual projection of that meaning

For the current concrete starting point in the existing `LaundryLog.penpot` file, see [Penpot Board Seed](penpot-board-seed.md).

For the current visual and historical inspiration references around this board work, see [Inspiration](inspiration.md).

## Current Live Workflow

The current practical working loop is:

1. edit the live Penpot file
2. use that live backend state as the current projection
3. export a `.penpot` file only when a checkpoint artifact is needed

So the exported file should be read as:

- portable
- inspectable
- useful

but still as a snapshot rather than the always-current live board.

## Responsive Viewing Direction

For the Event Modeling projection itself, current pressure points are:

- phone is not the primary target
- tablet and desktop are the primary targets
- a normal laptop width should read comfortably without feeling cramped
- a wide desktop view should be able to show a fuller path without artificial narrowness

So the Event Modeling board should be treated as:

- responsive enough to remain usable on smaller devices
- but intentionally optimized for tablet, desktop, and wide-screen reading

## Token Pressure

This board work should eventually be token-governed rather than hand-tuned card by card.

The first likely token concerns are:

- page gutter and row spacing
- slice card width
- lane gap
- title and body typography
- surface and border colors by slice type
- note-card and screenshot-card treatment
- breakpoint behavior for normal desktop versus extended-width views

That means the Event Modeling projection should stay aligned with the broader `FnHCI` / `FnUI` token direction instead of inventing a one-off visual system.

## Multiple Actor Lanes

The first LaundryLog path is still effectively single-actor.

If later paths introduce multiple actor roles, we should expect the projection rules to evolve toward:

- explicit actor lanes within a path row
- clearer cross-lane handoff cues
- more width pressure on the desktop view

So the current row-per-path rule is a strong starting point, not the final lane policy for every path.

## Current Component Pattern

For slice work, prefer this Penpot editing pattern:

1. duplicate a component base
2. evolve the duplicate into the new visual base
3. instantiate that evolved base back into the active board
4. adjust instance text or state where needed

That keeps the board component-driven instead of turning it into ad hoc one-off drawing.

## Current Component-State Lab

The live `LaundryLog` file now also includes `ComponentStateTokenLab.V1` as a focused lab for reusable component behavior.

Current verified findings from that lab:

- `Button.Option` instance text edits exported correctly
- `Button.Option` detached-instance text edits also exported correctly
- `Input.Text` instance text edits exported correctly
- `Input.Text` detached-instance text edits also exported correctly
- a fresh overlay label exported correctly once it was given an explicit visible size

That means the current export problem is narrower than \"all component-derived text is unreliable\".

The stronger current reading is:

- the recovered slice-card shells are the suspicious case
- ordinary reusable controls like buttons and inputs currently behave much better
- we should keep using component instances for normal reuse and state pressure whenever they work
- detaching should stay a deliberate escape hatch, not the default workflow

## Current Token Caution

The same live lab also showed that token work needs its own explicit verification.

Current verified caution:

- `shape.applyToken(...)` currently threw a Penpot-side `check error` in the live plugin path
- a first `token.applyToShapes(...)` probe did not yet give us a clean, inspectable token-binding result

So the current practical rule is:

- do not assume that dropping in a fresh local text box and reapplying tokens is already a solved workflow
- treat token-governed replacement text as an explicit lab concern
- re-verify token binding before we depend on it for component-state generation or later `FnHCI` / `FnUI` bridging

## Current Recovered Slice Language

The current visual direction is being pulled most strongly from the recovered EM-1 `Slice.html` lineage, especially `f63e1af`.

The language there is:

- a slice reads as a card, not as a tangled diagram
- the card has a strong header band
- the card carries a small type marker and system marker
- the semantic rows stay calm and readable
- the stronger title bar sits above a lighter structured data panel
- the overall page reads more like a designed product surface than a raw modeling tool

## Current V2 Penpot Bases

The live `LaundryLog` Penpot file now contains first-pass Event Modeling slice bases built from that recovered language:

- `CommandSlice.Base.V2`
- `ViewSlice.Base.V2`

And the first visible PATH 1 instances derived from them:

- `PATH1: CommandSlice - Set Location (V2 Instance)`
- `PATH1: ViewSlice - Ready (V2 Instance)`

The rough earlier first two PATH 1 cards were hidden rather than silently deleted so the transition stays traceable.

## Current PATHS Row Proof

The live `PATHS` page now also contains a first readable row projection:

- `PATH1.Row.V1`

Current characteristics:

- one horizontal row for `PATH 1`
- short descriptive note above the row
- alternating `CommandSlice` and `ViewSlice` cards across the path
- single-actor reading for now
- screen rows still acting as screenshot-like references rather than full embedded screenshots

So the `PATHS` page is now past the stage of being only screen boards and rough experiments.

## Current Surface Rule

For this component-driven slice work:

- plugin/MCP is currently the stronger live editing surface
- backend API is currently the stronger inspection/export surface

The API mutation seam still matters, but it is currently lower-level and more schema-sensitive than the plugin path for this kind of board evolution.

One additional practical finding from the current `PATH1.Row.V1` work is:

- detached component shells are a good base for readable path cards
- but directly editing inherited text did not reliably show up in exported visuals
- the current reliable approach is to hide the inherited text nodes and overlay fresh local text nodes for the card-specific content

That should be treated as a current Penpot workaround, not the final desired component pattern.

Current sharper reading from the text-mutation lab:

- fresh non-component text exports correctly
- direct edits to inherited text inside the current component-derived slice shells did not export correctly
- detaching alone did not fix that export problem
- renaming the inherited text node did not fix it either
- overlay text did render correctly

So for the longer-term `FnHCI` / `FnUI` direction, the healthier target is still:

- stay on real component instances where possible
- prefer variants or explicit stateful components over detaching when the change is really a state change
- use overlay text only where the current Penpot behavior forces it and record that choice explicitly

Current token caveat:

- the current slice-title texts in this experiment had no token bindings
- so the overlay workaround did not yet prove token-preserving behavior
- that means token-governed text surfaces should be treated as a separate concern that still needs explicit proof
