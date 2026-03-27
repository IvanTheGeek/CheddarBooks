# LaundryLog Candidate Events

This note captures the current first-pass candidate events for LaundryLog.

These are not final.

They are the working list we should pressure during the first Event Modeling pass.

## Current Session Meaning

For now, "session" is better treated as a UX/read-model concept than as a required first-class domain event.

Current understanding:

- the app shows a current session window over recent laundry entries
- that window exists to make the active laundry period easy to follow on screen
- if enough time passes since the last entry, the next open should naturally feel like a new session
- that grouping can be derived without forcing a `LaundrySessionStarted` event

## First-Pass Durable Events

### 1. `LaundryLocationCaptured`

Meaning:

- the laundry activity now has a durably known location

Likely carried facts:

- user-facing location text
- capture method
- optional coordinates when available
- possible matched-location identity when reuse is available
- recorded time

Why it likely matters:

- location is the first meaningful business context for the journal
- one location can anchor multiple washer and dryer entries
- later reuse, matching, and sharing all depend on having a durable location record

Current first-pass properties:

- `location_name`
  the user-facing name that will be shown in the log
- `capture_method`
  how the location was captured
- `recorded_at_utc`
  when the location record was captured in the app, stored in UTC

Current optional properties:

- `coordinates`
  only when GPS or map-assisted capture is actually used
- `matched_location_id`
  only when the app matched an existing reusable location record
- `notes`
  only if we later decide freeform location notes are worth carrying

First concrete example: manual typed location

```toml
event_name = "LaundryLocationCaptured"
location_name = "Love's #123 - Springfield, OH"
capture_method = "manual-text"
recorded_at_utc = "2026-03-27T13:42:00Z"
```

What this example pressures:

- `location_name` is probably required in the first path
- `capture_method` should likely be explicit rather than inferred
- durable event time should be stored in UTC
- the last captured location can reasonably remain the active location until the user changes it
- coordinates do not need to be forced into the manual-entry path
- a reusable matched location can remain deferred until GPS or lookup flows are modeled
- later GPS help should probably suggest or refine location rather than silently replacing the active location

### 2. `LaundryExpenseLogged`

Meaning:

- one laundry expense was logged into the journal

Likely carried facts:

- expense kind
- quantity
- unit price
- total
- payment method
- location context
- `occurred_at_utc`
- `recorded_at_utc`

Why it likely matters:

- this is the core audit-defense journal event
- current-session totals can derive from repeated logged expenses
- washer and dryer expenses can both be represented by repeated instances of this event
- views can still present local time without losing a stable UTC basis in storage

## Strong Candidates For Soon-After

These are plausible but can wait until the first path is stable:

### `LaundryExpenseCorrected`

Useful when:

- we want correction without pretending the original entry never happened

### `LaundryExpenseRemoved`

Useful when:

- we need an explicit reversal/removal path

### `LaundryLocationRefined`

Useful when:

- we later want a durable way to improve an initially rough location record without pretending the earlier capture never happened

## Derived Or Deferred Session Concepts

These may still matter, but they currently look more like derived session-window behavior or later workflow boundaries than first-pass durable events:

### `LaundrySessionStarted`

Useful when:

- we later decide an explicit start boundary has audit value beyond the first recorded entry

### `LaundrySessionLocationSet`

Useful when:

- we later decide location belongs to a session-window concept instead of a more direct location-capture event

### `LaundrySessionEnded`

Useful when:

- we want a formal close-out boundary instead of deriving session end from inactivity or later reads

## Deferred For Later Modeling

Do not force these into the first path yet:

- `PaymentStatementLineMatched`
- `ReceiptEvidenceAttached`
- `LocationSuggestedFromDevice`
- `ExistingLocationMatchedFromCoordinates`
- `LaundrySessionMergedAcrossDevices`
- `TaxReportEntryPrepared`

Those may all matter later, but they are not needed to get the first useful event line in place.
