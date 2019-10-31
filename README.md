# Dotnet WebApi Template

Inspired by [solidservices](https://github.com/dotnetjunkie/solidservices)

- [Installation](#installation)
- [Usage](#usage)
- [Solution Layout](#layout)

## Installation

To install the template in the `dotnet new` list, run
```
install.bat
```

To uninstall the template run:
```
install.bat u
```

## Usage

To create a new project using the template with all the default options run:

```
dotnet new webapi-bizapp --copyrightName "Your Copyright"
```

Then navigate to the WebApi project and run `dotnet run`. The application runs on port 5000,
but should return 404's until you implement your own routes and handlers.

To see all the available template options run:

```
dotnet new webapi-bizapp --help
```


## Layout
The layout of the solution is driven by a Command/Query separation while isolating
the Domain layer

### Domain Layer
Classes/Services to serve the Aggregate(s)

### App Layer
Classes/Services to serve the domain in a command/query fashion

### Data Layer
Classes/Services to persist the domain Aggregate(s)

### WebApi Layer
Classes/Services to run a minimal web request/response application. This should
help you focus on building your application and not relying so much on the framework.
Routes are used rather than controllers to force you to implement your own command/query
handling
