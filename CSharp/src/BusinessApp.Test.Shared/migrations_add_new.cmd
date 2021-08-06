:: Adds a new migration

@echo off

IF [%1] == [] (
    echo "Migration name must be the first argument"
    exit /b %errorlevel%
)

dotnet ef migrations add %1 -v -s ..\BusinessApp.Infrastructure.Persistence.IntegrationTest\ --context BusinessAppTestDbContext
