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

## Usage

Let's take for example a list of animals, where each animal can be either a wild or domestic animal. The animals can be
of different types, such as dogs and cats. We want to deserialize this list into a collection of objects, where each
object represents a specific animal type.

First thing is to create the classes that represent the animal types.

```csharp
public class WildDog { }
public class DomesticDog { }
public class WildCat { }
public class DomesticCat { }
```

Then for a given JSON string, we need to use the `JsonSerialier` class to parse it into a `JsonNode` object.

```json
[
  {
    "Type": "Dog",
    "Origin": "Wild"
  },
  {
    "Type": "Dog",
    "Origin": "Domestic"
  },
  {
    "Type": "Cat",
    "Origin": "Wild"
  },
  {
    "Type": "Cat",
    "Origin": "Domestic"
  }
]
```

```csharp
JsonNode node = JsonSerializer.Deserialize<JsonNode>(json);
```

Finally we can use the `Discriminator` class to configure the scheme and deserialize a `JsonNode` object to a collection
of objects (with concrete types).

```csharp
IEnumerable<object> objects = new Discriminator(JsonSerializerOptions.Default, "Type", "Origin")
    .Add<WildDog>("Dog", "Wild")
    .Add<DomesticDog>("Dog", "Domestic")
    .Add<WildCat>("Cat", "Wild")
    .Add<DomesticCat>("Cat", "Domestic")
    .Discriminate(node);

// You can use the ".OfType<T>()" extension method to filter the objects by type.
WildDog dog = objects
    .OfType<WildDog>()
    .Single();
```

### Schemaless objects

If you want to deserialize objects that do not have a corresponding class, you need to enable it in the `JsonOptions`.
The schemaless objects will be deserialized as `IReadOnlyDictionary<string, object>`.

**IMPORTANT**: This feature uses recursion to travel through the nested JSON objects, so it can
cause `StackOverflowException` if an object is too deep. Unless you really need this feature and you are sure that there
are not too many levels of nested objects, it is best to keep it **disabled**.

## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for more details.
