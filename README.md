# Discriminalizer [![latest version](https://img.shields.io/nuget/v/discriminalizer)](https://www.nuget.org/packages/discriminalizer)

.NET library designed for seamless JSON deserialization of objects with complex discrimination requirements, built on
top
of System.Text.Json.

## Motivation

In the realm of JSON deserialization, accommodating objects with intricate discrimination requirements can be
challenging. While System.Text.Json provides robust support for basic deserialization tasks, handling complex
discrimination logic often involves writing cumbersome and error-prone custom converters.

The library simplifies this process by offering a straightforward solution for deserializing JSON data into objects with
complex discrimination needs. By extending System.Text.Json's functionality, we provide developers with a seamless way
to deserialize JSON, even when dealing with intricate discrimination requirements.

With this library, developers can efficiently map JSON data to their object models without the need for extensive custom
converter implementations. This simplifies the deserialization process, resulting in cleaner and more maintainable code.

## Features

### Schemabased Type Discrimination

**The Schemabased Type Discrimination** feature allows JSON deserialization of objects based on specific properties
within the JSON data.

JSON Example:

```json
[
  { "Prop1": "Class", "Prop2": 1 },
  { "Prop1": "Class", "Prop2": 2 }
]
```

C# Example:

```csharp
IEnumerable<FirstClass> discriminated = new SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop1", "Prop2")
    .WithType<FirstClass>("Class", "1")
    .WithType<SecondClass>("Class", "2")
    .Discriminate(json)
    .OfType<FirstClass>();
```

F# Example:

```fsharp
let discriminated: FirstClass seq =
    SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop1", "Prop2")
        .WithType<FirstClass>("Class", "1")
        .WithType<SecondClass>("Class", "2")
        .Discriminate(jsonNode)
    |> Seq.choose (fun obj -> match obj with | :? FirstClass as fcls -> Some fcls | _ -> None)
```

In this example, the JSON data `{"Prop1": "Class", "Prop2": 1 }` would be deserialized into an instance of SubClass1
because "Prop1" is "Class" and "Prop2" is 1. If "Prop2" was 2, it would be deserialized into an instance of SubClass2.

### Schemaless Type Discrimination

**The Schemaless Discrimination** feature allows JSON deserialization of objects without the need for specific
properties within the
JSON data. This feature is particularly useful when dealing with JSON data where the type of the object to be
deserialized is not determined by the values of certain properties in the JSON data.

JSON Example:

```json
[
  { "Prop1": "Class" },
  { "Prop1": 50 },
  "Hello World"
]
```

C# Example

```csharp
IEnumerable<object> discriminated = new SchemalessDiscriminator().Discriminate(jsonNode);
```

F# Example

```fsharp
let discriminated: obj seq = SchemalessDiscriminator().Discriminate(jsonNode)
```

In this example, the given JSON input will be converted to a collection of two dictionaries and one string value,
casted as objects.

### Composite Type Discrimination

**The Composite Type Discrimination** feature allows for JSON deserialization of objects using a combination of both
Schemabased and Schemaless Discrimination. You can also combine multiple Schemabased discriminators.

JSON Example:

```json
[
  { "Prop1": "Class" },
  { "Prop3": 50 },
  { "Prop6": "Class" }
]
```

C# Example:

```csharp
Discriminator discriminator1 = new SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop1")
    .WithType<FirstClass>("Class");

Discriminator discriminator2 = new SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop3")
    .WithType<FirstClass>("50");

Discriminator discriminator3 = new SchemalessDiscriminator();

Discriminator discriminator = new CompositeDiscriminator(discriminator1, discriminator2, discriminator3);

IEnumerable<FirstClass> discriminated = discriminator
    .Discriminate(jsonNode)
    .OfType<FirstClass>();
```

F# Example:

```fsharp
let discriminator1 = SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop1")
    .WithType<FirstClass>("Class")
    
let discriminator2 = SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop3")
    .WithType<FirstClass>("50")
    
let discriminator3 = SchemalessDiscriminator()

let discriminator = CompositeDiscriminator(discriminator1, discriminator2, discriminator3)

let discriminated: FirstClass seq =
    discriminator
    .Discriminate(jsonNode)
    |> Seq.choose (fun obj -> match obj with | :? FirstClass as fcls -> Some fcls | _ -> None)
```

In this example, the given JSON input will be discriminated as a collection of two objects with `FirstClass` type and
one dictionary.

## License

See [License](LICENSE) for more details.