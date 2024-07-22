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

# Add a new migration
Write-Output "Adding new migration..."
dotnet ef migrations add Users2 -c ApplicationDbContext -o Data/Migrations

# Update the database
Write-Output "Updating the database..."
dotnet ef database update -c ApplicationDbContext --verbose

# Build the project
Write-Output "Building the project..."
dotnet build

# Run the seed data script
Write-Output "Running the seed data script..."
dotnet run /seed
