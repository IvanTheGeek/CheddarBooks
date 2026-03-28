namespace CheddarBooks.LaundryLog.UI

open System
open System.Net
open System.Text

/// Identifies the current screen-surface examples carried by the HTML proving ground.
type ScreenSurfaceState =
    | NewSessionScreen of surfaceName: string * note: string option * NewSessionPrimitiveState
    | EntryFormScreen of surfaceName: string * note: string option * EntryFormPrimitiveState

[<RequireQualifiedAccess>]
module ScreenSurfaceState =
    /// Returns the stable visible title for a screen surface.
    let title =
        function
        | NewSessionScreen (surfaceName, _, _) -> surfaceName
        | EntryFormScreen (surfaceName, _, _) -> surfaceName

    /// Returns the current optional screen note.
    let note =
        function
        | NewSessionScreen (_, note, _) -> note
        | EntryFormScreen (_, note, _) -> note

/// Carries the first deterministic LaundryLog screen-renderer examples.
[<RequireQualifiedAccess>]
module ScreenHtmlExamples =
    /// Returns the first concrete screen set used by the current HTML screen proving ground.
    let laundryLogBaseScreens () : ScreenSurfaceState list =
        [ NewSessionScreen
              ( "Screen.NewSession - Awaiting Location",
                Some "manual location entry before the first expense",
                PrimitiveStateExamples.newSessionAwaitingLocation () )
          NewSessionScreen
              ( "Screen.NewSession - Ready To Set",
                Some "manual location entered and ready to confirm",
                PrimitiveStateExamples.newSessionLocationEntered () )
          EntryFormScreen
              ( "Screen.EntryForm - Washer Draft",
                Some "first expense draft inside the current location context",
                PrimitiveStateExamples.entryFormWasherCardDraft () )
          EntryFormScreen
              ( "Screen.EntryForm - Card Details Expanded",
                Some "card selected and the additional payment section is disclosed",
                PrimitiveStateExamples.entryFormCardDetailsExpanded () )
          EntryFormScreen
              ( "Screen.EntryForm - Logged Success",
                Some "entry logged and the surface is ready for the next quick entry",
                PrimitiveStateExamples.entryFormLoggedSuccess () ) ]

