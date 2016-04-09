# Introduction #

Autofac's [collection registrations](Collections.md) provide an opt-in way to support 'resolve many'.

An alternative model is implemented as an extension: `Autofac.Modules.ImplicitCollectionSupportModule`.

This augments the built-in collection support with a 'default' ability to resolve more than one instance of a service:

```
var builder = new ContainerBuilder();

builder.RegisterModule(new ImplicitCollectionSupportModule());

builder.Register("hello").As<string>();
builder.Register("world").As<string>();

var container = builder.Build();

// Resolving IEnumerable<T> retrieves all components that provide service T
var strings = container.Resolve<IEnumerable<string>>();

foreach (var s in strings)
  Console.WriteLine(s);
```

The example above prints:

```
hello
world
```