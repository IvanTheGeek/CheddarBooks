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

                  match MoneyInputState.tryCreate controlId "$" None "0.00" [ "$2.50"; "$3.00" ] with
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
                  Expect.equal state.PriceInput.QuickFillLabels [ "$2.50"; "$3.00"; "$3.50" ] "Expected the Penpot-backed quick-fill values."
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
                  Expect.stringContains htmlDocument ">CaptureLaundryLocation</div>" "Expected the first command-slice WHEN clause to use the command block title."
                  Expect.stringContains htmlDocument "project the current laundry session for the active location" "Expected the first view-slice WHEN clause.")

              testCase "Classic path renderer includes a property-width toggle mode" (fun () ->
                  let pathRow = SliceHtmlExamples.path1ManualLocationWasherDryer ()

                  let htmlDocument =
                      SliceHtmlRenderer.renderDocument
                          (SliceRenderOptions.classicEventModel "LaundryLog PATH 1")
                          pathRow

                  Expect.stringContains htmlDocument "Expand Property Width" "Expected a path-level property-width toggle."
                  Expect.stringContains htmlDocument "path-document--properties-wide" "Expected the renderer to emit the widened path mode class."
                  Expect.stringContains htmlDocument "--slice-wide-columns" "Expected the widened track count to be present in the document.")

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
                      "Expected the long property value to render in the indented value span.") ]
