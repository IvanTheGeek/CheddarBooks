namespace CheddarBooks.LaundryLog.UI

open System

/// Identifies one stable local control id in LaundryLog primitive-state mappings.
type PrimitiveControlId = private PrimitiveControlId of string

[<RequireQualifiedAccess>]
module PrimitiveControlId =
    let private isAllowedCharacter character =
        Char.IsAsciiLetterOrDigit character || character = '-'

    /// Creates a validated local control id using an explicit lowercase slug allowlist.
    let tryCreate (value: string) =
        if String.IsNullOrWhiteSpace value then
            Error "Primitive control identifiers must not be blank."
        elif value |> Seq.exists (isAllowedCharacter >> not) then
            Error "Primitive control identifiers may contain only ASCII letters, digits, and hyphen."
        elif value <> value.ToLowerInvariant() then
            Error "Primitive control identifiers must use lowercase values."
        else
            Ok(PrimitiveControlId value)

    /// Extracts the underlying control id slug.
    let value (PrimitiveControlId value) = value

/// Distinguishes the current emphasis levels for action-driving buttons in LaundryLog.
type ActionButtonEmphasis =
    | Primary
    | Secondary
    | Supporting
    | Success

/// Describes the current header-bar pressure from LaundryLog screens.
type HeaderBarState =
    { Title: string
      Subtitle: string option
      BadgeText: string option }

[<RequireQualifiedAccess>]
module HeaderBarState =
    /// Creates a validated header-bar state with the current LaundryLog pressure fields.
    let tryCreate title subtitle badgeText =
        if String.IsNullOrWhiteSpace title then
            Error "Header bars must have a title."
        else
            Ok
                { Title = title.Trim()
                  Subtitle = subtitle |> Option.map (fun (value: string) -> value.Trim())
                  BadgeText = badgeText |> Option.map (fun (value: string) -> value.Trim()) }

/// Distinguishes the current readiness tone for one status chip.
type StatusChipTone =
    | Ready
    | NeedsAttention

/// Describes one compact status chip near the current command area.
type StatusChipState =
    { ControlId: PrimitiveControlId
      IconText: string
      LabelText: string
      Tone: StatusChipTone }

[<RequireQualifiedAccess>]
module StatusChipState =
    /// Creates a validated status-chip state.
    let tryCreate controlId iconText labelText tone =
        if String.IsNullOrWhiteSpace iconText then
            Error "Status chips must provide an icon text."
        elif String.IsNullOrWhiteSpace labelText then
            Error "Status chips must provide a label text."
        else
            Ok
                { ControlId = controlId
                  IconText = iconText.Trim()
                  LabelText = labelText.Trim()
                  Tone = tone }

/// Describes one text-input primitive state.
type TextInputState =
    { ControlId: PrimitiveControlId
      PlaceholderText: string
      ValueText: string option
      IsReadOnly: bool }

[<RequireQualifiedAccess>]
module TextInputState =
    /// Creates a validated text-input state.
    let tryCreate controlId placeholderText valueText isReadOnly =
        if String.IsNullOrWhiteSpace placeholderText then
            Error "Text inputs must provide placeholder text."
        else
            Ok
                { ControlId = controlId
                  PlaceholderText = placeholderText.Trim()
                  ValueText = valueText |> Option.map (fun (value: string) -> value.Trim())
                  IsReadOnly = isReadOnly }

/// Describes one action-driving button state.
type ActionButtonState =
    { ControlId: PrimitiveControlId
      Label: string
      IsEnabled: bool
      Emphasis: ActionButtonEmphasis }

[<RequireQualifiedAccess>]
module ActionButtonState =
    /// Creates a validated action-button state.
    let tryCreate controlId label isEnabled emphasis =
        if String.IsNullOrWhiteSpace label then
            Error "Action buttons must have a label."
        else
            Ok
                { ControlId = controlId
                  Label = label.Trim()
                  IsEnabled = isEnabled
                  Emphasis = emphasis }

/// Describes one option inside a reusable option group.
type OptionChoiceState =
    { ChoiceId: PrimitiveControlId
      Label: string
      IsSelected: bool
      IsEnabled: bool }

[<RequireQualifiedAccess>]
module OptionChoiceState =
    /// Creates a validated option choice state.
    let tryCreate choiceId label isSelected isEnabled =
        if String.IsNullOrWhiteSpace label then
            Error "Option choices must have a label."
        else
            Ok
                { ChoiceId = choiceId
                  Label = label.Trim()
                  IsSelected = isSelected
                  IsEnabled = isEnabled }

/// Describes one group of related option choices.
type OptionGroupState =
    { ControlId: PrimitiveControlId
      Label: string option
      Choices: OptionChoiceState list }

[<RequireQualifiedAccess>]
module OptionGroupState =
    let private hasDuplicateChoiceIds choices =
        let ids = choices |> List.map (fun choice -> PrimitiveControlId.value choice.ChoiceId)
        ids.Length <> (ids |> List.distinct).Length

    /// Creates a validated option-group state.
    let tryCreate controlId label choices =
        if List.isEmpty choices then
            Error "Option groups must contain at least one choice."
        elif hasDuplicateChoiceIds choices then
            Error "Option groups must not contain duplicate choice identifiers."
        else
            Ok
                { ControlId = controlId
                  Label = label |> Option.map (fun (value: string) -> value.Trim())
                  Choices = choices }

/// Describes one stepper control with increment, decrement, and visible value.
type StepperState =
    { ControlId: PrimitiveControlId
      DecrementLabel: string
      IncrementLabel: string
      ValueText: string
      CanDecrement: bool
      CanIncrement: bool }

