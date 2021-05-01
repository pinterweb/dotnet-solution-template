FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY *.crt /usr/local/share/ca-certificates/
RUN apt-get update && apt-get install -y ca-certificates
RUN update-ca-certificates

# Just copy everything since we need to preserve dir structure & there is no other way to do it
WORKDIR /src
COPY . .
RUN rm *.crt
RUN dotnet restore

WORKDIR "/src/."
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BusinessApp.WebApi.dll"]
