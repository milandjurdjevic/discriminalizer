namespace Discriminalizer

open System
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core
open System.Linq

type SchemabasedDiscriminator(fields: string array, values: Map<string, Type>, options: JsonSerializerOptions) =
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

    interface IDiscriminator with
        member this.Discriminate node =
            match node with
            | :? JsonArray as array ->
                array.OfType<JsonObject>()
                |> Seq.map this.TryParse
                |> Seq.filter Option.isSome
                |> Seq.map Option.get
                |> box
            | :? JsonObject as object -> object |> this.TryParse |> Option.defaultValue null
            | _ -> null

        member this.Discern node =
            match node with
            | :? JsonObject as object -> this.TryDiscern object |> Option.isSome
            | :? JsonArray as array ->
                array.OfType<JsonObject>()
                |> Seq.map this.TryDiscern
                |> Seq.forall Option.isSome
            | _ -> false
