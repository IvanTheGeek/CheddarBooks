# CheddarBooks LaundryLog FnHCI Primitive Map

This note captures the first practical bridge from the current LaundryLog screens and Penpot components into renderer-neutral `FnHCI` primitives.

The goal is not to freeze the entire UI system.

The goal is to identify the smallest reusable interaction vocabulary that the current LaundryLog work is already pressuring.

## Why This Note Exists

LaundryLog is now far enough along that it is no longer just:

- a few screenshots
- a concept page
- a list of screens

It is starting to pressure a real cross-platform UI vocabulary.

That means we should begin naming:

- what is app-specific
- what is reusable interaction structure
- what should likely become `FnHCI` primitives later

## Current Boundary

The current working split should be:

- LaundryLog owns the app meaning
- `FnHCI` should own reusable interaction primitives
- `FnUI` should project those primitives into a runtime such as web or later native hosts

So LaundryLog should not try to turn every screen element into its own forever-app-specific widget if the interaction role is clearly reusable.

## Current Screen Pressure

The current visible screens are:

- [`Screen.NewSession`](screens.md)
- [`Screen.EntryForm`](screens.md)

The current command and path pressure is:

- capture location
- log one laundry expense
- repeat that logging flow quickly within the same current session window

That means the most important primitives are the ones that support:

- one clear command
- obvious current context
- repeated option selection
- fast numeric adjustment and entry
- a visible running total

## First Primitive Candidates

The current LaundryLog work suggests this first reusable primitive set:

- `HeaderBar`
- `TextInput`
- `ActionButton`
- `OptionButton`
- `OptionGroup`
- `Stepper`
- `MoneyInput`
- `SummaryBar`
- `EntryCard`

These are not final library names yet.

They are the current best durable labels for what the screens appear to need.

## First Mapping

### `Header` -> `HeaderBar`

Current role:

- identify the current screen
- reinforce brand/app identity
- carry light supporting context

This looks like a reusable top-level primitive, not a LaundryLog-only one.

### `Input.Location` -> `TextInput`

Current role:

- capture free text
- show current draft location value
- later allow location refinement

The domain meaning is LaundryLog-specific.

The input interaction is not.

### `Button.GPS` -> `ActionButton`

Current role:

- trigger a supporting location action
- not the main path for the first happy path

This suggests:

- same primitive family as other action buttons
- different emphasis than the main submit action

### `Button.SetLocation` -> `ActionButton`

Current role:

- commit the location capture command

This is clearly a command-driving action button.

It should likely be a reusable action primitive with a semantic emphasis level, not a LaundryLog-only widget type.

### `Button.Machine.Washer`, `Button.Machine.Dryer`, `Button.Machine.Supplies` -> `OptionButton` inside `OptionGroup`

Current role:

- select one domain option from a small set

This is a strong candidate for a reusable choice primitive:

- an `OptionGroup` owns the selection state
- an `OptionButton` renders each choice

LaundryLog owns the option meanings:

- washer
- dryer
- supplies

### `Button.Payment.*` -> `OptionButton` inside `OptionGroup`

Current role:

- select one payment method

This appears to use the same interaction pattern as machine-type choice, even though the domain category is different.

That is a strong sign that the reusable primitive should sit above the LaundryLog domain.

### `Button.Counter` and quantity controls -> `Stepper`

Current role:

- increment or decrement a value quickly
- keep the current value visible

The current `+/-` quantity controls suggest a reusable `Stepper` primitive rather than two unrelated buttons plus a separate display.

### `Input.Price` and price-adjustment helpers -> `MoneyInput`

Current role:

- enter or adjust one money amount
- support direct input plus quick adjustments

The current price area is more specific than a plain text input, but broader than LaundryLog alone.

So the likely split is:

- `MoneyInput` as a reusable primitive
- LaundryLog-specific quick-fill choices layered around it

### `Display.EntryTotal` -> `SummaryBar`

Current role:

- keep a running total visible
- reinforce the current session context

This feels like a reusable summary/status primitive rather than a domain-specific one.

LaundryLog owns what the number means.

The summary-bar interaction and visual role should be reusable.

### `Card.Entry` -> `EntryCard`

Current role:

- show one recent logged expense in a compact readable form

This is likely reusable at the pattern level, though not necessarily as a universal primitive at the same tier as button or input.

For now it is best treated as:

- a reusable view pattern candidate
- still closer to the app boundary than the smallest primitives

## What Should Stay App-Specific

These should stay in LaundryLog rather than moving into generic `FnHCI` abstraction:

- what a laundry location means
- what a washer/dryer/supplies choice means
- how the current session window is derived
- how payment meaning is interpreted
- how entry totals and recent entries are described to the user

`FnHCI` should not own those meanings.

It should own the interaction structure they happen to use.

## Relationship To Current View Work

The current [`View Contracts`](view-contracts.md) should be read together with this note.

The view contracts name:

- the app-level view shapes
- the shell-level regions
- the current purpose of each screen

This note sits one level lower and asks:

- what reusable primitive pressure is visible inside those views

## First Working Rule

If two different LaundryLog sections use the same interaction pattern with different domain meaning, prefer one reusable primitive with app-owned semantics over two unrelated custom widgets.

That is the clearest signal we currently have for what should later live in `FnHCI`.

## Next Likely Steps

1. keep refining this map against the real Penpot component structure
2. use Penpot-backed example states to pressure the local app model
3. connect the mapped primitives to the current token vocabulary
4. later move the truly reusable parts into the `FnTools` line
5. keep testing the map against the actual Event Modeling paths instead of only against the pictures

## Related

- [Concept UI Page](concept-ui-page.md)
- [Penpot Screen Evidence](penpot-screen-evidence.md)
- [FnHCI Primitive State Shapes](fnhci-primitive-state-shapes.md)
- [Screens](screens.md)
- [View Contracts](view-contracts.md)
- [Event Modeling](event-modeling/README.md)
