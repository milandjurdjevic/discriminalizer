[<VerifyXunit.UsesVerify>]
module Discriminalizer.DiscriminatorTest

open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Json.Serialization
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

let discriminator =
    Discriminator(JsonSerializerOptions.Default, "Type", "Origin")
        .WithSchema<WildDog>("Dog", "Wild")
        .WithSchema<DomesticDog>("Dog", "Domestic")
        .WithSchema<WildCat>("Cat", "Wild")
        .WithSchema<DomesticCat>("Cat", "Domestic")

let deserialize (value: string) =
    let bytes = Encoding.UTF8.GetBytes value
    use stream = new MemoryStream(bytes)
    JsonSerializer.Deserialize<JsonNode> stream

[<Fact>]
let ``deserialize object`` () =
    // lang=json
    """{"Type": "Dog", "Origin": "Domestic" }"""
    |> deserialize
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``deserialize array with null`` () =
    // lang=json
    """[{"Type": "Dog", "Origin": "Domestic"}, null]"""
    |> deserialize
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``deserialize array`` () =
    // lang=json
    """
    [
      {"Type": "Dog", "Origin": "Domestic" },
      {"Type": "Cat", "Origin": "Domestic" },
      {"Type": "Cat", "Origin": "Wild" }
    ]
    """
    |> deserialize
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``deserialize schemaless object`` () =
    //lang=json
    """
      {
        "Type": "Fish",
        "Wild": true,
        "Weight": 2.5,
        "Attributes": [
         "Fast",
         "Small"
        ],
        "Animals": [
          { "Type": "Fish", "Origin": "Wild" },
          { "Type": "Fish", "Origin": null }
        ]
      }
    """
    |> deserialize
    |> discriminator.WithSchemaless().Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``deserialize schemaless array`` () =
    //lang=json
    """
    [
      {
        "Type": "Fish",
        "Wild": false,
        "Weight": 2.5,
        "Attributes": [
         "Fast",
         "Small"
        ],
        "Animals": [
          { "Type": "Fish", "Origin": "Wild" },
          { "Type": "Fish", "Origin": null }
        ]
      },
      {
        "Type": "Fish",
        "Wild": true,
        "Weight": 25,
        "Attributes": [
         "Slow",
         "Large"
        ]
      }
    ]
    """
    |> deserialize
    |> discriminator.WithSchemaless().Discriminate
    |> Verifier.Verify
    |> _.ToTask()
