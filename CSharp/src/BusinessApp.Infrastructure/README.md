# BusinessApp.Infrastructure
> Supporting Services

_Depends on the Kernel project_

This is the main infrastructure project for application services. Use this
project to support the Api project by adding dependencies for email, logging
validation, authentication etc. All supporting services run as
`IRequestHandler<TRequest, TResponse>` decorators or adapters.

## Features

//#if validation
- Validation
//#endif
- Logging
- Authorization
//#if hasbatch
- Batch Request Handling
//#endif
//#if macro
- Macro "like" Request Handling\
  _A macro is a single request from a client that issues batch commands_
//#endif
- Event Streaming
//#if metadata
- Command Streaming
//#endif
- Query Filtering
- JSON parsing
- Unique ID Generation
//#if automation
- Automation\
  _Triggers "workflows", where an event can trigger another request_
//#endif
