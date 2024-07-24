# Ensure the script stops on errors
$ErrorActionPreference = "Stop"

# Define the migrations directory path
$migrationsPath = "Data/Migrations"

# Function to output messages in different colors
function Write-Log {
    param (
        [string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::White
    )
    Write-Host $Message -ForegroundColor $Color
}

# Remove existing migrations if the directory exists
Write-Log "Removing existing migrations if they exist..." -Color Cyan
if (Test-Path $migrationsPath) {
    Remove-Item -Recurse -Force $migrationsPath
    Write-Log "Existing migrations removed." -Color Green
} else {
    Write-Log "Migrations directory does not exist, nothing to remove." -Color Yellow
}

# Add new migrations for each DbContext
Write-Log "Adding new migration for ApplicationDbContext..." -Color Cyan
dotnet ef migrations add Users2 -c ApplicationDbContext -o Data/Migrations

Write-Log "Adding new migration for PersistedGrantDbContext..." -Color Cyan
dotnet ef migrations add PersistedGrantMigration -c PersistedGrantDbContext -o Data/Migrations

Write-Log "Adding new migration for ConfigurationDbContext..." -Color Cyan
dotnet ef migrations add ConfigurationMigration -c ConfigurationDbContext -o Data/Migrations

# Update the database for each DbContext
Write-Log "Updating the database for ApplicationDbContext..." -Color Cyan
dotnet ef database update -c ApplicationDbContext 
Write-Log "Database updated for ApplicationDbContext." -Color Green

Write-Log "Updating the database for PersistedGrantDbContext..." -Color Cyan
dotnet ef database update -c PersistedGrantDbContext 
Write-Log "Database updated for PersistedGrantDbContext." -Color Green

Write-Log "Updating the database for ConfigurationDbContext..." -Color Cyan
dotnet ef database update -c ConfigurationDbContext 
Write-Log "Database updated for ConfigurationDbContext." -Color Green

# Build the project
Write-Log "Building the project..." -Color Cyan
dotnet build
Write-Log "Project built successfully." -Color Green

# Run the seed data script
Write-Log "Running the seed data script..." -Color Cyan
dotnet run /seed
Write-Log "Seed data script executed." -Color Green
