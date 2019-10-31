dotnet publish BusinessApp.WebApi\BusinessApp.WebApi.csproj -c Debug
if %errorlevel% neq 0 exit /b %errorlevel%
docker-compose up --build -d
if %errorlevel% neq 0 exit /b %errorlevel%
