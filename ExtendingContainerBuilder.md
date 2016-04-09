# Introduction #

There are many, many ways to register components with the container. This example demonstrates how to extend the container so that components can be marked with attributes and registered an assembly-at-a-time. The eventual syntax we will enable is:

```
builder.RegisterAssembly(Assembly.Load(...));
```

_Note, this example code has never been compiled, let alone tested. If you really want this functionality, pop a message onto the discussion group first, we can lend a hand and consider posting the results for others to use._

# Creating the Attribute #

This simplistic example is going to use a single attribute to mark a class as being a component, along with a single service it provides.

```
[AttributeUsage(AttributeTargets.Class)]
public class ComponentAttribute : Attribute
{
  public Type ServiceType { get; private set; }

  public ComponentAttribute(Type serviceType)
  {
    ServiceType = serviceType;
  }
}
```

We can now mark our components like this:

```
[Component(typeof(INotifier))]
public class MessageBoxNotifier : INotifier { ... }
```

# Adding a Module #

The module will register all of the types from an assembly with a container, based on the custom attribute:

```
public class AssemblyModule : IModule
{
  Assembly _assembly;

  public AssemblyModule(Assembly assembly)
  {
    _assembly = assembly;
  }

  public void Configure(Container container)
  {
    var builder = new ContainerBuilder();
    foreach (var component in _assembly.GetTypes())
    {
      var attr = component
                   .GetCustomAttributes(typeof(ComponentAttribute), false)
                   .OfType<ComponentAttribute>()
                   .FirstOrDefault();
      if (attr != null)
        builder.Register(component).As(attr.ServiceType);
    }
    builder.Build(container);
  }
}
```

Note that we use the builder to make registrations on a container that was passed to us.

# Extending the Builder Syntax #

This is the easy part:

```
public static class AssemblyBuilderExtension
{
  public static void RegisterAssembly(this ContainerBuilder builder, Assembly assembly)
  {
    builder.Register(new AssemblyModule(assembly));
  }
}
```

And we're done! Syntactic sugar and all.

# More Advanced Extensions #

Our extension method above returned `void`. Most of the provided `Register()` methods return a derivative of the `IRegistrar` interface, to add the familiar syntax including `As()`, `WithScope()` and `Named()`.

This is implemented on the types deriving from `Autofac.Builder.Registrar` - while this type is not exposed by Autofac, you should be able to get some guidance from its implementation when adding more advanced extensions of your own. At some point, if there is enough interest or need, Autofac may expose an equivalent of `Registrar` to simplify this task.