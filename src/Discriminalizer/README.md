# Discriminalizer

Deserialize JSON data into objects based on discriminator settings.

## Features

### Schemabased Type Discrimination

**The Schemabased Type Discrimination** feature allows JSON deserialization of objects based on specific properties
within the JSON data.

JSON Example

```json
[
  { "Prop1": "Class", "Prop2": 1 },
  { "Prop1": "Class", "Prop2": 2 }
]
```

C# Example

```csharp
IEnumerable<FirstClass> discriminated = new SchemabasedDiscriminator(JsonSerializerOptions.Default, "Prop1", "Prop2")
    .WithType<FirstClass>("Class", "1")
    .WithType<SecondClass>("Class", "2")
    .Discriminate(json)
    .OfType<FirstClass>();
```

In this example, the JSON data `{"Prop1": "Class", "Prop2": 1 }` would be deserialized into an instance of SubClass1
because "Prop1" is "Class" and "Prop2" is 1. If "Prop2" was 2, it would be deserialized into an instance of SubClass2.

### Schemaless Type Discrimination

**The Schemaless Discrimination** feature allows JSON deserialization of objects without the need for specific
properties within the
JSON data. This feature is particularly useful when dealing with JSON data where the type of the object to be
deserialized is not determined by the values of certain properties in the JSON data.

JSON Example

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

In this example, the given JSON input will be discriminated as a collection of two objects with `FirstClass` type and
one dictionary.