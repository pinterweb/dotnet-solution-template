:: Ensures the database is up to date
@echo off

dotnet ef database update -s ..\BusinessApp.WebApi\ -v --context BusinessAppDbContext
