:: Ensures the database is up to date

dotnet ef database update -s ..\BusinessApp.WebApi\ -v --context BusinessAppReadOnlyDbContext
