# BusinessApp.WebApi
> Logic to handle your http routes_

_Depends on the CompositionRoot project_

The purpose of this project is to serve http requests to your API layer so it
can run your business logic. **There are no controllers**, so use endpoint
routing that hooks into your API `IRequestHandler{T, R}` services.

## Getting starting

Create endpoints in [Routes.cs](/CSharp/src/BusinessApp.WebApi/Routes.cs)

