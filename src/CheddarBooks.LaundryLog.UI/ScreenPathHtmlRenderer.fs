namespace CheddarBooks.LaundryLog.UI

open System.Net
open System.Text

/// Carries the small system-facing boot surface used by the first screen-path proving ground.
type AppBootScreenState =
    { SurfaceName: string
      Note: string option
      Header: HeaderBarState
      PrimaryMessage: string
      SecondaryMessage: string
      BootChecks: string list }

/// Distinguishes the current screen-path surfaces.
type ScreenPathSurfaceState =
    | AppBootSurface of AppBootScreenState
    | AppScreenSurface of ScreenSurfaceState

/// Describes one ordered screen-path step.
type ScreenPathStepState =
    { StepKey: string
      StepTitle: string
      StepNote: string option
      LensLabel: string
      Surface: ScreenPathSurfaceState }

/// Describes one rendered screen path.
type ScreenPathState =
    { PathId: string
      Title: string
      Description: string
      Steps: ScreenPathStepState list }

[<RequireQualifiedAccess>]
module ScreenPathHtmlExamples =
    let private expect description result =
        match result with
        | Ok value -> value
        | Error message -> failwith $"Expected a valid {description}. {message}"

    let private appBootSurface () =
        { SurfaceName = "Screen.AppStart - Boot"
          Note = Some "fresh app launch before local/PWA startup checks complete"
          Header = HeaderBarState.tryCreate "LaundryLog" None None |> expect "app boot header"
          PrimaryMessage = "Starting LaundryLog"
          SecondaryMessage = "Checking local state, offline cache, and first-run setup."
          BootChecks =
            [ "AppStarted observed"
              "PWA/runtime checks running"
              "No known local session yet"
              "Route to Need Location when startup completes" ] }

    /// Returns the first app/screen-path lens for LaundryLog.
    let path1StartupToFirstEntry () : ScreenPathState =
        { PathId = "PATH1"
          Title = "PATH 1: App Started -> Need Location -> First Entry"
          Description =
            "Screen-path lens over the first LaundryLog flow. This is not only AEM business slices; it follows app/system and user-visible screen states in order."
          Steps =
            [ { StepKey = "01-app-started"
                StepTitle = "AppStarted"
                StepNote = Some "User taps the app icon and the app/runtime boot sequence begins."
                LensLabel = "app/system lens"
                Surface = appBootSurface () |> AppBootSurface }
              { StepKey = "02-need-location"
                StepTitle = "Need Location"
                StepNote = Some "Fresh start with no known location yet."
                LensLabel = "screen path lens"
                Surface =
                    NewSessionScreen
                        ( "Screen.NewSession - Awaiting Location",
                          Some "manual location entry before the first expense",
                          PrimitiveStateExamples.newSessionAwaitingLocation () )
                    |> AppScreenSurface }
              { StepKey = "03-ready-to-set-location"
                StepTitle = "Ready To Set Location"
                StepNote = Some "The location text is entered and the confirm action is available."
                LensLabel = "screen path lens"
                Surface =
                    NewSessionScreen
                        ( "Screen.NewSession - Ready To Set",
                          Some "manual location entered and ready to confirm",
                          PrimitiveStateExamples.newSessionLocationEntered () )
                    |> AppScreenSurface }
              { StepKey = "04-entry-form-ready"
                StepTitle = "Entry Form Ready"
                StepNote = Some "The app enters the main entry surface after location capture."
                LensLabel = "screen path lens"
                Surface =
                    EntryFormScreen
                        ( "Screen.EntryForm - Ready At Location",
                          Some "location captured and the expense form is ready for the first selection",
                          PrimitiveStateExamples.entryFormReadyAtLocation () )
                    |> AppScreenSurface }
              { StepKey = "05-washer-draft"
                StepTitle = "Washer Draft"
                StepNote = Some "The first expense draft is composed on the main screen."
                LensLabel = "screen path lens"
                Surface =
                    EntryFormScreen
                        ( "Screen.EntryForm - Washer Draft",
                          Some "first expense draft inside the current location context",
                          PrimitiveStateExamples.entryFormWasherCardDraft () )
                    |> AppScreenSurface }
              { StepKey = "06-logged-success"
                StepTitle = "Logged Success"
                StepNote = Some "The first entry is logged and the screen is ready for the next quick entry."
                LensLabel = "screen path lens"
                Surface =
                    EntryFormScreen
                        ( "Screen.EntryForm - Logged Success",
                          Some "entry logged and the surface is ready for the next quick entry",
                          PrimitiveStateExamples.entryFormLoggedSuccess () )
                    |> AppScreenSurface } ] }

