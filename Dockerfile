FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build

ARG USER__CA_CERT
ARG CONTAINER__EXTRA_DEPS

RUN apt-get update && apt-get install -y ca-certificates $CONTAINER__EXTRA_DEPS
COPY *.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates

RUN useradd -ms /bin/bash dotnet

WORKDIR /home/dotnet/dotnet-sln-template
USER dotnet

EXPOSE 5000
EXPOSE 5001

CMD ["dotnet"]
