[<VerifyXunit.UsesVerify>]
module Discriminalizer.JsonTests

open System.IO
open System.Text
open System.Text.Json
open System.Threading
open VerifyXunit
open Xunit

[<AbstractClass>]
type Shape() =
    abstract member Sides: float
    abstract member Dimensions: float
    abstract member Name: string

type Circle() =
    inherit Shape()
    member val Radius = 0.0 with get, set
    override val Name = "Circle"
    override val Sides = 1
    override val Dimensions = 2

type Rectangle() =
    inherit Shape()
    member val Height = 0.0 with get, set
    member val Width = 0.0 with get, set
    override val Name = "Rectangle"
    override val Sides = 4
    override val Dimensions = 2

type Tetrahedron() =
    inherit Shape()
    member val Volume = 0.0 with get, set
    override val Name = "Tetrahedron"
    override val Sides = 4
    override val Dimensions = 3

[<Theory>]
// Deserialize single object that is defined in a scheme.
// lang=json
[<InlineData("""{"Name": "Circle", "Radius": 5.0 }""")>]
// Deserialize array that contains objects without scheme.
// lang=json
[<InlineData("""
[
  { 
    "Info": { "Name": "Circle", "Radius": 5.0 }
  },
  {
    "Data": [
      { "Name": "Name", "Value": "Circle" },
      { "Name": "Radius", "Value": 5.0 }
    ]
  }
]""")>]
// Deserialize array that contains objects with two schemes.
// lang=json
[<InlineData("""
[
  { "Name": "Circle", "Radius": 5.0 },
  { "Sides": 4, "Dimensions": 2, "Height": 10.0, "Width": 8.0 },
  { "Sides": 4, "Dimensions": 3, "Volume": 100 }
]
""")>]
// Deserialize array that contains a null object.
// lang=json
[<InlineData("""[{"Name": "Circle"}, null]""")>]
let ``The deserialized input matches the verified output`` (json: string) =
    async {

        let discriminator1 =
            Discriminator("Sides", "Dimensions")
                .With<Circle>("1", "2")
                .With<Rectangle>("4", "2")
                .With<Tetrahedron>("4", "3")

        let discriminator2 =
            Discriminator("Name")
                .With<Circle>("Circle")
                .With<Rectangle>("Rectangle")
                .With<Tetrahedron>("Tetrahedron")

        let discriminators = [| discriminator1; discriminator2 |]
        let bytes = Encoding.UTF8.GetBytes json
        let options = JsonSerializerOptions()
        use stream = new MemoryStream(bytes)

        let! output =
            Json.OfStream stream discriminators options CancellationToken.None
            |> Async.AwaitTask

        do!
            Verifier.Verify(output).UseParameters(json).HashParameters().ToTask()
            |> Async.AwaitTask
            |> Async.Ignore
    }
