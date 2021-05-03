# BusinessApp.Kernel
> Core Services

Also known as the "Shared Kernel", these *core* services are shared by all projects
and require no dependencies. Typically, you would find these services in the
Domain layer of a project. However, these services are too generic to be part of
an application's domain, and therefore are moved to here to be shared.

__If you have specific domain logic, create a BusiniessApp.Domain project__
