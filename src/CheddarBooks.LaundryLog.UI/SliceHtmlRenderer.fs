namespace CheddarBooks.LaundryLog.UI

open System
open System.Net
open System.Text
open CheddarBooks.LaundryLog

/// Distinguishes the current supported block kinds inside a rendered slice card.
type SliceBlockKind =
    | Screen
    | Command
    | Event
    | View

[<RequireQualifiedAccess>]
module SliceBlockKind =
    /// Returns the stable uppercase badge label for the block kind.
    let badgeLabel =
        function
        | SliceBlockKind.Screen -> "SCREEN"
        | SliceBlockKind.Command -> "COMMAND"
        | SliceBlockKind.Event -> "EVENT"
        | SliceBlockKind.View -> "VIEW"

    /// Returns the stable CSS class suffix for the block kind.
    let cssClass =
        function
        | SliceBlockKind.Screen -> "screen"
        | SliceBlockKind.Command -> "command"
        | SliceBlockKind.Event -> "event"
        | SliceBlockKind.View -> "view"

/// Describes the content carried inside one rendered slice block.
type SliceBlockContent =
    | ScreenshotPlaceholder of string
    | PropertyLines of string list

/// Describes one rendered block inside a CommandSlice or ViewSlice card.
type SliceBlockState =
    { Kind: SliceBlockKind
      Title: string
      MetaBadges: string list
      Content: SliceBlockContent
      FooterBadgeText: string option }

[<RequireQualifiedAccess>]
module SliceBlockState =
    /// Creates a validated slice block using small explicit surface rules.
    let tryCreate kind title metaBadges content footerBadgeText =
        let isBlank value = String.IsNullOrWhiteSpace value
        let footerBadgeText : string option = footerBadgeText

        if isBlank title then
            Error "Slice blocks must provide a title."
        elif metaBadges |> List.exists isBlank then
            Error "Slice block badges must not be blank."
        else
            match content with
            | ScreenshotPlaceholder label when isBlank label ->
                Error "Screenshot placeholders must provide visible label text."
            | PropertyLines lines when lines |> List.exists isBlank ->
                Error "Slice block property lines must not contain blanks."
            | _ ->
                Ok
                    { Kind = kind
                      Title = title.Trim()
                      MetaBadges = metaBadges |> List.map (fun value -> value.Trim())
                      Content = content
                      FooterBadgeText = footerBadgeText |> Option.map (fun (value: string) -> value.Trim()) }

/// Describes one fully rendered CommandSlice card.
type CommandSliceCardState =
    { AppName: string
      Title: string
      Description: string
      Screen: SliceBlockState
      Command: SliceBlockState
      Event: SliceBlockState }

[<RequireQualifiedAccess>]
module CommandSliceCardState =
    /// Creates a validated CommandSlice card state.
    let tryCreate appName title description screen command event =
        if String.IsNullOrWhiteSpace appName then
            Error "CommandSlice cards must provide an app name."
        elif String.IsNullOrWhiteSpace title then
            Error "CommandSlice cards must provide a title."
        elif String.IsNullOrWhiteSpace description then
            Error "CommandSlice cards must provide a description."
        else
            Ok
                { AppName = appName.Trim()
                  Title = title.Trim()
                  Description = description.Trim()
                  Screen = screen
                  Command = command
                  Event = event }

/// Describes one fully rendered ViewSlice card.
type ViewSliceCardState =
    { AppName: string
      Title: string
      Description: string
      Screen: SliceBlockState option
      View: SliceBlockState }

[<RequireQualifiedAccess>]
module ViewSliceCardState =
    /// Creates a validated ViewSlice card state.
    let tryCreate appName title description screen view =
        if String.IsNullOrWhiteSpace appName then
            Error "ViewSlice cards must provide an app name."
        elif String.IsNullOrWhiteSpace title then
            Error "ViewSlice cards must provide a title."
        elif String.IsNullOrWhiteSpace description then
            Error "ViewSlice cards must provide a description."
        else
            Ok
                { AppName = appName.Trim()
                  Title = title.Trim()
                  Description = description.Trim()
                  Screen = screen
                  View = view }

/// Distinguishes the supported slice cards that can appear in a rendered path row.
type PathSliceCard =
    | CommandSlice of CommandSliceCardState
    | ViewSlice of ViewSliceCardState

