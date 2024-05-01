[<VerifyXunit.UsesVerify>]
module Discriminalizer.Tests.SchemalessDiscriminatorTest

open System
open System.Text.Json.Nodes
open Discriminalizer
open VerifyXunit
open Xunit

let verify object =
    let discriminator: Discriminator = SchemalessDiscriminator()
    discriminator.Discriminate object |> Verifier.Verify |> _.ToTask()

[<Fact>]
let ``discriminate object`` () =
    JsonNode.Parse
        """
        {
            "name": "John",
            "age": 40,
            "employed": true,
            "contact": {
                "email": "john@mail.com",
                "phone": "12345"
            },
            "transactions": [100, -50.5]
        }
        """
    |> verify

[<Fact>]
let ``discriminate array`` () =
    JsonNode.Parse
        """
        [
            {
                "name": "John1",
                "age": 40,
                "employed": true,
                "contact": {
                    "email": "john1@mail.com",
                    "phone": "12345"
                },
                "transactions": [100, -50.5]
            },
            {
                "name": "John2",
                "age": 30,
                "employed": true,
                "contact": {
                    "email": "john2@mail.com",
                    "phone": "54321"
                },
                "transactions": [100, -35.5]
            }
        ]
        """
    |> verify

[<Fact>]
let ``discriminate byte max`` () =
    verify <| JsonValue.Create(Byte.MaxValue)

[<Fact>]
let ``discriminate byte min`` () =
    verify <| JsonValue.Create(Byte.MinValue)

[<Fact>]
let ``discriminate int16 max`` () =
    verify <| JsonValue.Create(Int16.MaxValue)

[<Fact>]
let ``discriminate int16 min`` () =
    verify <| JsonValue.Create(Int16.MinValue)

[<Fact>]
let ``discriminate int32 max`` () =
    verify <| JsonValue.Create(Int32.MaxValue)

[<Fact>]
let ``discriminate int32 min`` () =
    verify <| JsonValue.Create(Int32.MinValue)

[<Fact>]
let ``discriminate int64 max`` () =
    verify <| JsonValue.Create(Int64.MaxValue)

[<Fact>]
let ``discriminate int64 min`` () =
    verify <| JsonValue.Create(Int64.MinValue)

[<Fact>]
let ``discriminate float max`` () =
    verify <| JsonValue.Create(Single.MaxValue)

[<Fact>]
let ``discriminate float min`` () =
    verify <| JsonValue.Create(Single.MinValue)

[<Fact>]
let ``discriminate double max`` () =
    verify <| JsonValue.Create(Double.MaxValue)

[<Fact>]
let ``discriminate double min`` () =
    verify <| JsonValue.Create(Double.MinValue)

[<Fact(Skip = "Decimals are not properly supported")>]
let ``discriminate decimal max`` () =
    verify <| JsonValue.Create(Decimal.MaxValue)

[<Fact(Skip = "Decimals are not properly supported")>]
let ``discriminate decimal min`` () =
    verify <| JsonValue.Create(Decimal.MinValue)
