module Discriminalizer.Tests.Fixture

open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open Discriminalizer

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

let discriminators =
    [|
       // Type and Origin based discriminator
       Discriminator("Type", "Origin")
           .With<WildDog>("Dog", "Wild")
           .With<DomesticDog>("Dog", "Domestic")
           .With<WildCat>("Cat", "Wild")
           .With<DomesticCat>("Cat", "Domestic")
       // Type based discriminator
       Discriminator("Type").With<Cat>("Cat").With<Dog>("Dog") |]

let deserialize (options: JsonOptions) (json: string) =
    async {
        let bytes = Encoding.UTF8.GetBytes json
        use stream = new MemoryStream(bytes)
        return! Json.OfStream stream options CancellationToken.None |> Async.AwaitTask
    }

let defaultOptions =
    { Serializer = JsonSerializerOptions()
      Discriminators = discriminators
      IncludeSchemaless = false }
