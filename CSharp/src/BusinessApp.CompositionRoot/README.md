# README

The purpose of this project is to register all your services from an application
agnostic point of view (e.g not dependent on webapi or console project).

_Application specific services (e.g. webapi service) should be registered in their own project_

All services are registered via `IBootstrapRegister` service decorators. Create a
decorator and register your services either before or after the others. When the
application starts, it will search all files in the startup assembly for classes
that implement `IBootstrapRegister`

# Internal Dependencies

    [Api](/CSharp/src/BusinessApp.Api)
    [Infrastructure](/CSharp/src/BusinessApp.Infrastructure)
//#if efcore
    [Infrastructure.EntityFramework](/CSharp/src/BusinessApp.EntityFramework)
//#endif
    [Kernel](/CSharp/src/BusinessApp.Kernel)

# External Dependencies

    - [Simple Injector](https://github.com/simpleinjector/SimpleInjector)
    - Microsoft.Extensions.Logging.*
    - Microsoft.Extensions.Localization
