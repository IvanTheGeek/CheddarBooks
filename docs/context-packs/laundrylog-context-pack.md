# LaundryLog Context Pack

This is the first read-optimized startup bundle for work on `LaundryLog`.

Use it when a human or AI contributor needs the shortest practical path into the current LaundryLog line.

## Mission

LaundryLog is a small focused CheddarBooks tool app for quickly recording laundry expenses in a way that supports later recordkeeping and stronger audit defense.

## Current Role In This Repo

LaundryLog is the first concrete tool app in the extracted `CheddarBooks` repo.

It is currently:

- the main proving ground for local app memory
- the first concrete application surface in this repo
- the place where requirements, Event Modeling, and UI direction are being made explicit

## Read First

1. [`../foundation.md`](../foundation.md)
2. [`../agent-readme.md`](../agent-readme.md)
3. [`../current-focus.md`](../current-focus.md)
4. [`../laundrylog/introduction.md`](../laundrylog/introduction.md)
5. [`../laundrylog/requirements.md`](../laundrylog/requirements.md)

## Read Next

- [`../laundrylog/README.md`](../laundrylog/README.md)
- [`../laundrylog/concept-ui-page.md`](../laundrylog/concept-ui-page.md)
- [`../laundrylog/screens.md`](../laundrylog/screens.md)
- [`../laundrylog/workflows.md`](../laundrylog/workflows.md)
- [`../laundrylog/view-contracts.md`](../laundrylog/view-contracts.md)
- [`../laundrylog/event-modeling/README.md`](../laundrylog/event-modeling/README.md)
- [`../../bootstrap-source.toml`](../../bootstrap-source.toml)

## Current Known Boundaries

- LaundryLog is a concrete small app, not only a platform demo
- it sits under the CheddarBooks line
- it should remain understandable locally in this repo
- local docs should not depend on chat memory
- upstream NEXUS doctrine may still exist, but local app meaning belongs here too

## Known Constraints

- the repo is still early and bootstrap-derived
- LaundryLog is still shaping its Event Modeling and UI direction
- offline use, phone-first entry, and later convergence matter early
- the app should stay simple and high-friction-free rather than expanding into a broad bookkeeping suite too early

## Open Questions

- what the first full happy-path implementation slice should be after the current Event Modeling pass
- how quickly the current Penpot and FnUI direction should become executable runtime work
- what the right local sync and convergence seam should be
- which parts of the app memory should remain local here versus upstream in NEXUS doctrine

## Next Likely Work Areas

- continue Event Modeling and path clarification
- keep screens, workflows, and command/view slices aligned
- move more of the current understanding into clear local durable docs
- grow the app implementation in step with the documented model

## Notes About This Pack

This file is a view.

It is meant to help startup and continuity.

If it drifts from the stronger LaundryLog docs, the stronger docs win and this pack should be updated.
