[<VerifyXunit.UsesVerify>]
module Discriminalizer.JsonTests

open System.Text.Json
open Discriminalizer.Tests
open VerifyXunit
open Xunit

[<Fact>]
let ``Deserialize a single object`` () =
    async {
        // lang=json
        let json = """{"Type": "Dog", "Origin": "Domestic" }"""
        let! output = Fixture.deserialize Fixture.defaultOptions json
        do! Verifier.Verify(output).ToTask() |> Async.AwaitTask |> Async.Ignore
    }

[<Fact>]
let ``Deserialize array that contains null`` () =
    async {
        // lang=json
        let json = """[{"Type": "Dog"}, null]"""
        let! output = Fixture.deserialize Fixture.defaultOptions json
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

        let! output = Fixture.deserialize Fixture.defaultOptions json
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
              Discriminators = Fixture.discriminators
              IncludeSchemaless = includeSchemaless }

        let! output = Fixture.deserialize options json

        do!
            Verifier.Verify(output).UseParameters(includeSchemaless).ToTask()
            |> Async.AwaitTask
            |> Async.Ignore
    }
