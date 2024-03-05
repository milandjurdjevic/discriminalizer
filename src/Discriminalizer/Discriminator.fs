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

    let tryToSchemaless (condition: bool) (node: JsonNode) =
        if condition then Some(toSchemaless node) else None

    let tryGetValue (object: JsonObject) (field: string) =
        object
        |> Seq.tryFind (_.Key.Equals(field, StringComparison.OrdinalIgnoreCase))
        |> Option.map (_.Value.ToString())

    let ofNode (node: JsonNode) = node.AsObject()

type Discriminator
    private (fields: string array, types: Map<string, Type>, options: JsonSerializerOptions, isSchemalessIncluded: bool)
    =
    new(options: JsonSerializerOptions, [<ParamArray>] paths: string array) =
        if not options.IsReadOnly then
            invalidArg "options" "Options must be read-only"

        Discriminator(paths, Map.empty, options, false)

    member _.WithSchema<'T>([<ParamArray>] values: string array) =
        let extended = types.Add(values |> String.concat String.Empty, typeof<'T>)
        Discriminator(fields, extended, options, isSchemalessIncluded)

    member _.WithSchemaless() =
        Discriminator(fields, types, options, true)

    member this.Discriminate(node: JsonNode) =
        match node with
        | :? JsonArray as array ->
            array
            |> Seq.filter (fun node -> not (isNull node) && node.GetValueKind() = JsonValueKind.Object)
            |> Seq.map JsonObject.ofNode
            |> Seq.map this.DiscriminateObject
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        | :? JsonObject as object ->
            object
            |> this.DiscriminateObject
            |> Option.map Seq.singleton
            |> Option.defaultValue Seq.empty
        | _ -> Seq.empty

    member private _.FormatDiscriminatorKey(object: JsonObject) =
        fields
        |> Seq.map (JsonObject.tryGetValue object)
        |> Seq.filter Option.isSome
        |> Seq.map Option.get
        |> String.concat String.Empty

    member private this.DiscriminateObject(object: JsonObject) : obj option =
        types
        |> Map.tryFind (this.FormatDiscriminatorKey object)
        |> Option.map (fun some -> object.Deserialize(some, options))
        |> Option.orElseWith (fun () -> object |> JsonObject.tryToSchemaless isSchemalessIncluded)
