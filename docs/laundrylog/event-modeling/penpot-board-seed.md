# LaundryLog Penpot Board Seed

This note records the current Penpot seed for the LaundryLog Event Modeling projection.

It exists so the board structure does not have to live only in memory or in the Penpot file itself.

## Current Seed

The current `LaundryLog.penpot` file already has an `Event Model` page.

Current observed Penpot pages:

- `Screens`
- `Components`
- `Event Model`
- `PATHS`

For the current Event Modeling work, the important Penpot pages are:

- `PATHS`
- `Event Model`

## Current Page Roles

The current working split should be:

- `PATHS`
  each `PATH` should read as its own horizontal row
- `Event Model`
  slice primitives, slice experiments, lens experiments, and visual grammar work
- `Screens`
  app-page surfaces and screenshot-oriented screen references
- `Components`
  component bases, tokens, and reusable visual building blocks

That lets the visual work stay organized instead of forcing one page to do everything.

## Current PATHS Page State

The `PATHS` page is not blank.

It already contains at least:

- large screen-oriented boards such as `Path1.3-EntryForm` and `Initial Screen`
- earlier rough `PATH1` slice boards
- the newer V2 slice instances:
  - `PATH1: CommandSlice - Set Location (V2 Instance)`
  - `PATH1: ViewSlice - Ready (V2 Instance)`
- the first readable row proof:
  - `PATH1.Row.V1`
- the current clean readable row:
  - `PATH1.Row.V2.Clean`

The current clean slice rebuild also produced a reusable component family that now belongs in the practical board seed:

- `SliceRow.Screen.Clean.V1`
- `SliceRow.Command.Clean.V1`
- `SliceRow.Event.Clean.V1`
- `SliceRow.View.Clean.V1`
- `SliceRow.Ref.Clean.V1`
- `CommandSlice.Clean.V1`
- `ViewSlice.Clean.V1`

So the current task is not to invent the page from nothing.

It is to normalize and refine what is already there into a clearer row-per-path projection.

## Current PATH 1 Row Proof

`PATH1.Row.V1` is the first live proof that the `PATHS` page can carry a readable path row rather than only loose screens and scratch boards.

Current shape:

- path title and short descriptive note
- one horizontal row of alternating `CommandSlice` and `ViewSlice` cards
- current screen slots as screenshot-like references
- single-actor reading for now

That proof is still early, but it establishes the right kind of projection surface to keep refining.

## Current Top-Level Structure

The `Event Model` page currently contains three top-level frames:

- `Primatives`
- `Slice1`
- `Slice2`

That means we are not starting from a blank board.

We already have:

- a primitive area
- at least two early slice attempts

## Current Primitive Seed

Inside the `Primatives` frame, the current observed building blocks are:

- `BOX BASE`
- `PROJECTION`
- `EVENT`
- `COMMAND`
- `UI`

This is a useful start because it already hints at the visual grammar we need.

## Current Slice Attempts

The current `Slice1` and `Slice2` frames each appear to contain:

- `Screen1`
- `COMMAND`
- `EVENT`

So the current board seed is already leaning toward:

- screen as visible interaction surface
- command as intent/action
- event as what became true

What is still missing or under-defined in the seed:

- a clearer `View` / `ViewSlice` treatment
- lane rules
- card naming rules
- left-to-right rhythm rules
- projection rules for different lenses

## Current Interpretation

The current Penpot page should be treated as:

- a real seed
- a scratch visual starting point
- a projection surface we can refine

It should not be treated as already-settled doctrine.

## Recommended Next Shape

The next iteration should likely refine the existing pages instead of replacing them.

The first good move is:

1. keep the `Event Model` page as the slice/lens workshop
2. keep the current primitive area as the seed palette
3. rename and normalize the primitives where needed
4. lay out `PATH 1` on the `PATHS` page as one readable horizontal row
5. use screenshots or strong screen references in the screen position of that row
6. add a real `View` primitive in the preferred language if `PROJECTION` is meant to become `View`

That also implies some cleanup pressure on the current `PATHS` page so it does not accumulate rough boards, finished boards, and screen references with no clear reading order.

## PATHS Page Working Rule

The current intended PATH projection is:

- one `PATH` per row on the `PATHS` page
- left-to-right reading for the primary happy-path flow
- nearby notes or descriptions allowed when they improve readability
- screen locations should prefer actual app-page screenshots or strong screen cards over vague placeholders

That makes the `PATHS` page a clearer overview surface than the more experimental `Event Model` page.

## Screen Placement Rule

For the current visual language:

- the screen position in a path row should show the app surface that the actor would actually see
- when possible, that should become a screenshot or screenshot-like projection of the app page
- the screen should help the reader understand the path, but should not overpower the business cards

## Multi-Actor Lane Pressure

The current first LaundryLog path is effectively a single-actor path.

That means one clean row can work well for now.

If later paths introduce multiple actor roles, the board will likely need:

- multiple actor lanes within the path row
- a clearer handoff rule between lanes
- possibly a wider tablet/desktop-first layout instead of a narrow stacked view

So the current row model is a good starting point, but it should not be treated as the final lane model for all paths.

## Naming Pressure

One immediate naming pressure is:

- the page currently uses `PROJECTION`
- the current durable Event Modeling term in this repo is `View`

So we should likely decide whether:

- `PROJECTION` is a broader Penpot/design term
- and `View` is the Event Modeling/business term

or whether:

- the Penpot card should simply be renamed to `VIEW`

That should be decided explicitly rather than allowed to drift.

## Practical Starting Rule

For now, we should start from the existing `Event Model` page and treat it as:

- the first Penpot projection seed for our own Event Modeling surface
- the place where slice components and lenses are refined before they are used on the `PATHS` page

That is better than forcing an external tool to become the source of truth for layout and lenses.