/// Describes one rendered PATH row.
type PathRowState =
    { PathId: string
      Title: string
      Description: string
      SliceCards: PathSliceCard list }

[<RequireQualifiedAccess>]
module PathRowState =
    /// Creates a validated PATH row state.
    let tryCreate pathId title description sliceCards =
        if String.IsNullOrWhiteSpace pathId then
            Error "Path rows must provide a stable path identifier."
        elif String.IsNullOrWhiteSpace title then
            Error "Path rows must provide a title."
        elif String.IsNullOrWhiteSpace description then
            Error "Path rows must provide a description."
        elif List.isEmpty sliceCards then
            Error "Path rows must contain at least one slice card."
        else
            Ok
                { PathId = pathId.Trim()
                  Title = title.Trim()
                  Description = description.Trim()
                  SliceCards = sliceCards }

/// Describes the current rendering options for the HTML path projection.
type SliceRenderOptions =
    { ShowProperties: bool
      ShowViewScreens: bool
      DocumentTitle: string }

[<RequireQualifiedAccess>]
module SliceRenderOptions =
    /// Classic Event Modeling lens with green-box View emphasis.
    let classicEventModel documentTitle =
        { ShowProperties = true
          ShowViewScreens = false
          DocumentTitle = documentTitle }

    /// UI-enriched lens that also shows the screen supported by the View.
    let uiNarrative documentTitle =
        { ShowProperties = true
          ShowViewScreens = true
          DocumentTitle = documentTitle }

