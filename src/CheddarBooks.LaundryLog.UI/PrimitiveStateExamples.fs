namespace CheddarBooks.LaundryLog.UI

/// Provides small hand-mapped example primitive states based on the current LaundryLog Penpot file.
[<RequireQualifiedAccess>]
module PrimitiveStateExamples =
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

    let private optionGroup controlIdValue label choices =
        OptionGroupState.tryCreate (controlId controlIdValue) label choices
        |> expect $"option group '{controlIdValue}'"

    /// Captures the current New Session primitive composition before a location value has been entered.
    let newSessionAwaitingLocation () : NewSessionPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" None None
            |> expect "new-session header"
          LocationInput =
            TextInputState.tryCreate (controlId "location-input") "Enter location..." None false
            |> expect "new-session location input"
          GpsAction = actionButton "use-gps-location" "Use GPS Location" true Supporting
          SetLocationAction = actionButton "set-location" "Set Location" false Primary }

    /// Captures the current New Session primitive composition after a manual location has been typed.
    let newSessionLocationEntered () : NewSessionPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" None None
            |> expect "location-entered header"
          LocationInput =
            TextInputState.tryCreate
                (controlId "location-input")
                "Enter location..."
                (Some "Love's #123 - Springfield, OH")
                false
            |> expect "location-entered input"
          GpsAction = actionButton "use-gps-location" "Use GPS Location" true Supporting
          SetLocationAction = actionButton "set-location" "Set Location" true Primary }

    /// Captures the current Entry Form primitive composition for the first washer/card draft flow.
    let entryFormWasherCardDraft () : EntryFormPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" (Some "Love's #123 - Springfield, OH") None
            |> expect "entry-form header"
          MachineTypeOptions =
            optionGroup
                "machine-type"
                None
                [ optionChoice "washer" "Washer" true
                  optionChoice "dryer" "Dryer" false
                  optionChoice "supplies" "Supplies" false ]
          QuantityStepper =
            StepperState.tryCreate (controlId "quantity-stepper") "-" "+" "1" false true
            |> expect "quantity stepper"
          PriceInput =
            MoneyInputState.tryCreate
                (controlId "price-input")
                "$"
                None
                "0.00"
                [ "$2.50"; "$3.00"; "$3.50" ]
            |> expect "price input"
          PaymentOptions =
            optionGroup
                "payment-type"
                None
                [ optionChoice "cash" "Cash" false
                  optionChoice "card" "Card" true
                  optionChoice "app" "App" false
                  optionChoice "points" "Points" false ]
          SessionTotal =
            SummaryBarState.tryCreate (controlId "session-total") "Session Total" "$0.00"
            |> expect "session total"
          SubmitAction = actionButton "log-expense" "Log Expense" true Primary }
