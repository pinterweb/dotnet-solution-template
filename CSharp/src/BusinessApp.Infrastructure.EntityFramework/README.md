# README

This is an infrastructure project for the application that isolates Entity
Framework Core services.

# Internal Dependencies

    - Main Infrastructure project so that contract models and data related
      request handlers can be registered
    - Kernel project because that has the main app services

# External Dependencies

    - https://github.com/dotnet/efcore
      - Object relational mapper for repository / unit of work implementations


# Next Steps
    - https://docs.microsoft.com/en-us/ef/core/modeling/
      Configure your models, preferable in separate configuration files
    - Run migrations in .\migrations_add.cmd "YouMigrationName"
