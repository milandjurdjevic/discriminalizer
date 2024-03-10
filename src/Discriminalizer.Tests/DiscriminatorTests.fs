[<VerifyXunit.UsesVerify>]
module Discriminalizer.DiscriminatorTests

open System.Text.Json
open Discriminalizer.Tests
open Discriminalizer.Tests.Schema
open VerifyXunit
open Xunit

[<Fact>]
let ``Deserialize object`` () =
    // lang=json
    """{"Type": "Dog", "Origin": "Domestic" }"""
    |> String.toStream
    |> Stream.toJsonNode
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``Deserialize array with null`` () =
    // lang=json
    """[{"Type": "Dog", "Origin": "Domestic"}, null]"""
    |> String.toStream
    |> Stream.toJsonNode
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``Deserialize array`` () =
    // lang=json
    """
    [
      {"Type": "Dog", "Origin": "Domestic" },
      {"Type": "Cat", "Origin": "Domestic" },
      {"Type": "Cat", "Origin": "Wild" }
    ]
    """
    |> String.toStream
    |> Stream.toJsonNode
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``Deserialize schemaless object`` () =
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
    |> String.toStream
    |> Stream.toJsonNode
    |> discriminator.WithSchemaless().Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``Deserialize schemaless array`` () =
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
    |> String.toStream
    |> Stream.toJsonNode
    |> discriminator.WithSchemaless().Discriminate
    |> Verifier.Verify
    |> _.ToTask()
