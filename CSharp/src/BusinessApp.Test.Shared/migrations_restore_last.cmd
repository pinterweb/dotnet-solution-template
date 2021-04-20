:: Restores the previous migration
@echo off

dotnet ef migrations remove -v -s ..\BusinessApp.Infrastructure.EntityFramework.IntegrationTest\ --context BusinessAppTestDbContext
