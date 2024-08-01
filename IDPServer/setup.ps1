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

# Function to show a loading bar
function Show-LoadingBar {
    param (
        [string]$Message,
        [int]$TotalSteps
    )
    
    $progress = 0
    $barWidth = 50

    Write-Host "$Message..." -NoNewline
    Write-Host -NoNewline "["
    while ($progress -le $TotalSteps) {
        $percentComplete = ($progress / $TotalSteps) * 100
        $numHashes = [math]::Round(($percentComplete / 100) * $barWidth)
        $numSpaces = $barWidth - $numHashes
        $bar = "#" * $numHashes + " " * $numSpaces
        Write-Host -NoNewline "`r[$bar] $([math]::Round($percentComplete, 2))% Complete"
        Start-Sleep -Milliseconds 200
        $progress++
    }
    Write-Host "`r[$bar] 100% Complete" -ForegroundColor Green
}

# Function to run dotnet commands and suppress specific warnings
function Run-DotnetCommand {
    param (
        [string]$Command
    )
    $output = Invoke-Expression $Command 2>&1
    # Filter out the specific warning message
    $filteredOutput = $output | Where-Object { $_ -notmatch "Entity Framework tools version '.*' is older than that of the runtime" }
    Write-Host $filteredOutput
}

# Remove existing migrations if the directory exists
Write-Log "Removing existing migrations if they exist..." -Color Cyan
if (Test-Path $migrationsPath) {
    Show-LoadingBar "Removing existing migrations" -TotalSteps 10
    Remove-Item -Recurse -Force $migrationsPath
    Write-Host "`rDone removing existing migrations." -ForegroundColor Green
} else {
    Write-Log "Migrations directory does not exist, nothing to remove." -Color Yellow
}

# Add new migrations for each DbContext
function Add-Migration {
    param (
        [string]$DbContext,
        [string]$MigrationName
    )
    Write-Log "Adding new migration for $DbContext..." -Color Cyan
    Show-LoadingBar "Adding migration $MigrationName for $DbContext" -TotalSteps 10
    Run-DotnetCommand "dotnet ef migrations add $MigrationName -c $DbContext -o Data/Migrations"
    Write-Host "`rDone adding migration $MigrationName for $DbContext." -ForegroundColor Green
}

Add-Migration -DbContext "ApplicationDbContext" -MigrationName "Users2"
Add-Migration -DbContext "PersistedGrantDbContext" -MigrationName "PersistedGrantMigration"
Add-Migration -DbContext "ConfigurationDbContext" -MigrationName "ConfigurationMigration"

# Update the database for each DbContext
function Update-Database {
    param (
        [string]$DbContext
    )
    Write-Log "Updating the database for $DbContext..." -Color Cyan
    Show-LoadingBar "Updating database for $DbContext" -TotalSteps 10
    Run-DotnetCommand "dotnet ef database update -c $DbContext"
    Write-Host "`rDone updating database for $DbContext." -ForegroundColor Green
}

Update-Database -DbContext "ApplicationDbContext"
Update-Database -DbContext "PersistedGrantDbContext"
Update-Database -DbContext "ConfigurationDbContext"

# Build the project
Write-Log "Building the project..." -Color Cyan
Show-LoadingBar "Building project" -TotalSteps 10
Run-DotnetCommand "dotnet build"
Write-Host "`rDone building project." -ForegroundColor Green

# Run the seed data script
Write-Log "Running the seed data script..." -Color Cyan
Show-LoadingBar "Running seed data script" -TotalSteps 10
Run-DotnetCommand "dotnet run /seed"
Write-Host "`rDone running seed data script." -ForegroundColor Green
