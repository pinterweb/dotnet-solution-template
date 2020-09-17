:: Ensures the database is up to date
@echo off

dotnet ef database update -s ..\JobPlanner.WebApi\ -v --context BusinessAppDbContext
