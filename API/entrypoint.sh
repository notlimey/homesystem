#!/bin/bash

# Wait for the database to be ready
until dotnet tool install --global dotnet-ef
do
    >&2 echo "Database is unavailable - sleeping"
    sleep 1
done

# Run migrations
>&2 echo "Database is up - executing migrations"
dotnet ef database update

# Start the application
dotnet API.dll
