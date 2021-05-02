# README

The purpose of this project is to serve http requests to your API layer so it
can run your business logic

# Internal Dependencies

    - [Infrastructure.WebApi](/CSharp/src/BusinessApp.WebApi)
      To use general purpose WebAPI services outside of controllers
    - [Api](/CSharp/src/BusinessApp.Api)
      To call your business logic via `IRequestHandler` classes

# External Dependencies

    - https://github.com/bchavez/Bogus (dev dependency)
      Needed because it is references in your API project for fake data
      generation

## Extras
//#if efcore
    - Will run migrations to generate a script that can be used to update your database
      (see the csproj file for more details)
//#endif
