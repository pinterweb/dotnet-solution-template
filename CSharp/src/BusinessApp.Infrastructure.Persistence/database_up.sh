# Ensures the database is up to date

dotnet ef database update -v -s ../BusinessApp.WebApi --context BusinessAppDbContext
