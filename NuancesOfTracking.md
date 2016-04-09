_This is really only interesting if you're interested in the differences between Autofac and other containers. If you're an Autofac user, you can ignore this and be confident that Autofac will always get object disposal right._

A number of containers are starting to provide unit-of-work-based instance tracking. The basic pattern follows the model of creating a disposable 'unit of work' object that tracks instances created by the container during its lifetime and disposes the instances when it is itself disposed (see [DeterministicDisposal](DeterministicDisposal.md).)

Autofac was designed from the ground up with this model in mind, so it avoids a couple of problems that other containers tend to neglect.

1. Not every object created within a unit of work 'belongs' to that unit of work

Imagine a lazily-instantiated singleton - a transient component created during a unit of work may be the first during an application run to request the singleton, so that singleton is created when the dependencies of the transient component are resolved. The singleton, though it was instantiated during the unit of work, must outlive it.

The naive solution to the problem would be to exclude singletons from the unit of work tracking system. This would have to apply to any other components with lifetimes outside of the unit of work too, e.g. those bound to the HTTP request, the current transaction, etc.

Unfortunately, those components not managed by the unit of work tracker often have transient dependencies of their own. These dependencies have dependencies of their own, and so-on. Those instances need to live as long as the object requesting them, even though other instances of the same component form part of the unit of work.

The net result is that unless sophisticated tracking is done in the container, instances are either not disposed when they should be, or get disposed prematurely. Autofac is, at the time of writing, the only container to get this right.

2. Different threads need independent units of work

Creating and disposing a 'scope' on the container in one thread should not affect instances created and used on another thread. A single, global concept of scope for a container does not allow for this, and tying scopes to individual threads is chaotic in many work scheduling models. Autofac allows easy segregation of units of work by making inner containers self contained but thread-agnostic.