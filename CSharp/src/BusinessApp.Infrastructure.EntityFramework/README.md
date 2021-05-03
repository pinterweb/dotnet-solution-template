# BusinessApp.Infrastructure.EntityFramework
> Entity Framework Supporting Services

_Depends on Kernel, Api and Infrastructure projects_

This is an infrastructure project for the application that isolates Entity
Framework Core services.

# Getting Started

- Configure your models, preferable in separate configuration files\
  https://docs.microsoft.com/en-us/ef/core/modeling/

- Run migrations with migrations_add scripts
  `.\migrations_add.cmd YourMigrationName`
  `./migrations_add.sh YourMigrationName`
