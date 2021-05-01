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

    - Kernel project because that has the main app services

# External Dependencies

<!--#if (fluentvalidation)-->
    - https://github.com/FluentValidation/FluentValidation
<!--#endif-->
    - https://github.com/RobThree/IdGen
      Supports client global id generation that can be used across applications.
      This is preferred over database id generation
<!--#if (dataannotations)-->
    - System Data Annotations for validation
<!--#endif-->
    - Microsoft Logging Extensions
<!--#if (json=="systemjson")-->
    - System JSON parsing
<!--#endif-->
<!--#if (json=="newtonsoft")-->
    - Newtonsoft JSON parsing
<!--#endif-->
