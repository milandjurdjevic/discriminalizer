module Discriminalizer.Deserializer

open System
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core
open System.Linq

type private Parser = JsonNode -> obj seq
type private Validator = JsonNode -> bool
type private Discriminator = Parser * Validator

let compose (discriminators: Discriminator seq) =
    let parser (node: JsonNode) : obj seq =
        discriminators
        |> Seq.tryFind (fun discriminator -> snd discriminator <| node)
        |> Option.map (fun discriminator -> fst discriminator <| node)
        |> Option.defaultValue Seq.empty

    let check (node: JsonNode) =
        discriminators |> Seq.exists (fun discriminator -> snd discriminator <| node)

    (parser, check)

let dynamicParse (node: JsonNode) =
    let rec parser (currentNode: JsonNode) =
        match currentNode with
        | :? JsonValue as value -> Core.parseJsonValue value
        | :? JsonObject as object -> object |> Seq.map (fun n -> n.Key, parser n.Value) |> readOnlyDict |> box
        | :? JsonArray as array -> array |> Seq.map parser |> box
        | _ -> null

    match parser node with
    | :? seq<obj> as enumerable -> enumerable
    | single -> Seq.singleton single

let concreteCheck (typeProvider: JsonObject -> Type option) (node: JsonNode) =
    match node with
    | :? JsonObject as object -> typeProvider object |> Option.isSome
    | :? JsonArray as array -> array.OfType<JsonObject>() |> Seq.map typeProvider |> Seq.forall Option.isSome
    | _ -> false

let concreteParser (typeProvider: JsonObject -> Type option) (options: JsonSerializerOptions) (node: JsonNode) =
    let objectParser (object: JsonObject) =
        typeProvider object |> Option.map (fun t -> object.Deserialize(t, options))

    match node with
    | :? JsonArray as array ->
        array.OfType<JsonObject>()
        |> Seq.map objectParser
        |> Seq.filter Option.isSome
        |> Seq.map Option.get
    | :? JsonObject as object ->
        object
        |> objectParser
        |> Option.map Seq.singleton
        |> Option.defaultValue Seq.empty
    | _ -> Seq.empty

let typeProvider (fields: string seq) (values: Map<string, Type>) (object: JsonObject) =
    let fieldValueProvider (object: JsonObject) field =
        object
        |> Seq.tryFind (_.Key.Equals(field, StringComparison.OrdinalIgnoreCase))
        |> Option.map (_.Value.ToString())

    fields
    |> Seq.map (fieldValueProvider object)
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    |> String.concat String.Empty
    |> values.TryFind
