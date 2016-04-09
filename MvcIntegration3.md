**Important Note:** If you're looking to support earlier versions of ASP.NET MVC see [this page](MvcIntegration2.md).

The Autofac MVC integration has been updated to take advantage of new ASP.NET MVC 3 features that were added to provide better support for dependency injection. You can read more about these new ASP.NET MVC 3 features in Brad Wilson's detailed [blog post series](http://bradwilson.typepad.com/blog/2010/07/service-location-pt1-introduction.html).

## Get Started with NuGet ##

The best way to start using ASP.NET MVC3 with Autofac is to install the Autofac.Mvc3 [NuGet](http://nuget.org) package.

After installing the package, all you need to do to enable dependency injection into controllers is to set the `DependencyResolver` in the application startup method of `Global.asax`:

```
protected void Application_Start()
{
    var builder = new ContainerBuilder();
    builder.RegisterControllers(typeof(MvcApplication).Assembly);
    var container = builder.Build();
    DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

    // Other MVC setup...
```

## New ASP.NET MVC 3 Projects ##

### Required References ###

Add a reference to the following assemblies:

  * `Autofac.dll`
  * `Autofac.Integration.Mvc.dll`

You will need to add usings for the `Autofac` and `Autofac.Integration.Mvc` namespaces.

### Registering Controllers ###

Inside the Application\_Start method in the `Global.asax.cs` register your controllers and their dependencies.

You can do this manually...

```
var builder = new ContainerBuilder();
builder.RegisterType<HomeController>().InstancePerHttpRequest();
```

...or you can use a provided extension method to register all the controllers in an assembly all at once:

```
var builder = new ContainerBuilder();
builder.RegisterControllers(Assembly.GetExecutingAssembly());
```

Note that ASP.NET MVC requests controllers by their concrete types, so registering them `As<IController>()` is incorrect. Also, if you register controllers manually and choose to specify lifetimes, you must register them as `InstancePerDependency()` or `InstancePerHttpRequest()` - **ASP.NET MVC will throw an exception if you try to reuse a controller instance for multiple requests**.

### Dependency Resolver ###

After building your container pass it into a new instance of the `AutofacDependencyResolver` class. Finally, use the static `DependencyResolver.SetResolver` method to let ASP.NET MVC know that it should locate services using the `AutofacDependencyResolver`. This is Autofac's implementation of the new `IDependencyResolver` interface.

```
IContainer container = builder.Build();
DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
```

### Register Model Binders ###

Similar to controllers, model binders (classes that implement `IModelBinder`) can be registered in `Global.asax.cs`. You can do this in one hit for an entire assembly:

```
var builder = new ContainerBuilder();
builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
builder.RegisterModelBinderProvider();
```

You must also remember to register the `AutofacModelBinderProvider` using the `RegisterModelBinderProvider` extension method. This is Autofac's implementation of the new `IModelBinderProvider` interface.

Because the `RegisterModelBinders` extension method uses assembly scanning to add the model binders you need to specify what type(s) the `IModelBinder` class is to be registered for.

This is done by using the `Autofac.Integration.Mvc.ModelBinderTypeAttribute`, like so:

```
[ModelBinderType(typeof(string))]
public class StringBinder : IModelBinder
{
    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
        //do implementation here
    }
}
```

Multiple instances of the `ModelBinderTypeAttribute` can be added to a class if it is to be registered for multiple types.

### Injecting HTTP Abstractions ###

The MVC Integration includes an Autofac module that will add HTTP request lifetime scoped registrations for the HTTP abstraction classes. The following abstract classes are included:

  * `HttpContextBase`
  * `HttpRequestBase`
  * `HttpResponseBase`
  * `HttpServerUtilityBase`
  * `HttpSessionStateBase`
  * `HttpApplicationStateBase`
  * `HttpBrowserCapabilitiesBase`
  * `HttpCachePolicyBase`
  * `VirtualPathProvider`

To use these abstractions add the `AutofacWebTypesModule` to the container using the standard `RegisterModule` method.

```
builder.RegisterModule(new AutofacWebTypesModule());
```

### View Page Injection ###

You can make [property injection](PropertyInjection.md) available to your MVC views by adding the `ViewRegistrationSource` to your `ContainerBuilder` before building the application container.

```
builder.RegisterSource(new ViewRegistrationSource());
```

Your view page must inherit from one of the base classes that MVC supports for creating views. When using the Razor view engine this will be the `WebViewPage` class.

```
public abstract class CustomViewPage : WebViewPage
{
    public IDependency Dependency { get; set; }
}
```

The `ViewPage`, `ViewMasterPage` and `ViewUserControl` classes are supported when using the WebForms view engine.

```
public abstract class CustomViewPage : ViewPage
{
    public IDependency Dependency { get; set; }
}
```

Ensure that your actual view page inherits from your custom base class. This can be achieved using the `@inherits` directive inside your `.cshtml` file for the Razor view engine.

```
@inherits Example.Views.Shared.CustomViewPage
```

When using the WebForms view engine you set the `Inherits` attribute on the `@ Page` directive inside you `.aspx` file instead.

```
<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Example.Views.Shared.CustomViewPage"%>
```

### Filter Attribute Property Injection ###

To make use of property injection for your filter attributes call the `RegisterFilterProvider` method on the `ContainerBuilder` before building your container and providing it to the `AutofacDependencyResolver`.

```
ContainerBuilder builder = new ContainerBuilder();
 
builder.RegisterControllers(Assembly.GetExecutingAssembly());
builder.Register(c => new Logger()).As<ILogger>().InstancePerHttpRequest();
builder.RegisterFilterProvider();
 
IContainer container = builder.Build();
DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
```

Then you can add properties to your filter attributes and any matching dependencies that are registered in the container will be injected into the properties. For example, the action filter below will have the `ILogger` instance that was registered above injected. Note that the attribute itself does not need to be registered in the container.

```
public class CustomActionFilter : ActionFilterAttribute
{
    public ILogger Logger { get; set; }
 
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        Logger.Log("OnActionExecuting");
    }
}
```

The same simple approach applies to the other filter attribute types such as authorization attributes.

```
public class CustomAuthorizeAttribute : AuthorizeAttribute
{
    public ILogger Logger { get; set; }
 
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        Logger.Log("AuthorizeCore");
        return true;
    }
}
```

After applying the attributes to your actions as required your work is done.

```
[CustomActionFilter]
[CustomAuthorizeAttribute]
public ActionResult Index()
{
    // ...
}
```

## Upgrading from ASP.NET MVC 2 ##

In order to take advantage of the new dependency injection support in ASP.NET MVC 3 a number of breaking changes were introduced. There is no longer a need to reference the [ASP.NET Integration](AspNetIntegration.md) (`Autofac.Integration.Web.dll`) in order to get the MVC Integration to work. The following steps outline how to get your code to a point were you are ready to use the new MVC Integration. After finishing the steps below please refer to the instructions outlined in [New ASP.NET MVC 3 Projects](MvcIntegration3#New_ASP.NET_MVC_3_Projects.md) to complete the process.

For information on upgrading your actual MVC project please see the "Upgrading an ASP.NET MVC 2 Project to ASP.NET MVC 3" section in the [ASP.NET MVC 3 release notes](http://www.asp.net/learn/whitepapers/mvc3-release-notes#upgrading). There is also a [MVC 3 Project Upgrade Tool](http://blogs.msdn.com/b/marcinon/archive/2011/01/13/mvc-3-project-upgrade-tool.aspx) for upgrading both MVC 2 and MVC 3 Beta (or RC) projects, but that currently only supports Visual Studio 2010 projects targeting .NET 4.

### Update References ###

The following references should be removed and replaced with those outlined in the [Required References](MvcIntegration3#Required_References.md) section:

  * `Autofac.Integration.Web.dll`
  * `Autofac.Integration.Web.Mvc.dll`

You will also need to update your using statements from `Autofac.Integration.Web.Mvc` to `Autofac.Integration.Mvc`.

### Remove IContainerProviderAccessor Interface ###

The `HttpApplication` class no longer needs to implement the `IContainerProviderAccessor` interface as [described](AspNetIntegration#Implement_IContainerProviderAccessor_in_Global.asax.md) in the ASP.NET Integration documentation. All code related to implementing the interface should be removed your `Global.asax.cs` file.

### Remove AutofacControllerFactory ###

The `AutofacControllerFactory` class no longer exists and references to it should be removed. There is no longer a need to set the default controller factory as the new `AutofacDependencyResolver` now takes over this responsibility.

### Remove HTTP Module Configuration ###

There is no longer a need to manually register any HTTP modules in the `web.config` file as [described](AspNetIntegration#Add_Modules_to_Web.config.md) in the ASP.NET Integration documentation. The appropriate HTTP module will automatically be loaded when your web application starts. Remove all references to the `ContainerDisposal` module (and `PropertyInjection` module if present) from your `web.config` file.

### Model Binding ###

If you were making use of the Model Binder injection from the previous integration please ensure that you now also register the `AutofacModelBinderProvider`. Instructions on how to do this can be found under [Register Model Binders](MvcIntegration3#Register_Model_Binders.md).

### ExtensibleActionInvoker Breaking Changes ###

If you were using the ExtensibleActionInvoker to perform property injection on filter attributes, you should now use the filter provider outlined in the [Filter Attribute Property Injection](MvcIntegration3#Filter_Attribute_Property_Injection.md) section above instead. This feature had to be removed from the ExtensibleActionInvoker due to a [bug](http://code.google.com/p/autofac/issues/detail?id=311).

## Example Implementation ##

The Autofac [source](http://code.google.com/p/autofac/source/checkout) contains a demo web application project called `Remember.Web`. It has been upgraded to use ASP.NET MVC 3 and demonstrates many of the aspects of MVC that Autofac is used to inject.