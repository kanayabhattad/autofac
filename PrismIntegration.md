# Status #

This is a work-in-progress and only available in source code via Subversion.

It targets the November 2008 Prism release but shouldn't be hard to port.

If you'd like to see this integration updated and distributed in binary form please vote for it here: http://autofac.uservoice.com/pages/25274-general/suggestions/279030-support-prism-from-microsoft-patterns-practices.

# Usage #

## Project Setup ##

Reference AutofacContrib.Prism.dll from your application.

## The Application Class ##

In the `OnStartup` method of your main application class (`App.xaml.cs`):

  * Create a `ContainerBuilder`
  * Register an `AutofacContrib.Prism.PrismModule` with the builder, passing the type of the shell window to its constructor
  * Register one or more Autofac modules configuring the application (see below)

An example application class (for the Stock Trader RI sample) might look like:

```
public partial class App : Application
{
    IContainer _container;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = new ContainerBuilder();
        builder.RegisterModule(new PrismModule(typeof(Shell)));
        builder.RegisterModule(new StockTraderRIModule());

        _container = builder.Build();

        var view = _container.Resolve<ShellPresenter>().View;
        view.ShowView();

        this.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container.Dispose();
        _container = null;
    }
}
```

## Application Module ##

The various Prism features can be customised, and the application modules set up via an app-specific Autofac module:

```
public class StockTraderRIModule : Autofac.Builder.Module
{
    protected override void Load(Autofac.Builder.ContainerBuilder builder)
    {
        base.Load(builder);

        builder.SetDefaultOwnership(Autofac.InstanceOwnership.External);

        builder.Register<EntLibLoggerAdapter>().As<ILoggerFacade>();

        builder.Register(c => new StaticModuleEnumerator()
                .AddModule(typeof(NewsModule))
                .AddModule(typeof(MarketModule))
                .AddModule(typeof(WatchModule), "MarketModule")
                .AddModule(typeof(PositionModule), "MarketModule", "NewsModule"))
            .As<IModuleEnumerator>();

        builder.Register(c => c.Resolve<Shell>()).As<IShellView>();

        builder.Register<ShellPresenter>();

        builder.RegisterTypesMatching(
            t => t.Name.Contains("Module")
                || t.Name.Contains("View")
                || t.Name.Contains("Proxy"))
            .ExternallyOwned();
    }
}
```