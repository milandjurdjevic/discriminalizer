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

module Schema =
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

    let discriminator =
        Discriminator(JsonSerializerOptions.Default, "Type", "Origin")
            .WithSchema<WildDog>("Dog", "Wild")
            .WithSchema<DomesticDog>("Dog", "Domestic")
            .WithSchema<WildCat>("Cat", "Wild")
            .WithSchema<DomesticCat>("Cat", "Domestic")
