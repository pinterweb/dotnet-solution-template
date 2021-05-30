# BusinessApp.Api
> Coordinates business tasks

_Depends on the Kernel and Infrastructure project_

The purpose of this project is to define your application programming
interface (API) tasks. This can be as simple as saving requests or interacting
with the domain layer to run business rules. This layer is specific to your
application and has meaning to the stakeholders.

__Important: Do not put business rules or knowledge here. Only coordinate business rules__

__Note: If you have a Domain project, you probably want to add a reference to it__

__Note: Bogus has been added to create fake data for your request handlers__

## Getting starting

### Setting up your data
Create "plain old c# classes" (POCO) that serve as your API contracts for requests.
Separate query classes from commands. Implement `IRequestHandler{T,R}` to serve
these requests and coordinate business tasks.

### Creating queries
Inherit from `IQuery` or `Query` to get a rich query experience. You probably do
not need to implement `IQueryHandler` because the Infrastructure project
automatically handles queries and filtering.

#### Creating commands
Create a class that implements `IRequestHandler` if you have specific business
logic for the application. If you are just saving the request object, you do not
have to do this.
