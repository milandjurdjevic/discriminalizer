namespace Discriminalizer

open System
open System.Text.Json
open System.Text.Json.Nodes
open Microsoft.FSharp.Core
open Deserializer

// OOP Wrapper API

[<AbstractClass>]
type Discriminator(discriminator: (JsonNode -> obj seq) * (JsonNode -> bool)) =

    new() =
        let discriminator = (fun _ -> Seq.empty), (fun _ -> false)
        Discriminator(discriminator)

    abstract member Discriminate: JsonNode -> obj seq
    default _.Discriminate node = fst discriminator <| node

    abstract member Discern: JsonNode -> bool
    default _.Discern(node: JsonNode) = snd discriminator <| node


type SchemabasedDiscriminator private (fields: string seq, values: Map<string, Type>, options: JsonSerializerOptions) =

    inherit
        Discriminator(
            ((concreteParser (typeProvider fields values) options), (concreteCheck (typeProvider fields values)))
        )

    new(options: JsonSerializerOptions, [<ParamArray>] fields: string array) =
        SchemabasedDiscriminator(fields, Map.empty, options)

    member _.WithType<'a>([<ParamArray>] typeValues: string array) =
        let key = typeValues |> String.concat String.Empty
        SchemabasedDiscriminator(fields, values.Add(key, typeof<'a>), options)

type SchemalessDiscriminator() =
    inherit Discriminator((dynamicParse, (fun _ -> true)))

type CompositeDiscriminator([<ParamArray>] discriminators: Discriminator array) =
    inherit
        Discriminator(
            discriminators
            |> Array.map (fun discriminator -> (discriminator.Discriminate, discriminator.Discern))
            |> compose
        )
