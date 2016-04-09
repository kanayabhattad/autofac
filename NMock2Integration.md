_Introduced in revision 284_

# Overview #

Autofac.Integration.NMock2 simplifies the process of unit testing components with complex interactions.

Autofac deals with the component resolution and deterministic disposal, while NMock dynamically provides mocks for the interfaces that were not registered.

# Project Integration #

You need to include Autofac.Integration.NMock2.dll, Autofac.dll and NMock2.dll in the references list of your project.

# Using the NMock2Integration #

Using the Autofac.Integration.NMock2 is as simple as

```
using (var mock = new AutoMock())
{
    Expect.Once.On(mock.Resolve<IServiceB>()).Method("RunB");
    Expect.Once.On(mock.Resolve<IServiceA>()).Method("RunA");        

    var component = mock.Create<TestComponent>();
    component.RunAll();
}
```

In this code snippet we test some `TestComponent` that has the dependencies on `IServiceA` and `IServiceA`. It is expected that their Run methods will be called when we execute `TestComponent.RunAll()`.

Note, that `using (var mock = new AutoMock())` statement is equal to `using (var mockery = new Mockery())` from the NMock2 and thus there is no need to call `Mockery.VerifyAllExpectationsHaveBeenMet()`.

If you want to ensure that methods get called in a certain order, then the following initialization could be used:
```
using (var mock = AutoMock.GetOrdered())
{
    ...
}
```

Here's the formal usage scenario:
  1. Setup the AutoMock with `using` statement.
  1. Define expectations.
  1. Create the test object in the container.
  1. Perform testable actions.
  1. When the container goes out of the scope, the expectations will be verified.

You can refer to NMock2 [cheat cheet](http://nmock.org/cheatsheet.html) for more ways to setup your expectations.

# Advanced Options #

You can use `AutoMock.Container` and `AutoMock.Mockery` if there is need for more advanced control over the instances behind the AutoMock.

For example, you might want to :
  * register the testable component with some explicit declaration;
  * test the Module with external dependencies;
  * manually call `Mockery.VerifyAllExpectationsHaveBeenMet()`