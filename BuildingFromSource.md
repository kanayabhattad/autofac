**Note:** As of Autofac 3, this document has been superseded by the README in the root of the codeline.

# Building a Release for .NET 4.0 #

**Important:** To build Autofac, you need to open a Visual Studio 2010 command prompt. This can be found in the _Start_ menu under _Microsoft Visual Studio 2010/Visual Studio Tools_.

The _Autofac.build_ file is an MSBuild script that will build, test and package the core Autofac assemblies.

The _Release_ target is used to build the redistributable versions found on the Autofac website.

The version can be specified via the _Version_ property.

```
    msbuild Autofac.build /T:Release /P:Version=2.3.4.567
```

After the process completes, binaries will be zipped to the _Build/__Package_ sub-folder.

## Help Files ##

The help file is not built in the .NET 4.0 version because the help generation tools do not yet support that framework version.

# Other Platforms #

The Silverlight and .NET 3.5 versions of Autofac **still require the VS2010 tool set** in order to be built from source.

The _BuildFramework_ parameter selects the target framework:

| **BuildFramework Value** | **Target Framework** | **Notes** |
|:-------------------------|:---------------------|:----------|
| _none_                   | .NET 4.0             |           |
| NET35                    | .NET 3.5             | Generates help file, see note below |
| SL4                      | Silverlight 4        | Not fully tested |
| SL3                      | Silverlight 3        |           |
| SL2                      | Silverlight 2        | No longer officially supported |

For example, to build against .NET 3.5 run:

```
    msbuild Autofac.build /T:Release /P:Version=2.3.4.567 /P:BuildFramework=NET35
```

The .NET 3.5 target requires [Microsoft Sandcastle](http://sandcastle.codeplex.com).

The Windows Phone 7 builds require the [.NET Portable Class Library tools](http://msdn.microsoft.com/en-us/library/gg597391.aspx).

**To build NET35 without generating Help files** select targets manually:

```
    msbuild Autofac.build /T:Clean;_Version;Build;Test /P:Version=2.3.4.567 /P:BuildFramework=NET35
```