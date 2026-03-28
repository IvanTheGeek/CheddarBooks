namespace CheddarBooks.LaundryLog

open System

/// Enumerates the currently supported CheddarBooks LaundryLog expense kinds.
type ExpenseKind =
    | Washer
    | Dryer
    | Supplies

[<RequireQualifiedAccess>]
module ExpenseKind =
    /// Returns the stable slug used in view state, derived data, and later persistence boundaries.
    let slug =
        function
        | ExpenseKind.Washer -> "washer"
        | ExpenseKind.Dryer -> "dryer"
        | ExpenseKind.Supplies -> "supplies"

    /// Returns the user-facing label for the expense kind.
    let displayName =
        function
        | ExpenseKind.Washer -> "Washer"
        | ExpenseKind.Dryer -> "Dryer"
        | ExpenseKind.Supplies -> "Supplies"

/// Enumerates the first supported payment methods for LaundryLog expense logging.
type PaymentMethod =
    | Cash
    | Card
    | App
    | Points

[<RequireQualifiedAccess>]
module PaymentMethod =
    /// Returns the stable slug used in view state, mapping rules, and later persistence boundaries.
    let slug =
        function
        | PaymentMethod.Cash -> "cash"
        | PaymentMethod.Card -> "card"
        | PaymentMethod.App -> "app"
        | PaymentMethod.Points -> "points"

    /// Returns the user-facing label for the payment method.
    let displayName =
        function
        | PaymentMethod.Cash -> "Cash"
        | PaymentMethod.Card -> "Card"
        | PaymentMethod.App -> "App"
        | PaymentMethod.Points -> "Points"

/// Enumerates the first known CheddarBooks LaundryLog workflow stages.
type SessionFlowStage =
    | NewSession
    | LocationEntered
    | EntryForm

[<RequireQualifiedAccess>]
module SessionFlowStage =
    /// Returns the stable path slug aligned with the current LaundryLog path vocabulary.
    let pathSlug =
        function
        | SessionFlowStage.NewSession -> "path1-1-new-session"
        | SessionFlowStage.LocationEntered -> "path1-2-location-entered"
        | SessionFlowStage.EntryForm -> "path1-3-entry-form"

    /// Returns the user-facing stage label.
    let displayName =
        function
        | SessionFlowStage.NewSession -> "New Session"
        | SessionFlowStage.LocationEntered -> "Location Entered"
        | SessionFlowStage.EntryForm -> "Entry Form"

/// Represents a validated human-entered laundry location label.
type LocationName = private LocationName of string

[<RequireQualifiedAccess>]
module LocationName =
    let private isAllowedCharacter character =
        Char.IsAsciiLetterOrDigit character
        || character = ' '
        || character = '-'
        || character = '\''
        || character = '.'
        || character = ','
        || character = '&'
        || character = '#'

    /// Creates a validated location label using an explicit ASCII allowlist.
    let tryCreate (value: string) =
        if String.IsNullOrWhiteSpace value then
            Error "Location names must not be blank."
        elif value |> Seq.exists (isAllowedCharacter >> not) then
            Error "Location names may contain only ASCII letters, digits, space, hyphen, apostrophe, dot, comma, ampersand, and hash."
        else
            Ok(LocationName(value.Trim()))

    /// Extracts the underlying location label.
    let value (LocationName value) = value
