namespace Discriminalizer

open System.Text.Json.Nodes

type SchemalessDiscriminator() =

    interface IDiscriminator with
        member this.Discriminate node =
            let rec parseNode (currentNode: JsonNode) =
                match currentNode with
                | :? JsonValue as value -> value.ToObject()
                | :? JsonObject as object ->
                    object |> Seq.map (fun n -> n.Key, parseNode n.Value) |> readOnlyDict |> box
                | :? JsonArray as array -> array |> Seq.map parseNode |> box
                | _ -> null

            parseNode node

        member this.Discern _ = true
