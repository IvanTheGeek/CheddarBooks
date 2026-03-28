# CheddarBooks LaundryLog View Contracts

This note captures the first code-facing view shape for LaundryLog as a tool app under the CheddarBooks domain.

For the fuller visual and workflow rationale behind these view contracts, see [Concept UI Page](concept-ui-page.md).

For the first reusable interaction-primitive bridge below the app-view level, see [FnHCI Primitive Map](fnhci-primitive-map.md).

For the first explicit local state-shape direction sitting between the current app and future shared primitives, see [FnHCI Primitive State Shapes](fnhci-primitive-state-shapes.md).

## Purpose

LaundryLog is the first concrete application domain riding on top of the FnHCI and FnUI shell work.

That means the first view contracts should be:

- small
- explicit
- stable enough to test
- close to the currently understood path states
- grounded in the concept-page work that already established the first practical mobile layout and command emphasis

## Current Active Views

The first app shell should expose:

- `new-session`
- `entry-form`

These map to the currently known path states:

- `new-session`
  - `Path1.1-NewSession`
  - `Path1.2-LocationEntered`
- `entry-form`
  - `Path1.3-EntryForm`

## Current Shell Regions

The first LaundryLog shell should stay minimal:

- navigation
- workspace
- status bar

Inspector- and command-heavy regions can come later if the real app starts needing them.

## Current View Intent

### `new-session`

Purpose:

- establish laundry location context
- support manual location-first flow
- keep GPS optional and future-facing

### `entry-form`

Purpose:

- log one expense entry in the current session
- support repeat entry within the same session
- keep running session total visible
- display UTC-backed event times in user-local view form when time is shown

## Time View Rule

LaundryLog should treat UTC as the durable storage and event-model basis for time.

Views should:

- render UTC-backed timestamps in the user's local view context
- avoid making the underlying stored time ambiguous
- keep the model/storage rule and the view/display rule separate

## Code Boundary

The first code boundary for this should live in:

- `CheddarBooks.LaundryLog`
- `CheddarBooks.LaundryLog.UI`

Those domain-specific contracts should depend on the renderer-neutral FnUI shell, not on Blazor implementation details directly.
