namespace Discriminalizer

open System
open System.Globalization
open System.Text.Json
open System.Text.Json.Nodes

type SchemalessDiscriminator() =

    static member private TryReturn(returnValue: bool * 'a) =
        if fst returnValue then
            Some(snd returnValue |> box)
        else
            None

    static member private MustBeFalse (condition: 'a -> bool) (returnValue: bool * 'a) =
        fst returnValue && condition (snd returnValue) |> not, snd returnValue

    static member private ParseNumberValue(value: string) =
        let culture = CultureInfo.InvariantCulture

        Byte.TryParse value
        |> SchemalessDiscriminator.TryReturn
        |> Option.orElseWith (fun _ -> Int16.TryParse(value, culture) |> SchemalessDiscriminator.TryReturn)
        |> Option.orElseWith (fun _ -> Int32.TryParse(value, culture) |> SchemalessDiscriminator.TryReturn)
        |> Option.orElseWith (fun _ -> Int64.TryParse(value, culture) |> SchemalessDiscriminator.TryReturn)
        |> Option.orElseWith (fun _ ->
            Single.TryParse(value, culture)
            |> SchemalessDiscriminator.MustBeFalse Single.IsInfinity
            |> SchemalessDiscriminator.TryReturn)
        |> Option.orElseWith (fun _ ->
            Double.TryParse(value, culture)
            |> SchemalessDiscriminator.MustBeFalse Double.IsInfinity
            |> SchemalessDiscriminator.TryReturn)
        // BUG: Decimal Parsed As Single
        |> Option.orElseWith (fun _ -> Decimal.TryParse(value, culture) |> SchemalessDiscriminator.TryReturn)
        |> Option.get


    static member private ParseValue(value: JsonValue) : obj =
        match value.GetValueKind() with
        | JsonValueKind.String -> value.GetValue<string>()
        | JsonValueKind.Number -> value.ToString() |> SchemalessDiscriminator.ParseNumberValue
        | JsonValueKind.True -> true
        | JsonValueKind.False -> false
        | JsonValueKind.Undefined -> null
        | JsonValueKind.Null -> null
        | _ -> invalidOp "Invalid JSON value kind."


    interface IDiscriminator with
        member this.Discriminate node =
            let rec parseNode (currentNode: JsonNode) =
                match currentNode with
                | :? JsonValue as value -> SchemalessDiscriminator.ParseValue value
                | :? JsonObject as object ->
                    object |> Seq.map (fun n -> n.Key, parseNode n.Value) |> readOnlyDict |> box
                | :? JsonArray as array -> array |> Seq.map parseNode |> box
                | _ -> null

            parseNode node

        member this.Discern _ = true
