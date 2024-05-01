module Discriminalizer.Tests.CompositeDiscriminatorTest

open System.Text.Json.Nodes
open Discriminalizer
open Xunit

type MockedDiscriminator(discern: bool, discriminate: obj seq) =
    inherit Discriminator()
    override _.Discern _ = discern
    override _.Discriminate _ = discriminate

[<Fact>]
let ``discern returns true if one of discriminators can discern`` () =
    CompositeDiscriminator(MockedDiscriminator(false, null), MockedDiscriminator(true, null))
    |> fun d -> d.Discern null |> Assert.True

[<Fact>]
let ``discern returns true if none of discriminators can discern`` () =
    CompositeDiscriminator(MockedDiscriminator(false, null), MockedDiscriminator(false, null))
    |> fun d -> d.Discern null |> Assert.False

[<Fact>]
let ``discriminate returns empty if none of discriminators can discern`` () =
    CompositeDiscriminator(MockedDiscriminator(false, Seq.singleton 0), MockedDiscriminator(false, Seq.singleton 0))
    |> fun d -> d.Discriminate <| JsonValue.Create 0 |> fun o -> o = Seq.empty |> Assert.True

[<Fact>]
let ``discriminate returns result of first discriminator that can discern`` () =
    CompositeDiscriminator(MockedDiscriminator(true, Seq.singleton 1), MockedDiscriminator(true, Seq.singleton 2))
    |> fun d ->
        d.Discriminate <| JsonValue.Create 0
        |> fun r -> r |> Seq.head = 1 |> Assert.True
