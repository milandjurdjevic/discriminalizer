namespace Discriminalizer

open System
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core
open System.Linq

[<AbstractClass>]
type Discriminator() =

    abstract member Discriminate: node: JsonNode -> obj seq

    abstract member Discern: node: JsonNode -> bool

type SchemabasedDiscriminator private (fields: string seq, values: Map<string, Type>, options: JsonSerializerOptions) =

    inherit Discriminator()

    new(options: JsonSerializerOptions, [<ParamArray>] fields: string array) =
        SchemabasedDiscriminator(fields, Map.empty, options)

    member _.WithType<'a>([<ParamArray>] typeValues: string array) =
        let key = typeValues |> String.concat String.Empty
        SchemabasedDiscriminator(fields, values.Add(key, typeof<'a>), options)

    member private _.TryDiscern(object: JsonObject) : Type option =
        let findFieldValue field =
            object
            |> Seq.tryFind (_.Key.Equals(field, StringComparison.OrdinalIgnoreCase))
            |> Option.map (_.Value.ToString())

        fields
        |> Seq.map findFieldValue
        |> Seq.filter Option.isSome
        |> Seq.map Option.get
        |> String.concat String.Empty
        |> values.TryFind

    member private this.TryParse(object: JsonObject) : obj option =
        this.TryDiscern object |> Option.map (fun t -> object.Deserialize(t, options))

    override this.Discriminate node =
        match node with
        | :? JsonArray as array ->
            array.OfType<JsonObject>()
            |> Seq.map this.TryParse
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        | :? JsonObject as object ->
            object
            |> this.TryParse
            |> Option.map Seq.singleton
            |> Option.defaultValue Seq.empty
        | _ -> Seq.empty

    override this.Discern node =
        match node with
        | :? JsonObject as object -> this.TryDiscern object |> Option.isSome
        | :? JsonArray as array ->
            array.OfType<JsonObject>()
            |> Seq.map this.TryDiscern
            |> Seq.forall Option.isSome
        | _ -> false

type SchemalessDiscriminator() =
    inherit Discriminator()

    override this.Discriminate node =
        let rec parseNode (currentNode: JsonNode) =
            match currentNode with
            | :? JsonValue as value -> value.ToObject()
            | :? JsonObject as object -> object |> Seq.map (fun n -> n.Key, parseNode n.Value) |> readOnlyDict |> box
            | :? JsonArray as array -> array |> Seq.map parseNode |> box
            | _ -> null

        match parseNode node with
        | :? seq<obj> as enumerable -> enumerable
        | single -> Seq.singleton single

    override this.Discern _ = true

type CompositeDiscriminator([<ParamArray>] discriminators: Discriminator array) =
    inherit Discriminator()

    override this.Discriminate node =
        discriminators
        |> Seq.tryFind (fun d -> d.Discern node)
        |> Option.map (fun d -> d.Discriminate node)
        |> Option.defaultValue Seq.empty

    override this.Discern node =
        discriminators |> Seq.exists (fun d -> d.Discern node)
