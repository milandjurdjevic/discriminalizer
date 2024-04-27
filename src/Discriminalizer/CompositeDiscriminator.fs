namespace Discriminalizer

type CompositeDiscriminator(discriminators: IDiscriminator seq) =

    interface IDiscriminator with
        member this.Discriminate node =
            discriminators
            |> Seq.tryFind (fun d -> d.Discern node)
            |> Option.map (fun d -> d.Discriminate node)
            |> Option.defaultValue null

        member this.Discern node =
            discriminators |> Seq.exists (fun d -> d.Discern node)
