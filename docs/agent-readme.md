# Agent README

This file is the AI-first orientation surface for `CheddarBooks`.

Read it after [`README.md`](../README.md) when you need to understand the repo locally instead of relying on prior chat memory.

## Purpose

`CheddarBooks` is the extracted concrete application line for the CheddarBooks area.

It begins with `LaundryLog`.

This repo should carry enough of its own durable memory to stay understandable locally, even when some conceptual roots still exist upstream in `NEXUS-EMERGING`.

## Current Role Of This Repo

Right now this repo is:

- the local memory and implementation surface for the CheddarBooks app line
- the first concrete extracted proving ground for LaundryLog
- a place where app-level docs, code, tests, and bootstrap provenance should remain aligned

It is not just a code dump from upstream.

It needs to be understandable on its own.

## Read Next

Use this order unless the task is very narrow:

1. [`README.md`](../README.md)
2. [`current-focus.md`](current-focus.md)
3. [`foundation.md`](foundation.md)
4. [`laundrylog/introduction.md`](laundrylog/introduction.md)
5. [`laundrylog/requirements.md`](laundrylog/requirements.md)
6. [`context-packs/laundrylog-context-pack.md`](context-packs/laundrylog-context-pack.md)
7. [`../bootstrap-source.toml`](../bootstrap-source.toml)

## Memory Rules For This Repo

This repo should preserve the distinction between:

- scratch notes
- durable local docs
- bootstrap provenance and other durable history
- derived views or context packs

Short version:

- do not leave important app discoveries only in chat
- keep local app docs up to date when local behavior or understanding changes
- use bootstrap provenance as traceable history, not as the only explanation of the repo
- treat context packs and current-focus docs as views, not as the deepest source truth

## Local Docs Versus Upstream Doctrine

Use local CheddarBooks docs for:

- app mission
- current focus
- LaundryLog behavior and requirements
- local code and test alignment
- repo-specific handoffs and context packs

Use upstream NEXUS doctrine when the topic is clearly foundation-level, such as:

- shared repo-memory rules
- LOGOS and canonical-history philosophy
- broader CORTEX and FORGE direction
- cross-repo architectural doctrine

But this repo should still be understandable even if the reader has not just come from NEXUS.

## Contribution Expectations

When local app understanding changes:

- update the relevant local docs
- update tests when app behavior changes
- when code, renderer, UI, HTML, CSS, or visible behavior changes, add or update tests by default
- if a relevant test is not added or updated, say why explicitly
- record discoveries durably if they will matter later
- keep README, foundation docs, and LaundryLog docs locally navigable

When work is docs-only or tests are not applicable, say so explicitly.

For concrete UI, HTML, CSS, and screen-renderer work:

- do not guess from memory, screenshots, or prior chat when the local code can be inspected directly
- inspect the actual renderer/source files first
- inspect the currently generated artifact when one exists
- for LaundryLog renderer work, keep the checked-in current artifact under [`../workspace/laundrylog/README.md`](../workspace/laundrylog/README.md) refreshed when the related renderer change is committed
- treat `tmp/` as scratch and the repo workspace as the durable current artifact surface
- when telling a human where to review current LaundryLog HTML, point them to the tracked `workspace/` artifact first, not `tmp/`, unless the task is specifically about scratch investigation
- prefer correcting the real code and renderer contract over describing what the UI "should probably be"
- add or update a regression test when a layout or rendering bug is fixed

For local F# Interactive artifact generation:

- prefer `.fsx` scripts plus `dotnet fsi --exec`
- do not default to piping large heredocs directly into raw `dotnet fsi`
- treat `dotnet fsi <<'EOF' ... EOF` as a scratch-only fallback, not the usual path
- if the artifact generation is recurring, prefer a checked-in helper script over ad hoc shell-wrapped REPL input
- for the tracked LaundryLog workspace HTML, use [`../scripts/laundrylog/refresh-workspace-html.sh`](../scripts/laundrylog/refresh-workspace-html.sh) as the usual refresh path
- for formal browser verification of tracked LaundryLog HTML behavior, use the Playwright workspace under [`../tests/browser/README.md`](../tests/browser/README.md)
- keep `Expecto` as the primary F# model/renderer test runner and use Playwright for browser-only truth such as DOM interaction, scrolling, visibility, and storage-backed UI state

## Verification Discipline

When verifying .NET work in this repo:

- do not run `dotnet build` and `dotnet run` for the test project in parallel against the same output tree
- prefer one of:
  - `dotnet build`, then `dotnet run --no-build --project ...`
  - or just `dotnet run --project ...` by itself

This avoids transient output-copy races in `bin/Debug/net10.0/` that can look like repo problems when they are really verification-command overlap.

## Scratch Versus Durable Docs

### Scratch

Use scratch for:

- temporary work notes
- short operational handoffs
- active local experimentation

Scratch is useful, but it should not quietly become long-term guidance.

### Durable Local Docs

Use durable docs for:

- mission
- current focus
- requirements
- workflows
- screen and Event Modeling notes
- context packs that should help future contributors start correctly

If the next collaborator should be able to recover it reliably, it belongs in durable docs.

## Related

- [`current-focus.md`](current-focus.md)
- [`context-packs/README.md`](context-packs/README.md)
- [`session-handoffs/README.md`](session-handoffs/README.md)
- [`context-packs/laundrylog-context-pack.md`](context-packs/laundrylog-context-pack.md)
