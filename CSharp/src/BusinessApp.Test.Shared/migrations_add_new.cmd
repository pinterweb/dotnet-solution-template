:: Adds a new migration

@echo off

IF [%1] == [] (
    echo "Migration name required"
    exit /b %errorlevel%
)

dotnet ef migrations add %1 -v -s ..\BusinessApp.Infrastructure.EntityFramework.IntegrationTest\ --context BusinessAppTestDbContext
