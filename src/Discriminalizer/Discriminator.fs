namespace Discriminalizer

open System
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core

type Discriminator
    private (fields: string array, types: Map<string, Type>, options: JsonSerializerOptions, includeSchemaless: bool) =
    new(options: JsonSerializerOptions, [<ParamArray>] paths: string array) =
        Discriminator(paths, Map.empty, options, false)

    member _.WithSchema<'T>([<ParamArray>] values: string array) =
        let extended = types.Add(values |> String.concat String.Empty, typeof<'T>)
        Discriminator(fields, extended, options, includeSchemaless)

    member _.WithSchemaless() =
        Discriminator(fields, types, options, true)

    member private _.TryFindType(object: JsonObject) =
        let tryFindFieldValue name =
            object
            |> Seq.tryFind (_.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
            |> Option.map (_.Value.ToString())

        fields
        |> Seq.map tryFindFieldValue
        |> Seq.filter Option.isSome
        |> Seq.map Option.get
        |> String.concat String.Empty
        |> types.TryFind

    member private _.TryDeserializeAsSchemaless(object: JsonObject) =
        if includeSchemaless then
            Some(Schemaless.ofNode object)
        else
            None

    member private this.DiscriminateSingle(object: JsonObject) : obj option =
        this.TryFindType object
        |> Option.map (fun t -> object.Deserialize(t, options))
        |> Option.orElse (this.TryDeserializeAsSchemaless object)

    member this.Discriminate(jsonNode: JsonNode) =
        match jsonNode with
        | :? JsonArray as array ->
            array
            |> Seq.filter (fun n -> not <| isNull n && n.GetValueKind() = JsonValueKind.Object)
            |> Seq.map (_.AsObject())
            |> Seq.map this.DiscriminateSingle
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        | :? JsonObject as object ->
            object
            |> this.DiscriminateSingle
            |> Option.map Seq.singleton
            |> Option.defaultValue Seq.empty
        | _ -> Seq.empty
