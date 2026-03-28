# CheddarBooks LaundryLog FnHCI Primitive State Shapes

This note captures the first explicit state-shape direction for the current LaundryLog primitive pressure.

It sits between:

- the primitive map
- the current app view contracts
- the future shared `FnTools.FnHCI` primitive library

The purpose is to make the current pressure code-facing without pretending the shared primitive library is already complete.

## Why This Note Exists

The current LaundryLog work is now strong enough to say more than:

- these screens exist
- these primitives seem likely

We can now say:

- what state each primitive likely needs
- what should stay app-specific
- what should later migrate into shared `FnHCI` shapes

## Current Working Rule

These shapes are currently local pressure shapes in `CheddarBooks`.

They are not yet the final shared `FnTools` surface.

That means:

- keep them small
- keep them explicit
- keep them close to the current screens and paths
- avoid inventing a giant generic UI framework too early

## First Primitive State Shapes

### `HeaderBarState`

Purpose:

- title the current screen
- optionally show secondary context
- optionally show a small badge or accent

Current likely fields:

- `title`
- `subtitle`
- `badge_text`

### `TextInputState`

Purpose:

- hold one current text entry surface
- support placeholder text
- support read-only or editable states

Current likely fields:

- `control_id`
- `placeholder_text`
- `value_text`
- `is_read_only`

### `ActionButtonState`

Purpose:

- represent one command-driving or support action

Current likely fields:

- `control_id`
- `label`
- `is_enabled`
- `emphasis`

Current likely emphasis values:

- `primary`
- `secondary`
- `supporting`

Examples in LaundryLog:

- `Set Location`
- `Use GPS`
- `Log Expense`

### `OptionChoiceState`

Purpose:

- represent one selectable option inside an option group

Current likely fields:

- `choice_id`
- `label`
- `is_selected`
- `is_enabled`

Examples in LaundryLog:

- washer
- dryer
- supplies
- cash
- card
- app
- points

### `OptionGroupState`

Purpose:

- represent one mutually meaningful group of choices

Current likely fields:

- `control_id`
- `label`
- `choices`

The current working assumption is that one option group must contain at least one choice and should not contain duplicate choice ids.

### `StepperState`

Purpose:

- represent a quick increment/decrement control with one visible value

Current likely fields:

- `control_id`
- `decrement_label`
- `increment_label`
- `value_text`
- `can_decrement`
- `can_increment`

Examples in LaundryLog:

- quantity `- 1 +`

### `MoneyInputState`

Purpose:

- represent a money-oriented entry field with optional quick-fill helpers

Current likely fields:

- `control_id`
- `currency_symbol`
- `value_text`
- `placeholder_text`
- `quick_fill_labels`

Examples in LaundryLog:

- price input
- quick-fill buttons such as `$2.50` or `$3.00`

### `SummaryBarState`

Purpose:

- show one important current summary value close to the command area

Current likely fields:

- `control_id`
- `label`
- `value_text`

Example in LaundryLog:

- session total

## First App-Level Compositions

The current screens imply two first app-level primitive compositions.

### `NewSessionPrimitiveState`

Current likely composition:

- `header`
- `location_input`
- `gps_action`
- `set_location_action`

### `EntryFormPrimitiveState`

Current likely composition:

- `header`
- `machine_type_options`
- `quantity_stepper`
- `price_input`
- `payment_options`
- `session_total`
- `submit_action`

## What Stays App-Specific

The following meanings still belong to LaundryLog, not to the primitive library:

- what counts as a valid laundry location
- which machine types exist
- which payment methods are allowed
- how the current session window is derived
- how totals and recent entries are described in domain terms

The primitive shapes should stay reusable.

The domain meanings should stay local.

## Small Example

```toml
screen = "new-session"

[header]
title = "LaundryLog"
subtitle = "Capture location before first expense"

[location_input]
control_id = "location-input"
placeholder_text = "Enter location..."
value_text = "Love's #123 - Springfield, OH"
is_read_only = false

[gps_action]
control_id = "use-gps"
label = "Use GPS"
is_enabled = true
emphasis = "supporting"

[set_location_action]
control_id = "set-location"
label = "Set Location"
is_enabled = true
emphasis = "primary"
```

## Why These Shapes Matter

These shapes give us:

- a code-facing bridge from the current Penpot and screen work
- something reviewable that does not depend on one AI remembering prior chats
- pressure toward a later shared `FnTools` primitive library

## Next Likely Steps

1. encode the first local pressure types in code
2. map the current screens into those local state shapes
3. compare them against the Penpot components and token vocabulary
4. later move the truly reusable parts into `FnTools`

## Related

- [FnHCI Primitive Map](fnhci-primitive-map.md)
- [View Contracts](view-contracts.md)
- [Screens](screens.md)
- [Event Modeling](event-modeling/README.md)
