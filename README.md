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

- [Schemabased Discrimination](src/Discriminalizer/README.md#schemabased-type-discrimination): Discriminate data based on specific properties within the JSON object.
- [Schemaless Discrimination](src/Discriminalizer/README.md#schemaless-type-discrimination): Discriminate data without the need for specific properties within the JSON object.
- [Composite Discrimination](src/Discriminalizer/README.md#composite-type-discrimination): Combine multiple discriminator types into one.

## License

See [License](LICENSE) for more details.