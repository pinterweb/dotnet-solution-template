@echo off

SET template_path=%~dp0CSharp

echo %cd%

IF [%1]==[u] (
    echo "Uninstalling %template_path%"
    dotnet new -u %template_path%
) ELSE (
    echo "Installing %template_path%"
    dotnet new -i %template_path%
)
