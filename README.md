# Dotnet Solution Template
_Layered dotnet solution template decorated with services_

Inspired by [solidservices](https://github.com/dotnetjunkie/solidservices) and
powered by [Simple Injector](https://github.com/simpleinjector/SimpleInjector) to provide rich
service decoration for your business needs. For project definitions see the main
[README](/CSharp) that goes along with new applications.

- [Installation](#installation)
- [Testing](#testing)
- [Running the webapi](#running-the-webapi)
- [Creating a new app](#creating-a-new-app)

## Installation

1. `git clone https://github.com/pinterweb/dotnet-webapi-template.git`
2. `cd dotnet-webapi-template`
3. `.\install.bat`\
   _to uninstall run `.\install.bat u`_

## Testing

1. `cd CSharp\src`
2. `dotnet test`

## Running the webapi

The webapi project can be run without creating a new application

1. `cd CSharp\src\BusinessApp.WebApi`
2. `dotnet watch run`\
   _read the "Getting Starting" in each project's README to determine the data_
   _and services you need_

## Creating a new app

To create a new solution with all the defaults run:

```
dotnet new webapi-bizapp -n <your-app-name> -o <your-app-directory>
```

The default setup gives you:\
- git directory
- git hooks to run tests on commits
- [Entity Framework Core](https://github.com/dotnet/efcore)
- JSON parsing with System.Text.Json
- docker setup
- data annotations validation
- static file support
- [Bogus](https://github.com/bchavez/Bogus) for fake data generation in development

You can optionally:\
- Replace System.Text.Json with [Newtonsoft](https://github.com/JamesNK/Newtonsoft.Json)
- Setup HATEOAS via [weblinking](https://tools.ietf.org/html/rfc8288)
- Add [Fluent Validation](https://github.com/FluentValidation/FluentValidation)
- Add window authentication for intranet apps
- Add CORS in development mode if your clients are built separately

To see all the available template options run:

```
dotnet new webapi-bizapp --help
```
