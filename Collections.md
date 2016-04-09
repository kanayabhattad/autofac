# This Page is Obsolete #

_Since version 2 of Autofac, `IEnumerable<T>` is always resolvable and will include all registered components for service `T`. The `RegisterCollection()` methods are kept for compatibility reasons but will be removed in a future release._

# Introduction #

To register multiple implementations of the same service, use `RegisterCollection<T>()` - if you want 5 loggers, it is recommended that you create a collection and add the loggers to it. This prevents an ugly situation when all of a sudden you need two distinct collections of loggers in the same application.

Autofac (as of 1.4) now also provides an 'automatic' way to resolve multiple instances of the same service - see [ImplicitCollectionSupport](ImplicitCollectionSupport.md).

# Example #

```
var builder = new ContainerBuilder();

builder.RegisterCollection<ILogger>()
  .As<IEnumerable<ILogger>>();

builder.Register<ConsoleLogger>()
  .As<ILogger>()
  .MemberOf<IEnumerable<ILogger>>();

builder.Register<EmailLogger>()
  .As<ILogger>()
  .MemberOf<IEnumerable<ILogger>>();

var container = builder.Build();

var loggers = container.Resolve<IEnumerable<ILogger>>();
foreach (var logger in loggers)
  logger.Info("Created my first collection registration!");
```

Collections are just regular components, so they can be used in autowiring just like anything else.

The `IEnumerable<T>`, `ICollection<T>` and `IList<T>` interfaces are supported.