/// Provides deterministic LaundryLog slice examples that back the first HTML/CSS proving ground.
[<RequireQualifiedAccess>]
module SliceHtmlExamples =
    let private expect description result =
        match result with
        | Ok value -> value
        | Error message -> failwith $"Expected a valid {description}. {message}"

    let private location =
        LocationName.tryCreate "Love's #123 - Springfield, OH"
        |> expect "path1 location"

    let private block kind title badges content footerBadgeText =
        SliceBlockState.tryCreate kind title badges content footerBadgeText
        |> expect $"slice block '{title}'"

    let private commandSlice appName title description screen command event =
        CommandSliceCardState.tryCreate appName title description screen command event
        |> expect $"command slice '{title}'"
        |> PathSliceCard.CommandSlice

    let private viewSlice appName title description screen view =
        ViewSliceCardState.tryCreate appName title description screen view
        |> expect $"view slice '{title}'"
        |> PathSliceCard.ViewSlice

    let private visibleLine expenseKind paymentMethod quantity lineTotal =
        VisibleLaundryExpenseViewLine.tryCreate expenseKind paymentMethod quantity lineTotal
        |> expect $"visible entry '{ExpenseKind.displayName expenseKind}'"

    let private currentSession visibleEntries runningTotalText lastRecordedAtLocalText =
        CurrentLaundrySessionViewState.tryCreate location visibleEntries runningTotalText lastRecordedAtLocalText
        |> expect "current session view state"

    let private quoted value = $"\"{value}\""

    let private formatLocationCaptureEvent recordedAtUtc =
        let captureMethodText = quoted "manual-text"

        [ $"location_name = {quoted (LocationName.value location)}"
          $"capture_method = {captureMethodText}"
          $"recorded_at_utc = {quoted recordedAtUtc}" ]

    let private formatExpenseCommand (command: LogLaundryExpenseCommandSliceState) =
        let unitPriceText = defaultArg command.UnitPriceText ""

        [ $"expense_kind = {quoted (ExpenseKind.slug command.SelectedExpenseKind)}"
          $"quantity = {command.Quantity}"
          $"unit_price = {quoted unitPriceText}"
          $"payment_method = {quoted (PaymentMethod.slug command.SelectedPaymentMethod)}" ]

    let private formatExpenseEvent recordedAtUtc (command: LogLaundryExpenseCommandSliceState) =
        [ yield $"location_name = {quoted (LocationName.value location)}"
          yield! formatExpenseCommand command
          yield $"recorded_at_utc = {quoted recordedAtUtc}" ]

    let private formatViewState (viewState: CurrentLaundrySessionViewState) =
        let visibleEntriesText =
            match viewState.VisibleEntries with
            | [] -> "[]"
            | entries ->
                entries
                |> List.map (fun entry ->
                    $"{ExpenseKind.displayName entry.ExpenseKind} x{entry.Quantity} {PaymentMethod.displayName entry.PaymentMethod} {entry.LineTotalText}")
                |> String.concat "; "
                |> fun value -> $"[{value}]"

        [ $"active_location_name = {quoted (LocationName.value viewState.ActiveLocationName)}"
          $"running_total = {quoted viewState.RunningTotalText}"
          $"visible_entries = {quoted visibleEntriesText}" ]

    /// Builds the first deterministic LaundryLog PATH 1 row using local command/event/view examples.
    let path1ManualLocationWasherDryer () : PathRowState =
        let captureMethodText = quoted "manual-text"

        let washerCommand =
            LogLaundryExpenseCommandSliceState.tryCreate
                ExpenseKind.Washer
                1
                (Some "3.00")
                PaymentMethod.Card
                [ "$2.50"; "$3.00"; "$3.50" ]
                true
            |> expect "washer command slice"

        let dryerCommand =
            LogLaundryExpenseCommandSliceState.tryCreate
                ExpenseKind.Dryer
                1
                (Some "2.50")
                PaymentMethod.Cash
                [ "$2.50"; "$3.00"; "$3.50" ]
                true
            |> expect "dryer command slice"

        let washerEntry = visibleLine ExpenseKind.Washer PaymentMethod.Card 1 "$3.00"
        let dryerEntry = visibleLine ExpenseKind.Dryer PaymentMethod.Cash 1 "$2.50"

        let readyView = currentSession [] "$0.00" None
        let washerView = currentSession [ washerEntry ] "$3.00" (Some "2026-03-27 09:47")
        let washerDryerView = currentSession [ washerEntry; dryerEntry ] "$5.50" (Some "2026-03-27 10:03")

        PathRowState.tryCreate
            "path1"
            "PATH 1: Manual Location -> Washer -> Dryer"
            "Single-actor readable row for the first LaundryLog path. The location is captured first, then washer and dryer expenses are logged into the same current session."
            [ commandSlice
                  "LaundryLog"
                  "Capture Laundry Location"
                  "manual location entry starts the path"
                  (block
                      SliceBlockKind.Screen
                      "Set Location Screen"
                      [ "User" ]
                      (ScreenshotPlaceholder "Screen snapshot")
                      (Some "ui lens"))
                  (block
                      SliceBlockKind.Command
                      "CaptureLaundryLocation"
                      []
                      (PropertyLines [ $"location_name = {quoted (LocationName.value location)}"; $"capture_method = {captureMethodText}" ])
                      (Some "business"))
                  (block
                      SliceBlockKind.Event
                      "LaundryLocationCaptured"
                      []
                      (PropertyLines (formatLocationCaptureEvent "2026-03-27T13:42:00Z"))
                      (Some "business"))
              viewSlice
                  "LaundryLog"
                  "Current Laundry Session"
                  "ready to log the first washer expense"
                  (Some
                      (block
                          SliceBlockKind.Screen
                          "Log Expense Screen - Ready"
                          [ "User" ]
                          (ScreenshotPlaceholder "Screen snapshot")
                          (Some "ui lens")))
                  (block
                      SliceBlockKind.View
                      "CurrentLaundrySession"
                      []
                      (PropertyLines (formatViewState readyView))
                      (Some "classic em"))
              commandSlice
                  "LaundryLog"
                  "Log Washer Expense"
                  "first expense recorded in the active location"
                  (block
                      SliceBlockKind.Screen
                      "Log Expense Screen - Ready"
                      [ "User" ]
                      (ScreenshotPlaceholder "Screen snapshot")
                      (Some "ui lens"))
                  (block
                      SliceBlockKind.Command
                      "LogLaundryExpense"
                      []
                      (PropertyLines (formatExpenseCommand washerCommand))
                      (Some "business"))
                  (block
                      SliceBlockKind.Event
                      "LaundryExpenseLogged"
                      []
                      (PropertyLines (formatExpenseEvent "2026-03-27T13:47:00Z" washerCommand))
                      (Some "business"))
              viewSlice
                  "LaundryLog"
                  "Current Laundry Session"
                  "washer entry is visible in the current session"
                  (Some
                      (block
                          SliceBlockKind.Screen
                          "Log Expense Screen - Washer Visible"
                          [ "User" ]
                          (ScreenshotPlaceholder "Screen snapshot")
                          (Some "ui lens")))
                  (block
                      SliceBlockKind.View
                      "CurrentLaundrySession"
                      []
                      (PropertyLines (formatViewState washerView))
                      (Some "classic em"))
              commandSlice
                  "LaundryLog"
                  "Log Dryer Expense"
                  "second expense recorded in the same session"
                  (block
                      SliceBlockKind.Screen
                      "Log Expense Screen - Washer Visible"
                      [ "User" ]
                      (ScreenshotPlaceholder "Screen snapshot")
                      (Some "ui lens"))
                  (block
                      SliceBlockKind.Command
                      "LogLaundryExpense"
                      []
                      (PropertyLines (formatExpenseCommand dryerCommand))
                      (Some "business"))
                  (block
                      SliceBlockKind.Event
                      "LaundryExpenseLogged"
                      []
                      (PropertyLines (formatExpenseEvent "2026-03-27T14:03:00Z" dryerCommand))
                      (Some "business"))
              viewSlice
                  "LaundryLog"
                  "Current Laundry Session"
                  "washer and dryer are visible in the current session"
                  (Some
                      (block
                          SliceBlockKind.Screen
                          "Log Expense Screen - Washer And Dryer Visible"
                          [ "User" ]
                          (ScreenshotPlaceholder "Screen snapshot")
                          (Some "ui lens")))
                  (block
                      SliceBlockKind.View
                      "CurrentLaundrySession"
                      []
                      (PropertyLines (formatViewState washerDryerView))
                      (Some "classic em")) ]
        |> expect "path1 row"