/// Renders deterministic HTML/CSS screen-path documents from the current screen surfaces.
[<RequireQualifiedAccess>]
module ScreenPathHtmlRenderer =
    let private htmlEncode (value: string) = WebUtility.HtmlEncode value

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

    let private renderBootSurface (builder: StringBuilder) (bootState: AppBootScreenState) =
        appendLine builder "<article class=\"ll-screen-surface\">"
        appendLine builder $"<div class=\"ll-screen-surface__name\">{htmlEncode bootState.SurfaceName}</div>"

        match bootState.Note with
        | Some noteText -> appendLine builder $"<p class=\"ll-screen-surface__note\">{htmlEncode noteText}</p>"
        | None -> ()

        appendLine builder "<section class=\"ll-phone-screen\">"
        renderBootHeader builder bootState.Header
        appendLine builder "<div class=\"ll-screen-body\">"
        appendLine builder "<section class=\"ll-panel ll-panel--boot\">"
        appendLine builder "<div class=\"ll-boot-state\">"
        appendLine builder "<div class=\"ll-boot-state__icon\">🧺</div>"
        appendLine builder $"<h3 class=\"ll-boot-state__title\">{htmlEncode bootState.PrimaryMessage}</h3>"
        appendLine builder $"<p class=\"ll-boot-state__subtitle\">{htmlEncode bootState.SecondaryMessage}</p>"
        appendLine builder "<div class=\"ll-boot-state__checks\">"

        bootState.BootChecks
        |> List.iter (fun checkLine ->
            appendLine builder $"<div class=\"ll-boot-state__check\">{htmlEncode checkLine}</div>")

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
        appendLine builder ".ll-path-document__header { margin-bottom: 0; display: flex; flex-direction: column; gap: 3px; }"
        appendLine builder ".ll-path-document__eyebrow { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #0e5883; }"
        appendLine builder ".ll-path-document__title { margin: 0; font-size: 0.96rem; line-height: 1.06; font-weight: 700; }"
        appendLine builder ".ll-path-document__description { margin: 0; color: #64748b; font-size: 0.72rem; line-height: 1.28; max-width: 84ch; }"
        appendLine builder ".ll-path-scroll-controls { display: grid; grid-template-columns: auto auto minmax(0, 1fr) auto auto; gap: 10px; align-items: center; padding: 8px 4px 10px; background: linear-gradient(180deg, #ffffff 0%, rgba(255, 255, 255, 0.98) 72%, rgba(255, 255, 255, 0.92) 100%); }"
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
        appendLine builder ".ll-path-step__lens { align-self: flex-start; padding: 0.2rem 0.5rem; border-radius: 999px; background: #e2e8f0; color: #475569; font-size: 0.66rem; font-weight: 700; letter-spacing: 0.04em; text-transform: uppercase; }"
        appendLine builder ".ll-panel--boot { min-height: 520px; justify-content: center; }"
        appendLine builder ".ll-boot-state { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 0.85rem; padding: 2rem 1rem; }"
        appendLine builder ".ll-boot-state__icon { width: 88px; height: 88px; border-radius: 1.25rem; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); display: flex; align-items: center; justify-content: center; font-size: 2.6rem; box-shadow: 0 8px 20px rgba(255, 183, 77, 0.35); }"
        appendLine builder ".ll-boot-state__title { margin: 0; font-size: 1.15rem; font-weight: 700; color: #0f172a; }"
        appendLine builder ".ll-boot-state__subtitle { margin: 0; font-size: 0.82rem; line-height: 1.35; color: #64748b; max-width: 26ch; }"
        appendLine builder ".ll-boot-state__checks { width: 100%; display: flex; flex-direction: column; gap: 0.45rem; }"
        appendLine builder ".ll-boot-state__check { padding: 0.65rem 0.8rem; border-radius: 0.75rem; background: #f8fafc; border: 1px solid #e2e8f0; font-size: 0.76rem; font-weight: 600; color: #475569; }"
        appendLine builder "@media (max-width: 920px) { .ll-path-document { padding-left: 12px; padding-right: 12px; } .ll-path-flow { grid-auto-columns: minmax(320px, 88vw); } }"
        appendLine builder "</style>"

    let private renderPathScript (builder: StringBuilder) =
        appendLine builder "<script>"
        appendLine builder "(function () {"
        appendLine builder "  const scrollbar = document.getElementById('ll-path-scrollbar');"
        appendLine builder "  const scrollbarContent = document.getElementById('ll-path-scrollbar-content');"
        appendLine builder "  const viewport = document.getElementById('ll-path-flow-viewport');"
        appendLine builder "  const flow = document.getElementById('ll-path-flow');"
        appendLine builder "  const startButton = document.getElementById('ll-path-nav-start');"
        appendLine builder "  const previousButton = document.getElementById('ll-path-nav-previous');"
        appendLine builder "  const nextButton = document.getElementById('ll-path-nav-next');"
        appendLine builder "  const endButton = document.getElementById('ll-path-nav-end');"
        appendLine builder "  if (!scrollbar || !scrollbarContent || !viewport || !flow) { return; }"
        appendLine builder "  const baseFlowPaddingRight = parseFloat(window.getComputedStyle(flow).paddingRight) || 0;"
        appendLine builder "  let syncingScroll = false;"
        appendLine builder "  let animatingScroll = false;"
        appendLine builder "  let animationFrameId = 0;"
        appendLine builder "  let currentStepMetrics = { targets: [0], logicalMaxTarget: 0 };"
        appendLine builder "  const measureStepMetrics = () => {"
        appendLine builder "    const steps = Array.from(flow.querySelectorAll('.ll-path-step'));"
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
        appendLine builder "    scrollbarContent.style.width = `${Math.ceil(flow.getBoundingClientRect().width)}px`;"
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
        appendLine builder "  const scrollToStart = () => animateScrollToColumn(0);"
        appendLine builder "  const scrollToEnd = () => animateScrollToColumn(logicalMaxScrollLeft());"
        appendLine builder "  const scrollByOneColumn = (direction) => {"
        appendLine builder "    const targets = currentStepMetrics.targets;"
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
        appendLine builder "  window.addEventListener('resize', syncWidth);"
        appendLine builder "  if (window.ResizeObserver) {"
        appendLine builder "    new ResizeObserver(syncWidth).observe(flow);"
        appendLine builder "  }"
        appendLine builder "})();"
        appendLine builder "</script>"

    /// Renders a self-contained HTML document for one current screen-path lens.
    let renderDocument (screenPathState: ScreenPathState) =
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
        appendLine builder "<main class=\"ll-path-document\">"
        appendLine builder "<header class=\"ll-path-document__header\">"
        appendLine builder $"<div class=\"ll-path-document__eyebrow\">{htmlEncode screenPathState.PathId} screen path</div>"
        appendLine builder $"<h1 class=\"ll-path-document__title\">{htmlEncode screenPathState.Title}</h1>"
        appendLine builder $"<p class=\"ll-path-document__description\">{htmlEncode screenPathState.Description}</p>"
        appendLine builder "</header>"
        appendLine builder "<section class=\"ll-path-scroll-controls\">"
        appendLine builder "<button id=\"ll-path-nav-start\" class=\"ll-path-nav-button\" type=\"button\" aria-label=\"Back to beginning\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--left\"></span></span></button>"
        appendLine builder "<button id=\"ll-path-nav-previous\" class=\"ll-path-nav-button\" type=\"button\" aria-label=\"Show the previous screen column\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--left\"></span></span></button>"
        appendLine builder "<div id=\"ll-path-scrollbar\" class=\"ll-path-scrollbar\" aria-label=\"Screen path horizontal scroll rail\">"
        appendLine builder "<div id=\"ll-path-scrollbar-content\" class=\"ll-path-scrollbar__content\"></div>"
        appendLine builder "</div>"
        appendLine builder "<button id=\"ll-path-nav-next\" class=\"ll-path-nav-button\" type=\"button\" aria-label=\"Show the next screen column\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--right\"></span><span class=\"ll-path-nav-button__bar\"></span></span></button>"
        appendLine builder "<button id=\"ll-path-nav-end\" class=\"ll-path-nav-button\" type=\"button\" aria-label=\"Go to end\"><span class=\"ll-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"ll-path-nav-button__chevron ll-path-nav-button__chevron--right\"></span><span class=\"ll-path-nav-button__bar\"></span><span class=\"ll-path-nav-button__bar\"></span></span></button>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"ll-path-stage\">"
        appendLine builder "<div id=\"ll-path-flow-viewport\" class=\"ll-path-flow-viewport\">"
        appendLine builder "<section id=\"ll-path-flow\" class=\"ll-path-flow\">"

        screenPathState.Steps
        |> List.iteri (fun index stepState ->
            let stepNumber = index + 1
            let stepNumberText = stepNumber.ToString("00")
            appendLine builder "<section class=\"ll-path-step\">"
            appendLine builder $"<div class=\"ll-path-step__eyebrow\">Step {stepNumberText} · {htmlEncode stepState.StepKey}</div>"
            appendLine builder $"<h2 class=\"ll-path-step__title\">{htmlEncode stepState.StepTitle}</h2>"

            match stepState.StepNote with
            | Some noteText -> appendLine builder $"<p class=\"ll-path-step__note\">{htmlEncode noteText}</p>"
            | None -> appendLine builder "<p class=\"ll-path-step__note\"></p>"

            appendLine builder $"<div class=\"ll-path-step__lens\">{htmlEncode stepState.LensLabel}</div>"
            renderPathSurface builder stepState.Surface
            appendLine builder "</section>")

        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        renderPathScript builder
        appendLine builder "</main>"
        appendLine builder "</body>"
        appendLine builder "</html>"

        builder.ToString()
