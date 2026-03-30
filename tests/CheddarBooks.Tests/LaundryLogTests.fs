namespace CheddarBooks.Tests

open Expecto
open FnTools.FnHCI.UI
open CheddarBooks.LaundryLog
open CheddarBooks.LaundryLog.UI

[<RequireQualifiedAccess>]
module LaundryLogTests =
    let private unwrap description result =
        match result with
        | Ok value -> value
        | Error message -> failtest $"Expected a valid {description}. {message}"

    let tests =
        testList
            "laundrylog"
            [ testCase "Expense kinds map to stable slugs" (fun () ->
                  Expect.equal (ExpenseKind.slug ExpenseKind.Washer) "washer" "Expected the washer expense slug."
                  Expect.equal (ExpenseKind.displayName ExpenseKind.Supplies) "Supplies" "Expected the supplies display name.")

              testCase "Payment methods map to stable slugs" (fun () ->
                  Expect.equal (PaymentMethod.slug PaymentMethod.Card) "card" "Expected the card payment slug."
                  Expect.equal (PaymentMethod.displayName PaymentMethod.Points) "Points" "Expected the points payment label.")

              testCase "Location names reject characters outside the explicit allowlist" (fun () ->
                  match LocationName.tryCreate "Main Street Laundry!" with
                  | Ok _ -> failtest "Expected disallowed punctuation to be rejected."
                  | Error message -> Expect.stringContains message "ASCII letters, digits" "Expected the allowlist validation message.")

              testCase "Session flow stages map to stable path slugs" (fun () ->
                  Expect.equal
                      (SessionFlowStage.pathSlug SessionFlowStage.NewSession)
                      "path1-1-new-session"
                      "Expected the new-session path slug."

                  Expect.equal
                      (SessionFlowStage.pathSlug SessionFlowStage.EntryForm)
                      "path1-3-entry-form"
                      "Expected the entry-form path slug.")

              testCase "LaundryLog shell creates the first stable app shell" (fun () ->
                  match LaundryLogShell.tryCreate () with
                  | Error message -> failtest $"Expected a valid LaundryLog shell. {message}"
                  | Ok shell ->
                      let applicationShell = LaundryLogShell.applicationShell shell
                      let defaultView = ApplicationShell.defaultView applicationShell |> ViewId.value
                      let declaredViews = ApplicationShell.views applicationShell |> List.map (fun view -> ViewId.value view.ViewId)

                      Expect.equal defaultView "new-session" "Expected the new-session view to be the default entry point."
                      Expect.sequenceEqual declaredViews [ "new-session"; "entry-form" ] "Expected the first two concrete LaundryLog views.")

              testCase "Primitive control ids use stable lowercase slugs" (fun () ->
                  match PrimitiveControlId.tryCreate "Set-Location" with
                  | Ok _ -> failtest "Expected uppercase primitive control ids to be rejected."
                  | Error message -> Expect.stringContains message "lowercase" "Expected the lowercase validation message."

                  match PrimitiveControlId.tryCreate "set-location" with
                  | Error message -> failtest $"Expected a valid primitive control id. {message}"
                  | Ok controlId -> Expect.equal (PrimitiveControlId.value controlId) "set-location" "Expected the primitive control id slug.")

              testCase "Option groups reject duplicate choice ids" (fun () ->
                  let controlId =
                      PrimitiveControlId.tryCreate "machine-type"
                      |> unwrap "primitive control id"

                  let washerChoice =
                      OptionChoiceState.tryCreate
                          (PrimitiveControlId.tryCreate "washer" |> unwrap "washer choice id")
                          "Washer"
                          true
                          true
                      |> unwrap "washer choice"

                  let duplicateWasherChoice =
                      OptionChoiceState.tryCreate
                          (PrimitiveControlId.tryCreate "washer" |> unwrap "duplicate washer choice id")
                          "Washer Again"
                          false
                          true
                      |> unwrap "duplicate washer choice"

                  match OptionGroupState.tryCreate controlId (Some "Machine Type") [ washerChoice; duplicateWasherChoice ] with
                  | Ok _ -> failtest "Expected duplicate option choice ids to be rejected."
                  | Error message -> Expect.stringContains message "duplicate" "Expected the duplicate-id validation message.")

              testCase "Money inputs carry placeholder and quick-fill labels" (fun () ->
                  let controlId =
                      PrimitiveControlId.tryCreate "price-input"
                      |> unwrap "price input control id"

                  match MoneyInputState.tryCreate controlId "$" None "0.00" [ "$2.50"; "$3.00" ] [] with
                  | Error message -> failtest $"Expected a valid money-input state. {message}"
                  | Ok moneyInput ->
                      Expect.equal moneyInput.PlaceholderText "0.00" "Expected the price placeholder text."
                      Expect.sequenceEqual moneyInput.QuickFillLabels [ "$2.50"; "$3.00" ] "Expected the quick-fill labels to stay stable.")

              testCase "Entry cards keep title detail and amount text" (fun () ->
                  let controlId =
                      PrimitiveControlId.tryCreate "entry-1-washer"
                      |> unwrap "entry card control id"

                  match EntryCardState.tryCreate controlId "Washer" (Some "Qty 1 • Card") (Some "$3.00") with
                  | Error message -> failtest $"Expected a valid entry card state. {message}"
                  | Ok entryCard ->
                      Expect.equal entryCard.TitleText "Washer" "Expected the entry-card title text."
                      Expect.equal entryCard.DetailText (Some "Qty 1 • Card") "Expected the entry-card detail text."
                      Expect.equal entryCard.AmountText (Some "$3.00") "Expected the entry-card amount text.")

              testCase "Penpot-backed new-session example starts with location unset" (fun () ->
                  let state = PrimitiveStateExamples.newSessionAwaitingLocation ()

                  Expect.equal state.Header.Title "LaundryLog" "Expected the Penpot-backed screen title."
                  Expect.equal state.LocationInput.ValueText None "Expected the first new-session example to begin without a location value."
                  Expect.equal state.GpsAction.Label "Use GPS Location" "Expected the GPS action label from the Penpot screen."
                  Expect.equal state.SetLocationAction.IsEnabled false "Expected Set Location to remain disabled before location entry.")

              testCase "Penpot-backed location-entered example enables set-location" (fun () ->
                  let state = PrimitiveStateExamples.newSessionLocationEntered ()

                  Expect.equal
                      state.LocationInput.ValueText
                      (Some "Love's #123 - Springfield, OH")
                      "Expected the entered location value to stay stable."

                  Expect.equal state.SetLocationAction.IsEnabled true "Expected Set Location to become enabled after location entry.")

              testCase "Penpot-backed entry-form example keeps washer and card selected" (fun () ->
                  let state = PrimitiveStateExamples.entryFormWasherCardDraft ()

                  let selectedMachineLabels =
                      state.MachineTypeOptions.Choices
                      |> List.filter (fun choice -> choice.IsSelected)
                      |> List.map (fun choice -> choice.Label)

                  let selectedPaymentLabels =
                      state.PaymentOptions.Choices
                      |> List.filter (fun choice -> choice.IsSelected)
                      |> List.map (fun choice -> choice.Label)

                  Expect.sequenceEqual selectedMachineLabels [ "Washer" ] "Expected washer to be the selected machine type."
                  Expect.sequenceEqual selectedPaymentLabels [ "Card" ] "Expected card to be the selected payment type."
                  Expect.equal
                      state.PriceInput.QuickFillLabels
                      [ "Historical $3.00"; "Last used $2.75"; "Community $3.50" ]
                      "Expected the historical Penpot-backed helper values."

                  Expect.equal state.PriceInput.QuarterAdjustButtons.Length 2 "Expected the quarter adjust pair for price nudging."
                  Expect.isSome state.PaymentDetailOptions "Expected the card payment example to include progressive-disclosure details."
                  Expect.equal state.StatusChips.Length 3 "Expected the command-readiness chip row."
                  Expect.isEmpty state.RecentEntries "Expected the draft Penpot example to begin without visible entry cards.")

              testCase "New-session primitive mapping follows location command-slice state" (fun () ->
                  let noLocationState =
                      CaptureLaundryLocationCommandSliceState.create None true
                      |> PrimitiveStateMappings.newSessionFromCommandSlice

                  let enteredLocation =
                      LocationName.tryCreate "Love's #123 - Springfield, OH"
                      |> unwrap "location name"

                  let withLocationState =
                      CaptureLaundryLocationCommandSliceState.create (Some enteredLocation) true
                      |> PrimitiveStateMappings.newSessionFromCommandSlice

                  Expect.equal noLocationState.SetLocationAction.IsEnabled false "Expected Set Location to remain disabled before a draft location exists."

                  Expect.equal
                      withLocationState.LocationInput.ValueText
                      (Some "Love's #123 - Springfield, OH")
                      "Expected the command-slice draft location to appear in the mapped input."

                  Expect.equal withLocationState.SetLocationAction.IsEnabled true "Expected Set Location to become enabled when location text exists.")

              testCase "Entry-form primitive mapping combines command-slice and view-slice state" (fun () ->
                  let location =
                      LocationName.tryCreate "Love's #123 - Springfield, OH"
                      |> unwrap "location name"

                  let washerEntry =
                      VisibleLaundryExpenseViewLine.tryCreate ExpenseKind.Washer PaymentMethod.Card 1 "$3.00"
                      |> unwrap "visible washer expense line"

                  let dryerEntry =
                      VisibleLaundryExpenseViewLine.tryCreate ExpenseKind.Dryer PaymentMethod.Cash 1 "$2.50"
                      |> unwrap "visible dryer expense line"

                  let sessionView =
                      CurrentLaundrySessionViewState.tryCreate location [ washerEntry; dryerEntry ] "$5.50" (Some "2026-03-28 01:10")
                      |> unwrap "current session view"

                  let expenseCommand =
                      LogLaundryExpenseCommandSliceState.tryCreate
                          ExpenseKind.Dryer
                          1
                          (Some "2.50")
                          PaymentMethod.Card
                          [ "$2.50"; "$3.00" ]
                          true
                      |> unwrap "expense command slice"

                  let mappedState = PrimitiveStateMappings.entryFormFromSlices sessionView expenseCommand

                  let selectedMachineLabels =
                      mappedState.MachineTypeOptions.Choices
                      |> List.filter (fun choice -> choice.IsSelected)
                      |> List.map (fun choice -> choice.Label)

                  let selectedPaymentLabels =
                      mappedState.PaymentOptions.Choices
                      |> List.filter (fun choice -> choice.IsSelected)
                      |> List.map (fun choice -> choice.Label)

                  Expect.equal mappedState.Header.Subtitle (Some "Love's #123 - Springfield, OH") "Expected the active location from the ViewSlice to appear in the header."
                  Expect.sequenceEqual selectedMachineLabels [ "Dryer" ] "Expected the selected expense kind from the CommandSlice to drive machine selection."
                  Expect.sequenceEqual selectedPaymentLabels [ "Card" ] "Expected the selected payment method from the CommandSlice to drive payment selection."
                  Expect.equal mappedState.SessionTotal.ValueText "$5.50" "Expected the running total from the ViewSlice to drive the summary bar."
                  Expect.equal mappedState.RecentEntries.Length 2 "Expected visible entries from the ViewSlice to project into recent entry cards."
                  Expect.equal mappedState.RecentEntries.Head.TitleText "Washer" "Expected the first recent entry card to reflect the first visible entry."
                  Expect.equal mappedState.RecentEntries.Head.DetailText (Some "Qty 1 • Card") "Expected the first recent entry card detail text."
                  Expect.equal mappedState.RecentEntries.Tail.Head.AmountText (Some "$2.50") "Expected the second recent entry amount text.")

              testCase "Classic path renderer includes PATH 1 command and view slices" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains htmlDocument "PATH 1: Manual Location -&gt; Washer -&gt; Dryer" "Expected the PATH 1 title in the rendered document."
                  Expect.stringContains htmlDocument "COMMAND SLICE" "Expected the command-slice label in the rendered document."
                  Expect.stringContains htmlDocument "VIEW SLICE" "Expected the view-slice label in the rendered document."
                  Expect.stringContains htmlDocument "CaptureLaundryLocation" "Expected the location command name in the rendered document."
                  Expect.stringContains htmlDocument "LaundryExpenseLogged" "Expected the logged-expense event title in the rendered document."
                  Expect.stringContains htmlDocument "CurrentLaundrySession" "Expected the current-session view title in the rendered document."
                  Expect.stringContains htmlDocument "classic em" "Expected the current lens footer badge.")

              testCase "Classic path renderer includes per-slice embedded GWT cards" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains htmlDocument "slice-card__gwt-row" "Expected each slice to reserve an embedded GWT row."
                  Expect.stringContains htmlDocument "slice-card__gwt-card--command" "Expected command slices to render embedded command GWT cards."
                  Expect.stringContains htmlDocument "slice-card__gwt-card--view" "Expected view slices to render embedded view GWT cards."
                  Expect.stringContains htmlDocument "command gwt" "Expected command-slice GWT cards."
                  Expect.stringContains htmlDocument "view gwt" "Expected view-slice GWT cards."
                  Expect.stringContains htmlDocument "GIVEN" "Expected the GIVEN stage label in the GWT band."
                  Expect.stringContains htmlDocument "WHEN" "Expected the WHEN stage label in the GWT band."
                  Expect.stringContains htmlDocument "THEN" "Expected the THEN stage label in the GWT band."
                  Expect.stringContains htmlDocument "path-document__gwt-ref--command" "Expected command-side GWT references to render as command mini-blocks."
                  Expect.stringContains htmlDocument "path-document__gwt-ref--event" "Expected event references to render as event mini-blocks."
                  Expect.stringContains htmlDocument "toggle-gwt-properties" "Expected GWT mini-block property toggles."
                  Expect.stringContains htmlDocument ">CaptureLaundryLocation</div>" "Expected the first command-slice WHEN clause to use the command block title."
                  Expect.stringContains htmlDocument "project the current laundry session for the active location" "Expected the first view-slice WHEN clause.")

              testCase "Classic path renderer includes per-slice property-width controls" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains htmlDocument "Expand slice width" "Expected a per-slice width toggle label."
                  Expect.stringContains htmlDocument "toggle-slice-width" "Expected a per-slice width action hook."
                  Expect.stringContains htmlDocument "slice-card--wide" "Expected the widened slice class to be present in the renderer styles."
                  Expect.stringContains htmlDocument "--current-slice-columns" "Expected the row to track live expanded-column count.")

              testCase "Classic path renderer includes path-level property-width controls" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains htmlDocument "Expand All Width" "Expected a path-level width expand control."
                  Expect.stringContains htmlDocument "Collapse All Width" "Expected a path-level width collapse control."
                  Expect.stringContains htmlDocument "data-action=\"expand-width\"" "Expected the width expand action hook."
                  Expect.stringContains htmlDocument "data-action=\"collapse-width\"" "Expected the width collapse action hook.")

              testCase "Classic path renderer hides the view screen blocks" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.isFalse
                      (htmlDocument.Contains("Log Expense Screen - Washer And Dryer Visible"))
                      "Expected the classic Event Modeling lens to hide the screen carried by the ViewSlice."

                  Expect.stringContains htmlDocument "Set Location Screen" "Expected command-side screen blocks to stay visible.")

              testCase "UI narrative path renderer shows the view-supported screens" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.uiNarrative "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains
                      htmlDocument
                      "Log Expense Screen - Washer And Dryer Visible"
                      "Expected the UI-enriched lens to include the screen carried by the ViewSlice."

                  Expect.stringContains htmlDocument "ui lens" "Expected the screen footer badge in the UI-enriched lens.")

              testCase "Renderer stacks long property values onto an indented next line" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains
                      htmlDocument
                      "slice-block__property-line--stacked"
                      "Expected long property values to use the stacked property-line format."

                  Expect.stringContains
                      htmlDocument
                      "<span class=\"slice-block__property-key\">location_name =</span>"
                      "Expected the property key to render separately from the value."

                  Expect.stringContains
                      htmlDocument
                      "<span class=\"slice-block__property-value\">&quot;Love&#39;s #123 - Springfield, OH&quot;</span>"
                      "Expected the long property value to render in the indented value span.")

              testCase "Screen renderer includes the current base LaundryLog screens" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains htmlDocument "Screen.EntryForm - v7 Primary" "Expected the v7 primary screen surface."
                  Expect.stringContains htmlDocument "Screen.NewSession - Awaiting Location" "Expected the awaiting-location screen surface."
                  Expect.stringContains htmlDocument "Screen.NewSession - Ready To Set" "Expected the ready-to-set screen surface."
                  Expect.stringContains htmlDocument "Screen.EntryForm - Ready At Location" "Expected the ready-at-location screen surface."
                  Expect.stringContains htmlDocument "Screen.EntryForm - Washer Draft" "Expected the entry-form screen surface."
                  Expect.stringContains htmlDocument "Screen.EntryForm - Card Details Expanded" "Expected the card-details-expanded screen surface."
                  Expect.stringContains htmlDocument "Screen.EntryForm - Logged Success" "Expected the logged-success screen surface."
                  Expect.stringContains htmlDocument "ll-gps-button" "Expected the GPS location control."
                  Expect.stringContains htmlDocument "Set Location" "Expected the set-location command action."
                  Expect.stringContains htmlDocument "Log Expense" "Expected the primary entry-form action."
                  Expect.stringContains htmlDocument "Session Total" "Expected the summary bar label."
                  Expect.stringContains htmlDocument "✓ Logged!" "Expected the success button state."
                  Expect.stringContains htmlDocument "✓ Entry logged — $5.00" "Expected the logged toast copy prefix."
                  Expect.stringContains htmlDocument "CASH" "Expected the logged toast payment text.")

              testCase "Screen renderer presents the dedicated v7 surface as the primary app screen" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains htmlDocument "ll-primary-surface" "Expected the primary screen wrapper."
                  Expect.stringContains htmlDocument "Supporting State Variants" "Expected the supporting-variants section."
                  Expect.stringContains
                      htmlDocument
                      "placeholder=\"Tap &#128205; or enter location\""
                      "Expected the primary surface to carry the v7 location-card placeholder."
                  Expect.isFalse
                      (htmlDocument.Contains("value=\"Tap &#128205; or enter location\""))
                      "Expected the empty v7 location field to render as a placeholder, not as a prefilled value."

                  let primaryIndex = htmlDocument.IndexOf("ll-primary-surface")
                  let v7PrimaryIndex = htmlDocument.IndexOf("Screen.EntryForm - v7 Primary")
                  let awaitingIndex = htmlDocument.IndexOf("Screen.NewSession - Awaiting Location")

                  Expect.isGreaterThan v7PrimaryIndex primaryIndex "Expected the v7 primary name inside the primary surface."
                  Expect.isGreaterThan awaitingIndex v7PrimaryIndex "Expected the awaiting-location variant to appear after the primary surface.")

              testCase "Screen path renderer sequences the first app and screen flow" (fun () ->
                  let htmlDocument =
                      ScreenPathHtmlRenderer.renderDocument (ScreenPathHtmlExamples.path1StartupToFirstEntry ())

                  Expect.stringContains
                      htmlDocument
                      "PATH 1: Fresh First Launch -&gt; Need Location -&gt; First Entry"
                      "Expected the first screen-path title."

                  Expect.stringContains htmlDocument "AppStarted" "Expected the app/system startup step."
                  Expect.stringContains htmlDocument "Screen.AppStart - Splash" "Expected the splash startup surface."
                  Expect.stringContains htmlDocument "Screen.AppStart - Runtime Checks" "Expected the runtime-check startup surface."
                  Expect.stringContains htmlDocument "Screen.AppStart - Route Resolved" "Expected the route-resolved startup surface."
                  Expect.stringContains htmlDocument "Screen.NewSession - Awaiting Location" "Expected the need-location screen state."
                  Expect.stringContains htmlDocument "Screen.EntryForm - Ready At Location" "Expected the ready-at-location screen state."
                  Expect.stringContains htmlDocument "Screen.EntryForm - Logged Success" "Expected the logged-success path state."
                  Expect.stringContains htmlDocument "Context · ScreenPath" "Expected the screen-path bounded context badge."
                  Expect.stringContains htmlDocument "Context · ApplicationLifecycle" "Expected the application-lifecycle bounded context badge."
                  Expect.stringContains htmlDocument "Context · RuntimeOrchestration" "Expected the runtime-orchestration bounded context badge."
                  Expect.stringContains htmlDocument "screen path lens" "Expected the screen-path lens badge."
                  Expect.stringContains htmlDocument "application lifecycle lens" "Expected the application-lifecycle startup lens badge."
                  Expect.stringContains htmlDocument "app runtime lens" "Expected the app-runtime startup lens badge."
                  Expect.stringContains htmlDocument "What Changed" "Expected each path step to state what changed."
                  Expect.stringContains htmlDocument "The first usable screen replaces the splash surface." "Expected path-specific screen change summaries."
                  Expect.stringContains htmlDocument "data-testid=\"screen-path-document\"" "Expected a stable browser-test hook for the path document."
                  Expect.stringContains htmlDocument "data-testid=\"path-step\"" "Expected stable browser-test hooks for rendered steps."
                  Expect.stringContains htmlDocument "data-testid=\"path-step-changes\"" "Expected stable browser-test hooks for step change summaries."
                  Expect.stringContains htmlDocument "data-step-key=\"01-app-started\"" "Expected the first screen-path step to expose its stable key for browser tests."
                  Expect.stringContains htmlDocument "fresh first launch with no known local data" "Expected explicit scenario context for the path."
                  Expect.stringContains htmlDocument "No saved location is available yet." "Expected the first-launch assumptions to be visible in detailed mode.")

              testCase "Screen path renderer includes view controls and headerless splash surfaces" (fun () ->
                  let htmlDocument =
                      ScreenPathHtmlRenderer.renderDocument (ScreenPathHtmlExamples.path1StartupToFirstEntry ())

                  Expect.stringContains htmlDocument "data-view-mode=\"standard\"" "Expected the path document to carry a default view mode."
                  Expect.stringContains htmlDocument "data-testid=\"path-view-toggle\"" "Expected stable browser-test hooks for path viewer controls."
                  Expect.stringContains htmlDocument "data-testid=\"path-lens-toggle\"" "Expected stable browser-test hooks for header lens visibility controls."
                  Expect.stringContains htmlDocument "data-testid=\"path-scenario\"" "Expected a stable browser-test hook for the scenario disclosure."
                  Expect.stringContains htmlDocument "data-testid=\"path-scenario-panel\"" "Expected a stable browser-test hook for the scenario detail panel."
                  Expect.stringContains htmlDocument "lensFilterStorageKey" "Expected lens visibility choices to be stored locally."
                  Expect.stringContains htmlDocument "step.hidden = !activeLensKeys.includes(stepLensKey);" "Expected header lens toggles to hide or show step columns."
                  Expect.stringContains htmlDocument "button.dataset.viewMode" "Expected the viewer controls to be wired by script."
                  Expect.stringContains htmlDocument "<section class=\"ll-phone-screen ll-phone-screen--boot\">" "Expected the splash family to render as a dedicated headerless phone screen."
                  Expect.stringContains htmlDocument "data-testid=\"boot-check\"" "Expected stable browser-test hooks for boot checkpoint states."
                  Expect.stringContains htmlDocument "data-status=\"active\"" "Expected active startup checkpoints."
                  Expect.stringContains htmlDocument "data-status=\"complete\"" "Expected completed startup checkpoints."
                  Expect.stringContains htmlDocument "Route to Need Location" "Expected route-resolution checkpoint copy in the splash progression.")

              testCase "Screen path renderer adds a synced top horizontal scroll rail" (fun () ->
                  let htmlDocument =
                      ScreenPathHtmlRenderer.renderDocument (ScreenPathHtmlExamples.path1StartupToFirstEntry ())

                  Expect.stringContains
                      htmlDocument
                      "html, body { height: 100%; overflow: hidden; }"
                      "Expected the browser-level scrollbars to be disabled for the screen-path page."

                  Expect.stringContains
                      htmlDocument
                      ".ll-path-stage { min-height: 0; overflow-y: auto; overflow-x: hidden;"
                      "Expected an internal vertical stage for screen-path rows."

                  Expect.stringContains
                      htmlDocument
                      "<button id=\"ll-path-nav-start\" class=\"ll-path-nav-button\" data-testid=\"path-nav-start\""
                      "Expected a dedicated button for returning to the beginning of the path."

                  Expect.stringContains
                      htmlDocument
                      "<button id=\"ll-path-nav-end\" class=\"ll-path-nav-button\" data-testid=\"path-nav-end\""
                      "Expected a dedicated button for jumping to the end of the path."

                  Expect.stringContains
                      htmlDocument
                      ".ll-path-nav-button__chevron--left { transform: rotate(-135deg); }"
                      "Expected the left navigation buttons to use explicit shaped chevrons instead of text glyphs."

                  Expect.stringContains
                      htmlDocument
                      ".ll-path-nav-button__chevron--right { transform: rotate(45deg); }"
                      "Expected the right navigation buttons to use explicit shaped chevrons instead of text glyphs."

                  Expect.stringContains
                      htmlDocument
                      "<div id=\"ll-path-flow-viewport\" class=\"ll-path-flow-viewport\" data-testid=\"path-flow-viewport\">"
                      "Expected the screen strip to live in its own horizontal viewport."

                  Expect.stringContains
                      htmlDocument
                      ".ll-path-flow { display: grid; grid-auto-flow: column; grid-auto-columns: minmax(360px, 380px); width: max-content;"
                      "Expected the screen strip to stay a single continuous horizontal grid."

                  Expect.stringContains
                      htmlDocument
                      ".ll-path-step[hidden] { display: none !important; }"
                      "Expected hidden path steps to be removed from layout when lens toggles disable them."

                  Expect.stringContains
                      htmlDocument
                      "setSyncedScrollLeft(scrollbar.scrollLeft);"
                      "Expected the top rail to drive horizontal scrolling of the screen strip through the shared sync helper."

                  Expect.stringContains
                      htmlDocument
                      "scrollbar.scrollLeft = clampLeft(viewport.scrollLeft);"
                      "Expected the rail to stay synced when the screen viewport itself is scrolled."

                  Expect.stringContains
                      htmlDocument
                      "const scrollByOneColumn = (direction) => {"
                      "Expected explicit one-column stepping logic for path navigation."

                  Expect.stringContains
                      htmlDocument
                      "const refreshScrollMetrics = () => {"
                      "Expected the path buttons to refresh measured scroll targets before stepping."

                  Expect.stringContains
                      htmlDocument
                      "const firstOffset = steps[0].offsetLeft;"
                      "Expected navigation to normalize the measured step offsets against the first rendered column."

                  Expect.stringContains
                      htmlDocument
                      "const contentRightEdge = Math.max(...steps.map((step, index) => normalizedTargets[index] + Math.round(step.getBoundingClientRect().width)));"
                      "Expected the end-of-content boundary to be measured from the real rendered columns."

                  Expect.stringContains
                      htmlDocument
                      "const maxStartIndexCandidate = normalizedTargets.findIndex((target) => contentRightEdge - target <= viewport.clientWidth + 1);"
                      "Expected the last logical whole-column target to be derived from what still fits in the viewport."

                  Expect.stringContains
                      htmlDocument
                      "const extraTrailingSpace = Math.max(0, currentStepMetrics.logicalMaxTarget - nativeMaxScroll);"
                      "Expected the path viewport to add enough trailing space for the last logical whole-column target."

                  Expect.stringContains
                      htmlDocument
                      "scrollbarContent.style.width = `${Math.ceil(scrollbar.offsetWidth + currentStepMetrics.logicalMaxTarget)}px`;"
                      "Expected the top rail to use the logical whole-column range so the far-right position matches the last readable path column."

                  Expect.stringContains
                      htmlDocument
                      "const setSyncedScrollLeft = (left) => {"
                      "Expected the path viewport and top rail to be updated together from one scroll position."

                  Expect.stringContains
                      htmlDocument
                      "const animateScrollToColumn = (targetLeft) => {"
                      "Expected button-driven navigation to use an explicit deterministic column animation."

                  Expect.stringContains
                      htmlDocument
                      "animationFrameId = window.requestAnimationFrame(tick);"
                      "Expected the path buttons to animate between whole-column targets with requestAnimationFrame."

                  Expect.stringContains
                      htmlDocument
                      "animateScrollToColumn(targets[targetIndex]);"
                      "Expected the next and previous buttons to land on measured whole-column targets."

                  Expect.stringContains
                      htmlDocument
                      "document.fonts.ready.then(() => syncWidth()).catch(() => {});"
                      "Expected the path to refresh its scroll metrics after fonts settle."

                  Expect.stringContains
                      htmlDocument
                      "const scrollToEnd = () => {"
                      "Expected the end button to use a dedicated logical end-navigation helper."

                  Expect.stringContains
                      htmlDocument
                      "animateScrollToColumn(logicalMaxScrollLeft());"
                      "Expected the end button to use the logical whole-column maximum rather than the native scroll width."

                  Expect.stringContains
                      htmlDocument
                      "startButton.disabled = currentLeft <= 2;"
                      "Expected the beginning button to show a disabled state at the start of the path."

                  Expect.stringContains
                      htmlDocument
                      ".ll-path-nav-button:disabled { background: #cbd5e1;"
                      "Expected inactive navigation buttons to use a stronger disabled visual state.")

              testCase "Screen path renderer includes update monitor controls for local artifacts" (fun () ->
                  let htmlDocument =
                      ScreenPathHtmlRenderer.renderDocument (ScreenPathHtmlExamples.path1StartupToFirstEntry ())

                  Expect.stringContains htmlDocument "Notify Me" "Expected a sticky notify-only update mode."
                  Expect.stringContains htmlDocument "Auto Refresh" "Expected a sticky auto-refresh update mode."
                  Expect.stringContains htmlDocument "data-testid=\"path-update-refresh\"" "Expected a stable browser-test hook for the manual refresh button."
                  Expect.stringContains htmlDocument "ll-path-update-status" "Expected a visible update-status surface."
                  Expect.stringContains htmlDocument "window.__llPathUpdateManifest" "Expected the page to read a sidecar manifest script."
                  Expect.stringContains htmlDocument ".update.js" "Expected the update monitor to target the companion update script."
                  Expect.stringContains htmlDocument "updateModeStorageKey" "Expected the update preference to be stored locally."
                  Expect.stringContains htmlDocument "window.location.reload()" "Expected the page to support explicit refresh.")

              testCase "Screen path renderer emits the sidecar update manifest script" (fun () ->
                  let manifestScript =
                      ScreenPathHtmlRenderer.renderUpdateManifestScript
                          { Version = "screen-path::test"
                            UpdatedAtUtc = "2026-03-29T12:34:56Z"
                            PollIntervalMs = 3000 }

                  Expect.stringContains manifestScript "window.__llPathUpdateManifest" "Expected the manifest bootstrap global."
                  Expect.stringContains manifestScript "screen-path::test" "Expected the manifest version."
                  Expect.stringContains manifestScript "2026-03-29T12:34:56Z" "Expected the manifest timestamp.")

              testCase "Screen renderer uses the reusable LaundryLog component blocks" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains htmlDocument "ll-header" "Expected the screen header block."
                  Expect.stringContains htmlDocument "ll-panel" "Expected the reusable screen panel block."
                  Expect.stringContains htmlDocument "ll-status-chip-row" "Expected the command-readiness chip row."
                  Expect.stringContains htmlDocument "ll-option-group" "Expected the option-group block."
                  Expect.stringContains htmlDocument "ll-stepper" "Expected the stepper block."
                  Expect.stringContains htmlDocument "ll-money-input" "Expected the money-input block."
                  Expect.stringContains htmlDocument "ll-quarter-button" "Expected the quarter-style price-adjust buttons."
                  Expect.stringContains htmlDocument "Choose Card" "Expected the progressive payment-detail section."
                  Expect.stringContains htmlDocument "<span class=\"ll-chip__amount\">$4.00</span>" "Expected the historical helper amount from the v7-aligned primary surface."
                  Expect.stringContains htmlDocument "<span class=\"ll-chip__subtext\">Historical</span>" "Expected the historical helper label."
                  Expect.stringContains htmlDocument "ll-feedback-banner" "Expected the visible success toast block."
                  Expect.stringContains htmlDocument "ll-entry-card" "Expected the recent-entry card block."
                  Expect.stringContains htmlDocument "3 Washers @ $3.75 • Credit Card" "Expected the v7-aligned primary entry list.")

              testCase "Screen renderer stacks quantity and unit price vertically like v7" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains
                      htmlDocument
                      ".ll-two-up { display: grid; grid-template-columns: 1fr; gap: 12px; }"
                      "Expected quantity and unit-price panels to stack vertically.")

              testCase "Screen renderer protects the location input from gps overlap" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains
                      htmlDocument
                      "*, *::before, *::after { box-sizing: border-box; }"
                      "Expected the v7-style border-box baseline."

                  Expect.stringContains
                      htmlDocument
                      ".ll-location-input { flex: 1; min-width: 0; display: flex; flex-direction: column; }"
                      "Expected the location input flex child to allow shrinking."

                  Expect.stringContains
                      htmlDocument
                      ".ll-gps-button { flex: 0 0 56px;"
                      "Expected the GPS button width to stay fixed instead of overlapping the input.")

              testCase "Screen renderer uses a three-column unit-price grid" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains
                      htmlDocument
                      ".ll-money-input-grid { display: grid; grid-template-columns: 72px minmax(0, 1fr) 72px;"
                      "Expected the unit-price control to use a fixed three-column grid."

                  let decreaseIndex = htmlDocument.IndexOf("data-control-id=\"price-quarter-down\"")
                  let fieldIndex = htmlDocument.IndexOf("data-control-id=\"price-input\"")
                  let increaseIndex = htmlDocument.IndexOf("data-control-id=\"price-quarter-up\"")

                  Expect.isGreaterThanOrEqual decreaseIndex 0 "Expected the decrease quarter button in the rendered HTML."
                  Expect.isGreaterThanOrEqual fieldIndex 0 "Expected the price input field in the rendered HTML."
                  Expect.isGreaterThanOrEqual increaseIndex 0 "Expected the increase quarter button in the rendered HTML."
                  Expect.isLessThan decreaseIndex fieldIndex "Expected the decrease button to render to the left of the price field."
                  Expect.isLessThan fieldIndex increaseIndex "Expected the increase button to render to the right of the price field.")

              testCase "Screen renderer keeps the session total fully boxed" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains
                      htmlDocument
                      ".ll-summary-bar { width: 100%; max-width: 100%; box-sizing: border-box;"
                      "Expected the session-total bar to use a full border-box width contract."

                  Expect.stringContains
                      htmlDocument
                      "border: 2px solid #ffcc80;"
                      "Expected the session-total bar to render a full border on all sides."

                  Expect.stringContains
                      htmlDocument
                      "margin: 0.6rem 0 1.2rem;"
                      "Expected the session-total bar to avoid side margins that can visually clip the border.")

              testCase "Screen renderer keeps validation chips in a counted grid row" (fun () ->
                  let htmlDocument =
                      ScreenHtmlRenderer.renderDocument
                          "LaundryLog Screen Components"
                          "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
                          (ScreenHtmlExamples.laundryLogBaseScreens ())

                  Expect.stringContains
                      htmlDocument
                      "<div class=\"ll-status-chip-row\" style=\"--status-chip-count: 3;\">"
                      "Expected the readiness chips to declare a fixed one-row chip count."

                  Expect.stringContains
                      htmlDocument
                      ".ll-status-chip-row { display: grid; grid-template-columns: repeat(var(--status-chip-count, 1), minmax(0, 1fr));"
                      "Expected the readiness chips to use a counted grid instead of wrapping flex."

                  Expect.stringContains
                      htmlDocument
                      ".ll-status-chip { min-width: 0; width: 100%;"
                      "Expected each readiness chip to shrink inside its grid column without wrapping to a second row.") ]
