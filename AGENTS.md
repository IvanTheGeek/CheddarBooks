# Repository Instructions

This repository is the extracted concrete CheddarBooks application line.

It begins with LaundryLog and should remain understandable locally.

Read in this order before substantial work:

1. [`README.md`](README.md)
2. [`docs/agent-readme.md`](docs/agent-readme.md)
3. [`docs/current-focus.md`](docs/current-focus.md)
4. [`docs/foundation.md`](docs/foundation.md)
5. [`docs/context-packs/laundrylog-context-pack.md`](docs/context-packs/laundrylog-context-pack.md)
6. the relevant LaundryLog docs, code, and tests

Working rules:

- do not rely on chat memory alone for app understanding
- do not guess from memory, old chat, screenshots, or stale assumptions when the current repo state can be inspected directly; verify the actual docs, code, tests, generated artifacts, and branch/worktree shape before acting
- when a task depends on an external package, check the package's official docs entry point, onboarding docs, and examples before inferring behavior from generated outputs or local quirks
- keep local durable docs current when local app understanding changes
- use local docs for app meaning and upstream NEXUS doctrine only where it is truly foundational
- inherit NEXUS-wide package guidance by default and add or update a local note under [`docs/reference/packages/`](docs/reference/packages/README.md) only when CheddarBooks has stronger package-specific workflows or gotchas
- keep scratch, durable docs, bootstrap provenance, and derived views distinct
- if a discovery will matter later, record it durably here
- for now, keep `main` as the default active branch and merge accepted side work back into `main` promptly instead of leaving extra branches alive
- when code, renderer, or visible behavior changes, add or update tests by default
- if a relevant test is not added or updated, say why explicitly
- for UI, HTML, CSS, and screen work, inspect the actual source and current generated artifact before changing behavior

Primary references:

- [`docs/agent-readme.md`](docs/agent-readme.md)
- [`docs/current-focus.md`](docs/current-focus.md)
- [`docs/reference/packages/README.md`](docs/reference/packages/README.md)
- [`docs/context-packs/laundrylog-context-pack.md`](docs/context-packs/laundrylog-context-pack.md)
- [`bootstrap-source.toml`](bootstrap-source.toml)
