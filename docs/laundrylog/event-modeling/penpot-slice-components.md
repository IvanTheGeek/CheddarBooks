# LaundryLog Penpot Slice Components

This note records the current clean slice-component direction inside the live `LaundryLog` Penpot file.

It exists so the component names, token seams, and page-placement rules do not stay trapped in the design file or in chat.

## Purpose

The current Penpot slice work is trying to stay as high on the component ladder as possible.

Current intended ladder:

- row component
- shell component
- path-row instance

That keeps the Event Modeling projection closer to future `FnHCI` / `FnUI` derivatives instead of dropping immediately into one-off cards.

## Current Clean Component Family

Current row components:

- `SliceRow.Screen.Clean.V1`
- `SliceRow.Command.Clean.V1`
- `SliceRow.Event.Clean.V1`
- `SliceRow.View.Clean.V1`

Current shell components:

- `CommandSlice.Clean.V1`
- `ViewSlice.Clean.V1`

Current first clean path-row projection:

- `PATH1.Row.V2.Clean`

## Current Visual Direction

The clean slice family is intentionally based on the calmer EM-1 slice language recovered from:

- `f63e1af`
- hosted `Slice.html`

That means the current visual grammar is aiming for:

- calm narrow cards
- strong outer border
- tinted header band
- small path pill
- uppercase slice-type marker
- short title and subtitle
- semantic rows that can be reused below the shell

## Current Row Roles

The current row family is carrying these roles:

- `Screen`
  visible app surface or screenshot-like screen reference
- `Command`
  actor intent in business/application language
- `Event`
  what became true
- `View`
  current business-visible state

That means the current shell composition is:

- `CommandSlice`
  `Screen -> Command -> Event`
- `ViewSlice`
  classic Event Modeling default: `View`

If a different lens wants to show a screen or actor surface above the `View`, that should be treated as a lens-specific projection rather than the default `ViewSlice` shape.

## Current Token Direction

Current local token sets in the live file:

- `EventModel.Foundation.V1`
- `EventModel.Viewport.Tablet.V1`
- `EventModel.Viewport.Desktop.V1`
- `EventModel.Viewport.Wide.V1`

Current token intent:

- foundation colors and borders
- viewport width and spacing pressure
- future breakpoint adaptation for tablet, desktop, and wide desktop reading

Current practical rule:

- align the clean slice family to the token vocabulary now
- bind tokens selectively and explicitly as that workflow becomes trustworthy
- do not pretend token binding is solved everywhere yet

## Current Page Placement Rule

Current intended page split remains:

- `PATHS`
  readable path rows
- `Event Model`
  slice experiments and grammar work
- `Screens`
  app-page surfaces
- `Components`
  component catalog and reusable visual bases

Current live Penpot quirk:

- top-level creation and top-level reparenting follow the active page

So the practical rule is:

- switch to the target page first
- then create or clean up top-level boards
- then verify the result

## Current Catalog Rule

Because of the active-page targeting behavior, treat the `Components` page as the visible catalog page for the clean slice family even if the underlying component main-instance lineage needs extra care.

That means:

- keep catalog instances on `Components`
- keep readable path instances on `PATHS`
- keep slice experiments on `Event Model`

## Current Screen Rule

The current clean row uses strong screen references, not yet full screenshot embedding.

That is acceptable for now.

The next screen-oriented upgrade should prefer:

- actual screenshot thumbnails
- or a screenshot-like projection from the real screen board

without changing the underlying slice meaning.

## Current Event Ref Rule

`Event Ref` is not part of the current default LaundryLog `ViewSlice` language.

Why:

- the `ViewSlice` should not show the event in classic Event Modeling reading
- the event belongs in the preceding `CommandSlice`
- if we need a later supporting reference row for some other lens, that should be introduced deliberately instead of being baked into the default `ViewSlice`

## Current Naming Rule

Current naming should stay explicit and layered:

- row components: `SliceRow.<Role>.Clean.V1`
- shell components: `<SliceType>.Clean.V1`
- path-row board: `PATH1.Row.V2.Clean`
- path-row instances: `PATH1.<kind>.<step>`

This keeps the Penpot file inspectable for both humans and future automation.

## Read Next

- [Penpot Projection](penpot-projection.md)
- [Penpot Board Seed](penpot-board-seed.md)
- [Inspiration](inspiration.md)
- [PATH 1](path-1-first-entry.md)
