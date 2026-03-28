namespace CheddarBooks.LaundryLog.UI

open System
open System.Globalization
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
        [ EntryFormScreen
              ( "Screen.EntryForm - v7 Primary",
                Some "recovered Claude-era mobile source of truth",
                PrimitiveStateExamples.entryFormV7PrimarySurface () )
          NewSessionScreen
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

    let private tryParseDecimal (value: string) =
        match Decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture) with
        | true, parsedValue -> Some parsedValue
        | false, _ -> None

    let private formatCurrency (amount: decimal) =
        amount.ToString("0.00", CultureInfo.InvariantCulture)

    let private optionIcon labelText =
        match labelText with
        | "Washer" -> Some "🌊"
        | "Dryer" -> Some "🔥"
        | "Supplies" -> Some "🧴"
        | "Cash" -> Some "💵"
        | "Points" -> Some "⭐"
        | "Card" -> Some "💳"
        | "App" -> Some "📱"
        | _ -> None

    let private splitQuickFillLabel (quickFillLabel: string) =
        let knownPrefixes =
            [ "Historical"
              "Last used"
              "Community" ]

        knownPrefixes
        |> List.tryPick (fun prefix ->
            let prefixWithSpace = prefix + " "

            if quickFillLabel.StartsWith(prefixWithSpace, StringComparison.Ordinal) then
                let amountText = quickFillLabel.Substring(prefixWithSpace.Length).Trim()
                Some(amountText, prefix)
            else
                None)
        |> Option.defaultValue (quickFillLabel, "")

    let private entryTotalText (screenState: EntryFormPrimitiveState) =
        let quantity =
            match Int32.TryParse(screenState.QuantityStepper.ValueText) with
            | true, parsedValue when parsedValue > 0 -> parsedValue
            | _ -> 1

        let unitPriceText =
            screenState.PriceInput.ValueText
            |> Option.defaultValue screenState.PriceInput.PlaceholderText

        let unitPriceAmount =
            tryParseDecimal unitPriceText
            |> Option.defaultValue 0.00m

        $"${formatCurrency (decimal quantity * unitPriceAmount)}"

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
        appendLine builder "<div class=\"ll-header__text\">"
        appendLine builder $"<h2 class=\"ll-header__title\">🧺 {htmlEncode headerState.Title}</h2>"
        appendLine builder "<p class=\"ll-header__subtitle\">by CheddarBooks</p>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"ll-header__badge\">🧀</div>"
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
        let groupId = PrimitiveControlId.value optionGroup.ControlId
        let groupCssClass, buttonCssClass =
            match groupId with
            | "machine-type" -> " ll-option-group--machine", " ll-chip--tile"
            | "payment-type" -> " ll-option-group--payment", " ll-chip--tile"
            | _ -> " ll-option-group--detail", " ll-chip--detail"

        appendLine builder "<div class=\"ll-field\">"

        match label, optionGroup.Label with
        | Some explicitLabel, _ ->
            appendLine builder $"<div class=\"ll-field__label\">{htmlEncode explicitLabel}</div>"
        | None, Some groupLabel ->
            appendLine builder $"<div class=\"ll-field__label\">{htmlEncode groupLabel}</div>"
        | None, None -> ()

        appendLine builder $"<div class=\"ll-option-group{groupCssClass}\">"

        optionGroup.Choices
        |> List.iter (fun choice ->
            let selectedCssClass = if choice.IsSelected then " ll-chip--selected" else ""
            let disabledAttribute = if choice.IsEnabled then "" else " disabled"

            let bodyHtml =
                match groupId with
                | "machine-type"
                | "payment-type" ->
                    let iconText = optionIcon choice.Label |> Option.defaultValue ""
                    $"<span class=\"ll-chip__icon\">{htmlEncode iconText}</span><span class=\"ll-chip__label\">{htmlEncode choice.Label}</span>"
                | _ ->
                    htmlEncode choice.Label

            appendLine
                builder
                $"<button type=\"button\" class=\"ll-chip{buttonCssClass}{selectedCssClass}\" data-choice-id=\"{PrimitiveControlId.value choice.ChoiceId}\"{disabledAttribute}>{bodyHtml}</button>")

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
                let amountText, labelText = splitQuickFillLabel quickFillLabel

                appendLine builder "<button type=\"button\" class=\"ll-chip ll-chip--quick-fill\">"
                appendLine builder $"<span class=\"ll-chip__amount\">{htmlEncode amountText}</span>"

                if not (String.IsNullOrWhiteSpace labelText) then
                    appendLine builder $"<span class=\"ll-chip__subtext\">{htmlEncode labelText}</span>"

                appendLine builder "</button>")

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

    let private renderValidationAction (builder: StringBuilder) (statusChips: StatusChipState list) (buttonState: ActionButtonState) =
        let emphasisCssClass =
            match buttonState.Emphasis with
            | Success -> " ll-button--success"
            | _ when buttonState.IsEnabled -> " ll-button--primary"
            | _ -> " ll-button--disabled"

        let disabledAttribute = if buttonState.IsEnabled then "" else " disabled"
        appendLine builder $"<button type=\"button\" class=\"ll-button ll-button--submit{emphasisCssClass}\" data-control-id=\"{PrimitiveControlId.value buttonState.ControlId}\"{disabledAttribute}>"

        if buttonState.IsEnabled || List.isEmpty statusChips then
            appendLine builder (htmlEncode buttonState.Label)
        else
            statusChips |> renderStatusChipRow builder

        appendLine builder "</button>"

    let private renderEntryCard (builder: StringBuilder) (entryCardState: EntryCardState) =
        appendLine builder "<article class=\"ll-entry-card\">"
        appendLine builder "<div class=\"ll-entry-card__topline\">"
        match entryCardState.AmountText with
        | Some amountText -> appendLine builder $"<span class=\"ll-entry-card__amount\">{htmlEncode amountText}</span>"
        | None -> appendLine builder $"<span class=\"ll-entry-card__amount\">{htmlEncode entryCardState.TitleText}</span>"

        appendLine builder $"<h4 class=\"ll-entry-card__title\">{htmlEncode entryCardState.TitleText}</h4>"

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
        appendLine builder "<div class=\"ll-screen-body\">"
        appendLine builder "<section class=\"ll-panel ll-panel--location\">"
        appendLine builder "<h3 class=\"ll-panel__title\">📍 Location</h3>"
        appendLine builder "<div class=\"ll-location-section\">"
        appendLine builder "<div class=\"ll-location-input\">"
        renderTextInput builder "Location" screenState.LocationInput
        appendLine builder "<div class=\"ll-location-info\">GPS will check personal &amp; community data</div>"
        appendLine builder "</div>"
        appendLine builder $"<button type=\"button\" class=\"ll-gps-button\" data-control-id=\"{PrimitiveControlId.value screenState.GpsAction.ControlId}\">📍</button>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "<div class=\"ll-primary-action-row\">"
        renderActionButton builder screenState.SetLocationAction
        appendLine builder "</div>"
        appendLine builder "</div>"
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
        appendLine builder "<div class=\"ll-screen-body\">"

        match screenState.LocationInput, screenState.GpsAction with
        | Some locationInput, Some gpsAction ->
            appendLine builder "<section class=\"ll-panel ll-panel--location\">"
            appendLine builder "<h3 class=\"ll-panel__title\">📍 Location</h3>"
            appendLine builder "<div class=\"ll-location-section\">"
            appendLine builder "<div class=\"ll-location-input\">"
            renderTextInput builder "Location" locationInput
            appendLine builder "<div class=\"ll-location-info\">GPS will check personal &amp; community data</div>"
            appendLine builder "</div>"
            appendLine builder $"<button type=\"button\" class=\"ll-gps-button\" data-control-id=\"{PrimitiveControlId.value gpsAction.ControlId}\">📍</button>"
            appendLine builder "</div>"
            appendLine builder "</section>"
        | _ ->
            match screenState.Header.Subtitle with
            | Some subtitle ->
                appendLine builder $"<div class=\"ll-location-context\">📍 {htmlEncode subtitle}</div>"
            | None -> ()

        appendLine builder "<section class=\"ll-panel ll-panel--compact\">"
        renderOptionGroup builder None screenState.MachineTypeOptions
        appendLine builder "</section>"

        appendLine builder "<div class=\"ll-two-up\">"
        renderPanelStart builder "Quantity"
        renderStepper builder "Quantity" screenState.QuantityStepper
        renderPanelEnd builder

        renderPanelStart builder "Unit Price"
        renderMoneyInput builder "Unit Price" screenState.PriceInput
        appendLine builder "<div class=\"ll-entry-total\">"
        appendLine builder "<span class=\"ll-entry-total__label\">Entry Total</span>"
        appendLine builder $"<span class=\"ll-entry-total__amount\">{htmlEncode (entryTotalText screenState)}</span>"
        appendLine builder "</div>"
        renderPanelEnd builder
        appendLine builder "</div>"

        appendLine builder "<section class=\"ll-panel ll-panel--compact\">"
        renderOptionGroup builder None screenState.PaymentOptions

        match screenState.PaymentDetailOptions with
        | Some paymentDetailOptions ->
            renderOptionGroup builder None paymentDetailOptions
        | None -> ()

        appendLine builder "</section>"

        match screenState.FeedbackBanner with
        | Some feedbackBanner -> renderFeedbackBanner builder feedbackBanner
        | None -> ()

        appendLine builder "<div class=\"ll-primary-action-row\">"
        renderValidationAction builder screenState.StatusChips screenState.SubmitAction
        appendLine builder "</div>"
        renderSummaryBar builder screenState.SessionTotal

        appendLine builder "<section class=\"ll-recent-entries\">"
        appendLine builder "<h3 class=\"ll-recent-entries__title\">Today's Entries</h3>"

        if List.isEmpty screenState.RecentEntries then
            appendLine builder "<p class=\"ll-empty-state\">No entries yet in this session.</p>"
        else
            appendLine builder "<div class=\"ll-entry-list\">"
            screenState.RecentEntries |> List.iter (renderEntryCard builder)
            appendLine builder "</div>"

        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "</article>"

    let private renderScreenSurface (builder: StringBuilder) =
        function
        | NewSessionScreen (surfaceName, note, screenState) ->
            renderNewSessionScreen builder surfaceName note screenState
        | EntryFormScreen (surfaceName, note, screenState) ->
            renderEntryFormScreen builder surfaceName note screenState

    let private splitPrimarySurface (screenSurfaces: ScreenSurfaceState list) =
        let isPrimaryEntryForm =
            function
            | EntryFormScreen (surfaceName, _, _) when surfaceName = "Screen.EntryForm - v7 Primary" -> true
            | _ -> false

        match screenSurfaces |> List.tryFind isPrimaryEntryForm with
        | Some primarySurface ->
            let supportingSurfaces =
                screenSurfaces
                |> List.filter (fun surface -> surface <> primarySurface)

            primarySurface, supportingSurfaces
        | None ->
            match screenSurfaces with
            | primarySurface :: supportingSurfaces -> primarySurface, supportingSurfaces
            | [] -> failwith "Expected at least one LaundryLog screen surface."

    let private renderStyles (builder: StringBuilder) =
        appendLine builder "<style>"
        appendLine builder ":root { color-scheme: light; }"
        appendLine builder "body { margin: 0; background: #f8f9fa; color: #2d3748; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif; }"
        appendLine builder ".ll-document { padding: 14px 16px 28px; max-width: 1040px; margin: 0 auto; }"
        appendLine builder ".ll-document__header { margin-bottom: 14px; display: flex; flex-direction: column; gap: 3px; }"
        appendLine builder ".ll-document__eyebrow { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #0e5883; }"
        appendLine builder ".ll-document__title { margin: 0; font-size: 0.96rem; line-height: 1.06; font-weight: 700; }"
        appendLine builder ".ll-document__description { margin: 0; color: #64748b; font-size: 0.72rem; line-height: 1.28; }"
        appendLine builder ".ll-primary-surface { display: flex; justify-content: center; margin-bottom: 22px; }"
        appendLine builder ".ll-primary-surface .ll-screen-surface { width: 100%; max-width: 380px; }"
        appendLine builder ".ll-primary-surface .ll-phone-screen { max-width: 380px; }"
        appendLine builder ".ll-primary-surface .ll-screen-surface__name { font-size: 0.68rem; }"
        appendLine builder ".ll-primary-surface .ll-screen-surface__note { font-size: 0.74rem; }"
        appendLine builder ".ll-supporting-surfaces { display: flex; flex-direction: column; gap: 10px; }"
        appendLine builder ".ll-supporting-surfaces__title { margin: 0; font-size: 0.78rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.08em; color: #64748b; }"
        appendLine builder ".ll-screen-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 320px)); gap: 18px; align-items: start; }"
        appendLine builder ".ll-screen-surface { display: flex; flex-direction: column; gap: 8px; }"
        appendLine builder ".ll-screen-surface__name { font-size: 0.64rem; font-weight: 700; letter-spacing: 0.06em; color: #0e5883; text-transform: uppercase; }"
        appendLine builder ".ll-screen-surface__note { margin: 0; color: #64748b; font-size: 0.72rem; line-height: 1.25; }"
        appendLine builder ".ll-phone-screen { width: 100%; max-width: 360px; min-height: 667px; box-sizing: border-box; background: transparent; display: flex; flex-direction: column; gap: 0; }"
        appendLine builder ".ll-phone-screen--tall { min-height: 980px; }"
        appendLine builder ".ll-screen-body { padding: 1rem; }"
        appendLine builder ".ll-location-context { margin: 0 0 1rem; color: #64748b; font-size: 0.82rem; font-weight: 600; }"
        appendLine builder ".ll-header { background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); color: white; padding: 1rem; box-shadow: 0 2px 8px rgba(0,0,0,0.15); display: flex; justify-content: space-between; align-items: center; border-radius: 0.75rem 0.75rem 0 0; }"
        appendLine builder ".ll-header__text { display: flex; flex-direction: column; gap: 0.125rem; }"
        appendLine builder ".ll-header__title { margin: 0; font-size: 1.5rem; font-weight: 700; line-height: 1.05; }"
        appendLine builder ".ll-header__subtitle { margin: 0; font-size: 0.75rem; opacity: 0.95; font-weight: 500; color: rgba(255,255,255,0.95); }"
        appendLine builder ".ll-header__badge { width: 48px; height: 48px; background: rgba(255, 255, 255, 0.25); border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; border: 2px solid rgba(255, 255, 255, 0.3); }"
        appendLine builder ".ll-panel { background: white; border-radius: 0.75rem; padding: 1.25rem; margin-bottom: 1rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); display: flex; flex-direction: column; gap: 0.75rem; }"
        appendLine builder ".ll-panel--compact { padding-top: 1rem; padding-bottom: 1rem; }"
        appendLine builder ".ll-panel--location { gap: 0.75rem; }"
        appendLine builder ".ll-panel__title { margin: 0; font-size: 0.875rem; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }"
        appendLine builder ".ll-field { display: flex; flex-direction: column; gap: 0.5rem; }"
        appendLine builder ".ll-field__label { font-size: 0.75rem; font-weight: 600; color: #64748b; text-align: center; text-transform: uppercase; letter-spacing: 0.05em; }"
        appendLine builder ".ll-location-section { display: flex; gap: 0.75rem; align-items: stretch; }"
        appendLine builder ".ll-location-input { flex: 1; display: flex; flex-direction: column; }"
        appendLine builder ".ll-location-info { font-size: 0.75rem; color: #64748b; margin-top: 0.5rem; font-weight: 500; }"
        appendLine builder ".ll-text-input { width: 100%; padding: 1rem; border: 3px solid #e2e8f0; border-radius: 0.75rem; font-size: 1rem; color: #2d3748; transition: border-color 0.2s; font-weight: 500; }"
        appendLine builder ".ll-text-input:focus { outline: none; border-color: #ffcc80; }"
        appendLine builder ".ll-text-input::placeholder { color: #8aa0b7; }"
        appendLine builder ".ll-gps-button { width: 64px; min-height: 56px; border: 3px solid #e2e8f0; background: white; border-radius: 0.75rem; font-size: 1.75rem; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; }"
        appendLine builder ".ll-gps-button:active { transform: scale(0.97); background: #fff8e1; border-color: #ffcc80; }"
        appendLine builder ".ll-primary-action-row { display: flex; flex-direction: column; gap: 0.75rem; }"
        appendLine builder ".ll-button { width: 100%; border: none; border-radius: 0.75rem; min-height: 64px; padding: 1rem; font-size: 1.05rem; font-weight: 700; cursor: pointer; transition: all 0.3s; display: flex; align-items: center; justify-content: center; flex-wrap: wrap; gap: 0.5rem; }"
        appendLine builder ".ll-button:disabled { cursor: not-allowed; }"
        appendLine builder ".ll-button--primary { background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); color: white; box-shadow: 0 4px 12px rgba(255, 204, 128, 0.4); }"
        appendLine builder ".ll-button--supporting { background: white; border: 3px solid #e2e8f0; color: #475569; box-shadow: none; }"
        appendLine builder ".ll-button--secondary { background: white; border: 3px solid #e2e8f0; color: #475569; box-shadow: none; }"
        appendLine builder ".ll-button--disabled { background: #cbd5e1; color: #475569; box-shadow: none; }"
        appendLine builder ".ll-button--success { background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%); color: white; box-shadow: 0 4px 12px rgba(34, 197, 94, 0.28); }"
        appendLine builder ".ll-option-group { display: grid; gap: 0.75rem; }"
        appendLine builder ".ll-option-group--machine { grid-template-columns: repeat(3, 1fr); }"
        appendLine builder ".ll-option-group--payment { grid-template-columns: repeat(2, 1fr); }"
        appendLine builder ".ll-option-group--detail { grid-template-columns: repeat(2, 1fr); margin-top: 0.25rem; }"
        appendLine builder ".ll-chip { border: 3px solid #e2e8f0; background: white; border-radius: 0.75rem; color: #475569; cursor: pointer; transition: all 0.2s; }"
        appendLine builder ".ll-chip--tile { min-height: 64px; padding: 0.875rem 0.5rem; font-size: 0.9375rem; font-weight: 600; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 0.2rem; }"
        appendLine builder ".ll-chip--detail { min-height: 48px; padding: 0.75rem 0.6rem; font-size: 0.82rem; font-weight: 600; }"
        appendLine builder ".ll-chip--selected { background: #fff8e1; border-color: #ffcc80; color: #f57c00; }"
        appendLine builder ".ll-chip__icon { font-size: 1rem; line-height: 1; }"
        appendLine builder ".ll-chip__label { line-height: 1.1; }"
        appendLine builder ".ll-two-up { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }"
        appendLine builder ".ll-stepper { display: flex; align-items: center; justify-content: center; gap: 1.5rem; padding: 0.5rem 0; }"
        appendLine builder ".ll-stepper__button { width: 72px; height: 72px; border: none; background: #ffcc80; color: white; border-radius: 50%; font-size: 2.5rem; font-weight: 700; cursor: pointer; transition: all 0.2s; box-shadow: 0 2px 8px rgba(255, 204, 128, 0.3); }"
        appendLine builder ".ll-stepper__button:active { transform: scale(0.95); background: #ffb74d; }"
        appendLine builder ".ll-stepper__value { font-size: 3rem; font-weight: 700; color: #2d3748; min-width: 80px; width: 80px; text-align: center; padding: 0.5rem; border: 3px solid transparent; border-radius: 0.5rem; background: transparent; }"
        appendLine builder ".ll-money-input { display: flex; align-items: center; justify-content: center; gap: 0.25rem; border: 0; background: transparent; overflow: visible; }"
        appendLine builder ".ll-money-input__currency { font-size: 1.5rem; font-weight: 700; color: #64748b; padding: 0; }"
        appendLine builder ".ll-money-input__field { width: 100%; max-width: 140px; padding: 0.875rem 0.5rem; border: 3px solid #e2e8f0; border-radius: 0.75rem; font-size: 1.5rem; font-weight: 700; color: #2d3748; text-align: center; background: white; }"
        appendLine builder ".ll-quarter-row { display: flex; align-items: center; justify-content: center; gap: 0.75rem; margin-bottom: 0.75rem; }"
        appendLine builder ".ll-quarter-button { width: 72px; height: 72px; border: none; background: linear-gradient(135deg, #e8e8e8 0%, #c0c0c0 100%); color: #4a4a4a; border-radius: 50%; font-size: 1rem; font-weight: 700; cursor: pointer; transition: all 0.2s; box-shadow: 0 3px 8px rgba(0, 0, 0, 0.2), inset 0 1px 3px rgba(255, 255, 255, 0.5); display: flex; align-items: center; justify-content: center; border: 3px solid #a8a8a8; position: relative; font-family: 'Courier New', monospace; }"
        appendLine builder ".ll-quarter-button::before { content: ''; position: absolute; inset: 2px; border-radius: 50%; border: 1px dashed rgba(128, 128, 128, 0.3); }"
        appendLine builder ".ll-quarter-button__sign { font-size: 1.25rem; font-weight: 900; margin-right: -0.125rem; z-index: 1; }"
        appendLine builder ".ll-quarter-button__coin { font-size: 1.125rem; font-weight: 700; z-index: 1; }"
        appendLine builder ".ll-quarter-button:active { transform: scale(0.95); box-shadow: 0 1px 4px rgba(0, 0, 0, 0.2), inset 0 1px 3px rgba(255, 255, 255, 0.5); }"
        appendLine builder ".ll-quick-fill-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.5rem; margin-bottom: 0.75rem; }"
        appendLine builder ".ll-chip--quick-fill { padding: 0.875rem 0.5rem; border: 2px solid #ffcc80; background: #fff8e1; border-radius: 0.5rem; font-size: 0.9375rem; font-weight: 600; color: #f57c00; cursor: pointer; transition: all 0.2s; display: flex; flex-direction: column; align-items: center; justify-content: center; }"
        appendLine builder ".ll-chip__amount { font-size: 0.9375rem; font-weight: 600; }"
        appendLine builder ".ll-chip__subtext { font-size: 0.625rem; opacity: 0.8; display: block; margin-top: 0.25rem; }"
        appendLine builder ".ll-entry-total { background: #f1f5f9; padding: 1rem; border-radius: 0.75rem; display: flex; justify-content: space-between; align-items: center; }"
        appendLine builder ".ll-entry-total__label { font-size: 0.875rem; font-weight: 600; color: #64748b; }"
        appendLine builder ".ll-entry-total__amount { font-size: 1.75rem; font-weight: 700; color: #2d3748; }"
        appendLine builder ".ll-status-chip-row { display: flex; flex-wrap: wrap; gap: 0.4rem; justify-content: center; width: 100%; }"
        appendLine builder ".ll-status-chip { display: inline-flex; align-items: center; gap: 0.25rem; padding: 0.375rem 0.75rem; background: rgba(255, 255, 255, 0.9); border-radius: 1rem; font-size: 0.875rem; font-weight: 600; color: #475569; box-shadow: none; }"
        appendLine builder ".ll-status-chip__label { color: inherit; }"
        appendLine builder ".ll-status-chip__mark { font-weight: 800; }"
        appendLine builder ".ll-status-chip--ready .ll-status-chip__mark { color: #16a34a; }"
        appendLine builder ".ll-status-chip--needs-attention .ll-status-chip__mark { color: #dc2626; }"
        appendLine builder ".ll-summary-bar { background: #fff8e1; padding: 0.875rem 1rem; border-bottom: 2px solid #ffcc80; border-top: 2px solid #ffcc80; display: flex; justify-content: space-between; align-items: center; margin: 1rem 0.25rem 1.5rem 0.25rem; border-radius: 0.5rem; }"
        appendLine builder ".ll-summary-bar__label { font-size: 0.875rem; font-weight: 600; color: #f57c00; text-transform: uppercase; letter-spacing: 0.05em; }"
        appendLine builder ".ll-summary-bar__value { font-size: 1.5rem; font-weight: 700; color: #f57c00; }"
        appendLine builder ".ll-feedback-banner { border-radius: 0.75rem; background: #dcfce7; border: 1px solid #86efac; color: #166534; padding: 0.75rem 1rem; font-size: 0.875rem; font-weight: 700; }"
        appendLine builder ".ll-recent-entries { margin-top: 0; }"
        appendLine builder ".ll-recent-entries__title { font-size: 0.875rem; font-weight: 600; color: #64748b; margin-bottom: 1rem; text-transform: uppercase; letter-spacing: 0.05em; }"
        appendLine builder ".ll-entry-list { display: flex; flex-direction: column; gap: 0.75rem; }"
        appendLine builder ".ll-entry-card { background: white; border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgba(0,0,0,0.08); border-left: 4px solid #cbd5e1; }"
        appendLine builder ".ll-entry-card__topline { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }"
        appendLine builder ".ll-entry-card__title { margin: 0; font-size: 0.75rem; color: #94a3b8; font-weight: 500; white-space: nowrap; }"
        appendLine builder ".ll-entry-card__amount { font-size: 1.5rem; font-weight: 700; color: #2d3748; white-space: nowrap; }"
        appendLine builder ".ll-entry-card__detail { margin: 0; font-size: 0.8125rem; color: #64748b; font-weight: 500; }"
        appendLine builder ".ll-empty-state { margin: 0; font-size: 0.75rem; color: #94a3b8; font-weight: 500; }"
        appendLine builder "@media (max-width: 920px) { .ll-document { padding-left: 12px; padding-right: 12px; } .ll-screen-grid { grid-template-columns: 1fr; } .ll-two-up { grid-template-columns: 1fr; } .ll-primary-surface .ll-screen-surface, .ll-primary-surface .ll-phone-screen { max-width: 100%; } }"
        appendLine builder "</style>"

    /// Renders a self-contained HTML document for the current LaundryLog screen proving ground.
    let renderDocument documentTitle description (screenSurfaces: ScreenSurfaceState list) =
        let primarySurface, supportingSurfaces = splitPrimarySurface screenSurfaces
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
        appendLine builder "<div class=\"ll-document__eyebrow\">Mobile Source Of Truth</div>"
        appendLine builder $"<h1 class=\"ll-document__title\">{htmlEncode documentTitle}</h1>"
        appendLine builder $"<p class=\"ll-document__description\">{htmlEncode description}</p>"
        appendLine builder "</header>"
        appendLine builder "<section class=\"ll-primary-surface\">"
        renderScreenSurface builder primarySurface
        appendLine builder "</section>"

        if not (List.isEmpty supportingSurfaces) then
            appendLine builder "<section class=\"ll-supporting-surfaces\">"
            appendLine builder "<h2 class=\"ll-supporting-surfaces__title\">Supporting State Variants</h2>"
            appendLine builder "<div class=\"ll-screen-grid\">"
            supportingSurfaces |> List.iter (renderScreenSurface builder)
            appendLine builder "</div>"
            appendLine builder "</section>"

        appendLine builder "</main>"
        appendLine builder "</body>"
        appendLine builder "</html>"

        builder.ToString()
