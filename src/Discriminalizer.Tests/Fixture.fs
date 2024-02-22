namespace Discriminalizer.Tests

open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Json.Serialization
open Discriminalizer

module String =
    let toStream (value: string) =
        let bytes = Encoding.UTF8.GetBytes value
        new MemoryStream(bytes)

module Stream =
    let toJsonNode (stream: Stream) =
        JsonSerializer.Deserialize<JsonNode> stream

module JsonNode =
    let deserialize (options: JsonOptions) (node: JsonNode) = Json.Deserialize node options

module Scheme =
    [<AbstractClass>]
    type Animal() =
        [<JsonPropertyName("Type")>]
        member val AnimalType: string = "" with get, set

        [<JsonPropertyName("Origin")>]
        member val AnimalOrigin: string = "" with get, set

        member this.ClassType: string = this.GetType().Name

    type Cat() =
        inherit Animal()

    type DomesticCat() =
        inherit Cat()

    type WildCat() =
        inherit Cat()

    type Dog() =
        inherit Animal()

    type DomesticDog() =
        inherit Dog()

    type WildDog() =
        inherit Dog()

    let options =
        { Serializer = JsonSerializerOptions()
          Discriminators =
            [|
               // Type and Origin based discriminator
               Discriminator("Type", "Origin")
                   .With<WildDog>("Dog", "Wild")
                   .With<DomesticDog>("Dog", "Domestic")
                   .With<WildCat>("Cat", "Wild")
                   .With<DomesticCat>("Cat", "Domestic")
               // Type based discriminator
               Discriminator("Type").With<Cat>("Cat").With<Dog>("Dog") |]
          IncludeSchemaless = false }
