# LaundryLog Event Modeling Open Questions

These are the questions we should keep visible while working the first LaundryLog event model.

## Session Boundary

Should "session" remain a derived read boundary:

- inferred from recent entries and inactivity gaps
- or promoted into an explicit stored event later if audit/reporting needs justify it

## Location Boundary

Should location be:

- part of `LaundryExpenseEntryAdded`
- its own event: `LaundrySessionLocationSet`
- or draft UI context that only becomes durable when the first entry is logged

Current leaning is to avoid forcing a separate start event too early.

## Entry Identity

Should `LaundryExpenseEntryAdded` carry:

- a stable entry id
- or should entry identity remain implicit until correction/removal paths appear

## Time Semantics

Do we want to distinguish:

- when the expense happened
- when the user recorded it

That may matter for later audit/reporting value.

## Payment Detail Depth

For the first path, should payment method be:

- required
- optional
- or deferred until later

## Session End

Do we need `LaundrySessionEnded` at all in the first modeled path, or should current-session boundaries stay derived from inactivity until the first entry flow is solid?
