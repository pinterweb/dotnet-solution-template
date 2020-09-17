# Restores the previous migration

if [ -z "$1"  ]
then
    echo "Migration name must be the first argument"
    exit 1
fi

"$(dotnet ef migrations remove -v -s ../BusinessApp.WebApi/ --context BusinessAppDbContext)"
