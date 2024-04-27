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

let discriminator: IDiscriminator =
    let fields = [| "Prop1"; "Prop2" |]

    let values = Map([ ("Class1", typeof<SubClass1>); ("Class2", typeof<SubClass2>) ])

    SchemabasedDiscriminator(fields, values, JsonSerializerOptions.Default)


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
