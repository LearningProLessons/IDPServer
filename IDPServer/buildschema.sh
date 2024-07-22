#!/bin/bash

rm -rf "Data/Migrations"

dotnet ef migrations add Users1 -c ApplicationDbContext -o Data/Migrations
dotnet ef database update -c ApplicationDbContext --verbose

dotnet build
dotnet run /seed
