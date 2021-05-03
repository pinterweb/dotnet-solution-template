# BusinessApp.CompositionRoot
> Service Registration

_Depends on API, Kernel and Infrastructure projects_
//#if efcore
_Depends on API, Kernel Entity Framework and Infrastructure projects_
//#endif

The purpose of this project is to register all your services from an application
agnostic point of view (e.g not dependent on webapi or console project).

_Application specific services (e.g. webapi service) should be registered in their own project_

## Getting Started

### Registering a new service
Create a new class that implements `IBootstrapRegister` and inject an inner
'IBootstrapRegister'. Then register your services either before or after
the inner handler (__do not forget to call the inner registration__). When the
application starts, it searches all files in the composition root assembly for
classes that implement `IBootstrapRegister`.
