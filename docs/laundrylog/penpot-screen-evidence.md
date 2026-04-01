# CheddarBooks LaundryLog Penpot Screen Evidence

This note records what the current `LaundryLog.penpot` file is already proving for the repo.

The purpose is practical:

- keep the Penpot file from becoming an opaque side artifact
- record what the current screens actually contain
- make clear what is hand-mapped today versus what is not automated yet

## Current Source Artifact

Current inspected source:

- `/home/ivan/NEXUS/tmp/LaundryLog.penpot`

Current working rule:

- the Penpot file is real design evidence
- the repo hand-maps from that evidence into docs and local UI shapes
- the repo does not yet treat the Penpot file as an automatic source generator

## Current Structure Confirmed From The File

The current file is a ZIP-based `.penpot` archive with readable JSON inside.

The current file contains:

- top-level file name `LaundryLog`
- page `Screens`
- frame `Screen.NewSession`
- frame `Screen.EntryForm`
- component `Button.Counter`

## Current Screen Evidence

From the current `Screens` page, the file presently includes:

- multiple `Screen.NewSession` frames at `375 x 667`
- one `Screen.EntryForm` frame at `375 x 1384`

That confirms the current design pressure is:

- mobile-first
- narrow phone-width baseline
- a much taller scrolling command surface for the entry form

## Current Visible Labels Confirmed

The current archive also contains these visible labels and options:

- `LaundryLog`
- `Use GPS Location`
- `Set Location`
- `Washer`
- `Dryer`
- `Supplies`
- `Cash`
- `Card`
- `App`
- `Points`

This matters because the current primitive work should not drift away from the actual design artifact.

## What We Are Using The File For Right Now

The current Penpot file is now being used as evidence for:

- the named screen surfaces
- the first primitive map
- the first local primitive state shapes
- the first example primitive compositions
- the first mobile-first visual baseline

## What Is Not True Yet

The repo is not yet doing any of these automatically:

- parsing the `.penpot` file into deterministic app state
- generating `FnHCI` primitives from Penpot boards
- generating `FnUI` runtime code from Penpot
- treating Penpot prototypes as the sole semantic source of truth

So the current state should be understood as:

- Penpot-informed
- hand-mapped
- reviewable
- not yet compiler-like

## Current Relationship To Repo Surfaces

Use this note together with:

- [Concept UI Page](concept-ui-page.md)
- [Screens](screens.md)
- [FnHCI Primitive Map](fnhci-primitive-map.md)
- [FnHCI Primitive State Shapes](fnhci-primitive-state-shapes.md)

## Next Likely Steps

1. keep refining the primitive map against real Penpot component structure
2. add more Penpot-backed example states as the path work grows
3. identify the first stable Penpot-to-`FnHCI` mapping rules
4. later build deterministic extraction or translation tooling
