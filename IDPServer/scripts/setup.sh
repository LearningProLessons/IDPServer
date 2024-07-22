#!/bin/bash

# Remove existing migrations
rm -rf "Data/Migrations"



# Build the project
dotnet build

# Run the seed data script
dotnet run /seed

# Start the application
dotnet IDPServer.dll

