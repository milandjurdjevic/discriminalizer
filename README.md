# Discriminalizer

.NET library designed for seamless JSON deserialization of complex object hierarchies, built on top of System.Text.Json.

## Motivation

Modern software development often involves dealing with diverse and complex data structures, particularly when working
with APIs or external data sources. JSON deserialization is a critical aspect of such scenarios, and traditional
approaches can become cumbersome, especially when handling polymorphic structures.
This project was born out of the need for a streamlined and flexible solution for
JSON deserialization within the .NET ecosystem. We recognized the challenges developers face when working with
polymorphic object hierarchies and aimed to simplify this process.
By leveraging the discriminator configuration approach and integrating with System.Text.Json, this library empowers
developers to handle intricate JSON structures effortlessly. Whether you are working on a web application, API
integration, or any project requiring efficient object deserialization, the library provides a
robust toolset to enhance your development experience.
We believe in fostering collaboration and welcome contributions from the community to make this library even more
versatile and effective. Join us in simplifying JSON deserialization in .NET, and let's build better software together!

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

Finally, we can use the `Discriminator` to deserialize the JSON.

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