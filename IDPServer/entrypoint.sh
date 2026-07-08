#!/bin/bash
set -e

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
RETRIES=30
until /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "Hello&Run1234" -C -Q "SELECT 1" > /dev/null 2>&1 || [ $RETRIES -eq 0 ]; do
    echo "Waiting for SQL Server, $((RETRIES--)) remaining attempts..."
    sleep 2
done

if [ $RETRIES -eq 0 ]; then
    echo "SQL Server is not available after 30 attempts. Exiting."
    exit 1
fi

echo "SQL Server is ready!"

# Execute your setup script
if [ -f "/app/IDPServer/setup.sh" ]; then
    echo "Running setup script..."
    chmod +x /app/IDPServer/setup.sh
    /app/IDPServer/setup.sh
else
    echo "Warning: setup.sh not found at /app/IDPServer/setup.sh"
fi

# Start the main application
echo "Starting application..."
exec dotnet IDPServer.dll