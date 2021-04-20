# Restores the database to the migration passed in

if [ -z "$1"  ]
then
    echo "Migration name must be the first argument"
    exit 1
fi

"$(dotnet ef database update %1 -v -s ..\BusinessApp.WebApi\ --context BusinessAppDbContext)"
