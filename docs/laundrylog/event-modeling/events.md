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

### 1. `LaundryExpenseEntryAdded`

Meaning:

- one expense line was logged into the journal

Likely carried facts:

- expense kind
- quantity
- unit price
- total
- payment method
- location context
- occurred/recorded time

Why it likely matters:

- this is the core audit-defense journal event
- current-session totals can derive from repeated additions
- batch entry naturally becomes repeated instances of this event

## Strong Candidates For Soon-After

These are plausible but can wait until the first path is stable:

### `LaundryExpenseEntryCorrected`

Useful when:

- we want correction without pretending the original entry never happened

### `LaundryExpenseEntryRemoved`

Useful when:

- we need an explicit reversal/removal path

## Derived Or Deferred Session Concepts

These may still matter, but they currently look more like derived session-window behavior or later workflow boundaries than first-pass durable events:

### `LaundrySessionStarted`

Useful when:

- we later decide an explicit start boundary has audit value beyond the first recorded entry

### `LaundrySessionLocationSet`

Useful when:

- location needs its own durable change history rather than simply being carried on an entry

### `LaundrySessionEnded`

Useful when:

- we want a formal close-out boundary instead of deriving session end from inactivity or later reads

## Deferred For Later Modeling

Do not force these into the first path yet:

- `PaymentStatementLineMatched`
- `ReceiptEvidenceAttached`
- `LocationSuggestedFromDevice`
- `LaundrySessionMergedAcrossDevices`
- `TaxReportEntryPrepared`

Those may all matter later, but they are not needed to get the first useful event line in place.
