:: Restores the previous migration
@echo off

dotnet ef migrations remove -v -s ..\BusinessApp.Data.IntegrationTest\ --context BusinessAppReadOnlyTestDbContext
