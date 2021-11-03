# BusinessApp.Test.Shared
> Supporting Test Services

This is project has shared services used by multiple layers of the project. See
the sub headings for more details

//#if efcore
## EF Migrations

The [DatabaseFixture.cs](./DatabaseFixture.cs) has the logic to setup and
teardown your test databases. This class is abstract so it can be used across
multiple test databases. For example, the persistence and webapi layer have
isolated integration testing database. Migrations scrips are provided to help
setup and remove entity framework migrations.

**When you update migrations from production, you must run migrations for the**
**test database too since these are separate databases**
//#endif
