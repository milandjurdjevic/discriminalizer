namespace Discriminalizer

open System.Diagnostics.CodeAnalysis
open System.Text.Json.Nodes
open Microsoft.FSharp.Core

type IDiscriminator =

    [<return: MaybeNull>]
    abstract member Discriminate: JsonNode -> obj

    abstract member Discern: JsonNode -> bool
