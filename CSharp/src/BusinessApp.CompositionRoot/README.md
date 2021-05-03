# BusinessApp.CompositionRoot
> Service Registration

_Depends on API, Kernel and Infrastructure projects_
//#if efcore
_Depends on API, Kernel Entity Framework and Infrastructure projects_
//#endif

The purpose of this project is to register all your services from an application
agnostic point of view (e.g not dependent on webapi or console project).

_Application specific services (e.g. webapi service) should be registered in their own project_

All services are registered via `IBootstrapRegister` service decorators. Create a
decorator and register your services either before or after the others. When the
application starts, it will search all files in the startup assembly for classes
that implement `IBootstrapRegister`
