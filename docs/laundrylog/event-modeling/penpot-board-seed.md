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

For the Event Modeling work, the important page is:

- `Event Model`

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

The next iteration should likely refine this existing page instead of replacing it.

The first good move is:

1. keep the `Event Model` page
2. keep the current primitive area as the seed palette
3. rename and normalize the primitives where needed
4. replace `Slice1` and `Slice2` with explicit slice frames for PATH 1
5. add a real `View` primitive in the preferred language if `PROJECTION` is meant to become `View`

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

That is better than forcing an external tool to become the source of truth for layout and lenses.
