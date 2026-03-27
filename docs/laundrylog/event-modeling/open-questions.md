# LaundryLog Event Modeling Open Questions

These are the questions we should keep visible while working the first LaundryLog event model.

## Session Boundary

Should `LaundrySessionStarted` happen:

- before location is known
- or only once location is confirmed

## Location Boundary

Should location be:

- its own event: `LaundrySessionLocationSet`
- or part of `LaundrySessionStarted`

Current leaning is to keep it separate because the current path is explicitly location-first and the distinction may matter later.

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

Do we need `LaundrySessionEnded` in the first modeled path, or can that stay deferred until the first entry flow is solid?
