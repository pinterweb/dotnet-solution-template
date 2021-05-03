# README

# Dotnet Solution Template
_Layered dotnet solution template decorated with services_

Inspired by [solidservices](https://github.com/dotnetjunkie/solidservices) and
powered by [Simple Injector](https://github.com/simpleinjector/SimpleInjector) to provide rich
service decoration for your business needs.

## Installation

1. `git clone https://github.com/pinterweb/dotnet-webapi-template.git`
2. `cd dotnet-webapi-template`
3. `.\install.bat`
   _to uninstall run `.\install.bat u`_

## Test

1. `cd CSharp\src`
2. `dotnet test`

## Running the Webapi

1. `cd CSharp\src\BusinessApp.WebApi`
2. `dotnet watch run`
   _read each README to determine the data and services you need_

## New App

To create a new solution with all the defaults run:

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
    - https://github.com/bchavez/Bogus for fake data generation in development

You can optionally:
    - Replace System.Text.Json with Newtonsoft
    - Setup HATEOAS via weblinking
    - Add https://github.com/FluentValidation/FluentValidation
    - Add window authentication for intranet apps
    - Add CORS in development mode if your clients are built separately

To see all the available template options run:

```
dotnet new webapi-bizapp --help
```

Once you are happy with your setup, navigate to the WebAPI project run `dotnet
watch run`.
