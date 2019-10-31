# TODO once i can figure out why dotnet restore is not working
# We should build and restore in docker

#Temp solution to publish locally and push dlls to docker
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY BusinessApp.WebApi/.env BusinessApp.WebApi/bin/Debug/netcoreapp2.2/publish/ ./
ENTRYPOINT ["dotnet", "BusinessApp.WebApi.dll"]
