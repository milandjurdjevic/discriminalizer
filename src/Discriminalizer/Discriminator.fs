namespace Discriminalizer

open System
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core

module private JsonValue =
    let toObject (value: JsonValue) : obj =
        match value.GetValueKind() with
        | JsonValueKind.String -> value.GetValue<string>()
        | JsonValueKind.Number -> value.GetValue<double>()
        | JsonValueKind.True -> value.GetValue<bool>()
        | JsonValueKind.False -> value.GetValue<bool>()
        | JsonValueKind.Undefined -> null
        | JsonValueKind.Null -> null
        | _ -> invalidOp "Unsupported value kind."

module private JsonObject =
    let rec toSchemaless (node: JsonNode) : obj =
        match node with
        | :? JsonValue as jsonValue -> jsonValue |> JsonValue.toObject
        | :? JsonObject as jsonObject ->
            jsonObject
            |> Seq.map (fun node -> node.Key, toSchemaless node.Value)
            |> readOnlyDict
            |> box
        | :? JsonArray as jsonArray -> jsonArray |> Seq.map toSchemaless |> box
        | _ -> null

    let tryGetValueOf (field: string) (object: JsonObject) =
        object
        |> Seq.tryFind (_.Key.Equals(field, StringComparison.OrdinalIgnoreCase))
        |> Option.map (_.Value.ToString())

    let getValuesOf (fields: string array) (object: JsonObject) =
        fields
        |> Seq.map (fun field -> object |> tryGetValueOf field)
        |> Seq.filter Option.isSome
        |> Seq.map Option.get

module private JsonNode =
    let isObject (node: JsonNode) =
        not (isNull node) && node.GetValueKind() = JsonValueKind.Object

module private Option =
    let ofConditional condition value = if condition then Some value else None

type Discriminator
    private (fields: string array, types: Map<string, Type>, options: JsonSerializerOptions, isSchemaless: bool) =
    new(options: JsonSerializerOptions, [<ParamArray>] paths: string array) =
        if not options.IsReadOnly then
            invalidArg "options" "Options must be read-only"

        Discriminator(paths, Map.empty, options, false)

    member _.WithSchema<'T>([<ParamArray>] values: string array) =
        let extended = types.Add(values |> String.concat String.Empty, typeof<'T>)
        Discriminator(fields, extended, options, isSchemaless)

    member _.WithSchemaless() =
        Discriminator(fields, types, options, true)

    member this.Discriminate(jsonNode: JsonNode) =
        match jsonNode with
        | :? JsonArray as jsonArray ->
            jsonArray
            |> Seq.filter JsonNode.isObject
            |> Seq.map (_.AsObject())
            |> Seq.map this.DiscriminateSingle
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        | :? JsonObject as jsonObject ->
            jsonObject
            |> this.DiscriminateSingle
            |> Option.map Seq.singleton
            |> Option.defaultValue Seq.empty
        | _ -> Seq.empty

    member private _.DiscriminateSingle(jsonObject: JsonObject) : obj option =
        jsonObject
        |> JsonObject.getValuesOf fields
        |> String.concat String.Empty
        |> types.TryFind
        |> Option.map (fun some -> jsonObject.Deserialize(some, options))
        |> Option.orElse (Option.ofConditional isSchemaless (JsonObject.toSchemaless jsonObject))
