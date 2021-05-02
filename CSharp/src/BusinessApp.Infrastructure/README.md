# README

This is the main infrastructure project for application services. Use this
project to support the Api project by adding dependencies for email, logging
validation, authentication etc. All supporting services are injected via
`IRequestHandler{TRequest, TResponse}` decorators, which decorate the main
handlers in the Api project.

__Note: There is a `IRequestHandler` that runs without any logic when no handler is found__

# Supported Services

    - Validation
    - Logging
    - Authorization
    - Batch Request Handling
    - Event Streaming
    - Command Streaming
    - Query Filtering
    - JSON parsing
    - Unique ID Generation

# Internal Dependencies

    [Kernel](/CSharp/src/BusinessApp.Kernel) because that has the main app services

# External Dependencies

//#if fluentvalidation
    - https://github.com/FluentValidation/FluentValidation
//#endif
    - https://github.com/RobThree/IdGen
      Supports client global id generation that can be used across applications.
      This is preferred over database id generation
//#if dataannotations
    - System Data Annotations for validation
//#endif
    - Microsoft Logging Extensions
//#if (json=="systemjson")
    - System JSON parsing
//#endif
//#if (json=="newtonsoft")
    - Newtonsoft JSON parsing
//#endif


## Services

### Decorators
Decorators are used all over the infrastructure projects to provide serialization,
http response handling, bootstrapping, exceptions etc. All decorators can be
identified with the `Decorator` prefix in the class name. Each `Decorator` will
have the same interface injected into its constructor, usually with the name
`inner`.

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
many of the same payload. Similarly on the query side,
`SingleQueryRequestDelegate` delegates the call to the `IEnumerable` query
handler so you only have to write a query handler for `IEnumerable` results.
This adapter will run ensure that only one result is returned, or an error.

## Proxy Handlers
Classes ending with `Proxy` name

## Authorization
Requests that have an `Authorize` attribute will be checked. Otherwise, no
authorization is performed
