namespace CheddarBooks.LaundryLog.UI

open System
open System.Net
open System.Text
open System.Text.Json

/// Tracks the visible status of one startup checkpoint in the boot sequence.
type AppBootCheckStatus =
    | Pending
    | Active
    | Complete

/// Carries one rendered startup checkpoint in the splash family.
type AppBootCheckState =
    { Label: string
      Status: AppBootCheckStatus }

/// Carries the small system-facing boot surface used by the first screen-path proving ground.
type AppBootScreenState =
    { SurfaceName: string
      Note: string option
      ShowHeader: bool
      Header: HeaderBarState
      PrimaryMessage: string
      SecondaryMessage: string
      BootChecks: AppBootCheckState list }

/// Distinguishes the current screen-path surfaces.
type ScreenPathSurfaceState =
    | AppBootSurface of AppBootScreenState
    | AppScreenSurface of ScreenSurfaceState

/// Distinguishes the current screen-path lenses.
type ScreenPathLensKind =
    | ApplicationLifecycleLens
    | AppRuntimeLens
    | ScreenPathLens

/// Describes one ordered screen-path step.
type ScreenPathStepState =
    { StepKey: string
      StepTitle: string
      StepNote: string option
      ContextLabel: string
      LensKind: ScreenPathLensKind
      LensLabel: string
      Surface: ScreenPathSurfaceState }

/// Describes one rendered screen path.
type ScreenPathState =
    { PathId: string
      Title: string
      Description: string
      ScenarioLabel: string
      Assumptions: string list
      Steps: ScreenPathStepState list }

/// Carries self-update metadata for a generated path artifact.
type ScreenPathUpdateState =
    { Version: string
      UpdatedAtUtc: string
      PollIntervalMs: int }

[<RequireQualifiedAccess>]
module ScreenPathHtmlExamples =
    let private expect description result =
        match result with
        | Ok value -> value
        | Error message -> failwith $"Expected a valid {description}. {message}"

    let private bootCheck label status = { Label = label; Status = status }

    let private appBootSurface surfaceName note primaryMessage secondaryMessage bootChecks =
        { SurfaceName = surfaceName
          Note = Some note
          ShowHeader = false
          Header = HeaderBarState.tryCreate "LaundryLog" None None |> expect "app boot header"
          PrimaryMessage = primaryMessage
          SecondaryMessage = secondaryMessage
          BootChecks = bootChecks }

    let private step stepKey stepTitle stepNote contextLabel lensKind lensLabel surface =
        { StepKey = stepKey
          StepTitle = stepTitle
          StepNote = stepNote
          ContextLabel = contextLabel
          LensKind = lensKind
          LensLabel = lensLabel
          Surface = surface }

    /// Returns the first app/screen-path lens for LaundryLog.
    let path1StartupToFirstEntry () : ScreenPathState =
        { PathId = "PATH1"
          Title = "PATH 1: Fresh First Launch -> Need Location -> First Entry"
          Description =
            "Screen-path lens over a fresh first launch with no known local data. It follows application lifecycle, app runtime, and user-visible screen states until the first expense is logged."
          ScenarioLabel = "fresh first launch with no known local data"
          Assumptions =
            [ "No saved location is available yet."
              "No active laundry session or pending draft exists."
              "Startup/runtime checks must finish before the first usable screen appears."
              "The initial route should resolve to Need Location before the first entry is composed." ]
          Steps =
            [ step
                  "01-app-started"
                  "AppStarted"
                  (Some "User taps the app icon and the splash screen appears immediately.")
                  "ApplicationLifecycle"
                  ApplicationLifecycleLens
                  "application lifecycle lens"
                  (appBootSurface
                      "Screen.AppStart - Splash"
                      "fresh first launch before runtime checks complete"
                      "Starting LaundryLog"
                      "Preparing the first-launch startup sequence."
                      [ bootCheck "AppStarted observed" Active
                        bootCheck "Runtime checks running" Pending
                        bootCheck "No known local session" Pending
                        bootCheck "Route to Need Location" Pending ]
                   |> AppBootSurface)
              step
                  "02-runtime-checks"
                  "Runtime Checks"
                  (Some "Startup/runtime checks begin while the splash state remains visible.")
                  "RuntimeOrchestration"
                  AppRuntimeLens
                  "app runtime lens"
                  (appBootSurface
                      "Screen.AppStart - Runtime Checks"
                      "first-launch runtime checks are in progress"
                      "Checking startup requirements"
                      "Inspecting local state, offline readiness, and runtime startup conditions."
                      [ bootCheck "AppStarted observed" Complete
                        bootCheck "Runtime checks running" Active
                        bootCheck "No known local session" Pending
                        bootCheck "Route to Need Location" Pending ]
                   |> AppBootSurface)
              step
                  "03-no-local-session"
                  "No Local Session"
                  (Some "Fresh-first-launch assumptions are confirmed: no saved location and no active session were found.")
                  "RuntimeOrchestration"
                  AppRuntimeLens
                  "app runtime lens"
                  (appBootSurface
                      "Screen.AppStart - No Local Session"
                      "fresh first launch confirmed from local/runtime checks"
                      "No local session found"
                      "No saved location, active session, or pending expense draft was discovered."
                      [ bootCheck "AppStarted observed" Complete
                        bootCheck "Runtime checks running" Complete
                        bootCheck "No known local session" Active
                        bootCheck "Route to Need Location" Pending ]
                   |> AppBootSurface)
              step
                  "04-route-resolved"
                  "Route Resolved"
                  (Some "Runtime orchestration resolves Need Location as the first usable screen.")
                  "RuntimeOrchestration"
                  AppRuntimeLens
                  "app runtime lens"
                  (appBootSurface
                      "Screen.AppStart - Route Resolved"
                      "runtime routes the app to the first usable screen"
                      "Routing to Need Location"
                      "Startup checks are complete and the first-launch path is ready to enter the app."
                      [ bootCheck "AppStarted observed" Complete
                        bootCheck "Runtime checks running" Complete
                        bootCheck "No known local session" Complete
                        bootCheck "Route to Need Location" Active ]
                   |> AppBootSurface)
              step
                  "05-need-location"
                  "Need Location"
                  (Some "Fresh start with no known location yet.")
                  "ScreenPath"
                  ScreenPathLens
                  "screen path lens"
                  (NewSessionScreen
                      ( "Screen.NewSession - Awaiting Location",
                        Some "manual location entry before the first expense",
                        PrimitiveStateExamples.newSessionAwaitingLocation () )
                   |> AppScreenSurface)
              step
                  "06-ready-to-set-location"
                  "Ready To Set Location"
                  (Some "The location text is entered and the confirm action is available.")
                  "ScreenPath"
                  ScreenPathLens
                  "screen path lens"
                  (NewSessionScreen
                      ( "Screen.NewSession - Ready To Set",
                        Some "manual location entered and ready to confirm",
                        PrimitiveStateExamples.newSessionLocationEntered () )
                   |> AppScreenSurface)
              step
                  "07-entry-form-ready"
                  "Entry Form Ready"
                  (Some "The app enters the main entry surface after location capture.")
                  "ScreenPath"
                  ScreenPathLens
                  "screen path lens"
                  (EntryFormScreen
                      ( "Screen.EntryForm - Ready At Location",
                        Some "location captured and the expense form is ready for the first selection",
                        PrimitiveStateExamples.entryFormReadyAtLocation () )
                   |> AppScreenSurface)
              step
                  "08-washer-draft"
                  "Washer Draft"
                  (Some "The first expense draft is composed on the main screen.")
                  "ScreenPath"
                  ScreenPathLens
                  "screen path lens"
                  (EntryFormScreen
                      ( "Screen.EntryForm - Washer Draft",
                        Some "first expense draft inside the current location context",
                        PrimitiveStateExamples.entryFormWasherCardDraft () )
                   |> AppScreenSurface)
              step
                  "09-logged-success"
                  "Logged Success"
                  (Some "The first entry is logged and the screen is ready for the next quick entry.")
                  "ScreenPath"
                  ScreenPathLens
                  "screen path lens"
                  (EntryFormScreen
                      ( "Screen.EntryForm - Logged Success",
                        Some "entry logged and the surface is ready for the next quick entry",
                        PrimitiveStateExamples.entryFormLoggedSuccess () )
                   |> AppScreenSurface) ] }

