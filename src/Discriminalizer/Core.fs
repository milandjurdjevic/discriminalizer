module Discriminalizer.Core

open System
open System.Globalization
open System.Text.Json
open System.Text.Json.Nodes

let tupleOption (this: bool * 'a) =
    if fst this then Some(snd this) else None

let tryParseNumber (number: string) =
    let culture = CultureInfo.InvariantCulture

    Byte.TryParse(number, culture)
    |> tupleOption
    |> Option.map box
    |> Option.orElseWith (fun () -> Int16.TryParse(number, culture) |> tupleOption |> Option.map box)
    |> Option.orElseWith (fun () -> Int32.TryParse(number, culture) |> tupleOption |> Option.map box)
    |> Option.orElseWith (fun () -> Int64.TryParse(number, culture) |> tupleOption |> Option.map box)
    |> Option.orElseWith (fun () ->
        Single.TryParse(number, culture)
        |> tupleOption
        |> Option.filter Single.IsFinite
        |> Option.map box)
    |> Option.orElseWith (fun () ->
        Double.TryParse(number, culture)
        |> tupleOption
        |> Option.filter Double.IsFinite
        |> Option.map box)
    // BUG: Decimal Parser Not Reached
    |> Option.orElseWith (fun () -> Decimal.TryParse(number, culture) |> tupleOption |> Option.map box)

let parseJsonValue (this: JsonValue) : obj =
    match this.GetValueKind() with
    | JsonValueKind.String -> this.GetValue<string>()
    | JsonValueKind.Number -> this.ToString() |> tryParseNumber |> Option.get
    | JsonValueKind.True -> true
    | JsonValueKind.False -> false
    | JsonValueKind.Undefined -> null
    | JsonValueKind.Null -> null
    | _ -> invalidOp $"Unknown value kind: {this.ToString()}."
