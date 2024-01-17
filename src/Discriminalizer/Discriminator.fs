namespace Discriminalizer

open System
open System.Collections.Generic
open System.Collections.Immutable

type Discriminator private (paths: IReadOnlyCollection<string>, values: IReadOnlyDictionary<string, Type>) =

    new([<ParamArray>] paths: string array) = Discriminator(paths, ImmutableDictionary.Empty)

    member this.Paths = paths
    member this.Values = values

    member this.With<'T>([<ParamArray>] keys: string array) =
        let dictionary = Dictionary(values)
        dictionary.Add(keys |> String.concat String.Empty, typeof<'T>)
        Discriminator(paths, dictionary.ToImmutableDictionary())