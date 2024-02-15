[<VerifyXunit.UsesVerify>]
module Discriminalizer.JsonTests

open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open VerifyXunit
open Xunit

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

[<Fact>]
let ``Deserialize a single object`` () =
    async {
        // lang=json
        let json = """{"Type": "Dog", "Origin": "Domestic" }"""
        let! output = deserialize defaultOptions json
        do! Verifier.Verify(output).ToTask() |> Async.AwaitTask |> Async.Ignore
    }

[<Fact>]
let ``Deserialize array that contains null`` () =
    async {
        // lang=json
        let json = """[{"Type": "Dog"}, null]"""
        let! output = deserialize defaultOptions json
        do! Verifier.Verify(output).ToTask() |> Async.AwaitTask |> Async.Ignore
    }

[<Fact>]
let ``Deserialize array with two discriminator schemes`` () =
    async {
        // lang=json
        let json =
            """
            [
              {"Type": "Dog", "Origin": "Domestic" },
              {"Type": "Cat", "Origin": "Domestic" },
              {"Type": "Cat", "Origin": "Wild" },
              {"Type": "Cat" },
              {"Type": "Dog" }
            ]
            """

        let! output = deserialize defaultOptions json
        do! Verifier.Verify(output).ToTask() |> Async.AwaitTask |> Async.Ignore
    }

[<Theory>]
[<InlineData true>]
[<InlineData false>]
let ``The deserialized array with some schemaless objects`` (includeSchemaless: bool) =
    async {
        //lang=json
        let json =
            """
            [
              { "Type": "Dog", "Origin": "Domestic" },
              { 
                "Animal": { "Type": "Fish", "Origin": "Wild" }
              },
              {
                "Animals": [
                  { "Type": "Fish", "Origin": "Wild" },
                  { "Type": "Fish", "Origin": "Domestic" }
                ]
              }
            ]
            """

        let options =
            { Serializer = JsonSerializerOptions()
              Discriminators = discriminators
              IncludeSchemaless = includeSchemaless }

        let! output = deserialize options json

        do!
            Verifier.Verify(output).UseParameters(includeSchemaless).ToTask()
            |> Async.AwaitTask
            |> Async.Ignore
    }
