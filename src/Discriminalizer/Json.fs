namespace Discriminalizer

open System.Text.Json

type JsonOptions =
    { Serializer: JsonSerializerOptions
      Discriminators: Discriminator seq
      IncludeSchemaless: bool }

module Json =

    open System
    open System.IO
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
        | :? JsonObject as jsonObject ->
            jsonObject
            |> Seq.map (fun node -> node.Key, toSchemaless node.Value)
            |> readOnlyDict
            :> obj
        | :? JsonArray as jsonArray -> jsonArray |> Seq.map toSchemaless :> obj
        | _ -> failwith $"Unsupported node type: {node.GetType().FullName}"

    let private ofObject (options: JsonOptions) (json: JsonNode) =
        options.Discriminators
        |> Seq.map (fun discriminator -> discriminator, discriminator.Paths |> Seq.map (tryFindNodeValue json))
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
                | true -> Some(toSchemaless json)
                | false -> None
            | Some some -> Some(json.Deserialize(some, options.Serializer))

    let private ofArray (options: JsonOptions) (json: JsonArray) =
        json |> Seq.filter (fun i -> not (isNull i)) |> Seq.map (ofObject options)


    let OfStream (stream: Stream) (options: JsonOptions) (cancellationToken: CancellationToken) =
        task {
            let! node = JsonSerializer.DeserializeAsync<JsonNode>(stream, options.Serializer, cancellationToken)

            return
                match node with
                | :? JsonArray as array -> ofArray options array |> Seq.filter (_.IsSome) |> Seq.map Option.get
                | :? JsonObject as object ->
                    ofObject options object
                    |> fun option ->
                        match option with
                        | Some some -> [ some ]
                        | None -> Seq.empty<obj>
                | _ -> Seq.empty<obj>
        }
