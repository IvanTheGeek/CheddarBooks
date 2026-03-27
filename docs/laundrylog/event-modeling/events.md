# LaundryLog Candidate Events

This note captures the current first-pass candidate events for LaundryLog.

These are not final.

They are the working list we should pressure during the first Event Modeling pass.

## First-Pass Events

### 1. `LaundrySessionStarted`

Meaning:

- a new laundry session/journal session has begun

Why it likely matters:

- gives the session its own durable identity
- separates one laundromat visit from another
- supports later grouping, totals, and audit trails

### 2. `LaundrySessionLocationSet`

Meaning:

- the session has been given a human-meaningful location

Why it likely matters:

- location is part of the journal value
- the current app flow is explicitly location-first
- it is a likely audit/usefulness boundary for later reports

### 3. `LaundryExpenseEntryAdded`

Meaning:

- one expense line was logged against the current session

Likely carried facts:

- expense kind
- quantity
- unit price
- total
- payment method
- occurred/recorded time

Why it likely matters:

- this is the core audit-defense journal event
- session totals can derive from repeated additions
- batch entry naturally becomes repeated instances of this event

## Strong Candidates For Soon-After

These are plausible but can wait until the first path is stable:

### `LaundryExpenseEntryCorrected`

Useful when:

- we want correction without pretending the original entry never happened

### `LaundryExpenseEntryRemoved`

Useful when:

- we need an explicit reversal/removal path

### `LaundrySessionEnded`

Useful when:

- we want a formal close-out boundary for a session

## Deferred For Later Modeling

Do not force these into the first path yet:

- `PaymentStatementLineMatched`
- `ReceiptEvidenceAttached`
- `LocationSuggestedFromDevice`
- `LaundrySessionMergedAcrossDevices`
- `TaxReportEntryPrepared`

Those may all matter later, but they are not needed to get the first useful event line in place.
