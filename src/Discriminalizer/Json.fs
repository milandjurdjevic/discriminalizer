namespace Discriminalizer

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core

type Discriminator private (paths: IReadOnlyCollection<string>, values: IReadOnlyDictionary<string, Type>) =
    new([<ParamArray>] paths: string array) = Discriminator(paths, ImmutableDictionary.Empty)

    member _.Paths = paths
    member _.Values = values

    member _.With<'T>([<ParamArray>] keys: string array) =
        let dictionary = Dictionary(values)
        dictionary.Add(keys |> String.concat String.Empty, typeof<'T>)
        Discriminator(paths, dictionary.ToImmutableDictionary())

module private JsonSerializerOptions =
    let toReadOnly (options: JsonSerializerOptions) =
        options.MakeReadOnly()
        options

type JsonOptions private (serializer: JsonSerializerOptions, discriminators: Discriminator seq, includeSchemaless: bool)
    =
    new() = JsonOptions(JsonSerializerOptions.Default |> JsonSerializerOptions.toReadOnly, Seq.empty, false)

    member _.Serializer = serializer
    member _.Discriminators = discriminators
    member _.IncludeSchemaless = includeSchemaless

    member _.WithSerializer(serializer: JsonSerializerOptions) =
        JsonOptions(serializer |> JsonSerializerOptions.toReadOnly, discriminators, includeSchemaless)

    member _.WithDiscriminator(discriminator) =
        let discriminators = discriminator |> Seq.singleton |> Seq.append discriminators
        JsonOptions(serializer, discriminators, includeSchemaless)

    member _.WithSchemaless() =
        JsonOptions(serializer, discriminators, true)

module private Nullable =
    let isNotNull nullable = not (isNull nullable)

module private JsonObject =
    let rec private toSchemaless (node: JsonNode) =
        match node with
        | :? JsonValue as jsonValue -> jsonValue.ToString() :> obj
        | :? JsonObject as jsonObject ->
            jsonObject
            |> Seq.map (fun node -> node.Key, toSchemaless node.Value)
            |> readOnlyDict
            :> obj
        | :? JsonArray as jsonArray -> jsonArray |> Seq.map toSchemaless :> obj
        | _ -> failwith $"Unsupported node type: {node.GetType().FullName}"

    let private tryGetNode (path: string) (object: JsonObject) =
        object
        |> Seq.tryFind (_.Key.Equals(path, StringComparison.OrdinalIgnoreCase))
        |> fun value ->
            match value with
            | None -> None
            | Some some -> Some(some.Value.ToString())

    let deserialize (options: JsonOptions) (object: JsonObject) =
        options.Discriminators
        |> Seq.map (fun discriminator ->
            discriminator, discriminator.Paths |> Seq.map (fun path -> tryGetNode path object))
        |> Seq.filter (fun (_, values) -> values |> Seq.forall (fun value -> value.IsSome))
        |> Seq.map (fun (discriminator, values) -> discriminator, values |> Seq.map (_.Value))
        |> Seq.map (fun (discriminator, values) ->
            discriminator.Values.TryGetValue(values |> String.concat String.Empty))
        |> Seq.filter fst
        |> Seq.map snd
        |> Seq.tryHead
        |> fun typeOption ->
            match typeOption with
            | None ->
                match options.IncludeSchemaless with
                | true -> Some(toSchemaless object)
                | false -> None
            | Some some -> Some(object.Deserialize(some, options.Serializer))

module Json =
    let Deserialize (node: JsonNode) (options: JsonOptions) =
        match node with
        | :? JsonArray as array ->
            array
            |> Seq.filter Nullable.isNotNull
            |> Seq.map _.AsObject()
            |> Seq.map (JsonObject.deserialize options)
            |> Seq.filter (_.IsSome)
            |> Seq.map Option.get
        | :? JsonObject as object ->
            object
            |> JsonObject.deserialize options
            |> Option.map Seq.singleton
            |> Option.defaultValue Seq.empty
        | _ -> Seq.empty
