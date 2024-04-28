namespace Discriminalizer

open System
open System.Globalization
open System.Runtime.CompilerServices

type internal StringExtensions() =

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
