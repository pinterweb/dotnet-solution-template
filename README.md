# Dotnet Solution Template
> Layered dotnet solution template decorated with services

Inspired by [solidservices](https://github.com/dotnetjunkie/solidservices) and
powered by [Simple Injector](https://github.com/simpleinjector/SimpleInjector) to provide rich
service decoration for your business needs. For project definitions see the main
[README](/CSharp) that goes along with new applications.

- [Installation](#installation)
- [Testing](#testing)
- [Running the webapi](#running-the-webapi)
- [Creating a new app](#creating-a-new-app)

## Installation

1. `git clone https://github.com/pinterweb/dotnet-solution-template.git`
2. `cd dotnet-solution-template`
3. `.\install.bat` or `sh ./install.sh`\
   _to uninstall run `.\install.bat u` or `sh ./install.sh u`_

### Docker
1. `docker-compose run --rm --service-ports dotnet-sln-template bash`
1. `sh ./install.sh`

## Testing

1. `cd CSharp\src`
2. `dotnet test`

## Running the webapi

The webapi project can be run without creating a new application

1. `cd CSharp\src\BusinessApp.WebApi`
2. `dotnet watch run`\
   _read the "Getting Started" sections in each project's README to determine the data_
   _and services needed_

## Creating a new app

To create a new solution with all the defaults run:

```
dotnet new sln-layers -n <your-app-name> -o <your-app-directory>
```

The default setup gives you:
- git directory
- git hooks to run tests on commits
- JSON parsing with System.Text.Json
- Docker setup
- [Bogus](https://github.com/bchavez/Bogus) for fake data generation in development

To see all the available template options run:

```
dotnet new sln-layers --help
```

Once you installed your new app do not forget to:
- [ ] Add something in your CONTRIBUTING
- [ ] Commit your code to source control (e.g. `feat(all): Add initial infrastructure`)
- [ ] Add your continuous integration assets (e.g. azure-pipeline.yml)
- [ ] Commit your code to source control (e.g. `chore(build): Add build/release assets`)

For Docker:
- [ ] Add extra dependencies in your docker development container by setting the
      CONTAINER__EXTRA_DEPS environmental variable
- [ ] Copy over any custom root CA certs to get NuGet to work
