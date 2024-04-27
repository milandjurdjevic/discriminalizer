namespace Discriminalizer

open System
open System.Globalization
open System.Text.Json
open System.Text.Json.Nodes

type SchemalessDiscriminator() =
    interface IDiscriminator with
        member this.Discriminate node =
            let parseNumberString (value: string) =
                let tryReturn (returnValue: bool * 'a) =
                    if fst returnValue then Some(snd returnValue) else None

                let tryReturnBoxed returnValue =
                    returnValue |> tryReturn |> Option.map box

                let tryReturnBoxedIfNot (isInfinity: 'a -> bool) returnValue =
                    tryReturn returnValue
                    |> Option.bind (fun v -> if isInfinity v then None else Some v)
                    |> Option.map box

                let culture = CultureInfo.InvariantCulture

                Byte.TryParse value
                |> tryReturnBoxed
                |> Option.orElseWith (fun _ -> Int16.TryParse(value, culture) |> tryReturnBoxed)
                |> Option.orElseWith (fun _ -> Int32.TryParse(value, culture) |> tryReturnBoxed)
                |> Option.orElseWith (fun _ -> Int64.TryParse(value, culture) |> tryReturnBoxed)
                |> Option.orElseWith (fun _ -> Single.TryParse(value, culture) |> tryReturnBoxedIfNot Single.IsInfinity)
                |> Option.orElseWith (fun _ -> Double.TryParse(value, culture) |> tryReturnBoxedIfNot Double.IsInfinity)
                // BUG: Decimal Parsed As Single
                |> Option.orElseWith (fun _ -> Decimal.TryParse(value, culture) |> tryReturnBoxed)
                |> Option.get

            let parseValue (value: JsonValue) : obj =
                match value.GetValueKind() with
                | JsonValueKind.String -> value.GetValue<string>()
                | JsonValueKind.Number -> value.ToString() |> parseNumberString
                | JsonValueKind.True -> true
                | JsonValueKind.False -> false
                | JsonValueKind.Undefined -> null
                | JsonValueKind.Null -> null
                | _ -> invalidOp "Invalid JSON value kind."

            let rec parseNode (currentNode: JsonNode) =
                match currentNode with
                | :? JsonValue as value -> parseValue value
                | :? JsonObject as object ->
                    object |> Seq.map (fun n -> n.Key, parseNode n.Value) |> readOnlyDict |> box
                | :? JsonArray as array -> array |> Seq.map parseNode |> box
                | _ -> null

            parseNode node

        member this.Discern _ = true
