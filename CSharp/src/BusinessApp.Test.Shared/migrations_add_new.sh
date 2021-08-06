 #!/usr/bin/env bash

# Adds a new migration

if [ -z "$1"  ]
then
    echo "Migration name must be the first argument"
    exit 1
fi

dotnet ef migrations add $1 -v -s ../BusinessApp.Infrastructure.Persistence.IntegrationTest/ --context BusinessAppTestDbContext
