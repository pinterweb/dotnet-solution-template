# BusinessApp.Infrastructure.EntityFramework

This is an infrastructure project for the application that isolates Entity
Framework Core services.

# Internal Dependencies

[Infrastructure](/CSharp/src/BusinessApp.Infrastructure)
So that contract models and data related request handlers can be registered

[Kernel](/CSharp/src/BusinessApp.Kernel)

# External Dependencies

[Entity Framework Core](https://github.com/dotnet/efcore)

# Next Steps

- https://docs.microsoft.com/en-us/ef/core/modeling/
  Configure your models, preferable in separate configuration files

- Run migrations with migrations_add scripts
  `.\migrations_add.cmd YourMigrationName`
  `./migrations_add.sh YourMigrationName`
