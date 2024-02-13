module Discriminalizer.Json

open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Text.Json.Nodes
open System.Threading
open Microsoft.FSharp.Core

let private tryFindNodeValue (node: JsonNode) (path: string) : string option =
    node.AsObject()
    |> Seq.tryFind (_.Key.Equals(path, StringComparison.OrdinalIgnoreCase))
    |> fun value ->
        match value with
        | None -> None
        | Some some -> Some(some.Value.ToString())

let rec private toSchemaless (node: JsonNode) =
    match node with
    | :? JsonValue as jsonValue -> jsonValue.ToString() :> obj
    | :? JsonObject as jsonObject -> jsonObject |> Seq.map (fun x -> x.Key, toSchemaless x.Value) |> readOnlyDict :> obj
    | :? JsonArray as jsonArray -> jsonArray |> Seq.map toSchemaless :> obj
    | _ -> failwith "Unsupported Json Type"

let private ofObject (discriminators: Discriminator seq) (options: JsonSerializerOptions) (json: JsonNode) =
    discriminators
    |> Seq.map (fun discriminator -> discriminator, discriminator.Paths |> Seq.map (tryFindNodeValue json))
    |> Seq.filter (fun (_, values) -> values |> Seq.forall (fun value -> value.IsSome))
    |> Seq.map (fun (discriminator, values) -> discriminator, values |> Seq.map (_.Value))
    |> Seq.map (fun (discriminator, values) -> discriminator.Values.TryGetValue(values |> String.concat String.Empty))
    |> Seq.filter fst
    |> Seq.map snd
    |> Seq.tryHead
    |> fun returnType ->
        match returnType with
        | None -> json |> toSchemaless
        | Some some -> json.Deserialize(some, options)

let private ofArray (discriminators: Discriminator seq) (options: JsonSerializerOptions) (json: JsonArray) =
    json
    |> Seq.filter (fun i -> not (isNull i))
    |> Seq.map (ofObject discriminators options)

let private enumerate object = seq { yield object }

let OfStream
    (stream: Stream)
    (discriminators: IReadOnlyCollection<Discriminator>)
    (options: JsonSerializerOptions)
    (cancellationToken: CancellationToken)
    =
    task {
        let! node = JsonSerializer.DeserializeAsync<JsonNode>(stream, options, cancellationToken)

        return
            match node with
            | :? JsonArray as array -> ofArray discriminators options array
            | :? JsonObject as object -> ofObject discriminators options object |> enumerate
            | _ -> Seq.empty<obj>
    }
