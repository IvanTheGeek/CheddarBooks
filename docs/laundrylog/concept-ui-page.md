# CheddarBooks LaundryLog Concept UI Page

This note captures the more extensive concept-UI work that led to the first LaundryLog screens.

It exists so the current screen shapes are not just a few isolated screenshots or scattered chat memories. The goal is to keep the real design intent durable and reviewable.

## Primary Source Threads

The strongest current source threads are:

- [`019d174e-e9e9-7732-8fb0-053fb558797f.toml`](../../../NEXUS-EMERGING/NEXUS-EventStore/projections/conversations/019d174e-e9e9-7732-8fb0-053fb558797f.toml)
  `Mobile app interface brainstorming`
- [`019d174e-ea7b-71ca-91ea-5f3ad56b32fd.toml`](../../../NEXUS-EMERGING/NEXUS-EventStore/projections/conversations/019d174e-ea7b-71ca-91ea-5f3ad56b32fd.toml)
  `LaundryLog`
- [`019d174e-ea9a-7776-bbf0-fa54eb01b344.toml`](../../../NEXUS-EMERGING/NEXUS-EventStore/projections/conversations/019d174e-ea9a-7776-bbf0-fa54eb01b344.toml)
  `LaundryLog walking skeleton mockup`

## What The Concept Page Is Trying To Do

The core concept is not a large dashboard or an accounting report page.

It is a small mobile-first command-oriented surface for the real moment when someone is standing in a truck stop or laundromat and needs to log laundry costs quickly while the facts are still fresh.

The page is meant to support:

- one current location context
- repeated entries over the next few hours
- thumb-friendly use on a phone
- fast logging without fighting the interface

That means the concept page is more like a focused working surface than a broad application home page.

## Core UI Idea

The current concept is essentially:

1. establish location
2. log washer or dryer or supplies expense
3. keep using the same screen shape while the user is in laundry mode
4. keep a visible running session total

The "session" here is user-facing convenience, not necessarily the first durable business event.

## Main Layout Direction

The main concept page converged toward a single-column mobile layout with:

- a simple top header with LaundryLog identity
- location shown first and prominently
- the expense-entry controls directly below
- the running session total near the action area rather than hidden away
- the recent entries list below the primary command area

This keeps the current action above the fold while still showing enough context.

## Location First

One of the clearest design conclusions was:

- location is the first important context
- everything else on the page depends on it

That led to the current `Set Location` first-screen idea and the later rule that the current location should remain visible and active until the user changes it.

Design implications:

- location stays at the top
- GPS is helpful but not required for the first path
- manual text entry must work well
- later matching or community location intelligence can improve the experience without blocking the basic flow

## Entry Form Shape

The first entry form concept converged toward these sections:

- location context display
- machine type selection
- quantity control
- unit price control
- payment selection
- log action
- session total bar

This was not meant as abstract CRUD form design. Each part is there because it supports the real-world laundry logging task.

## Important UX Decisions From The Iteration Thread

### Touch Targets

The controls should be large and thumb-friendly.

That includes:

- large machine-type buttons
- large `+/-` controls
- enough spacing to avoid accidental taps

### No Custom Numeric Modal

The concept moved away from custom in-app number pads.

The preferred direction is:

- tap the quantity value to use the device keypad
- tap the unit price value to use the device keypad

This keeps the app simpler and makes it feel more native to the device.

### Machine Type Should Be Obvious Without Heavy Headings

The thread converged on removing extra section headings where the buttons already carry the meaning clearly.

So the machine type section should rely on the obviousness of:

- washer
- dryer
- supplies

rather than burning vertical space on a big heading.

### Quantity And Unit Price Labels Should Be Integrated

The later design direction pushed toward:

- direct labels near the control they describe
- less stacked heading text
- tighter, cleaner layout

### Payment Is Likely Sticky Within A Session

The likely interaction rule is:

- once a payment type is selected, it stays selected for later entries in the same laundry session window
- but it remains easy to change if needed

### Session Total Should Stay Prominent

The design thread clearly preferred a visible session total bar rather than burying totals in a header card.

The current durable direction is:

- session total remains easy to scan
- it stays near the command area
- it should feel like active context, not like a separate report

## Visual Language

The concept design also converged on a few visual preferences:

- lighter orange rather than a deep aggressive orange
- neutral slate-style supporting colors
- compact but not cramped spacing
- clear status without too much ornament

There was also early interest in:

- a future CheddarBooks badge in the header
- a cat mascot or friendly brand touch later

Those are useful brand notes, but they were secondary to getting the workflow right.

## Validation And Command Emphasis

The walking-skeleton thread added an important insight:

- the command slice is the primary focus
- the view exists to support the command clearly

That led to the idea that the main action button can visibly show what is still missing or invalid, rather than forcing the user to hunt for errors elsewhere.

The current command emphasis fits the broader Event Modeling direction:

- the screen should show what the command needs
- the command should not be drowned in decorative UI

## Penpot And Component Pressure

The later Penpot thread translated this concept page into reusable design-system pressure.

The important direction there was:

- each meaningful full-screen state can be a board
- the board should be built from reusable components
- component naming and layering should respect Penpot conventions

Emerging component examples included:

- `Button.Option`
- `Button.Counter`
- `Button.SetLocation`
- `Screen.NewSession`
- `Screen.EntryForm`

So the concept page is not only a picture. It is also the source pressure for the reusable component system.

## Current Concrete Visual References

The current repo screenshots that best carry this concept forward are:

- [S_SetLocation.png](ScreenShots/S_SetLocation.png)
- [S_LogLaundryExpense.png](ScreenShots/S_LogLaundryExpense.png)

These do not replace the richer design rationale in the source conversations, but they are the best current compact visual references in the repo.

## What Should Stay Stable

Even if the styling evolves, the core concept should stay stable:

- location-first flow
- command-centered screen
- repeated quick entries in one place
- visible running session total
- thumb-friendly controls
- device-native numeric entry
- simple, practical, non-bureaucratic feel

## Related

- [Introduction](introduction.md)
- [Requirements](requirements.md)
- [Screens](screens.md)
- [View Contracts](view-contracts.md)
- [Event Modeling](event-modeling/README.md)
