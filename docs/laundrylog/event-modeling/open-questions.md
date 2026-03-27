# LaundryLog Event Modeling Open Questions

These are the questions we should keep visible while working the first LaundryLog event model.

## Session Boundary

Should "session" remain a derived read boundary:

- inferred from recent entries and inactivity gaps
- or promoted into an explicit stored event later if audit/reporting needs justify it

## Location Boundary

Should the first durable location event be `LaundryLocationCaptured`:

- with one broad shape that covers manual entry, GPS-assisted matching, and later coordinate capture
- or should those become more separate events earlier

Current leaning is that location should be the first durable business event, even if the acquisition variants are deferred behind that shared result.

## Location Acquisition Depth

For the first path, should `LaundryLocationCaptured` carry:

- just the user-facing location text
- text plus optional coordinates when available
- or also a reusable location identity when an existing location is matched

## Entry Identity

Should `LaundryExpenseLogged` carry:

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

## Primary Happy Path Scope

Is the first happy path complete once it proves:

- location captured
- washer expense logged
- dryer expense logged

Current leaning is yes. Supplies, corrections, and alternate location-acquisition branches can expand from there.

## Session End

Do we need `LaundrySessionEnded` at all in the first modeled path, or should current-session boundaries stay derived from inactivity until the first entry flow is solid?
