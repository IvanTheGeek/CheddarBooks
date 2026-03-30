namespace CheddarBooks.LaundryLog.UI

open System
open System.Net
open System.Text
open System.Text.Json

/// Groups the current NM contexts into broader color families.
type ContextGroupKind =
    | AppRuntime
    | Interaction
    | Business

[<RequireQualifiedAccess>]
module ContextGroupKind =
    let label =
        function
        | AppRuntime -> "App Runtime"
        | Interaction -> "Interaction"
        | Business -> "Business"

    let domKey =
        function
        | AppRuntime -> "app-runtime"
        | Interaction -> "interaction"
        | Business -> "business"

/// Distinguishes the current bounded contexts visible in the NM path.
type NmContextKind =
    | ApplicationLifecycle
    | RuntimeOrchestration
    | ScreenPath
    | EventModeling

[<RequireQualifiedAccess>]
module NmContextKind =
    let domKey =
        function
        | ApplicationLifecycle -> "application-lifecycle"
        | RuntimeOrchestration -> "runtime-orchestration"
        | ScreenPath -> "screen-path"
        | EventModeling -> "event-modeling"

    let label =
        function
        | ApplicationLifecycle -> "ApplicationLifecycle"
        | RuntimeOrchestration -> "RuntimeOrchestration"
        | ScreenPath -> "ScreenPath"
        | EventModeling -> "EventModeling"

/// Distinguishes the first lens families carried by the NM path.
type NmLensKind =
    | Lifecycle
    | Runtime
    | Screen
    | Aem

[<RequireQualifiedAccess>]
module NmLensKind =
    let domKey =
        function
        | Lifecycle -> "lifecycle"
        | Runtime -> "runtime"
        | Screen -> "screen"
        | Aem -> "aem"

    let label =
        function
        | Lifecycle -> "Lifecycle"
        | Runtime -> "Runtime"
        | Screen -> "Screen"
        | Aem -> "AEM"

/// Distinguishes how the top surface previews should be presented across the page.
type NmSurfacePresentation =
    | Thumbnail
    | Full

[<RequireQualifiedAccess>]
module NmSurfacePresentation =
    let domKey =
        function
        | Thumbnail -> "thumbnail"
        | Full -> "full"

    let label =
        function
        | Thumbnail -> "Thumbnail"
        | Full -> "Full"

/// Distinguishes the surface shell variants carried by NM columns.
type NmColumnSurfaceState =
    | NmBootSurface of AppBootScreenState
    | NmAppScreenSurface of ScreenSurfaceState
    | NmAemSliceSurface of topSurface: ScreenSurfaceState option * sliceCard: PathSliceCard

/// Describes one explicit ordered NM column.
type NmColumnState =
    { ColumnKey: string
      ColumnTitle: string
      ColumnNote: string option
      ChangeItems: string list
      ContextGroup: ContextGroupKind
      PrimaryContext: NmContextKind
      VisibleInLenses: NmLensKind list
      ActorRoleBadge: string option
      Surface: NmColumnSurfaceState
      TechnicalSurfaceLabel: string option
      HumanSurfaceTitle: string }

/// Describes one complete NM path document.
type NmPathState =
    { PathId: string
      Title: string
      Description: string
      ScenarioLabel: string
      Assumptions: string list
      Columns: NmColumnState list }

/// Carries self-update metadata for a generated NM path artifact.
type NmPathUpdateState =
    { Version: string
      UpdatedAtUtc: string
      PollIntervalMs: int }

[<RequireQualifiedAccess>]
module NmPathHtmlExamples =
    let private getScreenPathState () = ScreenPathHtmlExamples.path1StartupToFirstEntry ()
    let private getAemPathState () = SliceHtmlExamples.path1ManualLocationWasherDryer ()

    let private screenPathStep stepKey =
        getScreenPathState().Steps
        |> List.find (fun stepState -> stepState.StepKey = stepKey)

    let private bootSurface stepKey =
        match (screenPathStep stepKey).Surface with
        | AppBootSurface bootState -> bootState
        | _ -> failwith $"Expected '{stepKey}' to carry an app boot surface."

    let private appScreenSurface stepKey =
        match (screenPathStep stepKey).Surface with
        | AppScreenSurface screenSurface -> screenSurface
        | _ -> failwith $"Expected '{stepKey}' to carry an app screen surface."

    let private aemSliceCard index =
        match getAemPathState().SliceCards |> List.tryItem index with
        | Some sliceCard -> sliceCard
        | None -> failwith $"Expected AEM slice card index {index} in PATH1."

    let private technicalLabel =
        function
        | NmBootSurface bootState -> Some bootState.SurfaceName
        | NmAppScreenSurface screenSurface -> Some(ScreenSurfaceState.title screenSurface)
        | NmAemSliceSurface (Some topSurface, _) -> Some(ScreenSurfaceState.title topSurface)
        | NmAemSliceSurface (None, PathSliceCard.CommandSlice sliceState) -> Some $"CommandSlice.{sliceState.Title}"
        | NmAemSliceSurface (None, PathSliceCard.ViewSlice sliceState) -> Some $"ViewSlice.{sliceState.Title}"

    let private column
        columnKey
        columnTitle
        columnNote
        changeItems
        contextGroup
        primaryContext
        visibleInLenses
        actorRoleBadge
        surface
        humanSurfaceTitle
        =
        { ColumnKey = columnKey
          ColumnTitle = columnTitle
          ColumnNote = columnNote
          ChangeItems = changeItems
          ContextGroup = contextGroup
          PrimaryContext = primaryContext
          VisibleInLenses = visibleInLenses
          ActorRoleBadge = actorRoleBadge
          Surface = surface
          TechnicalSurfaceLabel = technicalLabel surface
          HumanSurfaceTitle = humanSurfaceTitle }

    /// Returns the first ATLAS/NM proving-ground path for LaundryLog.
    let path1FirstLaunchFirstEntry () : NmPathState =
        { PathId = "PATH1-NM"
          Title = "PATH 1 NM: Fresh First Launch -> Need Location -> First Entry"
          Description =
            "First ATLAS/NM path surface for LaundryLog. It weaves application lifecycle, runtime orchestration, screen-path state, and AEM business slices into one ordered column flow."
          ScenarioLabel = "fresh first launch with no known local data"
          Assumptions =
            [ "No saved location is available yet."
              "No active laundry session or pending draft exists."
              "Startup/runtime checks must finish before the first usable screen appears."
              "The initial route should resolve to Need Location before the first entry is composed."
              "Business-state changes should appear as their own AEM columns rather than being collapsed into the neighboring screen columns." ]
          Columns =
            [ column
                  "01-app-started"
                  "AppStarted"
                  (Some "The user launches the app and the first splash-state surface becomes visible.")
                  [ "The splash screen becomes visible as the first surface."
                    "The AppStarted checkpoint becomes active."
                    "No runtime or route checkpoints are complete yet." ]
                  ContextGroupKind.AppRuntime
                  NmContextKind.ApplicationLifecycle
                  [ NmLensKind.Lifecycle ]
                  (Some "System")
                  (NmBootSurface(bootSurface "01-app-started"))
                  "Splash Screen"
              column
                  "02-runtime-checks"
                  "Runtime Checks"
                  (Some "Startup/runtime checks begin while the splash surface remains visible.")
                  [ "The splash message changes to startup checking."
                    "AppStarted becomes complete."
                    "Runtime checks becomes the active checkpoint." ]
                  ContextGroupKind.AppRuntime
                  NmContextKind.RuntimeOrchestration
                  [ NmLensKind.Runtime ]
                  (Some "System")
                  (NmBootSurface(bootSurface "02-runtime-checks"))
                  "Splash Screen"
              column
                  "03-no-local-session"
                  "No Local Session"
                  (Some "Fresh-first-launch assumptions are confirmed from local/runtime inspection.")
                  [ "Runtime checks become complete."
                    "No known local session becomes the active checkpoint."
                    "The splash message confirms there is no saved location, active session, or pending draft." ]
                  ContextGroupKind.AppRuntime
                  NmContextKind.RuntimeOrchestration
                  [ NmLensKind.Runtime ]
                  (Some "System")
                  (NmBootSurface(bootSurface "03-no-local-session"))
                  "Splash Screen"
              column
                  "04-route-resolved"
                  "Route Resolved"
                  (Some "Runtime orchestration resolves Need Location as the first usable screen.")
                  [ "All earlier startup checkpoints are complete."
                    "Route to Need Location becomes the active checkpoint."
                    "The splash message changes from checking to entering the app." ]
                  ContextGroupKind.AppRuntime
                  NmContextKind.RuntimeOrchestration
                  [ NmLensKind.Runtime ]
                  (Some "System")
                  (NmBootSurface(bootSurface "04-route-resolved"))
                  "Splash Screen"
              column
                  "05-need-location"
                  "Need Location"
                  (Some "Fresh start with no known location yet.")
                  [ "The first usable screen replaces the splash surface."
                    "The location field is empty."
                    "Set Location remains disabled until a location is entered." ]
                  ContextGroupKind.Interaction
                  NmContextKind.ScreenPath
                  [ NmLensKind.Screen ]
                  (Some "User")
                  (NmAppScreenSurface(appScreenSurface "05-need-location"))
                  "Set Location Screen"
              column
                  "06-ready-to-set-location"
                  "Ready To Set Location"
                  (Some "The user has entered location text and the confirm action is now available.")
                  [ "The location input now contains the chosen text."
                    "Set Location becomes enabled."
                    "The rest of the expense form is still not visible yet." ]
                  ContextGroupKind.Interaction
                  NmContextKind.ScreenPath
                  [ NmLensKind.Screen ]
                  (Some "User")
                  (NmAppScreenSurface(appScreenSurface "06-ready-to-set-location"))
                  "Set Location Screen"
              column
                  "07-capture-laundry-location"
                  "CaptureLaundryLocation"
                  (Some "The business command for location capture becomes explicit in the NM flow.")
                  [ "The entered location draft is now expressed as a command."
                    "The user action is represented in business terms, not only as screen state."
                    "A successful command is expected to produce LaundryLocationCaptured." ]
                  ContextGroupKind.Business
                  NmContextKind.EventModeling
                  [ NmLensKind.Aem ]
                  (Some "User")
                  (NmAemSliceSurface(Some(appScreenSurface "06-ready-to-set-location"), aemSliceCard 0))
                  "Set Location Screen"
              column
                  "08-current-laundry-session-location"
                  "CurrentLaundrySession"
                  (Some "The first business projection after location capture is now explicit.")
                  [ "The current session view now carries the active location."
                    "No visible laundry entries exist yet."
                    "The app can enter the ready-to-log entry form with the business view now established." ]
                  ContextGroupKind.Business
                  NmContextKind.EventModeling
                  [ NmLensKind.Aem ]
                  None
                  (NmAemSliceSurface(Some(appScreenSurface "07-entry-form-ready"), aemSliceCard 1))
                  "Laundry Entry Screen"
              column
                  "09-entry-form-ready"
                  "Entry Form Ready"
                  (Some "The app enters the main entry surface after the location is captured.")
                  [ "The main entry screen replaces the location-only screen."
                    "Machine, quantity, price, and payment controls become visible."
                    "Log Expense is still blocked until the required entry choices are complete." ]
                  ContextGroupKind.Interaction
                  NmContextKind.ScreenPath
                  [ NmLensKind.Screen ]
                  (Some "User")
                  (NmAppScreenSurface(appScreenSurface "07-entry-form-ready"))
                  "Laundry Entry Screen"
              column
                  "10-washer-draft"
                  "Washer Draft"
                  (Some "The first expense draft is composed on the main screen.")
                  [ "Washer becomes the selected machine."
                    "Card payment details are expanded."
                    "The draft is complete enough for Log Expense to become available." ]
                  ContextGroupKind.Interaction
                  NmContextKind.ScreenPath
                  [ NmLensKind.Screen ]
                  (Some "User")
                  (NmAppScreenSurface(appScreenSurface "08-washer-draft"))
                  "Laundry Entry Screen"
              column
                  "11-log-laundry-expense"
                  "LogLaundryExpense"
                  (Some "The first washer expense draft is now explicit as a business command.")
                  [ "The selected machine, quantity, price, and payment are now represented as command data."
                    "The washer draft is pressure-tested from a business-event perspective."
                    "A successful command is expected to produce LaundryExpenseLogged." ]
                  ContextGroupKind.Business
                  NmContextKind.EventModeling
                  [ NmLensKind.Aem ]
                  (Some "User")
                  (NmAemSliceSurface(Some(appScreenSurface "08-washer-draft"), aemSliceCard 2))
                  "Laundry Entry Screen"
              column
                  "12-current-laundry-session-washer"
                  "CurrentLaundrySession"
                  (Some "The business projection now shows the first logged washer entry.")
                  [ "The current session view now includes the first visible washer entry."
                    "The running total changes to reflect the first expense."
                    "The app is now ready for the next quick entry." ]
                  ContextGroupKind.Business
                  NmContextKind.EventModeling
                  [ NmLensKind.Aem ]
                  None
                  (NmAemSliceSurface(Some(appScreenSurface "09-logged-success"), aemSliceCard 3))
                  "Laundry Entry Screen"
              column
                  "13-logged-success"
                  "Logged Success"
                  (Some "The first entry is logged and the surface is ready for the next quick entry.")
                  [ "A logged-success confirmation appears."
                    "Session total updates to the new amount."
                    "Today's entries now shows logged items for continued quick entry." ]
                  ContextGroupKind.Interaction
                  NmContextKind.ScreenPath
                  [ NmLensKind.Screen ]
                  (Some "User")
                  (NmAppScreenSurface(appScreenSurface "09-logged-success"))
                  "Laundry Entry Screen" ] }

