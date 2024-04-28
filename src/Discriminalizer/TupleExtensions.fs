namespace Discriminalizer

open System.Runtime.CompilerServices

type internal TupleExtensions() =

    [<Extension>]
    static member ToOption(this: bool * 'a) =
        if fst this then Some(snd this) else None
