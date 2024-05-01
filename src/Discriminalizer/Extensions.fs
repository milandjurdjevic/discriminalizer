namespace Discriminalizer

open System
open System.Globalization
open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Nodes

type internal Extensions() =

    [<Extension>]
    static member ToOption(this: bool * 'a) =
        if fst this then Some(snd this) else None


    [<Extension>]
    static member ToNumber(this: string) =
        let culture = CultureInfo.InvariantCulture

        Byte.TryParse(this, culture).ToOption()
        |> Option.map box
        |> Option.orElseWith (fun () -> Int16.TryParse(this, culture).ToOption() |> Option.map box)
        |> Option.orElseWith (fun () -> Int32.TryParse(this, culture).ToOption() |> Option.map box)
        |> Option.orElseWith (fun () -> Int64.TryParse(this, culture).ToOption() |> Option.map box)
        |> Option.orElseWith (fun () ->
            Single.TryParse(this, culture).ToOption()
            |> Option.filter Single.IsFinite
            |> Option.map box)
        |> Option.orElseWith (fun () ->
            Double.TryParse(this, culture).ToOption()
            |> Option.filter Double.IsFinite
            |> Option.map box)
        // BUG: Decimal Parser Not Reached
        |> Option.orElseWith (fun () -> Decimal.TryParse(this, culture).ToOption() |> Option.map box)

    [<Extension>]
    static member ToObject(this: JsonValue) : obj =
        match this.GetValueKind() with
        | JsonValueKind.String -> this.GetValue<string>()
        | JsonValueKind.Number -> this.ToString() |> _.ToNumber() |> Option.get
        | JsonValueKind.True -> true
        | JsonValueKind.False -> false
        | JsonValueKind.Undefined -> null
        | JsonValueKind.Null -> null
        | _ -> invalidOp $"Unknown value kind: {this.ToString()}."
