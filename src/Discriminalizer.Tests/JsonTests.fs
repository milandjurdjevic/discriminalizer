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

[<Theory>]
// Deserialize single object that is defined in a scheme.
// lang=json
[<InlineData("""{"Type": "Dog", "Origin": "Domestic" }""")>]
// Deserialize array that contains objects without scheme (schemaless).
// lang=json
[<InlineData("""
[
  { 
    "Animal": { "Type": "Fish", "Origin": "Wild" }
  },
  {
    "Animals": [
      { "Type": "Fish", "Origin": "Wild" },
      { "Type": "Fish", "Origin": "Domestic" }
    ]
  }
]""")>]
// Deserialize array that contains objects with two schemes.
// lang=json
[<InlineData("""
[
  {"Type": "Dog", "Origin": "Domestic" },
  {"Type": "Cat", "Origin": "Domestic" },
  {"Type": "Cat", "Origin": "Wild" },
  {"Type": "Cat" },
  {"Type": "Dog" }
]
""")>]
// Deserialize array that contains a null object.
// lang=json
[<InlineData("""[{"Type": "Dog"}, null]""")>]
let ``The deserialized input matches the verified output`` (json: string) =
    async {
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

        let bytes = Encoding.UTF8.GetBytes json
        use stream = new MemoryStream(bytes)

        let options =
            { Serializer = JsonSerializerOptions()
              Discriminators = discriminators
              IncludeSchemaless = true }

        let! output = Json.OfStream stream options CancellationToken.None |> Async.AwaitTask

        do!
            Verifier.Verify(output).UseParameters(json).HashParameters().ToTask()
            |> Async.AwaitTask
            |> Async.Ignore
    }
