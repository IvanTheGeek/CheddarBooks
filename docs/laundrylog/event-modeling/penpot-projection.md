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

## Early Penpot Board Guidance

The first Penpot Event Modeling board should probably aim for:

- one row or lane logic per role/lens, if that reads well visually
- one clear left-to-right path
- explicit `CommandSlice` and `ViewSlice` rhythm
- green `View` cards as business-state anchors
- blue/orange command/event pairs for action history
- light screen cards to show the visible interaction surface without letting the UI dominate the board

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
