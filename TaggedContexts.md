# This article seems to be more confusing now than helpful. A new article on lifetime is in order for Autofac 2. It's updated anyway. #

# Introduction #

Tagged scopes are useful in applications managing components in a hierarchy of contexts with different lifetimes - e.g. Global/Request/Transaction in a web application.

It is usually only necessary to tag components whose scope matches one of the levels of the hierarchy other than the root or a leaf (e.g. Request in the example above.) This is because the root lifetime will automatically house singletons, and the leaves will by default house InstancePerLifetimeScope() or InstancePerDependency() component instances.

# Tagging Components and Lifetimes #

A tag can be of any type - strings or enumerations are the most common options:

```
builder.Register(c => new HomeController()).InMatchingLifetimeScope("request");

var container = builder.Build();
container.Tag = "application";
```

In this example `"application"` is the tag applied to the root lifetime in which singleton instances will reside, and `"request"` is the tag applied to the lifetime servicing HTTP requests:

```
// A 'request lifetime' is created for every web request
// and disposed of when the request processing completes
var requestLifetime = container.BeginLifetimeScope();
requestLifetime.Tag = "request";
```

# Resolving Instances #

## In a lifetime without a matching tag or matching parent... ##

Each component registered for a tagged lifetime will be resolved only in that lifetime - so its child lifetimes will access instances attached to the parent. Attempting to resolve `HomeController` in `container` without creating a tagged lifetime will fail.

## In a container with a matching tag... ##

Each `requestLifetime` will have a single unique instance of `HomeController` because of the `InstancePerMatchingLifetimeScope()` declaration. Default (`InstancePerDependency()`) components are also supported.

## Further nesting... ##

Sub-lifetimes of `requestLifetime` will be able to resolve `HomeController` to access the single instance in their parent `requestLifetime`.

If default (`InstancePerDependency()`) scope is used, each request in a sub-lifetime will resolve a new instance, however that instance's lifetime will be bound to `requestLifetime` rather than the lifetime in which it is resolved.