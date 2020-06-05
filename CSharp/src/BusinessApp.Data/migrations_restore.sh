# Restores the previous migration

"$(dotnet ef migrations remove -v -s ../BusinessApp.WebApi/ --context BusinessAppReadOnlyDbContext)"
