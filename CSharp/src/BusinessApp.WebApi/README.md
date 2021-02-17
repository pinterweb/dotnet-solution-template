# README
_Logic to handle http requests_

## Bootstrap
All services are registered via `IBootstrapRegister` decoration. Create a
decorator and register your services either before or after the others. To
register a new application decorator, retrieve the pipeline via the context and
integrate the service decorator in the already existing pipeline. When the
application starts, it will search all files in the startup assembly for classes
that implement `IBootstrapRegister`

## Decorators
Decorators are used all over the WebApi to provide serialization, http
response handling, bootstrapping, exceptions etc. All decorators can be
identified with the `Decorator` prefix in the class name. Each `Decorator` will
have the same interface injected into its constructor, usually with the name
`inner`

## Adapter Handlers
Adapter handlers are special handlers that reduce the amount of code you have to
write. There a few out of the box, `MacroBatchRequestAdapter`,
`BatchRequestAdapter`. `MacroBatchRequestAdapter` accepts a single request and
expands it into an `IEnumerable` request. For example, a user may want to change
the vales on many records to the same values. Instead of sending a payload with many
requests that all have the same data, this adapter is used. `BatchRequestAdapter`
accepts an `IEnumerable` request and calls the actual handler that handled the
single request. This single handler is normally your api logic to change data.
With this adapter you do not have to write multiple handlers to handle one or
many of the same payload. Similiarly on the query side,
`SingleQueryRequestDelegate` delegates the call to the `IEnumerable` query
handler so you only have to write a query handler for `IEnumerable` results.
This adapter will run ensure that only one result is returned, or an error.

## Proxy Handlers
These are special handlers to stop *SimpleInjector* decoration. A proxy handler
bakes the `TConsumer` into its generic definition. The `TConsumer` is the
concrete implementation for handling the request and will have no decoration.
This allows the `BatchRequestAdapter` to call the actual request handler many times
without having to worry about re-decorating the request. However, decoration can
still be controlled in the proxy classes. For example, the
`ValidationRequestDecorator` runs for every call to validate each request in the
`IEnumerable`.

## Authorization
Requests that have an `Authorize` attribute will be checked. Otherwise, not
authorization is performed
