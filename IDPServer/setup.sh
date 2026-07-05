#!/usr/bin/env bash

set -euo pipefail

# ==========================================
# Configuration
# ==========================================

MIGRATIONS_PATH="Data/Migrations"

# ==========================================
# Colors
# ==========================================

RED="\033[0;31m"
GREEN="\033[0;32m"
YELLOW="\033[1;33m"
CYAN="\033[0;36m"
WHITE="\033[0m"
NC="\033[0m"

# ==========================================
# Logging
# ==========================================

write_log() {
    local message="$1"
    local color="${2:-$WHITE}"

    echo -e "${color}${message}${NC}"
}

# ==========================================
# Progress Bar
# ==========================================

show_loading_bar() {
    local message="$1"
    local total_steps="$2"

    local bar_width=50

    echo
    echo "$message..."

    for ((progress=0; progress<=total_steps; progress++))
    do
        percent=$(( progress * 100 / total_steps ))
        hashes=$(( percent * bar_width / 100 ))
        spaces=$(( bar_width - hashes ))

        bar=$(printf "%${hashes}s" | tr ' ' '#')
        empty=$(printf "%${spaces}s")

        printf "\r[%-50s] %3d%% Complete" "${bar}${empty}" "$percent"

        sleep 0.2
    done

    printf "\n"
    echo -e "${GREEN}Done.${NC}"
}

# ==========================================
# Execute dotnet command
# ==========================================

run_dotnet_command() {
    local output
    local exit_code

    output=$("$@" 2>&1)
    exit_code=$?

    if [[ $exit_code -ne 0 ]]; then
        echo
        echo -e "${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        echo -e "${RED}Command failed:${NC}"
        echo
        echo "$@"
        echo
        echo "$output"
        echo -e "${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        exit $exit_code
    fi

    # Hide the annoying EF version warning only on success
    echo "$output" | grep -v "Entity Framework tools version .* is older than that of the runtime"
}

# ==========================================
# Remove Existing Migrations
# ==========================================

write_log "Removing existing migrations if they exist..." "$CYAN"

if [[ -d "$MIGRATIONS_PATH" ]]; then

    show_loading_bar "Removing existing migrations" 10

    rm -rf "$MIGRATIONS_PATH"

    echo -e "${GREEN}Existing migrations removed.${NC}"

else

    write_log "Migrations directory does not exist. Nothing to remove." "$YELLOW"

fi

# ==========================================
# Add Migration
# ==========================================

add_migration() {

    local context="$1"
    local migration="$2"

    write_log "Creating migration '$migration' for $context..." "$CYAN"

    if run_dotnet_command \
        dotnet ef migrations add "$migration" \
        -c "$context" \
        -o Data/Migrations
    then
        echo -e "${GREEN}✔ $context migration created.${NC}"
    fi
}

add_migration "ApplicationDbContext" "Users2"
add_migration "PersistedGrantDbContext" "PersistedGrantMigration"
add_migration "ConfigurationDbContext" "ConfigurationMigration"

# ==========================================
# Update Database
# ==========================================

update_database() {

    local context="$1"

    write_log "Updating $context..." "$CYAN"

    if run_dotnet_command \
        dotnet ef database update \
        -c "$context"
    then
        echo -e "${GREEN}✔ $context updated.${NC}"
    fi
}

update_database "ApplicationDbContext"
update_database "PersistedGrantDbContext"
update_database "ConfigurationDbContext"

# ==========================================
# Build
# ==========================================

write_log "Building the project..." "$CYAN"

show_loading_bar "Building project" 10

run_dotnet_command dotnet build

echo -e "${GREEN}Project built successfully.${NC}"

# ==========================================
# Seed Database
# ==========================================

write_log "Running seed data..." "$CYAN"

show_loading_bar "Running seed data" 10

run_dotnet_command dotnet run /seed

echo -e "${GREEN}Seed completed successfully.${NC}"

echo
echo -e "${GREEN}=========================================${NC}"
echo -e "${GREEN}All operations completed successfully.${NC}"
echo -e "${GREEN}=========================================${NC}"