/// Renders deterministic HTML/CSS screen-path documents from the current screen surfaces.
[<RequireQualifiedAccess>]
module ScreenPathHtmlRenderer =
    let private htmlEncode (value: string) = WebUtility.HtmlEncode value
    let private jsonString (value: string) = JsonSerializer.Serialize value

    let private lensKindDomKey =
        function
        | ApplicationLifecycleLens -> "application-lifecycle"
        | AppRuntimeLens -> "app-runtime"
        | ScreenPathLens -> "screen-path"

    let private lensKindButtonLabel =
        function
        | ApplicationLifecycleLens -> "Lifecycle"
        | AppRuntimeLens -> "Runtime"
        | ScreenPathLens -> "Screen Path"

    let private defaultUpdateState () =
        let now = DateTime.UtcNow

        { Version = $"screen-path::{now:yyyyMMddHHmmssfff}"
          UpdatedAtUtc = now.ToString("yyyy-MM-ddTHH:mm:ssZ")
          PollIntervalMs = 3000 }

    let private appendLine (builder: StringBuilder) (value: string) =
        builder.AppendLine(value) |> ignore

    let private renderBootHeader (builder: StringBuilder) (headerState: HeaderBarState) =
        appendLine builder "<header class=\"ll-header\">"
        appendLine builder "<div class=\"ll-header__text\">"
        appendLine builder $"<h2 class=\"ll-header__title\">🧺 {htmlEncode headerState.Title}</h2>"
        appendLine builder "<p class=\"ll-header__subtitle\">by CheddarBooks</p>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"ll-header__badge\">🧀</div>"
        appendLine builder "</header>"

    let private bootCheckStatusClass =
        function
        | Pending -> "ll-boot-state__check--pending"
        | Active -> "ll-boot-state__check--active"
        | Complete -> "ll-boot-state__check--complete"

    let private bootCheckStatusDomKey =
        function
        | Pending -> "pending"
        | Active -> "active"
        | Complete -> "complete"

    let private bootCheckStatusGlyph =
        function
        | Pending -> "○"
        | Active -> "◉"
        | Complete -> "✓"

    let private renderBootSurface (builder: StringBuilder) (bootState: AppBootScreenState) =
        appendLine builder "<article class=\"ll-screen-surface\">"
        appendLine builder $"<div class=\"ll-screen-surface__name\">{htmlEncode bootState.SurfaceName}</div>"

        match bootState.Note with
        | Some noteText -> appendLine builder $"<p class=\"ll-screen-surface__note\">{htmlEncode noteText}</p>"
        | None -> ()

        appendLine builder "<section class=\"ll-phone-screen ll-phone-screen--boot\">"

        if bootState.ShowHeader then
            renderBootHeader builder bootState.Header

        appendLine builder "<div class=\"ll-screen-body\">"
        appendLine builder "<section class=\"ll-panel ll-panel--boot\">"
        appendLine builder "<div class=\"ll-boot-state\">"
        appendLine builder "<div class=\"ll-boot-state__icon\">🧺</div>"
        appendLine builder $"<h3 class=\"ll-boot-state__title\">{htmlEncode bootState.PrimaryMessage}</h3>"
        appendLine builder $"<p class=\"ll-boot-state__subtitle\">{htmlEncode bootState.SecondaryMessage}</p>"
        appendLine builder "<div class=\"ll-boot-state__checks\">"

        bootState.BootChecks
        |> List.iter (fun checkState ->
            let statusClass = bootCheckStatusClass checkState.Status
            let statusDomKey = bootCheckStatusDomKey checkState.Status
            let statusGlyph = bootCheckStatusGlyph checkState.Status

            appendLine builder $"<div class=\"ll-boot-state__check {statusClass}\" data-testid=\"boot-check\" data-status=\"{statusDomKey}\" data-check-label=\"{htmlEncode checkState.Label}\">"
            appendLine builder $"<span class=\"ll-boot-state__check-glyph\" aria-hidden=\"true\">{statusGlyph}</span>"
            appendLine builder $"<span class=\"ll-boot-state__check-label\">{htmlEncode checkState.Label}</span>"
            appendLine builder "</div>")

        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "</article>"

    let private renderPathSurface (builder: StringBuilder) =
        function
        | AppBootSurface bootState -> renderBootSurface builder bootState
        | AppScreenSurface screenSurface -> appendLine builder (ScreenHtmlRenderer.renderSurfaceHtml screenSurface)

    let private renderPathStyles (builder: StringBuilder) =
        appendLine builder "<style>"
        appendLine builder "html, body { height: 100%; overflow: hidden; }"
        appendLine builder ".ll-path-document { height: 100vh; padding: 14px 16px 18px; max-width: 100%; margin: 0 auto; display: grid; grid-template-rows: auto auto minmax(0, 1fr); overflow: hidden; }"
        appendLine builder ".ll-path-document__header { margin-bottom: 0; display: flex; flex-direction: column; gap: 6px; }"
        appendLine builder ".ll-path-document__topline { display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 16px; align-items: start; }"
        appendLine builder ".ll-path-document__title-zone { min-width: 0; display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".ll-path-document__eyebrow { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #0e5883; }"
        appendLine builder ".ll-path-document__title { margin: 0; font-size: 0.96rem; line-height: 1.06; font-weight: 700; }"
        appendLine builder ".ll-path-document__context { align-self: flex-start; min-width: 0; }"
        appendLine builder ".ll-path-document__context > summary { list-style: none; display: inline-flex; align-items: center; gap: 0.45rem; padding: 0.42rem 0.72rem; border-radius: 999px; background: #f8fafc; border: 1px solid #dbe5f1; cursor: pointer; color: #0f172a; }"
        appendLine builder ".ll-path-document__context > summary::-webkit-details-marker { display: none; }"
        appendLine builder ".ll-path-document__context-label { font-size: 0.63rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #64748b; }"
        appendLine builder ".ll-path-document__context-title { font-size: 0.74rem; font-weight: 700; color: #0f172a; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 34vw; }"
        appendLine builder ".ll-path-document__context-panel { margin-top: 0.55rem; padding: 0.72rem 0.85rem; border-radius: 0.95rem; background: #f8fafc; border: 1px solid #dbe5f1; display: grid; gap: 0.45rem; max-width: 58rem; }"
        appendLine builder ".ll-path-document__description { margin: 0; color: #64748b; font-size: 0.72rem; line-height: 1.28; }"
        appendLine builder ".ll-path-document__assumptions { margin: 0; padding-left: 1rem; display: grid; gap: 0.18rem; color: #475569; font-size: 0.72rem; line-height: 1.28; }"
        appendLine builder ".ll-path-document__header-controls { display: flex; gap: 0.9rem; flex-wrap: wrap; justify-content: flex-end; align-items: flex-start; }"
        appendLine builder ".ll-path-document__view-controls { display: flex; flex-direction: column; gap: 0.28rem; align-items: flex-start; }"
        appendLine builder ".ll-path-document__view-label { font-size: 0.63rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #64748b; }"
        appendLine builder ".ll-path-document__view-buttons { display: flex; gap: 0.45rem; flex-wrap: wrap; }"
        appendLine builder ".ll-path-view-toggle { border: 1px solid #cbd5e1; border-radius: 999px; background: #ffffff; color: #475569; font-size: 0.7rem; font-weight: 700; padding: 0.42rem 0.72rem; cursor: pointer; }"
        appendLine builder ".ll-path-view-toggle:hover { border-color: #94a3b8; }"
        appendLine builder ".ll-path-view-toggle.is-active { background: #0e5883; border-color: #0e5883; color: #ffffff; }"
        appendLine builder ".ll-path-document__lens-controls { display: flex; flex-direction: column; gap: 0.28rem; align-items: flex-start; }"
        appendLine builder ".ll-path-document__lens-buttons { display: flex; gap: 0.45rem; flex-wrap: wrap; }"
        appendLine builder ".ll-path-lens-toggle { border: 1px solid #cbd5e1; border-radius: 999px; background: #ffffff; color: #475569; font-size: 0.7rem; font-weight: 700; padding: 0.42rem 0.72rem; cursor: pointer; }"
        appendLine builder ".ll-path-lens-toggle:hover { border-color: #94a3b8; }"
        appendLine builder ".ll-path-lens-toggle.is-active { background: #0e5883; border-color: #0e5883; color: #ffffff; }"
        appendLine builder ".ll-path-document__update-controls { display: flex; flex-direction: column; gap: 0.28rem; align-items: flex-start; min-width: 280px; }"
        appendLine builder ".ll-path-document__update-label { font-size: 0.63rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #64748b; }"
        appendLine builder ".ll-path-document__update-buttons { display: flex; gap: 0.45rem; flex-wrap: wrap; align-items: center; }"
        appendLine builder ".ll-path-update-toggle, .ll-path-update-refresh { border: 1px solid #cbd5e1; border-radius: 999px; background: #ffffff; color: #475569; font-size: 0.7rem; font-weight: 700; padding: 0.42rem 0.72rem; cursor: pointer; }"
        appendLine builder ".ll-path-update-toggle:hover, .ll-path-update-refresh:hover:not(:disabled) { border-color: #94a3b8; }"
        appendLine builder ".ll-path-update-toggle.is-active { background: #0e5883; border-color: #0e5883; color: #ffffff; }"
        appendLine builder ".ll-path-update-refresh { background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); border-color: #ffb74d; color: #ffffff; }"
        appendLine builder ".ll-path-update-refresh:disabled { background: #e2e8f0; border-color: #cbd5e1; color: #94a3b8; cursor: default; }"
        appendLine builder ".ll-path-document__update-status { font-size: 0.7rem; line-height: 1.2; color: #64748b; }"
        appendLine builder ".ll-path-document__update-status[data-state=\"available\"] { color: #9a3412; font-weight: 700; }"
        appendLine builder ".ll-path-document__update-status[data-state=\"error\"] { color: #b91c1c; }"
        appendLine builder ".ll-path-document__update-status[data-state=\"auto\"] { color: #0e5883; font-weight: 700; }"
        appendLine builder ".ll-path-document[data-view-mode=\"summary\"] .ll-path-document__assumptions, .ll-path-document[data-view-mode=\"summary\"] .ll-path-step__note, .ll-path-document[data-view-mode=\"summary\"] .ll-path-step__meta, .ll-path-document[data-view-mode=\"summary\"] .ll-screen-surface__name, .ll-path-document[data-view-mode=\"summary\"] .ll-screen-surface__note { display: none; }"
        appendLine builder ".ll-path-document[data-view-mode=\"summary\"] .ll-path-document__context-panel { display: none; }"
        appendLine builder ".ll-path-document[data-view-mode=\"standard\"] .ll-path-document__context-panel .ll-path-document__assumptions { display: none; }"
        appendLine builder ".ll-path-scroll-controls { display: grid; grid-template-columns: auto auto minmax(0, 1fr) auto auto; gap: 10px; align-items: center; padding: 4px 4px 8px; background: linear-gradient(180deg, #ffffff 0%, rgba(255, 255, 255, 0.98) 72%, rgba(255, 255, 255, 0.92) 100%); }"
        appendLine builder ".ll-path-nav-button { width: 42px; height: 42px; border: 0; border-radius: 999px; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); color: #ffffff; font-size: 1rem; font-weight: 800; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; box-shadow: 0 2px 8px rgba(255, 183, 77, 0.32); }"
        appendLine builder ".ll-path-nav-button:hover:not(:disabled) { filter: brightness(0.98); }"
        appendLine builder ".ll-path-nav-button:active:not(:disabled) { transform: scale(0.96); }"
        appendLine builder ".ll-path-nav-button:disabled { background: #cbd5e1; color: #94a3b8; cursor: default; box-shadow: none; }"
        appendLine builder ".ll-path-nav-button__icon { display: inline-flex; align-items: center; justify-content: center; gap: 0.12rem; line-height: 1; }"
        appendLine builder ".ll-path-nav-button__bar { width: 3px; height: 15px; border-radius: 999px; background: currentColor; flex: 0 0 auto; }"
        appendLine builder ".ll-path-nav-button__chevron { width: 11px; height: 11px; border-top: 3px solid currentColor; border-right: 3px solid currentColor; flex: 0 0 auto; }"
        appendLine builder ".ll-path-nav-button__chevron--right { transform: rotate(45deg); }"
        appendLine builder ".ll-path-nav-button__chevron--left { transform: rotate(-135deg); }"
        appendLine builder ".ll-path-scrollbar { overflow-x: auto; overflow-y: hidden; scrollbar-gutter: stable both-edges; }"
        appendLine builder ".ll-path-scrollbar__content { height: 1px; }"
        appendLine builder ".ll-path-stage { min-height: 0; overflow-y: auto; overflow-x: hidden; padding: 0 4px 12px; }"
        appendLine builder ".ll-path-flow-viewport { overflow-x: auto; overflow-y: visible; scrollbar-width: none; }"
        appendLine builder ".ll-path-flow-viewport::-webkit-scrollbar { display: none; }"
        appendLine builder ".ll-path-flow { display: grid; grid-auto-flow: column; grid-auto-columns: minmax(360px, 380px); width: max-content; gap: 18px; align-items: start; padding: 4px 4px 18px; }"
        appendLine builder ".ll-path-step { display: flex; flex-direction: column; gap: 8px; }"
        appendLine builder ".ll-path-step__eyebrow { font-size: 0.64rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #94a3b8; }"
        appendLine builder ".ll-path-step__title { margin: 0; font-size: 0.82rem; line-height: 1.15; font-weight: 700; color: #0f172a; }"
        appendLine builder ".ll-path-step__note { margin: 0; font-size: 0.72rem; line-height: 1.28; color: #64748b; min-height: 2.4em; }"
        appendLine builder ".ll-path-step__meta { display: flex; gap: 0.45rem; flex-wrap: wrap; align-items: center; }"
        appendLine builder ".ll-path-step__context { align-self: flex-start; padding: 0.2rem 0.5rem; border-radius: 999px; background: #eef2f7; color: #475569; font-size: 0.66rem; font-weight: 700; letter-spacing: 0.04em; }"
        appendLine builder ".ll-path-step__lens { align-self: flex-start; padding: 0.2rem 0.5rem; border-radius: 999px; background: #e2e8f0; color: #475569; font-size: 0.66rem; font-weight: 700; letter-spacing: 0.04em; text-transform: uppercase; }"
        appendLine builder ".ll-panel--boot { min-height: 520px; justify-content: center; }"
        appendLine builder ".ll-boot-state { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 0.85rem; padding: 2rem 1rem; }"
        appendLine builder ".ll-boot-state__icon { width: 88px; height: 88px; border-radius: 1.25rem; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); display: flex; align-items: center; justify-content: center; font-size: 2.6rem; box-shadow: 0 8px 20px rgba(255, 183, 77, 0.35); }"
        appendLine builder ".ll-boot-state__title { margin: 0; font-size: 1.15rem; font-weight: 700; color: #0f172a; }"
        appendLine builder ".ll-boot-state__subtitle { margin: 0; font-size: 0.82rem; line-height: 1.35; color: #64748b; max-width: 26ch; }"
        appendLine builder ".ll-boot-state__checks { width: 100%; display: flex; flex-direction: column; gap: 0.45rem; }"
        appendLine builder ".ll-boot-state__check { padding: 0.65rem 0.8rem; border-radius: 0.75rem; background: #f8fafc; border: 1px solid #e2e8f0; font-size: 0.76rem; font-weight: 600; color: #475569; display: grid; grid-template-columns: auto minmax(0, 1fr); gap: 0.55rem; align-items: center; text-align: left; }"
        appendLine builder ".ll-boot-state__check-glyph { font-size: 0.88rem; line-height: 1; }"
        appendLine builder ".ll-boot-state__check--pending { background: #f8fafc; color: #94a3b8; }"
        appendLine builder ".ll-boot-state__check--active { background: #fff7ed; border-color: #fdba74; color: #9a3412; }"
        appendLine builder ".ll-boot-state__check--complete { background: #eff6ff; border-color: #93c5fd; color: #1d4ed8; }"
        appendLine builder "@media (max-width: 920px) { .ll-path-document { padding-left: 12px; padding-right: 12px; } .ll-path-document__topline { grid-template-columns: 1fr; } .ll-path-document__header-controls { justify-content: flex-start; } .ll-path-document__context-title { max-width: 72vw; } .ll-path-flow { grid-auto-columns: minmax(320px, 88vw); } }"
        appendLine builder "</style>"

    let private renderPathScript (builder: StringBuilder) (screenPathState: ScreenPathState) (updateState: ScreenPathUpdateState option) =
        appendLine builder "<script>"
        appendLine builder "(function () {"
        appendLine builder "  const pathDocument = document.querySelector('.ll-path-document');"
        appendLine builder "  const scrollbar = document.getElementById('ll-path-scrollbar');"
        appendLine builder "  const scrollbarContent = document.getElementById('ll-path-scrollbar-content');"
        appendLine builder "  const viewport = document.getElementById('ll-path-flow-viewport');"
        appendLine builder "  const flow = document.getElementById('ll-path-flow');"
        appendLine builder "  const startButton = document.getElementById('ll-path-nav-start');"
        appendLine builder "  const previousButton = document.getElementById('ll-path-nav-previous');"
        appendLine builder "  const nextButton = document.getElementById('ll-path-nav-next');"
        appendLine builder "  const endButton = document.getElementById('ll-path-nav-end');"
        appendLine builder "  const viewModeButtons = Array.from(document.querySelectorAll('.ll-path-view-toggle'));"
        appendLine builder "  const lensButtons = Array.from(document.querySelectorAll('.ll-path-lens-toggle'));"
        appendLine builder "  const stepElements = Array.from(document.querySelectorAll('.ll-path-step'));"
        appendLine builder "  const updateModeButtons = Array.from(document.querySelectorAll('.ll-path-update-toggle'));"
        appendLine builder "  const updateStatus = document.getElementById('ll-path-update-status');"
        appendLine builder "  const refreshButton = document.getElementById('ll-path-refresh-now');"
        appendLine builder $"  const updateModeStorageKey = 'll-path-update-mode::{htmlEncode (screenPathState.PathId.ToLowerInvariant())}';"
        appendLine builder $"  const lensFilterStorageKey = 'll-path-lenses::{htmlEncode (screenPathState.PathId.ToLowerInvariant())}';"
        appendLine builder "  const defaultLensKeys = ['application-lifecycle', 'app-runtime', 'screen-path'];"
        appendLine builder "  const applyViewMode = (mode) => {"
        appendLine builder "    if (!pathDocument) { return; }"
        appendLine builder "    pathDocument.dataset.viewMode = mode;"
        appendLine builder "    viewModeButtons.forEach((button) => {"
        appendLine builder "      const isActive = button.dataset.viewMode === mode;"
        appendLine builder "      button.classList.toggle('is-active', isActive);"
        appendLine builder "      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');"
        appendLine builder "    });"
        appendLine builder "  };"
        appendLine builder "  viewModeButtons.forEach((button) => {"
        appendLine builder "    button.addEventListener('click', () => applyViewMode(button.dataset.viewMode || 'standard'));"
        appendLine builder "  });"
        appendLine builder "  applyViewMode((pathDocument && pathDocument.dataset.viewMode) || 'standard');"
        appendLine builder "  const readStoredLensKeys = () => {"
        appendLine builder "    try {"
        appendLine builder "      const raw = window.localStorage.getItem(lensFilterStorageKey);"
        appendLine builder "      if (!raw) { return defaultLensKeys.slice(); }"
        appendLine builder "      const parsed = JSON.parse(raw);"
        appendLine builder "      if (!Array.isArray(parsed)) { return defaultLensKeys.slice(); }"
        appendLine builder "      const normalized = parsed.filter((value, index, array) => typeof value === 'string' && defaultLensKeys.includes(value) && array.indexOf(value) === index);"
        appendLine builder "      return normalized.length > 0 ? normalized : defaultLensKeys.slice();"
        appendLine builder "    } catch (_) {"
        appendLine builder "      return defaultLensKeys.slice();"
        appendLine builder "    }"
        appendLine builder "  };"
        appendLine builder "  const persistLensKeys = (lensKeys) => {"
        appendLine builder "    try { window.localStorage.setItem(lensFilterStorageKey, JSON.stringify(lensKeys)); } catch (_) { }"
        appendLine builder "  };"
        appendLine builder "  const applyLensFilters = (lensKeys) => {"
        appendLine builder "    const activeLensKeys = lensKeys.filter((value, index, array) => defaultLensKeys.includes(value) && array.indexOf(value) === index);"
        appendLine builder "    if (activeLensKeys.length === 0) { return; }"
        appendLine builder "    if (pathDocument) { pathDocument.dataset.activeLenses = activeLensKeys.join(','); }"
        appendLine builder "    lensButtons.forEach((button) => {"
        appendLine builder "      const isActive = activeLensKeys.includes(button.dataset.lensKey || '');"
        appendLine builder "      button.classList.toggle('is-active', isActive);"
        appendLine builder "      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');"
        appendLine builder "    });"
        appendLine builder "    stepElements.forEach((step) => {"
        appendLine builder "      const stepLensKey = step.dataset.lensKey || '';"
        appendLine builder "      step.hidden = !activeLensKeys.includes(stepLensKey);"
        appendLine builder "    });"
        appendLine builder "    persistLensKeys(activeLensKeys);"
        appendLine builder "    window.requestAnimationFrame(() => syncWidth());"
        appendLine builder "  };"
        appendLine builder "  lensButtons.forEach((button) => {"
        appendLine builder "    button.addEventListener('click', () => {"
        appendLine builder "      const clickedLensKey = button.dataset.lensKey || '';"
        appendLine builder "      const currentLensKeys = readStoredLensKeys();"
        appendLine builder "      const isActive = currentLensKeys.includes(clickedLensKey);"
        appendLine builder "      const nextLensKeys = isActive"
        appendLine builder "        ? (currentLensKeys.length > 1 ? currentLensKeys.filter((lensKey) => lensKey !== clickedLensKey) : currentLensKeys)"
        appendLine builder "        : currentLensKeys.concat(clickedLensKey);"
        appendLine builder "      applyLensFilters(nextLensKeys);"
        appendLine builder "    });"
        appendLine builder "  });"
        appendLine builder "  applyLensFilters(readStoredLensKeys());"
        appendLine builder "  const setUpdateStatus = (message, state) => {"
        appendLine builder "    if (!updateStatus) { return; }"
        appendLine builder "    updateStatus.textContent = message;"
        appendLine builder "    if (state) { updateStatus.dataset.state = state; } else { delete updateStatus.dataset.state; }"
        appendLine builder "  };"
        appendLine builder "  const setRefreshPending = (isPending) => {"
        appendLine builder "    if (!refreshButton) { return; }"
        appendLine builder "    refreshButton.disabled = !isPending;"
        appendLine builder "    refreshButton.hidden = !isPending;"
        appendLine builder "  };"
        appendLine builder "  const readStoredUpdateMode = () => {"
        appendLine builder "    try { return window.localStorage.getItem(updateModeStorageKey) || 'notify'; } catch (_) { return 'notify'; }"
        appendLine builder "  };"
        appendLine builder "  const persistUpdateMode = (mode) => {"
        appendLine builder "    try { window.localStorage.setItem(updateModeStorageKey, mode); } catch (_) { }"
        appendLine builder "  };"
        appendLine builder "  const applyUpdateMode = (mode) => {"
        appendLine builder "    const normalizedMode = mode === 'auto' ? 'auto' : 'notify';"
        appendLine builder "    if (pathDocument) { pathDocument.dataset.updateMode = normalizedMode; }"
        appendLine builder "    persistUpdateMode(normalizedMode);"
        appendLine builder "    updateModeButtons.forEach((button) => {"
        appendLine builder "      const isActive = button.dataset.updateMode === normalizedMode;"
        appendLine builder "      button.classList.toggle('is-active', isActive);"
        appendLine builder "      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');"
        appendLine builder "    });"
        appendLine builder "    return normalizedMode;"
        appendLine builder "  };"
        appendLine builder "  updateModeButtons.forEach((button) => {"
        appendLine builder "    button.addEventListener('click', () => applyUpdateMode(button.dataset.updateMode || 'notify'));"
        appendLine builder "  });"
        appendLine builder "  applyUpdateMode(readStoredUpdateMode());"
        appendLine builder "  if (!scrollbar || !scrollbarContent || !viewport || !flow) { return; }"
        appendLine builder "  const baseFlowPaddingRight = parseFloat(window.getComputedStyle(flow).paddingRight) || 0;"
        appendLine builder "  let syncingScroll = false;"
        appendLine builder "  let animatingScroll = false;"
        appendLine builder "  let animationFrameId = 0;"
        appendLine builder "  let currentStepMetrics = { targets: [0], logicalMaxTarget: 0 };"
        appendLine builder "  const measureStepMetrics = () => {"
        appendLine builder "    const steps = Array.from(flow.querySelectorAll('.ll-path-step')).filter((step) => !step.hidden);"
        appendLine builder "    if (steps.length === 0) {"
        appendLine builder "      return { targets: [0], logicalMaxTarget: 0 };"
        appendLine builder "    }"
        appendLine builder "    const firstOffset = steps[0].offsetLeft;"
        appendLine builder "    const normalizedTargets = steps.map((step) => Math.max(0, Math.round(step.offsetLeft - firstOffset)));"
        appendLine builder "    const contentRightEdge = Math.max(...steps.map((step, index) => normalizedTargets[index] + Math.round(step.getBoundingClientRect().width)));"
        appendLine builder "    const maxStartIndexCandidate = normalizedTargets.findIndex((target) => contentRightEdge - target <= viewport.clientWidth + 1);"
        appendLine builder "    const maxStartIndex = maxStartIndexCandidate >= 0 ? maxStartIndexCandidate : normalizedTargets.length - 1;"
        appendLine builder "    const targets = normalizedTargets.slice(0, maxStartIndex + 1);"
        appendLine builder "    const logicalMaxTarget = targets.length > 0 ? targets[targets.length - 1] : 0;"
        appendLine builder "    return { targets, logicalMaxTarget };"
        appendLine builder "  };"
        appendLine builder "  const logicalMaxScrollLeft = () => currentStepMetrics.logicalMaxTarget;"
        appendLine builder "  const clampLeft = (left) => Math.max(0, Math.min(left, logicalMaxScrollLeft()));"
        appendLine builder "  const setSyncedScrollLeft = (left) => {"
        appendLine builder "    const clampedLeft = clampLeft(left);"
        appendLine builder "    syncingScroll = true;"
        appendLine builder "    viewport.scrollLeft = clampedLeft;"
        appendLine builder "    scrollbar.scrollLeft = clampedLeft;"
        appendLine builder "    syncingScroll = false;"
        appendLine builder "    updateButtonState();"
        appendLine builder "  };"
        appendLine builder "  const cancelScrollAnimation = () => {"
        appendLine builder "    if (animationFrameId !== 0) {"
        appendLine builder "      window.cancelAnimationFrame(animationFrameId);"
        appendLine builder "      animationFrameId = 0;"
        appendLine builder "    }"
        appendLine builder "    animatingScroll = false;"
        appendLine builder "  };"
        appendLine builder "  const stepIndexAtOrBefore = (targets, currentLeft) => {"
        appendLine builder "    let index = 0;"
        appendLine builder "    for (let i = 0; i < targets.length; i += 1) {"
        appendLine builder "      if (targets[i] <= currentLeft + 4) { index = i; }"
        appendLine builder "    }"
        appendLine builder "    return index;"
        appendLine builder "  };"
        appendLine builder "  const animateScrollToColumn = (targetLeft) => {"
        appendLine builder "    const startLeft = viewport.scrollLeft;"
        appendLine builder "    const endLeft = clampLeft(targetLeft);"
        appendLine builder "    if (Math.abs(endLeft - startLeft) <= 1) {"
        appendLine builder "      setSyncedScrollLeft(endLeft);"
        appendLine builder "      return;"
        appendLine builder "    }"
        appendLine builder "    cancelScrollAnimation();"
        appendLine builder "    animatingScroll = true;"
        appendLine builder "    const durationMs = 240;"
        appendLine builder "    const startedAt = window.performance.now();"
        appendLine builder "    const easeInOutQuad = (progress) => progress < 0.5 ? 2 * progress * progress : 1 - Math.pow(-2 * progress + 2, 2) / 2;"
        appendLine builder "    const tick = (now) => {"
        appendLine builder "      const progress = Math.min(1, (now - startedAt) / durationMs);"
        appendLine builder "      const easedProgress = easeInOutQuad(progress);"
        appendLine builder "      const nextLeft = Math.round(startLeft + ((endLeft - startLeft) * easedProgress));"
        appendLine builder "      setSyncedScrollLeft(nextLeft);"
        appendLine builder "      if (progress < 1) {"
        appendLine builder "        animationFrameId = window.requestAnimationFrame(tick);"
        appendLine builder "      } else {"
        appendLine builder "        animationFrameId = 0;"
        appendLine builder "        animatingScroll = false;"
        appendLine builder "        setSyncedScrollLeft(endLeft);"
        appendLine builder "      }"
        appendLine builder "    };"
        appendLine builder "    animationFrameId = window.requestAnimationFrame(tick);"
        appendLine builder "  };"
        appendLine builder "  const updateButtonState = () => {"
        appendLine builder "    const currentLeft = viewport.scrollLeft;"
        appendLine builder "    const currentMax = logicalMaxScrollLeft();"
        appendLine builder "    if (startButton) { startButton.disabled = currentLeft <= 2; }"
        appendLine builder "    if (previousButton) { previousButton.disabled = currentLeft <= 2; }"
        appendLine builder "    if (nextButton) { nextButton.disabled = currentLeft >= currentMax - 2; }"
        appendLine builder "    if (endButton) { endButton.disabled = currentLeft >= currentMax - 2; }"
        appendLine builder "  };"
        appendLine builder "  const syncWidth = () => {"
        appendLine builder "    flow.style.paddingRight = `${baseFlowPaddingRight}px`;"
        appendLine builder "    currentStepMetrics = measureStepMetrics();"
        appendLine builder "    const nativeMaxScroll = Math.max(0, flow.scrollWidth - viewport.clientWidth);"
        appendLine builder "    const extraTrailingSpace = Math.max(0, currentStepMetrics.logicalMaxTarget - nativeMaxScroll);"
        appendLine builder "    flow.style.paddingRight = `${baseFlowPaddingRight + extraTrailingSpace}px`;"
        appendLine builder "    scrollbarContent.style.width = `${Math.ceil(scrollbar.offsetWidth + currentStepMetrics.logicalMaxTarget)}px`;"
        appendLine builder "    if (!syncingScroll) { scrollbar.scrollLeft = clampLeft(viewport.scrollLeft); }"
        appendLine builder "    updateButtonState();"
        appendLine builder "  };"
        appendLine builder "  scrollbar.addEventListener('scroll', () => {"
        appendLine builder "    if (syncingScroll || animatingScroll) { return; }"
        appendLine builder "    cancelScrollAnimation();"
        appendLine builder "    setSyncedScrollLeft(scrollbar.scrollLeft);"
        appendLine builder "  });"
        appendLine builder "  viewport.addEventListener('scroll', () => {"
        appendLine builder "    if (syncingScroll || animatingScroll) { return; }"
        appendLine builder "    cancelScrollAnimation();"
        appendLine builder "    syncingScroll = true;"
        appendLine builder "    scrollbar.scrollLeft = clampLeft(viewport.scrollLeft);"
        appendLine builder "    syncingScroll = false;"
        appendLine builder "    updateButtonState();"
        appendLine builder "  });"
        appendLine builder "  const refreshScrollMetrics = () => {"
        appendLine builder "    syncWidth();"
        appendLine builder "    return currentStepMetrics;"
        appendLine builder "  };"
        appendLine builder "  const scrollToStart = () => {"
        appendLine builder "    refreshScrollMetrics();"
        appendLine builder "    animateScrollToColumn(0);"
        appendLine builder "  };"
        appendLine builder "  const scrollToEnd = () => {"
        appendLine builder "    refreshScrollMetrics();"
        appendLine builder "    animateScrollToColumn(logicalMaxScrollLeft());"
        appendLine builder "  };"
        appendLine builder "  const scrollByOneColumn = (direction) => {"
        appendLine builder "    const targets = refreshScrollMetrics().targets;"
        appendLine builder "    if (targets.length === 0) { return; }"
        appendLine builder "    const currentIndex = stepIndexAtOrBefore(targets, viewport.scrollLeft);"
        appendLine builder "    const targetIndex = Math.max(0, Math.min(targets.length - 1, currentIndex + direction));"
        appendLine builder "    animateScrollToColumn(targets[targetIndex]);"
        appendLine builder "  };"
        appendLine builder "  if (startButton) { startButton.addEventListener('click', scrollToStart); }"
        appendLine builder "  if (previousButton) { previousButton.addEventListener('click', () => scrollByOneColumn(-1)); }"
        appendLine builder "  if (nextButton) { nextButton.addEventListener('click', () => scrollByOneColumn(1)); }"
        appendLine builder "  if (endButton) { endButton.addEventListener('click', scrollToEnd); }"
        appendLine builder "  syncWidth();"
        appendLine builder "  window.requestAnimationFrame(() => syncWidth());"
        appendLine builder "  window.addEventListener('resize', syncWidth);"
        appendLine builder "  window.addEventListener('load', syncWidth);"
        appendLine builder "  if (window.ResizeObserver) {"
        appendLine builder "    new ResizeObserver(syncWidth).observe(flow);"
        appendLine builder "  }"
        appendLine builder "  if (document.fonts && document.fonts.ready) {"
        appendLine builder "    document.fonts.ready.then(() => syncWidth()).catch(() => {});"
        appendLine builder "  }"

        match updateState with
        | Some updateInfo ->
            let versionJson = JsonSerializer.Serialize(updateInfo.Version)
            let updatedAtJson = JsonSerializer.Serialize(updateInfo.UpdatedAtUtc)

            appendLine builder $"  const currentArtifactVersion = {versionJson};"
            appendLine builder $"  const currentArtifactUpdatedAt = {updatedAtJson};"
            appendLine builder $"  const updatePollIntervalMs = {updateInfo.PollIntervalMs};"
            appendLine builder "  let pendingUpdateManifest = null;"
            appendLine builder "  let updatePollTimer = 0;"
            appendLine builder "  let updateCheckInFlight = false;"
            appendLine builder "  const buildUpdateManifestUrl = () => {"
            appendLine builder "    const pathName = window.location.pathname;"
            appendLine builder "    const manifestPath = /\\.html?$/i.test(pathName) ? pathName.replace(/\\.html?$/i, '.update.js') : `${pathName}.update.js`;"
            appendLine builder "    return `${manifestPath}?ts=${Date.now()}`;"
            appendLine builder "  };"
            appendLine builder "  const loadUpdateManifest = () => {"
            appendLine builder "    return new Promise((resolve, reject) => {"
            appendLine builder "      delete window.__llPathUpdateManifest;"
            appendLine builder "      const script = document.createElement('script');"
            appendLine builder "      script.async = true;"
            appendLine builder "      script.src = buildUpdateManifestUrl();"
            appendLine builder "      script.onload = () => {"
            appendLine builder "        const manifest = window.__llPathUpdateManifest || null;"
            appendLine builder "        script.remove();"
            appendLine builder "        resolve(manifest);"
            appendLine builder "      };"
            appendLine builder "      script.onerror = () => {"
            appendLine builder "        script.remove();"
            appendLine builder "        reject(new Error('update manifest unavailable'));"
            appendLine builder "      };"
            appendLine builder "      document.head.appendChild(script);"
            appendLine builder "    });"
            appendLine builder "  };"
            appendLine builder "  const maybeApplyPendingUpdate = () => {"
            appendLine builder "    if (!pendingUpdateManifest) { return; }"
            appendLine builder "    if (readStoredUpdateMode() === 'auto') {"
            appendLine builder "      setUpdateStatus(`Update detected (${pendingUpdateManifest.updatedAt || 'new build'}) — refreshing…`, 'auto');"
            appendLine builder "      window.location.reload();"
            appendLine builder "    }"
            appendLine builder "  };"
            appendLine builder "  const checkForArtifactUpdate = async () => {"
            appendLine builder "    if (updateCheckInFlight) { return; }"
            appendLine builder "    updateCheckInFlight = true;"
            appendLine builder "    try {"
            appendLine builder "      const manifest = await loadUpdateManifest();"
            appendLine builder "      if (!manifest || !manifest.version) {"
            appendLine builder "        setUpdateStatus('Update monitor unavailable for this artifact.', 'error');"
            appendLine builder "        return;"
            appendLine builder "      }"
            appendLine builder "      if (manifest.version !== currentArtifactVersion) {"
            appendLine builder "        pendingUpdateManifest = manifest;"
            appendLine builder "        setRefreshPending(true);"
            appendLine builder "        setUpdateStatus(`Update available (${manifest.updatedAt || 'new build'}).`, 'available');"
            appendLine builder "        maybeApplyPendingUpdate();"
            appendLine builder "      } else if (!pendingUpdateManifest) {"
            appendLine builder "        setRefreshPending(false);"
            appendLine builder "        setUpdateStatus(`Up to date (${currentArtifactUpdatedAt}).`, null);"
            appendLine builder "      }"
            appendLine builder "    } catch (_) {"
            appendLine builder "      setUpdateStatus('Update monitor unavailable for this artifact.', 'error');"
            appendLine builder "    } finally {"
            appendLine builder "      updateCheckInFlight = false;"
            appendLine builder "    }"
            appendLine builder "  };"
            appendLine builder "  if (refreshButton) {"
            appendLine builder "    refreshButton.addEventListener('click', () => window.location.reload());"
            appendLine builder "  }"
            appendLine builder "  updateModeButtons.forEach((button) => {"
            appendLine builder "    button.addEventListener('click', maybeApplyPendingUpdate);"
            appendLine builder "  });"
            appendLine builder "  setRefreshPending(false);"
            appendLine builder "  setUpdateStatus(`Up to date (${currentArtifactUpdatedAt}).`, null);"
            appendLine builder "  checkForArtifactUpdate();"
            appendLine builder "  updatePollTimer = window.setInterval(checkForArtifactUpdate, updatePollIntervalMs);"
        | None ->
            appendLine builder "  setRefreshPending(false);"
            appendLine builder "  setUpdateStatus('Live update monitor disabled for this artifact.', null);"

        appendLine builder "})();"
        appendLine builder "</script>"

    let renderUpdateManifestScript (updateState: ScreenPathUpdateState) =
        let builder = StringBuilder()

        appendLine builder $"window.__llPathUpdateManifest = {{ version: {jsonString updateState.Version}, updatedAt: {jsonString updateState.UpdatedAtUtc} }};"
        builder.ToString()

    /// Renders a self-contained HTML document for one current screen-path lens.
    let renderDocumentWithUpdateState (screenPathState: ScreenPathState) (updateState: ScreenPathUpdateState option) =
        let builder = StringBuilder()

        appendLine builder "<!DOCTYPE html>"
        appendLine builder "<html lang=\"en\">"
        appendLine builder "<head>"
        appendLine builder "<meta charset=\"utf-8\">"
        appendLine builder "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
        appendLine builder $"<title>{htmlEncode screenPathState.Title}</title>"
        appendLine builder (ScreenHtmlRenderer.renderStyleBlock ())
        renderPathStyles builder
        appendLine builder "</head>"
        appendLine builder "<body>"
        appendLine builder "<main class=\"ll-path-document\" data-view-mode=\"standard\" data-testid=\"screen-path-document\">"
        appendLine builder "<header class=\"ll-path-document__header\">"
        appendLine builder "<div class=\"ll-path-document__topline\">"
        appendLine builder "<div class=\"ll-path-document__title-zone\">"
        appendLine builder $"<div class=\"ll-path-document__eyebrow\">{htmlEncode screenPathState.PathId} screen path</div>"
        appendLine builder $"<h1 class=\"ll-path-document__title\">{htmlEncode screenPathState.Title}</h1>"
        appendLine builder "<details class=\"ll-path-document__context\" data-testid=\"path-scenario\">"
        appendLine builder "<summary data-testid=\"path-scenario-summary\">"
        appendLine builder "<span class=\"ll-path-document__context-label\">Scenario</span>"
        appendLine builder $"<span class=\"ll-path-document__context-title\">{htmlEncode screenPathState.ScenarioLabel}</span>"
        appendLine builder "</summary>"
        appendLine builder "<div class=\"ll-path-document__context-panel\" data-testid=\"path-scenario-panel\">"
        appendLine builder $"<p class=\"ll-path-document__description\">{htmlEncode screenPathState.Description}</p>"
        appendLine builder "<ul class=\"ll-path-document__assumptions\">"

        screenPathState.Assumptions
        |> List.iter (fun assumption ->
            appendLine builder $"<li>{htmlEncode assumption}</li>")

        appendLine builder "</ul>"
        appendLine builder "</div>"
        appendLine builder "</details>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"ll-path-document__header-controls\">"
        appendLine builder "<section class=\"ll-path-document__view-controls\" aria-label=\"Path viewer controls\">"
        appendLine builder "<div class=\"ll-path-document__view-label\">View</div>"
        appendLine builder "<div class=\"ll-path-document__view-buttons\">"
        appendLine builder "<button class=\"ll-path-view-toggle\" type=\"button\" data-testid=\"path-view-toggle\" data-view-mode=\"summary\" aria-pressed=\"false\">Summary</button>"
        appendLine builder "<button class=\"ll-path-view-toggle\" type=\"button\" data-testid=\"path-view-toggle\" data-view-mode=\"standard\" aria-pressed=\"true\">Standard</button>"
        appendLine builder "<button class=\"ll-path-view-toggle\" type=\"button\" data-testid=\"path-view-toggle\" data-view-mode=\"detailed\" aria-pressed=\"false\">Detailed</button>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"ll-path-document__lens-controls\" aria-label=\"Lens visibility controls\">"
        appendLine builder "<div class=\"ll-path-document__view-label\">Lenses</div>"
        appendLine builder "<div class=\"ll-path-document__lens-buttons\">"

        [ ApplicationLifecycleLens; AppRuntimeLens; ScreenPathLens ]
        |> List.iter (fun lensKind ->
            let lensKey = lensKindDomKey lensKind
            let lensLabel = lensKindButtonLabel lensKind
            appendLine builder $"<button class=\"ll-path-lens-toggle\" type=\"button\" data-testid=\"path-lens-toggle\" data-lens-key=\"{htmlEncode lensKey}\" aria-pressed=\"true\">{htmlEncode lensLabel}</button>")

        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"ll-path-document__update-controls\" aria-label=\"Path update controls\">"
        appendLine builder "<div class=\"ll-path-document__update-label\">Updates</div>"
        appendLine builder "<div class=\"ll-path-document__update-buttons\">"
        appendLine builder "<button class=\"ll-path-update-toggle\" type=\"button\" data-testid=\"path-update-toggle\" data-update-mode=\"notify\" aria-pressed=\"true\">Notify Me</button>"
        appendLine builder "<button class=\"ll-path-update-toggle\" type=\"button\" data-testid=\"path-update-toggle\" data-update-mode=\"auto\" aria-pressed=\"false\">Auto Refresh</button>"
        appendLine builder "<button id=\"ll-path-refresh-now\" class=\"ll-path-update-refresh\" data-testid=\"path-update-refresh\" type=\"button\" hidden>Refresh Now</button>"
        appendLine builder "</div>"
        appendLine builder "<div id=\"ll-path-update-status\" class=\"ll-path-document__update-status\" aria-live=\"polite\"></div>"
        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</header>"
        appendLine builder "<section class=\"ll-path-scroll-controls\">"
        appendLine builder "<button id=\"ll-path-nav-start\" class=\"ll-path-nav-button\" data-testid=\"path-nav-start\" type=\"button\" aria-label=\"Back to beginning\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--left\"></span></span></button>"
        appendLine builder "<button id=\"ll-path-nav-previous\" class=\"ll-path-nav-button\" data-testid=\"path-nav-previous\" type=\"button\" aria-label=\"Show the previous screen column\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--left\"></span></span></button>"
        appendLine builder "<div id=\"ll-path-scrollbar\" class=\"ll-path-scrollbar\" data-testid=\"path-scrollbar\" aria-label=\"Screen path horizontal scroll rail\">"
        appendLine builder "<div id=\"ll-path-scrollbar-content\" class=\"ll-path-scrollbar__content\" data-testid=\"path-scrollbar-content\"></div>"
        appendLine builder "</div>"
        appendLine builder "<button id=\"ll-path-nav-next\" class=\"ll-path-nav-button\" data-testid=\"path-nav-next\" type=\"button\" aria-label=\"Show the next screen column\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--right\"></span><span class=\"ll-path-nav-button__bar\"></span></span></button>"
        appendLine builder "<button id=\"ll-path-nav-end\" class=\"ll-path-nav-button\" data-testid=\"path-nav-end\" type=\"button\" aria-label=\"Go to end\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--right\"></span><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__bar\"></span></span></button>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"ll-path-stage\">"
        appendLine builder "<div id=\"ll-path-flow-viewport\" class=\"ll-path-flow-viewport\" data-testid=\"path-flow-viewport\">"
        appendLine builder "<section id=\"ll-path-flow\" class=\"ll-path-flow\" data-testid=\"path-flow\">"

        screenPathState.Steps
        |> List.iteri (fun index stepState ->
            let stepNumber = index + 1
            let stepNumberText = stepNumber.ToString("00")
            let stepLensKey = lensKindDomKey stepState.LensKind

            appendLine builder $"<section class=\"ll-path-step\" data-testid=\"path-step\" data-step-key=\"{htmlEncode stepState.StepKey}\" data-lens-key=\"{htmlEncode stepLensKey}\">"
            appendLine builder $"<div class=\"ll-path-step__eyebrow\">Step {stepNumberText} · {htmlEncode stepState.StepKey}</div>"
            appendLine builder $"<h2 class=\"ll-path-step__title\">{htmlEncode stepState.StepTitle}</h2>"

            match stepState.StepNote with
            | Some noteText -> appendLine builder $"<p class=\"ll-path-step__note\">{htmlEncode noteText}</p>"
            | None -> appendLine builder "<p class=\"ll-path-step__note\"></p>"

            appendLine builder "<div class=\"ll-path-step__meta\">"
            appendLine builder $"<div class=\"ll-path-step__context\">Context · {htmlEncode stepState.ContextLabel}</div>"
            appendLine builder $"<div class=\"ll-path-step__lens\">{htmlEncode stepState.LensLabel}</div>"
            appendLine builder "</div>"
            renderPathSurface builder stepState.Surface
            appendLine builder "</section>")

        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        renderPathScript builder screenPathState updateState
        appendLine builder "</main>"
        appendLine builder "</body>"
        appendLine builder "</html>"

        builder.ToString()

    let renderDocument (screenPathState: ScreenPathState) =
        renderDocumentWithUpdateState screenPathState (Some(defaultUpdateState ()))
