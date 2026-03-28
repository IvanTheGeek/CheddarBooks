namespace CheddarBooks.LaundryLog.UI

open System
open CheddarBooks.LaundryLog

/// Captures the current command-side state for the CaptureLaundryLocation CommandSlice.
type CaptureLaundryLocationCommandSliceState =
    { DraftLocationName: LocationName option
      CanUseGps: bool }

[<RequireQualifiedAccess>]
module CaptureLaundryLocationCommandSliceState =
    /// Creates the first command-side state for location capture.
    let create draftLocationName canUseGps =
        { DraftLocationName = draftLocationName
          CanUseGps = canUseGps }

/// Captures the current command-side state for the LogLaundryExpense CommandSlice.
type LogLaundryExpenseCommandSliceState =
    { SelectedExpenseKind: ExpenseKind
      Quantity: int
      UnitPriceText: string option
      SelectedPaymentMethod: PaymentMethod
      QuickFillLabels: string list
      CanSubmit: bool }

[<RequireQualifiedAccess>]
module LogLaundryExpenseCommandSliceState =
    /// Creates a validated command-side state for logging one laundry expense.
    let tryCreate selectedExpenseKind quantity unitPriceText selectedPaymentMethod quickFillLabels canSubmit =
        if quantity < 1 then
            Error "LogLaundryExpense command state must have quantity of at least 1."
        elif quickFillLabels |> List.exists String.IsNullOrWhiteSpace then
            Error "LogLaundryExpense quick-fill labels must not be blank."
        else
            Ok
                { SelectedExpenseKind = selectedExpenseKind
                  Quantity = quantity
                  UnitPriceText = unitPriceText |> Option.map (fun (value: string) -> value.Trim())
                  SelectedPaymentMethod = selectedPaymentMethod
                  QuickFillLabels = quickFillLabels |> List.map (fun (value: string) -> value.Trim())
                  CanSubmit = canSubmit }

/// Captures one visible expense line inside the current laundry-session View.
type VisibleLaundryExpenseViewLine =
    { ExpenseKind: ExpenseKind
      PaymentMethod: PaymentMethod
      Quantity: int
      LineTotalText: string }

[<RequireQualifiedAccess>]
module VisibleLaundryExpenseViewLine =
    /// Creates a validated visible expense line for the current-session View.
    let tryCreate expenseKind paymentMethod quantity lineTotalText =
        if quantity < 1 then
            Error "Visible laundry expense lines must have quantity of at least 1."
        elif String.IsNullOrWhiteSpace lineTotalText then
            Error "Visible laundry expense lines must provide a line total text."
        else
            Ok
                { ExpenseKind = expenseKind
                  PaymentMethod = paymentMethod
                  Quantity = quantity
                  LineTotalText = lineTotalText.Trim() }

/// Captures the current View-backed context for the active laundry session.
type CurrentLaundrySessionViewState =
    { ActiveLocationName: LocationName
      VisibleEntries: VisibleLaundryExpenseViewLine list
      RunningTotalText: string
      LastRecordedAtLocalText: string option }

[<RequireQualifiedAccess>]
module CurrentLaundrySessionViewState =
    /// Creates a validated View-backed state for the current laundry session.
    let tryCreate activeLocationName visibleEntries runningTotalText lastRecordedAtLocalText =
        if String.IsNullOrWhiteSpace runningTotalText then
            Error "CurrentLaundrySession View state must provide a running total text."
        else
            Ok
                { ActiveLocationName = activeLocationName
                  VisibleEntries = visibleEntries
                  RunningTotalText = runningTotalText.Trim()
                  LastRecordedAtLocalText = lastRecordedAtLocalText |> Option.map (fun (value: string) -> value.Trim()) }

/// Maps current CommandSlice and ViewSlice state into the first LaundryLog primitive compositions.
[<RequireQualifiedAccess>]
module PrimitiveStateMappings =
    let private expect description result =
        match result with
        | Ok value -> value
        | Error message -> failwith $"Expected a valid {description}. {message}"

    let private controlId value =
        PrimitiveControlId.tryCreate value
        |> expect $"control id '{value}'"

    let private actionButton controlIdValue label isEnabled emphasis =
        ActionButtonState.tryCreate (controlId controlIdValue) label isEnabled emphasis
        |> expect $"action button '{label}'"

    let private optionChoice controlIdValue label isSelected =
        OptionChoiceState.tryCreate (controlId controlIdValue) label isSelected true
        |> expect $"option choice '{label}'"

    let private machineTypeOptions selectedExpenseKind =
        [ ExpenseKind.Washer; ExpenseKind.Dryer; ExpenseKind.Supplies ]
        |> List.map (fun expenseKind ->
            optionChoice
                (ExpenseKind.slug expenseKind)
                (ExpenseKind.displayName expenseKind)
                (expenseKind = selectedExpenseKind))
        |> fun choices -> OptionGroupState.tryCreate (controlId "machine-type") None choices
        |> expect "machine type options"

    let private paymentOptions selectedPaymentMethod =
        [ PaymentMethod.Cash; PaymentMethod.Card; PaymentMethod.App; PaymentMethod.Points ]
        |> List.map (fun paymentMethod ->
            optionChoice
                (PaymentMethod.slug paymentMethod)
                (PaymentMethod.displayName paymentMethod)
                (paymentMethod = selectedPaymentMethod))
        |> fun choices -> OptionGroupState.tryCreate (controlId "payment-type") None choices
        |> expect "payment options"

    /// Maps the current CaptureLaundryLocation CommandSlice into the New Session primitive composition.
    let newSessionFromCommandSlice (commandSlice: CaptureLaundryLocationCommandSliceState) : NewSessionPrimitiveState =
        let draftLocationValue =
            commandSlice.DraftLocationName
            |> Option.map LocationName.value

        { Header =
            HeaderBarState.tryCreate "LaundryLog" None None
            |> expect "new-session header"
          LocationInput =
            TextInputState.tryCreate (controlId "location-input") "Enter location..." draftLocationValue false
            |> expect "new-session location input"
          GpsAction = actionButton "use-gps-location" "Use GPS Location" commandSlice.CanUseGps Supporting
          SetLocationAction = actionButton "set-location" "Set Location" draftLocationValue.IsSome Primary }

    /// Maps the current LogLaundryExpense CommandSlice plus current-session View into the Entry Form primitive composition.
    let entryFormFromSlices
        (viewState: CurrentLaundrySessionViewState)
        (commandSlice: LogLaundryExpenseCommandSliceState)
        : EntryFormPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" (Some(LocationName.value viewState.ActiveLocationName)) None
            |> expect "entry-form header"
          MachineTypeOptions = machineTypeOptions commandSlice.SelectedExpenseKind
          QuantityStepper =
            StepperState.tryCreate
                (controlId "quantity-stepper")
                "-"
                "+"
                (string commandSlice.Quantity)
                (commandSlice.Quantity > 1)
                true
            |> expect "quantity stepper"
          PriceInput =
            MoneyInputState.tryCreate
                (controlId "price-input")
                "$"
                commandSlice.UnitPriceText
                "0.00"
                commandSlice.QuickFillLabels
            |> expect "price input"
          PaymentOptions = paymentOptions commandSlice.SelectedPaymentMethod
          SessionTotal =
            SummaryBarState.tryCreate (controlId "session-total") "Session Total" viewState.RunningTotalText
            |> expect "session total"
          SubmitAction = actionButton "log-expense" "Log Expense" commandSlice.CanSubmit Primary }
