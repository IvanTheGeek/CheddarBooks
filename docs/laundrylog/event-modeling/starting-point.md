# LaundryLog Event Modeling Starting Point

This is the first deliberate Event Modeling starting point for LaundryLog.

## Purpose

We are not trying to model the whole future CheddarBooks system yet.

We are trying to model the smallest real LaundryLog behavior that is useful:

- capture the location
- log a washer expense
- log a dryer expense
- show the current session window from that activity history

That should be enough to establish the first real event line.

## Modeling Boundary

In this pass, stay focused on:

- one user
- one device
- one derived current session window
- one location
- washer and dryer entries at that location

Defer for now:

- multi-device convergence details
- card-statement matching
- receipt/image capture
- GPS-versus-manual location branches beyond the first durable location result
- reporting views
- tax/report exports
- advanced correction flows

## First Modeling Sequence

The current order is:

1. agree on the first events
2. lay out `PATH 1`
3. notice missing events or missing reads
4. revise the event list
5. only then start shaping commands, policies, and view reactions

## Why Events First

LaundryLog exists to create a trustworthy journal of laundry expenses over time.

That means the durable expense-entry history matters more than the first UI polish pass.

If we get the first durable event line right, the UI, storage, derived current-session view, and later convergence work have something stable to grow from.
