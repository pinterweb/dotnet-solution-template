# README
_Logic to handle http requests_

This is the main infrastructure project for web api services in a controllerless
app.

## Services

# Internal Dependencies

    [CompositionRoot](/CSharp/src/BusinessApp.CompositionRoot)

# External Dependencies

//#if winauth
    - AspNetCore negotiate authentication
//#endif
    - Microsoft Logging
    - [Simple Injector](https://github.com/simpleinjector/SimpleInjector)
    - System.IO.Pipelines for JSON parsing
    - AspNetCore.App to bring in all required services to support a runnable
      ASPNET core app.

## Services
    - git hooks
