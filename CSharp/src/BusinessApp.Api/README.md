# BusinessApp.Api
> Business logic handling

_Depends on the Kernel and Infrastructure project_

The purpose of this project is to separate out your specific application (API)
business logic, separate from all the infrastructure noise. It should allow you
to focus on your business needs. `IRequestHandler` is the main service to handle
user requests.

__Note: If you have a Domain project, you probably want to add a reference to it__

__Bogus has been added to create fake data for your request handlers__

## Getting starting

### Setting up your data
Create "plain old c# classes" (POCO) that serve as your API contracts for requests.
This should force you to separate queries and commands. Inherit from `IQuery`
to get a rich query experience (Commands do not have to inherit from any
objects).

### Setting up your logic
- Create a class that implements `IRequestHandler` for one of these request
  data objects.
