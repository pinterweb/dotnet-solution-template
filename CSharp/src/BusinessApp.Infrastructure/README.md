# BusinessApp.Infrastructure
> Supporting Services

_Depends on the Kernel project_

This is the main infrastructure project for application services. Use this
project to support the Api project by adding dependencies for email, logging
validation, authentication etc. All supporting services run as
`IRequestHandler<TRequest, TResponse>` decorators or adapters.

## Features

- Validation
- Logging
- Authorization
- Batch Request Handling
- Event Streaming
- Command Streaming
- Query Filtering
- JSON parsing
- Unique ID Generation
