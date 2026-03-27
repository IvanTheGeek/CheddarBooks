# LaundryLog PATH 1: First Entry Through Current Session View

This is the first `PATH` we should model.

It is intentionally small and concrete.

## PATH Goal

A user can:

- set or confirm the location context
- record the first expense
- see that expense in the current session view

## Intended Happy Path

1. user opens the app
2. user enters the location
3. user confirms the location
4. user enters the first laundry expense
5. user logs the expense
6. the app shows the current session with that entry and running total
7. user can now continue adding more entries in the same session

## Current Event Hypothesis

The first pass likely wants this durable event line:

1. `LaundryExpenseEntryAdded`

That is enough to prove:

- the journal has at least one durable expense line
- the location can be part of the durable record without forcing a separate session-start event
- the current session view can be derived from recent entries

## First Read/Reaction Expectations

Even before we model reads formally, this path implies a few obvious reactions:

- after `LaundryExpenseEntryAdded`, the app should show at least one logged entry and a running total
- the app should treat that entry as the anchor for the current session window
- later opens after a long enough gap should naturally show a fresh session window instead of the old one

Those are not the main modeling focus yet, but they are useful pressure when checking whether the event line feels complete.

## What To Look For During Modeling

As we work this path in the visual Event Modeling tool, we should watch for:

- missing events
- event names that are actually commands in disguise
- facts that belong on a different event
- whether location should be part of `LaundryExpenseEntryAdded` or remain its own event
- whether "current session" should stay a derived read concern rather than a stored event boundary
- whether the first-entry path needs an explicit session-close event right away
