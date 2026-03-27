# LaundryLog PATH 1: Location Through Washer And Dryer Entries

This is the first `PATH` we should model.

It is intentionally small and concrete.

## PATH Goal

A user can:

- capture and define the location
- log a washer expense
- log a dryer expense
- see both entries in the current session view

## Intended Happy Path

1. user opens the app
2. user enters or confirms the location
   example: `Love's #123 - Springfield, OH`
3. the app captures that location as the active laundry context
4. user enters a washer expense
5. user logs the washer expense
6. user enters a dryer expense
7. user logs the dryer expense
8. the app shows both entries and the running total in the current session view

## Current Event Hypothesis

The first pass likely wants this durable event line:

1. `LaundryLocationCaptured`
2. `LaundryExpenseLogged` for washer
3. `LaundryExpenseLogged` for dryer

## Current Command Hypothesis

The first pass likely wants this command line:

1. `CaptureLaundryLocation`
2. `LogLaundryExpense` for washer
3. `LogLaundryExpense` for dryer

That gives us:

- one clear command that asks the app to establish the active location
- one reusable expense-logging command for repeated washer and dryer entries
- a cleaner separation between user intent and the durable facts that result

That is enough to prove:

- the journal begins with durable location context
- the journal has at least one durable expense line
- repeated logged expenses can accumulate under one captured location
- the current session view can be derived from recent entries

Current concrete starting example:

- `LaundryLocationCaptured`
  `location_name = "Love's #123 - Springfield, OH"`
  `capture_method = "manual-text"`

## First Read/Reaction Expectations

Even before we model reads formally, this path implies a few obvious reactions:

- after `LaundryLocationCaptured`, the app should show active location context
- that location should remain active for later entries until the user changes it
- after the washer entry, the app should show one logged entry and a running total
- after the dryer entry, the app should show both entries and the updated total
- the app should treat that grouped recent activity as the current session window
- later opens after a long enough gap should naturally show a fresh session window instead of the old one

Those are not the main modeling focus yet, but they are useful pressure when checking whether the event line feels complete.

## What To Look For During Modeling

As we work this path in the visual Event Modeling tool, we should watch for:

- missing events
- event names that are actually commands in disguise
- facts that belong on a different event
- whether `LaundryLocationCaptured` is the right first durable boundary
- whether location matching/GPS details belong on that event or in later refinement events
- whether future GPS help should suggest a location change instead of automatically replacing the active location
- whether "current session" should stay a derived read concern rather than a stored event boundary
- whether the washer-plus-dryer happy path is enough before expanding to supplies and corrections
- whether the first-entry path needs an explicit session-close event right away