/// Renders deterministic HTML/CSS slice projections for LaundryLog PATH work.
[<RequireQualifiedAccess>]
module SliceHtmlRenderer =
    let private htmlEncode (value: string) = WebUtility.HtmlEncode value

    let private appendLine (builder: StringBuilder) (value: string) =
        builder.AppendLine(value) |> ignore

    let private renderBadge (builder: StringBuilder) cssClass text =
        appendLine builder $"<span class=\"{cssClass}\">{htmlEncode text}</span>"

    let private renderBlockContent (builder: StringBuilder) showProperties content =
        match content with
        | ScreenshotPlaceholder label ->
            appendLine
                builder
                $"<div class=\"slice-block__snapshot\"><span>{htmlEncode label}</span></div>"
        | PropertyLines propertyLines when showProperties ->
            appendLine builder "<details class=\"slice-block__properties-panel\">"
            appendLine builder "<summary class=\"slice-block__properties-summary\">properties</summary>"
            appendLine builder "<div class=\"slice-block__properties\">"

            propertyLines
            |> List.iter (fun line ->
                appendLine builder $"<div class=\"slice-block__property-line\">{htmlEncode line}</div>")

            appendLine builder "</div>"
            appendLine builder "</details>"
        | PropertyLines _ ->
            appendLine builder "<div class=\"slice-block__properties slice-block__properties--hidden\"></div>"

    let private renderBlock (builder: StringBuilder) showProperties (blockState: SliceBlockState) =
        let blockCssClass = SliceBlockKind.cssClass blockState.Kind

        appendLine builder $"<section class=\"slice-block slice-block--{blockCssClass}\">"
        appendLine builder "<div class=\"slice-block__topline\">"
        appendLine builder $"<h3 class=\"slice-block__title\">{htmlEncode blockState.Title}</h3>"
        appendLine builder "<div class=\"slice-block__badges\">"

        blockState.MetaBadges
        |> List.iter (renderBadge builder "slice-block__badge slice-block__badge--meta")

        renderBadge builder "slice-block__badge slice-block__badge--kind" (SliceBlockKind.badgeLabel blockState.Kind)
        appendLine builder "</div>"
        appendLine builder "</div>"
        renderBlockContent builder showProperties blockState.Content

        match blockState.FooterBadgeText with
        | Some footerBadgeText ->
            appendLine builder "<div class=\"slice-block__footer\">"
            renderBadge builder "slice-block__footer-badge" footerBadgeText
            appendLine builder "</div>"
        | None -> ()

        appendLine builder "</section>"

    let private renderCommandSlice (builder: StringBuilder) options (sliceState: CommandSliceCardState) =
        appendLine builder "<article class=\"slice-card slice-card--command\">"
        appendLine builder "<div class=\"slice-card__topline\">"
        renderBadge builder "slice-card__app-pill" sliceState.AppName
        appendLine builder "<div class=\"slice-card__kind\">COMMAND SLICE</div>"
        appendLine builder "</div>"
        appendLine builder $"<h2 class=\"slice-card__title\">{htmlEncode sliceState.Title}</h2>"
        appendLine builder $"<p class=\"slice-card__description\">{htmlEncode sliceState.Description}</p>"
        appendLine builder "<div class=\"slice-card__body\">"
        renderBlock builder options.ShowProperties sliceState.Screen
        renderBlock builder options.ShowProperties sliceState.Command
        renderBlock builder options.ShowProperties sliceState.Event
        appendLine builder "</div>"
        appendLine builder "</article>"

    let private renderViewSlice (builder: StringBuilder) options (sliceState: ViewSliceCardState) =
        appendLine builder "<article class=\"slice-card slice-card--view\">"
        appendLine builder "<div class=\"slice-card__topline\">"
        renderBadge builder "slice-card__app-pill" sliceState.AppName
        appendLine builder "<div class=\"slice-card__kind\">VIEW SLICE</div>"
        appendLine builder "</div>"
        appendLine builder $"<h2 class=\"slice-card__title\">{htmlEncode sliceState.Title}</h2>"
        appendLine builder $"<p class=\"slice-card__description\">{htmlEncode sliceState.Description}</p>"
        appendLine builder "<div class=\"slice-card__body slice-card__body--view\">"

        if options.ShowViewScreens then
            sliceState.Screen |> Option.iter (renderBlock builder options.ShowProperties)
        else
            appendLine builder "<div class=\"slice-card__slot slice-card__slot--empty slice-card__slot--screen-gap\" aria-hidden=\"true\"></div>"

        renderBlock builder options.ShowProperties sliceState.View
        appendLine builder "<div class=\"slice-card__slot slice-card__slot--empty\" aria-hidden=\"true\"></div>"
        appendLine builder "</div>"
        appendLine builder "</article>"

    let private renderSliceCard (builder: StringBuilder) options =
        function
        | PathSliceCard.CommandSlice commandSlice -> renderCommandSlice builder options commandSlice
        | PathSliceCard.ViewSlice viewSlice -> renderViewSlice builder options viewSlice

    let private renderScript (builder: StringBuilder) =
        appendLine builder "<script>"
        appendLine builder "document.addEventListener('DOMContentLoaded', function () {"
        appendLine builder "  const expandButton = document.querySelector('[data-action=\"expand-properties\"]');"
        appendLine builder "  const collapseButton = document.querySelector('[data-action=\"collapse-properties\"]');"
        appendLine builder "  if (expandButton) {"
        appendLine builder "    expandButton.addEventListener('click', function () {"
        appendLine builder "      document.querySelectorAll('.slice-block__properties-panel').forEach(function (panel) {"
        appendLine builder "        panel.open = true;"
        appendLine builder "      });"
        appendLine builder "    });"
        appendLine builder "  }"
        appendLine builder "  if (collapseButton) {"
        appendLine builder "    collapseButton.addEventListener('click', function () {"
        appendLine builder "      document.querySelectorAll('.slice-block__properties-panel').forEach(function (panel) {"
        appendLine builder "        panel.open = false;"
        appendLine builder "      });"
        appendLine builder "    });"
        appendLine builder "  }"
        appendLine builder "});"
        appendLine builder "</script>"

    let private renderStyles (builder: StringBuilder) =
        appendLine builder "<style>"
        appendLine builder ":root { color-scheme: light; }"
        appendLine builder "body { margin: 0; background: linear-gradient(180deg, #f3f5f8 0%, #e9edf3 100%); color: #0d2440; font-family: \"IBM Plex Sans\", \"Aptos\", \"Segoe UI\", sans-serif; }"
        appendLine builder ".path-document { --screen-row-min-height: 98px; --detail-row-min-height: 58px; --screen-snapshot-height: 22px; padding: 8px 10px 10px; }"
        appendLine builder ".path-document__header { max-width: none; margin-bottom: 8px; }"
        appendLine builder ".path-document__title { margin: 0; font-size: 1.0rem; line-height: 1.02; }"
        appendLine builder ".path-document__description { margin: 3px 0 0; max-width: none; font-size: 0.7rem; line-height: 1.2; color: #48627f; white-space: nowrap; }"
        appendLine builder ".path-document__header-actions { margin-top: 5px; display: flex; align-items: center; gap: 8px; }"
        appendLine builder ".path-document__action { border: 1px solid #9ab3d0; background: rgba(255, 255, 255, 0.92); color: #27435c; border-radius: 999px; padding: 4px 10px; font-size: 0.62rem; font-weight: 700; letter-spacing: 0.04em; cursor: pointer; }"
        appendLine builder ".path-document__action:hover { background: rgba(255, 255, 255, 1.0); }"
        appendLine builder ".path-document__row { display: flex; gap: 10px; overflow-x: auto; align-items: stretch; padding: 2px 2px 6px; }"
        appendLine builder ".slice-card { flex: 0 0 224px; min-height: 0; border-radius: 20px; border: 4px solid #15263d; box-shadow: 0 10px 24px rgba(10, 27, 49, 0.1); padding: 7px 7px 8px; display: flex; flex-direction: column; }"
        appendLine builder ".slice-card--command { background: linear-gradient(180deg, #dff1ff 0%, #eff7ff 100%); }"
        appendLine builder ".slice-card--view { background: linear-gradient(180deg, #dbfae4 0%, #effbf3 100%); }"
        appendLine builder ".slice-card__topline { display: flex; align-items: center; justify-content: space-between; gap: 10px; }"
        appendLine builder ".slice-card__app-pill { display: inline-flex; align-items: center; justify-content: center; padding: 3px 9px; border-radius: 999px; background: #ffb54d; color: white; font-size: 0.62rem; font-weight: 700; line-height: 1; }"
        appendLine builder ".slice-card__kind { color: #0e5883; font-size: 0.58rem; font-weight: 700; letter-spacing: 0.14em; text-transform: uppercase; text-align: right; }"
        appendLine builder ".slice-card__title { margin: 7px 0 0; font-size: 0.82rem; line-height: 1.04; }"
        appendLine builder ".slice-card__description { margin: 3px 0 0; font-size: 0.66rem; line-height: 1.18; color: #506b87; min-height: 1.36em; }"
        appendLine builder ".slice-card__body { display: grid; grid-template-rows: minmax(var(--screen-row-min-height), max-content) minmax(var(--detail-row-min-height), max-content) minmax(var(--detail-row-min-height), max-content); gap: 6px; margin-top: 8px; align-content: start; }"
        appendLine builder ".slice-card__slot--empty { min-height: var(--detail-row-min-height); border-radius: 16px; background: transparent; }"
        appendLine builder ".slice-card__slot--screen-gap { min-height: var(--screen-row-min-height); }"
        appendLine builder ".slice-block { display: flex; flex-direction: column; gap: 3px; border-radius: 14px; padding: 6px 7px; border: 2px solid; min-height: 0; overflow: visible; }"
        appendLine builder ".slice-block--screen { background: rgba(255, 255, 255, 0.78); border-color: #c8dcff; }"
        appendLine builder ".slice-block--command { background: rgba(74, 150, 255, 0.18); border-color: #8bc3ff; }"
        appendLine builder ".slice-block--event { background: rgba(255, 167, 57, 0.2); border-color: #ffb777; }"
        appendLine builder ".slice-block--view { background: rgba(103, 229, 130, 0.18); border-color: #78e39d; }"
        appendLine builder ".slice-block__topline { display: grid; grid-template-columns: 1fr; grid-template-rows: auto auto; row-gap: 5px; min-height: 0; }"
        appendLine builder ".slice-block__badges { display: flex; flex-direction: row; justify-content: flex-end; gap: 4px; grid-row: 1; }"
        appendLine builder ".slice-block__title { margin: 0; font-size: 0.72rem; line-height: 1.14; font-weight: 700; overflow-wrap: normal; word-break: normal; hyphens: none; grid-row: 2; }"
        appendLine builder ".slice-block--screen .slice-block__title { font-weight: 400; font-size: 0.64rem; white-space: nowrap; }"
        appendLine builder ".slice-block__badge { display: inline-flex; align-items: center; justify-content: center; padding: 2px 7px; border-radius: 999px; border: 1px solid #c2d4e8; background: rgba(255, 255, 255, 0.92); font-size: 0.52rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #4a647f; white-space: nowrap; }"
        appendLine builder ".slice-block__badge--kind { color: #203954; }"
        appendLine builder ".slice-block__snapshot { min-height: var(--screen-snapshot-height); border-radius: 10px; border: 1px dashed #c7d7ea; background: linear-gradient(180deg, #eef4fb 0%, #e7eef7 100%); display: flex; align-items: center; justify-content: center; text-align: center; padding: 6px; color: #7087a0; font-size: 0.7rem; font-weight: 600; }"
        appendLine builder ".slice-block__properties-panel { margin: 0; display: flex; flex-direction: column; gap: 0; flex: 0 0 auto; min-height: 0; }"
        appendLine builder ".slice-block__properties-panel[open] { gap: 3px; }"
        appendLine builder ".slice-block__properties-panel > summary { list-style: none; cursor: pointer; }"
        appendLine builder ".slice-block__properties-panel > summary::-webkit-details-marker { display: none; }"
        appendLine builder ".slice-block__properties-summary { align-self: flex-end; display: inline-flex; align-items: center; justify-content: center; padding: 1px 6px; border-radius: 999px; border: 1px solid #c6d4e2; background: rgba(255, 255, 255, 0.94); color: #566d86; font-size: 0.5rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; }"
        appendLine builder ".slice-block__properties { display: flex; flex-direction: column; gap: 2px; background: rgba(255, 255, 255, 0.32); border-radius: 9px; padding: 6px 7px 5px; min-height: 48px; overflow: visible; }"
        appendLine builder ".slice-block__properties-panel:not([open]) .slice-block__properties { display: none; }"
        appendLine builder ".slice-block__properties--hidden { min-height: 0; padding: 0; background: transparent; }"
        appendLine builder ".slice-block__property-line { font-family: \"IBM Plex Mono\", \"Cascadia Mono\", \"Consolas\", monospace; font-size: 0.58rem; line-height: 1.18; color: #27435c; overflow-wrap: break-word; word-break: normal; }"
        appendLine builder ".slice-block__footer { display: flex; justify-content: flex-end; margin-top: auto; }"
        appendLine builder ".slice-block__footer-badge { display: inline-flex; align-items: center; justify-content: center; padding: 2px 7px; border-radius: 999px; border: 1px solid #c6d4e2; background: rgba(255, 255, 255, 0.94); color: #566d86; font-size: 0.52rem; font-weight: 700; letter-spacing: 0.08em; text-transform: lowercase; white-space: nowrap; }"
        appendLine builder "@media (max-width: 1200px) { .slice-card { flex-basis: 216px; } .path-document { padding-left: 8px; padding-right: 8px; } }"
        appendLine builder "@media (max-width: 900px) { .path-document__row { gap: 8px; } .slice-card { flex-basis: 208px; } }"
        appendLine builder "</style>"

    /// Renders a full self-contained HTML document for the supplied PATH row.
    let renderDocument options (pathRow: PathRowState) =
        let builder = StringBuilder()

        appendLine builder "<!DOCTYPE html>"
        appendLine builder "<html lang=\"en\">"
        appendLine builder "<head>"
        appendLine builder "<meta charset=\"utf-8\">"
        appendLine builder "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
        appendLine builder $"<title>{htmlEncode options.DocumentTitle}</title>"
        renderStyles builder
        appendLine builder "</head>"
        appendLine builder "<body>"
        appendLine builder "<main class=\"path-document\">"
        appendLine builder "<header class=\"path-document__header\">"
        appendLine builder $"<h1 class=\"path-document__title\">{htmlEncode pathRow.Title}</h1>"
        appendLine builder $"<p class=\"path-document__description\">{htmlEncode pathRow.Description}</p>"
        appendLine builder "<div class=\"path-document__header-actions\">"
        appendLine builder "<button type=\"button\" class=\"path-document__action\" data-action=\"expand-properties\">Expand All Properties</button>"
        appendLine builder "<button type=\"button\" class=\"path-document__action\" data-action=\"collapse-properties\">Collapse All Properties</button>"
        appendLine builder "</div>"
        appendLine builder "</header>"
        appendLine builder "<section class=\"path-document__row\">"
        pathRow.SliceCards |> List.iter (renderSliceCard builder options)
        appendLine builder "</section>"
        appendLine builder "</main>"
        renderScript builder
        appendLine builder "</body>"
        appendLine builder "</html>"

        builder.ToString()
