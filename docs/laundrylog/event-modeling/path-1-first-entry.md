# LaundryLog PATH 1: Start Session Through First Entry

This is the first `PATH` we should model.

It is intentionally small and concrete.

## PATH Goal

A user can:

- begin a laundry outing
- set the location
- record the first expense

## Intended Happy Path

1. user opens the app
2. user begins a new laundry session
3. user enters the location
4. user confirms the location
5. user enters the first laundry expense
6. user logs the expense
7. user can now continue adding more entries in the same session

## Current Event Hypothesis

The first pass likely wants this event line:

1. `LaundrySessionStarted`
2. `LaundrySessionLocationSet`
3. `LaundryExpenseEntryAdded`

That is enough to prove:

- the session exists
- the location is part of the record
- the journal has at least one durable expense line

## First Read/Reaction Expectations

Even before we model reads formally, this path implies a few obvious reactions:

- after `LaundrySessionStarted`, the app should know there is an active session
- after `LaundrySessionLocationSet`, the app should show location context
- after `LaundryExpenseEntryAdded`, the app should show at least one logged entry and a running total

Those are not the main modeling focus yet, but they are useful pressure when checking whether the event line feels complete.

## What To Look For During Modeling

As we work this path in the visual Event Modeling tool, we should watch for:

- missing events
- event names that are actually commands in disguise
- facts that belong on a different event
- whether location should be part of session start or remain its own event
- whether the first-entry path needs an explicit session-close event right away
