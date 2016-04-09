# Introduction #

The decorator pattern is very handy and generally means wrapping one object in another that has the same interface but adds additional features before delegating to the wrapped object.

To implement decorators create an `OnActivating` handler that replaces the `Instance` property in the event argument with the decorated instance.

_Note, it is advisable to delegate `IDisposable.Dispose()` to the decorated instance if that interface is implemented._