/// Renders deterministic HTML/CSS LaundryLog screens from the current primitive-state examples.
[<RequireQualifiedAccess>]
module ScreenHtmlRenderer =
    let private htmlEncode (value: string) = WebUtility.HtmlEncode value

    let private appendLine (builder: StringBuilder) (value: string) =
        builder.AppendLine(value) |> ignore

    let private renderActionButton (builder: StringBuilder) (buttonState: ActionButtonState) =
        let emphasisCssClass =
            match buttonState.Emphasis with
            | Primary -> "ll-button--primary"
            | Secondary -> "ll-button--secondary"
            | Supporting -> "ll-button--supporting"
            | Success -> "ll-button--success"

        let disabledAttribute = if buttonState.IsEnabled then "" else " disabled"
        appendLine
            builder
            $"<button type=\"button\" class=\"ll-button {emphasisCssClass}\" data-control-id=\"{PrimitiveControlId.value buttonState.ControlId}\"{disabledAttribute}>{htmlEncode buttonState.Label}</button>"

    let private renderHeaderBar (builder: StringBuilder) (headerState: HeaderBarState) =
        appendLine builder "<header class=\"ll-header\">"
        appendLine builder "<div class=\"ll-header__topline\">"
        appendLine builder "<span class=\"ll-header__brand\">LaundryLog</span>"

        match headerState.BadgeText with
        | Some badgeText ->
            appendLine builder $"<span class=\"ll-header__badge\">{htmlEncode badgeText}</span>"
        | None -> ()

        appendLine builder "</div>"
        appendLine builder $"<h2 class=\"ll-header__title\">{htmlEncode headerState.Title}</h2>"

        match headerState.Subtitle with
        | Some subtitle ->
            appendLine builder $"<p class=\"ll-header__subtitle\">{htmlEncode subtitle}</p>"
        | None -> ()

        appendLine builder "</header>"

    let private renderStatusChip (builder: StringBuilder) (chipState: StatusChipState) =
        let toneCssClass =
            match chipState.Tone with
            | Ready -> " ll-status-chip--ready"
            | NeedsAttention -> " ll-status-chip--needs-attention"

        let markText =
            match chipState.Tone with
            | Ready -> "✓"
            | NeedsAttention -> "✗"

        appendLine
            builder
            $"<div class=\"ll-status-chip{toneCssClass}\" data-control-id=\"{PrimitiveControlId.value chipState.ControlId}\"><span class=\"ll-status-chip__icon\">{htmlEncode chipState.IconText}</span><span class=\"ll-status-chip__label\">{htmlEncode chipState.LabelText}</span><span class=\"ll-status-chip__mark\">{markText}</span></div>"

    let private renderStatusChipRow (builder: StringBuilder) (chipStates: StatusChipState list) =
        if not (List.isEmpty chipStates) then
            appendLine builder "<div class=\"ll-status-chip-row\">"
            chipStates |> List.iter (renderStatusChip builder)
            appendLine builder "</div>"

    let private renderPanelStart (builder: StringBuilder) title =
        appendLine builder $"<section class=\"ll-panel\"><h3 class=\"ll-panel__title\">{htmlEncode title}</h3>"

    let private renderPanelEnd (builder: StringBuilder) =
        appendLine builder "</section>"

    let private renderTextInput (builder: StringBuilder) label (inputState: TextInputState) =
        let visibleValue =
            inputState.ValueText
            |> Option.defaultValue inputState.PlaceholderText
            |> htmlEncode

        let readOnlyAttribute = if inputState.IsReadOnly then " readonly" else ""
        appendLine builder "<div class=\"ll-field\">"
        appendLine builder $"<label class=\"ll-field__label\" for=\"{PrimitiveControlId.value inputState.ControlId}\">{htmlEncode label}</label>"
        appendLine
            builder
            $"<input id=\"{PrimitiveControlId.value inputState.ControlId}\" class=\"ll-text-input\" type=\"text\" value=\"{visibleValue}\" placeholder=\"{htmlEncode inputState.PlaceholderText}\"{readOnlyAttribute}>"
        appendLine builder "</div>"

    let private renderOptionGroup (builder: StringBuilder) label (optionGroup: OptionGroupState) =
        appendLine builder "<div class=\"ll-field\">"

        match label, optionGroup.Label with
        | Some explicitLabel, _ ->
            appendLine builder $"<div class=\"ll-field__label\">{htmlEncode explicitLabel}</div>"
        | None, Some groupLabel ->
            appendLine builder $"<div class=\"ll-field__label\">{htmlEncode groupLabel}</div>"
        | None, None -> ()

        appendLine builder "<div class=\"ll-option-group\">"

        optionGroup.Choices
        |> List.iter (fun choice ->
            let selectedCssClass = if choice.IsSelected then " ll-chip--selected" else ""
            let disabledAttribute = if choice.IsEnabled then "" else " disabled"

            appendLine
                builder
                $"<button type=\"button\" class=\"ll-chip{selectedCssClass}\" data-choice-id=\"{PrimitiveControlId.value choice.ChoiceId}\"{disabledAttribute}>{htmlEncode choice.Label}</button>")

        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderStepper (builder: StringBuilder) label (stepperState: StepperState) =
        let decrementDisabled = if stepperState.CanDecrement then "" else " disabled"
        let incrementDisabled = if stepperState.CanIncrement then "" else " disabled"
        appendLine builder "<div class=\"ll-field\">"
        appendLine builder $"<div class=\"ll-field__label\">{htmlEncode label}</div>"
        appendLine builder "<div class=\"ll-stepper\">"
        appendLine
            builder
            $"<button type=\"button\" class=\"ll-stepper__button\" data-control-id=\"{PrimitiveControlId.value stepperState.ControlId}-decrement\"{decrementDisabled}>{htmlEncode stepperState.DecrementLabel}</button>"
        appendLine builder $"<div class=\"ll-stepper__value\">{htmlEncode stepperState.ValueText}</div>"
        appendLine
            builder
            $"<button type=\"button\" class=\"ll-stepper__button\" data-control-id=\"{PrimitiveControlId.value stepperState.ControlId}-increment\"{incrementDisabled}>{htmlEncode stepperState.IncrementLabel}</button>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderQuarterAdjustButton (builder: StringBuilder) (buttonState: QuarterAdjustButtonState) =
        let signText =
            match buttonState.Direction with
            | Decrease -> "−"
            | Increase -> "+"

        let disabledAttribute = if buttonState.IsEnabled then "" else " disabled"
        let directionCssClass =
            match buttonState.Direction with
            | Decrease -> " ll-quarter-button--decrease"
            | Increase -> " ll-quarter-button--increase"

        appendLine
            builder
            $"<button type=\"button\" class=\"ll-quarter-button{directionCssClass}\" data-control-id=\"{PrimitiveControlId.value buttonState.ControlId}\" aria-label=\"Adjust by {signText}{htmlEncode buttonState.AmountText}\"{disabledAttribute}><span class=\"ll-quarter-button__sign\">{signText}</span><span class=\"ll-quarter-button__coin\">{htmlEncode buttonState.AmountText}</span></button>"

    let private renderMoneyInput (builder: StringBuilder) label (moneyInputState: MoneyInputState) =
        let visibleValue =
            moneyInputState.ValueText
            |> Option.defaultValue moneyInputState.PlaceholderText
            |> htmlEncode

        appendLine builder "<div class=\"ll-field\">"
        appendLine builder $"<div class=\"ll-field__label\">{htmlEncode label}</div>"
        appendLine builder "<div class=\"ll-money-input\">"
        appendLine builder $"<span class=\"ll-money-input__currency\">{htmlEncode moneyInputState.CurrencySymbol}</span>"
        appendLine
            builder
            $"<input class=\"ll-money-input__field\" type=\"text\" data-control-id=\"{PrimitiveControlId.value moneyInputState.ControlId}\" value=\"{visibleValue}\" placeholder=\"{htmlEncode moneyInputState.PlaceholderText}\">"
        appendLine builder "</div>"

        if not (List.isEmpty moneyInputState.QuarterAdjustButtons) then
            appendLine builder "<div class=\"ll-quarter-row\">"
            moneyInputState.QuarterAdjustButtons |> List.iter (renderQuarterAdjustButton builder)
            appendLine builder "</div>"

        if not (List.isEmpty moneyInputState.QuickFillLabels) then
            appendLine builder "<div class=\"ll-quick-fill-row\">"

            moneyInputState.QuickFillLabels
            |> List.iter (fun quickFillLabel ->
                appendLine builder $"<button type=\"button\" class=\"ll-chip ll-chip--quick-fill\">{htmlEncode quickFillLabel}</button>")

            appendLine builder "</div>"

        appendLine builder "</div>"

    let private renderSummaryBar (builder: StringBuilder) (summaryBarState: SummaryBarState) =
        appendLine builder "<div class=\"ll-summary-bar\">"
        appendLine builder $"<span class=\"ll-summary-bar__label\">{htmlEncode summaryBarState.Label}</span>"
        appendLine builder $"<span class=\"ll-summary-bar__value\">{htmlEncode summaryBarState.ValueText}</span>"
        appendLine builder "</div>"

    let private renderFeedbackBanner (builder: StringBuilder) (feedbackBannerState: FeedbackBannerState) =
        appendLine
            builder
            $"<div class=\"ll-feedback-banner\" data-control-id=\"{PrimitiveControlId.value feedbackBannerState.ControlId}\">{htmlEncode feedbackBannerState.MessageText}</div>"

    let private renderEntryCard (builder: StringBuilder) (entryCardState: EntryCardState) =
        appendLine builder "<article class=\"ll-entry-card\">"
        appendLine builder "<div class=\"ll-entry-card__topline\">"
        appendLine builder $"<h4 class=\"ll-entry-card__title\">{htmlEncode entryCardState.TitleText}</h4>"

        match entryCardState.AmountText with
        | Some amountText -> appendLine builder $"<span class=\"ll-entry-card__amount\">{htmlEncode amountText}</span>"
        | None -> ()

        appendLine builder "</div>"

        match entryCardState.DetailText with
        | Some detailText -> appendLine builder $"<p class=\"ll-entry-card__detail\">{htmlEncode detailText}</p>"
        | None -> ()

        appendLine builder "</article>"

    let private renderNewSessionScreen (builder: StringBuilder) surfaceName note (screenState: NewSessionPrimitiveState) =
        appendLine builder "<article class=\"ll-screen-surface\">"
        appendLine builder $"<div class=\"ll-screen-surface__name\">{htmlEncode surfaceName}</div>"

        match note with
        | Some noteText -> appendLine builder $"<p class=\"ll-screen-surface__note\">{htmlEncode noteText}</p>"
        | None -> ()

        appendLine builder "<section class=\"ll-phone-screen\">"
        renderHeaderBar builder screenState.Header
        renderPanelStart builder "Location"
        renderTextInput builder "Location" screenState.LocationInput
        appendLine builder "<div class=\"ll-action-stack\">"
        renderActionButton builder screenState.GpsAction
        renderActionButton builder screenState.SetLocationAction
        appendLine builder "</div>"
        renderPanelEnd builder
        appendLine builder "</section>"
        appendLine builder "</article>"

    let private renderEntryFormScreen (builder: StringBuilder) surfaceName note (screenState: EntryFormPrimitiveState) =
        appendLine builder "<article class=\"ll-screen-surface\">"
        appendLine builder $"<div class=\"ll-screen-surface__name\">{htmlEncode surfaceName}</div>"

        match note with
        | Some noteText -> appendLine builder $"<p class=\"ll-screen-surface__note\">{htmlEncode noteText}</p>"
        | None -> ()

        appendLine builder "<section class=\"ll-phone-screen ll-phone-screen--tall\">"
        renderHeaderBar builder screenState.Header
        renderStatusChipRow builder screenState.StatusChips

        renderPanelStart builder "Machine Type"
        renderOptionGroup builder None screenState.MachineTypeOptions
        renderPanelEnd builder

        appendLine builder "<div class=\"ll-two-up\">"
        renderPanelStart builder "Quantity"
        renderStepper builder "Quantity" screenState.QuantityStepper
        renderPanelEnd builder

        renderPanelStart builder "Unit Price"
        renderMoneyInput builder "Unit Price" screenState.PriceInput
        renderPanelEnd builder
        appendLine builder "</div>"

        renderPanelStart builder "Payment"
        renderOptionGroup builder None screenState.PaymentOptions

        match screenState.PaymentDetailOptions with
        | Some paymentDetailOptions ->
            renderOptionGroup builder None paymentDetailOptions
        | None -> ()

        renderPanelEnd builder

        match screenState.FeedbackBanner with
        | Some feedbackBanner -> renderFeedbackBanner builder feedbackBanner
        | None -> ()

        appendLine builder "<div class=\"ll-primary-action-row\">"
        renderActionButton builder screenState.SubmitAction
        appendLine builder "</div>"
        renderSummaryBar builder screenState.SessionTotal

        renderPanelStart builder "Recent Entries"

        if List.isEmpty screenState.RecentEntries then
            appendLine builder "<p class=\"ll-empty-state\">No entries yet in this session.</p>"
        else
            appendLine builder "<div class=\"ll-entry-list\">"
            screenState.RecentEntries |> List.iter (renderEntryCard builder)
            appendLine builder "</div>"

        renderPanelEnd builder
        appendLine builder "</section>"
        appendLine builder "</article>"

    let private renderScreenSurface (builder: StringBuilder) =
        function
        | NewSessionScreen (surfaceName, note, screenState) ->
            renderNewSessionScreen builder surfaceName note screenState
        | EntryFormScreen (surfaceName, note, screenState) ->
            renderEntryFormScreen builder surfaceName note screenState

    let private renderStyles (builder: StringBuilder) =
        appendLine builder "<style>"
        appendLine builder ":root { color-scheme: light; }"
        appendLine builder "body { margin: 0; background: linear-gradient(180deg, #f3f5f8 0%, #e9edf3 100%); color: #0d2440; font-family: \"IBM Plex Sans\", \"Aptos\", \"Segoe UI\", sans-serif; }"
        appendLine builder ".ll-document { padding: 14px 16px 22px; }"
        appendLine builder ".ll-document__header { margin-bottom: 14px; }"
        appendLine builder ".ll-document__title { margin: 0; font-size: 1.15rem; line-height: 1.06; }"
        appendLine builder ".ll-document__description { margin: 4px 0 0; color: #48627f; font-size: 0.82rem; line-height: 1.3; }"
        appendLine builder ".ll-screen-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 360px)); gap: 18px; align-items: start; }"
        appendLine builder ".ll-screen-surface { display: flex; flex-direction: column; gap: 8px; }"
        appendLine builder ".ll-screen-surface__name { font-size: 0.72rem; font-weight: 700; letter-spacing: 0.06em; color: #0e5883; text-transform: uppercase; }"
        appendLine builder ".ll-screen-surface__note { margin: 0; color: #58718b; font-size: 0.72rem; line-height: 1.25; }"
        appendLine builder ".ll-phone-screen { width: 100%; max-width: 375px; min-height: 667px; box-sizing: border-box; border-radius: 28px; border: 4px solid #15263d; background: linear-gradient(180deg, #fcfdff 0%, #f6f9fd 100%); box-shadow: 0 16px 32px rgba(10, 27, 49, 0.12); padding: 14px; display: flex; flex-direction: column; gap: 12px; }"
        appendLine builder ".ll-phone-screen--tall { min-height: 1020px; }"
        appendLine builder ".ll-header { display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".ll-header__topline { display: flex; justify-content: space-between; align-items: center; gap: 8px; }"
        appendLine builder ".ll-header__brand { display: inline-flex; align-items: center; justify-content: center; padding: 4px 10px; border-radius: 999px; background: #ffb54d; color: white; font-size: 0.66rem; font-weight: 700; line-height: 1; }"
        appendLine builder ".ll-header__badge { display: inline-flex; align-items: center; justify-content: center; padding: 3px 8px; border-radius: 999px; background: rgba(255,255,255,0.92); border: 1px solid #c6d4e2; color: #566d86; font-size: 0.62rem; font-weight: 700; }"
        appendLine builder ".ll-header__title { margin: 0; font-size: 1.18rem; line-height: 1.08; }"
        appendLine builder ".ll-header__subtitle { margin: 0; color: #47617c; font-size: 0.82rem; line-height: 1.25; }"
        appendLine builder ".ll-status-chip-row { display: flex; flex-wrap: wrap; gap: 8px; }"
        appendLine builder ".ll-status-chip { display: inline-flex; align-items: center; gap: 6px; min-height: 34px; padding: 0 10px; border-radius: 999px; background: #718096; color: rgba(255,255,255,0.96); font-size: 0.72rem; font-weight: 700; box-shadow: inset 0 0 0 1px rgba(255,255,255,0.12); }"
        appendLine builder ".ll-status-chip__label { color: rgba(255,255,255,0.96); }"
        appendLine builder ".ll-status-chip__mark { font-weight: 800; }"
        appendLine builder ".ll-status-chip--ready .ll-status-chip__mark { color: #dcfce7; }"
        appendLine builder ".ll-status-chip--needs-attention .ll-status-chip__mark { color: #fecaca; }"
        appendLine builder ".ll-panel { display: flex; flex-direction: column; gap: 9px; background: rgba(255,255,255,0.78); border: 1px solid #d7e3f0; border-radius: 18px; padding: 12px; }"
        appendLine builder ".ll-panel__title { margin: 0; font-size: 0.78rem; font-weight: 700; letter-spacing: 0.04em; text-transform: uppercase; color: #27435c; }"
        appendLine builder ".ll-field { display: flex; flex-direction: column; gap: 6px; }"
        appendLine builder ".ll-field__label { color: #47617c; font-size: 0.72rem; font-weight: 700; }"
        appendLine builder ".ll-text-input, .ll-money-input__field { width: 100%; box-sizing: border-box; border: 1px solid #c6d7ea; border-radius: 14px; padding: 12px 13px; background: #f7fbff; color: #16304a; font-size: 0.94rem; line-height: 1.2; }"
        appendLine builder ".ll-text-input::placeholder, .ll-money-input__field::placeholder { color: #8aa0b7; }"
        appendLine builder ".ll-action-stack, .ll-primary-action-row { display: flex; flex-direction: column; gap: 8px; }"
        appendLine builder ".ll-button { width: 100%; border-radius: 16px; border: 1px solid transparent; padding: 12px 14px; font-size: 0.9rem; font-weight: 700; line-height: 1.15; cursor: pointer; }"
        appendLine builder ".ll-button:disabled { cursor: not-allowed; opacity: 0.58; }"
        appendLine builder ".ll-button--primary { background: #0ea5e9; border-color: #0284c7; color: white; }"
        appendLine builder ".ll-button--secondary { background: #eff6ff; border-color: #bfdbfe; color: #1d4ed8; }"
        appendLine builder ".ll-button--supporting { background: rgba(255,255,255,0.92); border-color: #c6d4e2; color: #27435c; }"
        appendLine builder ".ll-button--success { background: linear-gradient(180deg, #22c55e 0%, #16a34a 100%); border-color: #15803d; color: white; box-shadow: 0 10px 18px rgba(22, 163, 74, 0.22); }"
        appendLine builder ".ll-option-group { display: flex; flex-wrap: wrap; gap: 8px; }"
        appendLine builder ".ll-chip { border-radius: 999px; border: 1px solid #c6d4e2; background: rgba(255,255,255,0.94); color: #27435c; padding: 7px 12px; font-size: 0.78rem; font-weight: 700; cursor: pointer; }"
        appendLine builder ".ll-chip--selected { background: #0ea5e9; border-color: #0284c7; color: white; }"
        appendLine builder ".ll-chip--quick-fill { background: #fff7ed; border-color: #fdba74; color: #9a3412; }"
        appendLine builder ".ll-two-up { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }"
        appendLine builder ".ll-stepper { display: grid; grid-template-columns: 56px 1fr 56px; gap: 10px; align-items: center; }"
        appendLine builder ".ll-stepper__button { border-radius: 16px; border: 1px solid #c6d4e2; background: rgba(255,255,255,0.94); color: #27435c; min-height: 52px; font-size: 1.18rem; font-weight: 700; cursor: pointer; }"
        appendLine builder ".ll-stepper__value { border-radius: 16px; border: 1px solid #c6d7ea; background: #f7fbff; padding: 14px 12px; font-size: 1rem; font-weight: 700; text-align: center; }"
        appendLine builder ".ll-money-input { display: grid; grid-template-columns: auto 1fr; align-items: center; border: 1px solid #c6d7ea; border-radius: 14px; background: #f7fbff; overflow: hidden; }"
        appendLine builder ".ll-money-input__currency { padding: 0 0 0 12px; color: #47617c; font-size: 0.88rem; font-weight: 700; }"
        appendLine builder ".ll-money-input__field { border: 0; background: transparent; padding-left: 6px; }"
        appendLine builder ".ll-quarter-row { display: flex; align-items: center; gap: 10px; margin-top: 6px; }"
        appendLine builder ".ll-quarter-button { position: relative; display: inline-grid; place-items: center; width: 58px; height: 58px; border-radius: 50%; border: 1px solid #94a3b8; background: radial-gradient(circle at 30% 28%, #ffffff 0%, #f8fafc 28%, #d9e2ec 56%, #b8c5d1 74%, #eef2f7 100%); color: #1f3349; box-shadow: inset 0 2px 2px rgba(255,255,255,0.7), inset 0 -2px 2px rgba(71,85,105,0.18), 0 4px 8px rgba(15, 23, 42, 0.12); cursor: pointer; }"
        appendLine builder ".ll-quarter-button__sign { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); font-size: 1.02rem; font-weight: 800; }"
        appendLine builder ".ll-quarter-button__coin { font-size: 0.84rem; font-weight: 800; letter-spacing: 0.01em; }"
        appendLine builder ".ll-quarter-button:disabled { cursor: not-allowed; opacity: 0.52; }"
        appendLine builder ".ll-quick-fill-row { display: flex; flex-wrap: wrap; gap: 8px; }"
        appendLine builder ".ll-summary-bar { display: flex; justify-content: space-between; align-items: center; gap: 10px; border-radius: 18px; background: linear-gradient(90deg, #fff5eb 0%, #ffe5c5 100%); border: 1px solid #fdba74; padding: 11px 12px; }"
        appendLine builder ".ll-summary-bar__label { color: #9a3412; font-size: 0.76rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; }"
        appendLine builder ".ll-summary-bar__value { color: #7c2d12; font-size: 1rem; font-weight: 800; }"
        appendLine builder ".ll-feedback-banner { border-radius: 16px; background: linear-gradient(90deg, #ecfdf5 0%, #dcfce7 100%); border: 1px solid #86efac; color: #166534; padding: 11px 12px; font-size: 0.82rem; font-weight: 700; }"
        appendLine builder ".ll-entry-list { display: flex; flex-direction: column; gap: 8px; }"
        appendLine builder ".ll-entry-card { border-radius: 14px; border: 1px solid #d7e3f0; background: rgba(255,255,255,0.92); padding: 10px 11px; display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".ll-entry-card__topline { display: flex; justify-content: space-between; align-items: flex-start; gap: 8px; }"
        appendLine builder ".ll-entry-card__title { margin: 0; font-size: 0.83rem; line-height: 1.15; }"
        appendLine builder ".ll-entry-card__amount { color: #0e5883; font-size: 0.82rem; font-weight: 800; white-space: nowrap; }"
        appendLine builder ".ll-entry-card__detail { margin: 0; color: #4d6781; font-size: 0.72rem; line-height: 1.25; }"
        appendLine builder ".ll-empty-state { margin: 0; color: #6b7f95; font-size: 0.76rem; line-height: 1.25; }"
        appendLine builder "@media (max-width: 920px) { .ll-document { padding-left: 12px; padding-right: 12px; } .ll-screen-grid { grid-template-columns: 1fr; } .ll-two-up { grid-template-columns: 1fr; } }"
        appendLine builder "</style>"

    /// Renders a self-contained HTML document for the current LaundryLog screen proving ground.
    let renderDocument documentTitle description (screenSurfaces: ScreenSurfaceState list) =
        let builder = StringBuilder()

        appendLine builder "<!DOCTYPE html>"
        appendLine builder "<html lang=\"en\">"
        appendLine builder "<head>"
        appendLine builder "<meta charset=\"utf-8\">"
        appendLine builder "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
        appendLine builder $"<title>{htmlEncode documentTitle}</title>"
        renderStyles builder
        appendLine builder "</head>"
        appendLine builder "<body>"
        appendLine builder "<main class=\"ll-document\">"
        appendLine builder "<header class=\"ll-document__header\">"
        appendLine builder $"<h1 class=\"ll-document__title\">{htmlEncode documentTitle}</h1>"
        appendLine builder $"<p class=\"ll-document__description\">{htmlEncode description}</p>"
        appendLine builder "</header>"
        appendLine builder "<section class=\"ll-screen-grid\">"
        screenSurfaces |> List.iter (renderScreenSurface builder)
        appendLine builder "</section>"
        appendLine builder "</main>"
        appendLine builder "</body>"
        appendLine builder "</html>"

        builder.ToString()
