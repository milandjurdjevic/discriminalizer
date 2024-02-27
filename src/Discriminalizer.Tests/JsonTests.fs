[<VerifyXunit.UsesVerify>]
module Discriminalizer.JsonTests

open Discriminalizer.Tests
open VerifyXunit
open Xunit

[<Fact>]
let ``Deserialize single object`` () =
    // lang=json
    """{"Type": "Dog", "Origin": "Domestic" }"""
    |> String.toStream
    |> Stream.toJsonNode
    |> JsonNode.deserialize Scheme.options
    |> Verifier.Verify


[<Fact>]
let ``Deserialize array that contains null`` () =
    // lang=json
    """[{"Type": "Dog"}, null]"""
    |> String.toStream
    |> Stream.toJsonNode
    |> JsonNode.deserialize Scheme.options
    |> Verifier.Verify

[<Fact>]
let ``Deserialize array with two discriminator schemes`` () =
    // lang=json
    """
    [
      {"Type": "Dog", "Origin": "Domestic" },
      {"Type": "Cat", "Origin": "Domestic" },
      {"Type": "Cat", "Origin": "Wild" },
      {"Type": "Cat" },
      {"Type": "Dog" }
    ]
    """
    |> String.toStream
    |> Stream.toJsonNode
    |> JsonNode.deserialize Scheme.options
    |> Verifier.Verify

[<Theory>]
[<InlineData true>]
[<InlineData false>]
let ``Deserialize single schemaless object`` (includeSchemaless: bool) =
    //lang=json
    """
      {
        "Animals": [
          { "Type": "Fish", "Origin": "Wild" },
          { "Type": "Fish", "Origin": "Domestic" }
        ]
      }
    """
    |> String.toStream
    |> Stream.toJsonNode
    |> JsonNode.deserialize (Scheme.options.WithSchemaless())
    |> Verifier.Verify
    |> _.UseParameters(includeSchemaless)
    |> _.HashParameters()

[<Theory>]
[<InlineData true>]
[<InlineData false>]
let ``Deserialize array with some schemaless objects`` (includeSchemaless: bool) =
    //lang=json
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
    |> String.toStream
    |> Stream.toJsonNode
    |> JsonNode.deserialize (Scheme.options.WithSchemaless())
    |> Verifier.Verify
    |> _.UseParameters(includeSchemaless)
    |> _.HashParameters()
