# Important #

These instructions are for **ASP.NET MVC Version 2** (MVC2) which is no longer supported in the packages available from the Downloads section. (If you are looking for MVC3 instructions, see MvcIntegration3.)

If you’re still using ASP.NET MVC2 in your application, the simplest course of action is to continue using the Autofac 2.3 release series and the appropriate integration bundled with that, until you’re able to upgrade wholesale to [ASP.NET MVC3](MvcIntegration3.md).

If you would like to use Autofac 2.4+ with ASP.NET MVC, there is an updated Autofac.Mvc2 NuGet package now on the feed. If you’re already using the MVC2 NuGet package, update the package and you should be fine. If not, first remove all Autofac`*`.dll references from your project and then install the Autofac.Mvc2 package using the “Add Library Reference” dialog.

**Note**, the latest Autofac.Mvc2 package requires assembly binding redirects for the older MVC integration DLL bind to the newer Autofac version. The following section needs to be added to App.config or Web.config (substituting 2.4.3.700 for the most recent Autofac version):

```
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
    <dependentAssembly>
      <assemblyIdentity name="Autofac" publicKeyToken="17863af14b0044da" />
      <bindingRedirect oldVersion="2.3.2.632" newVersion="2.4.3.700" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="Autofac.Integration.Web" publicKeyToken="17863af14b0044da" />
      <bindingRedirect oldVersion="2.3.2.632" newVersion="2.4.3.700" />
    </dependentAssembly>
  </assemblyBinding>
</runtime>
```

# Getting ASP.NET MVC #

To use any version of ASP.NET MVC with .NET 3.5 you need to install the release from http://asp.net/mvc/.

ASP.NET MVC 2.0 is included in .NET 4.0.

**ASP.NET MVC 3.0:** With the release of MVC3 the integration has changed. See the [MvcIntegration3](MvcIntegration3.md) page for details on MVC3 usage.

# General Principles #

Integration into ASP.NET MVC projects and general usage - resolving dependencies, component lifetimes, etc. - [is the same as standard ASP.NET web forms](AspNetIntegration.md). It will help you to be familiar with that integration and usage as ASP.NET MVC integration and usage simply adds a small bit to that.

# Project Integration #

The way that Autofac integrates with ASP.NET MVC is through a combination of [IHttpModule](http://msdn.microsoft.com/en-us/library/system.web.ihttpmodule.aspx) implementations (for component lifetime management) and a custom controller factory (for injecting dependencies into MVC controllers).

**Integrating with ASP.NET MVC means performing all of the [standard ASP.NET application integration steps](AspNetIntegration#Standard_Integration.md) and then performing some specific additional steps.**

  * [Perform standard ASP.NET integration](AspNetIntegration#Standard_Integration.md).
    * [Reference Autofac assemblies.](AspNetIntegration#Reference_Assemblies.md)
    * [Add Autofac web modules to Web.config.](AspNetIntegration#Add_Modules_to_Web.config.md)
    * [Implement IContainerProviderAccessor in Global.asax.](AspNetIntegration#Implement_IContainerProviderAccessor_in_Global.asax.md)
  * [Register your MVC controllers with Autofac.](MvcIntegration2#Register_Controllers.md)
  * [Register your model binders with Autofac.](MvcIntegration2#Register_Model_Binders.md)
  * [Set the MVC controller factory.](MvcIntegration2#Set_the_Controller_Factory.md)

## Standard ASP.NET Integration ##

Instructions for standard ASP.NET application integration can be found on the [ASP.NET Integration](AspNetIntegration.md) wiki page in the [Standard Integration](AspNetIntegration#Standard_Integration.md) section. Those steps are applicable for ASP.NET MVC as well:

  * [Reference Autofac assemblies.](AspNetIntegration#Reference_Assemblies.md)
  * [Add Autofac web modules to Web.config.](AspNetIntegration#Add_Modules_to_Web.config.md)
  * [Implement IContainerProviderAccessor in Global.asax.](AspNetIntegration#Implement_IContainerProviderAccessor_in_Global.asax.md)

## Register Controllers ##

During application startup in Global.asax you need to register your application's MVC controllers. You do this so the Autofac controller factory can resolve the controller and its dependencies.

You can do this manually...

```
var builder = new ContainerBuilder();
builder.RegisterType<HomeController>().HttpRequestScoped();
```

...or you can use a provided extension method to register all the controllers in an assembly all at once:

```
var builder = new ContainerBuilder();
builder.RegisterControllers(Assembly.GetExecutingAssembly());
```

Note that ASP.NET MVC requests controllers by their concrete types, so registering them `As<IController>()` is incorrect. Also, if you register controllers manually and choose to specify lifetimes, you must register them as `InstancePerDependency()` or `HttpRequestScoped()` - **ASP.NET MVC will throw an exception if you try to reuse a controller instance for multiple requests**.

## Register Model Binders ##

Similar to controllers, model binders (classes that implement IModelBinder) can be registered in Global.asax. You can do this in one for an entire assembly:

```
var builder = new ContainerBuilder();
builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
```

Because the `RegisterModelBinders` extension method uses assembly scanning to add the model binders you need to specify what type(s) the `IModelBinder` class is to be registered for.

This is done by using the `Autofac.Integration.Web.Mvc.ModelBinderTypeAttribute`, like so:

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

## Set the Controller Factory ##

During application startup, after registering all of your dependencies and controllers, set the MVC controller factory to the Autofac controller factory. An example `Global.asax` implementation below shows this:

```
public class Global : HttpApplication, IContainerProviderAccessor
{
  // Provider that holds the application container.
  static IContainerProvider _containerProvider;

  // Instance property that will be used by Autofac HttpModules
  // to resolve and inject dependencies.
  public IContainerProvider ContainerProvider
  {
    get { return _containerProvider; }
  }

  protected void Application_Start(object sender, EventArgs e)
  {
    // Build up your application container and register your dependencies.
    var builder = new ContainerBuilder();
    builder.RegisterType<SomeDependency>();
    // ... continue registering dependencies and register your controllers...
    builder.RegisterControllers(Assembly.GetExecutingAssembly());
    // .. and you can register model binders
    builder.RegisterModelBinders(Assembly.GetExecutingAssembly());

    // Once you're done registering things, set the container
    // provider up with your registrations.
    _containerProvider = new ContainerProvider(builder.Build());

    // Set the controller factory using the container provider.
    ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(ContainerProvider));

    // Finish the rest of MVC standard setup like route registration.
    RegisterRoutes(RouteTable.Routes);
  }

  // ASP.NET standard route registration; see MVC documentation.
  static void RegisterRoutes(RouteCollection routes) { }
}
```


## An Example Implementation ##

The source for this project contains a demo web application project called `Remember.Web` which uses Autofac2, MVC 2.0. It demonstrates many of the aspects of MVC Autofac is used to inject.