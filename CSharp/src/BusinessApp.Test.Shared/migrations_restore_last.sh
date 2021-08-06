# Restores the previous migration

dotnet ef migrations remove -v -s ../BusinessApp.Infrastructure.Persistence.IntegrationTest/ --context BusinessAppTestDbContext
