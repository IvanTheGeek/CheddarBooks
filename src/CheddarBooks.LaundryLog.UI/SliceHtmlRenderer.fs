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

/// Distinguishes the first supported scenario stages for GWT-style path bands.
type GwtStage =
    | Given
    | When
    | Then

[<RequireQualifiedAccess>]
module GwtStage =
    /// Returns the stable uppercase stage label for the current scenario cell.
    let label =
        function
        | GwtStage.Given -> "GIVEN"
        | GwtStage.When -> "WHEN"
        | GwtStage.Then -> "THEN"

/// Describes one stage clause inside a per-slice GWT card.
type GwtClause =
    { Stage: GwtStage
      Text: string }

[<RequireQualifiedAccess>]
module GwtClause =
    /// Creates a validated GWT clause for a single slice card.
    let tryCreate stage text =
        if String.IsNullOrWhiteSpace text then
            Error "GWT clauses must provide text."
        else
            Ok
                { Stage = stage
                  Text = text.Trim() }

/// Distinguishes the two current per-slice GWT purposes.
type SliceGwtKind =
    | CommandRules
    | ViewProjection

[<RequireQualifiedAccess>]
module SliceGwtKind =
    /// Returns the stable badge label for the current GWT card kind.
    let label =
        function
        | SliceGwtKind.CommandRules -> "command gwt"
        | SliceGwtKind.ViewProjection -> "view gwt"

    /// Returns the stable CSS class suffix for the current GWT card kind.
    let cssClass =
        function
        | SliceGwtKind.CommandRules -> "command"
        | SliceGwtKind.ViewProjection -> "view"

/// Describes one per-slice GWT card aligned beneath a rendered path row.
type SliceGwtCard =
    { Kind: SliceGwtKind
      Clauses: GwtClause list }

[<RequireQualifiedAccess>]
module SliceGwtCard =
    /// Creates a validated per-slice GWT card.
    let tryCreate kind clauses =
        if List.isEmpty clauses then
            Error "GWT cards must contain at least one clause."
        else
            Ok
                { Kind = kind
                  Clauses = clauses }

/// Describes one rendered GWT row aligned beneath the path slices.
type PathGwtRow =
    { Title: string
      Cards: SliceGwtCard list }

[<RequireQualifiedAccess>]
module PathGwtRow =
    /// Creates a validated GWT row for a path band.
    let tryCreate title cards =
        if String.IsNullOrWhiteSpace title then
            Error "GWT rows must provide a title."
        elif List.isEmpty cards then
            Error "GWT rows must contain at least one card."
        else
            Ok
                { Title = title.Trim()
                  Cards = cards }

/// Describes one rendered PATH row.
type PathRowState =
    { PathId: string
      Title: string
      Description: string
      SliceCards: PathSliceCard list
      GwtRows: PathGwtRow list }

[<RequireQualifiedAccess>]
module PathRowState =
    /// Creates a validated PATH row state.
    let tryCreate pathId title description sliceCards gwtRows =
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
                  SliceCards = sliceCards
                  GwtRows = gwtRows }

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

    let private gwtClause stage text =
        GwtClause.tryCreate stage text
        |> expect $"gwt clause '{text}'"

    let private gwtCard kind clauses =
        SliceGwtCard.tryCreate kind clauses
        |> expect "gwt card"

    let private gwtRow title cards =
        PathGwtRow.tryCreate title cards
        |> expect $"gwt row '{title}'"

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
            [ gwtRow
                  "PATH 1 GWT"
                  [ gwtCard
                        SliceGwtKind.CommandRules
                        [ gwtClause GwtStage.Given "no active laundry location has been captured yet"
                          gwtClause GwtStage.When "CaptureLaundryLocation is issued with manual text"
                          gwtClause GwtStage.Then "LaundryLocationCaptured becomes true" ]
                    gwtCard
                        SliceGwtKind.ViewProjection
                        [ gwtClause GwtStage.Given "LaundryLocationCaptured exists as the current location fact"
                          gwtClause GwtStage.When "the current laundry session is projected for that active location"
                          gwtClause GwtStage.Then "CurrentLaundrySession is ready with empty visible entries" ]
                    gwtCard
                        SliceGwtKind.CommandRules
                        [ gwtClause GwtStage.Given "an active location is already captured for the session"
                          gwtClause GwtStage.When "LogLaundryExpense is issued for one washer load"
                          gwtClause GwtStage.Then "LaundryExpenseLogged records the washer expense" ]
                    gwtCard
                        SliceGwtKind.ViewProjection
                        [ gwtClause GwtStage.Given "LaundryExpenseLogged includes a washer expense in the active location"
                          gwtClause GwtStage.When "the current laundry session is projected over visible entries"
                          gwtClause GwtStage.Then "CurrentLaundrySession shows the washer entry and $3.00 total" ]
                    gwtCard
                        SliceGwtKind.CommandRules
                        [ gwtClause GwtStage.Given "the current session already contains the washer expense"
                          gwtClause GwtStage.When "LogLaundryExpense is issued for one dryer load"
                          gwtClause GwtStage.Then "LaundryExpenseLogged records the dryer expense" ]
                    gwtCard
                        SliceGwtKind.ViewProjection
                        [ gwtClause GwtStage.Given "washer and dryer expenses exist in the active location window"
                          gwtClause GwtStage.When "the current laundry session is projected over visible entries"
                          gwtClause GwtStage.Then "CurrentLaundrySession shows washer and dryer with $5.50 total" ] ] ]
        |> expect "path1 row"

