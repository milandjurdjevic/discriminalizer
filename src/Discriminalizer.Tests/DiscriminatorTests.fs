[<VerifyXunit.UsesVerify>]
module Discriminalizer.DiscriminatorTests

open Discriminalizer.Tests
open VerifyXunit
open Xunit

[<Fact>]
let ``Deserialize object`` () =
    // lang=json
    """{"Type": "Dog", "Origin": "Domestic" }"""
    |> String.toStream
    |> Stream.toJsonNode
    |> Scheme.discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()


[<Fact>]
let ``Deserialize array with null`` () =
    // lang=json
    """[{"Type": "Dog", "Origin": "Domestic"}, null]"""
    |> String.toStream
    |> Stream.toJsonNode
    |> Scheme.discriminator.Discriminate
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
    |> Scheme.discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Theory>]
[<InlineData true>]
[<InlineData false>]
let ``Deserialize schemaless object`` (includeSchemaless: bool) =
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
    |> Scheme.discriminator.AsSchemaless().Discriminate
    |> Verifier.Verify
    |> _.UseParameters(includeSchemaless)
    |> _.HashParameters()
    |> _.ToTask()

[<Theory>]
[<InlineData true>]
[<InlineData false>]
let ``Deserialize array with schemaless objects`` (includeSchemaless: bool) =
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
    |> Scheme.discriminator.AsSchemaless().Discriminate
    |> Verifier.Verify
    |> _.UseParameters(includeSchemaless)
    |> _.HashParameters()
    |> _.ToTask()
