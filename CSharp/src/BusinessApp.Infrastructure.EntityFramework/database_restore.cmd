:: Restores the database to the migration passed in

@echo off

IF [%1] == [] (
    echo "Migration name required"
    exit /b %errorlevel%
)

dotnet ef database update %1 -s ..\BusinessApp.WebApi\ -v --context BusinessAppDbContext
