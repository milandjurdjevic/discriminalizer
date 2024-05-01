[<VerifyXunit.UsesVerify>]
module Discriminalizer.SchemabasedDiscriminatorTest

open System.Text.Json
open System.Text.Json.Nodes
open VerifyXunit
open Xunit

[<AbstractClass>]
type BaseClass() =
    member this.ClrType: string = this.GetType().Name

type SubClass1() =
    inherit BaseClass()

type SubClass2() =
    inherit SubClass1()

let discriminator: Discriminator =
    SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop1", "Prop2")
        .WithType<SubClass1>("Class", "1")
        .WithType<SubClass2>("Class", "2")

[<Fact>]
let ``discriminate object`` () =
    JsonNode.Parse """{"Prop1": "Class", "Prop2": 1 }"""
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()

[<Fact>]
let ``discriminate array`` () =
    JsonNode.Parse """[{"Prop1": "Class", "Prop2": 1 }, { "Prop1": "Class", "Prop2": 2 }]"""
    |> discriminator.Discriminate
    |> Verifier.Verify
    |> _.ToTask()
