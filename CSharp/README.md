# BusinessApp
_$(product_description)_

## Summary

This project is a layered C# solution, built around the concept of stratified design.
The kernel & analyzer projects are at the very core/bottom providing support
for all projects. Infrastructure code is separated out depending on its
function to isolate dependencies. To find out more information on a project,
see the README:

## Projects

[CompositionRoot](/CSharp/src/BusinessApp.CompositionRoot)
Registers all services

[WebApi](/CSharp/src/BusinessApp.WebApi)
The runnable aspnet web api entrypoint

[Infrastructure.WebApi](/CSharp/src/BusinessApp.WebApi)
Services to support your controllerless web api project

//#if efcore
[Infrastructure.EntityFramework](/CSharp/src/BusinessApp.EntityFramework)
Services to support persisting and querying data with entity framework core
//#endif

[Api](/CSharp/src/BusinessApp.Api)
Your app's business logic

[Infrastructure](/CSharp/src/BusinessApp.Infrastructure)
Services to support your app's business logic

[Analyzers](/CSharp/src/BusinessApp.Analyzers)
Code generators to make your life easier

[Kernel](/CSharp/src/BusinessApp.Kernel)
The core code shared by all projects

## Getting Started

- Add your continuous integration assets (e.g. azure-pipeline.yml)
- Commit your code to source control (e.g. `feat(all): Initial commit`)
- Create your query & commands models in the Api project
//#if efcore
- Setup `IEntityConfiguration<{TModel}>` classes for any query contracts and
  commands. Command data is automatically saved to the database and query
  contracts are queried by entity framework.
- Run .\migrations_add to setup your database
//#endif
- Create routes in `Routes.cs` located in the WebApi project
  _note: Any requests inheriting from `IQuery` returning an `IEnumerable`, will_
  _be handled by an `IRequestHandler{T, EnvelopeContract{TResponse}}`_
//#if (!efcore)
- Create a `IRequestHandler{TRequest, TResponse}` in the Api project to handle
   queries and commands.
   _there is no data persistence, so you will have to set that up yourself_
   _However, you can use Bogus to fake data from a request handler_
//#endif
//#if efcore
- Optional: Create a `IRequestHandler{TRequest, TResponse}` in the Api project
   _this step is optional if you just want to save the command data. A generic_
   _request handler will run and save the command data to a database if you_
   _configure the command in an `IEntityConfiguration<{TModel}> classes`._
   _Similarly, query objects inheriting from `IQuery` will already have a request_
   _handler to run the query. Make sure to setup your query contracts in an_
   _`IEntityConfiguration<{TModel}>` class first. Also, you can use the Bogus_
   _library if you want to bypass data persistence, just be sure to setup your_
   _own request handler to generate this fake data_
//#endif
//#if docker
- Run `docker-compose up` from the src directory.
  _the webapi image depends on the db image. However, it could take a little bit_
  _for the database to start. That will throw an error when the webapi starts up._
  _I would suggest running `docker-compose up -d db` first and wait._
//#endif
//#if (!docker)
- Run `dotnet watch run`
//#endif
