# CheddarBooks LaundryLog Command/View To Primitive Seam

This note captures the first explicit seam between:

- Event Modeling `CommandSlice` intent
- Event Modeling `ViewSlice` context
- the local LaundryLog primitive compositions used by the UI work

The goal is to stop this boundary from living only in chat or being implied across multiple docs.

## Why This Seam Exists

The current LaundryLog work is now strong enough to say:

- `CommandSlice` tells us what the user is trying to do now
- `ViewSlice` tells us what the current business context looks like now
- the UI primitive state should be composed from both, not from either one alone

That gives us a much cleaner bridge between Event Modeling and actual screen state.

## Current Working Rule

Use these layers in this order:

1. `CommandSlice`
   current user intent and editable draft state
2. `Event`
   the durable facts that become true
3. `ViewSlice`
   the current business-visible context derived from the durable line
4. primitive state
   the UI-ready composition used to render the current screen

This is a semantic layering order, not a mandatory immediate linear chain.

Do not flatten it into:

- `CommandSlice -> Event -> ViewSlice -> primitive state`

as if each layer must be the very next visible slice.

Instead:

- a `CommandSlice` produces durable event fact(s)
- a `ViewSlice` is justified by prior event fact(s)
- the consumed event does not need to come from the immediately previous slice
- multiple `ViewSlice`s may consume the same prior event
- primitive state may be composed from the current command-side draft and a view-side context that depends on earlier durable events

The primitive state should not become the semantic source of truth.

It is a projection layer that exists to make the current screen explicit and testable.

## ViewSlice Clarification

In the preferred Event Modeling language here:

- a `ViewSlice` can be just the green-box `View`
- with the right lens on, a `ViewSlice` can also include the resultant business-visible screen that the `View` supports

So the `View` remains the dataset or structure.

The `ViewSlice` can be read either:

- narrowly, as the `View` itself
- or more broadly, as the `View` plus the resultant business-visible screen state

That broader reading is useful here because LaundryLog is actively connecting Event Modeling work to Penpot screens and later FnUI runtime screens.

The important anti-pattern to avoid is describing the broader seam as one fixed triplet such as:

- `LaunchApp -> AppStarted -> SplashVisible`

That wording hides the real rule:

- the command slice produces the event
- later view slices consume prior event(s)
- the view dependency belongs in the view explanation or `VIEW GWT`, not as a forced immediate third step

## Current First Seam

### `CaptureLaundryLocation` -> `NewSessionPrimitiveState`

For the first location-capture screen, the primitive state is mostly driven by the current `CommandSlice`.

Current command-side pressure:

- draft location text
- whether GPS is currently available
- whether `Set Location` should be enabled

That means the current `NewSession` primitive state is mainly:

- header
- location input
- GPS action
- set-location action

### `LogLaundryExpense` + `CurrentLaundrySession` -> `EntryFormPrimitiveState`

For the entry form, the primitive state needs both:

- the command-side draft
- the view-side business context

Current command-side pressure:

- selected expense kind
- quantity
- draft unit price
- selected payment method
- quick-fill values
- whether submit is currently allowed

Current view-side pressure:

- active location
- running session total
- visible recent entries

That means the current `EntryForm` primitive state is composed from both sides.

Those visible recent entries now project into local `EntryCardState` values.

## What Lives Where Right Now

### Command-side state

Current local code-facing shapes:

- `CaptureLaundryLocationCommandSliceState`
- `LogLaundryExpenseCommandSliceState`

### View-side state

Current local code-facing shapes:

- `VisibleLaundryExpenseViewLine`
- `CurrentLaundrySessionViewState`

### Primitive projection

Current local code-facing mapping module:

- `PrimitiveStateMappings`

This is still local to `CheddarBooks`.

It is not yet the final shared `FnTools` abstraction boundary.

## What Is Not Covered Yet

The current seam does not yet model everything visible in the eventual app:

- richer validation detail
- time display surfaces
- GPS acquisition details
- location matching and reuse
- corrections or deletions

That is intentional.

The current goal is to make the smallest credible bridge explicit.

## Current Code Surface

The first code-facing seam now lives in:

- `CheddarBooks.LaundryLog`
- `CheddarBooks.LaundryLog.UI`

The current mapping starts from:

- `ExpenseKind`
- `PaymentMethod`
- `LocationName`
- `CaptureLaundryLocationCommandSliceState`
- `LogLaundryExpenseCommandSliceState`
- `CurrentLaundrySessionViewState`

And projects into:

- `NewSessionPrimitiveState`
- `EntryFormPrimitiveState`

## Why This Helps

This seam gives us:

- a reviewable bridge from Event Modeling to screen state
- something testable that does not depend on chat memory
- a way to compare the model against Penpot and the running UI later
- a smaller and safer path toward future shared `FnHCI` primitives

The concrete washer and dryer examples in the Event Modeling docs should now be treated as the semantic reference points for:

- `VisibleLaundryExpenseViewLine`
- `EntryCardState`

## Related

- [Event Modeling](event-modeling/README.md)
- [Views, ViewSlices, And Business Screenshots](event-modeling/views.md)
- [Candidate Commands And CommandSlices](event-modeling/commands.md)
- [FnHCI Primitive Map](fnhci-primitive-map.md)
- [FnHCI Primitive State Shapes](fnhci-primitive-state-shapes.md)
- [Penpot Screen Evidence](penpot-screen-evidence.md)