[<RequireQualifiedAccess>]
module StepperState =
    /// Creates a validated stepper state.
    let tryCreate controlId decrementLabel incrementLabel valueText canDecrement canIncrement =
        if String.IsNullOrWhiteSpace decrementLabel then
            Error "Steppers must provide a decrement label."
        elif String.IsNullOrWhiteSpace incrementLabel then
            Error "Steppers must provide an increment label."
        elif String.IsNullOrWhiteSpace valueText then
            Error "Steppers must provide a visible value."
        else
            Ok
                { ControlId = controlId
                  DecrementLabel = decrementLabel.Trim()
                  IncrementLabel = incrementLabel.Trim()
                  ValueText = valueText.Trim()
                  CanDecrement = canDecrement
                  CanIncrement = canIncrement }

/// Describes one money-input primitive with optional quick-fill labels.
type QuarterAdjustDirection =
    | Decrease
    | Increase

/// Describes one quarter-style price adjust button.
type QuarterAdjustButtonState =
    { ControlId: PrimitiveControlId
      Direction: QuarterAdjustDirection
      AmountText: string
      IsEnabled: bool }

[<RequireQualifiedAccess>]
module QuarterAdjustButtonState =
    /// Creates a validated quarter-adjust button state.
    let tryCreate controlId direction amountText isEnabled =
        if String.IsNullOrWhiteSpace amountText then
            Error "Quarter-adjust buttons must provide an amount text."
        else
            Ok
                { ControlId = controlId
                  Direction = direction
                  AmountText = amountText.Trim()
                  IsEnabled = isEnabled }

/// Describes one money-input primitive with optional quick-fill labels.
type MoneyInputState =
    { ControlId: PrimitiveControlId
      CurrencySymbol: string
      ValueText: string option
      PlaceholderText: string
      QuickFillLabels: string list
      QuarterAdjustButtons: QuarterAdjustButtonState list }

[<RequireQualifiedAccess>]
module MoneyInputState =
    /// Creates a validated money-input state.
    let tryCreate controlId currencySymbol valueText placeholderText quickFillLabels quarterAdjustButtons =
        if String.IsNullOrWhiteSpace currencySymbol then
            Error "Money inputs must provide a currency symbol."
        elif String.IsNullOrWhiteSpace placeholderText then
            Error "Money inputs must provide placeholder text."
        elif quickFillLabels |> List.exists String.IsNullOrWhiteSpace then
            Error "Money input quick-fill labels must not be blank."
        else
            Ok
                { ControlId = controlId
                  CurrencySymbol = currencySymbol.Trim()
                  ValueText = valueText |> Option.map (fun (value: string) -> value.Trim())
                  PlaceholderText = placeholderText.Trim()
                  QuickFillLabels = quickFillLabels |> List.map (fun (value: string) -> value.Trim())
                  QuarterAdjustButtons = quarterAdjustButtons }

/// Describes one summary or status bar near the current command area.
type SummaryBarState =
    { ControlId: PrimitiveControlId
      Label: string
      ValueText: string }

[<RequireQualifiedAccess>]
module SummaryBarState =
    /// Creates a validated summary-bar state.
    let tryCreate controlId label valueText =
        if String.IsNullOrWhiteSpace label then
            Error "Summary bars must provide a label."
        elif String.IsNullOrWhiteSpace valueText then
            Error "Summary bars must provide a visible value."
        else
            Ok
                { ControlId = controlId
                  Label = label.Trim()
                  ValueText = valueText.Trim() }

/// Describes one visible toast or feedback banner after a completed action.
type FeedbackBannerState =
    { ControlId: PrimitiveControlId
      MessageText: string }

[<RequireQualifiedAccess>]
module FeedbackBannerState =
    /// Creates a validated feedback-banner state.
    let tryCreate controlId messageText =
        if String.IsNullOrWhiteSpace messageText then
            Error "Feedback banners must provide a visible message."
        else
            Ok
                { ControlId = controlId
                  MessageText = messageText.Trim() }

/// Describes one compact visible entry card in the current session list.
type EntryCardState =
    { ControlId: PrimitiveControlId
      TitleText: string
      DetailText: string option
      AmountText: string option }

[<RequireQualifiedAccess>]
module EntryCardState =
    /// Creates a validated entry-card state.
    let tryCreate controlId titleText detailText amountText =
        if String.IsNullOrWhiteSpace titleText then
            Error "Entry cards must provide a title."
        else
            Ok
                { ControlId = controlId
                  TitleText = titleText.Trim()
                  DetailText = detailText |> Option.map (fun (value: string) -> value.Trim())
                  AmountText = amountText |> Option.map (fun (value: string) -> value.Trim()) }

/// Describes the first local primitive composition for the New Session view.
type NewSessionPrimitiveState =
    { Header: HeaderBarState
      LocationInput: TextInputState
      GpsAction: ActionButtonState
      SetLocationAction: ActionButtonState }

/// Describes the first local primitive composition for the Entry Form view.
type EntryFormPrimitiveState =
    { Header: HeaderBarState
      StatusChips: StatusChipState list
      MachineTypeOptions: OptionGroupState
      QuantityStepper: StepperState
      PriceInput: MoneyInputState
      PaymentOptions: OptionGroupState
      PaymentDetailOptions: OptionGroupState option
      SessionTotal: SummaryBarState
      FeedbackBanner: FeedbackBannerState option
      RecentEntries: EntryCardState list
      SubmitAction: ActionButtonState }
