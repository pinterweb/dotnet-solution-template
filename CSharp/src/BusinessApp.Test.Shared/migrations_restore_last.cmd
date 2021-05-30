:: Restores the previous migration
@echo off

dotnet ef migrations remove -v -s ..\BusinessApp.Infrastructure.Persistence.IntegrationTest\ --context BusinessAppTestDbContext
