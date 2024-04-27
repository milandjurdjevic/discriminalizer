[<VerifyXunit.UsesVerify>]
module Discriminalizer.Tests.SchemalessTest

open System
open System.Collections.Generic
open System.Text.Json.Nodes
open Discriminalizer
open VerifyXunit
open Xunit

let object =
    JsonObject(
        [ KeyValuePair.Create("name", JsonValue.Create("John") :> JsonNode)
          KeyValuePair.Create("age", JsonValue.Create(40) :> JsonNode)
          KeyValuePair.Create("employed", JsonValue.Create(true) :> JsonNode)
          KeyValuePair.Create(
              "contact",
              JsonObject(
                  [ KeyValuePair.Create("email", JsonValue.Create("john@mail.com") :> JsonNode)
                    KeyValuePair.Create("phone", JsonValue.Create("12345") :> JsonNode) ]
              )
              :> JsonNode
          )
          KeyValuePair.Create("transactions", JsonArray(JsonValue.Create(100), JsonValue.Create(-50.5)) :> JsonNode) ]
    )
    :> JsonNode

[<Fact>]
let ``deserialize object as schemaless`` () =
    Schemaless.ofNode object |> Verifier.Verify |> _.ToTask()

[<Fact>]
let ``deserialize array as schemaless`` () =
    JsonArray(object) |> Schemaless.ofNode |> Verifier.Verify |> _.ToTask()

[<Fact>]
let ``deserialize byte max as schemaless`` () =
    JsonValue.Create(Byte.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Byte.MaxValue)
    |> Assert.True

[<Fact>]
let ``deserialize byte min as schemaless`` () =
    JsonValue.Create(Byte.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Byte.MinValue)
    |> Assert.True

[<Fact>]
let ``deserialize int16 max as schemaless`` () =
    JsonValue.Create(Int16.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Int16.MaxValue)
    |> Assert.True

[<Fact>]
let ``deserialize int16 min as schemaless`` () =
    JsonValue.Create(Int16.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Int16.MinValue)
    |> Assert.True

[<Fact>]
let ``deserialize int32 max as schemaless`` () =
    JsonValue.Create(Int32.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Int32.MaxValue)
    |> Assert.True

[<Fact>]
let ``deserialize int32 min as schemaless`` () =
    JsonValue.Create(Int32.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Int32.MinValue)
    |> Assert.True

[<Fact>]
let ``deserialize int64 max as schemaless`` () =
    JsonValue.Create(Int64.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Int64.MaxValue)
    |> Assert.True

[<Fact>]
let ``deserialize int64 min as schemaless`` () =
    JsonValue.Create(Int64.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Int64.MinValue)
    |> Assert.True

[<Fact>]
let ``deserialize float max as schemaless`` () =
    JsonValue.Create(Single.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Single.MaxValue)
    |> Assert.True

[<Fact>]
let ``deserialize float min as schemaless`` () =
    JsonValue.Create(Single.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Single.MinValue)
    |> Assert.True

[<Fact>]
let ``deserialize double max as schemaless`` () =
    JsonValue.Create(Double.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Double.MaxValue)
    |> Assert.True

[<Fact>]
let ``deserialize double min as schemaless`` () =
    JsonValue.Create(Double.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Double.MinValue)
    |> Assert.True

[<Fact(Skip = "Decimals are not yet supported")>]
let ``deserialize decimal max as schemaless`` () =
    JsonValue.Create(Decimal.MaxValue)
    |> Schemaless.ofNode
    |> _.Equals(Decimal.MaxValue)
    |> Assert.True

[<Fact(Skip = "Decimals are not yet supported")>]
let ``deserialize decimal min as schemaless`` () =
    JsonValue.Create(Decimal.MinValue)
    |> Schemaless.ofNode
    |> _.Equals(Decimal.MinValue)
    |> Assert.True
