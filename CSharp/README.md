# BusinessApp
_$(product_description)_

- [Layout](#layout)
- [Getting Started](#getting-started)
- [Usage](#usage)

## Layout
This application takes a layered approach to isolate services. Starting from the
very inside:

    - Kernel: The core services used by all projects
    - Analyzers: Core services generated at compile time & used by all projects
    - Infrastructure: Application services used to support the API business
      logic and implementation.
    - Api: Contract & Request handlers to facilitate business logic
//#if efcore
    - Infrastructure.EntityFramework: Isolates entity framework for data
      persistence
//#endif
    - Infrastructure.WebApi: General services to support any type of web
      requests. Purpose for separating this from WebApi project is to keep your
      actual application projects slim.
    - WebApi: Entry point for your webapi application
    - Composition Root: Entry point to register services for any application.

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
