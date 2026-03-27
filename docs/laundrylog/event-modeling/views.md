# LaundryLog Views, ViewSlices, And Business Screenshots

This note captures the first View shape for LaundryLog in Event Modeling terms.

In the business lens, this is the closest thing to the "screenshot" idea.

If someone comes in using Event Modeling search terms, this is roughly the place they may expect "read model" language, but the preferred LaundryLog term is `View`.

In the preferred Event Modeling language here, this is the `ViewSlice` side.

This note is about:

- what the user can see
- what the business state appears to be
- what decision the user can make next

## First ViewSlice

### `CurrentLaundrySession`

Purpose:

- show the active location
- show the currently visible laundry expense lines
- show the running total
- make the next likely action obvious

This is the View dataset/structure that should sit behind the current-session view in the app.

## Current First-Pass View Fields

- `active_location_name`
- `active_location_capture_method`
- `visible_entries`
- `running_total`
- `last_recorded_at_local`

Notes:

- durable event time stays in UTC
- the View localizes time for display
- the View is derived from the durable event line, not stored as the source of truth

## PATH 1 Business Screenshots

### Screenshot 1: After `LaundryLocationCaptured`

Business meaning:

- the location is known
- no expenses have been logged yet
- the user is ready to log the first washer expense

Expected visible state:

- active location: `Love's #123 - Springfield, OH`
- visible entries: none yet
- running total: `0.00`
- next obvious action: log washer expense

### Screenshot 2: After First `LaundryExpenseLogged`

Business meaning:

- the first washer expense is now in the journal
- the user is still working at the same location
- the current session total has started

Expected visible state:

- active location: `Love's #123 - Springfield, OH`
- visible entries:
  - washer entry
- running total: washer line total
- next obvious action: log dryer expense or another washer expense

### Screenshot 3: After Second `LaundryExpenseLogged`

Business meaning:

- washer and dryer activity are both now visible in the same current session
- the running total reflects both entries
- the user can continue the same session or stop and come back later

Expected visible state:

- active location: `Love's #123 - Springfield, OH`
- visible entries:
  - washer entry
  - dryer entry
- running total: washer + dryer
- next obvious action: log another expense or leave the session as-is

## Mapping To Existing UI Notes

Current likely UI mapping is:

- `Screen.NewSession`
  for the location-capture state before the first expense
- `Screen.EntryForm`
  for the ongoing current-session view after location is known

See also:

- [`../screens.md`](../screens.md)
- [`../view-contracts.md`](../view-contracts.md)
