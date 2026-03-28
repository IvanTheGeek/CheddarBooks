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

    let private statusChip controlIdValue iconText labelText tone =
        StatusChipState.tryCreate (controlId controlIdValue) iconText labelText tone
        |> expect $"status chip '{labelText}'"

    let private optionChoice controlIdValue label isSelected =
        OptionChoiceState.tryCreate (controlId controlIdValue) label isSelected true
        |> expect $"option choice '{label}'"

    let private optionGroup controlIdValue label choices =
        OptionGroupState.tryCreate (controlId controlIdValue) label choices
        |> expect $"option group '{controlIdValue}'"

    let private quarterAdjustButton controlIdValue direction amountText =
        QuarterAdjustButtonState.tryCreate (controlId controlIdValue) direction amountText true
        |> expect $"quarter adjust button '{controlIdValue}'"

    let private feedbackBanner controlIdValue messageText =
        FeedbackBannerState.tryCreate (controlId controlIdValue) messageText
        |> expect $"feedback banner '{messageText}'"

    let private locationInput valueText =
        TextInputState.tryCreate (controlId "location-input") "Tap 📍 or enter location" valueText false
        |> expect "entry-form location input"

    let private gpsAction () =
        actionButton "use-gps-location" "Use GPS Location" true Supporting

    let private recentEntry controlIdValue titleText detailText amountText =
        EntryCardState.tryCreate
            (controlId controlIdValue)
            titleText
            (Some detailText)
            (Some amountText)
        |> expect $"recent entry '{controlIdValue}'"

    /// Captures the recovered v7 mobile design surface as the primary LaundryLog screen anchor.
    let entryFormV7PrimarySurface () : EntryFormPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" None None
            |> expect "v7 primary header"
          LocationInput = locationInput None |> Some
          GpsAction = gpsAction () |> Some
          StatusChips =
            [ statusChip "status-location" "📍" "Location" NeedsAttention
              statusChip "status-type" "🌊" "Type" NeedsAttention
              statusChip "status-payment" "💳" "Payment" NeedsAttention ]
          MachineTypeOptions =
            optionGroup
                "machine-type"
                None
                [ optionChoice "washer" "Washer" false
                  optionChoice "dryer" "Dryer" false
                  optionChoice "supplies" "Supplies" false ]
          QuantityStepper =
            StepperState.tryCreate (controlId "quantity-stepper") "-" "+" "1" false true
            |> expect "v7 primary quantity stepper"
          PriceInput =
            MoneyInputState.tryCreate
                (controlId "price-input")
                "$"
                (Some "3.00")
                "0.00"
                [ "Last used $3.75"; "Historical $4.00"; "Community $3.50" ]
                [ quarterAdjustButton "price-quarter-down" Decrease "25¢"
                  quarterAdjustButton "price-quarter-up" Increase "25¢" ]
            |> expect "v7 primary price input"
          PaymentOptions =
            optionGroup
                "payment-type"
                None
                [ optionChoice "cash" "Cash" false
                  optionChoice "points" "Points" false
                  optionChoice "card" "Card" false
                  optionChoice "app" "App" false ]
          PaymentDetailOptions = None
          SessionTotal =
            SummaryBarState.tryCreate (controlId "session-total") "Session Total" "$0.00"
            |> expect "v7 primary session total"
          FeedbackBanner = None
          RecentEntries =
            [ recentEntry "entry-v7-washer" "2:34 PM" "3 Washers @ $3.75 • Credit Card" "$11.25"
              recentEntry "entry-v7-dryer" "2:38 PM" "3 Dryers @ $2.00 • Credit Card" "$6.00"
              recentEntry "entry-v7-supplies" "2:40 PM" "1 Supplies @ $2.50 • Cash" "$2.50" ]
          SubmitAction = actionButton "log-entry" "Log Entry" false Primary }

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
          LocationInput = locationInput (Some "Love's #123 - Springfield, OH") |> Some
          GpsAction = gpsAction () |> Some
          StatusChips =
            [ statusChip "status-location" "📍" "Location" Ready
              statusChip "status-type" "🌊" "Type" Ready
              statusChip "status-payment" "💳" "Payment" Ready ]
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
                (Some "3.00")
                "0.00"
                [ "Historical $3.00"; "Last used $2.75"; "Community $3.50" ]
                [ quarterAdjustButton "price-quarter-down" Decrease "25¢"
                  quarterAdjustButton "price-quarter-up" Increase "25¢" ]
            |> expect "price input"
          PaymentOptions =
            optionGroup
                "payment-type"
                None
                [ optionChoice "cash" "Cash" false
                  optionChoice "card" "Card" true
                  optionChoice "app" "App" false
                  optionChoice "points" "Points" false ]
          PaymentDetailOptions =
            optionGroup
                "payment-card-detail"
                (Some "Choose Card")
                [ optionChoice "card-visa-1234" "Visa •1234" true
                  optionChoice "card-business-8890" "Business MC •8890" false ]
            |> Some
          SessionTotal =
            SummaryBarState.tryCreate (controlId "session-total") "Session Total" "$0.00"
            |> expect "session total"
          FeedbackBanner = None
          RecentEntries = []
          SubmitAction = actionButton "log-expense" "Log Expense" true Primary }

    /// Captures the current Entry Form primitive composition after CARD is tapped but before a card detail is chosen.
    let entryFormCardDetailsExpanded () : EntryFormPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" (Some "Love's #123 - Springfield, OH") None
            |> expect "entry-form card-detail header"
          LocationInput = locationInput (Some "Love's #123 - Springfield, OH") |> Some
          GpsAction = gpsAction () |> Some
          StatusChips =
            [ statusChip "status-location" "📍" "Location" Ready
              statusChip "status-type" "🌊" "Type" Ready
              statusChip "status-payment" "💳" "Payment" NeedsAttention ]
          MachineTypeOptions =
            optionGroup
                "machine-type"
                None
                [ optionChoice "washer" "Washer" true
                  optionChoice "dryer" "Dryer" false
                  optionChoice "supplies" "Supplies" false ]
          QuantityStepper =
            StepperState.tryCreate (controlId "quantity-stepper") "-" "+" "1" false true
            |> expect "card-detail quantity stepper"
          PriceInput =
            MoneyInputState.tryCreate
                (controlId "price-input")
                "$"
                (Some "3.00")
                "0.00"
                [ "Historical $3.00"; "Last used $2.75"; "Community $3.50" ]
                [ quarterAdjustButton "price-quarter-down" Decrease "25¢"
                  quarterAdjustButton "price-quarter-up" Increase "25¢" ]
            |> expect "card-detail price input"
          PaymentOptions =
            optionGroup
                "payment-type"
                None
                [ optionChoice "cash" "Cash" false
                  optionChoice "card" "Card" true
                  optionChoice "app" "App" false
                  optionChoice "points" "Points" false ]
          PaymentDetailOptions =
            optionGroup
                "payment-card-detail"
                (Some "Choose Card")
                [ optionChoice "card-visa-1234" "Visa •1234" false
                  optionChoice "card-business-8890" "Business MC •8890" false
                  optionChoice "card-company-4421" "Fleet Card •4421" false ]
            |> Some
          SessionTotal =
            SummaryBarState.tryCreate (controlId "session-total") "Session Total" "$0.00"
            |> expect "card-detail session total"
          FeedbackBanner = None
          RecentEntries = []
          SubmitAction = actionButton "log-expense" "Log Expense" false Primary }

    /// Captures the current Entry Form primitive composition immediately after one washer entry was logged.
    let entryFormLoggedSuccess () : EntryFormPrimitiveState =
        { Header =
            HeaderBarState.tryCreate "LaundryLog" (Some "Love's #123 - Springfield, OH") None
            |> expect "entry-form logged header"
          LocationInput = locationInput (Some "Love's #123 - Springfield, OH") |> Some
          GpsAction = gpsAction () |> Some
          StatusChips =
            [ statusChip "status-location" "📍" "Location" Ready
              statusChip "status-type" "🌊" "Type" NeedsAttention
              statusChip "status-payment" "💳" "Payment" Ready ]
          MachineTypeOptions =
            optionGroup
                "machine-type"
                None
                [ optionChoice "washer" "Washer" false
                  optionChoice "dryer" "Dryer" false
                  optionChoice "supplies" "Supplies" false ]
          QuantityStepper =
            StepperState.tryCreate (controlId "quantity-stepper") "-" "+" "1" false true
            |> expect "logged quantity stepper"
          PriceInput =
            MoneyInputState.tryCreate
                (controlId "price-input")
                "$"
                (Some "3.00")
                "0.00"
                [ "Historical $3.00"; "Last used $3.00"; "Community $3.25" ]
                [ quarterAdjustButton "price-quarter-down" Decrease "25¢"
                  quarterAdjustButton "price-quarter-up" Increase "25¢" ]
            |> expect "logged price input"
          PaymentOptions =
            optionGroup
                "payment-type"
                None
                [ optionChoice "cash" "Cash" true
                  optionChoice "card" "Card" false
                  optionChoice "app" "App" false
                  optionChoice "points" "Points" false ]
          PaymentDetailOptions = None
          SessionTotal =
            SummaryBarState.tryCreate (controlId "session-total") "Session Total" "$5.00"
            |> expect "logged session total"
          FeedbackBanner = feedbackBanner "entry-logged" "✓ Entry logged — $5.00 · CASH" |> Some
          RecentEntries =
            [ recentEntry "entry-1-washer" "2026-03-28 01:10" "1 Washer @ $3.00 • Cash" "$3.00"
              recentEntry "entry-2-dryer" "2026-03-28 01:13" "1 Dryer @ $2.00 • Cash" "$2.00" ]
          SubmitAction = actionButton "log-expense" "✓ Logged!" true Success }
