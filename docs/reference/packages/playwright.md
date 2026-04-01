# Playwright

## Purpose In CheddarBooks

Playwright is the formal browser-verification surface for the tracked LaundryLog HTML workspace.

Use it here for:

- browser interaction truth
- DOM and visibility checks
- scrolling and storage-backed UI behavior
- repeatable verification over the tracked workspace artifacts

## Official Docs To Check First

Before guessing from local browser behavior, check:

1. Playwright docs entry point  
   <https://playwright.dev/>
2. Playwright test docs  
   <https://playwright.dev/docs/intro>
3. Playwright locator and assertions docs  
   <https://playwright.dev/docs/locators>

## Local Usage Here

Primary local references:

- [`tests/browser/README.md`](../../../tests/browser/README.md)
- [`scripts/laundrylog/verify-workspace-html.sh`](../../../scripts/laundrylog/verify-workspace-html.sh)
- [`workspace/laundrylog/README.md`](../../../workspace/laundrylog/README.md)

## Important Local Conventions

- do not target `file://` first for formal Playwright verification
- serve tracked local HTML over local HTTP for browser-proof work
- prefer the checked-in verify helper so refresh, Expecto, and Playwright run in serial
- treat Playwright as browser truth, not as a replacement for the F# model/renderer tests

## Local Gotchas

- Playwright MCP browser work should not assume `file:` URLs are usable
- formal verification should run against the tracked workspace artifacts, not scratch `tmp/` output unless the task is specifically about scratch investigation
