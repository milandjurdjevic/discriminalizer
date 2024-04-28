namespace Discriminalizer

open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Nodes

type internal JsonValueExtensions() =

    [<Extension>]
    static member ToObject(this: JsonValue) : obj =
        match this.GetValueKind() with
        | JsonValueKind.String -> this.GetValue<string>()
        | JsonValueKind.Number -> this.ToString() |> _.ToNumber() |> Option.get
        | JsonValueKind.True -> true
        | JsonValueKind.False -> false
        | JsonValueKind.Undefined -> null
        | JsonValueKind.Null -> null
        | _ -> invalidOp $"Unknown value kind: {this.ToString()}."