/// Renders deterministic HTML/CSS slice projections for LaundryLog PATH work.
[<RequireQualifiedAccess>]
module SliceHtmlRenderer =
    type private SliceCardRowHeight =
        | ScreenRow
        | DetailRow

    type private SliceCardRowContent =
        | BlockRow of SliceBlockState
        | EmptyRow

    type private SliceCardRow =
        { Height: SliceCardRowHeight
          Content: SliceCardRowContent }

    let private htmlEncode (value: string) = WebUtility.HtmlEncode value

    let private appendLine (builder: StringBuilder) (value: string) =
        builder.AppendLine(value) |> ignore

    let private renderBadge (builder: StringBuilder) cssClass text =
        appendLine builder $"<span class=\"{cssClass}\">{htmlEncode text}</span>"

    let private renderPropertyLine (builder: StringBuilder) (line: string) =
        let separator = " = "
        let separatorIndex = line.IndexOf(separator, StringComparison.Ordinal)

        if separatorIndex < 0 then
            appendLine builder $"<div class=\"slice-block__property-line\">{htmlEncode line}</div>"
        else
            let key = line.Substring(0, separatorIndex)
            let value = line.Substring(separatorIndex + separator.Length)
            let shouldStack = line.Length >= 34 || value.Length >= 22

            if shouldStack then
                appendLine builder "<div class=\"slice-block__property-line slice-block__property-line--stacked\">"
                appendLine builder $"<span class=\"slice-block__property-key\">{htmlEncode key} =</span>"
                appendLine builder $"<span class=\"slice-block__property-value\">{htmlEncode value}</span>"
                appendLine builder "</div>"
            else
                appendLine builder "<div class=\"slice-block__property-line slice-block__property-line--inline\">"
                appendLine builder $"<span class=\"slice-block__property-key\">{htmlEncode key} =</span>"
                appendLine builder $"<span class=\"slice-block__property-value\">{htmlEncode value}</span>"
                appendLine builder "</div>"

    let private screenRow content =
        { Height = SliceCardRowHeight.ScreenRow
          Content = content }

    let private detailRow content =
        { Height = SliceCardRowHeight.DetailRow
          Content = content }

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
            |> List.iter (renderPropertyLine builder)

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

    let private renderCardRow (builder: StringBuilder) options (row: SliceCardRow) =
        let rowCssClass =
            match row.Height with
            | SliceCardRowHeight.ScreenRow -> "screen"
            | SliceCardRowHeight.DetailRow -> "detail"

        appendLine builder $"<div class=\"slice-card__row slice-card__row--{rowCssClass}\">"

        match row.Content with
        | SliceCardRowContent.BlockRow blockState -> renderBlock builder options.ShowProperties blockState
        | SliceCardRowContent.EmptyRow ->
            appendLine builder "<div class=\"slice-card__slot slice-card__slot--empty slice-card__slot--row-fill\" aria-hidden=\"true\"></div>"

        appendLine builder "</div>"

    let private renderSliceCardShell
        (builder: StringBuilder)
        options
        sliceCssClass
        appName
        kindLabel
        title
        description
        rows
        =
        appendLine builder $"<article class=\"slice-card {sliceCssClass}\">"
        appendLine builder "<div class=\"slice-card__header\">"
        appendLine builder "<div class=\"slice-card__topline\">"
        renderBadge builder "slice-card__app-pill" appName
        appendLine builder $"<div class=\"slice-card__kind\">{htmlEncode kindLabel}</div>"
        appendLine builder "</div>"
        appendLine builder $"<h2 class=\"slice-card__title\">{htmlEncode title}</h2>"
        appendLine builder $"<p class=\"slice-card__description\">{htmlEncode description}</p>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"slice-card__body\">"

        rows
        |> List.iter (renderCardRow builder options)

        appendLine builder "</div>"
        appendLine builder "</article>"

    let private renderCommandSlice (builder: StringBuilder) options (sliceState: CommandSliceCardState) =
        renderSliceCardShell
            builder
            options
            "slice-card--command"
            sliceState.AppName
            "COMMAND SLICE"
            sliceState.Title
            sliceState.Description
            [ screenRow (SliceCardRowContent.BlockRow sliceState.Screen)
              detailRow (SliceCardRowContent.BlockRow sliceState.Command)
              detailRow (SliceCardRowContent.BlockRow sliceState.Event) ]

    let private renderViewSlice (builder: StringBuilder) options (sliceState: ViewSliceCardState) =
        renderSliceCardShell
            builder
            options
            "slice-card--view"
            sliceState.AppName
            "VIEW SLICE"
            sliceState.Title
            sliceState.Description
            [ if options.ShowViewScreens then
                  match sliceState.Screen with
                  | Some screen -> screenRow (SliceCardRowContent.BlockRow screen)
                  | None -> screenRow SliceCardRowContent.EmptyRow
              else
                  screenRow SliceCardRowContent.EmptyRow
              detailRow (SliceCardRowContent.BlockRow sliceState.View)
              detailRow SliceCardRowContent.EmptyRow ]

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

    let private renderGwtClause (builder: StringBuilder) (clause: GwtClause) =
        appendLine builder "<div class=\"path-document__gwt-clause\">"
        renderBadge builder "path-document__gwt-stage" (GwtStage.label clause.Stage)
        appendLine builder $"<div class=\"path-document__gwt-text\">{htmlEncode clause.Text}</div>"
        appendLine builder "</div>"

    let private renderGwtCard (builder: StringBuilder) (card: SliceGwtCard) =
        let kindCssClass = SliceGwtKind.cssClass card.Kind

        appendLine builder $"<article class=\"path-document__gwt-card path-document__gwt-card--{kindCssClass}\">"
        appendLine builder "<div class=\"path-document__gwt-topline\">"
        renderBadge builder "path-document__gwt-kind" (SliceGwtKind.label card.Kind)
        appendLine builder "</div>"
        appendLine builder "<div class=\"path-document__gwt-clauses\">"
        card.Clauses |> List.iter (renderGwtClause builder)
        appendLine builder "</div>"
        appendLine builder "</article>"

    let private renderGwtRow (builder: StringBuilder) (row: PathGwtRow) =
        appendLine builder "<section class=\"path-document__gwt-row-wrap\">"
        appendLine builder $"<h2 class=\"path-document__gwt-row-title\">{htmlEncode row.Title}</h2>"
        appendLine builder "<div class=\"path-document__gwt-row\">"
        row.Cards |> List.iter (renderGwtCard builder)
        appendLine builder "</div>"
        appendLine builder "</section>"

    let private renderStyles (builder: StringBuilder) =
        appendLine builder "<style>"
        appendLine builder ":root { color-scheme: light; }"
        appendLine builder "body { margin: 0; background: linear-gradient(180deg, #f3f5f8 0%, #e9edf3 100%); color: #0d2440; font-family: \"IBM Plex Sans\", \"Aptos\", \"Segoe UI\", sans-serif; }"
        appendLine builder ".path-document { --slice-card-width: 224px; --screen-row-height: 98px; --detail-row-height: 58px; --screen-snapshot-height: 22px; --slice-title-height: 1.34rem; --slice-description-height: 1.04rem; --property-value-indent: 1.2rem; --gwt-row-height: 108px; padding: 8px 10px 10px; }"
        appendLine builder ".path-document__header { max-width: none; margin-bottom: 8px; }"
        appendLine builder ".path-document__title { margin: 0; font-size: 1.0rem; line-height: 1.02; }"
        appendLine builder ".path-document__description { margin: 3px 0 0; max-width: none; font-size: 0.7rem; line-height: 1.2; color: #48627f; white-space: nowrap; }"
        appendLine builder ".path-document__header-actions { margin-top: 5px; display: flex; align-items: center; gap: 8px; }"
        appendLine builder ".path-document__action { border: 1px solid #9ab3d0; background: rgba(255, 255, 255, 0.92); color: #27435c; border-radius: 999px; padding: 4px 10px; font-size: 0.62rem; font-weight: 700; letter-spacing: 0.04em; cursor: pointer; }"
        appendLine builder ".path-document__action:hover { background: rgba(255, 255, 255, 1.0); }"
        appendLine builder ".path-document__row { display: grid; grid-auto-flow: column; grid-auto-columns: var(--slice-card-width); grid-template-rows: auto var(--slice-title-height) var(--slice-description-height) minmax(var(--screen-row-height), max-content) minmax(var(--detail-row-height), max-content) minmax(var(--detail-row-height), max-content); column-gap: 10px; row-gap: 6px; overflow-x: auto; align-items: start; padding: 2px 2px 6px; }"
        appendLine builder ".path-document__gwt-bands { display: flex; flex-direction: column; gap: 8px; margin-top: 10px; }"
        appendLine builder ".path-document__gwt-row-wrap { display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".path-document__gwt-row-title { margin: 0; font-size: 0.72rem; line-height: 1.1; color: #35506a; }"
        appendLine builder ".path-document__gwt-row { display: grid; grid-template-columns: repeat(var(--slice-columns), var(--slice-card-width)); column-gap: 10px; align-items: stretch; overflow-x: auto; padding: 1px 2px 2px; }"
        appendLine builder ".path-document__gwt-card { min-height: var(--gwt-row-height); border-radius: 16px; border: 2px solid #b8cadc; background: rgba(255, 255, 255, 0.84); box-shadow: 0 6px 14px rgba(10, 27, 49, 0.06); padding: 7px 8px; display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".path-document__gwt-card--command { background: rgba(226, 240, 255, 0.92); border-color: #a8c9ee; }"
        appendLine builder ".path-document__gwt-card--view { background: rgba(232, 249, 237, 0.92); border-color: #9ad7ab; }"
        appendLine builder ".path-document__gwt-topline { display: flex; justify-content: flex-start; }"
        appendLine builder ".path-document__gwt-kind { display: inline-flex; align-items: center; justify-content: center; padding: 2px 7px; border-radius: 999px; border: 1px solid #c2d4e8; background: rgba(255, 255, 255, 0.92); font-size: 0.52rem; font-weight: 700; letter-spacing: 0.08em; color: #4a647f; text-transform: lowercase; }"
        appendLine builder ".path-document__gwt-clauses { display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".path-document__gwt-clause { display: grid; grid-template-columns: auto 1fr; align-items: start; column-gap: 6px; }"
        appendLine builder ".path-document__gwt-stage { display: inline-flex; align-items: center; justify-content: center; padding: 2px 6px; border-radius: 999px; border: 1px solid #c2d4e8; background: rgba(255, 255, 255, 0.92); font-size: 0.5rem; font-weight: 700; letter-spacing: 0.08em; color: #4a647f; }"
        appendLine builder ".path-document__gwt-text { font-size: 0.62rem; line-height: 1.22; color: #0f2740; }"
        appendLine builder ".slice-card { min-height: 0; border-radius: 20px; border: 4px solid #15263d; box-shadow: 0 10px 24px rgba(10, 27, 49, 0.1); padding: 7px 7px 8px; display: grid; grid-template-rows: subgrid; grid-row: 1 / span 6; align-content: start; }"
        appendLine builder ".slice-card--command { background: linear-gradient(180deg, #dff1ff 0%, #eff7ff 100%); }"
        appendLine builder ".slice-card--view { background: linear-gradient(180deg, #dbfae4 0%, #effbf3 100%); }"
        appendLine builder ".slice-card__header, .slice-card__body { display: contents; }"
        appendLine builder ".slice-card__topline { grid-row: 1; display: flex; align-items: center; justify-content: space-between; gap: 10px; }"
        appendLine builder ".slice-card__app-pill { display: inline-flex; align-items: center; justify-content: center; padding: 3px 9px; border-radius: 999px; background: #ffb54d; color: white; font-size: 0.62rem; font-weight: 700; line-height: 1; }"
        appendLine builder ".slice-card__kind { color: #0e5883; font-size: 0.58rem; font-weight: 700; letter-spacing: 0.14em; text-transform: uppercase; text-align: right; }"
        appendLine builder ".slice-card__title { grid-row: 2; margin: 3px 0 0; font-size: 0.82rem; line-height: 1.04; overflow: hidden; }"
        appendLine builder ".slice-card__description { grid-row: 3; margin: 0; font-size: 0.66rem; line-height: 1.18; color: #506b87; overflow: hidden; }"
        appendLine builder ".slice-card__row { display: flex; flex-direction: column; min-height: 0; }"
        appendLine builder ".slice-card__row--screen { grid-row: 4; }"
        appendLine builder ".slice-card__body .slice-card__row:nth-of-type(2) { grid-row: 5; }"
        appendLine builder ".slice-card__body .slice-card__row:nth-of-type(3) { grid-row: 6; }"
        appendLine builder ".slice-card__row > .slice-block { flex: 1; }"
        appendLine builder ".slice-card__slot--empty { min-height: 0; border-radius: 16px; background: transparent; }"
        appendLine builder ".slice-card__slot--row-fill { flex: 1; }"
        appendLine builder ".slice-block { display: flex; flex-direction: column; gap: 3px; border-radius: 14px; padding: 6px 7px; border: 2px solid; min-height: 0; overflow: visible; }"
        appendLine builder ".slice-block--screen { min-height: var(--screen-row-height); box-sizing: border-box; background: rgba(255, 255, 255, 0.78); border-color: #c8dcff; }"
        appendLine builder ".slice-block--command { min-height: var(--detail-row-height); box-sizing: border-box; background: rgba(74, 150, 255, 0.18); border-color: #8bc3ff; }"
        appendLine builder ".slice-block--event { min-height: var(--detail-row-height); box-sizing: border-box; background: rgba(255, 167, 57, 0.2); border-color: #ffb777; }"
        appendLine builder ".slice-block--view { min-height: var(--detail-row-height); box-sizing: border-box; background: rgba(103, 229, 130, 0.18); border-color: #78e39d; }"
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
        appendLine builder ".slice-block__property-line { font-family: \"IBM Plex Mono\", \"Cascadia Mono\", \"Consolas\", monospace; font-size: 0.58rem; line-height: 1.18; color: #27435c; }"
        appendLine builder ".slice-block__property-line--inline { display: flex; flex-wrap: wrap; gap: 0.3rem; }"
        appendLine builder ".slice-block__property-line--stacked { display: flex; flex-direction: column; }"
        appendLine builder ".slice-block__property-key { white-space: nowrap; }"
        appendLine builder ".slice-block__property-value { overflow-wrap: break-word; word-break: normal; }"
        appendLine builder ".slice-block__property-line--stacked .slice-block__property-value { padding-left: var(--property-value-indent); }"
        appendLine builder ".slice-block__footer { display: flex; justify-content: flex-end; margin-top: auto; }"
        appendLine builder ".slice-block__footer-badge { display: inline-flex; align-items: center; justify-content: center; padding: 2px 7px; border-radius: 999px; border: 1px solid #c6d4e2; background: rgba(255, 255, 255, 0.94); color: #566d86; font-size: 0.52rem; font-weight: 700; letter-spacing: 0.08em; text-transform: lowercase; white-space: nowrap; }"
        appendLine builder "@media (max-width: 1200px) { .path-document { --slice-card-width: 216px; padding-left: 8px; padding-right: 8px; } }"
        appendLine builder "@media (max-width: 900px) { .path-document { --slice-card-width: 208px; } .path-document__row, .path-document__gwt-row { column-gap: 8px; } }"
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
        appendLine builder $"<main class=\"path-document\" style=\"--slice-columns: {pathRow.SliceCards.Length};\">"
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

        if not (List.isEmpty pathRow.GwtRows) then
            appendLine builder "<section class=\"path-document__gwt-bands\">"
            pathRow.GwtRows |> List.iter (renderGwtRow builder)
            appendLine builder "</section>"

        appendLine builder "</main>"
        renderScript builder
        appendLine builder "</body>"
        appendLine builder "</html>"

        builder.ToString()
