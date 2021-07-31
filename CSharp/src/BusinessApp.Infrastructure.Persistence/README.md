# BusinessApp.Infrastructure.Persistence
> Supporting Services to persist and retrieve data

_Depends on Kernel, Api and Infrastructure projects_

This is an infrastructure project for the application that isolates persistence
technologies such as Entity Framework

# Getting Started

- Configure your models, preferable in separate configuration files\
  https://docs.microsoft.com/en-us/ef/core/modeling/
//#if metadata
  _When command streaming is setup, inherit from MetadataEntityConfiguration<T>_
  _to save your request data alongside the metadata_
//#endif

- Run migrations with migrations_add scripts\
  `.\migrations_add.cmd YourMigrationName`\
  `./migrations_add.sh YourMigrationName`
