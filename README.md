# Discriminalizer

.NET library designed for seamless JSON deserialization of object with complex discrimination requirements, built on top
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

Next thing is to define the classes that represent the animals.

```csharp
public class WildDog { }
public class DomesticDog { }
public class WildCat { }
public class DomesticCat { }
```

Now we can use the `Discriminator` class to configure the deserialization scheme. The `Discriminator` has a constructor
that takes the names of the properties that will be used to determine the type of the object.

The `With` method is used to register the types that will be used to deserialize the objects. It take an array of
property values, that will be used to determine if the JSON object represents the registered type.

```csharp
Discriminator discriminator = new Discriminator("Type", "Origin")
    .With<WildDog>("Dog", "Wild")
    .With<DomesticDog>("Dog", "Domestic")
    .With<WildCat>("Cat", "Wild")
    .With<DomesticCat>("Cat", "Domestic");
```

Finally, we can use the `discriminator` to deserialize the JSON, using `Json.OfStream` method.

```csharp
// Configure the deserialization options. You need to provide JsonSerializerOptions, a list of discriminators and
// a flag that indicates if "schemaless" objects should be deserialized as well. 
// Schemaless objects are objects without a coresponding class.
JsonOptions options = new(JsonSerializerOptions.Default, [discriminator], false);

// Variable "jsonStream" is a JSON string converted to Stream. For simplicity, it is excluded from this example.
IEnumerable<object> objects = await Json.OfStream(jsonStream, options, CancellationToken.None);

// You can use the ".OfType<T>()" extension method to filter the objects by type.
WildDog dog = objects.OfType<WildDog>().Single();
```

## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for more details.