# To Do #
  * Merge in the [Whitebox](http://whitebox.googlecode.com) profiler project
  * Update all wiki documentation to the 3.x version
  * Architecture supporting discovery of resolved type under Whitebox (currently only the component type is shown in a resolve operation, not the requested service - Autofac needs updating to fully support describing dependencies)
  * Missing wiki doc:
    * Contravariant registration source

# Complete #
  * Update solution to build with VS2012
  * Switch to a single-target model:
    * Common functionality in Portable Class Libraries.
    * Platform-specific functionality in standard libraries targeting either full .NET or PCL as needed.
  * Ensure tests are runnable in VS.
  * Fully adopt NuGet (semver, -prerel) versioning conventions
  * Add NuGet symbol/source packages
  * Add a Metro app sample so we can run appcert.exe to analyze Autofac core compatibility. (appcert requires admin permissions, so don't run it as part of the normal build.)
  * Create one core/contrib solution
    * Rename contrib libraries to "Extras"
    * Cull unmaintained contrib projects
  * Add testing for partial trust scenarios. (To ensure things are properly marked up to allow partial trust callers. Augments the secannotate report.)
  * Add FxCop support to core libraries and select Extras.
  * Strongly-typed concrete classes as metadata views
  * Update the notification system to post to a group.
  * Address the issues in all the ignored tests. (Only tests related to specific filed issues are now ignored.)
  * Allow packages to be built/released independently, i.e. don't force a release of core just to update a contrib/integration package
  * Update build script to externalize project metadata [as in this forum thread](https://groups.google.com/forum/#!topic/autofac/kw33nq8CCrk) so project targets can be modified without modifying central build script logic.
  * Make sure secannotate.exe [runs and passes](http://stackoverflow.com/questions/12360534/how-can-i-successfully-run-secannotate-exe-on-a-library-that-depends-on-a-portab) on all libraries.

# Skipped #
  * Switch Autofac core unit tests to be PCL tests. (Skipped since it adds little value at this time. Maybe do this in the future.)
  * Switch Autofac.Configuration to be a PCL. (Skipped because System.Configuration and app.config files are only available in full .NET.)