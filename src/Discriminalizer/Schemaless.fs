module internal Discriminalizer.Schemaless

open System
open System.Globalization
open System.Text.Json
open System.Text.Json.Nodes

// Converts the string number the smallest possible .NET number type that can hold the value.
let private convertStringNumber (value: string) : obj =
    let tryReturn (result: bool * 'a) =
        if fst result then Some(snd result) else None

    let tryReturnBoxed result = result |> tryReturn |> Option.map box

    let noneIf (shouldNone: 'a -> bool) (result: 'a option) =
        result |> Option.bind (fun v -> if shouldNone v then None else Some v)

    let orElse (b: unit -> obj option) (a: obj option) = a |> Option.orElse <| b ()

    let int16Parse () =
        Int16.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturnBoxed

    let int32Parse () =
        Int32.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturnBoxed

    let int64Parse () =
        Int64.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturnBoxed

    let singleParse () =
        Single.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturn
        |> noneIf Single.IsInfinity
        |> Option.map box

    let doubleParse () =
        Double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturn
        |> noneIf Double.IsInfinity
        |> Option.map box

    let decimalParse () =
        Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturnBoxed

    let byteParse () =
        Byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture)
        |> tryReturnBoxed

    byteParse ()
    |> orElse int16Parse
    |> orElse int32Parse
    |> orElse int64Parse
    |> orElse singleParse
    |> orElse doubleParse
    |> orElse decimalParse
    |> Option.get

// Converts a JSON value to a .NET object.
let private convertValue (value: JsonValue) : obj =
    match value.GetValueKind() with
    | JsonValueKind.String -> value.GetValue<string>()
    | JsonValueKind.Number -> value.ToString() |> convertStringNumber
    | JsonValueKind.True -> true
    | JsonValueKind.False -> false
    | JsonValueKind.Undefined -> null
    | JsonValueKind.Null -> null
    | _ -> invalidOp "Invalid JSON value kind."

// Converts a JSON node to a .NET object.
let rec ofNode (node: JsonNode) : obj =
    match node with
    | :? JsonValue as value -> convertValue value
    | :? JsonObject as object -> object |> Seq.map (fun n -> n.Key, ofNode n.Value) |> readOnlyDict |> box
    | :? JsonArray as array -> array |> Seq.map ofNode |> box
    | _ -> null
