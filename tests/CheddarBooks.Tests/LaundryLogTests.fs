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
                  Expect.equal state.PriceInput.QuickFillLabels [ "$2.50"; "$3.00"; "$3.50" ] "Expected the Penpot-backed quick-fill values.") ]
