:: Restores the previous migration
@echo off

dotnet ef migrations remove -s ..\BusinessApp.WebApi\ -v --context BusinessAppDbContext
