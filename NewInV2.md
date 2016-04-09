# Introduction #

Autofac 2.x introduces some significant changes with the aim of simplifying and streamlining Autofac usage. This page highlights the most important changes for Autofac 1.x users.

For an overview of the new features for new users, see http://nblumhardt.com/2010/04/introducing-autofac-2-1-rtw/ and http://nblumhardt.com/2010/05/autofac-2-2-released/.

# Namespace Changes #

Most namespaces have been updated to fit a scheme that should keep things more organised and easier to find.

The biggest change is that the `Autofac.Builder` namespace has been moved into `Autofac`, and an additional namespace `Autofac.Core` created to hold types that are less-frequently used.

Most applications can now be built using the `Autofac` namespace only. New users will find this namespace easier to explore, as only the types relating to the most common scenarios are there.

# Builder Syntax Changes #

## Separation of Registration Overloads ##

Whereas previously the type/delegate/instance registration methods were all called `Register()`, these have now been given distinct names. Only delegates can be registered with the `Register()` method, other registration types are more explicit:

```
builder.Register(c => new Plane());
builder.Register<Car>();
builder.Register(new Boat());
```

Becomes:

```
builder.Register(c => new Plane()); // Unchanged
builder.RegisterType<Car>();
builder.RegisterInstance(new Boat());
```

This change makes the available functionality easier to find in IntelliSense(TM), as well as bringing consistency with other methods like `RegisterGeneric()`, `RegisterModule()` etc.

## Non-Shared ('Factory') Components by Default ##

The default lifetime in Autofac 1 is 'singleton'. In Autofac 2, components default to 'factory'.

## New Lifetime/Sharing Terminology ##

The names for lifetime/sharing configuration have been changed to reflect changes in the underlying architecture, and to remove common sources of confusion.

| **Old Autofac 1 Name** | **New Autofac 2 Name** | **Notes** |
|:-----------------------|:-----------------------|:----------|
| `SingletonScoped`      | `SingleInstance`       | Removes confusion with the Singleton pattern |
| `FactoryScoped`        | `InstancePerDependency` | Removes confusion with the Factory pattern and other features |
| `ContainerScoped`      | `InstancePerLifetimeScope` |           |
| `InContext`            | `InstancePerMatchingLifetimeScope` |           |

## Strongly-typed Activation Events ##

The `OnActivating` configuration method now has a strongly-typed parameter:

```
builder.RegisterType<Connection>().OnActivating(c => c.Open());
```

## Adding to an Existing Container ##

In Autofac 1.x registraitons can be added to an existing container with `ContainerBuilder.Build(IContainer)`.

In Autofac 2, registrations are added when creating a new lifetime scope:

```
var container = ...
using (var lifetime = container.BeginLifetimeScope(builder =>
{
   builder.RegisterType<X>();
   builder.RegisterType<Y>();
}))
{
   lifetime.Resolve<X>() //....
}
```

For changes to an existing container, use  `ContainerBuilder.Update()`:

```
var container = // something already built

var updater = new ContainerBuilder();
updater.RegisterType<A>();
updater.Register(c => new B()).As<IB>();

// Add the registrations to the container
updater.Update(container);
```

Configuration this way is often much harder to follow than a simple "register then build" style, so again, use this feature only where necessary.

## Other Syntax Changes ##

  * `DefaultOnly` has been renamed `PreserveDefaults`
  * `OnlyIf` has been removed; the `RegisterCallback` method now provides this ability

# Lifetime and Scope #

Autofac 2 is more explicit about the role of nesting in lifetime.

Instead of creating nested containers with `CreateInnerContainer()`, as in:

```
using (var requestContainer = container.CreateInnerContainer())
{
}
```

An `ILifetimeScope` object is now created using `BeginLifetimeScope()`:

```
using (var requestScope = container.BeginLifetimeScope())
{
}
```

# Modules Included by Default #

## Collection Support ("Resolve All") ##

All instances supporting a particular service can be retrieved by resolving `IEnumerable<T>` as in:

```
builder.RegisterType<A1>().As<IA>();
builder.RegisterType<A2>().As<IA>();

var container = builder.Build();

// Contains an instance of both A1 and A2
Assert.AreEqual(2, container.Resolve<IEnumerable<IA>>().Count());
```

Types can use `IEnumerable<T>` to declare a dependency, and Autofac 2 will automatically provide all instances as above:

```
class UsesA
{
  public UsesA(IEnumerable<IA> allAs) { }
}
```

## Auto-Generated Factories ##

When one component needs to dynamically create instances of another, it can take a dependency on a delegate that returns the type of component it requires:

```
class UsesServer
{
  Func<IServerConnection> _connectionFactory;

  public UsesServer(Func<IServerConnection> connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public void OnNewWork()
  {
    IServerConnection connection = _connectionFactory();
    // ...
  }
}
```

No explicit configuration is required to use this feature:

```
builder.RegisterType<FtpConnection>().As<IServerConnection>();
builder.RegisterType<UsesServer>();
```

By registering `IServerConnection`, Autofac 2 will automatically inject delegates that return type `IServerConnection`.

# Owned Instances #

Autofac 2 introduces the `Owned<T>` type, and automatically resolves dependencies of this type for any available services.

A component that needs to dynamically create instances of another component can accept a `Func<Owned<T>>` to gain this functionality.

A component that gains references to `Owned<T>` is responsible for calling `Owned<T>.Dispose` when the instance is no longer required. It is a bug to not clean up owned instances.

Under the hood, an `Owned<T>` is allocated its own nested lifetime scope, so all of its dependencies will be cleaned up when it is.

# Container Instance Ownership Changes #

In Autofac 1, weak references are held by the container. This makes sense if the objects being referenced use disposal to release GC/finalizer resources, but if the dispose method contains application logic then GC timing could introduce unexpected behaviour.

Autofac 2 holds normal references. To opt out of this behaviour and mange disposal manually, use the `ExternallyOwned` registration modifier.

# Convention-Based (Data-Driven) Registration #

The `RegisterAssemblyTypes` method provides syntax for declaratively specifying how a set of types is registered:

```
Assembly controllers = // ...

builder.RegisterAssemblyTypes(controllers)
  .Where(t => t.IsAssignableTo(typeof(IController))
  .Named(t => "controller-" + t.Name.ToLower());
```

Other syntax elements for lifetime, ownership etc. are available through the same methods as for regular types.

An overloaded version of `As` allows types to be mapped to the services they provide, and the `AsImplementedInterfaces` method supports the common convention of having components provide all interfaces they implement.

# Changes to Named Services #

Named services now require type information as well, and this must match in both the registration and resolution calls:

```
builder.RegisterType<Foo>().Named<IFoo>("foo");
```

matches:

```
container.Resolve<IFoo>("foo");
```

but not:

```
container.Resolve<IBar>("foo");
```

This means that names can now be used with open generic registrations, among other things.

# XML Configuration Changes #

## Separate Assembly for XML Configuration ##

`ConfigurationSettingsReader` and related XML configuration support has been moved to `Autofac.Configuration.dll`

This will allow the XML configuration support to be significantly extended without bloating the core assembly.

It is also a goal of Autofac 2 for the same core assembly (Autofac.dll) to run under both .NET and Silverlight. Moving the `System.Configuration`-dependent classes, which are not supported on Silverlight, out of the core assembly enables this.

## Syntax Changes ##

**Element Names Changed**

| **Autofac 1.x Name** | **Autofac 2 Name** |
|:---------------------|:-------------------|
| `scope`              | `instance-scope`   |
| `ownership`          | `instance-ownership` |
| `extendedProperties` | `metadata`         |
| `extendedProperty`   | `item`             |

**Instance Scope Value Changes**

| **Autofac 1.x Name** | **Autofac 2 Name** |
|:---------------------|:-------------------|
| `factory`            | `per-dependency`   |
| `singleton`          | `single-instance`  |
| `container`          | `per-lifetime-scope` |

**Instance Ownership Value Changes**

| **Autofac 1.x Name** | **Autofac 2 Name** |
|:---------------------|:-------------------|
| `scope`              | `lifetime-scope`   |

**Property Injection Value Changes**

Rather than 'none', 'all' and 'unset' the values are now 'yes' or 'no'.

# ASP.NET MVC Integration Changes #

Autofac 1.4 and earlier 2.1 versions used `AutofacControllerModule` as a way of finding and registering ASP.NET MVC controllers with the container.

In the Autofac 2.1 MVC integration, there is a new `ContainerBuilder` extension method called `RegisterControllers`.

```
var builder = new ContainerBuilder();

// Was:
// builder.RegisterModule(new AutofacControllerModule(...))
builder.RegisterControllers(Assembly.GetExecutingAssembly());

_containerProvider = new ContainerProvider(builder.Build());
ControllerBuilder.Current.SetControllerFactory(
                new AutofacControllerFactory(_containerProvider));
```

The reason for the switch is that `RegisterControllers` is a thin wrapper around the new assembly scanner, and supports the same syntax as the rest of Autofac’s registration methods:

```
builder.RegisterControllers(assembly)
  .InjectProperties()
  .OnActivated(e => Log.Information("Controller created: " + e.Instance));
```