/// Renders deterministic HTML/CSS NM path documents from the current LaundryLog surfaces.
[<RequireQualifiedAccess>]
module NmPathHtmlRenderer =
    let private htmlEncode (value: string) = WebUtility.HtmlEncode value
    let private jsonString (value: string) = JsonSerializer.Serialize value
    let private domSlug (value: string) = value.Trim().ToLowerInvariant().Replace(" ", "-")

    let private appendLine (builder: StringBuilder) (value: string) =
        builder.AppendLine(value) |> ignore

    let private defaultUpdateState () =
        let now = DateTime.UtcNow

        { Version = $"nm-path::{now:yyyyMMddHHmmssfff}"
          UpdatedAtUtc = now.ToString("yyyy-MM-ddTHH:mm:ssZ")
          PollIntervalMs = 3000 }

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

    let private renderBootSurfaceHtml (bootState: AppBootScreenState) =
        let builder = StringBuilder()
        appendLine builder "<article class=\"ll-screen-surface\">"
        appendLine builder $"<div class=\"ll-screen-surface__name\">{htmlEncode bootState.SurfaceName}</div>"

        match bootState.Note with
        | Some noteText -> appendLine builder $"<p class=\"ll-screen-surface__note\">{htmlEncode noteText}</p>"
        | None -> ()

        appendLine builder "<section class=\"ll-phone-screen ll-phone-screen--boot\">"

        if bootState.ShowHeader then
            appendLine builder "<header class=\"ll-header\">"
            appendLine builder "<div class=\"ll-header__text\">"
            appendLine builder $"<h2 class=\"ll-header__title\">🧺 {htmlEncode bootState.Header.Title}</h2>"
            appendLine builder "<p class=\"ll-header__subtitle\">by CheddarBooks</p>"
            appendLine builder "</div>"
            appendLine builder "<div class=\"ll-header__badge\">🧀</div>"
            appendLine builder "</header>"

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
        builder.ToString()

    let private lensDomKeys (columnState: NmColumnState) =
        columnState.VisibleInLenses
        |> List.map NmLensKind.domKey
        |> String.concat ","

    let private columnKindLabel =
        function
        | { Surface = NmBootSurface _ } -> "BOOT"
        | { Surface = NmAppScreenSurface _ } -> "SCREEN"
        | { Surface = NmAemSliceSurface (_, PathSliceCard.CommandSlice _) } -> "COMMAND"
        | { Surface = NmAemSliceSurface (_, PathSliceCard.ViewSlice _) } -> "VIEW"

    let private columnKindDomKey =
        function
        | { Surface = NmBootSurface _ } -> "boot"
        | { Surface = NmAppScreenSurface _ } -> "screen"
        | { Surface = NmAemSliceSurface (_, PathSliceCard.CommandSlice _) } -> "command"
        | { Surface = NmAemSliceSurface (_, PathSliceCard.ViewSlice _) } -> "view"

    let private nonAemDetailKindLabel =
        function
        | { PrimaryContext = NmContextKind.ApplicationLifecycle } -> "TRANSITION"
        | { PrimaryContext = NmContextKind.RuntimeOrchestration } -> "ORCHESTRATION"
        | { PrimaryContext = NmContextKind.ScreenPath } -> "INTERACTION"
        | { PrimaryContext = NmContextKind.EventModeling } -> "STATE"

    let private screenBoxTitle (columnState: NmColumnState) = columnState.HumanSurfaceTitle

    let private screenBoxNote =
        function
        | { PrimaryContext = NmContextKind.ApplicationLifecycle } ->
            "The visible boot surface linked to this lifecycle step."
        | { PrimaryContext = NmContextKind.RuntimeOrchestration } ->
            "The visible boot surface linked to this runtime orchestration step."
        | { PrimaryContext = NmContextKind.ScreenPath } ->
            "The user-visible screen surface linked to this path step."
        | { PrimaryContext = NmContextKind.EventModeling } ->
            "The linked app surface that frames this Event Modeling slice."

    let private groupMeaning =
        function
        | ContextGroupKind.AppRuntime -> "App Runtime covers startup, lifecycle, route choice, and other app/system mechanics."
        | ContextGroupKind.Interaction -> "Interaction covers user-visible screens and UI progression through the path."
        | ContextGroupKind.Business -> "Business covers commands, events, and views that express the business truth."

    let private groupWhyThisColumn (columnState: NmColumnState) =
        match columnState.ContextGroup with
        | ContextGroupKind.AppRuntime ->
            $"This column is in App Runtime because {columnState.ColumnTitle} is driven by app/system startup or route mechanics."
        | ContextGroupKind.Interaction ->
            $"This column is in Interaction because {columnState.ColumnTitle} is represented as a user-visible screen or interaction step."
        | ContextGroupKind.Business ->
            $"This column is in Business because {columnState.ColumnTitle} is expressed in Event Modeling terms rather than only as UI state."

    let private contextMeaning =
        function
        | NmContextKind.ApplicationLifecycle ->
            "ApplicationLifecycle owns the meaning of app phase changes like start, resume, and suspend."
        | NmContextKind.RuntimeOrchestration ->
            "RuntimeOrchestration owns startup checks, route resolution, and coordination between app/runtime and the business flow."
        | NmContextKind.ScreenPath ->
            "ScreenPath owns the ordered user-visible screen states in the path."
        | NmContextKind.EventModeling ->
            "EventModeling owns commands, events, and views that express business change and business state."

    let private contextWhyThisColumn (columnState: NmColumnState) =
        match columnState.PrimaryContext with
        | NmContextKind.ApplicationLifecycle ->
            $"This column is in ApplicationLifecycle because {columnState.ColumnTitle} marks an app lifecycle phase becoming visible."
        | NmContextKind.RuntimeOrchestration ->
            $"This column is in RuntimeOrchestration because {columnState.ColumnTitle} coordinates checks, route choice, or state handoff."
        | NmContextKind.ScreenPath ->
            $"This column is in ScreenPath because {columnState.ColumnTitle} is a user-visible screen or interaction state."
        | NmContextKind.EventModeling ->
            $"This column is in EventModeling because {columnState.ColumnTitle} is represented as a business command or business view."

    let private lensMeaning =
        function
        | NmLensKind.Lifecycle -> "Lifecycle lens focuses on app phase changes."
        | NmLensKind.Runtime -> "Runtime lens focuses on orchestration and app mechanics."
        | NmLensKind.Screen -> "Screen lens focuses on user-visible UI progression."
        | NmLensKind.Aem -> "AEM lens focuses on business commands, events, and views."

    let private lensWhyThisColumn (columnState: NmColumnState) =
        function
        | NmLensKind.Lifecycle ->
            $"This column appears in the Lifecycle lens because {columnState.ColumnTitle} contributes to the app lifecycle story."
        | NmLensKind.Runtime ->
            $"This column appears in the Runtime lens because {columnState.ColumnTitle} contributes to app/runtime coordination."
        | NmLensKind.Screen ->
            $"This column appears in the Screen lens because {columnState.ColumnTitle} contributes to the user-visible path."
        | NmLensKind.Aem ->
            $"This column appears in the AEM lens because {columnState.ColumnTitle} is represented through Event Modeling."

    let private roleMeaning =
        function
        | "System" -> "Actor · System means the app/runtime is acting at this point in the path."
        | "User" -> "Actor · User means the human is acting through the UI at this point in the path."
        | roleText -> $"Actor · {roleText} is the acting party represented in this column."

    let private roleWhyThisColumn (columnState: NmColumnState) =
        function
        | "System" -> $"This actor badge is System because {columnState.ColumnTitle} happens through app/runtime work rather than direct user input."
        | "User" -> $"This actor badge is User because {columnState.ColumnTitle} depends on or expresses a direct user action."
        | roleText -> $"This actor badge is {roleText} because that actor is the relevant participant in {columnState.ColumnTitle}."

    let private selectedChoiceLabels (optionGroup: OptionGroupState) =
        optionGroup.Choices
        |> List.filter (fun choice -> choice.IsSelected)
        |> List.map (fun choice -> choice.Label)

    let private locationPreviewText (textInput: TextInputState) =
        textInput.ValueText |> Option.defaultValue textInput.PlaceholderText

    let private pricePreviewText (priceInput: MoneyInputState) =
        priceInput.ValueText |> Option.defaultValue priceInput.PlaceholderText

    let private entryPreviewTotal (screenState: EntryFormPrimitiveState) =
        if String.IsNullOrWhiteSpace screenState.SessionTotal.ValueText then
            pricePreviewText screenState.PriceInput
        else
            screenState.SessionTotal.ValueText

    let private renderThumbnailChoicePill (builder: StringBuilder) isSelected label =
        let selectedClass = if isSelected then " nm-thumbnail__choice-pill--selected" else ""
        appendLine builder $"<span class=\"nm-thumbnail__choice-pill{selectedClass}\">{htmlEncode label}</span>"

    let private renderBootThumbnail (builder: StringBuilder) (bootState: AppBootScreenState) =
        appendLine builder "<div class=\"nm-thumbnail nm-thumbnail--boot\" data-testid=\"nm-surface-thumbnail\">"
        appendLine builder "<div class=\"nm-thumbnail__device\">"
        appendLine builder "<div class=\"nm-thumbnail__device-top\"></div>"
        appendLine builder "<div class=\"nm-thumbnail__hero\">"
        appendLine builder "<div class=\"nm-thumbnail__hero-icon\">🧺</div>"
        appendLine builder $"<div class=\"nm-thumbnail__hero-title\">{htmlEncode bootState.PrimaryMessage}</div>"
        appendLine builder $"<div class=\"nm-thumbnail__hero-subtitle\">{htmlEncode bootState.SecondaryMessage}</div>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-thumbnail__checkpoint-stack\">"

        bootState.BootChecks
        |> List.truncate 4
        |> List.iter (fun checkState ->
            let statusClass =
                match checkState.Status with
                | Pending -> " nm-thumbnail__checkpoint--pending"
                | Active -> " nm-thumbnail__checkpoint--active"
                | Complete -> " nm-thumbnail__checkpoint--complete"

            appendLine builder $"<div class=\"nm-thumbnail__checkpoint{statusClass}\"></div>")

        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderNewSessionThumbnail (builder: StringBuilder) (screenState: NewSessionPrimitiveState) =
        let locationText = locationPreviewText screenState.LocationInput
        let locationClass = if screenState.LocationInput.ValueText.IsSome then " nm-thumbnail__field--filled" else ""
        let actionClass =
            if screenState.SetLocationAction.IsEnabled then
                " nm-thumbnail__button--enabled"
            else
                " nm-thumbnail__button--disabled"

        appendLine builder "<div class=\"nm-thumbnail nm-thumbnail--new-session\" data-testid=\"nm-surface-thumbnail\">"
        appendLine builder "<div class=\"nm-thumbnail__device\">"
        appendLine builder "<div class=\"nm-thumbnail__device-top\"></div>"
        appendLine builder "<div class=\"nm-thumbnail__app-bar\"></div>"
        appendLine builder "<div class=\"nm-thumbnail__screen-body\">"
        appendLine builder "<div class=\"nm-thumbnail__panel\">"
        appendLine builder $"<div class=\"nm-thumbnail__field{locationClass}\">{htmlEncode locationText}</div>"
        appendLine builder "<div class=\"nm-thumbnail__field-action\">📍</div>"
        appendLine builder "</div>"
        appendLine builder $"<div class=\"nm-thumbnail__button{actionClass}\">{htmlEncode screenState.SetLocationAction.Label}</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderEntryFormThumbnail (builder: StringBuilder) (screenState: EntryFormPrimitiveState) =
        let selectedMachine =
            selectedChoiceLabels screenState.MachineTypeOptions
            |> List.tryHead

        let orderedMachineChoices =
            screenState.MachineTypeOptions.Choices
            |> List.sortByDescending (fun choice -> choice.IsSelected)
            |> List.truncate 3

        let totalText = entryPreviewTotal screenState
        let priceText = pricePreviewText screenState.PriceInput
        let quantityText = screenState.QuantityStepper.ValueText
        let feedbackClass = if screenState.FeedbackBanner.IsSome then " nm-thumbnail__total-bar--success" else ""
        let selectedMachineText = selectedMachine |> Option.defaultValue "Entry"

        appendLine builder "<div class=\"nm-thumbnail nm-thumbnail--entry-form\" data-testid=\"nm-surface-thumbnail\">"
        appendLine builder "<div class=\"nm-thumbnail__device\">"
        appendLine builder "<div class=\"nm-thumbnail__device-top\"></div>"
        appendLine builder "<div class=\"nm-thumbnail__app-bar\"></div>"
        appendLine builder "<div class=\"nm-thumbnail__screen-body\">"

        match screenState.LocationInput with
        | Some locationInput ->
            appendLine builder $"<div class=\"nm-thumbnail__location-pill\">{htmlEncode (locationPreviewText locationInput)}</div>"
        | None -> ()

        appendLine builder "<div class=\"nm-thumbnail__choice-row\">"

        orderedMachineChoices
        |> List.iter (fun choice -> renderThumbnailChoicePill builder choice.IsSelected choice.Label)

        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-thumbnail__metric-row\">"
        appendLine builder $"<div class=\"nm-thumbnail__metric-box\">Qty {htmlEncode quantityText}</div>"
        appendLine builder $"<div class=\"nm-thumbnail__metric-box\">{htmlEncode priceText}</div>"
        appendLine builder "</div>"
        appendLine builder $"<div class=\"nm-thumbnail__total-bar{feedbackClass}\">"
        appendLine builder $"<span>{htmlEncode selectedMachineText}</span>"
        appendLine builder $"<strong>{htmlEncode totalText}</strong>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderStaticSurfaceThumbnail (builder: StringBuilder) =
        function
        | NmBootSurface bootState -> renderBootThumbnail builder bootState
        | NmAppScreenSurface (NewSessionScreen (_, _, screenState)) -> renderNewSessionThumbnail builder screenState
        | NmAppScreenSurface (EntryFormScreen (_, _, screenState)) -> renderEntryFormThumbnail builder screenState
        | NmAemSliceSurface (Some(NewSessionScreen (_, _, screenState)), _) -> renderNewSessionThumbnail builder screenState
        | NmAemSliceSurface (Some(EntryFormScreen (_, _, screenState)), _) -> renderEntryFormThumbnail builder screenState
        | NmAemSliceSurface (None, _) ->
            appendLine builder "<div class=\"nm-thumbnail nm-thumbnail--empty\" data-testid=\"nm-surface-thumbnail\"><div class=\"nm-thumbnail__empty\">No screen linked</div></div>"

    let private renderSurfaceFragment =
        function
        | NmBootSurface bootState -> renderBootSurfaceHtml bootState
        | NmAppScreenSurface screenSurface -> ScreenHtmlRenderer.renderSurfaceHtml screenSurface
        | NmAemSliceSurface (Some topSurface, _) -> ScreenHtmlRenderer.renderSurfaceHtml topSurface
        | NmAemSliceSurface (None, _) -> "<div class=\"nm-column__surface-empty\">No screen surface linked.</div>"

    let private surfaceNativeSize =
        function
        | NmBootSurface _ -> 360, 667
        | NmAppScreenSurface (NewSessionScreen _) -> 360, 667
        | NmAppScreenSurface (EntryFormScreen _) -> 360, 980
        | NmAemSliceSurface (Some(NewSessionScreen _), _) -> 360, 667
        | NmAemSliceSurface (Some(EntryFormScreen _), _) -> 360, 980
        | NmAemSliceSurface (None, _) -> 360, 667

    let private surfaceSizingStyle surfaceState =
        let nativeWidth, nativeHeight = surfaceNativeSize surfaceState
        $" style=\"--nm-surface-native-width: {nativeWidth}px; --nm-surface-native-height: {nativeHeight}px;\""

    let private renderBadgePopover (builder: StringBuilder) columnKey pillType label meaning whyThisColumn modifierClass =
        let popoverId = $"nm-pill-popover-{columnKey}-{pillType}-{domSlug label}"
        let badgeCategoryLabel =
            match pillType with
            | "group" -> "Context Group badge"
            | "context" -> "Bounded Context badge"
            | "lens"
            | "screen-lens" -> "Lens badge"
            | "role"
            | "screen-role" -> "Actor badge"
            | otherType -> $"{otherType} badge"

        appendLine builder $"<div class=\"nm-column__pill-wrap\" data-testid=\"nm-column-pill-wrap\" data-pill-type=\"{htmlEncode pillType}\">"
        appendLine
            builder
            $"<button class=\"nm-column__badge {modifierClass}\" type=\"button\" data-testid=\"nm-column-pill\" data-pill-type=\"{htmlEncode pillType}\" data-pill-label=\"{htmlEncode label}\" aria-describedby=\"{htmlEncode popoverId}\">{htmlEncode label}</button>"
        appendLine
            builder
            $"<div id=\"{htmlEncode popoverId}\" class=\"nm-column__pill-popover\" data-testid=\"nm-column-pill-popover\" data-pill-type=\"{htmlEncode pillType}\" role=\"tooltip\">"
        appendLine builder "<div class=\"nm-column__pill-popover-header\">"
        appendLine builder "<div class=\"nm-column__pill-popover-heading\">"
        appendLine builder $"<div class=\"nm-column__pill-popover-label\">{htmlEncode badgeCategoryLabel}</div>"
        appendLine builder $"<div class=\"nm-column__pill-popover-title\">{htmlEncode label}</div>"
        appendLine builder "</div>"
        appendLine builder "<button class=\"nm-column__pill-popover-close\" type=\"button\" data-testid=\"nm-column-pill-popover-close\" aria-label=\"Close explanation\">Close</button>"
        appendLine builder "</div>"
        appendLine builder $"<p class=\"nm-column__pill-popover-text\">{htmlEncode meaning}</p>"
        appendLine builder $"<p class=\"nm-column__pill-popover-text\">{htmlEncode whyThisColumn}</p>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderClassificationRow (builder: StringBuilder) (columnState: NmColumnState) =
        appendLine builder "<div class=\"nm-column__classification\">"
        appendLine builder "<div class=\"nm-column__classification-row nm-column__classification-row--primary\">"
        appendLine builder "<div class=\"nm-column__meta nm-column__meta--primary\" data-testid=\"nm-column-meta\">"

        renderBadgePopover
            builder
            columnState.ColumnKey
            "group"
            (ContextGroupKind.label columnState.ContextGroup)
            (groupMeaning columnState.ContextGroup)
            (groupWhyThisColumn columnState)
            "nm-column__badge--group"

        renderBadgePopover
            builder
            columnState.ColumnKey
            "context"
            (NmContextKind.label columnState.PrimaryContext)
            (contextMeaning columnState.PrimaryContext)
            (contextWhyThisColumn columnState)
            "nm-column__badge--context"

        appendLine builder "</div>"
        appendLine builder $"<span class=\"nm-column__kind\" data-testid=\"nm-column-kind\">{htmlEncode (columnKindLabel columnState)}</span>"
        appendLine builder "</div>"

        appendLine builder "<div class=\"nm-column__classification-row nm-column__classification-row--secondary\">"
        appendLine builder "<div class=\"nm-column__meta nm-column__meta--secondary\">"

        columnState.VisibleInLenses
        |> List.iter (fun lensKind ->
            renderBadgePopover
                builder
                columnState.ColumnKey
                "lens"
                (NmLensKind.label lensKind)
                (lensMeaning lensKind)
                (lensWhyThisColumn columnState lensKind)
                "nm-column__badge--lens")

        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderColumnSlot (builder: StringBuilder) slotKind extraClasses renderContent =
        let slotClass =
            if String.IsNullOrWhiteSpace extraClasses then
                $"nm-column__slot nm-column__slot--{slotKind}"
            else
                $"nm-column__slot nm-column__slot--{slotKind} {extraClasses}"

        appendLine
            builder
            $"<div class=\"{slotClass}\" data-testid=\"nm-column-slot\" data-slot-kind=\"{slotKind}\">"

        appendLine builder "<div class=\"nm-column__slot-body\" data-testid=\"nm-column-slot-body\" data-slot-body>"
        renderContent ()
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderEmptyColumnSlot (builder: StringBuilder) slotKind =
        renderColumnSlot builder slotKind "nm-column__slot--empty" (fun () -> ())

    let private slotRowDefinition slotKind = $"minmax(var(--nm-slot-{slotKind}-height, 0px), auto)"

    let private columnTemplateRows slotKinds =
        slotKinds
        |> List.map slotRowDefinition
        |> String.concat " "

    let private renderStructuredDetailBox
        (builder: StringBuilder)
        detailKind
        detailKindDomKey
        titleText
        bodyText
        detailLines
        technicalLabel
        dataTestId
        =
        appendLine
            builder
            $"<section class=\"nm-column__detail-box nm-column__detail-box--{htmlEncode detailKindDomKey}\" data-testid=\"{htmlEncode dataTestId}\" data-detail-kind=\"{htmlEncode detailKindDomKey}\">"
        appendLine builder "<div class=\"nm-column__detail-topline\">"
        appendLine builder $"<span class=\"nm-column__detail-kind\">{htmlEncode detailKind}</span>"
        appendLine builder $"<h3 class=\"nm-column__detail-title\">{htmlEncode titleText}</h3>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-column__detail-copy\" data-testid=\"nm-column-detail-copy\">"

        match bodyText with
        | Some noteText -> appendLine builder $"<p class=\"nm-column__detail-note\">{htmlEncode noteText}</p>"
        | None -> ()

        match technicalLabel with
        | Some surfaceLabel -> appendLine builder $"<div class=\"nm-column__technical\" data-testid=\"nm-column-technical\">Surface · {htmlEncode surfaceLabel}</div>"
        | None -> ()

        if not (List.isEmpty detailLines) then
            appendLine builder "<ul class=\"nm-column__detail-list\">"

            detailLines
            |> List.iter (fun lineText -> appendLine builder $"<li>{htmlEncode lineText}</li>")

            appendLine builder "</ul>"

        appendLine builder "</div>"
        appendLine builder "</section>"

    let private renderColumnSurfacePreview (builder: StringBuilder) (columnState: NmColumnState) =
        let surfaceSizing = surfaceSizingStyle columnState.Surface

        appendLine builder "<div class=\"nm-column__surface-preview-stage\">"
        appendLine
            builder
            $"<div class=\"nm-column__surface-button\" data-testid=\"nm-surface-open\" data-action=\"open-surface-overlay\" data-surface-label=\"{htmlEncode columnState.ColumnTitle}\" role=\"button\" tabindex=\"0\" aria-label=\"Expand {htmlEncode columnState.ColumnTitle} surface\">"
        appendLine builder $"<div class=\"nm-column__surface-frame\"{surfaceSizing}>"
        appendLine builder "<div class=\"nm-column__surface-thumbnail-frame\" data-testid=\"nm-surface-thumbnail-frame\">"
        renderStaticSurfaceThumbnail builder columnState.Surface
        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-column__surface-live\" data-testid=\"nm-surface-live\">"
        appendLine builder "<div class=\"nm-column__surface-preview\">"
        appendLine builder "<div class=\"nm-column__surface-rendering\" data-testid=\"nm-surface-rendering\">"
        appendLine builder (renderSurfaceFragment columnState.Surface)
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"

    let private renderScreenDetailBox (builder: StringBuilder) (columnState: NmColumnState) =
        appendLine
            builder
            "<section class=\"nm-column__detail-box nm-column__detail-box--screen\" data-testid=\"nm-column-screen-box\" data-detail-kind=\"screen\">"
        appendLine builder "<div class=\"nm-column__detail-topline\">"
        appendLine builder "<div class=\"nm-column__detail-label-row\" data-testid=\"nm-column-screen-label-row\">"

        match columnState.ActorRoleBadge with
        | Some roleText ->
            renderBadgePopover
                builder
                columnState.ColumnKey
                "screen-role"
                roleText
                (roleMeaning roleText)
                (roleWhyThisColumn columnState roleText)
                "nm-column__badge--role"
        | None -> ()

        appendLine builder "<span class=\"nm-column__detail-kind\">SCREEN</span>"
        appendLine builder "</div>"
        appendLine builder $"<h3 class=\"nm-column__detail-title\">{htmlEncode (screenBoxTitle columnState)}</h3>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-column__detail-copy\" data-testid=\"nm-column-detail-copy\">"
        appendLine builder $"<p class=\"nm-column__detail-note\">{htmlEncode (screenBoxNote columnState)}</p>"

        if columnState.PrimaryContext = NmContextKind.EventModeling then
            appendLine builder "<div class=\"nm-column__screen-meta\" data-testid=\"nm-column-screen-meta\">"
            renderBadgePopover
                builder
                columnState.ColumnKey
                "screen-lens"
                "ui lens"
                "ui lens marks the linked app surface that frames the business slice."
                $"This screen box uses ui lens because {columnState.ColumnTitle} is being grounded in the app surface around the business slice."
                "nm-column__badge--surface-lens"

            appendLine builder "</div>"

        match columnState.TechnicalSurfaceLabel with
        | Some surfaceLabel ->
            appendLine
                builder
                $"<div class=\"nm-column__technical\" data-testid=\"nm-column-technical\">Surface · {htmlEncode surfaceLabel}</div>"
        | None -> ()

        appendLine builder "</div>"
        renderColumnSurfacePreview builder columnState
        appendLine builder "</section>"

    let private renderNonAemSlots (builder: StringBuilder) (columnState: NmColumnState) =
        renderColumnSlot builder "screen" "" (fun () -> renderScreenDetailBox builder columnState)

        renderColumnSlot builder "primary" "" (fun () ->
            renderStructuredDetailBox
                builder
                (nonAemDetailKindLabel columnState)
                (nonAemDetailKindLabel columnState |> domSlug)
                columnState.ColumnTitle
                columnState.ColumnNote
                columnState.ChangeItems
                None
                "nm-column-changes")

    let private renderEmbeddedAemPrimary (builder: StringBuilder) (sliceCard: PathSliceCard) =
        match sliceCard with
        | PathSliceCard.CommandSlice commandSlice ->
            appendLine builder (SliceHtmlRenderer.renderEmbeddedBlockHtml (SliceRenderOptions.classicEventModel "NM Embedded Slice") commandSlice.Command)
        | PathSliceCard.ViewSlice viewSlice ->
            appendLine builder (SliceHtmlRenderer.renderEmbeddedBlockHtml (SliceRenderOptions.classicEventModel "NM Embedded Slice") viewSlice.View)

    let private renderEmbeddedAemSecondary (builder: StringBuilder) (sliceCard: PathSliceCard) =
        match sliceCard with
        | PathSliceCard.CommandSlice commandSlice ->
            appendLine builder (SliceHtmlRenderer.renderEmbeddedBlockHtml (SliceRenderOptions.classicEventModel "NM Embedded Slice") commandSlice.Event)
        | PathSliceCard.ViewSlice _ -> ()

    let private renderEmbeddedAemGwt (builder: StringBuilder) (sliceCard: PathSliceCard) =
        match sliceCard with
        | PathSliceCard.CommandSlice commandSlice ->
            match commandSlice.Gwt with
            | Some gwtCard -> appendLine builder (SliceHtmlRenderer.renderEmbeddedGwtHtml (SliceRenderOptions.classicEventModel "NM Embedded Slice") gwtCard)
            | None -> ()
        | PathSliceCard.ViewSlice viewSlice ->
            match viewSlice.Gwt with
            | Some gwtCard -> appendLine builder (SliceHtmlRenderer.renderEmbeddedGwtHtml (SliceRenderOptions.classicEventModel "NM Embedded Slice") gwtCard)
            | None -> ()

    let private hasSecondaryAemSlot =
        function
        | PathSliceCard.CommandSlice _ -> true
        | PathSliceCard.ViewSlice _ -> false

    let private hasGwtAemSlot =
        function
        | PathSliceCard.CommandSlice commandSlice -> commandSlice.Gwt.IsSome
        | PathSliceCard.ViewSlice viewSlice -> viewSlice.Gwt.IsSome

    let private renderAemSlots (builder: StringBuilder) (columnState: NmColumnState) (sliceCard: PathSliceCard) =
        renderColumnSlot builder "screen" "" (fun () -> renderScreenDetailBox builder columnState)
        renderColumnSlot builder "primary" "nm-column__slot--aem" (fun () -> renderEmbeddedAemPrimary builder sliceCard)

        if hasSecondaryAemSlot sliceCard then
            renderColumnSlot builder "secondary" "nm-column__slot--aem" (fun () -> renderEmbeddedAemSecondary builder sliceCard)
        elif hasGwtAemSlot sliceCard then
            renderEmptyColumnSlot builder "secondary"

        if hasGwtAemSlot sliceCard then
            renderColumnSlot builder "gwt" "nm-column__slot--aem" (fun () -> renderEmbeddedAemGwt builder sliceCard)

    let private renderedSlotKinds =
        function
        | { Surface = NmAemSliceSurface (_, sliceCard) } ->
            if hasGwtAemSlot sliceCard then
                [ "header"; "screen"; "primary"; "secondary"; "gwt" ]
            elif hasSecondaryAemSlot sliceCard then
                [ "header"; "screen"; "primary"; "secondary" ]
            else
                [ "header"; "screen"; "primary" ]
        | _ -> [ "header"; "screen"; "primary" ]

    let private renderColumn (builder: StringBuilder) (index: int) (columnState: NmColumnState) =
        let stepNumberText = (index + 1).ToString("00")
        let groupDomKey = ContextGroupKind.domKey columnState.ContextGroup
        let contextDomKey = NmContextKind.domKey columnState.PrimaryContext
        let kindDomKey = columnKindDomKey columnState
        let templateRows = columnTemplateRows (renderedSlotKinds columnState)

        appendLine
            builder
            $"<section class=\"nm-column nm-column--{htmlEncode groupDomKey} nm-column--context-{htmlEncode contextDomKey} nm-column--kind-{htmlEncode kindDomKey}\" style=\"--nm-column-template-rows: {htmlEncode templateRows};\" data-testid=\"nm-path-column\" data-column-key=\"{htmlEncode columnState.ColumnKey}\" data-context-key=\"{htmlEncode contextDomKey}\" data-lens-keys=\"{htmlEncode (lensDomKeys columnState)}\">"
        renderColumnSlot builder "header" "" (fun () ->
            appendLine builder "<div class=\"nm-column__header\">"
            renderClassificationRow builder columnState
            appendLine builder $"<div class=\"nm-column__eyebrow\">Step {stepNumberText} · {htmlEncode columnState.ColumnKey}</div>"
            appendLine builder $"<h2 class=\"nm-column__title\">{htmlEncode columnState.ColumnTitle}</h2>"

            match columnState.ColumnNote with
            | Some noteText -> appendLine builder $"<p class=\"nm-column__note\">{htmlEncode noteText}</p>"
            | None -> appendLine builder "<p class=\"nm-column__note\"></p>"

            appendLine builder "</div>")

        match columnState.Surface with
        | NmAemSliceSurface (_, sliceCard) -> renderAemSlots builder columnState sliceCard
        | _ -> renderNonAemSlots builder columnState

        appendLine builder "</section>"

    let private renderStyles (builder: StringBuilder) =
        appendLine builder "<style>"
        appendLine builder "html, body { height: 100%; overflow: hidden; }"
        appendLine builder "body { margin: 0; background: linear-gradient(180deg, #eef2f7 0%, #e6ecf4 100%); color: #0d2440; font-family: \"IBM Plex Sans\", \"Aptos\", \"Segoe UI\", sans-serif; }"
        appendLine builder ".nm-path-document { --nm-column-width: 292px; --nm-surface-scale: 0.24; --nm-surface-slot-height: 136px; --nm-surface-frame-width: 182px; height: 100vh; padding: 14px 16px 18px; display: grid; grid-template-rows: auto auto minmax(0, 1fr); overflow: hidden; }"
        appendLine builder ".nm-path-document[data-surface-mode=\"thumbnail\"] { --nm-surface-scale: 0.24; --nm-surface-slot-height: 136px; --nm-surface-frame-width: 182px; }"
        appendLine builder ".nm-path-document[data-surface-mode=\"full\"] { --nm-surface-scale: 0.28; --nm-surface-slot-height: 228px; --nm-surface-frame-width: 246px; }"
        appendLine builder ".nm-path-document__header { display: flex; flex-direction: column; gap: 6px; }"
        appendLine builder ".nm-path-document__topline { display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 16px; align-items: start; }"
        appendLine builder ".nm-path-document__title-zone { min-width: 0; display: flex; flex-direction: column; gap: 4px; }"
        appendLine builder ".nm-path-document__identity-row { display: flex; align-items: center; gap: 0.55rem; min-width: 0; flex-wrap: wrap; }"
        appendLine builder ".nm-path-document__app-pill { display: inline-flex; align-items: center; justify-content: center; padding: 0.28rem 0.68rem; border-radius: 999px; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); color: #ffffff; font-size: 0.68rem; font-weight: 700; line-height: 1; box-shadow: 0 2px 8px rgba(255, 183, 77, 0.24); }"
        appendLine builder ".nm-path-document__eyebrow { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #0e5883; }"
        appendLine builder ".nm-path-document__title { margin: 0; font-size: 0.98rem; line-height: 1.06; font-weight: 700; }"
        appendLine builder ".nm-path-document__context { align-self: flex-start; min-width: 0; }"
        appendLine builder ".nm-path-document__context > summary { list-style: none; display: inline-flex; align-items: center; gap: 0.45rem; padding: 0.42rem 0.72rem; border-radius: 999px; background: #f8fafc; border: 1px solid #dbe5f1; cursor: pointer; color: #0f172a; }"
        appendLine builder ".nm-path-document__context > summary::-webkit-details-marker { display: none; }"
        appendLine builder ".nm-path-document__context-label { font-size: 0.63rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #64748b; }"
        appendLine builder ".nm-path-document__context-title { font-size: 0.74rem; font-weight: 700; color: #0f172a; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 34vw; }"
        appendLine builder ".nm-path-document__context-panel { margin-top: 0.55rem; padding: 0.72rem 0.85rem; border-radius: 0.95rem; background: #f8fafc; border: 1px solid #dbe5f1; display: grid; gap: 0.45rem; max-width: 62rem; }"
        appendLine builder ".nm-path-document__description { margin: 0; color: #64748b; font-size: 0.72rem; line-height: 1.28; }"
        appendLine builder ".nm-path-document__assumptions { margin: 0; padding-left: 1rem; display: grid; gap: 0.18rem; color: #475569; font-size: 0.72rem; line-height: 1.28; }"
        appendLine builder ".nm-path-document__header-controls { display: flex; gap: 0.9rem; flex-wrap: wrap; justify-content: flex-end; align-items: flex-start; }"
        appendLine builder ".nm-path-document__control-group { display: flex; flex-direction: column; gap: 0.28rem; align-items: flex-start; }"
        appendLine builder ".nm-path-document__control-label { font-size: 0.63rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #64748b; }"
        appendLine builder ".nm-path-document__control-buttons { display: flex; gap: 0.45rem; flex-wrap: wrap; }"
        appendLine builder ".nm-path-toggle, .nm-path-update-toggle, .nm-path-update-refresh { border: 1px solid #cbd5e1; border-radius: 999px; background: #ffffff; color: #475569; font-size: 0.7rem; font-weight: 700; padding: 0.42rem 0.72rem; cursor: pointer; }"
        appendLine builder ".nm-path-toggle:hover, .nm-path-update-toggle:hover, .nm-path-update-refresh:hover:not(:disabled) { border-color: #94a3b8; }"
        appendLine builder ".nm-path-toggle.is-active, .nm-path-update-toggle.is-active { background: #0e5883; border-color: #0e5883; color: #ffffff; }"
        appendLine builder ".nm-path-update-refresh { background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); border-color: #ffb74d; color: #ffffff; }"
        appendLine builder ".nm-path-update-refresh:disabled { background: #e2e8f0; border-color: #cbd5e1; color: #94a3b8; cursor: default; }"
        appendLine builder ".nm-path-document__update-status { font-size: 0.7rem; line-height: 1.2; color: #64748b; }"
        appendLine builder ".nm-path-document__update-status[data-state=\"available\"] { color: #9a3412; font-weight: 700; }"
        appendLine builder ".nm-path-document__update-status[data-state=\"error\"] { color: #b91c1c; }"
        appendLine builder ".nm-path-document__update-status[data-state=\"auto\"] { color: #0e5883; font-weight: 700; }"
        appendLine builder ".nm-path-document[data-view-mode=\"summary\"] .nm-path-document__context-panel, .nm-path-document[data-view-mode=\"summary\"] .nm-column__note, .nm-path-document[data-view-mode=\"summary\"] .nm-column__meta, .nm-path-document[data-view-mode=\"summary\"] .nm-column__detail-copy, .nm-path-document[data-view-mode=\"summary\"] .ll-screen-surface__name, .nm-path-document[data-view-mode=\"summary\"] .ll-screen-surface__note { display: none; }"
        appendLine builder ".nm-path-scroll-controls { display: grid; grid-template-columns: auto auto minmax(0, 1fr) auto auto; gap: 10px; align-items: center; padding: 4px 4px 8px; background: linear-gradient(180deg, #ffffff 0%, rgba(255, 255, 255, 0.98) 72%, rgba(255, 255, 255, 0.92) 100%); }"
        appendLine builder ".nm-path-nav-button { width: 42px; height: 42px; border: 0; border-radius: 999px; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); color: #ffffff; font-size: 1rem; font-weight: 800; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; box-shadow: 0 2px 8px rgba(255, 183, 77, 0.32); }"
        appendLine builder ".nm-path-nav-button:hover:not(:disabled) { filter: brightness(0.98); }"
        appendLine builder ".nm-path-nav-button:active:not(:disabled) { transform: scale(0.96); }"
        appendLine builder ".nm-path-nav-button:disabled { background: #cbd5e1; color: #94a3b8; cursor: default; box-shadow: none; }"
        appendLine builder ".nm-path-nav-button__icon { display: inline-flex; align-items: center; justify-content: center; gap: 0.12rem; line-height: 1; }"
        appendLine builder ".nm-path-nav-button__bar { width: 3px; height: 15px; border-radius: 999px; background: currentColor; flex: 0 0 auto; }"
        appendLine builder ".nm-path-nav-button__chevron { width: 11px; height: 11px; border-top: 3px solid currentColor; border-right: 3px solid currentColor; flex: 0 0 auto; }"
        appendLine builder ".nm-path-nav-button__chevron--right { transform: rotate(45deg); }"
        appendLine builder ".nm-path-nav-button__chevron--left { transform: rotate(-135deg); }"
        appendLine builder ".nm-path-scrollbar { overflow-x: auto; overflow-y: hidden; scrollbar-gutter: stable both-edges; }"
        appendLine builder ".nm-path-scrollbar__content { height: 1px; }"
        appendLine builder ".nm-path-stage { min-height: 0; overflow-y: auto; overflow-x: hidden; padding: 0 4px 12px; }"
        appendLine builder ".nm-path-flow-viewport { overflow-x: auto; overflow-y: visible; scrollbar-width: none; }"
        appendLine builder ".nm-path-flow-viewport::-webkit-scrollbar { display: none; }"
        appendLine builder ".nm-path-flow { display: grid; grid-auto-flow: column; grid-auto-columns: minmax(var(--nm-column-width), var(--nm-column-width)); width: max-content; gap: 18px; align-items: start; padding: 4px 4px 18px; }"
        appendLine builder ".nm-column { display: grid; grid-template-rows: var(--nm-column-template-rows, minmax(var(--nm-slot-header-height, 0px), auto) minmax(var(--nm-slot-screen-height, 0px), auto) minmax(var(--nm-slot-primary-height, 0px), auto)); gap: 10px; border-radius: 24px; border: 4px solid #15263d; box-shadow: 0 10px 24px rgba(10, 27, 49, 0.1); padding: 10px 10px 12px; min-height: 0; overflow: visible; align-content: start; }"
        appendLine builder ".nm-column[hidden] { display: none !important; }"
        appendLine builder ".nm-column--app-runtime { background: linear-gradient(180deg, #e5eefb 0%, #f7fbff 100%); }"
        appendLine builder ".nm-column--interaction { background: linear-gradient(180deg, #eef8f1 0%, #fbfffc 100%); }"
        appendLine builder ".nm-column--business { background: linear-gradient(180deg, #f4f6fa 0%, #ffffff 100%); }"
        appendLine builder ".nm-column--context-application-lifecycle { background: linear-gradient(180deg, #e7effc 0%, #f8fbff 100%); border-color: #19395c; }"
        appendLine builder ".nm-column--context-runtime-orchestration { background: linear-gradient(180deg, #edf5ff 0%, #fbfdff 100%); border-color: #22506b; }"
        appendLine builder ".nm-column--context-screen-path { background: linear-gradient(180deg, #eef9f2 0%, #fbfffc 100%); border-color: #215743; }"
        appendLine builder ".nm-column--context-event-modeling { background: linear-gradient(180deg, #f4f6fa 0%, #ffffff 100%); border-color: #1a2c45; }"
        appendLine builder ".nm-column--context-event-modeling.nm-column--kind-command { background: linear-gradient(180deg, #dff1ff 0%, #eff7ff 100%); }"
        appendLine builder ".nm-column--context-event-modeling.nm-column--kind-view { background: linear-gradient(180deg, #dbfae4 0%, #effbf3 100%); }"
        appendLine builder ".nm-column__slot { min-height: 0; display: flex; align-items: stretch; }"
        appendLine builder ".nm-column__slot-body { width: 100%; min-height: 0; height: 100%; display: flex; align-items: stretch; }"
        appendLine builder ".nm-column__slot-body > * { width: 100%; }"
        appendLine builder ".nm-column__slot--empty { pointer-events: none; }"
        appendLine builder ".nm-column__slot--empty .nm-column__slot-body { min-height: 0; height: 100%; }"
        appendLine builder ".nm-column__header { display: flex; flex-direction: column; gap: 5px; }"
        appendLine builder ".nm-column__classification { display: grid; gap: 4px; }"
        appendLine builder ".nm-column__classification-row { display: grid; align-items: start; }"
        appendLine builder ".nm-column__classification-row--primary { grid-template-columns: minmax(0, 1fr) auto; gap: 10px; }"
        appendLine builder ".nm-column__classification-row--secondary { grid-template-columns: minmax(0, 1fr); }"
        appendLine builder ".nm-column__kind { display: inline-flex; align-items: center; justify-content: center; padding: 2px 7px; border-radius: 4px; border: 1px solid #bfcad7; background: #cbd5e1; color: #1e293b; font-size: 0.5rem; font-weight: 700; letter-spacing: 0.06em; text-transform: uppercase; line-height: 1; white-space: nowrap; margin-top: 2px; }"
        appendLine builder ".nm-column__eyebrow { font-size: 0.64rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #94a3b8; }"
        appendLine builder ".nm-column__title { margin: 0; font-size: 0.84rem; line-height: 1.05; font-weight: 700; color: #0f172a; }"
        appendLine builder ".nm-column__note { margin: 0; font-size: 0.7rem; line-height: 1.24; color: #556b85; min-height: 0; }"
        appendLine builder ".nm-column__surface-preview-stage { display: flex; justify-content: center; padding-top: 0.3rem; }"
        appendLine builder ".nm-column__surface-button { display: flex; justify-content: center; width: 100%; border: 0; background: transparent; padding: 0; cursor: zoom-in; text-align: left; border-radius: 18px; }"
        appendLine builder ".nm-column__surface-button:focus-visible { outline: 2px solid #0e5883; outline-offset: 2px; }"
        appendLine builder ".nm-column__surface-frame { --nm-surface-native-width: 360px; --nm-surface-native-height: 667px; position: relative; width: min(100%, var(--nm-surface-frame-width)); border-radius: 16px; border: 2px solid rgba(111, 135, 163, 0.3); background: linear-gradient(180deg, #ffffff 0%, #f4f7fb 100%); box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.92); overflow: hidden; height: var(--nm-surface-slot-height); min-height: 0; display: flex; justify-content: center; align-items: center; padding: 8px; }"
        appendLine builder ".nm-column__surface-thumbnail-frame { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; }"
        appendLine builder ".nm-column__surface-live { width: 100%; height: 100%; display: flex; align-items: flex-start; justify-content: center; }"
        appendLine builder ".nm-path-document[data-surface-mode=\"thumbnail\"] .nm-column__surface-thumbnail-frame { display: flex; }"
        appendLine builder ".nm-path-document[data-surface-mode=\"thumbnail\"] .nm-column__surface-live { display: none; }"
        appendLine builder ".nm-path-document[data-surface-mode=\"full\"] .nm-column__surface-thumbnail-frame { display: none; }"
        appendLine builder ".nm-path-document[data-surface-mode=\"full\"] .nm-column__surface-live { display: flex; }"
        appendLine builder ".nm-column__surface-preview { position: relative; width: 100%; height: 100%; overflow: hidden; border-radius: 12px; display: flex; justify-content: center; align-items: flex-start; }"
        appendLine builder ".nm-column__surface-rendering { position: absolute; top: 8px; left: 50%; width: var(--nm-surface-native-width); min-height: var(--nm-surface-native-height); margin-left: calc(var(--nm-surface-native-width) / -2); transform: scale(var(--nm-surface-scale)); transform-origin: top center; pointer-events: none; filter: drop-shadow(0 5px 12px rgba(15, 23, 42, 0.16)); }"
        appendLine builder ".nm-column__surface-empty { min-height: 140px; display: flex; align-items: center; justify-content: center; color: #64748b; font-size: 0.78rem; font-weight: 600; }"
        appendLine builder ".nm-thumbnail { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; }"
        appendLine builder ".nm-thumbnail__device { width: 96px; min-height: 118px; border-radius: 18px; border: 1px solid #c7d4e4; background: linear-gradient(180deg, #ffffff 0%, #f8fbff 100%); box-shadow: 0 10px 22px rgba(15, 23, 42, 0.1); padding: 7px 7px 8px; display: grid; gap: 7px; justify-items: stretch; }"
        appendLine builder ".nm-thumbnail__device-top { justify-self: center; width: 34px; height: 4px; border-radius: 999px; background: #d6deea; }"
        appendLine builder ".nm-thumbnail__app-bar { height: 12px; border-radius: 8px; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); }"
        appendLine builder ".nm-thumbnail__screen-body { display: grid; gap: 7px; }"
        appendLine builder ".nm-thumbnail__panel { display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 5px; align-items: center; }"
        appendLine builder ".nm-thumbnail__field { min-width: 0; border-radius: 8px; border: 1px solid #d7e3f0; background: #ffffff; color: #94a3b8; font-size: 0.4rem; line-height: 1.2; padding: 5px 6px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }"
        appendLine builder ".nm-thumbnail__field--filled { color: #1e3a5f; }"
        appendLine builder ".nm-thumbnail__field-action { width: 20px; height: 20px; border-radius: 8px; border: 1px solid #d7e3f0; display: flex; align-items: center; justify-content: center; font-size: 0.48rem; color: #ff7d51; background: #ffffff; }"
        appendLine builder ".nm-thumbnail__button { border-radius: 9px; padding: 5px 6px; text-align: center; font-size: 0.42rem; font-weight: 700; letter-spacing: 0.01em; }"
        appendLine builder ".nm-thumbnail__button--enabled { background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); color: #ffffff; }"
        appendLine builder ".nm-thumbnail__button--disabled { background: #eef2f7; color: #94a3b8; border: 1px solid #dbe5f1; }"
        appendLine builder ".nm-thumbnail__location-pill { border-radius: 999px; border: 1px solid #d7e3f0; background: #ffffff; color: #274463; font-size: 0.4rem; line-height: 1.2; padding: 4px 7px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }"
        appendLine builder ".nm-thumbnail__choice-row { display: flex; flex-wrap: wrap; gap: 4px; justify-content: center; }"
        appendLine builder ".nm-thumbnail__choice-pill { display: inline-flex; align-items: center; justify-content: center; min-width: 0; padding: 2px 5px; border-radius: 999px; border: 1px solid #d7e3f0; background: #ffffff; color: #5b7088; font-size: 0.38rem; font-weight: 700; line-height: 1; }"
        appendLine builder ".nm-thumbnail__choice-pill--selected { border-color: #ffb74d; background: #fff7ed; color: #c26d0b; }"
        appendLine builder ".nm-thumbnail__metric-row { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 4px; }"
        appendLine builder ".nm-thumbnail__metric-box { border-radius: 8px; background: #f8fafc; border: 1px solid #dbe5f1; color: #334155; font-size: 0.4rem; font-weight: 700; text-align: center; padding: 4px 5px; }"
        appendLine builder ".nm-thumbnail__total-bar { display: flex; justify-content: space-between; align-items: center; gap: 6px; border-radius: 9px; padding: 5px 6px; background: #e8eef6; color: #1e3a5f; font-size: 0.4rem; line-height: 1; font-weight: 700; }"
        appendLine builder ".nm-thumbnail__total-bar strong { font-size: 0.44rem; }"
        appendLine builder ".nm-thumbnail__total-bar--success { background: #e7f7ec; color: #1f6b37; }"
        appendLine builder ".nm-thumbnail__hero { display: grid; justify-items: center; gap: 6px; padding-top: 4px; }"
        appendLine builder ".nm-thumbnail__hero-icon { width: 30px; height: 30px; border-radius: 10px; background: linear-gradient(135deg, #ffcc80 0%, #ffb74d 100%); display: flex; align-items: center; justify-content: center; font-size: 0.92rem; box-shadow: 0 5px 12px rgba(255, 183, 77, 0.28); }"
        appendLine builder ".nm-thumbnail__hero-title { font-size: 0.43rem; font-weight: 700; color: #0f172a; text-align: center; }"
        appendLine builder ".nm-thumbnail__hero-subtitle { font-size: 0.38rem; line-height: 1.28; color: #64748b; text-align: center; }"
        appendLine builder ".nm-thumbnail__checkpoint-stack { display: grid; gap: 4px; }"
        appendLine builder ".nm-thumbnail__checkpoint { height: 6px; border-radius: 999px; background: #dbe5f1; }"
        appendLine builder ".nm-thumbnail__checkpoint--pending { background: #e2e8f0; }"
        appendLine builder ".nm-thumbnail__checkpoint--active { background: #fdba74; }"
        appendLine builder ".nm-thumbnail__checkpoint--complete { background: #93c5fd; }"
        appendLine builder ".nm-thumbnail__empty { display: flex; align-items: center; justify-content: center; width: 100%; height: 100%; border-radius: 12px; border: 1px dashed #cbd5e1; color: #64748b; font-size: 0.62rem; font-weight: 600; background: #f8fafc; }"
        appendLine builder ".nm-column__meta { display: flex; flex-wrap: wrap; gap: 0.38rem; align-items: flex-start; min-width: 0; }"
        appendLine builder ".nm-column__meta--secondary { gap: 0.32rem; }"
        appendLine builder ".nm-column__pill-wrap { position: relative; display: inline-flex; max-width: 100%; }"
        appendLine builder ".nm-column__pill-wrap[data-open=\"true\"] { z-index: 80; }"
        appendLine builder ".nm-column__badge { display: inline-flex; align-items: center; justify-content: center; padding: 2px 8px; border-radius: 999px; border: 1px solid #c2d4e8; background: rgba(255, 255, 255, 0.94); font-size: 0.56rem; font-weight: 700; letter-spacing: 0.01em; text-transform: none; color: #334155; white-space: nowrap; cursor: help; }"
        appendLine builder ".nm-column__badge--group { background: #0f2740; border-color: #0f2740; color: #ffffff; }"
        appendLine builder ".nm-column__badge--interactive { appearance: none; -webkit-appearance: none; }"
        appendLine builder ".nm-column__badge--interactive:focus-visible { outline: 2px solid #0e5883; outline-offset: 2px; }"
        appendLine builder ".nm-column__badge--role { text-transform: none; }"
        appendLine builder ".nm-column__pill-popover { position: absolute; top: calc(100% + 8px); left: 0; z-index: 30; width: min(250px, 72vw); display: grid; gap: 0.3rem; padding: 0.72rem 0.8rem; border-radius: 0.85rem; border: 1px solid #cbd5e1; background: rgba(255,255,255,0.98); box-shadow: 0 14px 30px rgba(15, 23, 42, 0.16); opacity: 0; visibility: hidden; transform: translateY(-4px); pointer-events: none; transition: opacity 120ms ease, transform 120ms ease, visibility 120ms ease; }"
        appendLine builder ".nm-column__pill-wrap[data-open=\"true\"] .nm-column__pill-popover { opacity: 1; visibility: visible; transform: translateY(0); pointer-events: auto; }"
        appendLine builder ".nm-column__pill-popover-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 0.5rem; }"
        appendLine builder ".nm-column__pill-popover-heading { display: grid; gap: 0.14rem; }"
        appendLine builder ".nm-column__pill-popover-label { font-size: 0.54rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #64748b; }"
        appendLine builder ".nm-column__pill-popover-title { font-size: 0.64rem; font-weight: 700; letter-spacing: 0.04em; color: #0f172a; }"
        appendLine builder ".nm-column__pill-popover-close { border: 1px solid #cbd5e1; border-radius: 999px; background: #ffffff; color: #475569; font-size: 0.58rem; font-weight: 700; line-height: 1; padding: 0.28rem 0.5rem; cursor: pointer; }"
        appendLine builder ".nm-column__pill-popover-close:hover { border-color: #94a3b8; }"
        appendLine builder ".nm-column__pill-popover-close:focus-visible { outline: 2px solid #0e5883; outline-offset: 2px; }"
        appendLine builder ".nm-column__detail-label-row .nm-column__pill-popover, .nm-column__screen-meta .nm-column__pill-popover { left: auto; right: 0; }"
        appendLine builder ".nm-column__pill-popover-text { margin: 0; color: #475569; font-size: 0.68rem; line-height: 1.3; }"
        appendLine builder ".nm-column__detail-box { display: grid; gap: 7px; border-radius: 18px; padding: 10px 11px; border: 2px solid; min-height: 0; height: 100%; overflow: visible; background: rgba(255, 255, 255, 0.86); box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.72); }"
        appendLine builder ".nm-column__detail-box--screen { border-color: #c8dcff; background: linear-gradient(180deg, #ffffff 0%, #f8fbff 100%); }"
        appendLine builder ".nm-column__detail-box--transition { border-color: #89b6e8; background: rgba(82, 155, 246, 0.14); }"
        appendLine builder ".nm-column__detail-box--orchestration { border-color: #8ac3dd; background: rgba(84, 165, 200, 0.14); }"
        appendLine builder ".nm-column__detail-box--interaction { border-color: #8ad1a1; background: rgba(93, 194, 120, 0.16); }"
        appendLine builder ".nm-column__detail-topline { display: grid; gap: 5px; }"
        appendLine builder ".nm-column__detail-label-row { display: flex; flex-wrap: wrap; justify-content: flex-end; align-items: center; gap: 0.32rem; }"
        appendLine builder ".nm-column__detail-kind { justify-self: end; display: inline-flex; align-items: center; justify-content: center; padding: 2px 6px; border-radius: 4px; border: 1px solid #bfcad7; background: #cbd5e1; font-size: 0.5rem; font-weight: 700; letter-spacing: 0.06em; text-transform: uppercase; color: #1e293b; }"
        appendLine builder ".nm-column__detail-title { margin: 0; font-size: 0.72rem; line-height: 1.14; font-weight: 700; color: #0f172a; }"
        appendLine builder ".nm-column__detail-copy { display: grid; gap: 0.28rem; }"
        appendLine builder ".nm-column__screen-meta { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: 0.32rem; }"
        appendLine builder ".nm-column__detail-note { margin: 0; color: #4f657f; font-size: 0.68rem; line-height: 1.25; }"
        appendLine builder ".nm-column__technical { font-size: 0.6rem; font-weight: 700; letter-spacing: 0.04em; color: #48627f; }"
        appendLine builder ".nm-column__detail-list { margin: 0; padding-left: 1rem; display: grid; gap: 0.14rem; color: #475569; font-size: 0.68rem; line-height: 1.22; }"
        appendLine builder ".nm-column__slot--aem .slice-block { min-height: 0; height: 100%; }"
        appendLine builder ".nm-column__slot--aem .slice-card__gwt-card { min-height: 0; height: 100%; }"
        appendLine builder ".nm-column__slot--aem .slice-card__width-action { display: none; }"
        appendLine builder ".nm-surface-overlay { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.66); display: none; align-items: center; justify-content: center; padding: 20px; z-index: 40; }"
        appendLine builder ".nm-surface-overlay.is-open { display: flex; }"
        appendLine builder ".nm-surface-overlay__dialog { width: min(92vw, 520px); max-height: 92vh; background: #f8fafc; border-radius: 20px; box-shadow: 0 20px 44px rgba(15, 23, 42, 0.34); display: flex; flex-direction: column; overflow: hidden; }"
        appendLine builder ".nm-surface-overlay__header { display: flex; justify-content: space-between; align-items: center; gap: 12px; padding: 0.9rem 1rem; border-bottom: 1px solid #dbe5f1; background: #ffffff; }"
        appendLine builder ".nm-surface-overlay__title { margin: 0; font-size: 0.88rem; font-weight: 700; color: #0f172a; }"
        appendLine builder ".nm-surface-overlay__close { border: 1px solid #cbd5e1; background: #ffffff; color: #475569; border-radius: 999px; padding: 0.4rem 0.75rem; font-size: 0.72rem; font-weight: 700; cursor: pointer; }"
        appendLine builder ".nm-surface-overlay__body { padding: 1rem; overflow: auto; }"
        appendLine builder ".nm-surface-overlay__surface { display: flex; justify-content: center; }"
        appendLine builder ".nm-surface-overlay__surface .nm-column__surface-rendering { position: static; top: auto; left: auto; margin-left: 0; transform: none !important; width: var(--nm-surface-native-width, auto); min-height: 0; pointer-events: auto; }"
        appendLine builder ".nm-surface-overlay__surface .ll-screen-surface { width: 100%; max-width: 380px; }"
        appendLine builder ".nm-surface-overlay__surface .ll-phone-screen { max-width: 380px; min-height: auto; }"
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
        appendLine builder "@media (max-width: 980px) { .nm-path-document { padding-left: 12px; padding-right: 12px; } .nm-path-document__topline { grid-template-columns: 1fr; } .nm-path-document__header-controls { justify-content: flex-start; } .nm-path-document__context-title { max-width: 72vw; } .nm-path-flow { grid-auto-columns: minmax(280px, 84vw); } }"
        appendLine builder "</style>"

    let private renderScript (builder: StringBuilder) (pathState: NmPathState) (updateState: NmPathUpdateState option) =
        appendLine builder "<script>"
        appendLine builder "(function () {"
        appendLine builder "  const pathDocument = document.querySelector('.nm-path-document');"
        appendLine builder "  const scrollbar = document.getElementById('nm-path-scrollbar');"
        appendLine builder "  const scrollbarContent = document.getElementById('nm-path-scrollbar-content');"
        appendLine builder "  const viewport = document.getElementById('nm-path-flow-viewport');"
        appendLine builder "  const flow = document.getElementById('nm-path-flow');"
        appendLine builder "  const startButton = document.getElementById('nm-path-nav-start');"
        appendLine builder "  const previousButton = document.getElementById('nm-path-nav-previous');"
        appendLine builder "  const nextButton = document.getElementById('nm-path-nav-next');"
        appendLine builder "  const endButton = document.getElementById('nm-path-nav-end');"
        appendLine builder "  const viewModeButtons = Array.from(document.querySelectorAll('.nm-path-view-toggle'));"
        appendLine builder "  const lensButtons = Array.from(document.querySelectorAll('.nm-path-lens-toggle'));"
        appendLine builder "  const surfaceButtons = Array.from(document.querySelectorAll('.nm-path-surface-toggle'));"
        appendLine builder "  const columnElements = Array.from(document.querySelectorAll('.nm-column'));"
        appendLine builder "  const slotKinds = ['header', 'screen', 'primary', 'secondary', 'gwt'];"
        appendLine builder "  const updateModeButtons = Array.from(document.querySelectorAll('.nm-path-update-toggle'));"
        appendLine builder "  const updateStatus = document.getElementById('nm-path-update-status');"
        appendLine builder "  const refreshButton = document.getElementById('nm-path-refresh-now');"
        appendLine builder "  const overlay = document.getElementById('nm-surface-overlay');"
        appendLine builder "  const overlayTitle = document.getElementById('nm-surface-overlay-title');"
        appendLine builder "  const overlaySurface = document.getElementById('nm-surface-overlay-surface');"
        appendLine builder "  const overlayClose = document.getElementById('nm-surface-overlay-close');"
        appendLine builder "  const pillWraps = Array.from(document.querySelectorAll('[data-testid=\"nm-column-pill-wrap\"]'));"
        appendLine builder $"  const updateModeStorageKey = 'll-nm-path-update-mode::{htmlEncode (pathState.PathId.ToLowerInvariant())}';"
        appendLine builder $"  const lensFilterStorageKey = 'll-nm-path-lenses::{htmlEncode (pathState.PathId.ToLowerInvariant())}';"
        appendLine builder $"  const surfaceModeStorageKey = 'll-nm-path-surface::{htmlEncode (pathState.PathId.ToLowerInvariant())}';"
        appendLine builder "  const defaultLensKeys = ['lifecycle', 'runtime', 'screen', 'aem'];"
        appendLine builder "  const defaultSurfaceMode = 'thumbnail';"
        appendLine builder "  const applyViewMode = (mode) => {"
        appendLine builder "    if (!pathDocument) { return; }"
        appendLine builder "    const normalizedMode = mode === 'summary' ? 'summary' : 'detailed';"
        appendLine builder "    pathDocument.dataset.viewMode = normalizedMode;"
        appendLine builder "    viewModeButtons.forEach((button) => {"
        appendLine builder "      const isActive = button.dataset.viewMode === normalizedMode;"
        appendLine builder "      button.classList.toggle('is-active', isActive);"
        appendLine builder "      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');"
        appendLine builder "    });"
        appendLine builder "    window.requestAnimationFrame(() => syncWidth());"
        appendLine builder "  };"
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
        appendLine builder "  const syncSlotHeights = () => {"
        appendLine builder "    if (!pathDocument) { return; }"
        appendLine builder "    slotKinds.forEach((slotKind) => {"
        appendLine builder "      pathDocument.style.setProperty(`--nm-slot-${slotKind}-height`, '0px');"
        appendLine builder "    });"
        appendLine builder "    void pathDocument.offsetHeight;"
        appendLine builder "    const visibleColumns = columnElements.filter((column) => !column.hidden);"
        appendLine builder "    slotKinds.forEach((slotKind) => {"
        appendLine builder "      let maxHeight = 0;"
        appendLine builder "      visibleColumns.forEach((column) => {"
        appendLine builder "        const slotBody = column.querySelector(`[data-slot-kind=\"${slotKind}\"] [data-slot-body]`);"
        appendLine builder "        if (!slotBody) { return; }"
        appendLine builder "        const slotContent = slotBody.firstElementChild;"
        appendLine builder "        const measuredHeight = Math.ceil(slotContent ? slotContent.getBoundingClientRect().height : 0);"
        appendLine builder "        if (measuredHeight > maxHeight) { maxHeight = measuredHeight; }"
        appendLine builder "      });"
        appendLine builder "      pathDocument.style.setProperty(`--nm-slot-${slotKind}-height`, `${maxHeight}px`);"
        appendLine builder "    });"
        appendLine builder "  };"
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
        appendLine builder "  const updateButtonState = () => {"
        appendLine builder "    const currentLeft = viewport.scrollLeft;"
        appendLine builder "    const currentMax = logicalMaxScrollLeft();"
        appendLine builder "    if (startButton) { startButton.disabled = currentLeft <= 2; }"
        appendLine builder "    if (previousButton) { previousButton.disabled = currentLeft <= 2; }"
        appendLine builder "    if (nextButton) { nextButton.disabled = currentLeft >= currentMax - 2; }"
        appendLine builder "    if (endButton) { endButton.disabled = currentLeft >= currentMax - 2; }"
        appendLine builder "  };"
        appendLine builder "  const readStoredSurfaceMode = () => {"
        appendLine builder "    try { return window.localStorage.getItem(surfaceModeStorageKey) || defaultSurfaceMode; } catch (_) { return defaultSurfaceMode; }"
        appendLine builder "  };"
        appendLine builder "  const persistSurfaceMode = (mode) => {"
        appendLine builder "    try { window.localStorage.setItem(surfaceModeStorageKey, mode); } catch (_) { }"
        appendLine builder "  };"
        appendLine builder "  const applySurfaceMode = (mode) => {"
        appendLine builder "    if (!pathDocument) { return; }"
        appendLine builder "    const normalizedMode = mode === 'full' ? 'full' : 'thumbnail';"
        appendLine builder "    pathDocument.dataset.surfaceMode = normalizedMode;"
        appendLine builder "    persistSurfaceMode(normalizedMode);"
        appendLine builder "    surfaceButtons.forEach((button) => {"
        appendLine builder "      const isActive = button.dataset.surfaceMode === normalizedMode;"
        appendLine builder "      button.classList.toggle('is-active', isActive);"
        appendLine builder "      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');"
        appendLine builder "    });"
        appendLine builder "    window.requestAnimationFrame(() => syncWidth());"
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
        appendLine builder "    columnElements.forEach((column) => {"
        appendLine builder "      const lensKeys = (column.dataset.lensKeys || '').split(',').filter(Boolean);"
        appendLine builder "      const shouldShow = lensKeys.some((lensKey) => activeLensKeys.includes(lensKey));"
        appendLine builder "      column.hidden = !shouldShow;"
        appendLine builder "    });"
        appendLine builder "    persistLensKeys(activeLensKeys);"
        appendLine builder "    window.requestAnimationFrame(() => syncWidth());"
        appendLine builder "  };"
        appendLine builder "  const stepIndexAtOrBefore = (targets, currentLeft) => {"
        appendLine builder "    let index = 0;"
        appendLine builder "    for (let i = 0; i < targets.length; i += 1) {"
        appendLine builder "      if (targets[i] <= currentLeft + 4) { index = i; }"
        appendLine builder "    }"
        appendLine builder "    return index;"
        appendLine builder "  };"
        appendLine builder "  let syncingScroll = false;"
        appendLine builder "  let animatingScroll = false;"
        appendLine builder "  let animationFrameId = 0;"
        appendLine builder "  let currentStepMetrics = { targets: [0], logicalMaxTarget: 0 };"
        appendLine builder "  const baseFlowPaddingRight = flow ? (parseFloat(window.getComputedStyle(flow).paddingRight) || 0) : 0;"
        appendLine builder "  const measureStepMetrics = () => {"
        appendLine builder "    const columns = Array.from(flow.querySelectorAll('.nm-column')).filter((column) => !column.hidden);"
        appendLine builder "    if (columns.length === 0) { return { targets: [0], logicalMaxTarget: 0 }; }"
        appendLine builder "    const firstOffset = columns[0].offsetLeft;"
        appendLine builder "    const normalizedTargets = columns.map((column) => Math.max(0, Math.round(column.offsetLeft - firstOffset)));"
        appendLine builder "    const contentRightEdge = Math.max(...columns.map((column, index) => normalizedTargets[index] + Math.round(column.getBoundingClientRect().width)));"
        appendLine builder "    const maxStartIndexCandidate = normalizedTargets.findIndex((target) => contentRightEdge - target <= ((viewport?.clientWidth ?? 0) + 1));"
        appendLine builder "    const maxStartIndex = maxStartIndexCandidate >= 0 ? maxStartIndexCandidate : Math.max(0, normalizedTargets.length - 1);"
        appendLine builder "    const targets = normalizedTargets.slice(0, maxStartIndex + 1);"
        appendLine builder "    const logicalMaxTarget = targets.length > 0 ? targets[targets.length - 1] : 0;"
        appendLine builder "    return { targets, logicalMaxTarget };"
        appendLine builder "  };"
        appendLine builder "  const logicalMaxScrollLeft = () => currentStepMetrics.logicalMaxTarget;"
        appendLine builder "  const clampLeft = (left) => Math.max(0, Math.min(left, logicalMaxScrollLeft()));"
        appendLine builder "  const syncWidth = () => {"
        appendLine builder "    if (!flow || !viewport || !scrollbar || !scrollbarContent) { return; }"
        appendLine builder "    syncSlotHeights();"
        appendLine builder "    flow.style.paddingRight = `${baseFlowPaddingRight}px`;"
        appendLine builder "    currentStepMetrics = measureStepMetrics();"
        appendLine builder "    const nativeMaxScroll = Math.max(0, viewport.scrollWidth - viewport.clientWidth);"
        appendLine builder "    const extraTrailingSpace = Math.max(0, currentStepMetrics.logicalMaxTarget - nativeMaxScroll);"
        appendLine builder "    flow.style.paddingRight = `${baseFlowPaddingRight + extraTrailingSpace}px`;"
        appendLine builder "    const actualMaxScroll = Math.max(0, viewport.scrollWidth - viewport.clientWidth);"
        appendLine builder "    const clampedTargets = currentStepMetrics.targets.map((target) => Math.min(target, actualMaxScroll));"
        appendLine builder "    const dedupedTargets = clampedTargets.filter((target, index, array) => index === 0 || target > array[index - 1]);"
        appendLine builder "    currentStepMetrics = {"
        appendLine builder "      targets: dedupedTargets.length > 0 ? dedupedTargets : [0],"
        appendLine builder "      logicalMaxTarget: dedupedTargets.length > 0 ? dedupedTargets[dedupedTargets.length - 1] : 0"
        appendLine builder "    };"
        appendLine builder "    scrollbarContent.style.width = `${Math.ceil(scrollbar.offsetWidth + currentStepMetrics.logicalMaxTarget)}px`;"
        appendLine builder "    if (!syncingScroll) { scrollbar.scrollLeft = clampLeft(viewport.scrollLeft); }"
        appendLine builder "    updateButtonState();"
        appendLine builder "  };"
        appendLine builder "  const refreshScrollMetrics = () => {"
        appendLine builder "    syncWidth();"
        appendLine builder "    return currentStepMetrics;"
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
        appendLine builder "  const scrollToStart = () => { refreshScrollMetrics(); animateScrollToColumn(0); };"
        appendLine builder "  const scrollToEnd = () => { refreshScrollMetrics(); animateScrollToColumn(logicalMaxScrollLeft()); };"
        appendLine builder "  const scrollByOneColumn = (direction) => {"
        appendLine builder "    const targets = refreshScrollMetrics().targets;"
        appendLine builder "    if (targets.length === 0) { return; }"
        appendLine builder "    const currentIndex = stepIndexAtOrBefore(targets, viewport.scrollLeft);"
        appendLine builder "    const targetIndex = Math.max(0, Math.min(targets.length - 1, currentIndex + direction));"
        appendLine builder "    animateScrollToColumn(targets[targetIndex]);"
        appendLine builder "  };"
        appendLine builder "  viewModeButtons.forEach((button) => {"
        appendLine builder "    button.addEventListener('click', () => applyViewMode(button.dataset.viewMode || 'detailed'));"
        appendLine builder "  });"
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
        appendLine builder "  surfaceButtons.forEach((button) => {"
        appendLine builder "    button.addEventListener('click', () => applySurfaceMode(button.dataset.surfaceMode || defaultSurfaceMode));"
        appendLine builder "  });"
        appendLine builder "  if (startButton) { startButton.addEventListener('click', scrollToStart); }"
        appendLine builder "  if (previousButton) { previousButton.addEventListener('click', () => scrollByOneColumn(-1)); }"
        appendLine builder "  if (nextButton) { nextButton.addEventListener('click', () => scrollByOneColumn(1)); }"
        appendLine builder "  if (endButton) { endButton.addEventListener('click', scrollToEnd); }"
        appendLine builder "  if (scrollbar) {"
        appendLine builder "    scrollbar.addEventListener('scroll', () => {"
        appendLine builder "      if (syncingScroll || animatingScroll) { return; }"
        appendLine builder "      cancelScrollAnimation();"
        appendLine builder "      setSyncedScrollLeft(scrollbar.scrollLeft);"
        appendLine builder "    });"
        appendLine builder "  }"
        appendLine builder "  if (viewport) {"
        appendLine builder "    viewport.addEventListener('scroll', () => {"
        appendLine builder "      if (syncingScroll || animatingScroll) { return; }"
        appendLine builder "      cancelScrollAnimation();"
        appendLine builder "      syncingScroll = true;"
        appendLine builder "      scrollbar.scrollLeft = clampLeft(viewport.scrollLeft);"
        appendLine builder "      syncingScroll = false;"
        appendLine builder "      updateButtonState();"
        appendLine builder "    });"
        appendLine builder "  }"
        appendLine builder "  const openOverlay = (surfaceActivator) => {"
        appendLine builder "    if (!overlay || !overlaySurface || !overlayTitle) { return; }"
        appendLine builder "    const rendering = surfaceActivator.querySelector('.nm-column__surface-rendering');"
        appendLine builder "    if (!rendering) { return; }"
        appendLine builder "    overlaySurface.innerHTML = '';"
        appendLine builder "    overlaySurface.appendChild(rendering.cloneNode(true));"
        appendLine builder "    overlayTitle.textContent = surfaceActivator.dataset.surfaceLabel || 'Surface';"
        appendLine builder "    overlay.classList.add('is-open');"
        appendLine builder "    overlay.hidden = false;"
        appendLine builder "  };"
        appendLine builder "  const closeOverlay = () => {"
        appendLine builder "    if (!overlay || !overlaySurface) { return; }"
        appendLine builder "    overlay.classList.remove('is-open');"
        appendLine builder "    overlay.hidden = true;"
        appendLine builder "    overlaySurface.innerHTML = '';"
        appendLine builder "  };"
        appendLine builder "  const closePillPopovers = () => {"
        appendLine builder "    pillWraps.forEach((wrap) => { delete wrap.dataset.open; });"
        appendLine builder "  };"
        appendLine builder "  const openPillPopover = (pillWrap) => {"
        appendLine builder "    pillWraps.forEach((wrap) => {"
        appendLine builder "      if (wrap === pillWrap) {"
        appendLine builder "        wrap.dataset.open = 'true';"
        appendLine builder "      } else {"
        appendLine builder "        delete wrap.dataset.open;"
        appendLine builder "      }"
        appendLine builder "    });"
        appendLine builder "  };"
        appendLine builder "  pillWraps.forEach((pillWrap) => {"
        appendLine builder "    const pill = pillWrap.querySelector('[data-testid=\"nm-column-pill\"]');"
        appendLine builder "    const closeButton = pillWrap.querySelector('[data-testid=\"nm-column-pill-popover-close\"]');"
        appendLine builder "    pillWrap.addEventListener('mouseenter', () => openPillPopover(pillWrap));"
        appendLine builder "    pillWrap.addEventListener('mouseleave', () => closePillPopovers());"
        appendLine builder "    pillWrap.addEventListener('focusin', () => openPillPopover(pillWrap));"
        appendLine builder "    pillWrap.addEventListener('focusout', () => {"
        appendLine builder "      window.requestAnimationFrame(() => {"
        appendLine builder "        if (!pillWrap.contains(document.activeElement)) {"
        appendLine builder "          delete pillWrap.dataset.open;"
        appendLine builder "        }"
        appendLine builder "      });"
        appendLine builder "    });"
        appendLine builder "    if (pill) {"
        appendLine builder "      pill.addEventListener('click', (event) => {"
        appendLine builder "        event.preventDefault();"
        appendLine builder "        event.stopPropagation();"
        appendLine builder "        openPillPopover(pillWrap);"
        appendLine builder "      });"
        appendLine builder "    }"
        appendLine builder "    if (closeButton) {"
        appendLine builder "      closeButton.addEventListener('click', (event) => {"
        appendLine builder "        event.preventDefault();"
        appendLine builder "        event.stopPropagation();"
        appendLine builder "        closePillPopovers();"
        appendLine builder "      });"
        appendLine builder "    }"
        appendLine builder "  });"
        appendLine builder "  document.querySelectorAll('[data-action=\"open-surface-overlay\"]').forEach((surfaceActivator) => {"
        appendLine builder "    surfaceActivator.addEventListener('click', () => openOverlay(surfaceActivator));"
        appendLine builder "    surfaceActivator.addEventListener('keydown', (event) => {"
        appendLine builder "      if (event.key === 'Enter' || event.key === ' ') {"
        appendLine builder "        event.preventDefault();"
        appendLine builder "        openOverlay(surfaceActivator);"
        appendLine builder "      }"
        appendLine builder "    });"
        appendLine builder "  });"
        appendLine builder "  if (overlayClose) { overlayClose.addEventListener('click', closeOverlay); }"
        appendLine builder "  if (overlay) {"
        appendLine builder "    overlay.addEventListener('click', (event) => {"
        appendLine builder "      if (event.target === overlay) { closeOverlay(); }"
        appendLine builder "    });"
        appendLine builder "  }"
        appendLine builder "  document.addEventListener('keydown', (event) => {"
        appendLine builder "    if (event.key === 'Escape') { closePillPopovers(); closeOverlay(); }"
        appendLine builder "  });"
        appendLine builder "  document.addEventListener('click', (event) => {"
        appendLine builder "    const target = event.target;"
        appendLine builder "    if (!(target instanceof Node)) { return; }"
        appendLine builder "    const clickedInsidePill = pillWraps.some((pillWrap) => pillWrap.contains(target));"
        appendLine builder "    if (!clickedInsidePill) { closePillPopovers(); }"
        appendLine builder "  });"
        appendLine builder "  let updateStatusTimer = 0;"
        appendLine builder "  const setUpdateStatusState = (state) => {"
        appendLine builder "    if (!updateStatus) { return; }"
        appendLine builder "    if (state) { updateStatus.dataset.state = state; } else { delete updateStatus.dataset.state; }"
        appendLine builder "  };"
        appendLine builder "  const clearUpdateStatusTimer = () => {"
        appendLine builder "    if (updateStatusTimer !== 0) {"
        appendLine builder "      window.clearInterval(updateStatusTimer);"
        appendLine builder "      updateStatusTimer = 0;"
        appendLine builder "    }"
        appendLine builder "  };"
        appendLine builder "  const setUpdateStatusMessage = (message, state) => {"
        appendLine builder "    if (!updateStatus) { return; }"
        appendLine builder "    clearUpdateStatusTimer();"
        appendLine builder "    updateStatus.textContent = message;"
        appendLine builder "    setUpdateStatusState(state);"
        appendLine builder "  };"
        appendLine builder "  const parseUpdatedAt = (updatedAtIso) => {"
        appendLine builder "    const parsed = new Date(updatedAtIso || '');"
        appendLine builder "    return Number.isNaN(parsed.getTime()) ? null : parsed;"
        appendLine builder "  };"
        appendLine builder "  const formatUpdatedAtClock = (updatedAtIso) => {"
        appendLine builder "    const parsed = parseUpdatedAt(updatedAtIso);"
        appendLine builder "    if (!parsed) { return null; }"
        appendLine builder "    return new Intl.DateTimeFormat(undefined, { hour: 'numeric', minute: '2-digit' }).format(parsed);"
        appendLine builder "  };"
        appendLine builder "  const formatUpdatedAtAge = (updatedAtIso) => {"
        appendLine builder "    const parsed = parseUpdatedAt(updatedAtIso);"
        appendLine builder "    if (!parsed) { return null; }"
        appendLine builder "    const diffMs = Math.max(0, Date.now() - parsed.getTime());"
        appendLine builder "    const totalMinutes = Math.floor(diffMs / 60000);"
        appendLine builder "    if (totalMinutes < 1) { return 'just now'; }"
        appendLine builder "    if (totalMinutes < 60) { return `${totalMinutes} minute${totalMinutes === 1 ? '' : 's'} ago`; }"
        appendLine builder "    const totalHours = Math.floor(totalMinutes / 60);"
        appendLine builder "    if (totalHours < 24) { return `${totalHours} hour${totalHours === 1 ? '' : 's'} ago`; }"
        appendLine builder "    const totalDays = Math.floor(totalHours / 24);"
        appendLine builder "    return `${totalDays} day${totalDays === 1 ? '' : 's'} ago`;"
        appendLine builder "  };"
        appendLine builder "  const setUpdateStatusTimestamp = (updatedAtIso, state, prefix) => {"
        appendLine builder "    if (!updateStatus) { return; }"
        appendLine builder "    clearUpdateStatusTimer();"
        appendLine builder "    const render = () => {"
        appendLine builder "      const formattedClock = formatUpdatedAtClock(updatedAtIso);"
        appendLine builder "      const formattedAge = formatUpdatedAtAge(updatedAtIso);"
        appendLine builder "      if (!formattedClock || !formattedAge) {"
        appendLine builder "        setUpdateStatusMessage(prefix || 'Update status unavailable.', state);"
        appendLine builder "        return;"
        appendLine builder "      }"
        appendLine builder "      const lead = prefix ? `${prefix} | ` : '';"
        appendLine builder "      updateStatus.textContent = `${lead}UPDATED: ${formattedClock} | ${formattedAge}`;"
        appendLine builder "      setUpdateStatusState(state);"
        appendLine builder "    };"
        appendLine builder "    render();"
        appendLine builder "    updateStatusTimer = window.setInterval(render, 30000);"
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
        appendLine builder "  applyViewMode((pathDocument && pathDocument.dataset.viewMode) || 'detailed');"
        appendLine builder "  applySurfaceMode(readStoredSurfaceMode());"
        appendLine builder "  applyLensFilters(readStoredLensKeys());"
        appendLine builder "  applyUpdateMode(readStoredUpdateMode());"
        appendLine builder "  syncWidth();"
        appendLine builder "  window.requestAnimationFrame(() => syncWidth());"
        appendLine builder "  window.addEventListener('resize', syncWidth);"
        appendLine builder "  window.addEventListener('load', syncWidth);"
        appendLine builder "  if (window.ResizeObserver && flow) {"
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
            appendLine builder "  let updateCheckInFlight = false;"
            appendLine builder "  const buildUpdateManifestUrl = () => {"
            appendLine builder "    const pathName = window.location.pathname;"
            appendLine builder "    const manifestPath = /\\.html?$/i.test(pathName) ? pathName.replace(/\\.html?$/i, '.update.js') : `${pathName}.update.js`;"
            appendLine builder "    return `${manifestPath}?ts=${Date.now()}`;"
            appendLine builder "  };"
            appendLine builder "  const loadUpdateManifest = () => {"
            appendLine builder "    return new Promise((resolve, reject) => {"
            appendLine builder "      delete window.__llNmPathUpdateManifest;"
            appendLine builder "      const script = document.createElement('script');"
            appendLine builder "      script.async = true;"
            appendLine builder "      script.src = buildUpdateManifestUrl();"
            appendLine builder "      script.onload = () => {"
            appendLine builder "        const manifest = window.__llNmPathUpdateManifest || null;"
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
            appendLine builder "      setUpdateStatusTimestamp(pendingUpdateManifest.updatedAt || currentArtifactUpdatedAt, 'auto', 'UPDATE DETECTED');"
            appendLine builder "      window.location.reload();"
            appendLine builder "    }"
            appendLine builder "  };"
            appendLine builder "  const checkForArtifactUpdate = async () => {"
            appendLine builder "    if (updateCheckInFlight) { return; }"
            appendLine builder "    updateCheckInFlight = true;"
            appendLine builder "    try {"
            appendLine builder "      const manifest = await loadUpdateManifest();"
            appendLine builder "      if (!manifest || !manifest.version) {"
            appendLine builder "        setUpdateStatusMessage('Update monitor unavailable for this artifact.', 'error');"
            appendLine builder "        return;"
            appendLine builder "      }"
            appendLine builder "      if (manifest.version !== currentArtifactVersion) {"
            appendLine builder "        pendingUpdateManifest = manifest;"
            appendLine builder "        setRefreshPending(true);"
            appendLine builder "        setUpdateStatusTimestamp(manifest.updatedAt || currentArtifactUpdatedAt, 'available', 'UPDATE AVAILABLE');"
            appendLine builder "        maybeApplyPendingUpdate();"
            appendLine builder "      } else if (!pendingUpdateManifest) {"
            appendLine builder "        setRefreshPending(false);"
            appendLine builder "        setUpdateStatusTimestamp(currentArtifactUpdatedAt, null, null);"
            appendLine builder "      }"
            appendLine builder "    } catch (_) {"
            appendLine builder "      setUpdateStatusMessage('Update monitor unavailable for this artifact.', 'error');"
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
            appendLine builder "  setUpdateStatusTimestamp(currentArtifactUpdatedAt, null, null);"
            appendLine builder "  checkForArtifactUpdate();"
            appendLine builder "  window.setInterval(checkForArtifactUpdate, updatePollIntervalMs);"
        | None ->
            appendLine builder "  setRefreshPending(false);"
            appendLine builder "  setUpdateStatusMessage('Live update monitor disabled for this artifact.', null);"

        appendLine builder "})();"
        appendLine builder "</script>"

    let renderUpdateManifestScript (updateState: NmPathUpdateState) =
        let builder = StringBuilder()
        appendLine builder $"window.__llNmPathUpdateManifest = {{ version: {jsonString updateState.Version}, updatedAt: {jsonString updateState.UpdatedAtUtc} }};"
        builder.ToString()

    /// Renders a self-contained HTML document for one current NM path.
    let renderDocumentWithUpdateState (pathState: NmPathState) (updateState: NmPathUpdateState option) =
        let builder = StringBuilder()

        appendLine builder "<!DOCTYPE html>"
        appendLine builder "<html lang=\"en\">"
        appendLine builder "<head>"
        appendLine builder "<meta charset=\"utf-8\">"
        appendLine builder "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
        appendLine builder $"<title>{htmlEncode pathState.Title}</title>"
        appendLine builder (ScreenHtmlRenderer.renderStyleBlock ())
        appendLine builder (SliceHtmlRenderer.renderStyleBlock ())
        renderStyles builder
        appendLine builder "</head>"
        appendLine builder "<body>"
        appendLine builder "<div id=\"nm-surface-overlay\" class=\"nm-surface-overlay\" data-testid=\"nm-surface-overlay\" hidden>"
        appendLine builder "<div class=\"nm-surface-overlay__dialog\">"
        appendLine builder "<div class=\"nm-surface-overlay__header\">"
        appendLine builder "<h2 id=\"nm-surface-overlay-title\" class=\"nm-surface-overlay__title\">Surface</h2>"
        appendLine builder "<button id=\"nm-surface-overlay-close\" class=\"nm-surface-overlay__close\" data-testid=\"nm-surface-overlay-close\" type=\"button\">Close</button>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-surface-overlay__body\">"
        appendLine builder "<div id=\"nm-surface-overlay-surface\" class=\"nm-surface-overlay__surface\"></div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "<main class=\"nm-path-document\" data-testid=\"nm-path-document\" data-view-mode=\"detailed\" data-surface-mode=\"thumbnail\">"
        appendLine builder "<header class=\"nm-path-document__header\">"
        appendLine builder "<div class=\"nm-path-document__topline\">"
        appendLine builder "<div class=\"nm-path-document__title-zone\">"
        appendLine builder "<div class=\"nm-path-document__identity-row\">"
        appendLine builder "<span class=\"nm-path-document__app-pill\" data-testid=\"nm-path-app-pill\">LaundryLog</span>"
        appendLine builder $"<div class=\"nm-path-document__eyebrow\">{htmlEncode pathState.PathId} nm path</div>"
        appendLine builder "</div>"
        appendLine builder $"<h1 class=\"nm-path-document__title\">{htmlEncode pathState.Title}</h1>"
        appendLine builder "<details class=\"nm-path-document__context\" data-testid=\"nm-path-scenario\">"
        appendLine builder "<summary data-testid=\"nm-path-scenario-summary\">"
        appendLine builder "<span class=\"nm-path-document__context-label\">Scenario</span>"
        appendLine builder $"<span class=\"nm-path-document__context-title\">{htmlEncode pathState.ScenarioLabel}</span>"
        appendLine builder "</summary>"
        appendLine builder "<div class=\"nm-path-document__context-panel\" data-testid=\"nm-path-scenario-panel\">"
        appendLine builder $"<p class=\"nm-path-document__description\">{htmlEncode pathState.Description}</p>"
        appendLine builder "<ul class=\"nm-path-document__assumptions\">"

        pathState.Assumptions
        |> List.iter (fun assumption -> appendLine builder $"<li>{htmlEncode assumption}</li>")

        appendLine builder "</ul>"
        appendLine builder "</div>"
        appendLine builder "</details>"
        appendLine builder "</div>"
        appendLine builder "<div class=\"nm-path-document__header-controls\">"
        appendLine builder "<section class=\"nm-path-document__control-group\" aria-label=\"Path viewer controls\">"
        appendLine builder "<div class=\"nm-path-document__control-label\">View</div>"
        appendLine builder "<div class=\"nm-path-document__control-buttons\">"
        appendLine builder "<button class=\"nm-path-toggle nm-path-view-toggle\" type=\"button\" data-testid=\"nm-path-view-toggle\" data-view-mode=\"summary\" aria-pressed=\"false\">Summary</button>"
        appendLine builder "<button class=\"nm-path-toggle nm-path-view-toggle is-active\" type=\"button\" data-testid=\"nm-path-view-toggle\" data-view-mode=\"detailed\" aria-pressed=\"true\">Detailed</button>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"nm-path-document__control-group\" aria-label=\"Lens visibility controls\">"
        appendLine builder "<div class=\"nm-path-document__control-label\">Lenses</div>"
        appendLine builder "<div class=\"nm-path-document__control-buttons\">"

        [ NmLensKind.Lifecycle; NmLensKind.Runtime; NmLensKind.Screen; NmLensKind.Aem ]
        |> List.iter (fun lensKind ->
            appendLine builder $"<button class=\"nm-path-toggle nm-path-lens-toggle is-active\" type=\"button\" data-testid=\"nm-path-lens-toggle\" data-lens-key=\"{htmlEncode (NmLensKind.domKey lensKind)}\" aria-pressed=\"true\">{htmlEncode (NmLensKind.label lensKind)}</button>")

        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"nm-path-document__control-group\" aria-label=\"Surface presentation controls\">"
        appendLine builder "<div class=\"nm-path-document__control-label\">Surface</div>"
        appendLine builder "<div class=\"nm-path-document__control-buttons\">"
        appendLine builder "<button class=\"nm-path-toggle nm-path-surface-toggle is-active\" type=\"button\" data-testid=\"nm-path-surface-toggle\" data-surface-mode=\"thumbnail\" aria-pressed=\"true\">Thumbnail</button>"
        appendLine builder "<button class=\"nm-path-toggle nm-path-surface-toggle\" type=\"button\" data-testid=\"nm-path-surface-toggle\" data-surface-mode=\"full\" aria-pressed=\"false\">Full</button>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"nm-path-document__control-group\" aria-label=\"Path update controls\">"
        appendLine builder "<div class=\"nm-path-document__control-label\">Updates</div>"
        appendLine builder "<div class=\"nm-path-document__control-buttons\">"
        appendLine builder "<button class=\"nm-path-update-toggle\" type=\"button\" data-testid=\"nm-path-update-toggle\" data-update-mode=\"notify\" aria-pressed=\"true\">Notify Me</button>"
        appendLine builder "<button class=\"nm-path-update-toggle\" type=\"button\" data-testid=\"nm-path-update-toggle\" data-update-mode=\"auto\" aria-pressed=\"false\">Auto Refresh</button>"
        appendLine builder "<button id=\"nm-path-refresh-now\" class=\"nm-path-update-refresh\" data-testid=\"nm-path-update-refresh\" type=\"button\" hidden>Refresh Now</button>"
        appendLine builder "</div>"
        appendLine builder "<div id=\"nm-path-update-status\" class=\"nm-path-document__update-status\" data-testid=\"nm-path-update-status\" aria-live=\"polite\"></div>"
        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</div>"
        appendLine builder "</header>"
        appendLine builder "<section class=\"nm-path-scroll-controls\">"
        appendLine builder "<button id=\"nm-path-nav-start\" class=\"nm-path-nav-button\" data-testid=\"nm-path-nav-start\" type=\"button\" aria-label=\"Back to beginning\"><span class=\"nm-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"nm-path-nav-button__bar\"></span><span class=\"nm-path-nav-button__bar\"></span><span class=\"nm-path-nav-button__chevron nm-path-nav-button__chevron--left\"></span></span></button>"
        appendLine builder "<button id=\"nm-path-nav-previous\" class=\"nm-path-nav-button\" data-testid=\"nm-path-nav-previous\" type=\"button\" aria-label=\"Show the previous column\"><span class=\"nm-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"nm-path-nav-button__bar\"></span><span class=\"nm-path-nav-button__chevron nm-path-nav-button__chevron--left\"></span></span></button>"
        appendLine builder "<div id=\"nm-path-scrollbar\" class=\"nm-path-scrollbar\" data-testid=\"nm-path-scrollbar\" aria-label=\"NM path horizontal scroll rail\">"
        appendLine builder "<div id=\"nm-path-scrollbar-content\" class=\"nm-path-scrollbar__content\"></div>"
        appendLine builder "</div>"
        appendLine builder "<button id=\"nm-path-nav-next\" class=\"nm-path-nav-button\" data-testid=\"nm-path-nav-next\" type=\"button\" aria-label=\"Show the next column\"><span class=\"nm-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"nm-path-nav-button__chevron nm-path-nav-button__chevron--right\"></span><span class=\"nm-path-nav-button__bar\"></span></span></button>"
        appendLine builder "<button id=\"nm-path-nav-end\" class=\"nm-path-nav-button\" data-testid=\"nm-path-nav-end\" type=\"button\" aria-label=\"Go to end\"><span class=\"nm-path-nav-button__icon\" aria-hidden=\"true\"><span class=\"nm-path-nav-button__chevron nm-path-nav-button__chevron--right\"></span><span class=\"nm-path-nav-button__bar\"></span><span class=\"nm-path-nav-button__bar\"></span></span></button>"
        appendLine builder "</section>"
        appendLine builder "<section class=\"nm-path-stage\">"
        appendLine builder "<div id=\"nm-path-flow-viewport\" class=\"nm-path-flow-viewport\" data-testid=\"nm-path-flow-viewport\">"
        appendLine builder "<section id=\"nm-path-flow\" class=\"nm-path-flow\" data-testid=\"nm-path-flow\">"

        pathState.Columns
        |> List.iteri (renderColumn builder)

        appendLine builder "</section>"
        appendLine builder "</div>"
        appendLine builder "</section>"
        appendLine builder (SliceHtmlRenderer.renderScriptBlock ())
        renderScript builder pathState updateState
        appendLine builder "</main>"
        appendLine builder "</body>"
        appendLine builder "</html>"
        builder.ToString()

    let renderDocument (pathState: NmPathState) =
        renderDocumentWithUpdateState pathState (Some(defaultUpdateState ()))
