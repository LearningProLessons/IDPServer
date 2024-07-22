# Ensure the script stops on errors
$ErrorActionPreference = "Stop"

# Define the migrations directory path
$migrationsPath = "Data/Migrations"

# Remove existing migrations if the directory exists
Write-Output "Removing existing migrations if they exist..."
if (Test-Path $migrationsPath) {
    Remove-Item -Recurse -Force $migrationsPath
} else {
    Write-Output "Migrations directory does not exist, nothing to remove."
}

# Add new migrations for each DbContext
Write-Output "Adding new migration for ApplicationDbContext..."
dotnet ef migrations add Users2 -c ApplicationDbContext -o Data/Migrations

Write-Output "Adding new migration for PersistedGrantDbContext..."
dotnet ef migrations add PersistedGrantMigration -c PersistedGrantDbContext -o Data/Migrations

Write-Output "Adding new migration for ConfigurationDbContext..."
dotnet ef migrations add ConfigurationMigration -c ConfigurationDbContext -o Data/Migrations

# Update the database for each DbContext
Write-Output "Updating the database for ApplicationDbContext..."
dotnet ef database update -c ApplicationDbContext --verbose

Write-Output "Updating the database for PersistedGrantDbContext..."
dotnet ef database update -c PersistedGrantDbContext --verbose

Write-Output "Updating the database for ConfigurationDbContext..."
dotnet ef database update -c ConfigurationDbContext --verbose

# Build the project
Write-Output "Building the project..."
dotnet build

# Run the seed data script
Write-Output "Running the seed data script..."
dotnet run /seed
