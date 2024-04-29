module Discriminalizer.Tests.CompositeDiscriminatorTest

open System.Text.Json.Nodes
open Discriminalizer
open Xunit

[<Fact>]
let ``discern returns true if one of discriminators can discern`` () =
    CompositeDiscriminator(
        { new IDiscriminator with
            member this.Discriminate _ = null
            override this.Discern _ = false },
        { new IDiscriminator with
            member this.Discern _ = true
            override this.Discriminate _ = null }
    )
    :> IDiscriminator
    |> fun d -> d.Discern null |> Assert.True

[<Fact>]
let ``discern returns true if none of discriminators can discern`` () =
    CompositeDiscriminator(
        { new IDiscriminator with
            member this.Discriminate _ = null
            override this.Discern _ = false },
        { new IDiscriminator with
            member this.Discern _ = false
            override this.Discriminate _ = null }
    )
    :> IDiscriminator
    |> fun d -> d.Discern null |> Assert.False

[<Fact>]
let ``discriminate returns null if none of discriminators can discern`` () =
    CompositeDiscriminator(
        { new IDiscriminator with
            member this.Discriminate _ = 0
            override this.Discern _ = false },
        { new IDiscriminator with
            member this.Discern _ = false
            override this.Discriminate _ = 0 }
    )
    :> IDiscriminator
    |> fun d -> d.Discriminate <| JsonValue.Create 0 |> Assert.Null

[<Fact>]
let ``discriminate returns result of first discriminator that can discern`` () =
    CompositeDiscriminator(
        { new IDiscriminator with
            member this.Discriminate _ = 1
            override this.Discern _ = true },
        { new IDiscriminator with
            member this.Discern _ = true
            override this.Discriminate _ = 2 }
    )
    :> IDiscriminator
    |> fun d -> d.Discriminate <| JsonValue.Create 0 |> fun r -> r = 1 |> Assert.True
