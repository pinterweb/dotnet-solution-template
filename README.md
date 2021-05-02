# Dotnet WebApi Template
_Light weight webapi template surrounded by a lot of infrastructure to get your started_

Inspired by [solidservices](https://github.com/dotnetjunkie/solidservices) and
powered by [Simple Injector](https://github.com/simpleinjector/SimpleInjector) to provide rich
service decoration for your business needs.

- [Summary](#summary)
- [Installation](#installation)
- [Usage](#usage)


## Summary
This project is a layered C# code, built around the concept of stratified design.
The kernel & analyzer projects are at the center, supporting infrastructure
code. Infrastructure code is separated out depending on its function to isolate
dependencies that should not be shared. To find out more information on the
projecs, see the READMEs in each projects:

    [Analyzers](/CSharp/src/BusinessApp.Analyzers)
    Code generators to make your life easier
    [Api](/CSharp/src/BusinessApp.Api)
    Your app's business logic
    [CompositionRoot](/CSharp/src/BusinessApp.CompositionRoot)
    Registers all services
    [Infrastructure](/CSharp/src/BusinessApp.Infrastructure)
    Services to support your app's business logic
    [Infrastructure.EntityFramework](/CSharp/src/BusinessApp.EntityFramework)
    Services to support persisting and querying data with entity framework core
    [Infrastructure.WebApi](/CSharp/src/BusinessApp.WebApi)
    Services to support your controllerless web api project
    [Kernel](/CSharp/src/BusinessApp.Kernel)
    The core code shared by all projects
    [WebApi](/CSharp/src/BusinessApp.WebApi)
    The runnable aspnet web api

## Installation

1. `git clone https://github.com/pinterweb/dotnet-webapi-template.git`
2. `cd dotnet-webapi-template`
3. `.\install.bat`
   _to uninstall run `.\install.bat u`_

## Usage

To create a new project with all the defaults run:

```
dotnet new webapi-bizapp -n <your-app-name> -o <your-app-directory>"
```

The default setup gives you:
    - git directory
    - git hooks to run tests on commits
    - https://github.com/dotnet/efcore
    - JSON parsing with System.Text.Json
    - docker setup
    - data annotations validation
    - static file support
    - https://github.com/bchavez/Bogus for development


You can optionally:
    - Replace System.Text.Json with Newtonsoft
    - Setup HATEOAS via weblinking
    - Add https://github.com/FluentValidation/FluentValidation
    - Add window authentication for intranet apps
    - Add CORS in development mode if your clients are build separately

To see all the available template options run:

```
dotnet new webapi-bizapp --help
```

Once you are happy with your setup, navigate to the WebAPI project run `dotnet
watch run`.
