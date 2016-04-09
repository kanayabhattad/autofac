# Introduction #

Autofac is designed from the ground up to be used in multi-threaded applications.

This page summarises the approach to thread safety for both users and the Autofac maintainers.

# Thread-Safe Types #

The following types are safe for concurrent access by multiple threads:

  * `Container`
  * `ComponentRegistry`
  * `Disposer` (default implementation of `IDisposer`)
  * `LifetimeScope` (default implementation of `ILifetimeScope`)

These types cover practically all of the runtime/resolution scenarios.

The following types are designed for single-threaded access at configuration time:

  * `ContainerBuilder`

So, a correct Autofac application will use a `ContainerBuilder` on a single thread to create the container at startup. Subsequent use of the container can occur on any thread.

## Important ##

The temporary `IComponentContext` parameter `c` passed to registration delegates is not thread safe. **The following code is broken**:

```
    builder.Register(c => new Foo(c))
```

The correct version of the above is:

```
    builder.Register(c => new Foo(c.Resolve<IComponentContext>()))
```

# Deadlock Avoidance #

Autofac is designed in such a way that deadlocks won't occur in normal use. This section is a guide for maintainers or extension writers.

## Lock Acquisition Order ##

Locks may be acquired in the following order:

  * A thread holding a lock for any of the following may not acquire any further locks:
    * `ComponentRegistry`
    * `Disposer`
  * A thread holding the lock for a `LifetimeScope` may subsequently acquire the lock for:
    * Its parent `LifetimeScope`
    * Any of the items listed above

## Rules of Thumb ##

  * Don't call back into the container in handlers for the `Preparing`, `Activating` or `Activated` events: use the supplied `IComponentContext` instead